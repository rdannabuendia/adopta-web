namespace Adopta.Api.Middleware;

/// <summary>
/// Filtro de seguridad del primer inicio de sesión: si el token pertenece a alguien que todavía usa su
/// contraseña temporal, la API solo responde al cambio de contraseña. Así no basta con ocultar el menú
/// en el front: con la contraseña temporal no se puede consumir ningún otro endpoint.
/// </summary>
public class CambioPasswordMiddleware
{
    public const string ClaimType = "debe_cambiar_password";
    public const string Codigo = "CAMBIO_PASSWORD_REQUERIDO";

    private static readonly string[] RutasPermitidas =
    {
        "/api/auth/change-password",
        "/api/auth/me"
    };

    private readonly RequestDelegate _next;

    public CambioPasswordMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var debeCambiar = context.User.Identity?.IsAuthenticated == true
            && context.User.HasClaim(ClaimType, "true");

        var path = context.Request.Path;
        var esApi = path.StartsWithSegments("/api");
        var permitida = RutasPermitidas.Any(r => path.Equals(r, StringComparison.OrdinalIgnoreCase));

        if (debeCambiar && esApi && !permitida)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                mensaje = "Debes cambiar tu contraseña temporal antes de continuar.",
                codigo = Codigo
            });
            return;
        }

        await _next(context);
    }
}
