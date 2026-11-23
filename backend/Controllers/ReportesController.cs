using Adopta.Api.DTOs;
using Adopta.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Adopta.Api.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize(Roles = "Administrador")]
public class ReportesController : ControllerBase
{
    private const string MimeExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string MimePdf = "application/pdf";

    private readonly IReportesService _reportes;
    private readonly IReporteExportService _export;

    public ReportesController(IReportesService reportes, IReporteExportService export)
    {
        _reportes = reportes;
        _export = export;
    }

    private async Task<IActionResult> Ejecutar(Func<Task<IActionResult>> accion)
    {
        try { return await accion(); }
        catch (NegocioException ex) { return StatusCode(ex.Status, new ErrorResponseDto { Mensaje = ex.Message }); }
    }

    [HttpGet("panel")]
    public Task<IActionResult> Panel() =>
        Ejecutar(async () => Ok(await _reportes.PanelAsync()));

    [HttpGet("solicitudes")]
    public Task<IActionResult> Solicitudes([FromQuery] ReportesQueryDto q) =>
        Ejecutar(async () => Ok(await _reportes.SolicitudesAsync(q.Desde, q.Hasta)));

    [HttpGet("mascotas")]
    public Task<IActionResult> Mascotas([FromQuery] ReportesQueryDto q) =>
        Ejecutar(async () => Ok(await _reportes.MascotasAsync(q.Desde, q.Hasta)));

    [HttpGet("adopciones")]
    public Task<IActionResult> Adopciones([FromQuery] ReportesQueryDto q) =>
        Ejecutar(async () => Ok(await _reportes.AdopcionesAsync(q.Desde, q.Hasta)));

    /// <summary>GET api/reportes/exportar?tipo=solicitudes&amp;formato=pdf&amp;desde=2026-10-01&amp;hasta=2026-10-20&amp;nombre=mi_reporte</summary>
    [HttpGet("exportar")]
    public Task<IActionResult> Exportar([FromQuery] ExportarReporteQueryDto q) =>
        Ejecutar(async () =>
        {
            var tipo = (q.Tipo ?? "").Trim().ToLowerInvariant();
            if (!ReportesService.TiposValidos.Contains(tipo))
                throw new NegocioException("Selecciona el tipo de reporte.");

            var formato = (q.Formato ?? "").Trim().ToLowerInvariant();
            if (formato is not ("pdf" or "excel"))
                throw new NegocioException("Selecciona el formato del archivo (PDF o Excel).");

            var documento = await _reportes.DocumentoAsync(tipo, q.Desde, q.Hasta);
            var esPdf = formato == "pdf";
            var bytes = esPdf ? _export.ToPdf(documento) : _export.ToExcel(documento);

            var nombre = NombreArchivo(q.Nombre, tipo) + (esPdf ? ".pdf" : ".xlsx");
            return File(bytes, esPdf ? MimePdf : MimeExcel, nombre);
        });

    /// <summary>Solo letras, números, guion y guion bajo; evita rutas y caracteres raros en la cabecera.</summary>
    private static string NombreArchivo(string? solicitado, string tipo)
    {
        var limpio = Regex.Replace((solicitado ?? "").Trim(), @"\.(pdf|xlsx?)$", "", RegexOptions.IgnoreCase);
        limpio = Regex.Replace(limpio, @"[^A-Za-z0-9_\-]+", "_").Trim('_');
        if (limpio.Length > 80) limpio = limpio[..80];
        return limpio.Length > 0 ? limpio : $"reporte_{tipo}_{DateTime.Today:yyyyMMdd}";
    }
}
