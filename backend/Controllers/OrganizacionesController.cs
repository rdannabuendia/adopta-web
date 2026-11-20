using Adopta.Api.DTOs;
using Adopta.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adopta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizacionesController : ControllerBase
{
    private readonly IOrganizacionService _service;

    public OrganizacionesController(IOrganizacionService service) => _service = service;

    // POST /api/organizaciones/solicitud  
    [HttpPost("solicitud")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Solicitar([FromForm] SolicitudOrganizacionRequestDto request)
    {
        try
        {
            var id = await _service.RegistrarSolicitudAsync(request);
            return StatusCode(StatusCodes.Status201Created, new SolicitudOrganizacionResponseDto
            {
                IdOrganizacion = id,
                Mensaje = "Solicitud enviada. Te avisaremos por correo cuando sea revisada."
            });
        }
        catch (NegocioException ex)
        {
            return StatusCode(ex.Status, new ErrorResponseDto { Mensaje = ex.Message });
        }
    }

    // GET /api/organizaciones/solicitudes?estado=Pendiente  (tabla del módulo de organizaciones)
    [HttpGet("solicitudes")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Solicitudes([FromQuery] string estado = "Pendiente")
    {
        if (estado is not ("Pendiente" or "Activa" or "Inactiva"))
            return BadRequest(new ErrorResponseDto { Mensaje = "Estado no válido." });

        return Ok(await _service.ListarAsync(estado));
    }

    // GET /api/organizaciones/documentos/{id}  (ver el documento para validarlo)
    [HttpGet("documentos/{idDocumento:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Documento(int idDocumento)
    {
        var doc = await _service.ObtenerDocumentoAsync(idDocumento);
        if (doc is null)
            return NotFound(new ErrorResponseDto { Mensaje = "Documento no encontrado." });

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(doc.Value.Ruta, doc.Value.Mime); // inline: el navegador lo muestra
    }
}