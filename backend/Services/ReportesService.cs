using System.Globalization;
using Adopta.Api.Data;
using Adopta.Api.DTOs;
using Microsoft.EntityFrameworkCore;
using static Adopta.Api.Services.EstadosCatalogo;
using static Adopta.Api.Services.ServiceHelpers;

namespace Adopta.Api.Services;

public interface IReportesService
{
    Task<PanelGeneralDto> PanelAsync();
    Task<ReporteSolicitudesDto> SolicitudesAsync(DateTime? desde, DateTime? hasta);
    Task<ReporteMascotasDto> MascotasAsync(DateTime? desde, DateTime? hasta);
    Task<ReporteAdopcionesDto> AdopcionesAsync(DateTime? desde, DateTime? hasta);
    /// <summary>Modelo neutro del reporte (se convierte a PDF / Excel en IReporteExportService).</summary>
    Task<ReporteDocumento> DocumentoAsync(string tipo, DateTime? desde, DateTime? hasta);
}

/// <summary>Estadísticas globales de la plataforma. Solo para administradores.</summary>
public class ReportesService : IReportesService
{
    public static readonly string[] TiposValidos = { "general", "adopciones", "solicitudes", "mascotas" };

    private const int MesesPorDefecto = 8;   // la gráfica muestra los últimos 8 meses
    private const int MaxMeses = 36;
    private const int MaxFilasDetalle = 5000;

    private static readonly string[] Meses = { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
    private const string FechaCorta = "dd/MM/yyyy";

    private readonly AdoptaDbContext _db;

    public ReportesService(AdoptaDbContext db) => _db = db;

    // =================== Panel general ===================

    public async Task<PanelGeneralDto> PanelAsync()
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        var organizaciones = await _db.Organizaciones.AsNoTracking().CountAsync(o => o.Estado == "Activa");
        var orgNuevas = await _db.Organizaciones.AsNoTracking()
            .CountAsync(o => o.Estado == "Activa" && o.FechaRegistro >= inicioMes);

        var estadosMascota = await _db.EstadosMascota.AsNoTracking().ToListAsync();
        var porEstadoMascota = await _db.Mascotas.AsNoTracking()
            .GroupBy(m => m.IdEstadoMascota)
            .Select(g => new { Id = g.Key, N = g.Count() })
            .ToListAsync();
        var disponibles = porEstadoMascota
            .Where(c => MascotaDisponible(NombreDe(estadosMascota.Select(e => (e.IdEstadoMascota, e.Nombre)), c.Id)))
            .Sum(c => c.N);

        var adopciones = await _db.Adopciones.AsNoTracking().CountAsync();
        var adopcionesMes = await _db.Adopciones.AsNoTracking().CountAsync(a => a.FechaAdopcion >= inicioMes);

        var estadosSol = await _db.EstadosSolicituds.AsNoTracking().ToListAsync();
        var porEstadoSol = await _db.SolicitudesAdopcions.AsNoTracking()
            .GroupBy(s => s.IdEstadoSolicitud)
            .Select(g => new { Id = g.Key, N = g.Count() })
            .ToListAsync();
        var pares = estadosSol.Select(e => (e.IdEstadoSolicitud, e.Nombre)).ToList();
        var pendientes = porEstadoSol.Where(c => EsAbierta(NombreDe(pares, c.Id))).Sum(c => c.N);
        var porRevisar = porEstadoSol.Where(c => EsSinRevisar(NombreDe(pares, c.Id))).Sum(c => c.N);

        var (inicio, meses) = Ventana(null, null);
        var fechas = await _db.SolicitudesAdopcions.AsNoTracking()
            .Where(s => s.FechaSolicitud >= inicio)
            .Select(s => s.FechaSolicitud)
            .ToListAsync();

        return new PanelGeneralDto
        {
            Organizaciones = organizaciones,
            OrganizacionesNuevasMes = orgNuevas,
            MascotasRegistradas = porEstadoMascota.Sum(c => c.N),
            MascotasDisponibles = disponibles,
            AdopcionesRealizadas = adopciones,
            AdopcionesMes = adopcionesMes,
            SolicitudesPendientes = pendientes,
            SolicitudesPorRevisar = porRevisar,
            ActividadMensual = Serie(fechas, inicio, meses)
        };
    }

