using Adopta.Api.Data;          
using Adopta.Api.DTOs;
using Adopta.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Api.Services;

public interface IOrganizacionAdminService
{
    Task<PagedResultDto<OrganizacionListaDto>> ListarAsync(OrganizacionesQueryDto f);
    Task<List<string>> CiudadesAsync();
    Task<OrganizacionDetalleDto> DetalleAsync(int id);
    Task<List<MotivoBajaDto>> MotivosBajaAsync();
    Task DarDeBajaAsync(int id, int idAdmin, BajaOrganizacionRequestDto dto);

    Task<PagedResultDto<SolicitudRegistroListaDto>> SolicitudesAsync(OrganizacionesQueryDto f);
    Task AprobarAsync(int id);
    Task RechazarAsync(int id, string motivo);
}

public class OrganizacionAdminService : IOrganizacionAdminService
{
    private readonly AdoptaDbContext _db;
    private readonly IEmailService _email;
    private readonly IPasswordResetService _reset;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private readonly ILogger<OrganizacionAdminService> _logger;

    public OrganizacionAdminService(AdoptaDbContext db, IEmailService email, IPasswordResetService reset,
        IConfiguration config, IHostEnvironment env, ILogger<OrganizacionAdminService> logger)
    {
        _db = db;
        _email = email;
        _reset = reset;
        _config = config;
        _env = env;
        _logger = logger;
    }

    // ---------- Listado (organizaciones ya resueltas: Activa / Inactiva) ----------
    public async Task<PagedResultDto<OrganizacionListaDto>> ListarAsync(OrganizacionesQueryDto f)
    {
        var buscar = f.Buscar?.Trim();
        var estado = f.Estado;
        var tipo = f.Tipo;
        var ciudad = f.Ciudad;

        var q = from o in _db.Organizaciones.AsNoTracking()
                join u in _db.Usuarios.AsNoTracking() on o.IdUsuario equals u.IdUsuario
                where o.Estado != "Pendiente"
                select new { o, u };

        if (!string.IsNullOrEmpty(buscar))
            q = q.Where(x => x.o.Nombre.Contains(buscar) || (x.o.Ciudad != null && x.o.Ciudad.Contains(buscar)));
        if (estado is "Activa" or "Inactiva")
            q = q.Where(x => x.o.Estado == estado);
        if (!string.IsNullOrEmpty(tipo))
            q = q.Where(x => x.o.TipoOrganizacion == tipo);
        if (!string.IsNullOrEmpty(ciudad))
            q = q.Where(x => x.o.Ciudad == ciudad);

        var (pagina, tamano) = Paginar(f);
        var total = await q.CountAsync();

        var items = await q
            .OrderBy(x => x.o.Nombre)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(x => new OrganizacionListaDto
            {
                IdOrganizacion = x.o.IdOrganizacion,
                Nombre = x.o.Nombre,
                FotoUrl = x.u.FotoPerfil,
                Ubicacion = x.o.Ciudad ?? x.o.Direccion,
                TipoOrganizacion = x.o.TipoOrganizacion,
                TotalMascotas = _db.Mascotas.Count(m => m.IdOrganizacion == x.o.IdOrganizacion),
                Estado = x.o.Estado
            })
            .ToListAsync();

        foreach (var i in items) i.FotoUrl = PerfilService.UrlFoto(i.FotoUrl);

        return Resultado(items, total, pagina, tamano);
    }

