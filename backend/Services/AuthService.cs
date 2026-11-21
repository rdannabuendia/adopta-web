using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Adopta.Api.Data;
using Adopta.Api.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Adopta.Api.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
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

        var nombreCompleto = $"{usuario.Nombre} {usuario.ApellidoPaterno}".Trim();
        var (token, expiracion) = GenerarToken(usuario.IdUsuario, usuario.Correo, nombreCompleto, rol);
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
                FotoUrl = PerfilService.UrlFoto(usuario.FotoPerfil)
            }
        };
    }

    private (string Token, DateTime Expiracion) GenerarToken(int id, string correo, string nombre, string rol)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiracion = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpireMinutes"] ?? "60"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, correo),
            new Claim(ClaimTypes.Name, nombre),
            new Claim(ClaimTypes.Role, rol)
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expiracion,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiracion);
    }
}