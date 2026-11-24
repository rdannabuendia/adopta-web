namespace Adopta.Api.DTOs;

public class SerieMensualDto
{
    public string Periodo { get; set; } = null!;
    public string Etiqueta { get; set; } = null!;
    public int Valor { get; set; }
}

public class PanelGeneralDto
{
    public int Organizaciones { get; set; }
    public int OrganizacionesNuevasMes { get; set; }
    public int MascotasRegistradas { get; set; }
    public int MascotasDisponibles { get; set; }
    public int AdopcionesRealizadas { get; set; }
    public int AdopcionesMes { get; set; }
    public int SolicitudesPendientes { get; set; }
    public int SolicitudesPorRevisar { get; set; }
    public List<SerieMensualDto> ActividadMensual { get; set; } = new();
}

public class ReporteSolicitudesDto
{
    public int Total { get; set; }
    public int Aprobadas { get; set; }
    public int Pendientes { get; set; }
    public int Rechazadas { get; set; }
    public int Canceladas { get; set; }
    public int PorcentajeAprobadas { get; set; }
    public int PorcentajePendientes { get; set; }
    public int PorcentajeRechazadas { get; set; }
    public List<SerieMensualDto> PorMes { get; set; } = new();
}

public class ReporteMascotasDto
{
    public int Total { get; set; }
    public int Disponibles { get; set; }
    public int EnProceso { get; set; }
    public int Adoptadas { get; set; }
    public int Otras { get; set; }
    public int PorcentajeDisponibles { get; set; }
    public int PorcentajeEnProceso { get; set; }
    public int PorcentajeAdoptadas { get; set; }
    public List<SerieMensualDto> PorMes { get; set; } = new();
}

public class ReporteAdopcionesDto
{
    public int Total { get; set; }
    public int EsteMes { get; set; }
    public int PromedioMensual { get; set; }
    public List<SerieMensualDto> PorMes { get; set; } = new();
}

public class ReportesQueryDto
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}

public class ExportarReporteQueryDto
{
    public string? Tipo { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public string? Formato { get; set; }
    public string? Nombre { get; set; }
}

// ---- Modelo neutro que luego se escribe a PDF o Excel ----

public class ReporteDocumento
{
    public string Titulo { get; set; } = null!;
    public string Periodo { get; set; } = null!;
    public List<ReporteSeccion> Secciones { get; set; } = new();
}

public class ReporteSeccion
{
    public string Titulo { get; set; } = null!;
    public string[] Columnas { get; set; } = Array.Empty<string>();
    public List<object[]> Filas { get; set; } = new();
}
