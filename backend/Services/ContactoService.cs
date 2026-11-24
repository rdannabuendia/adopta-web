using System.Collections.Concurrent;
using Adopta.Api.DTOs;
using static Adopta.Api.Services.EmailPlantilla;

namespace Adopta.Api.Services;

public interface IContactoService
{
    /// <summary>Envía el mensaje al buzón de soporte. Lanza NegocioException si no se puede.</summary>
    Task EnviarAsync(ContactoRequestDto dto, string? ip);
}

public class ContactoService : IContactoService
{
    private const int MaxMensajes = 5;
    private static readonly TimeSpan Ventana = TimeSpan.FromMinutes(10);

    // Límite sencillo por IP para que nadie use el formulario como spam (en memoria).
    private static readonly ConcurrentDictionary<string, List<DateTime>> Intentos = new();

    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly ILogger<ContactoService> _logger;

    public ContactoService(IEmailService email, IConfiguration config, ILogger<ContactoService> logger)
    {
        _email = email;
        _config = config;
        _logger = logger;
    }

    public async Task EnviarAsync(ContactoRequestDto dto, string? ip)
    {
        // Bot: finge éxito y no envía nada.
        if (!string.IsNullOrWhiteSpace(dto.Web)) return;

        if (SuperaLimite(ip ?? "desconocida"))
            throw new NegocioException(
                "Enviaste varios mensajes seguidos. Espera unos minutos e inténtalo de nuevo.",
                StatusCodes.Status429TooManyRequests);

        var nombre = dto.Nombre.Trim();
        var correo = dto.Correo.Trim();
        var asunto = dto.Asunto.Trim();
        var mensaje = dto.Mensaje.Trim();

        var destino = _config["Contact:To"];
        if (string.IsNullOrWhiteSpace(destino)) destino = CorreoSoporte;

        var cuerpo =
            Parrafo("Recibiste un nuevo mensaje desde el formulario de contacto de la página de Adopta+.") +
            Datos(("Nombre", nombre), ("Correo", correo), ("Asunto", asunto),
                  ("Recibido", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))) +
            Parrafo("<strong>Mensaje</strong>") +
            Cita(HtmlConSaltos(mensaje)) +
            Nota("Puedes responder directamente a este correo: la respuesta llegará a " + Html(correo) + ".");

        try
        {
            await _email.EnviarAsync(destino, $"[Contacto] {asunto} — {nombre}",
                Envolver("Nuevo mensaje de contacto", cuerpo, $"{nombre} te escribió: {asunto}"),
                replyTo: correo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar el mensaje de contacto de {Correo}", correo);
            throw new NegocioException(
                "No pudimos enviar tu mensaje. Inténtalo de nuevo en unos minutos.",
                StatusCodes.Status503ServiceUnavailable);
        }

        // Acuse de recibo para quien escribió (si falla no es grave).
        try
        {
            var acuse =
                Parrafo($"Hola <strong>{Html(nombre)}</strong>,") +
                Parrafo("¡Gracias por escribirnos! Recibimos tu mensaje y nuestro equipo te responderá lo antes posible.") +
                Parrafo("<strong>Esto fue lo que nos enviaste</strong>") +
                Datos(("Asunto", asunto)) +
                Cita(HtmlConSaltos(mensaje)) +
                Nota("Si no fuiste tú quien envió este mensaje, puedes ignorar este correo.");

            await _email.EnviarAsync(correo, "Recibimos tu mensaje - Adopta+",
                Envolver("Recibimos tu mensaje", acuse, "Gracias por contactar a Adopta+"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo enviar el acuse de recibo a {Correo}", correo);
        }
    }

    private static bool SuperaLimite(string ip)
    {
        var ahora = DateTime.UtcNow;
        var lista = Intentos.GetOrAdd(ip, _ => new List<DateTime>());
        lock (lista)
        {
            lista.RemoveAll(t => ahora - t > Ventana);
            if (lista.Count >= MaxMensajes) return true;
            lista.Add(ahora);
            return false;
        }
    }
}
