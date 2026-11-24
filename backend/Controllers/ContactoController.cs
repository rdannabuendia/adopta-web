using Adopta.Api.DTOs;
using Adopta.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adopta.Api.Controllers;

/// <summary>Endpoints públicos que usa la landing (sin sesión).</summary>
[ApiController]
[AllowAnonymous]
public class ContactoController : ControllerBase
{
    private readonly IContactoService _contacto;
    private readonly IPublicoService _publico;

    public ContactoController(IContactoService contacto, IPublicoService publico)
    {
        _contacto = contacto;
        _publico = publico;
    }

    /// <summary>Formulario de contacto → llega a adoptasoporte@gmail.com</summary>
    [HttpPost("api/contacto")]
    public async Task<IActionResult> Enviar([FromBody] ContactoRequestDto dto)
    {
        try
        {
            await _contacto.EnviarAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(new MensajeResponseDto { Mensaje = "Mensaje enviado correctamente." });
        }
        catch (NegocioException ex)
        {
            return StatusCode(ex.Status, new ErrorResponseDto { Mensaje = ex.Message });
        }
    }

    /// <summary>Mascotas disponibles en orden aleatorio para el carrusel de la landing.</summary>
    [HttpGet("api/publico/mascotas")]
    public async Task<IActionResult> MascotasAleatorias([FromQuery] int cantidad = 10) =>
        Ok(await _publico.MascotasAleatoriasAsync(cantidad));
}
