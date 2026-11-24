using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Adopta.Api.DTOs;
using Adopta.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adopta.Api.Controllers;

[ApiController]
[Route("api/notificaciones")]
[Authorize(Roles = "Administrador,Organizacion")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionesService _notificaciones;

    public NotificacionesController(INotificacionesService notificaciones) => _notificaciones = notificaciones;

    private int IdUsuario => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    private async Task<IActionResult> Ejecutar(Func<Task<IActionResult>> accion)
    {
        try { return await accion(); }
        catch (NegocioException ex) { return StatusCode(ex.Status, new ErrorResponseDto { Mensaje = ex.Message }); }
    }

    [HttpGet]
    public Task<IActionResult> Listar([FromQuery] bool soloNoLeidas = false, [FromQuery] int limite = 20) =>
        Ejecutar(async () => Ok(await _notificaciones.ListarAsync(IdUsuario, soloNoLeidas, limite)));

    [HttpGet("contador")]
    public Task<IActionResult> Contador() =>
        Ejecutar(async () => Ok(new NotificacionesContadorDto { NoLeidas = await _notificaciones.ContarNoLeidasAsync(IdUsuario) }));

    [HttpPost("{id:int}/leer")]
    public Task<IActionResult> Leer(int id) =>
        Ejecutar(async () =>
        {
            await _notificaciones.MarcarLeidaAsync(IdUsuario, id);
            return Ok(new MensajeResponseDto { Mensaje = "Notificación marcada como leída." });
        });

    [HttpPost("leer-todas")]
    public Task<IActionResult> LeerTodas() =>
        Ejecutar(async () =>
        {
            await _notificaciones.MarcarTodasLeidasAsync(IdUsuario);
            return Ok(new MensajeResponseDto { Mensaje = "Todas las notificaciones se marcaron como leídas." });
        });
}
