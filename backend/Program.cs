using System.Security.Claims;
using System.Text;
using Adopta.Api.Data;
using Adopta.Api.DTOs;
using Adopta.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// Base de datos (MySQL, proveedor de Oracle)
var cs = cfg.GetConnectionString("Default")!;
builder.Services.AddDbContext<AdoptaDbContext>(o => o.UseMySQL(cs));

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(o =>
    {
        // Errores de validación con el formato { mensaje } que lee el front
        o.InvalidModelStateResponseFactory = ctx =>
        {
            var mensaje = ctx.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m)) ?? "Datos inválidos.";

            return new BadRequestObjectResult(new ErrorResponseDto { Mensaje = mensaje });
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IContactoService, ContactoService>();
builder.Services.AddScoped<IPublicoService, PublicoService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IOrganizacionService, OrganizacionService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IOrganizacionAdminService, OrganizacionAdminService>();
builder.Services.AddScoped<IMascotaService, MascotaService>();
builder.Services.AddScoped<ISeguimientoService, SeguimientoService>();
builder.Services.AddScoped<ISolicitudAdopcionService, SolicitudAdopcionService>();
builder.Services.AddScoped<IReportesService, ReportesService>();
builder.Services.AddScoped<INotificacionesService, NotificacionesService>();
builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddSingleton<IReporteExportService, ReporteExportService>();

// CORS
builder.Services.AddCors(o => o.AddPolicy("front", p => p
    .WithOrigins(cfg.GetSection("Cors:Origins").Get<string[]>()!)
    .AllowAnyHeader()
    .AllowAnyMethod()));

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = cfg["Jwt:Issuer"],
        ValidAudience = cfg["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!))
    });
builder.Services.AddAuthorization();

// Carpeta pública de imágenes (fotos de perfil, y después las de mascotas)
var uploadsRoot = Path.Combine(builder.Environment.ContentRootPath, cfg["Storage:PublicRoot"] ?? "Uploads");
Directory.CreateDirectory(uploadsRoot);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads"
});

// Fotos de seguimiento que sube el adoptante desde la app móvil: si la app las guarda en otra carpeta
// (Storage:MobileRoot, con subcarpeta "seguimiento"), se publican también bajo /uploads/seguimiento.
var mobileRoot = cfg["Storage:MobileRoot"];
if (!string.IsNullOrWhiteSpace(mobileRoot))
{
    var seguimientoDir = Path.Combine(Path.GetFullPath(mobileRoot, builder.Environment.ContentRootPath), "seguimiento");
    if (Directory.Exists(seguimientoDir))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(seguimientoDir),
            RequestPath = "/uploads/seguimiento"
        });
    }
    else
    {
        app.Logger.LogWarning("Storage:MobileRoot no tiene la carpeta 'seguimiento': {Dir}", seguimientoDir);
    }
}

var mobileBaseUrl = cfg["Mobile:BaseUrl"]?.TrimEnd('/');
if (!string.IsNullOrWhiteSpace(mobileBaseUrl))
{
    app.Use(async (ctx, next) =>
    {
        if (HttpMethods.IsGet(ctx.Request.Method) && ctx.Request.Path.StartsWithSegments("/uploads"))
        {
            ctx.Response.Redirect($"{mobileBaseUrl}{ctx.Request.Path}{ctx.Request.QueryString}", permanent: false);
            return;
        }
        await next();
    });
}

app.UseCors("front");
app.UseAuthentication();
app.UseAuthorization();

// =========================================================================
// Middleware de Auditoría de Transacciones (.txt) - FD-1
// Registra: Timestamp | Quien | Modulo | Que | Resultado
// =========================================================================
var logLock = new SemaphoreSlim(1, 1);
var logsDir = Path.Combine(app.Environment.ContentRootPath, "Logs");
Directory.CreateDirectory(logsDir);

