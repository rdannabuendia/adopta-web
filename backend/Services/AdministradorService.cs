using System.Net;
using System.Security.Cryptography;
using Adopta.Api.Data;
using Adopta.Api.Data.Entities;
using Adopta.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Api.Services;

public interface IAdministradorService
{
    Task<PagedResultDto<AdministradorListaDto>> ListarAsync(AdministradoresQueryDto f, int idActual);
    Task<AdministradorDetalleDto> DetalleAsync(int id, int idActual);
    Task<AdministradorDetalleDto> CrearAsync(AdministradorRequestDto dto, int idActual);
    Task<AdministradorDetalleDto> ActualizarAsync(int id, AdministradorRequestDto dto, int idActual);
    Task CambiarEstadoAsync(int id, int idActual, string estado);
    Task ReenviarCredencialesAsync(int id);
}

public class AdministradorService : IAdministradorService
{
    private const string RolAdmin = "Administrador";
    private static readonly string[] EstadosPermitidos = { "Activo", "Inactivo" };

    private readonly AdoptaDbContext _db;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private readonly ILogger<AdministradorService> _logger;

    public AdministradorService(AdoptaDbContext db, IEmailService email, IConfiguration config,
        IHostEnvironment env, ILogger<AdministradorService> logger)
    {
        _db = db;
        _email = email;
        _config = config;
        _env = env;
        _logger = logger;
    }

    // Solo usuarios con rol Administrador
    private IQueryable<Usuario> Admins(bool tracking = false)
    {
        var q = _db.Usuarios.Where(u => u.IdRolNavigation.Nombre == RolAdmin);
        return tracking ? q : q.AsNoTracking();
    }

    // ---------- Listar ----------
    public async Task<PagedResultDto<AdministradorListaDto>> ListarAsync(AdministradoresQueryDto f, int idActual)
    {
        var q = Admins();

        foreach (var palabra in ServiceHelpers.Palabras(f.Buscar))
        {
            var p = palabra;
            q = q.Where(u => u.Nombre.Contains(p) || u.ApellidoPaterno.Contains(p)
                || (u.ApellidoMaterno != null && u.ApellidoMaterno.Contains(p)) || u.Correo.Contains(p));
        }

        if (f.Estado is "Activo" or "Inactivo")
            q = q.Where(u => u.Estado == f.Estado);

        var (pagina, tamano) = ServiceHelpers.Paginar(f.Pagina, f.Tamano, 50);
        var total = await q.CountAsync();

        var filas = await q
            .OrderBy(u => u.Nombre).ThenBy(u => u.ApellidoPaterno)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return ServiceHelpers.Resultado(filas.Select(u => Mapear(u, idActual)).ToList(), total, pagina, tamano);
    }

    // ---------- Detalle ----------
    public async Task<AdministradorDetalleDto> DetalleAsync(int id, int idActual)
    {
        var u = await Admins().FirstOrDefaultAsync(x => x.IdUsuario == id)
            ?? throw new NegocioException("Administrador no encontrado.", StatusCodes.Status404NotFound);
        return MapearDetalle(u, idActual);
    }

    // ---------- Crear ----------
    public async Task<AdministradorDetalleDto> CrearAsync(AdministradorRequestDto dto, int idActual)
    {
        var datos = Normalizar(dto);
        await ValidarCorreoLibreAsync(datos.Correo, null);

        var idRol = await _db.Roles.Where(r => r.Nombre == RolAdmin).Select(r => r.IdRol).FirstOrDefaultAsync();
        if (idRol == 0)
            throw new NegocioException("No existe el rol Administrador en la base de datos.", StatusCodes.Status500InternalServerError);

        var passwordTemporal = GenerarPasswordTemporal();

        var usuario = new Usuario
        {
            IdRol = idRol,
            Nombre = datos.Nombre,
            ApellidoPaterno = datos.ApellidoPaterno,
            ApellidoMaterno = datos.ApellidoMaterno,
            Correo = datos.Correo,
            Telefono = datos.Telefono,
            Contrasena = BCrypt.Net.BCrypt.HashPassword(passwordTemporal, 12),
            Estado = "Activo",
            DebeCambiarPassword = true
        };

        // Si el correo no sale, se deshace el alta: no queda un administrador que nadie puede usar.
        await using var tx = await _db.Database.BeginTransactionAsync();
        _db.Usuarios.Add(usuario);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException) // carrera con el UNIQUE del correo
        {
            throw new NegocioException("Ya existe una cuenta registrada con este correo.", StatusCodes.Status409Conflict);
        }

        await EnviarCredencialesAsync(usuario, passwordTemporal, esReenvio: false);
        await tx.CommitAsync();

