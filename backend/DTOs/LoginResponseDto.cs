namespace Adopta.Api.DTOs;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public bool DebeCambiarPassword { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public UsuarioDto Usuario { get; set; } = new();
}
public class ErrorResponseDto
{
    public string Mensaje { get; set; } = string.Empty;
}
