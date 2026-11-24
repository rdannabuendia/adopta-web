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
public class PerfilController : ControllerBase
{
    private readonly IPerfilService _service;

    public PerfilController(IPerfilService service) => _service = service;

    private int IdUsuario => int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    // GET /api/perfil
    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        try
        {
            return Ok(await _service.ObtenerAsync(IdUsuario));
        }
        catch (NegocioException ex)
        {
            return StatusCode(ex.Status, new ErrorResponseDto { Mensaje = ex.Message });
        }
    }

    // PUT /api/perfil 
    [HttpPut]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Actualizar([FromForm] ActualizarPerfilRequestDto request)
    {
        try
        {
            return Ok(await _service.ActualizarAsync(IdUsuario, request));
        }
        catch (NegocioException ex)
        {
            return StatusCode(ex.Status, new ErrorResponseDto { Mensaje = ex.Message });
        }
    }
}