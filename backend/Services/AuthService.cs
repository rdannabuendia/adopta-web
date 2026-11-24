using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Adopta.Api.Data;
using Adopta.Api.DTOs;
using Adopta.Api.Middleware;
using Adopta.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Adopta.Api.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> CambiarPasswordAsync(int idUsuario, CambiarPasswordRequestDto request);
}

public class AuthService : IAuthService
{
    private readonly AdoptaDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AdoptaDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var correo = request.Email.Trim().ToLower();

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == correo);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.Contrasena))
            return null;

        if (usuario.Estado != "Activo")
            throw new UnauthorizedAccessException(
                usuario.Estado == "Bloqueado"
                    ? "Tu cuenta está bloqueada. Contacta al administrador."
                    : "Tu cuenta está inactiva. Contacta al administrador.");

        var rol = usuario.IdRolNavigation.Nombre;
        if (rol is not ("Administrador" or "Organizacion"))
            throw new UnauthorizedAccessException(
                "Esta plataforma es solo para organizaciones y administradores. Usa la aplicación móvil.");

        return ConstruirRespuesta(usuario, rol);
    }

    // Primer inicio de sesión de un administrador: cambia la contraseña temporal por una propia.
    public async Task<LoginResponseDto> CambiarPasswordAsync(int idUsuario, CambiarPasswordRequestDto request)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario && u.Estado == "Activo")
            ?? throw new NegocioException("Usuario no encontrado.", StatusCodes.Status404NotFound);

        // 400 y no 401: el front cierra la sesión ante un 401
        if (!BCrypt.Net.BCrypt.Verify(request.PasswordActual, usuario.Contrasena))
            throw new NegocioException("La contraseña actual no es correcta.");

        if (request.PasswordActual == request.NuevaPassword)
            throw new NegocioException("La nueva contraseña debe ser distinta a la temporal.");

        usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(request.NuevaPassword, 12);
        usuario.DebeCambiarPassword = false;
        await _db.SaveChangesAsync();

        // Token nuevo, ya sin la marca de "debe cambiar contraseña"
        return ConstruirRespuesta(usuario, usuario.IdRolNavigation.Nombre);
    }

    private LoginResponseDto ConstruirRespuesta(Adopta.Api.Data.Entities.Usuario usuario, string rol)
    {
        var nombreCompleto = $"{usuario.Nombre} {usuario.ApellidoPaterno}".Trim();
        var (token, expiracion) = GenerarToken(
            usuario.IdUsuario, usuario.Correo, nombreCompleto, rol, usuario.DebeCambiarPassword);

        return new LoginResponseDto
        {
            Token = token,
            Expiracion = expiracion,
            Usuario = new UsuarioDto
            {
                Id = usuario.IdUsuario,
                Nombre = nombreCompleto,
                Email = usuario.Correo,
                Rol = rol,
                FotoUrl = PerfilService.UrlFoto(usuario.FotoPerfil),
                DebeCambiarPassword = usuario.DebeCambiarPassword
            }
        };
    }

    private (string Token, DateTime Expiracion) GenerarToken(
        int id, string correo, string nombre, string rol, bool debeCambiarPassword)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiracion = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpireMinutes"] ?? "60"));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, id.ToString()),
            new(JwtRegisteredClaimNames.Email, correo),
            new(ClaimTypes.Name, nombre),
            new(ClaimTypes.Role, rol)
        };

        // Mientras exista esta marca, CambioPasswordMiddleware solo deja usar el cambio de contraseña
        if (debeCambiarPassword)
            claims.Add(new Claim(CambioPasswordMiddleware.ClaimType, "true"));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expiracion,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiracion);
    }
}