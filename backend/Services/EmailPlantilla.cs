using System.Net;
using System.Text;

namespace Adopta.Api.Services;

/// <summary>
/// Plantilla HTML compartida por todos los correos de Adopta+.
/// Usa tablas y estilos en línea para verse igual en Gmail, Outlook y apps móviles.
/// </summary>
public static class EmailPlantilla
{
    public const string CorreoSoporte = "adoptasoporte@gmail.com";

    private const string Teal = "#6AA3A3";
    private const string TealOscuro = "#3F8796";
    private const string Navy = "#1D3F5C";
    private const string Texto = "#33474F";
    private const string Gris = "#6B7C85";
    private const string Fondo = "#EEF5F5";
    private const string Fuente = "'Segoe UI',Helvetica,Arial,sans-serif";

    // ---------- Bloques reutilizables ----------

    public static string Html(string? texto) => WebUtility.HtmlEncode(texto ?? string.Empty);

    /// <summary>Texto con saltos de línea conservados (para mensajes escritos por personas).</summary>
    public static string HtmlConSaltos(string? texto) =>
        Html(texto).Replace("\r\n", "\n").Replace("\n", "<br/>");

    public static string Parrafo(string html) =>
        $"""<p style="margin:0 0 16px;font-size:15px;line-height:1.65;color:{Texto};">{html}</p>""";

    public static string Nota(string html) =>
        $"""<p style="margin:20px 0 0;font-size:13px;line-height:1.6;color:{Gris};">{html}</p>""";

    public static string Boton(string texto, string url) => $"""
        <table role="presentation" cellpadding="0" cellspacing="0" border="0" align="center" style="margin:28px auto;">
          <tr>
            <td align="center" bgcolor="{Teal}" style="border-radius:10px;">
              <a href="{Html(url)}" target="_blank"
                 style="display:inline-block;padding:14px 34px;font-family:{Fuente};font-size:15px;font-weight:700;color:#FFFFFF;text-decoration:none;border-radius:10px;">
                {Html(texto)}
              </a>
            </td>
          </tr>
        </table>
        """;

    /// <summary>Caja con pares etiqueta / valor (los valores se codifican aquí).</summary>
    public static string Datos(params (string Etiqueta, string? Valor)[] filas)
    {
        var sb = new StringBuilder();
        sb.Append($"""<table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="margin:8px 0 20px;background:#F4FAFA;border:1px solid #D5E7E7;border-radius:10px;">""");
        foreach (var (etiqueta, valor) in filas)
        {
            sb.Append($"""
                <tr>
                  <td style="padding:11px 16px;border-bottom:1px solid #E1EEEE;font-family:{Fuente};font-size:12px;font-weight:700;letter-spacing:.04em;text-transform:uppercase;color:{TealOscuro};width:34%;vertical-align:top;">{Html(etiqueta)}</td>
                  <td style="padding:11px 16px;border-bottom:1px solid #E1EEEE;font-family:{Fuente};font-size:14px;color:{Texto};vertical-align:top;">{Html(valor)}</td>
                </tr>
                """);
        }
        sb.Append("</table>");
        return sb.ToString();
    }

    /// <summary>Recuadro con acento lateral para citar un mensaje o un motivo.</summary>
    public static string Cita(string html) => $"""
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="margin:8px 0 20px;">
          <tr>
            <td width="4" bgcolor="{Teal}" style="border-radius:4px;font-size:0;line-height:0;">&nbsp;</td>
            <td style="padding:14px 18px;background:#F4FAFA;font-family:{Fuente};font-size:15px;line-height:1.65;color:{Texto};border-radius:0 10px 10px 0;">{html}</td>
          </tr>
        </table>
        """;

    // ---------- Estructura general ----------

    /// <param name="titulo">Encabezado grande dentro de la tarjeta.</param>
    /// <param name="cuerpo">HTML ya armado con los bloques de arriba.</param>
    /// <param name="preheader">Texto que muestran los clientes de correo junto al asunto.</param>
    public static string Envolver(string titulo, string cuerpo, string? preheader = null)
    {
        var pre = string.IsNullOrWhiteSpace(preheader) ? "" :
            $"""<div style="display:none;max-height:0;overflow:hidden;opacity:0;font-size:1px;line-height:1px;color:{Fondo};">{Html(preheader)}</div>""";

        return $"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
              <meta charset="utf-8"/>
              <meta name="viewport" content="width=device-width,initial-scale=1"/>
              <title>{Html(titulo)}</title>
            </head>
            <body style="margin:0;padding:0;background:{Fondo};">
              {pre}
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" bgcolor="{Fondo}" style="background:{Fondo};">
                <tr>
                  <td align="center" style="padding:32px 14px;">
                    <table role="presentation" width="600" cellpadding="0" cellspacing="0" border="0" style="width:100%;max-width:600px;">

                      <!-- Encabezado de marca -->
                      <tr>
                        <td align="center" bgcolor="{Teal}" style="background:{Teal};border-radius:16px 16px 0 0;padding:30px 24px 26px;">
                          <div style="font-family:{Fuente};font-size:30px;font-weight:800;letter-spacing:-.5px;color:#FFFFFF;">
                            Adopta<span style="color:#D8EEEE;">+</span>
                          </div>
                          <div style="font-family:{Fuente};font-size:12px;letter-spacing:.14em;text-transform:uppercase;color:#E3F2F3;margin-top:6px;">
                            Más que una adopción
                          </div>
                        </td>
                      </tr>

                      <!-- Tarjeta de contenido -->
                      <tr>
                        <td bgcolor="#FFFFFF" style="background:#FFFFFF;padding:36px 36px 30px;font-family:{Fuente};border-left:1px solid #DCEAEA;border-right:1px solid #DCEAEA;">
                          <h1 style="margin:0 0 20px;font-family:{Fuente};font-size:24px;line-height:1.3;font-weight:800;color:{Navy};">{Html(titulo)}</h1>
                          {cuerpo}
                        </td>
                      </tr>

                      <!-- Pie -->
                      <tr>
                        <td bgcolor="#F7FBFB" align="center" style="background:#F7FBFB;border:1px solid #DCEAEA;border-top:1px solid #E3EFEF;border-radius:0 0 16px 16px;padding:22px 28px;font-family:{Fuente};">
                          <p style="margin:0 0 6px;font-size:13px;color:{Gris};">
                            ¿Necesitas ayuda? Escríbenos a
                            <a href="mailto:{CorreoSoporte}" style="color:{TealOscuro};font-weight:600;text-decoration:none;">{CorreoSoporte}</a>
                          </p>
                          <p style="margin:0;font-size:11.5px;line-height:1.6;color:#93A3AA;">
                            Este es un mensaje automático de Adopta+, por favor no respondas directamente a este correo.<br/>
                            © {DateTime.Now.Year} Adopta+ · León, Guanajuato, México
                          </p>
                        </td>
                      </tr>

                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }
}