    // =================== Solicitudes ===================

    public async Task<ReporteSolicitudesDto> SolicitudesAsync(DateTime? desde, DateTime? hasta)
    {
        ValidarRango(desde, hasta);
        var q = _db.SolicitudesAdopcions.AsNoTracking().AsQueryable();
        if (desde is DateTime d) { var ini = d.Date; q = q.Where(s => s.FechaSolicitud >= ini); }
        if (hasta is DateTime h) { var fin = h.Date.AddDays(1); q = q.Where(s => s.FechaSolicitud < fin); }

        var estados = (await _db.EstadosSolicituds.AsNoTracking().ToListAsync())
            .Select(e => (e.IdEstadoSolicitud, e.Nombre)).ToList();
        var conteo = await q.GroupBy(s => s.IdEstadoSolicitud)
            .Select(g => new { Id = g.Key, N = g.Count() }).ToListAsync();

        int Suma(Func<string, bool> filtro) => conteo.Where(c => filtro(NombreDe(estados, c.Id))).Sum(c => c.N);

        var total = conteo.Sum(c => c.N);
        var aprobadas = Suma(EsAprobada);
        var rechazadas = Suma(EsRechazada);

        var (inicio, meses) = Ventana(desde, hasta);
        var fechas = await FechasAsync(q.Select(s => s.FechaSolicitud), desde, hasta, inicio);

        var abiertas = Suma(EsAbierta);
        return new ReporteSolicitudesDto
        {
            Total = total,
            Aprobadas = aprobadas,
            Pendientes = abiertas,
            Rechazadas = rechazadas,
            Canceladas = Suma(EsCancelada),
            PorcentajeAprobadas = Porcentaje(aprobadas, total),
            PorcentajePendientes = Porcentaje(abiertas, total),
            PorcentajeRechazadas = Porcentaje(rechazadas, total),
            PorMes = Serie(fechas, inicio, meses)
        };
    }

    // =================== Mascotas ===================

    public async Task<ReporteMascotasDto> MascotasAsync(DateTime? desde, DateTime? hasta)
    {
        ValidarRango(desde, hasta);
        // Los totales son la situación actual del catálogo; el periodo solo afecta la gráfica (altas por mes)
        var estados = (await _db.EstadosMascota.AsNoTracking().ToListAsync())
            .Select(e => (e.IdEstadoMascota, e.Nombre)).ToList();
        var conteo = await _db.Mascotas.AsNoTracking()
            .GroupBy(m => m.IdEstadoMascota)
            .Select(g => new { Id = g.Key, N = g.Count() }).ToListAsync();

        int Suma(Func<string, bool> filtro) => conteo.Where(c => filtro(NombreDe(estados, c.Id))).Sum(c => c.N);

        var total = conteo.Sum(c => c.N);
        var disponibles = Suma(MascotaDisponible);
        var enProceso = Suma(MascotaEnProceso);
        var adoptadas = Suma(MascotaAdoptada);

        var (inicio, meses) = Ventana(desde, hasta);
        var fechas = await FechasAsync(_db.Mascotas.AsNoTracking().Select(m => m.FechaRegistro), desde, hasta, inicio);

        return new ReporteMascotasDto
        {
            Total = total,
            Disponibles = disponibles,
            EnProceso = enProceso,
            Adoptadas = adoptadas,
            Otras = total - disponibles - enProceso - adoptadas,
            PorcentajeDisponibles = Porcentaje(disponibles, total),
            PorcentajeEnProceso = Porcentaje(enProceso, total),
            PorcentajeAdoptadas = Porcentaje(adoptadas, total),
            PorMes = Serie(fechas, inicio, meses)
        };
    }

    // =================== Adopciones ===================

