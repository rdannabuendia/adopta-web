using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Adopta.Api.DTOs;
using Adopta.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adopta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]   // solo un administrador puede registrar a otros
public class AdministradoresController : ControllerBase
{
    private readonly IAdministradorService _service;

    public AdministradoresController(IAdministradorService service) => _service = service;

    private int IdActual => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    private async Task<IActionResult> Ejecutar(Func<Task<IActionResult>> accion)
    {
        try { return await accion(); }
        catch (NegocioException ex) { return StatusCode(ex.Status, new ErrorResponseDto { Mensaje = ex.Message }); }
    }

    // GET /api/administradores?buscar=&estado=&pagina=&tamano=
    [HttpGet]
    public Task<IActionResult> Listar([FromQuery] AdministradoresQueryDto query) =>
        Ejecutar(async () => Ok(await _service.ListarAsync(query, IdActual)));

    // GET /api/administradores/5
    [HttpGet("{id:int}")]
    public Task<IActionResult> Detalle(int id) =>
        Ejecutar(async () => Ok(await _service.DetalleAsync(id, IdActual)));

    // POST /api/administradores  → crea, genera contraseña temporal y envía el correo
    [HttpPost]
    public Task<IActionResult> Crear([FromBody] AdministradorRequestDto request) =>
        Ejecutar(async () =>
        {
            var creado = await _service.CrearAsync(request, IdActual);
            return StatusCode(StatusCodes.Status201Created, creado);
        });

    // PUT /api/administradores/5
    [HttpPut("{id:int}")]
    public Task<IActionResult> Actualizar(int id, [FromBody] AdministradorRequestDto request) =>
        Ejecutar(async () => Ok(await _service.ActualizarAsync(id, request, IdActual)));

    // PATCH /api/administradores/5/estado   { "estado": "Activo" | "Inactivo" }
    [HttpPatch("{id:int}/estado")]
    public Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoAdministradorDto request) =>
        Ejecutar(async () =>
        {
            await _service.CambiarEstadoAsync(id, IdActual, request.Estado);
            return Ok(new MensajeResponseDto
            {
                Mensaje = request.Estado == "Activo" ? "El administrador fue reactivado." : "El administrador fue desactivado."
            });
        });

    // POST /api/administradores/5/reenviar-credenciales
    [HttpPost("{id:int}/reenviar-credenciales")]
    public Task<IActionResult> ReenviarCredenciales(int id) =>
        Ejecutar(async () =>
        {
            await _service.ReenviarCredencialesAsync(id);
            return Ok(new MensajeResponseDto { Mensaje = "Enviamos una nueva contraseña temporal al correo del administrador." });
        });
}
