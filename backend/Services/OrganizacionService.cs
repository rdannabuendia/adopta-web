using System.Security.Cryptography;
using Adopta.Api.Data;
using Adopta.Api.Data.Entities;
using Adopta.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Api.Services;

public class NegocioException : Exception
{
    public int Status { get; }

    public NegocioException(string mensaje, int status = StatusCodes.Status400BadRequest) : base(mensaje)
        => Status = status;
}

public interface IOrganizacionService
{
    Task<int> RegistrarSolicitudAsync(SolicitudOrganizacionRequestDto dto);
    Task<List<OrganizacionSolicitudDto>> ListarAsync(string estado);
    Task<(string Ruta, string Mime, string Nombre)?> ObtenerDocumentoAsync(int idDocumento);
}

public class OrganizacionService : IOrganizacionService
{
    private const long MaxBytes = 5 * 1024 * 1024;

    // Extensión → (mime, firma de los primeros bytes del archivo)
    private static readonly Dictionary<string, (string Mime, byte[] Firma)> Formatos = new()
    {
        [".pdf"] = ("application/pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 }),
        [".png"] = ("image/png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
        [".jpg"] = ("image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF }),
        [".jpeg"] = ("image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF }),
    };

    private readonly AdoptaDbContext _db;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private readonly ILogger<OrganizacionService> _logger;

    public OrganizacionService(AdoptaDbContext db, IEmailService email, IConfiguration config,
        IHostEnvironment env, ILogger<OrganizacionService> logger)
    {
        _db = db;
        _email = email;
        _config = config;
        _env = env;
        _logger = logger;
    }

    private string StorageRoot => Path.Combine(_env.ContentRootPath, _config["Storage:Root"] ?? "Storage");

    // ---------- 1. Registrar solicitud ----------
    public async Task<int> RegistrarSolicitudAsync(SolicitudOrganizacionRequestDto dto)
    {
        if (!dto.AceptaTerminos)
            throw new NegocioException("Debes aceptar los términos y condiciones.");

        var nombre = dto.Nombre.Trim();
        var correo = dto.Email.Trim();
        var correoLower = correo.ToLower();

        var archivos = new (string Tipo, string Etiqueta, IFormFile File)[]
        {
            ("ExistenciaLegal", "comprobante de existencia legal", dto.ExistenciaLegal),
            ("ComprobanteDomicilio", "comprobante de domicilio", dto.ComprobanteDomicilio),
            ("IdentificacionResponsable", "identificación del responsable", dto.IdentificacionResponsable),
        };

        // Validar archivos antes de tocar la BD
        var formatos = new List<(string Ext, string Mime)>();
        foreach (var a in archivos)
            formatos.Add(await ValidarArchivoAsync(a.File, a.Etiqueta));

        if (await _db.Usuarios.AnyAsync(u => u.Correo.ToLower() == correoLower))
            throw new NegocioException(
                "Ya existe una cuenta o solicitud registrada con este correo.", StatusCodes.Status409Conflict);

        var idRolOrg = await _db.Roles.Where(r => r.Nombre == "Organizacion").Select(r => r.IdRol).FirstAsync();
        var idRolAdmin = await _db.Roles.Where(r => r.Nombre == "Administrador").Select(r => r.IdRol).FirstAsync();

        string? carpetaAbs = null;

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // Usuario inactivo con contraseña aleatoria (la define al ser aprobado, con el enlace por correo)
            var usuario = new Usuario
            {
                IdRol = idRolOrg,
                Nombre = Recortar(nombre, 80),
                ApellidoPaterno = "",
                Correo = correo,
                Contrasena = BCrypt.Net.BCrypt.HashPassword(
                    Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)), 12),
                Telefono = dto.Telefono.Trim(),
                Estado = "Inactivo"
            };
            _db.Usuarios.Add(usuario);

            try { await _db.SaveChangesAsync(); }
            catch (DbUpdateException) // carrera con la restricción UNIQUE del correo
            {
                throw new NegocioException(
                    "Ya existe una cuenta o solicitud registrada con este correo.", StatusCodes.Status409Conflict);
            }

            var org = new Organizacione
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = nombre,
                TipoOrganizacion = dto.TipoOrganizacion.Trim(),
                Direccion = dto.Direccion.Trim(),
                Telefono = dto.Telefono.Trim(),
                CorreoContacto = correo,
                Estado = "Pendiente"
            };
            _db.Organizaciones.Add(org);
            await _db.SaveChangesAsync();

            // Documentos en disco (fuera de wwwroot: solo se sirven por el endpoint de admin)
            var carpetaRel = $"organizaciones/{org.IdOrganizacion}";
            carpetaAbs = Path.Combine(StorageRoot, "organizaciones", org.IdOrganizacion.ToString());
            Directory.CreateDirectory(carpetaAbs);

            for (var i = 0; i < archivos.Length; i++)
            {
                var (tipo, _, file) = archivos[i];
                var (ext, mime) = formatos[i];
                var nombreAlmacenado = $"{Guid.NewGuid():N}{ext}";

                string hash;
                await using (var input = file.OpenReadStream())
                {
                    hash = Convert.ToHexString(await SHA256.HashDataAsync(input)).ToLowerInvariant();
                    input.Position = 0;
                    await using var output = File.Create(Path.Combine(carpetaAbs, nombreAlmacenado));
                    await input.CopyToAsync(output);
                }

                _db.DocumentosOrganizacions.Add(new DocumentosOrganizacion
                {
                    IdOrganizacion = org.IdOrganizacion,
                    TipoDocumento = tipo,
                    NombreOriginal = Recortar(Path.GetFileName(file.FileName), 255),
                    RutaRelativa = $"{carpetaRel}/{nombreAlmacenado}",
                    MimeType = mime,
                    TamanoBytes = (uint)file.Length,
                    HashSha256 = hash
                });
            }

            // Notificación para cada administrador activo
            var admins = await _db.Usuarios
                .Where(u => u.IdRol == idRolAdmin && u.Estado == "Activo")
                .Select(u => u.IdUsuario)
                .ToListAsync();

            foreach (var idAdmin in admins)
            {
                _db.Notificaciones.Add(new Notificacione
                {
                    IdUsuario = idAdmin,
                    Titulo = "Nueva solicitud de organización",
                    Mensaje = $"{org.Nombre} solicitó registrarse y está pendiente de revisión.",
                    Tipo = "SolicitudOrganizacion"
                });
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            await EnviarConfirmacionAsync(correo, nombre);
            return org.IdOrganizacion;
        }
        catch
        {
            // Rollback de la BD (al salir del using) y limpieza de archivos
            if (carpetaAbs is not null && Directory.Exists(carpetaAbs))
                Directory.Delete(carpetaAbs, recursive: true);
            throw;
        }
    }

    // ---------- 2. Listado para el módulo de organizaciones (admin) ----------
    public async Task<List<OrganizacionSolicitudDto>> ListarAsync(string estado)
    {
        var orgs = await _db.Organizaciones.AsNoTracking()
            .Where(o => o.Estado == estado)
            .OrderBy(o => o.FechaRegistro)
            .ToListAsync();

        var ids = orgs.Select(o => o.IdOrganizacion).ToList();
        var docs = await _db.DocumentosOrganizacions.AsNoTracking()
            .Where(d => ids.Contains(d.IdOrganizacion))
            .ToListAsync();

        return orgs.Select(o => new OrganizacionSolicitudDto
        {
            IdOrganizacion = o.IdOrganizacion,
            Nombre = o.Nombre,
            TipoOrganizacion = o.TipoOrganizacion,
            Correo = o.CorreoContacto,
            Telefono = o.Telefono,
            Direccion = o.Direccion,
            FechaRegistro = o.FechaRegistro,
            Estado = o.Estado,
            Documentos = docs.Where(d => d.IdOrganizacion == o.IdOrganizacion)
                .Select(d => new DocumentoOrganizacionDto
                {
                    IdDocumento = d.IdDocumento,
                    Tipo = d.TipoDocumento,
                    NombreOriginal = d.NombreOriginal,
                    TamanoBytes = d.TamanoBytes
                }).ToList()
        }).ToList();
    }

    // ---------- 3. Ver un documento (admin) ----------
    public async Task<(string Ruta, string Mime, string Nombre)?> ObtenerDocumentoAsync(int idDocumento)
    {
        var doc = await _db.DocumentosOrganizacions.AsNoTracking()
            .FirstOrDefaultAsync(d => d.IdDocumento == idDocumento);
        if (doc is null) return null;

        var raiz = Path.GetFullPath(StorageRoot);
        var ruta = Path.GetFullPath(Path.Combine(raiz, doc.RutaRelativa));

        // Protección contra path traversal
        if (!ruta.StartsWith(raiz + Path.DirectorySeparatorChar) || !File.Exists(ruta))
            return null;

        return (ruta, doc.MimeType, doc.NombreOriginal);
    }

    // ---------- Helpers ----------
    private static async Task<(string Ext, string Mime)> ValidarArchivoAsync(IFormFile file, string etiqueta)
    {
        if (file.Length == 0)
            throw new NegocioException($"El archivo de {etiqueta} está vacío.");
        if (file.Length > MaxBytes)
            throw new NegocioException($"El archivo de {etiqueta} supera los 5 MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Formatos.TryGetValue(ext, out var formato))
            throw new NegocioException($"El archivo de {etiqueta} debe ser PDF, JPG o PNG.");

        // Verifica el contenido real, no solo la extensión
        var cabecera = new byte[formato.Firma.Length];
        await using (var s = file.OpenReadStream())
        {
            var leidos = await s.ReadAsync(cabecera.AsMemory(0, cabecera.Length));
            if (leidos < cabecera.Length || !cabecera.SequenceEqual(formato.Firma))
                throw new NegocioException($"El archivo de {etiqueta} está dañado o no coincide con su formato.");
        }

        return (ext, formato.Mime);
    }

    private static string Recortar(string texto, int max) => texto.Length <= max ? texto : texto[..max];

    private async Task EnviarConfirmacionAsync(string correo, string nombreOrg)
    {
        var html = EmailPlantilla.Envolver(
            "Recibimos tu solicitud",
            EmailPlantilla.Parrafo("Hola,") +
            EmailPlantilla.Parrafo($"Recibimos la solicitud de registro de <strong>{EmailPlantilla.Html(nombreOrg)}</strong>. ¡Gracias por querer ser parte de Adopta+!") +
            EmailPlantilla.Parrafo("Nuestro equipo revisará la información y los documentos que enviaste. Te avisaremos por este mismo correo cuando tu cuenta sea aprobada. Mientras tanto no podrás iniciar sesión.") +
            EmailPlantilla.Nota("Si no realizaste esta solicitud, ignora este mensaje."),
            "Tu solicitud de registro está en revisión");

        try { await _email.EnviarAsync(correo, "Recibimos tu solicitud - Adopta+", html); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar la confirmación de solicitud a {Correo}", correo);
        }
    }
}