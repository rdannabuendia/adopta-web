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

// Servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IOrganizacionService, OrganizacionService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IOrganizacionAdminService, OrganizacionAdminService>();
builder.Services.AddScoped<IMascotaService, MascotaService>();

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

app.UseCors("front");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();