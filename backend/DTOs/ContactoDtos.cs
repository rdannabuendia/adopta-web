using System.ComponentModel.DataAnnotations;

namespace Adopta.Api.DTOs;

/// <summary>Mensaje del formulario de contacto de la landing.</summary>
public class ContactoRequestDto
{
    [Required(ErrorMessage = "Escribe tu nombre.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe tu correo electrónico.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(120, ErrorMessage = "El correo no puede exceder 120 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona un asunto.")]
    [StringLength(120, ErrorMessage = "El asunto no puede exceder 120 caracteres.")]
    public string Asunto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe tu mensaje.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "El mensaje debe tener entre 5 y 500 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Campo trampa para bots: las personas lo dejan vacío (está oculto en la página).</summary>
    public string? Web { get; set; }
}

/// <summary>Mascota que se muestra en el carrusel público de la landing.</summary>
public class MascotaPublicaDto
{
    public int IdMascota { get; set; }
    public string Nombre { get; set; } = null!;
    public string Especie { get; set; } = null!;
    public string? Raza { get; set; }
    public string Sexo { get; set; } = null!;
    public string? Tamano { get; set; }
    public string? EdadAproximada { get; set; }
    public string? FotoUrl { get; set; }
    public string Organizacion { get; set; } = null!;
}