app.Use(async (context, next) =>
{
    // Solo audita rutas de negocio /api (ignora Swagger, archivos estáticos y consulta de auditoría)
    if (!context.Request.Path.StartsWithSegments("/api")
        || context.Request.Path.StartsWithSegments("/api/auditoria"))
    {
        await next();
        return;
    }

    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    // Captura el correo si viene en el body de login antes de ejecutar
    string? correoLogin = null;
    if (context.Request.Path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase)
        && HttpMethods.IsPost(context.Request.Method))
    {
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
        var match = System.Text.RegularExpressions.Regex.Match(
            body, "\"email\"\\s*:\\s*\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success) correoLogin = match.Groups[1].Value;
    }

    await next(); // Ejecuta el controlador y sus transacciones en BD

    stopwatch.Stop();

    var ahoraLocal = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-6));
    var timestamp = ahoraLocal.ToString("yyyy-MM-ddTHH:mm:ss.fff-06:00");

    var emailClaim = context.User.FindFirst(ClaimTypes.Email)?.Value;
    var idUsuario = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst("sub")?.Value;
    var rol = context.User.FindFirst(ClaimTypes.Role)?.Value;

    var quien = emailClaim
                ?? correoLogin
                ?? (idUsuario is not null ? $"UsuarioID:{idUsuario}({rol ?? "Auth"})" : "Publico/NoAutenticado");

    var segmentos = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
    var modulo = segmentos.Length > 1 ? $"MODULO-{segmentos[1].ToUpperInvariant()}" : "MODULO-API";

    var ruta = context.Request.Path.Value ?? "";
    var que = ruta switch
    {
        var r when r.EndsWith("/auth/login", StringComparison.OrdinalIgnoreCase) => "Log in (Inicio de sesión)",
        var r when r.EndsWith("/auth/logout", StringComparison.OrdinalIgnoreCase) => "Log out (Cierre de sesión)",
        _ => $"Operativa: {context.Request.Method} {ruta}"
    };

    var estado = context.Response.StatusCode < 400 ? "EXITO" : "FALLO";
    var resultado = $"{estado} | HTTP={context.Response.StatusCode} | Tiempo={stopwatch.ElapsedMilliseconds}ms";

    var lineaTxt = $"Timestamp={timestamp} | Quien={quien} | Modulo={modulo} | Que={que} | Resultado={resultado}\n";

    var logFile = Path.Combine(logsDir, $"transacciones-{ahoraLocal:yyyy-MM-dd}.txt");
    await logLock.WaitAsync();
    try
    {
        await File.AppendAllTextAsync(logFile, lineaTxt);
    }
    finally
    {
        logLock.Release();
    }
});

// Endpoint para auditar el Log Out desde el frontend
app.MapPost("/api/auth/logout", () => Results.Ok(new { mensaje = "Sesión cerrada correctamente" }))
   .RequireAuthorization();

// 1. Lista todos los archivos .txt generados en la carpeta Logs con sus rutas
app.MapGet("/api/auditoria", () =>
{
    if (!Directory.Exists(logsDir))
        return Results.Text("La carpeta Logs aún no existe.", "text/plain; charset=utf-8");

    var archivos = Directory.GetFiles(logsDir, "*.txt")
        .Select(Path.GetFileName)
        .OrderByDescending(nombre => nombre)
        .ToList();

    if (archivos.Count == 0)
        return Results.Text("Aún no hay archivos .txt registrados.", "text/plain; charset=utf-8");

    var listado = "ARCHIVOS DE AUDITORÍA DISPONIBLES:\n" +
                  "----------------------------------\n" +
                  string.Join("\n", archivos.Select(a => $"- {a}  -->  /api/auditoria/{a}"));

    return Results.Text(listado, "text/plain; charset=utf-8");
});

// 2. Abre cualquier archivo .txt específico por su nombre (ej. /api/auditoria/transacciones-2026-11-28.txt)
app.MapGet("/api/auditoria/{nombreArchivo}", async (string nombreArchivo) =>
{
    var nombreSeguro = Path.GetFileName(nombreArchivo);
    var ruta = Path.Combine(logsDir, nombreSeguro);

    if (!File.Exists(ruta))
        return Results.NotFound($"No se encontró el archivo: {nombreSeguro}");

    var contenido = await File.ReadAllTextAsync(ruta);
    return Results.Text(contenido, "text/plain; charset=utf-8");
});

app.MapControllers();

app.Run();