    public Task<List<string>> CiudadesAsync() =>
        _db.Organizaciones.AsNoTracking()
            .Where(o => o.Estado != "Pendiente" && o.Ciudad != null && o.Ciudad != "")
            .Select(o => o.Ciudad!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

    // ---------- Detalle (sirve para organizaciones y para solicitudes) ----------
    public async Task<OrganizacionDetalleDto> DetalleAsync(int id)
    {
        var datos = await (from org in _db.Organizaciones.AsNoTracking()
                           join u in _db.Usuarios.AsNoTracking() on org.IdUsuario equals u.IdUsuario
                           where org.IdOrganizacion == id
                           select new { o = org, u }).FirstOrDefaultAsync()
            ?? throw new NegocioException("Organización no encontrada.", StatusCodes.Status404NotFound);

        var docs = await _db.DocumentosOrganizacions.AsNoTracking()
            .Where(d => d.IdOrganizacion == id)
            .OrderBy(d => d.IdDocumento)
            .ToListAsync();

        var totalMascotas = await _db.Mascotas.CountAsync(m => m.IdOrganizacion == id);

        BajaInfoDto? baja = null;
        if (datos.o.Estado == "Inactiva")
        {
            baja = await (from b in _db.BajasOrganizacions.AsNoTracking()
                          join m in _db.MotivosBajas.AsNoTracking() on b.IdMotivoBaja equals m.IdMotivoBaja
                          where b.IdOrganizacion == id
                          orderby b.FechaRegistro descending
                          select new BajaInfoDto
                          {
                              Motivo = m.Nombre,
                              Descripcion = b.Descripcion,
                              FechaRevision = b.FechaRevision,
                              FechaRegistro = b.FechaRegistro
                          }).FirstOrDefaultAsync();
        }

        var o = datos.o;
        return new OrganizacionDetalleDto
        {
            IdOrganizacion = o.IdOrganizacion,
            Nombre = o.Nombre,
            FotoUrl = PerfilService.UrlFoto(datos.u.FotoPerfil),
            Correo = o.CorreoContacto ?? datos.u.Correo,
            Telefono = o.Telefono,
            Direccion = o.Direccion,
            Ciudad = o.Ciudad,
            CodigoPostal = o.CodigoPostal,
            TipoOrganizacion = o.TipoOrganizacion,
            Descripcion = o.Descripcion,
            SitioWeb = o.SitioWeb,
            HorarioAtencion = o.HorarioAtencion,
            Facebook = o.Facebook,
            Instagram = o.Instagram,
            Youtube = o.Youtube,
            Estado = o.Estado,
            FechaRegistro = o.FechaRegistro,
            TotalMascotas = totalMascotas,
            Documentos = docs.Select(d => new DocumentoOrganizacionDto
            {
                IdDocumento = d.IdDocumento,
                Tipo = d.TipoDocumento,
                NombreOriginal = d.NombreOriginal,
                TamanoBytes = d.TamanoBytes
            }).ToList(),
            Baja = baja
        };
    }

    // ---------- Baja por incumplimiento ----------
    public Task<List<MotivoBajaDto>> MotivosBajaAsync() =>
        _db.MotivosBajas.AsNoTracking()
            .OrderBy(m => m.IdMotivoBaja)
            .Select(m => new MotivoBajaDto { IdMotivoBaja = m.IdMotivoBaja, Nombre = m.Nombre })
            .ToListAsync();

    public async Task DarDeBajaAsync(int id, int idAdmin, BajaOrganizacionRequestDto dto)
    {
        var descripcion = dto.Descripcion.Trim();
        if (descripcion.Length < 10)
            throw new NegocioException("La descripción debe tener al menos 10 caracteres.");
        if (dto.FechaRevision.Date > DateTime.Today.AddDays(1))
            throw new NegocioException("La fecha de revisión no puede ser futura.");

        var org = await _db.Organizaciones.FirstOrDefaultAsync(o => o.IdOrganizacion == id)
            ?? throw new NegocioException("Organización no encontrada.", StatusCodes.Status404NotFound);
        if (org.Estado != "Activa")
            throw new NegocioException("Solo se puede dar de baja a una organización activa.", StatusCodes.Status409Conflict);

        var motivo = await _db.MotivosBajas.AsNoTracking().FirstOrDefaultAsync(m => m.IdMotivoBaja == dto.IdMotivoBaja)
            ?? throw new NegocioException("El motivo seleccionado no es válido.");

        var usuario = await _db.Usuarios.FirstAsync(u => u.IdUsuario == org.IdUsuario);

        await using var tx = await _db.Database.BeginTransactionAsync();

        org.Estado = "Inactiva";
        usuario.Estado = "Bloqueado"; 
        _db.BajasOrganizacions.Add(new BajasOrganizacion
        {
            IdOrganizacion = id,
            IdMotivoBaja = motivo.IdMotivoBaja,
            IdUsuarioAdmin = idAdmin,
            Descripcion = descripcion,
            FechaRevision = dto.FechaRevision.Date
        });
        await _db.SaveChangesAsync();

        // Sus mascotas disponibles dejan de mostrarse
        await _db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE mascotas
            SET id_estado_mascota = (SELECT id_estado_mascota FROM estados_mascota WHERE nombre = 'No disponible')
            WHERE id_organizacion = {id}
              AND id_estado_mascota = (SELECT id_estado_mascota FROM estados_mascota WHERE nombre = 'Disponible')");

        await tx.CommitAsync();

        await EnviarAsync(usuario.Correo, "Tu organización fue dada de baja - Adopta+", Plantilla(
            "Tu organización fue dada de baja",
            EmailPlantilla.Parrafo($"La organización <strong>{Html(org.Nombre)}</strong> fue dada de baja de Adopta+ por incumplimiento.") +
            EmailPlantilla.Datos(("Motivo", motivo.Nombre), ("Detalle", descripcion)) +
            EmailPlantilla.Nota("Mientras la baja esté vigente no podrás iniciar sesión. Si crees que se trata de un error, contacta al administrador."),
            "Tu organización fue dada de baja de Adopta+"));
    }

    // ---------- Solicitudes de registro ----------
    public async Task<PagedResultDto<SolicitudRegistroListaDto>> SolicitudesAsync(OrganizacionesQueryDto f)
    {
        var buscar = f.Buscar?.Trim();
        var tipo = f.Tipo;
        var desde = f.Desde?.Date;
        var hastaExcl = f.Hasta?.Date.AddDays(1);

        var q = _db.Organizaciones.AsNoTracking().Where(o => o.Estado == "Pendiente");

        if (!string.IsNullOrEmpty(buscar)) q = q.Where(o => o.Nombre.Contains(buscar));
        if (!string.IsNullOrEmpty(tipo)) q = q.Where(o => o.TipoOrganizacion == tipo);
        if (desde is not null) q = q.Where(o => o.FechaRegistro >= desde);
        if (hastaExcl is not null) q = q.Where(o => o.FechaRegistro < hastaExcl);

        var (pagina, tamano) = Paginar(f);
        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(o => o.FechaRegistro)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(o => new SolicitudRegistroListaDto
            {
                IdOrganizacion = o.IdOrganizacion,
                Nombre = o.Nombre,
                TipoOrganizacion = o.TipoOrganizacion,
                FechaSolicitud = o.FechaRegistro,
                TotalDocumentos = _db.DocumentosOrganizacions.Count(d => d.IdOrganizacion == o.IdOrganizacion),
                Estado = o.Estado
            })
            .ToListAsync();

        return Resultado(items, total, pagina, tamano);
    }

    public async Task AprobarAsync(int id)
    {
        var org = await _db.Organizaciones.FirstOrDefaultAsync(o => o.IdOrganizacion == id)
            ?? throw new NegocioException("Solicitud no encontrada.", StatusCodes.Status404NotFound);
        if (org.Estado != "Pendiente")
            throw new NegocioException("Esta solicitud ya fue resuelta.", StatusCodes.Status409Conflict);

        var usuario = await _db.Usuarios.FirstAsync(u => u.IdUsuario == org.IdUsuario);

        org.Estado = "Activa";
        usuario.Estado = "Activo";
        await _db.SaveChangesAsync();

        var link = await _reset.GenerarEnlaceAsync(usuario.IdUsuario, TimeSpan.FromDays(7));

        await EnviarAsync(usuario.Correo, "¡Tu organización fue aprobada! - Adopta+", Plantilla(
            "¡Tu organización fue aprobada!",
            EmailPlantilla.Parrafo($"¡Buenas noticias! La solicitud de <strong>{Html(org.Nombre)}</strong> fue aprobada.") +
            EmailPlantilla.Parrafo("Crea tu contraseña para empezar a usar la plataforma:") +
            EmailPlantilla.Boton("Crear contraseña", link) +
            EmailPlantilla.Nota("El enlace vence en 7 días. Después puedes usar “¿Olvidaste tu contraseña?” en el inicio de sesión."),
            "Ya puedes crear tu contraseña y empezar en Adopta+"));
    }

    public async Task RechazarAsync(int id, string motivo)
    {
        motivo = (motivo ?? string.Empty).Trim();
        if (motivo.Length < 10) throw new NegocioException("El motivo debe tener al menos 10 caracteres.");
        if (motivo.Length > 500) throw new NegocioException("El motivo no puede exceder 500 caracteres.");

        var org = await _db.Organizaciones.FirstOrDefaultAsync(o => o.IdOrganizacion == id)
            ?? throw new NegocioException("Solicitud no encontrada.", StatusCodes.Status404NotFound);
        if (org.Estado != "Pendiente")
            throw new NegocioException("Esta solicitud ya fue resuelta.", StatusCodes.Status409Conflict);

        var usuario = await _db.Usuarios.FirstAsync(u => u.IdUsuario == org.IdUsuario);
        var correo = usuario.Correo;
        var nombre = org.Nombre;

        _db.Organizaciones.Remove(org);
        _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync();

        BorrarDocumentos(id);

        await EnviarAsync(correo, "Resultado de tu solicitud - Adopta+", Plantilla(
            "Resultado de tu solicitud",
            EmailPlantilla.Parrafo($"Revisamos la solicitud de registro de <strong>{Html(nombre)}</strong> y por ahora no pudimos aprobarla.") +
            EmailPlantilla.Parrafo("<strong>Motivo</strong>") +
            EmailPlantilla.Cita(EmailPlantilla.HtmlConSaltos(motivo)) +
            EmailPlantilla.Nota("Puedes corregir la información y enviar una nueva solicitud cuando quieras."),
            "Resultado de la revisión de tu solicitud"));
    }

    // ---------- Helpers ----------
    private static (int Pagina, int Tamano) Paginar(OrganizacionesQueryDto f) =>
        (Math.Max(f.Pagina, 1), Math.Clamp(f.Tamano, 1, 50));

    private static PagedResultDto<T> Resultado<T>(List<T> items, int total, int pagina, int tamano) => new()
    {
        Items = items,
        Total = total,
        Pagina = pagina,
        Tamano = tamano,
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamano))
    };

    private static string Html(string texto) => System.Net.WebUtility.HtmlEncode(texto);

    private static string Plantilla(string titulo, string cuerpo, string? preheader = null) =>
        EmailPlantilla.Envolver(titulo, cuerpo, preheader);

    private void BorrarDocumentos(int idOrganizacion)
    {
        try
        {
            var carpeta = Path.Combine(_env.ContentRootPath, _config["Storage:Root"] ?? "Storage",
                "organizaciones", idOrganizacion.ToString());
            if (Directory.Exists(carpeta)) Directory.Delete(carpeta, recursive: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron borrar los documentos de la organización {Id}", idOrganizacion);
        }
    }

    private async Task EnviarAsync(string para, string asunto, string html)
    {
        try { await _email.EnviarAsync(para, asunto, html); }
        catch (Exception ex) { _logger.LogError(ex, "No se pudo enviar el correo '{Asunto}' a {Correo}", asunto, para); }
    }
}