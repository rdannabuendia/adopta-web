using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Adopta.Api.Data;
using Adopta.Api.Data.Entities;
using Adopta.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Api.Services;

public interface IPerfilService
{
    Task<PerfilDto> ObtenerAsync(int idUsuario);
    Task<PerfilDto> ActualizarAsync(int idUsuario, ActualizarPerfilRequestDto dto);
}

public class PerfilService : IPerfilService
{
    private const long MaxFotoBytes = 2 * 1024 * 1024;
    private static readonly string[] TiposOrganizacion =
        { "Refugio", "Rescatista independiente", "Asociación civil / ONG", "Fundación", "Clínica veterinaria", "Otro" };

    private static readonly Regex TelefonoRegex = new(@"^\+?[0-9\s\-()]{10,20}$");
    private static readonly Regex CodigoPostalRegex = new(@"^\d{5}$");

    private static readonly Dictionary<string, byte[]> FormatosFoto = new()
    {
        [".png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
        [".jpg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        [".jpeg"] = new byte[] { 0xFF, 0xD8, 0xFF },
    };

    private readonly AdoptaDbContext _db;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private readonly ILogger<PerfilService> _logger;

    public PerfilService(AdoptaDbContext db, IConfiguration config, IHostEnvironment env, ILogger<PerfilService> logger)
    {
        _db = db;
        _config = config;
        _env = env;
        _logger = logger;
    }

    private string PublicRoot => Path.Combine(_env.ContentRootPath, _config["Storage:PublicRoot"] ?? "Uploads");

    public static string? UrlFoto(string? rutaRelativa) =>
        string.IsNullOrEmpty(rutaRelativa) ? null : $"/uploads/{rutaRelativa}";

    public async Task<PerfilDto> ObtenerAsync(int idUsuario)
    {
        var usuario = await _db.Usuarios.AsNoTracking()
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario)
            ?? throw new NegocioException("Usuario no encontrado.", StatusCodes.Status404NotFound);

        Organizacione? org = null;
        if (usuario.IdRolNavigation.Nombre == "Organizacion")
            org = await _db.Organizaciones.AsNoTracking().FirstOrDefaultAsync(o => o.IdUsuario == idUsuario);

        return Mapear(usuario, org);
    }

    public async Task<PerfilDto> ActualizarAsync(int idUsuario, ActualizarPerfilRequestDto dto)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario)
            ?? throw new NegocioException("Usuario no encontrado.", StatusCodes.Status404NotFound);

        var esOrg = usuario.IdRolNavigation.Nombre == "Organizacion";
        Organizacione? org = null;

        if (esOrg)
        {
            org = await _db.Organizaciones.FirstOrDefaultAsync(o => o.IdUsuario == idUsuario)
                ?? throw new NegocioException("No se encontró la organización.", StatusCodes.Status404NotFound);
            AplicarOrganizacion(usuario, org, dto);
        }
        else
        {
            AplicarUsuario(usuario, dto);
        }

        var fotoAnterior = usuario.FotoPerfil;
        string? fotoNueva = null;

        if (dto.Foto is not null)
        {
            var ext = await ValidarFotoAsync(dto.Foto);
            fotoNueva = await GuardarFotoAsync(dto.Foto, ext);
            usuario.FotoPerfil = fotoNueva;
        }
        else if (dto.EliminarFoto)
        {
            usuario.FotoPerfil = null;
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            if (fotoNueva is not null) BorrarFoto(fotoNueva);
            throw;
        }

        if (fotoAnterior is not null && usuario.FotoPerfil != fotoAnterior)
            BorrarFoto(fotoAnterior);