    public async Task<ReporteAdopcionesDto> AdopcionesAsync(DateTime? desde, DateTime? hasta)
    {
        ValidarRango(desde, hasta);
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var hace12 = inicioMes.AddMonths(-11);   // 12 meses contando el actual

        var q = _db.Adopciones.AsNoTracking().AsQueryable();
        if (desde is DateTime d) { var ini = d.Date; q = q.Where(a => a.FechaAdopcion >= ini); }
        if (hasta is DateTime h) { var fin = h.Date.AddDays(1); q = q.Where(a => a.FechaAdopcion < fin); }

        var total = await q.CountAsync();
        var esteMes = await _db.Adopciones.AsNoTracking().CountAsync(a => a.FechaAdopcion >= inicioMes);
        var ultimos12 = await _db.Adopciones.AsNoTracking().CountAsync(a => a.FechaAdopcion >= hace12);

        var (inicio, meses) = Ventana(desde, hasta);
        var fechas = await FechasAsync(q.Select(a => a.FechaAdopcion), desde, hasta, inicio);

        return new ReporteAdopcionesDto
        {
            Total = total,
            EsteMes = esteMes,
            PromedioMensual = (int)Math.Round(ultimos12 / 12.0),
            PorMes = Serie(fechas, inicio, meses)
        };
    }

    // =================== Documento para exportar ===================

    public async Task<ReporteDocumento> DocumentoAsync(string tipo, DateTime? desde, DateTime? hasta)
    {
        ValidarRango(desde, hasta);
        var periodo = Periodo(desde, hasta);

        return tipo switch
        {
            "general" => await DocumentoGeneralAsync(periodo),
            "adopciones" => await DocumentoAdopcionesAsync(desde, hasta, periodo),
            "solicitudes" => await DocumentoSolicitudesAsync(desde, hasta, periodo),
            "mascotas" => await DocumentoMascotasAsync(desde, hasta, periodo),
            _ => throw new NegocioException("El tipo de reporte no es válido.")
        };
    }

    private async Task<ReporteDocumento> DocumentoGeneralAsync(string periodo)
    {
        var p = await PanelAsync();
        return new ReporteDocumento
        {
            Titulo = "Estadísticas generales de Adopta+",
            Periodo = "Situación a " + DateTime.Today.ToString(FechaCorta, CultureInfo.InvariantCulture),
            Secciones =
            {
                Indicadores(
                    ("Organizaciones activas", p.Organizaciones),
                    ("Organizaciones nuevas este mes", p.OrganizacionesNuevasMes),
                    ("Mascotas registradas", p.MascotasRegistradas),
                    ("Mascotas disponibles", p.MascotasDisponibles),
                    ("Adopciones realizadas", p.AdopcionesRealizadas),
                    ("Adopciones este mes", p.AdopcionesMes),
                    ("Solicitudes pendientes", p.SolicitudesPendientes),
                    ("Solicitudes por revisar", p.SolicitudesPorRevisar)),
                Mensual("Solicitudes recibidas por mes", p.ActividadMensual)
            }
        };
    }

    private async Task<ReporteDocumento> DocumentoSolicitudesAsync(DateTime? desde, DateTime? hasta, string periodo)
    {
        var r = await SolicitudesAsync(desde, hasta);

        var q = _db.SolicitudesAdopcions.AsNoTracking().AsQueryable();
        if (desde is DateTime d) { var ini = d.Date; q = q.Where(s => s.FechaSolicitud >= ini); }
        if (hasta is DateTime h) { var fin = h.Date.AddDays(1); q = q.Where(s => s.FechaSolicitud < fin); }

        var filas = await q.OrderByDescending(s => s.FechaSolicitud).Take(MaxFilasDetalle)
            .Select(s => new
            {
                s.FechaSolicitud,
                Mascota = s.IdMascotaNavigation.Nombre,
                Organizacion = s.IdMascotaNavigation.IdOrganizacionNavigation.Nombre,
                AdNombre = s.IdAdoptanteNavigation.Nombre,
                AdPaterno = s.IdAdoptanteNavigation.ApellidoPaterno,
                AdMaterno = s.IdAdoptanteNavigation.ApellidoMaterno,
                Estado = s.IdEstadoSolicitudNavigation.Nombre
            }).ToListAsync();

        return new ReporteDocumento
        {
            Titulo = "Reporte de solicitudes de adopción",
            Periodo = periodo,
            Secciones =
            {
                Indicadores(
                    ("Total de solicitudes", r.Total),
                    ("Aprobadas", r.Aprobadas),
                    ("Pendientes", r.Pendientes),
                    ("Rechazadas", r.Rechazadas),
                    ("Canceladas", r.Canceladas)),
                Mensual("Solicitudes por mes", r.PorMes),
                new ReporteSeccion
                {
                    Titulo = $"Detalle (máx. {MaxFilasDetalle})",
                    Columnas = new[] { "Fecha", "Mascota", "Organización", "Adoptante", "Estado" },
                    Filas = filas.Select(f => new object[]
                    {
                        f.FechaSolicitud, f.Mascota, f.Organizacion,
                        NombreCompleto(f.AdNombre, f.AdPaterno, f.AdMaterno), f.Estado
                    }).ToList()
                }
            }
        };
    }

