using System.Globalization;
using System.Text;
using Adopta.Api.Data;
using Adopta.Api.Data.Entities;
using Adopta.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Api.Services;

public interface ISeguimientoService
{
    Task<PagedResultDto<SeguimientoListaDto>> ListarAsync(int? idOrganizacion, SeguimientoQueryDto f);
    Task<SeguimientoDetalleDto> DetalleAsync(int idAdopcion, int? idOrganizacion);
    Task<List<EstadoAdaptacionDto>> EstadosAdaptacionAsync();
    Task<PagedResultDto<DevolucionListaDto>> ListarDevolucionesAsync(int? idOrganizacion, DevolucionQueryDto f);
    Task<DevolucionDetalleDto> DetalleDevolucionAsync(int idDevolucion, int? idOrganizacion);

    // Solo organización
    Task ActualizarAdaptacionAsync(int idAdopcion, int idOrganizacion, int idUsuario, ActualizarAdaptacionRequestDto dto);
    Task AceptarDevolucionAsync(int idDevolucion, int idOrganizacion, int idUsuario, AceptarDevolucionRequestDto dto);
}

public class SeguimientoService : ISeguimientoService
{
    private const string Pendiente = "Pendiente";
    private const string Aceptada = "Aceptada";
    private const string SinSeguimiento = "Sin seguimiento";
    private const string Disponible = "Disponible";

    private const int DiasRevisionPorDefecto = 10;
    private const int MaxReportes = 20;
    private const int MaxActividad = 6;

    private readonly AdoptaDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<SeguimientoService> _logger;

