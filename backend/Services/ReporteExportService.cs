using System.Globalization;
using Adopta.Api.DTOs;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Adopta.Api.Services;

public interface IReporteExportService
{
    byte[] ToExcel(ReporteDocumento doc);
    byte[] ToPdf(ReporteDocumento doc);
}

public class ReporteExportService : IReporteExportService
{
    private const string Teal = "#6AA3A3";
    private const string Navy = "#1D3F5C";
    private const string FormatoFecha = "dd/MM/yyyy";

    static ReporteExportService()
    {
        // QuestPDF: licencia Community (organizaciones con ingresos anuales < 1 M USD). Revísala en questpdf.com/license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // =================== Excel ===================

    public byte[] ToExcel(ReporteDocumento doc)
    {
        using var wb = new XLWorkbook();

        // Una hoja por sección; la primera lleva el título y el periodo
        var primera = true;
        foreach (var seccion in doc.Secciones)
        {
            var ws = wb.Worksheets.Add(NombreHoja(seccion.Titulo));
            var fila = 1;

            if (primera)
            {
                ws.Cell(fila, 1).Value = doc.Titulo;
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 1).Style.Font.FontSize = 14;
                ws.Cell(fila + 1, 1).Value = doc.Periodo;
                fila += 3;
                primera = false;
            }

            for (var c = 0; c < seccion.Columnas.Length; c++)
            {
                var celda = ws.Cell(fila, c + 1);
                celda.Value = seccion.Columnas[c];
                celda.Style.Font.Bold = true;
                celda.Style.Font.FontColor = XLColor.White;
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml(Teal);
            }

            foreach (var datos in seccion.Filas)
            {
                fila++;
                for (var c = 0; c < datos.Length; c++)
                {
                    var celda = ws.Cell(fila, c + 1);
                    switch (datos[c])
                    {
                        case int n:
                            celda.Value = n;
                            break;
                        case DateTime d:
                            celda.Value = d;
                            celda.Style.DateFormat.Format = "dd/mm/yyyy";
                            break;
                        default:
                            celda.Value = datos[c]?.ToString() ?? "";
                            break;
                    }
                }
            }

            ws.Columns().AdjustToContents();
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static string NombreHoja(string titulo)
    {
        var limpio = new string(titulo.Where(ch => !"\\/?*[]:".Contains(ch)).ToArray()).Trim();
        if (limpio.Length == 0) limpio = "Hoja";
        return limpio.Length > 31 ? limpio[..31] : limpio;
    }

    // =================== PDF ===================

    public byte[] ToPdf(ReporteDocumento doc) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontSize(10));

                page.Header().PaddingBottom(10).Column(col =>
                {
                    col.Spacing(2);
                    col.Item().Text("Adopta+").FontSize(11).Bold().FontColor(Teal);
                    col.Item().Text(doc.Titulo).FontSize(20).Bold().FontColor(Navy);
                    col.Item().Text(doc.Periodo).FontColor(Colors.Grey.Darken1);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(16);
                    foreach (var seccion in doc.Secciones)
                    {
                        col.Item().Text(seccion.Titulo).FontSize(13).Bold().FontColor(Navy);
                        col.Item().Element(c => Tabla(c, seccion));
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                    t.Span($"Generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)} · Página ");
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf();

    private static void Tabla(IContainer container, ReporteSeccion seccion)
    {
        if (seccion.Filas.Count == 0)
        {
            container.Text("Sin registros en el periodo.").FontColor(Colors.Grey.Darken1);
            return;
        }

        container.Table(tabla =>
        {
            tabla.ColumnsDefinition(cols =>
            {
                foreach (var _ in seccion.Columnas) cols.RelativeColumn();
            });

            tabla.Header(h =>
            {
                foreach (var nombre in seccion.Columnas)
                    h.Cell().Background(Teal).Padding(5).Text(nombre).Bold().FontColor(Colors.White);
            });

            foreach (var fila in seccion.Filas)
            {
                foreach (var valor in fila)
                {
                    var texto = valor switch
                    {
                        DateTime d => d.ToString(FormatoFecha, CultureInfo.InvariantCulture),
                        int n => n.ToString(CultureInfo.InvariantCulture),
                        _ => valor?.ToString() ?? ""
                    };
                    tabla.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(texto);
                }
            }
        });
    }
}
