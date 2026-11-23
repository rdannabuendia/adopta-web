using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Adopta.Api.DTOs;
using Adopta.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adopta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Organizacion")]
public class SeguimientoController : ControllerBase
{
    private readonly ISeguimientoService _seguimiento;
    private readonly IMascotaService _mascotas;

    public SeguimientoController(ISeguimientoService seguimiento, IMascotaService mascotas)
    {
        _seguimiento = seguimiento;
        _mascotas = mascotas;
    }

    private int IdUsuario => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    private bool EsAdmin => User.IsInRole("Administrador");

    private async Task<int?> AlcanceAsync() =>
        EsAdmin ? null : await _mascotas.IdOrganizacionDeUsuarioAsync(IdUsuario);

    private async Task<IActionResult> Ejecutar(Func<Task<IActionResult>> accion)
    {
        try { return await accion(); }
        catch (NegocioException ex) { return StatusCode(ex.Status, new ErrorResponseDto { Mensaje = ex.Message }); }
    }

    // ---------- Adopciones en seguimiento ----------
    [HttpGet]
    public Task<IActionResult> Listar([FromQuery] SeguimientoQueryDto query) =>
        Ejecutar(async () => Ok(await _seguimiento.ListarAsync(await AlcanceAsync(), query)));

    [HttpGet("estados")]
    public Task<IActionResult> Estados() =>
        Ejecutar(async () => Ok(await _seguimiento.EstadosAdaptacionAsync()));

    [HttpGet("{id:int}")]
    public Task<IActionResult> Detalle(int id) =>
        Ejecutar(async () => Ok(await _seguimiento.DetalleAsync(id, await AlcanceAsync())));

    [HttpPost("{id:int}/adaptacion")]
    [Authorize(Roles = "Organizacion")]
    public Task<IActionResult> ActualizarAdaptacion(int id, [FromBody] ActualizarAdaptacionRequestDto request) =>
        Ejecutar(async () =>
        {
            var idOrg = await _mascotas.IdOrganizacionDeUsuarioAsync(IdUsuario);
            await _seguimiento.ActualizarAdaptacionAsync(id, idOrg, IdUsuario, request);
            return Ok(new MensajeResponseDto { Mensaje = "La adaptación se actualizó correctamente." });
        });

    // ---------- Solicitudes de devolución ----------
    [HttpGet("devoluciones")]
    public Task<IActionResult> Devoluciones([FromQuery] DevolucionQueryDto query) =>
        Ejecutar(async () => Ok(await _seguimiento.ListarDevolucionesAsync(await AlcanceAsync(), query)));

    [HttpGet("devoluciones/{id:int}")]
    public Task<IActionResult> DetalleDevolucion(int id) =>
        Ejecutar(async () => Ok(await _seguimiento.DetalleDevolucionAsync(id, await AlcanceAsync())));

    [HttpPost("devoluciones/{id:int}/aceptar")]
    [Authorize(Roles = "Organizacion")]
    public Task<IActionResult> AceptarDevolucion(int id, [FromBody] AceptarDevolucionRequestDto request) =>
        Ejecutar(async () =>
        {
            var idOrg = await _mascotas.IdOrganizacionDeUsuarioAsync(IdUsuario);
            await _seguimiento.AceptarDevolucionAsync(id, idOrg, IdUsuario, request ?? new AceptarDevolucionRequestDto());
            return Ok(new MensajeResponseDto { Mensaje = "La solicitud de devolución fue aceptada." });
        });
}
