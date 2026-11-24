using System.ComponentModel.DataAnnotations;

namespace Adopta.Api.DTOs;

public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;
}

public class ValidateResetTokenRequestDto
{
    [Required(ErrorMessage = "El enlace no es válido.")]
    public string Token { get; set; } = string.Empty;
}

public class ValidateResetTokenResponseDto
{
    public string EmailEnmascarado { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "El enlace no es válido.")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,100}$",
        ErrorMessage = "La contraseña debe tener mínimo 8 caracteres, con al menos una letra y un número.")]
    public string NuevaPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu contraseña.")]
    [Compare(nameof(NuevaPassword), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}

/// <summary>Cambio de contraseña con sesión iniciada (primer inicio de sesión de un administrador).</summary>
public class CambiarPasswordRequestDto
{
    [Required(ErrorMessage = "Ingresa tu contraseña actual.")]
    [StringLength(100)]
    public string PasswordActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,100}$",
        ErrorMessage = "La contraseña debe tener mínimo 8 caracteres, con al menos una letra y un número.")]
    public string NuevaPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu contraseña.")]
    [Compare(nameof(NuevaPassword), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}

public class MensajeResponseDto
{
    public string Mensaje { get; set; } = string.Empty;
}