        return Mapear(usuario, org);
    }

    private static void AplicarOrganizacion(Usuario usuario, Organizacione org, ActualizarPerfilRequestDto dto)
    {
        var nombre = Requerido(dto.Nombre, "el nombre de la organización", 120);

        var tipo = Requerido(dto.TipoOrganizacion, "el tipo de organización", 60);
        if (!TiposOrganizacion.Contains(tipo))
            throw new NegocioException("El tipo de organización no es válido.");

        var direccion = Requerido(dto.Direccion, "la dirección", 200);
        var ciudad = Requerido(dto.Ciudad, "la ciudad", 80);

        var telefono = Requerido(dto.Telefono, "el teléfono", 20);
        if (!TelefonoRegex.IsMatch(telefono))
            throw new NegocioException("El teléfono no es válido.");

        var descripcion = Opcional(dto.Descripcion, "la descripción", 1000);

        var cp = Opcional(dto.CodigoPostal, "el código postal", 10);
        if (cp is not null && !CodigoPostalRegex.IsMatch(cp))
            throw new NegocioException("El código postal debe tener 5 dígitos.");

        var correo = Opcional(dto.CorreoContacto, "el correo de contacto", 120);
        if (correo is not null && !new EmailAddressAttribute().IsValid(correo))
            throw new NegocioException("El correo de contacto no es válido.");

        var web = Opcional(dto.SitioWeb, "el sitio web", 255);
        if (web is not null && !(Uri.TryCreate(web, UriKind.Absolute, out var uri)
                                 && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
            throw new NegocioException("El sitio web debe iniciar con http:// o https://.");

        org.Nombre = nombre;
        org.TipoOrganizacion = tipo;
        org.Descripcion = descripcion;
        org.Direccion = direccion;
        org.Ciudad = ciudad;
        org.CodigoPostal = cp;
        org.Telefono = telefono;
        org.CorreoContacto = correo;
        org.SitioWeb = web;

        // usuarios.nombre es el espejo del nombre de la organización (se ve en la barra superior)
        usuario.Nombre = nombre.Length <= 80 ? nombre : nombre[..80];
        usuario.Telefono = telefono;
    }

    private static void AplicarUsuario(Usuario usuario, ActualizarPerfilRequestDto dto)
    {
        usuario.Nombre = Requerido(dto.Nombre, "tu nombre", 80);
        usuario.ApellidoPaterno = Requerido(dto.ApellidoPaterno, "tu apellido paterno", 60);
        usuario.ApellidoMaterno = Opcional(dto.ApellidoMaterno, "el apellido materno", 60);

        var telefono = Opcional(dto.Telefono, "el teléfono", 20);
        if (telefono is not null && !TelefonoRegex.IsMatch(telefono))
            throw new NegocioException("El teléfono no es válido.");
        usuario.Telefono = telefono;
    }

    private static string Requerido(string? valor, string campo, int max)
    {
        var v = valor?.Trim();
        if (string.IsNullOrEmpty(v)) throw new NegocioException($"Ingresa {campo}.");
        if (v.Length > max) throw new NegocioException($"Máximo {max} caracteres en {campo}.");
        return v;
    }

    private static string? Opcional(string? valor, string campo, int max)
    {
        var v = valor?.Trim();
        if (string.IsNullOrEmpty(v)) return null;
        if (v.Length > max) throw new NegocioException($"Máximo {max} caracteres en {campo}.");
        return v;
    }

    // ---------- Foto ----------
    private static async Task<string> ValidarFotoAsync(IFormFile file)
    {
        if (file.Length == 0) throw new NegocioException("La imagen está vacía.");
        if (file.Length > MaxFotoBytes) throw new NegocioException("La imagen supera los 2 MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FormatosFoto.TryGetValue(ext, out var firma))
            throw new NegocioException("La imagen debe ser JPG o PNG.");

        // Verifica el contenido real, no solo la extensión
        var cabecera = new byte[firma.Length];
        await using var s = file.OpenReadStream();
        var leidos = await s.ReadAsync(cabecera.AsMemory(0, cabecera.Length));
        if (leidos < cabecera.Length || !cabecera.SequenceEqual(firma))
            throw new NegocioException("La imagen está dañada o no coincide con su formato.");

        return ext == ".jpeg" ? ".jpg" : ext;
    }

    private async Task<string> GuardarFotoAsync(IFormFile file, string ext)
    {
        var carpeta = Path.Combine(PublicRoot, "perfiles");
        Directory.CreateDirectory(carpeta);

        var nombre = $"{Guid.NewGuid():N}{ext}"; // nombre nuevo cada vez: evita cachés viejas
        await using var output = File.Create(Path.Combine(carpeta, nombre));
        await file.CopyToAsync(output);

        return $"perfiles/{nombre}";
    }

    private void BorrarFoto(string rutaRelativa)
    {
        try
        {
            var raiz = Path.GetFullPath(PublicRoot);
            var ruta = Path.GetFullPath(Path.Combine(raiz, rutaRelativa));

            // Protección contra path traversal
            if (ruta.StartsWith(raiz + Path.DirectorySeparatorChar) && File.Exists(ruta))
                File.Delete(ruta);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo borrar la foto {Ruta}", rutaRelativa);
        }
    }
    private static PerfilDto Mapear(Usuario u, Organizacione? o) => new()
    {
        IdUsuario = u.IdUsuario,
        Rol = u.IdRolNavigation.Nombre,
        Correo = u.Correo,
        NombreCompleto = o is not null ? o.Nombre : $"{u.Nombre} {u.ApellidoPaterno}".Trim(),
        Nombre = u.Nombre,
        ApellidoPaterno = u.ApellidoPaterno,
        ApellidoMaterno = u.ApellidoMaterno,
        Telefono = u.Telefono,
        FotoUrl = UrlFoto(u.FotoPerfil),
        FechaRegistro = o?.FechaRegistro ?? u.FechaRegistro,
        Organizacion = o is null ? null : new PerfilOrganizacionDto
        {
            IdOrganizacion = o.IdOrganizacion,
            Nombre = o.Nombre,
            TipoOrganizacion = o.TipoOrganizacion,
            Descripcion = o.Descripcion,
            Direccion = o.Direccion,
            Ciudad = o.Ciudad,
            CodigoPostal = o.CodigoPostal,
            Telefono = o.Telefono,
            CorreoContacto = o.CorreoContacto,
            SitioWeb = o.SitioWeb,
            Estado = o.Estado
        }
    };
}