using System.Globalization;
using System.Text;
using Adopta.Api.DTOs;

namespace Adopta.Api.Services;

/// <summary>
/// Utilidades compartidas por los servicios de listados/detalles (paginación, nombres, textos).
/// SeguimientoService tiene copias privadas de algunas; pueden migrarse a esta clase cuando convenga.
/// </summary>
internal static class ServiceHelpers
{
    public static string NombreCompleto(string nombre, string paterno, string? materno) =>
        string.Join(' ', new[] { nombre, paterno, materno }.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!.Trim()));

    /// <summary>Palabras de una búsqueda (máx. 5): cada una debe coincidir con algún campo.</summary>
    public static IEnumerable<string> Palabras(string? texto) =>
        (texto ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(5);

    /// <summary>Texto recortado; null si queda vacío; error si excede el máximo.</summary>
    public static string? Opcional(string? valor, string campo, int max)
    {
        var v = valor?.Trim();
        if (string.IsNullOrEmpty(v)) return null;
        if (v.Length > max) throw new NegocioException($"Máximo {max} caracteres en {campo}.");
        return v;
    }

    /// <summary>Minúsculas y sin acentos: compara nombres de catálogo sin depender de cómo se escribieron en la BD.</summary>
    public static string Normalizar(string? texto)
    {
        var descompuesto = (texto ?? "").Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        return new string(descompuesto.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
    }

    public static (int Pagina, int Tamano) Paginar(int pagina, int tamano, int max) =>
        (Math.Max(pagina, 1), Math.Clamp(tamano, 1, max));

    public static PagedResultDto<T> Resultado<T>(List<T> items, int total, int pagina, int tamano) => new()
    {
        Items = items,
        Total = total,
        Pagina = pagina,
        Tamano = tamano,
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamano))
    };
}
