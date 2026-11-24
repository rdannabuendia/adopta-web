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
public class MascotasController : ControllerBase
{
    private readonly IMascotaService _mascotas;

    public MascotasController(IMascotaService mascotas) => _mascotas = mascotas;

    private int IdUsuario => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    private bool EsAdmin => User.IsInRole("Administrador");

    private async Task<IActionResult> Ejecutar(Func<Task<IActionResult>> accion)
    {
        try { return await accion(); }
        catch (NegocioException ex) { return StatusCode(ex.Status, new ErrorResponseDto { Mensaje = ex.Message }); }
    }

    // ---------- Admin: organizaciones con su resumen de mascotas ----------
    [HttpGet("organizaciones")]
    [Authorize(Roles = "Administrador")]
    public Task<IActionResult> Organizaciones([FromQuery] OrganizacionesQueryDto query) =>
        Ejecutar(async () => Ok(await _mascotas.OrganizacionesAsync(query)));

    // ---------- Admin: mascotas de una organización ----------
    [HttpGet("organizacion/{idOrganizacion:int}")]
    [Authorize(Roles = "Administrador")]
    public Task<IActionResult> DeOrganizacion(int idOrganizacion, [FromQuery] MascotasQueryDto query) =>
        Ejecutar(async () => Ok(await _mascotas.ListarAsync(idOrganizacion, query, ocultarNoDisponibles: false)));

    // ---------- Organización: mis mascotas ----------
    [HttpGet("mias")]
    [Authorize(Roles = "Organizacion")]
    public Task<IActionResult> Mias([FromQuery] MascotasQueryDto query) =>
        Ejecutar(async () =>
        {
            var idOrg = await _mascotas.IdOrganizacionDeUsuarioAsync(IdUsuario);
            return Ok(await _mascotas.ListarAsync(idOrg, query, ocultarNoDisponibles: true));
        });

    // ---------- Detalle (admin: cualquiera · organización: solo las suyas) ----------
    [HttpGet("{id:int}")]
    public Task<IActionResult> Detalle(int id) =>
        Ejecutar(async () =>
        {
            int? idOrg = EsAdmin ? null : await _mascotas.IdOrganizacionDeUsuarioAsync(IdUsuario);
            return Ok(await _mascotas.DetalleAsync(id, idOrg));
        });

    // ---------- Organización: registrar / editar / cambiar estatus ----------
    [HttpPost]
    [Authorize(Roles = "Organizacion")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(40 * 1024 * 1024)]
    public Task<IActionResult> Crear([FromForm] MascotaRequestDto request) =>
        Ejecutar(async () =>
        {
            var idOrg = await _mascotas.IdOrganizacionDeUsuarioAsync(IdUsuario);
            var id = await _mascotas.CrearAsync(idOrg, request);
            return StatusCode(StatusCodes.Status201Created,
                new MascotaGuardadaResponseDto { IdMascota = id, Mensaje = "Mascota registrada correctamente." });
        });

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organizacion")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(40 * 1024 * 1024)]
    public Task<IActionResult> Actualizar(int id, [FromForm] MascotaRequestDto request) =>
        Ejecutar(async () =>
        {
            var idOrg = await _mascotas.IdOrganizacionDeUsuarioAsync(IdUsuario);
            await _mascotas.ActualizarAsync(id, idOrg, request);
            return Ok(new MensajeResponseDto { Mensaje = "Los cambios se guardaron correctamente." });
        });

    [HttpPost("{id:int}/estado")]
    [Authorize(Roles = "Organizacion")]
    public Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoMascotaRequestDto request) =>
        Ejecutar(async () =>
        {
            var idOrg = await _mascotas.IdOrganizacionDeUsuarioAsync(IdUsuario);
            await _mascotas.CambiarEstadoAsync(id, idOrg, request);
            return Ok(new MensajeResponseDto { Mensaje = "El estatus se actualizó correctamente." });
        });
}