    private async Task<ReporteDocumento> DocumentoAdopcionesAsync(DateTime? desde, DateTime? hasta, string periodo)
    {
        var r = await AdopcionesAsync(desde, hasta);

        var q = _db.Adopciones.AsNoTracking().AsQueryable();
        if (desde is DateTime d) { var ini = d.Date; q = q.Where(a => a.FechaAdopcion >= ini); }
        if (hasta is DateTime h) { var fin = h.Date.AddDays(1); q = q.Where(a => a.FechaAdopcion < fin); }

        var filas = await q.OrderByDescending(a => a.FechaAdopcion).Take(MaxFilasDetalle)
            .Select(a => new
            {
                a.FechaAdopcion,
                Mascota = a.IdSolicitudNavigation.IdMascotaNavigation.Nombre,
                Organizacion = a.IdSolicitudNavigation.IdMascotaNavigation.IdOrganizacionNavigation.Nombre,
                AdNombre = a.IdSolicitudNavigation.IdAdoptanteNavigation.Nombre,
                AdPaterno = a.IdSolicitudNavigation.IdAdoptanteNavigation.ApellidoPaterno,
                AdMaterno = a.IdSolicitudNavigation.IdAdoptanteNavigation.ApellidoMaterno
            }).ToListAsync();

        return new ReporteDocumento
        {
            Titulo = "Reporte de adopciones",
            Periodo = periodo,
            Secciones =
            {
                Indicadores(
                    ("Adopciones del periodo", r.Total),
                    ("Adopciones este mes", r.EsteMes),
                    ("Promedio mensual (últimos 12 meses)", r.PromedioMensual)),
                Mensual("Adopciones por mes", r.PorMes),
                new ReporteSeccion
                {
                    Titulo = $"Detalle (máx. {MaxFilasDetalle})",
                    Columnas = new[] { "Fecha", "Mascota", "Organización", "Adoptante" },
                    Filas = filas.Select(f => new object[]
                    {
                        f.FechaAdopcion, f.Mascota, f.Organizacion, NombreCompleto(f.AdNombre, f.AdPaterno, f.AdMaterno)
                    }).ToList()
                }
            }
        };
    }

    private async Task<ReporteDocumento> DocumentoMascotasAsync(DateTime? desde, DateTime? hasta, string periodo)
    {
        var r = await MascotasAsync(desde, hasta);

        var q = _db.Mascotas.AsNoTracking().AsQueryable();
        if (desde is DateTime d) { var ini = d.Date; q = q.Where(m => m.FechaRegistro >= ini); }
        if (hasta is DateTime h) { var fin = h.Date.AddDays(1); q = q.Where(m => m.FechaRegistro < fin); }

        var filas = await q.OrderByDescending(m => m.FechaRegistro).Take(MaxFilasDetalle)
            .Select(m => new
            {
                m.FechaRegistro,
                m.Nombre,
                m.Especie,
                Organizacion = m.IdOrganizacionNavigation.Nombre,
                Estado = m.IdEstadoMascotaNavigation.Nombre
            }).ToListAsync();

        return new ReporteDocumento
        {
            Titulo = "Estadísticas de mascotas",
            Periodo = periodo,
            Secciones =
            {
                Indicadores(
                    ("Mascotas registradas (total actual)", r.Total),
                    ("Disponibles", r.Disponibles),
                    ("En proceso de adopción", r.EnProceso),
                    ("Adoptadas", r.Adoptadas),
                    ("Otros estados", r.Otras)),
                Mensual("Mascotas registradas por mes", r.PorMes),
                new ReporteSeccion
                {
                    Titulo = $"Mascotas registradas en el periodo (máx. {MaxFilasDetalle})",
                    Columnas = new[] { "Registro", "Nombre", "Especie", "Organización", "Estado actual" },
                    Filas = filas.Select(f => new object[] { f.FechaRegistro, f.Nombre, f.Especie, f.Organizacion, f.Estado }).ToList()
                }
            }
        };
    }

