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
    // Paleta de marca
    private const string Teal = "#6AA3A3";
    private const string TealOscuro = "#3F8796";
    private const string TealClaro = "#E3F2F3";
    private const string Zebra = "#F4FAFA";
    private const string Navy = "#1D3F5C";
    private const string Linea = "#D5E7E7";
    private const string GrisTexto = "#6B7C85";
    private const string FormatoFecha = "dd/MM/yyyy";

    static ReporteExportService()
    {
        // QuestPDF: licencia Community (organizaciones con ingresos anuales < 1 M USD). Revísala en questpdf.com/license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static string FechaGeneracion() =>
        DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

    private static bool EsNumero(object? v) => v is int or long or decimal or double or float;

    private static string Texto(object? valor) => valor switch
    {
        DateTime d => d.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        int n => n.ToString(CultureInfo.InvariantCulture),
        _ => valor?.ToString() ?? ""
    };

    // =================== Excel ===================

    public byte[] ToExcel(ReporteDocumento doc)
    {
        using var wb = new XLWorkbook();
        var generado = FechaGeneracion();

        foreach (var seccion in doc.Secciones)
        {
            var ws = wb.Worksheets.Add(NombreHoja(seccion.Titulo));
            ws.ShowGridLines = false;
            ws.TabColor = XLColor.FromHtml(Teal);

            var cols = Math.Max(seccion.Columnas.Length, 4);

            // --- Banda de marca ---
            var banda = ws.Range(1, 1, 1, cols);
            banda.Merge();
            banda.Style.Fill.BackgroundColor = XLColor.FromHtml(Teal);
            ws.Cell(1, 1).Value = "Adopta+  ·  Reporte";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            ws.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell(1, 1).Style.Alignment.Indent = 1;
            ws.Row(1).Height = 34;

            // --- Título, periodo y fecha ---
            ws.Cell(2, 1).Value = doc.Titulo;
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Font.FontSize = 15;
            ws.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml(Navy);
            ws.Row(2).Height = 26;

            ws.Cell(3, 1).Value = doc.Periodo;
            ws.Cell(3, 1).Style.Font.FontColor = XLColor.FromHtml(GrisTexto);

            ws.Cell(4, 1).Value = $"Generado el {generado}";
            ws.Cell(4, 1).Style.Font.FontSize = 9;
            ws.Cell(4, 1).Style.Font.FontColor = XLColor.FromHtml(GrisTexto);

            // --- Título de la sección ---
            const int filaSeccion = 6;
            ws.Cell(filaSeccion, 1).Value = seccion.Titulo;
            ws.Cell(filaSeccion, 1).Style.Font.Bold = true;
            ws.Cell(filaSeccion, 1).Style.Font.FontSize = 12;
            ws.Cell(filaSeccion, 1).Style.Font.FontColor = XLColor.FromHtml(TealOscuro);

            // --- Encabezados de la tabla ---
            const int filaEncabezado = 7;
            for (var c = 0; c < seccion.Columnas.Length; c++)
            {
                var celda = ws.Cell(filaEncabezado, c + 1);
                celda.Value = seccion.Columnas[c];
                celda.Style.Font.Bold = true;
                celda.Style.Font.FontColor = XLColor.White;
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml(TealOscuro);
                celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                celda.Style.Alignment.Indent = 1;
            }
            ws.Row(filaEncabezado).Height = 24;

            // --- Datos ---
            var fila = filaEncabezado;
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
                            celda.Style.NumberFormat.Format = "#,##0";
                            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            break;
                        case DateTime d:
                            celda.Value = d;
                            celda.Style.DateFormat.Format = "dd/mm/yyyy";
                            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            break;
                        default:
                            celda.Value = datos[c]?.ToString() ?? "";
                            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            celda.Style.Alignment.Indent = 1;
                            break;
                    }
                    celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                var rango = ws.Range(fila, 1, fila, seccion.Columnas.Length);
                if ((fila - filaEncabezado) % 2 == 0)
                    rango.Style.Fill.BackgroundColor = XLColor.FromHtml(Zebra);
                rango.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rango.Style.Border.BottomBorderColor = XLColor.FromHtml(Linea);
                ws.Row(fila).Height = 20;
            }

            if (seccion.Filas.Count == 0)
            {
                ws.Cell(filaEncabezado + 1, 1).Value = "Sin registros en el periodo.";
                ws.Cell(filaEncabezado + 1, 1).Style.Font.Italic = true;
                ws.Cell(filaEncabezado + 1, 1).Style.Font.FontColor = XLColor.FromHtml(GrisTexto);
            }
            else
            {
                // Total de registros al pie
                var pie = fila + 2;
                ws.Cell(pie, 1).Value = $"Total de registros: {seccion.Filas.Count}";
                ws.Cell(pie, 1).Style.Font.Bold = true;
                ws.Cell(pie, 1).Style.Font.FontColor = XLColor.FromHtml(Navy);

                // Filtros y encabezado fijo
                ws.Range(filaEncabezado, 1, fila, seccion.Columnas.Length).SetAutoFilter();
            }

            ws.SheetView.FreezeRows(filaEncabezado);

            // Ancho de columnas según el contenido de la tabla (sin contar los títulos de arriba)
            for (var c = 1; c <= seccion.Columnas.Length; c++)
            {
                var col = ws.Column(c);
                col.AdjustToContents(filaEncabezado);
                col.Width = Math.Clamp(col.Width + 3, 14, 55);
            }
            // Si la hoja tiene menos de 4 columnas, las de la banda quedan con ancho estándar
            for (var c = seccion.Columnas.Length + 1; c <= cols; c++) ws.Column(c).Width = 14;

            ws.PageSetup.FitToPages(1, 0);
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

    public byte[] ToPdf(ReporteDocumento doc)
    {
        var totalRegistros = doc.Secciones.Sum(s => s.Filas.Count);
        var generado = FechaGeneracion();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(t => t.FontSize(10).FontColor(Navy));

                // ---------- Encabezado: banda de marca + título ----------
                page.Header().Column(col =>
                {
                    col.Item().Background(Teal).PaddingVertical(14).PaddingHorizontal(36).Row(row =>
                    {
                        row.RelativeItem().Text(t =>
                        {
                            t.Span("Adopta").FontSize(20).Bold().FontColor(Colors.White);
                            t.Span("+").FontSize(20).Bold().FontColor(TealClaro);
                        });
                        row.AutoItem().AlignMiddle().Text("REPORTE")
                            .FontSize(9).Bold().FontColor(TealClaro).LetterSpacing(0.15f);
                    });

                    col.Item().PaddingHorizontal(36).PaddingTop(22).PaddingBottom(14).Column(c =>
                    {
                        c.Spacing(4);
                        c.Item().Text(doc.Titulo).FontSize(22).Bold().FontColor(Navy);
                        c.Item().Text(doc.Periodo).FontSize(11).FontColor(TealOscuro);
                        c.Item().PaddingTop(8).LineHorizontal(1).LineColor(Linea);
                    });
                });

                // ---------- Contenido ----------
                page.Content().PaddingHorizontal(36).PaddingTop(4).PaddingBottom(10).Column(col =>
                {
                    col.Spacing(18);

                    // Resumen
                    col.Item().Row(row =>
                    {
                        row.Spacing(12);
                        row.RelativeItem().Element(c => Tarjeta(c, doc.Secciones.Count.ToString(CultureInfo.InvariantCulture), "Secciones"));
                        row.RelativeItem().Element(c => Tarjeta(c, totalRegistros.ToString("N0", CultureInfo.InvariantCulture), "Registros"));
                        row.RelativeItem().Element(c => Tarjeta(c, generado, "Generado"));
                    });

                    foreach (var seccion in doc.Secciones)
                    {
                        col.Item().Column(sec =>
                        {
                            sec.Spacing(8);
                            sec.Item().BorderLeft(4).BorderColor(Teal).PaddingLeft(8)
                                .Text(seccion.Titulo).FontSize(13).Bold().FontColor(Navy);
                            sec.Item().Element(c => Tabla(c, seccion));
                        });
                    }
                });

                // ---------- Pie ----------
                page.Footer().PaddingHorizontal(36).PaddingBottom(18).Column(col =>
                {
                    col.Item().LineHorizontal(0.75f).LineColor(Linea);
                    col.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text("Adopta+ · Documento generado automáticamente")
                            .FontSize(8).FontColor(GrisTexto);
                        row.AutoItem().Text(t =>
                        {
                            t.DefaultTextStyle(x => x.FontSize(8).FontColor(GrisTexto));
                            t.Span($"{generado} · Página ");
                            t.CurrentPageNumber();
                            t.Span(" de ");
                            t.TotalPages();
                        });
                    });
                });
            });
        }).GeneratePdf();
    }

    private static void Tarjeta(IContainer container, string valor, string etiqueta) =>
        container.Background(TealClaro).Border(0.75f).BorderColor(Linea).Padding(10).Column(c =>
        {
            c.Spacing(2);
            c.Item().Text(valor).FontSize(15).Bold().FontColor(TealOscuro);
            c.Item().Text(etiqueta.ToUpperInvariant()).FontSize(7.5f).Bold().FontColor(GrisTexto).LetterSpacing(0.08f);
        });

    private static void Tabla(IContainer container, ReporteSeccion seccion)
    {
        if (seccion.Filas.Count == 0)
        {
            container.Background(Zebra).Padding(12)
                .Text("Sin registros en el periodo.").Italic().FontColor(GrisTexto);
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
                    h.Cell().Background(TealOscuro).PaddingVertical(7).PaddingHorizontal(8)
                        .Text(nombre).Bold().FontSize(9.5f).FontColor(Colors.White);
            });

            var i = 0;
            foreach (var fila in seccion.Filas)
            {
                var fondo = i++ % 2 == 0 ? "#FFFFFF" : Zebra;
                foreach (var valor in fila)
                {
                    var celda = tabla.Cell().Background(fondo)
                        .BorderBottom(0.5f).BorderColor(Linea)
                        .PaddingVertical(6).PaddingHorizontal(8);

                    if (EsNumero(valor)) celda.AlignRight().Text(Texto(valor));
                    else celda.Text(Texto(valor));
                }
            }
        });
    }
}
