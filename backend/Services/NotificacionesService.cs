using System.Collections.Concurrent;
using Adopta.Api.Data;
using Adopta.Api.Data.Entities;
using Adopta.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Api.Services;

public interface INotificacionesService
{
    Task<NotificacionesResumenDto> ListarAsync(int idUsuario, bool soloNoLeidas, int limite);
    Task<int> ContarNoLeidasAsync(int idUsuario);
    Task MarcarLeidaAsync(int idUsuario, int idNotificacion);
    Task MarcarTodasLeidasAsync(int idUsuario);
}

public class NotificacionesService : INotificacionesService
{
    private const int DiasHaciaAtras = 14;               
    private static readonly TimeSpan SincronizarCada = TimeSpan.FromSeconds(30);
    private static readonly ConcurrentDictionary<int, DateTime> UltimaSincronizacion = new();

    private readonly AdoptaDbContext _db;

    public NotificacionesService(AdoptaDbContext db) => _db = db;

    public async Task<NotificacionesResumenDto> ListarAsync(int idUsuario, bool soloNoLeidas, int limite)
    {
        await SincronizarAsync(idUsuario);
        limite = Math.Clamp(limite, 1, 50);

        var query = _db.Notificaciones.AsNoTracking().Where(n => n.IdUsuario == idUsuario);
        if (soloNoLeidas) query = query.Where(n => !n.Leida);

        var items = await query
            .OrderByDescending(n => n.FechaEnvio).ThenByDescending(n => n.IdNotificacion)
            .Take(limite)
            .Select(n => new NotificacionDto
            {
                IdNotificacion = n.IdNotificacion,
                Titulo = n.Titulo,
                Mensaje = n.Mensaje,
                Tipo = n.Tipo,
                Leida = n.Leida,
                Fecha = n.FechaEnvio,
                IdSolicitud = n.IdSolicitud
            })
            .ToListAsync();

        return new NotificacionesResumenDto
        {
            NoLeidas = await _db.Notificaciones.CountAsync(n => n.IdUsuario == idUsuario && !n.Leida),
            Items = items
        };
    }

    public async Task<int> ContarNoLeidasAsync(int idUsuario)
    {
        await SincronizarAsync(idUsuario);
        return await _db.Notificaciones.CountAsync(n => n.IdUsuario == idUsuario && !n.Leida);
    }