    // =================== Helpers ===================

    private static ReporteSeccion Indicadores(params (string Nombre, int Valor)[] items) => new()
    {
        Titulo = "Indicadores",
        Columnas = new[] { "Indicador", "Valor" },
        Filas = items.Select(i => new object[] { i.Nombre, i.Valor }).ToList()
    };

    private static ReporteSeccion Mensual(string titulo, List<SerieMensualDto> serie) => new()
    {
        Titulo = titulo,
        Columnas = new[] { "Mes", "Cantidad" },
        Filas = serie.Select(s => new object[] { s.Etiqueta, s.Valor }).ToList()
    };

    private static string Periodo(DateTime? desde, DateTime? hasta) =>
        desde is null && hasta is null
            ? "Todo el historial"
            : $"Del {(desde?.ToString(FechaCorta, CultureInfo.InvariantCulture) ?? "inicio")} al {(hasta?.ToString(FechaCorta, CultureInfo.InvariantCulture) ?? DateTime.Today.ToString(FechaCorta, CultureInfo.InvariantCulture))}";

    private static void ValidarRango(DateTime? desde, DateTime? hasta)
    {
        if (desde is DateTime d && hasta is DateTime h && d.Date > h.Date)
            throw new NegocioException("La fecha de inicio no puede ser posterior a la fecha de fin.");
    }

    private static int Porcentaje(int parte, int total) => total == 0 ? 0 : (int)Math.Round(parte * 100.0 / total);

    private static string NombreDe(IEnumerable<(int Id, string Nombre)> catalogo, int id) =>
        catalogo.FirstOrDefault(e => e.Id == id).Nombre ?? "";

    /// <summary>Primer mes de la gráfica y cuántos meses lleva (8 por defecto; el rango pedido, máx. 36).</summary>
    private static (DateTime Inicio, int Meses) Ventana(DateTime? desde, DateTime? hasta)
    {
        var fin = hasta?.Date ?? DateTime.Today;
        var finMes = new DateTime(fin.Year, fin.Month, 1);
        var inicioMes = desde is DateTime d ? new DateTime(d.Year, d.Month, 1) : finMes.AddMonths(-(MesesPorDefecto - 1));

        var meses = (finMes.Year - inicioMes.Year) * 12 + finMes.Month - inicioMes.Month + 1;
        if (meses > MaxMeses) { meses = MaxMeses; inicioMes = finMes.AddMonths(-(MaxMeses - 1)); }
        return (inicioMes, Math.Max(meses, 1));
    }

    /// <summary>Fechas a graficar: las del periodo pedido; sin inicio, las de la ventana por defecto.</summary>
    private static async Task<List<DateTime>> FechasAsync(IQueryable<DateTime> fechas, DateTime? desde, DateTime? hasta, DateTime inicioVentana)
    {
        var ini = desde?.Date ?? inicioVentana;
        fechas = fechas.Where(f => f >= ini);
        if (hasta is DateTime h)
        {
            var fin = h.Date.AddDays(1);
            fechas = fechas.Where(f => f < fin);
        }
        return await fechas.ToListAsync();
    }

    private static List<SerieMensualDto> Serie(IEnumerable<DateTime> fechas, DateTime inicio, int meses)
    {
        var conteo = fechas.GroupBy(f => (f.Year, f.Month)).ToDictionary(g => g.Key, g => g.Count());
        var variosAnios = inicio.Year != inicio.AddMonths(meses - 1).Year;

        var serie = new List<SerieMensualDto>(meses);
        for (var i = 0; i < meses; i++)
        {
            var m = inicio.AddMonths(i);
            serie.Add(new SerieMensualDto
            {
                Periodo = m.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                Etiqueta = variosAnios ? $"{Meses[m.Month - 1]} {m.Year % 100:00}" : Meses[m.Month - 1],
                Valor = conteo.GetValueOrDefault((m.Year, m.Month))
            });
        }
        return serie;
    }
}