    public SeguimientoService(AdoptaDbContext db, IConfiguration config, ILogger<SeguimientoService> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    private sealed class FilaAdopcion
    {
        public Adopcione A { get; set; } = null!;
        public SolicitudesAdopcion S { get; set; } = null!;
        public Mascota M { get; set; } = null!;
        public Usuario U { get; set; } = null!;   // adoptante
    }

    private sealed class FilaDevolucion
    {
        public SolicitudesDevolucion D { get; set; } = null!;
        public Adopcione A { get; set; } = null!;
        public SolicitudesAdopcion S { get; set; } = null!;
        public Mascota M { get; set; } = null!;
        public Usuario U { get; set; } = null!;   // adoptante
    }

    private IQueryable<FilaAdopcion> Adopciones(int? idOrganizacion) =>
        from a in _db.Adopciones.AsNoTracking()
        where a.SolicitudesDevolucion == null || a.SolicitudesDevolucion.Estado != Aceptada
        let s = a.IdSolicitudNavigation
        let m = s.IdMascotaNavigation
        where idOrganizacion == null || m.IdOrganizacion == idOrganizacion
        select new FilaAdopcion { A = a, S = s, M = m, U = s.IdAdoptanteNavigation };

    private IQueryable<FilaDevolucion> Devoluciones(int? idOrganizacion) =>
        from d in _db.SolicitudesDevolucions.AsNoTracking()
        let a = d.IdAdopcionNavigation
        let s = a.IdSolicitudNavigation
        let m = s.IdMascotaNavigation
        where idOrganizacion == null || m.IdOrganizacion == idOrganizacion
        select new FilaDevolucion { D = d, A = a, S = s, M = m, U = s.IdAdoptanteNavigation };

    public async Task<PagedResultDto<SeguimientoListaDto>> ListarAsync(int? idOrganizacion, SeguimientoQueryDto f)
    {
        var admin = idOrganizacion == null;
        var q = Adopciones(idOrganizacion);

        if (admin && f.IdOrganizacion is int idOrg)
            q = q.Where(x => x.M.IdOrganizacion == idOrg);

        foreach (var palabra in Palabras(f.Buscar))
        {
            var p = palabra;
            q = q.Where(x => x.M.Nombre.Contains(p)
                             || x.U.Nombre.Contains(p)
                             || x.U.ApellidoPaterno.Contains(p)
                             || (x.U.ApellidoMaterno != null && x.U.ApellidoMaterno.Contains(p))
                             || (admin && x.M.IdOrganizacionNavigation.Nombre.Contains(p)));
        }

        if (f.IdEstadoAdaptacion is int idEstado)
            q = q.Where(x => x.M.IdEstadoAdaptacion == idEstado);

        if (f.Desde is DateTime desde && f.Hasta is DateTime hasta && desde.Date > hasta.Date)
            throw new NegocioException("La fecha inicial no puede ser posterior a la fecha final.");
        if (f.Desde is DateTime d0)
        {
            var inicio = d0.Date;
            q = q.Where(x => x.A.FechaAdopcion >= inicio);
        }
        if (f.Hasta is DateTime d1)
        {
            var finExclusivo = d1.Date.AddDays(1);
            q = q.Where(x => x.A.FechaAdopcion < finExclusivo);
        }

        var (pagina, tamano) = Paginar(f.Pagina, f.Tamano, 50);
        var total = await q.CountAsync();

        var filas = await q
            .OrderByDescending(x => x.A.FechaAdopcion).ThenByDescending(x => x.A.IdAdopcion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(x => new
            {
                x.A.IdAdopcion,
                x.A.FechaAdopcion,
                x.M.IdMascota,
                Mascota = x.M.Nombre,
                x.M.Especie,
                Foto = x.M.FotosMascota
                    .OrderByDescending(ft => ft.EsPrincipal).ThenBy(ft => ft.IdFoto)
                    .Select(ft => ft.RutaFoto).FirstOrDefault(),
                x.M.IdEstadoAdaptacion,
                EstadoAdaptacion = x.M.IdEstadoAdaptacionNavigation.Nombre,
                x.M.IdOrganizacion,
                Organizacion = x.M.IdOrganizacionNavigation.Nombre,
                x.U.Nombre,
                x.U.ApellidoPaterno,
                x.U.ApellidoMaterno
            })
            .ToListAsync();

        var items = filas.Select(r => new SeguimientoListaDto
        {
            IdAdopcion = r.IdAdopcion,
            IdMascota = r.IdMascota,
            Mascota = r.Mascota,
            Especie = r.Especie,
            FotoUrl = PerfilService.UrlFoto(r.Foto),
            Adoptante = NombreCompleto(r.Nombre, r.ApellidoPaterno, r.ApellidoMaterno),
            FechaAdopcion = r.FechaAdopcion,
            IdEstadoAdaptacion = r.IdEstadoAdaptacion,
            EstadoAdaptacion = r.EstadoAdaptacion,
            IdOrganizacion = r.IdOrganizacion,
            Organizacion = r.Organizacion
        }).ToList();

        return Resultado(items, total, pagina, tamano);
    }

    public async Task<List<EstadoAdaptacionDto>> EstadosAdaptacionAsync()
    {
        var estados = await _db.EstadosAdaptacions.AsNoTracking()
            .OrderBy(e => e.IdEstadoAdaptacion).ToListAsync();

        // "Sin seguimiento" es el estado inicial (antes de adoptar): no se elige a mano.
        return estados
            .Where(e => !EsSinSeguimiento(e.Nombre))
            .Select(e => new EstadoAdaptacionDto { IdEstadoAdaptacion = e.IdEstadoAdaptacion, Nombre = e.Nombre })
            .ToList();
    }

    public async Task<SeguimientoDetalleDto> DetalleAsync(int idAdopcion, int? idOrganizacion)
    {
        var r = await Adopciones(idOrganizacion)
            .Where(x => x.A.IdAdopcion == idAdopcion)
            .Select(x => new
            {
                x.A.IdAdopcion,
                x.A.FechaAdopcion,
                x.M.IdMascota,
                Mascota = x.M.Nombre,
                x.M.Especie,
                x.M.Raza,
                x.M.EdadAproximada,
                Foto = x.M.FotosMascota
                    .OrderByDescending(ft => ft.EsPrincipal).ThenBy(ft => ft.IdFoto)
                    .Select(ft => ft.RutaFoto).FirstOrDefault(),
                x.M.IdEstadoAdaptacion,
                EstadoAdaptacion = x.M.IdEstadoAdaptacionNavigation.Nombre,
                x.M.IdOrganizacion,
                Organizacion = x.M.IdOrganizacionNavigation.Nombre,
                x.U.Nombre,
                x.U.ApellidoPaterno,
                x.U.ApellidoMaterno,
                x.U.Correo,
                x.U.Telefono
            })
            .FirstOrDefaultAsync()
            ?? throw new NegocioException("Seguimiento no encontrado.", StatusCodes.Status404NotFound);

        var reportes = await _db.ReportesSeguimientos.AsNoTracking()
            .Include(x => x.FotosSeguimientos)
            .Where(x => x.IdAdopcion == idAdopcion)
            .OrderByDescending(x => x.FechaReporte).ThenByDescending(x => x.IdReporte)
            .Take(MaxReportes)
            .ToListAsync();

        var historial = await _db.HistorialAdaptacions.AsNoTracking()
            .Where(h => h.IdMascota == r.IdMascota && h.FechaCambio >= r.FechaAdopcion)
            .OrderByDescending(h => h.FechaCambio).ThenByDescending(h => h.IdHistorialAdaptacion)
            .Take(MaxActividad)
            .Select(h => new { h.FechaCambio, Estado = h.IdEstadoAdaptacionNavigation.Nombre, h.Observaciones })
            .ToListAsync();

        var ultima = r.FechaAdopcion;
        if (reportes.Count > 0) ultima = Max(ultima, reportes.Max(x => x.FechaReporte));
        if (historial.Count > 0) ultima = Max(ultima, historial.Max(h => h.FechaCambio));

        var proxima = ultima.AddDays(DiasRevision);

        var idDevolucionPendiente = await _db.SolicitudesDevolucions.AsNoTracking()
            .Where(d => d.IdAdopcion == idAdopcion && d.Estado == Pendiente)
            .Select(d => (int?)d.IdDevolucion)
            .FirstOrDefaultAsync();

        var actividad = reportes
            .Select(x => new ActividadSeguimientoDto { Tipo = "Reporte", Fecha = x.FechaReporte })
            .Concat(historial.Select(h => new ActividadSeguimientoDto
            {
                Tipo = "Estado",
                Estado = h.Estado,
                Detalle = h.Observaciones,
                Fecha = h.FechaCambio
            }))
            .OrderByDescending(a => a.Fecha)
            .Take(MaxActividad)
            .ToList();

        return new SeguimientoDetalleDto
        {
            IdAdopcion = r.IdAdopcion,
            IdMascota = r.IdMascota,
            Mascota = r.Mascota,
            Especie = r.Especie,
            Raza = r.Raza,
            EdadAproximada = r.EdadAproximada,
            FotoUrl = PerfilService.UrlFoto(r.Foto),
            IdOrganizacion = r.IdOrganizacion,
            Organizacion = r.Organizacion,
            Adoptante = NombreCompleto(r.Nombre, r.ApellidoPaterno, r.ApellidoMaterno),
            AdoptanteCorreo = r.Correo,
            AdoptanteTelefono = r.Telefono,
            FechaAdopcion = r.FechaAdopcion,
            UltimaActualizacion = ultima,
            ProximaRevision = proxima,
            RevisionVencida = proxima.Date < DateTime.Today,
            IdEstadoAdaptacion = r.IdEstadoAdaptacion,
            EstadoAdaptacion = r.EstadoAdaptacion,
            IdDevolucionPendiente = idDevolucionPendiente,
            Actividad = actividad,
            Reportes = reportes.Select(x => new ReporteSeguimientoDto
            {
                IdReporte = x.IdReporte,
                Fecha = x.FechaReporte,
                Comportamiento = x.Comportamiento,
                Salud = x.Salud,
                Alimentacion = x.Alimentacion,
                Adaptacion = x.Adaptacion,
                Observaciones = x.Observaciones,
                Fotos = x.FotosSeguimientos
                    .OrderBy(ft => ft.IdFotoSeguimiento)
                    .Select(ft => PerfilService.UrlFoto(ft.RutaFoto)!)
                    .ToList()
            }).ToList()
        };
    }

    public async Task ActualizarAdaptacionAsync(int idAdopcion, int idOrganizacion, int idUsuario, ActualizarAdaptacionRequestDto dto)
    {
        var fila = await Adopciones(idOrganizacion)
            .Where(x => x.A.IdAdopcion == idAdopcion)
            .Select(x => new
            {
                x.S.IdSolicitud,
                IdAdoptante = x.U.IdUsuario,
                x.M.IdMascota,
                NombreMascota = x.M.Nombre,
                x.M.IdEstadoAdaptacion
            })
            .FirstOrDefaultAsync()
            ?? throw new NegocioException("Seguimiento no encontrado.", StatusCodes.Status404NotFound);

        var nuevo = (await _db.EstadosAdaptacions.AsNoTracking().ToListAsync())
            .FirstOrDefault(e => e.IdEstadoAdaptacion == dto.IdEstadoAdaptacion && !EsSinSeguimiento(e.Nombre))
            ?? throw new NegocioException("El estado de adaptación seleccionado no es válido.");

        var observaciones = Opcional(dto.Observaciones, "las observaciones", 1000);

        if (nuevo.IdEstadoAdaptacion == fila.IdEstadoAdaptacion && observaciones is null)
            throw new NegocioException("No hay cambios que guardar. Cambia el estado o agrega una observación.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var mascota = await _db.Mascotas.FirstAsync(m => m.IdMascota == fila.IdMascota);
        mascota.IdEstadoAdaptacion = nuevo.IdEstadoAdaptacion;

        _db.HistorialAdaptacions.Add(new HistorialAdaptacion
        {
            IdMascota = fila.IdMascota,
            IdEstadoAdaptacion = nuevo.IdEstadoAdaptacion,
            IdUsuarioResponsable = idUsuario,
            Observaciones = observaciones
        });

        _db.Notificaciones.Add(new Notificacione
        {
            IdUsuario = fila.IdAdoptante,
            IdSolicitud = fila.IdSolicitud,
            Tipo = "Seguimiento",
            Titulo = "Seguimiento actualizado",
            Mensaje = $"La organización actualizó el seguimiento de {fila.NombreMascota}: ahora está en \"{nuevo.Nombre}\"."
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        _logger.LogInformation("Adopción {Id}: adaptación de la mascota {Mascota} → '{Estado}' por usuario {Usuario}",
            idAdopcion, fila.IdMascota, nuevo.Nombre, idUsuario);
    }

    public async Task<PagedResultDto<DevolucionListaDto>> ListarDevolucionesAsync(int? idOrganizacion, DevolucionQueryDto f)
    {
        var admin = idOrganizacion == null;
        var q = Devoluciones(idOrganizacion);

        if (admin && f.IdOrganizacion is int idOrg)
            q = q.Where(x => x.M.IdOrganizacion == idOrg);

        if (f.Estado == Pendiente || f.Estado == Aceptada)
        {
            var estado = f.Estado;
            q = q.Where(x => x.D.Estado == estado);
        }

        foreach (var palabra in Palabras(f.Buscar))
        {
            var p = palabra;
            q = q.Where(x => x.M.Nombre.Contains(p)
                             || x.U.Nombre.Contains(p)
                             || x.U.ApellidoPaterno.Contains(p)
                             || (x.U.ApellidoMaterno != null && x.U.ApellidoMaterno.Contains(p))
                             || (admin && x.M.IdOrganizacionNavigation.Nombre.Contains(p)));
        }

        var (pagina, tamano) = Paginar(f.Pagina, f.Tamano, 50);
        var total = await q.CountAsync();

        var filas = await q
            .OrderBy(x => x.D.Estado == Pendiente ? 0 : 1)          
            .ThenByDescending(x => x.D.FechaSolicitud).ThenByDescending(x => x.D.IdDevolucion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(x => new
            {
                x.D.IdDevolucion,
                x.D.FechaSolicitud,
                x.D.Estado,
                Mascota = x.M.Nombre,
                x.M.Especie,
                x.M.IdOrganizacion,
                Organizacion = x.M.IdOrganizacionNavigation.Nombre,
                x.U.Nombre,
                x.U.ApellidoPaterno,
                x.U.ApellidoMaterno
            })
            .ToListAsync();

        var items = filas.Select(r => new DevolucionListaDto
        {
            IdDevolucion = r.IdDevolucion,
            Codigo = Codigo(r.IdDevolucion),
            Adoptante = NombreCompleto(r.Nombre, r.ApellidoPaterno, r.ApellidoMaterno),
            Mascota = r.Mascota,
            Especie = r.Especie,
            FechaSolicitud = r.FechaSolicitud,
            Estado = r.Estado,
            IdOrganizacion = r.IdOrganizacion,
            Organizacion = r.Organizacion
        }).ToList();

        return Resultado(items, total, pagina, tamano);
    }

    public async Task<DevolucionDetalleDto> DetalleDevolucionAsync(int idDevolucion, int? idOrganizacion)
    {
        var r = await Devoluciones(idOrganizacion)
            .Where(x => x.D.IdDevolucion == idDevolucion)
            .Select(x => new
            {
                x.D.IdDevolucion,
                x.D.IdAdopcion,
                x.D.Estado,
                x.D.Explicacion,
                x.D.FechaSolicitud,
                x.D.FechaRevision,
                x.D.ObservacionesOrganizacion,
                Organizacion = x.M.IdOrganizacionNavigation.Nombre,

                x.U.Nombre,
                x.U.ApellidoPaterno,
                x.U.ApellidoMaterno,
                x.U.Correo,
                x.U.Telefono,
                FotoAdoptante = x.U.FotoPerfil,
                Direccion = x.U.PerfilesAdoptante != null ? x.U.PerfilesAdoptante.Direccion : null,
                Ciudad = x.U.PerfilesAdoptante != null ? x.U.PerfilesAdoptante.Ciudad : null,

                x.M.IdMascota,
                Mascota = x.M.Nombre,
                x.M.Especie,
                x.M.Raza,
                x.M.EdadAproximada,
                FotoMascota = x.M.FotosMascota
                    .OrderByDescending(ft => ft.EsPrincipal).ThenBy(ft => ft.IdFoto)
                    .Select(ft => ft.RutaFoto).FirstOrDefault()
            })
            .FirstOrDefaultAsync()
            ?? throw new NegocioException("Solicitud de devolución no encontrada.", StatusCodes.Status404NotFound);

        var motivos = await _db.DetalleMotivosDevolucions.AsNoTracking()
            .Where(d => d.IdDevolucion == idDevolucion)
            .OrderBy(d => d.IdDetalle)
            .Select(d => d.IdMotivoNavigation.Nombre)
            .ToListAsync();

        return new DevolucionDetalleDto
        {
            IdDevolucion = r.IdDevolucion,
            Codigo = Codigo(r.IdDevolucion),
            IdAdopcion = r.IdAdopcion,
            Estado = r.Estado,
            Organizacion = r.Organizacion,
            Adoptante = new DevolucionAdoptanteDto
            {
                Nombre = NombreCompleto(r.Nombre, r.ApellidoPaterno, r.ApellidoMaterno),
                Correo = r.Correo,
                Telefono = r.Telefono,
                Direccion = Unir(r.Direccion, r.Ciudad),
                FotoUrl = PerfilService.UrlFoto(r.FotoAdoptante)
            },
            Mascota = new DevolucionMascotaDto
            {
                IdMascota = r.IdMascota,
                Nombre = r.Mascota,
                Especie = r.Especie,
                Raza = r.Raza,
                EdadAproximada = r.EdadAproximada,
                FotoUrl = PerfilService.UrlFoto(r.FotoMascota)
            },
            Motivos = motivos,
            Explicacion = r.Explicacion,
            FechaSolicitud = r.FechaSolicitud,
            FechaRevision = r.FechaRevision,
            ObservacionesOrganizacion = r.ObservacionesOrganizacion
        };
    }

    public async Task AceptarDevolucionAsync(int idDevolucion, int idOrganizacion, int idUsuario, AceptarDevolucionRequestDto dto)
    {
        var d = await _db.SolicitudesDevolucions
            .Include(x => x.IdAdopcionNavigation).ThenInclude(a => a.IdSolicitudNavigation).ThenInclude(s => s.IdMascotaNavigation)
            .FirstOrDefaultAsync(x => x.IdDevolucion == idDevolucion
                                      && x.IdAdopcionNavigation.IdSolicitudNavigation.IdMascotaNavigation.IdOrganizacion == idOrganizacion)
            ?? throw new NegocioException("Solicitud de devolución no encontrada.", StatusCodes.Status404NotFound);

        if (d.Estado != Pendiente)
            throw new NegocioException("Esta solicitud ya fue resuelta.", StatusCodes.Status409Conflict);

        var observaciones = Opcional(dto.Observaciones, "las observaciones", 500);

        var solicitud = d.IdAdopcionNavigation.IdSolicitudNavigation;
        var mascota = solicitud.IdMascotaNavigation;

        var idDisponible = await _db.EstadosMascota.AsNoTracking()
            .Where(e => e.Nombre == Disponible).Select(e => e.IdEstadoMascota).FirstAsync();
        var idSinSeguimiento = (await _db.EstadosAdaptacions.AsNoTracking().ToListAsync())
            .First(e => EsSinSeguimiento(e.Nombre)).IdEstadoAdaptacion;

        await using var tx = await _db.Database.BeginTransactionAsync();

        d.Estado = Aceptada;
        d.FechaRevision = DateTime.Now;
        d.IdUsuarioRevisor = idUsuario;
        d.ObservacionesOrganizacion = observaciones;
        mascota.IdEstadoMascota = idDisponible;
        mascota.IdEstadoAdaptacion = idSinSeguimiento;

        _db.HistorialAdaptacions.Add(new HistorialAdaptacion
        {
            IdMascota = mascota.IdMascota,
            IdEstadoAdaptacion = idSinSeguimiento,
            IdUsuarioResponsable = idUsuario,
            Observaciones = $"Devolución {Codigo(d.IdDevolucion)} aceptada."
        });

        _db.Notificaciones.Add(new Notificacione
        {
            IdUsuario = solicitud.IdAdoptante,
            IdSolicitud = solicitud.IdSolicitud,
            Tipo = "Devolucion",
            Titulo = "Devolución aceptada",
            Mensaje = $"La organización aceptó tu solicitud de devolución de {mascota.Nombre} ({Codigo(d.IdDevolucion)})."
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        _logger.LogInformation("Devolución {Codigo} aceptada por usuario {Usuario}; mascota {Mascota} vuelve a Disponible",
            Codigo(d.IdDevolucion), idUsuario, mascota.IdMascota);
    }

    private int DiasRevision => Math.Max(1, _config.GetValue<int?>("Seguimiento:DiasRevision") ?? DiasRevisionPorDefecto);

    private static string Codigo(int idDevolucion) => $"DEV-{idDevolucion:D4}";

    private static DateTime Max(DateTime a, DateTime b) => a >= b ? a : b;

    private static string NombreCompleto(string nombre, string paterno, string? materno) =>
        string.Join(' ', new[] { nombre, paterno, materno }.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!.Trim()));

    private static string? Unir(params string?[] partes)
    {
        var texto = string.Join(", ", partes.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.Trim()));
        return texto.Length == 0 ? null : texto;
    }

    private static IEnumerable<string> Palabras(string? texto) =>
        (texto ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(5);

    private static string? Opcional(string? valor, string campo, int max)
    {
        var v = valor?.Trim();
        if (string.IsNullOrEmpty(v)) return null;
        if (v.Length > max) throw new NegocioException($"Máximo {max} caracteres en {campo}.");
        return v;
    }

    private static bool EsSinSeguimiento(string nombre) => Normalizar(nombre) == Normalizar(SinSeguimiento);

    private static string Normalizar(string texto)
    {
        var descompuesto = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        return new string(descompuesto.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
    }

    private static (int Pagina, int Tamano) Paginar(int pagina, int tamano, int max) =>
        (Math.Max(pagina, 1), Math.Clamp(tamano, 1, max));

    private static PagedResultDto<T> Resultado<T>(List<T> items, int total, int pagina, int tamano) => new()
    {
        Items = items,
        Total = total,
        Pagina = pagina,
        Tamano = tamano,
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamano))
    };
}
