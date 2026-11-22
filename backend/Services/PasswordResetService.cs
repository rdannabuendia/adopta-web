using System.Security.Cryptography;
using System.Text;
using Adopta.Api.Data;
using Adopta.Api.Data.Entities;
using Adopta.Api.DTOs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Api.Services;

public interface IPasswordResetService
{
    Task SolicitarAsync(string email);
    Task<ValidateResetTokenResponseDto?> ValidarAsync(string token);
    Task<bool> RestablecerAsync(string token, string nuevaPassword);
    Task<string> GenerarEnlaceAsync(int idUsuario, TimeSpan vigencia);
}

public class PasswordResetService : IPasswordResetService
{
    private readonly AdoptaDbContext _db;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(AdoptaDbContext db, IEmailService email, IConfiguration config,
        IHostEnvironment env, ILogger<PasswordResetService> logger)
    {
        _db = db;
        _email = email;
        _config = config;
        _env = env;
        _logger = logger;
    }

    private int ExpireMinutes => _config.GetValue<int>("PasswordReset:ExpireMinutes", 30);
    private int CooldownSeconds => _config.GetValue<int>("PasswordReset:CooldownSeconds", 25);

    // ---------- 1. Solicitar enlace ----------
    public async Task SolicitarAsync(string email)
    {
        var correo = email.Trim().ToLower();

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == correo && u.Estado == "Activo");
        if (usuario is null) return;

        var ahora = DateTime.UtcNow;

        var hayReciente = await _db.TokensRecuperacions.AnyAsync(t =>
            t.IdUsuario == usuario.IdUsuario && t.FechaCreacion > ahora.AddSeconds(-CooldownSeconds));
        if (hayReciente) return;

        // Invalida los enlaces anteriores
        var pendientes = await _db.TokensRecuperacions
            .Where(t => t.IdUsuario == usuario.IdUsuario && !t.Usado)
            .ToListAsync();
        foreach (var p in pendientes) p.Usado = true;

        var token = NuevoToken();
        _db.TokensRecuperacions.Add(new TokensRecuperacion
        {
            IdUsuario = usuario.IdUsuario,
            TokenHash = Hash(token),
            FechaCreacion = ahora,
            FechaExpiracion = ahora.AddMinutes(ExpireMinutes),
            Usado = false
        });
        await _db.SaveChangesAsync();

        var baseUrl = _config["Frontend:BaseUrl"]!.TrimEnd('/');
        var link = $"{baseUrl}/cambiar-password?token={Uri.EscapeDataString(token)}";

        if (_env.IsDevelopment())
            _logger.LogInformation("Enlace de recuperación (DEV) para {Correo}: {Link}", usuario.Correo, link);

        try
        {
            await _email.EnviarAsync(usuario.Correo, "Restablece tu contraseña - Adopta+",
                ConstruirCorreo(usuario.Nombre, link));
        }
        catch (Exception ex)
        {
            // No se propaga: el usuario siempre ve la misma respuesta
            _logger.LogError(ex, "No se pudo enviar el correo de recuperación a {Correo}", usuario.Correo);
        }
    }

    // ---------- 2. Validar enlace (para mostrar el formulario) ----------
    public async Task<ValidateResetTokenResponseDto?> ValidarAsync(string token)
    {
        var registro = await BuscarValidoAsync(token);
        if (registro is null) return null;

        var usuario = await _db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUsuario == registro.IdUsuario && u.Estado == "Activo");
        if (usuario is null) return null;

        return new ValidateResetTokenResponseDto { EmailEnmascarado = Enmascarar(usuario.Correo) };
    }

    public async Task<bool> RestablecerAsync(string token, string nuevaPassword)
    {
        var registro = await BuscarValidoAsync(token);
        if (registro is null) return false;

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == registro.IdUsuario && u.Estado == "Activo");
        if (usuario is null) return false;

        usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(nuevaPassword, 12);
        registro.Usado = true;

        var otros = await _db.TokensRecuperacions
            .Where(t => t.IdUsuario == usuario.IdUsuario && !t.Usado && t.IdToken != registro.IdToken)
            .ToListAsync();
        foreach (var o in otros) o.Usado = true;

        await _db.SaveChangesAsync();
        return true;
    }
    private Task<TokensRecuperacion?> BuscarValidoAsync(string token)
    {
        var hash = Hash(token.Trim());
        var ahora = DateTime.UtcNow;
        return _db.TokensRecuperacions.FirstOrDefaultAsync(t =>
            t.TokenHash == hash && !t.Usado && t.FechaExpiracion > ahora);
    }

    // En PasswordResetService:
    public async Task<string> GenerarEnlaceAsync(int idUsuario, TimeSpan vigencia)
    {
        var ahora = DateTime.UtcNow;

        var pendientes = await _db.TokensRecuperacions
            .Where(t => t.IdUsuario == idUsuario && !t.Usado)
            .ToListAsync();
        foreach (var p in pendientes) p.Usado = true;

        var token = NuevoToken();
        _db.TokensRecuperacions.Add(new TokensRecuperacion
        {
            IdUsuario = idUsuario,
            TokenHash = Hash(token),
            FechaCreacion = ahora,
            FechaExpiracion = ahora.Add(vigencia),
            Usado = false
        });
        await _db.SaveChangesAsync();

        var baseUrl = _config["Frontend:BaseUrl"]!.TrimEnd('/');
        var link = $"{baseUrl}/cambiar-password?token={Uri.EscapeDataString(token)}";

        if (_env.IsDevelopment())
            _logger.LogInformation("Enlace de activación (DEV) para el usuario {Id}: {Link}", idUsuario, link);

        return link;
    }

    private static string NuevoToken() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private static string Enmascarar(string correo)
    {
        var partes = correo.Split('@');
        var local = partes[0];
        var visibles = Math.Min(3, local.Length / 2);
        return "******" + local[^visibles..] + "@" + partes[1];
    }

    private string ConstruirCorreo(string nombre, string link) => $"""
        <div style="font-family:Arial,sans-serif;max-width:480px;margin:auto;color:#222">
          <h2 style="color:#6aa3a3">Adopta+</h2>
          <p>Hola {System.Net.WebUtility.HtmlEncode(nombre)},</p>
          <p>Recibimos una solicitud para restablecer tu contraseña. Haz clic en el botón para crear una nueva:</p>
          <p style="text-align:center;margin:28px 0">
            <a href="{link}" style="background:#6aa3a3;color:#fff;padding:12px 28px;border-radius:10px;text-decoration:none;font-weight:bold">
              Cambiar contraseña
            </a>
          </p>
          <p style="font-size:13px;color:#666">El enlace vence en {ExpireMinutes} minutos y solo puede usarse una vez.
          Si no lo solicitaste, ignora este correo.</p>
        </div>
        """;
}