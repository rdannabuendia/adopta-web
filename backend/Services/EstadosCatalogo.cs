using static Adopta.Api.Services.ServiceHelpers;

namespace Adopta.Api.Services;

/// <summary>
/// Clasifica los nombres de los catálogos (estados_solicitud, estados_mascota) sin depender
/// de acentos, mayúsculas ni de si la BD dice "Aprobada" o "Aceptada".
/// </summary>
internal static class EstadosCatalogo
{
    private static readonly HashSet<string> Aprobada = new() { "aprobada", "aprobado", "aceptada", "aceptado" };
    private static readonly HashSet<string> Rechazada = new() { "rechazada", "rechazado" };
    private static readonly HashSet<string> Cancelada = new() { "cancelada", "cancelado" };

    public static bool EsAprobada(string nombre) => Aprobada.Contains(Normalizar(nombre));
    public static bool EsRechazada(string nombre) => Rechazada.Contains(Normalizar(nombre));
    public static bool EsCancelada(string nombre) => Cancelada.Contains(Normalizar(nombre));
    /// <summary>Todavía no se resuelve: pendiente, en revisión, documentación pendiente…</summary>
    public static bool EsAbierta(string nombre) => !EsAprobada(nombre) && !EsRechazada(nombre) && !EsCancelada(nombre);
    public static bool EsSinRevisar(string nombre) => Normalizar(nombre) == "pendiente";

    public static bool MascotaDisponible(string nombre) => Normalizar(nombre) == "disponible";
    public static bool MascotaEnProceso(string nombre) => Normalizar(nombre) == "en proceso de adopcion";
    public static bool MascotaAdoptada(string nombre) => Normalizar(nombre) == "adoptada";
}