    public async Task MarcarLeidaAsync(int idUsuario, int idNotificacion)
    {
        var n = await _db.Notificaciones.FirstOrDefaultAsync(x => x.IdNotificacion == idNotificacion && x.IdUsuario == idUsuario)
            ?? throw new NegocioException("Notificación no encontrada.", StatusCodes.Status404NotFound);
        if (n.Leida) return;
        n.Leida = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarcarTodasLeidasAsync(int idUsuario) =>
        await _db.Notificaciones
            .Where(n => n.IdUsuario == idUsuario && !n.Leida)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));

    // =================== Eventos originados en la app móvil (solo organizaciones) ===================

    private sealed record Aviso(int IdSolicitud, DateTime Fecha, string Titulo, string Mensaje);

    private async Task SincronizarAsync(int idUsuario)
    {
        var ahora = DateTime.UtcNow;
        if (UltimaSincronizacion.TryGetValue(idUsuario, out var ultima) && ahora - ultima < SincronizarCada) return;
        UltimaSincronizacion[idUsuario] = ahora;

        var idOrg = await _db.Organizaciones.Where(o => o.IdUsuario == idUsuario)
            .Select(o => (int?)o.IdOrganizacion).FirstOrDefaultAsync();
        if (idOrg is null) return; // administrador: sus avisos se insertan al ocurrir el evento

        var desde = DateTime.Now.AddDays(-DiasHaciaAtras);

        // Nueva solicitud de adopción para una mascota de la organización
        var nuevas = await _db.SolicitudesAdopcions.AsNoTracking()
            .Where(s => s.IdMascotaNavigation.IdOrganizacion == idOrg && s.FechaSolicitud >= desde)
            .Select(s => new
            {
                s.IdSolicitud, s.FechaSolicitud,
                Mascota = s.IdMascotaNavigation.Nombre,
                Adoptante = s.IdAdoptanteNavigation.Nombre + " " + s.IdAdoptanteNavigation.ApellidoPaterno
            }).ToListAsync();
        await InsertarNuevosAsync(idUsuario, "NuevaSolicitud", nuevas.Select(s => new Aviso(
            s.IdSolicitud, s.FechaSolicitud, "Nueva solicitud de adopción",
            $"{s.Adoptante} quiere adoptar a {s.Mascota}.")).ToList());

        // El adoptante envió los documentos que se le pidieron
        var docs = await _db.SolicitudesAdopcions.AsNoTracking()
            .Where(s => s.IdMascotaNavigation.IdOrganizacion == idOrg && s.FechaEnvioDocumentos != null && s.FechaEnvioDocumentos >= desde)
            .Select(s => new
            {
                s.IdSolicitud, Fecha = s.FechaEnvioDocumentos!.Value,
                Mascota = s.IdMascotaNavigation.Nombre,
                Adoptante = s.IdAdoptanteNavigation.Nombre + " " + s.IdAdoptanteNavigation.ApellidoPaterno
            }).ToListAsync();
        await InsertarNuevosAsync(idUsuario, "DocumentosRecibidos", docs.Select(s => new Aviso(
            s.IdSolicitud, s.Fecha, "Documentos recibidos",
            $"{s.Adoptante} envió su documentación para adoptar a {s.Mascota}.")).ToList());

        // Reporte de seguimiento de una adopción (se avisa del más reciente de cada adopción)
        var reportes = await _db.ReportesSeguimientos.AsNoTracking()
            .Where(r => r.IdAdopcionNavigation.IdSolicitudNavigation.IdMascotaNavigation.IdOrganizacion == idOrg && r.FechaReporte >= desde)
            .Select(r => new
            {
                r.IdAdopcionNavigation.IdSolicitud, r.FechaReporte,
                Mascota = r.IdAdopcionNavigation.IdSolicitudNavigation.IdMascotaNavigation.Nombre
            }).ToListAsync();
        await InsertarNuevosAsync(idUsuario, "ReporteSeguimiento", reportes
            .GroupBy(r => r.IdSolicitud)
            .Select(g => g.OrderByDescending(r => r.FechaReporte).First())
            .Select(r => new Aviso(r.IdSolicitud, r.FechaReporte, "Nuevo reporte de seguimiento",
                $"Hay un nuevo reporte de seguimiento de {r.Mascota}.")).ToList());

        // Solicitud de devolución pendiente
        var devoluciones = await _db.SolicitudesDevolucions.AsNoTracking()
            .Where(d => d.IdAdopcionNavigation.IdSolicitudNavigation.IdMascotaNavigation.IdOrganizacion == idOrg && d.FechaSolicitud >= desde)
            .Select(d => new
            {
                d.IdAdopcionNavigation.IdSolicitud, d.FechaSolicitud, d.Estado,
                Mascota = d.IdAdopcionNavigation.IdSolicitudNavigation.IdMascotaNavigation.Nombre
            }).ToListAsync();
        await InsertarNuevosAsync(idUsuario, "SolicitudDevolucion", devoluciones
            .Where(d => ServiceHelpers.Normalizar(d.Estado) == "pendiente")
            .Select(d => new Aviso(d.IdSolicitud, d.FechaSolicitud, "Solicitud de devolución",
                $"Se solicitó devolver a {d.Mascota}. Revísala para dar una respuesta.")).ToList());
    }

    /// <summary>Inserta un aviso solo si no hay otro del mismo tipo y solicitud igual o más reciente.</summary>
    private async Task InsertarNuevosAsync(int idUsuario, string tipo, List<Aviso> avisos)
    {
        if (avisos.Count == 0) return;

        var ids = avisos.Select(a => a.IdSolicitud).Distinct().ToList();
        var ultimas = (await _db.Notificaciones.AsNoTracking()
                .Where(n => n.IdUsuario == idUsuario && n.Tipo == tipo && n.IdSolicitud != null && ids.Contains(n.IdSolicitud.Value))
                .GroupBy(n => n.IdSolicitud!.Value)
                .Select(g => new { Id = g.Key, Fecha = g.Max(n => n.FechaEnvio) })
                .ToListAsync())
            .ToDictionary(x => x.Id, x => x.Fecha);

        var hay = false;
        foreach (var a in avisos)
        {
            if (ultimas.TryGetValue(a.IdSolicitud, out var f) && f >= a.Fecha) continue;
            _db.Notificaciones.Add(new Notificacione
            {
                IdUsuario = idUsuario,
                IdSolicitud = a.IdSolicitud,
                Tipo = tipo,
                Titulo = a.Titulo,
                Mensaje = a.Mensaje,
                FechaEnvio = a.Fecha
            });
            hay = true;
        }
        if (hay) await _db.SaveChangesAsync();
    }
}
