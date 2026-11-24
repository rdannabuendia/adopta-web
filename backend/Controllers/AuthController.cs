using Adopta.Api.DTOs;
using Adopta.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adopta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPasswordResetService _resetService;

    public AuthController(IAuthService authService, IPasswordResetService resetService)
    {
        _authService = authService;
        _resetService = resetService;
    }

    private string PrimerError() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault()
        ?? "Datos inválidos.";

    // POST /api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponseDto { Mensaje = PrimerError() });

        try
        {
            var resultado = await _authService.LoginAsync(request);

            if (resultado is null)
                return Unauthorized(new ErrorResponseDto { Mensaje = "Correo o contraseña incorrectos." });

            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex) // cuenta Inactiva o Bloqueada
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseDto { Mensaje = ex.Message });
        }
    }

    // POST /api/auth/forgot-password
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponseDto { Mensaje = PrimerError() });

        await _resetService.SolicitarAsync(request.Email);

        // Misma respuesta exista o no el correo
        return Ok(new MensajeResponseDto
        {
            Mensaje = "Si el correo está registrado, recibirás un enlace para restablecer tu contraseña."
        });
    }

    // POST /api/auth/validate-reset-token
    [HttpPost("validate-reset-token")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateResetToken([FromBody] ValidateResetTokenRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponseDto { Mensaje = PrimerError() });

        var resultado = await _resetService.ValidarAsync(request.Token);

        if (resultado is null)
            return BadRequest(new ErrorResponseDto { Mensaje = "El enlace no es válido o ya expiró." });

        return Ok(resultado);
    }

    // POST /api/auth/reset-password
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponseDto { Mensaje = PrimerError() });

        var ok = await _resetService.RestablecerAsync(request.Token, request.NuevaPassword);

        if (!ok)
            return BadRequest(new ErrorResponseDto { Mensaje = "El enlace no es válido o ya expiró." });

        return Ok(new MensajeResponseDto { Mensaje = "Tu contraseña se actualizó correctamente." });
    }

    // POST /api/auth/change-password  (sesión iniciada; es el único endpoint permitido con contraseña temporal)
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] CambiarPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponseDto { Mensaje = PrimerError() });

        var sub = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (!int.TryParse(sub, out var idUsuario))
            return Unauthorized(new ErrorResponseDto { Mensaje = "Sesión no válida." });

        try
        {
            return Ok(await _authService.CambiarPasswordAsync(idUsuario, request));
        }
        catch (NegocioException ex)
        {
            return StatusCode(ex.Status, new ErrorResponseDto { Mensaje = ex.Message });
        }
    }

    // GET /api/auth/me
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            Id = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value,
            Nombre = User.Identity?.Name,
            Rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        });
    }
}