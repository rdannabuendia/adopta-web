using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Adopta.Api.DTOs;
using Adopta.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adopta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]   // todo el módulo es solo para administradores
public class OrganizacionesController : ControllerBase
{
    private readonly IOrganizacionService _registro;
    private readonly IOrganizacionAdminService _admin;

    public OrganizacionesController(IOrganizacionService registro, IOrganizacionAdminService admin)
    {
        _registro = registro;
        _admin = admin;
    }

    private int IdAdmin => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    private async Task<IActionResult> Ejecutar(Func<Task<IActionResult>> accion)
    {
        try { return await accion(); }
        catch (NegocioException ex) { return StatusCode(ex.Status, new ErrorResponseDto { Mensaje = ex.Message }); }
    }

    // ---------- Público: solicitud de registro (formulario de la landing) ----------
    [HttpPost("solicitud")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public Task<IActionResult> Solicitar([FromForm] SolicitudOrganizacionRequestDto request) =>
        Ejecutar(async () =>
        {
            var id = await _registro.RegistrarSolicitudAsync(request);
            return StatusCode(StatusCodes.Status201Created, new SolicitudOrganizacionResponseDto
            {
                IdOrganizacion = id,
                Mensaje = "Solicitud enviada. Te avisaremos por correo cuando sea revisada."
            });
        });

    // ---------- Listado ----------
    [HttpGet]
    public Task<IActionResult> Listar([FromQuery] OrganizacionesQueryDto query) =>
        Ejecutar(async () => Ok(await _admin.ListarAsync(query)));

    [HttpGet("ciudades")]
    public Task<IActionResult> Ciudades() =>
        Ejecutar(async () => Ok(await _admin.CiudadesAsync()));

    [HttpGet("{id:int}")]
    public Task<IActionResult> Detalle(int id) =>
        Ejecutar(async () => Ok(await _admin.DetalleAsync(id)));

    // ---------- Baja por incumplimiento ----------
    [HttpGet("motivos-baja")]
    public Task<IActionResult> MotivosBaja() =>
        Ejecutar(async () => Ok(await _admin.MotivosBajaAsync()));

    [HttpPost("{id:int}/baja")]
    public Task<IActionResult> DarDeBaja(int id, [FromBody] BajaOrganizacionRequestDto request) =>
        Ejecutar(async () =>
        {
            await _admin.DarDeBajaAsync(id, IdAdmin, request);
            return Ok(new MensajeResponseDto { Mensaje = "La organización fue dada de baja." });
        });

    // ---------- Solicitudes de registro ----------
    [HttpGet("solicitudes")]
    public Task<IActionResult> Solicitudes([FromQuery] OrganizacionesQueryDto query) =>
        Ejecutar(async () => Ok(await _admin.SolicitudesAsync(query)));

    [HttpPost("{id:int}/aprobar")]
    public Task<IActionResult> Aprobar(int id) =>
        Ejecutar(async () =>
        {
            await _admin.AprobarAsync(id);
            return Ok(new MensajeResponseDto { Mensaje = "Solicitud aprobada. Se envió el correo para crear la contraseña." });
        });

    [HttpPost("{id:int}/rechazar")]
    public Task<IActionResult> Rechazar(int id, [FromBody] RechazarSolicitudRequestDto request) =>
        Ejecutar(async () =>
        {
            await _admin.RechazarAsync(id, request.Motivo);
            return Ok(new MensajeResponseDto { Mensaje = "Solicitud rechazada. Se notificó al solicitante." });
        });

    // ---------- Documentos de la solicitud ----------
    [HttpGet("documentos/{idDocumento:int}")]
    public async Task<IActionResult> Documento(int idDocumento)
    {
        var doc = await _registro.ObtenerDocumentoAsync(idDocumento);
        if (doc is null)
            return NotFound(new ErrorResponseDto { Mensaje = "Documento no encontrado." });

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(doc.Value.Ruta, doc.Value.Mime);
    }
}