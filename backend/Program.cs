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

// Middleware de Auditoría HTTP para SLA (Fuente de Datos FD-1)
var logLock = new SemaphoreSlim(1, 1);
var logsDir = Path.Combine(app.Environment.ContentRootPath, "Logs");
Directory.CreateDirectory(logsDir);

app.Use(async (context, next) =>
{
    // Solo audita endpoints de la API (ignora Swagger y archivos estáticos)
    if (!context.Request.Path.StartsWithSegments("/api"))
    {
        await next();
        return;
    }

    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    var requestId = Guid.NewGuid().ToString("N")[..8];

    await next(); // Ejecuta el endpoint real

    stopwatch.Stop();
    var ahoraLocal = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-6));
    var fechaLocal = ahoraLocal.ToString("yyyy-MM-ddTHH:mm:ss.fff-06:00");
    var cliente = context.Request.Headers["X-Client-Type"].FirstOrDefault() ?? "PWA";

    var lineaLog = $"[{fechaLocal}] [ReqID={requestId}] [{cliente}] " +
                   $"{context.Request.Method} {context.Request.Path} | " +
                   $"HTTP={context.Response.StatusCode} | duration_ms={stopwatch.ElapsedMilliseconds}\n";

    var logFile = Path.Combine(logsDir, $"api-access-{ahoraLocal:yyyy-MM-dd}.log");
    await logLock.WaitAsync();
    try
    {
        await File.AppendAllTextAsync(logFile, lineaLog);
    }
    finally
    {
        logLock.Release();
    }
});

app.MapControllers();

app.Run();