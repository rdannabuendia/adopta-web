using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Adopta.Api.DTOs;
using Adopta.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adopta.Api.Controllers;

[ApiController]
[Route("api/solicitudes")]
[Authorize(Roles = "Organizacion")]
public class SolicitudesAdopcionController : ControllerBase
{
    private readonly ISolicitudAdopcionService _solicitudes;
    private readonly IMascotaService _mascotas;

    public SolicitudesAdopcionController(ISolicitudAdopcionService solicitudes, IMascotaService mascotas)
    {
        _solicitudes = solicitudes;
        _mascotas = mascotas;
    }

    private int IdUsuario => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    private async Task<IActionResult> Ejecutar(Func<int, Task<IActionResult>> accion)
    {
        try { return await accion(await _mascotas.IdOrganizacionDeUsuarioAsync(IdUsuario)); }
        catch (NegocioException ex) { return StatusCode(ex.Status, new ErrorResponseDto { Mensaje = ex.Message }); }
    }

    // ---------- Consulta ----------
    [HttpGet]
    public Task<IActionResult> Listar([FromQuery] SolicitudesQueryDto query) =>
        Ejecutar(async idOrg => Ok(await _solicitudes.ListarAsync(idOrg, query)));

    [HttpGet("estados")]
    public Task<IActionResult> Estados() =>
        Ejecutar(async _ => Ok(await _solicitudes.EstadosAsync()));

    [HttpGet("{id:int}")]
    public Task<IActionResult> Detalle(int id) =>
        Ejecutar(async idOrg => Ok(await _solicitudes.DetalleAsync(id, idOrg)));

    // ---------- Documentos adjuntos ----------
    [HttpGet("{id:int}/documentos")]
    public Task<IActionResult> Documentos(int id) =>
        Ejecutar(async idOrg => Ok(await _solicitudes.DocumentosAsync(id, idOrg)));

    [HttpGet("archivos/{idArchivo:int}")]
    public Task<IActionResult> Archivo(int idArchivo) =>
        Ejecutar(async idOrg =>
        {
            var archivo = await _solicitudes.ObtenerArchivoAsync(idArchivo, idOrg);
            if (archivo is null)
                return NotFound(new ErrorResponseDto { Mensaje = "Documento no encontrado." });

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return archivo.Ruta is not null
                ? PhysicalFile(archivo.Ruta, archivo.Mime)
                : File(archivo.Contenido!, archivo.Mime);
        });

    // ---------- Acciones ----------
    [HttpPost("{id:int}/informacion-adicional")]
    public Task<IActionResult> SolicitarInformacion(int id, [FromBody] SolicitarInformacionRequestDto request) =>
        Ejecutar(async idOrg =>
        {
            await _solicitudes.SolicitarInformacionAsync(id, idOrg, IdUsuario, request);
            return Ok(new MensajeResponseDto { Mensaje = "Se envió tu mensaje al adoptante." });
        });

    [HttpPost("{id:int}/resolver")]
    public Task<IActionResult> Resolver(int id, [FromBody] ResolverSolicitudRequestDto request) =>
        Ejecutar(async idOrg =>
        {
            await _solicitudes.ResolverAsync(id, idOrg, IdUsuario, request);
            var aceptada = string.Equals(request.Decision, "Aceptada", StringComparison.OrdinalIgnoreCase);
            return Ok(new MensajeResponseDto
            {
                Mensaje = aceptada
                    ? "La solicitud fue aceptada. Se notificó al adoptante."
                    : "La solicitud fue rechazada. Se notificó al adoptante."
            });
        });
}