        return MapearDetalle(usuario, idActual);
    }

    // ---------- Actualizar datos ----------
    public async Task<AdministradorDetalleDto> ActualizarAsync(int id, AdministradorRequestDto dto, int idActual)
    {
        var usuario = await Admins(tracking: true).FirstOrDefaultAsync(u => u.IdUsuario == id)
            ?? throw new NegocioException("Administrador no encontrado.", StatusCodes.Status404NotFound);

        var datos = Normalizar(dto);
        if (!string.Equals(usuario.Correo, datos.Correo, StringComparison.OrdinalIgnoreCase))
        {
            // Cambiar el correo de quien aún no activa su cuenta dejaría las credenciales en la bandeja equivocada
            if (usuario.DebeCambiarPassword)
                throw new NegocioException(
                    "Este administrador aún no cambia su contraseña temporal; no se puede modificar su correo.",
                    StatusCodes.Status409Conflict);

            await ValidarCorreoLibreAsync(datos.Correo, id);
        }

        usuario.Nombre = datos.Nombre;
        usuario.ApellidoPaterno = datos.ApellidoPaterno;
        usuario.ApellidoMaterno = datos.ApellidoMaterno;
        usuario.Correo = datos.Correo;
        usuario.Telefono = datos.Telefono;

        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            throw new NegocioException("Ya existe una cuenta registrada con este correo.", StatusCodes.Status409Conflict);
        }

        return MapearDetalle(usuario, idActual);
    }

    // ---------- Activar / desactivar (baja lógica: el usuario tiene historial ligado) ----------
    public async Task CambiarEstadoAsync(int id, int idActual, string estado)
    {
        if (!EstadosPermitidos.Contains(estado))
            throw new NegocioException("El estado debe ser Activo o Inactivo.");
        if (id == idActual)
            throw new NegocioException("No puedes cambiar el estado de tu propia cuenta.", StatusCodes.Status409Conflict);

        var usuario = await Admins(tracking: true).FirstOrDefaultAsync(u => u.IdUsuario == id)
            ?? throw new NegocioException("Administrador no encontrado.", StatusCodes.Status404NotFound);

        if (usuario.Estado == "Bloqueado")
            throw new NegocioException("La cuenta está bloqueada; no se puede cambiar su estado desde aquí.", StatusCodes.Status409Conflict);

        usuario.Estado = estado;
        await _db.SaveChangesAsync();
    }

    // ---------- Reenviar credenciales (solo si todavía no cambia la contraseña temporal) ----------
    public async Task ReenviarCredencialesAsync(int id)
    {
        var usuario = await Admins(tracking: true).FirstOrDefaultAsync(u => u.IdUsuario == id)
            ?? throw new NegocioException("Administrador no encontrado.", StatusCodes.Status404NotFound);

        if (!usuario.DebeCambiarPassword)
            throw new NegocioException(
                "Este administrador ya definió su propia contraseña. Si la olvidó, puede usar «¿Olvidaste tu contraseña?» al iniciar sesión.",
                StatusCodes.Status409Conflict);
        if (usuario.Estado != "Activo")
            throw new NegocioException("Activa la cuenta antes de reenviar las credenciales.", StatusCodes.Status409Conflict);

        var passwordTemporal = GenerarPasswordTemporal();
        usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(passwordTemporal, 12);

        await using var tx = await _db.Database.BeginTransactionAsync();
        await _db.SaveChangesAsync();
        await EnviarCredencialesAsync(usuario, passwordTemporal, esReenvio: true);
        await tx.CommitAsync();
    }

    // ---------- Correo con las credenciales ----------
    private async Task EnviarCredencialesAsync(Usuario usuario, string passwordTemporal, bool esReenvio)
    {
        var baseUrl = (_config["Frontend:BaseUrl"] ?? "").TrimEnd('/');
        var loginUrl = $"{baseUrl}/login";
        var nombre = ServiceHelpers.NombreCompleto(usuario.Nombre, usuario.ApellidoPaterno, null);

        var intro = esReenvio
            ? "Generamos una <strong>nueva contraseña temporal</strong> para tu cuenta de administrador (la anterior ya no funciona)."
            : "Se creó una cuenta de <strong>administrador</strong> para ti en la plataforma.";

        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:480px;margin:auto;color:#222">
              <h2 style="color:#6aa3a3">Adopta+</h2>
              <p>Hola {Html(nombre)},</p>
              <p>{intro}</p>
              <table style="margin:20px 0;border-collapse:collapse;width:100%;background:#f4fbfc;border:1px solid #d8eaeb;border-radius:10px">
                <tr><td style="padding:10px 14px;color:#666;width:130px">Correo</td>
                    <td style="padding:10px 14px"><strong>{Html(usuario.Correo)}</strong></td></tr>
                <tr><td style="padding:10px 14px;color:#666">Contraseña temporal</td>
                    <td style="padding:10px 14px"><strong style="font-family:Consolas,monospace;font-size:16px;letter-spacing:.5px">{Html(passwordTemporal)}</strong></td></tr>
              </table>
              <p style="text-align:center;margin:28px 0">
                <a href="{Html(loginUrl)}" style="background:#6aa3a3;color:#fff;padding:12px 28px;border-radius:10px;text-decoration:none;font-weight:bold">Iniciar sesión</a>
              </p>
              <p style="font-size:13px;color:#666">Por seguridad, en tu primer inicio de sesión se te pedirá crear una contraseña propia;
              hasta entonces no podrás usar el sistema. No compartas este correo y bórralo después de cambiar tu contraseña.</p>
            </div>
            """;

        if (_env.IsDevelopment())
            _logger.LogInformation("Credenciales temporales (DEV) enviadas a {Correo}", usuario.Correo);

        try
        {
            await _email.EnviarAsync(usuario.Correo,
                esReenvio ? "Nuevas credenciales de acceso - Adopta+" : "Tu cuenta de administrador - Adopta+", html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar las credenciales a {Correo}", usuario.Correo);
            // Al propagar, la transacción abierta se revierte al liberarse (no se confirma el alta ni el cambio)
            throw new NegocioException(
                "No se pudo enviar el correo con las credenciales, así que no se realizó el cambio. Verifica el correo e inténtalo de nuevo.",
                StatusCodes.Status502BadGateway);
        }
    }

    // ---------- Helpers ----------
    private async Task ValidarCorreoLibreAsync(string correo, int? idExcluir)
    {
        var excluir = idExcluir ?? 0;
        var existe = await _db.Usuarios.AnyAsync(u => u.Correo.ToLower() == correo && u.IdUsuario != excluir);
        if (existe)
            throw new NegocioException("Ya existe una cuenta registrada con este correo.", StatusCodes.Status409Conflict);
    }

    private static (string Nombre, string ApellidoPaterno, string? ApellidoMaterno, string Correo, string? Telefono)
        Normalizar(AdministradorRequestDto dto)
    {
        var nombre = dto.Nombre?.Trim() ?? "";
        var paterno = dto.ApellidoPaterno?.Trim() ?? "";
        if (nombre.Length == 0) throw new NegocioException("El nombre es obligatorio.");
        if (paterno.Length == 0) throw new NegocioException("El apellido paterno es obligatorio.");

        var correo = (dto.Correo ?? "").Trim().ToLowerInvariant();
        if (correo.Length == 0) throw new NegocioException("El correo es obligatorio.");

        return (nombre, paterno,
            ServiceHelpers.Opcional(dto.ApellidoMaterno, "apellido materno", 60),
            correo,
            ServiceHelpers.Opcional(dto.Telefono, "teléfono", 20));
    }
    internal static string GenerarPasswordTemporal(int longitud = 12)
    {
        const string mayus = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string minus = "abcdefghijkmnopqrstuvwxyz";
        const string numeros = "23456789";
        const string simbolos = "!@#$%*?-";
        const string todos = mayus + minus + numeros + simbolos;

        char Tomar(string alfabeto) => alfabeto[RandomNumberGenerator.GetInt32(alfabeto.Length)];

        var chars = new List<char> { Tomar(mayus), Tomar(minus), Tomar(numeros), Tomar(simbolos) };
        while (chars.Count < longitud) chars.Add(Tomar(todos));

        // Fisher-Yates con CSPRNG para que los 4 obligatorios no queden siempre al inicio
        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return new string(chars.ToArray());
    }

    private static string Html(string texto) => WebUtility.HtmlEncode(texto);

    private static AdministradorListaDto Mapear(Usuario u, int idActual) => new()
    {
        IdUsuario = u.IdUsuario,
        Nombre = ServiceHelpers.NombreCompleto(u.Nombre, u.ApellidoPaterno, u.ApellidoMaterno),
        Correo = u.Correo,
        Telefono = u.Telefono,
        Estado = u.Estado,
        FechaRegistro = u.FechaRegistro,
        PendienteCambioPassword = u.DebeCambiarPassword,
        EsUsuarioActual = u.IdUsuario == idActual
    };

    private static AdministradorDetalleDto MapearDetalle(Usuario u, int idActual) => new()
    {
        IdUsuario = u.IdUsuario,
        Nombre = ServiceHelpers.NombreCompleto(u.Nombre, u.ApellidoPaterno, u.ApellidoMaterno),
        NombrePila = u.Nombre,
        ApellidoPaterno = u.ApellidoPaterno,
        ApellidoMaterno = u.ApellidoMaterno,
        Correo = u.Correo,
        Telefono = u.Telefono,
        Estado = u.Estado,
        FechaRegistro = u.FechaRegistro,
        PendienteCambioPassword = u.DebeCambiarPassword,
        EsUsuarioActual = u.IdUsuario == idActual
    };
}
