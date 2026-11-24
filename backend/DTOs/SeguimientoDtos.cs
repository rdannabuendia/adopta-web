using System.ComponentModel.DataAnnotations;

namespace Adopta.Api.DTOs;

// =================== Consultas ===================

public class SeguimientoQueryDto
{
    public string? Buscar { get; set; }
    public int? IdEstadoAdaptacion { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? IdOrganizacion { get; set; }
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 10;
}

public class DevolucionQueryDto
{
    public string? Buscar { get; set; }
    public string? Estado { get; set; }
    public int? IdOrganizacion { get; set; }
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 10;
}

// =================== Listado de adopciones en seguimiento ===================

public class SeguimientoListaDto
{
    public int IdAdopcion { get; set; }
    public int IdMascota { get; set; }
    public string Mascota { get; set; } = null!;
    public string Especie { get; set; } = null!;
    public string? FotoUrl { get; set; }
    public string Adoptante { get; set; } = null!;
    public DateTime FechaAdopcion { get; set; }
    public int IdEstadoAdaptacion { get; set; }
    public string EstadoAdaptacion { get; set; } = null!;
    public int IdOrganizacion { get; set; }
    public string Organizacion { get; set; } = null!;
}

public class EstadoAdaptacionDto
{
    public int IdEstadoAdaptacion { get; set; }
    public string Nombre { get; set; } = null!;
}

// =================== Detalle de seguimiento ===================

public class SeguimientoDetalleDto
{
    public int IdAdopcion { get; set; }
    public int IdMascota { get; set; }
    public string Mascota { get; set; } = null!;
    public string Especie { get; set; } = null!;
    public string? Raza { get; set; }
    public string? EdadAproximada { get; set; }
    public string? FotoUrl { get; set; }

    public int IdOrganizacion { get; set; }
    public string Organizacion { get; set; } = null!;

    public string Adoptante { get; set; } = null!;
    public string? AdoptanteCorreo { get; set; }
    public string? AdoptanteTelefono { get; set; }

    public DateTime FechaAdopcion { get; set; }
    /// <summary>Último reporte del adoptante o último cambio de estado (lo que sea más reciente).</summary>
    public DateTime UltimaActualizacion { get; set; }
    /// <summary>Calculada: última actualización + días de revisión (config Seguimiento:DiasRevision).</summary>
    public DateTime ProximaRevision { get; set; }
    public bool RevisionVencida { get; set; }

    public int IdEstadoAdaptacion { get; set; }
    public string EstadoAdaptacion { get; set; } = null!;

    public int? IdDevolucionPendiente { get; set; }

    public List<ActividadSeguimientoDto> Actividad { get; set; } = new();
    public List<ReporteSeguimientoDto> Reportes { get; set; } = new();
}

public class ActividadSeguimientoDto
{
    public string Tipo { get; set; } = null!;
    public string? Estado { get; set; }
    public string? Detalle { get; set; }
    public DateTime Fecha { get; set; }
}

public class ReporteSeguimientoDto
{
    public int IdReporte { get; set; }
    public DateTime Fecha { get; set; }
    public string? Comportamiento { get; set; }
    public string? Salud { get; set; }
    public string? Alimentacion { get; set; }
    public string? Adaptacion { get; set; }
    public string? Observaciones { get; set; }
    public List<string> Fotos { get; set; } = new();
}

public class ActualizarAdaptacionRequestDto
{
    [Required(ErrorMessage = "Selecciona el estado de adaptación.")]
    public int? IdEstadoAdaptacion { get; set; }

    [MaxLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres.")]
    public string? Observaciones { get; set; }
}

// =================== Solicitudes de devolución ===================

public class DevolucionListaDto
{
    public int IdDevolucion { get; set; }
    public string Codigo { get; set; } = null!;
    public string Adoptante { get; set; } = null!;
    public string Mascota { get; set; } = null!;
    public string Especie { get; set; } = null!;
    public DateTime FechaSolicitud { get; set; }
    public string Estado { get; set; } = null!;
    public int IdOrganizacion { get; set; }
    public string Organizacion { get; set; } = null!;
}

public class DevolucionDetalleDto
{
    public int IdDevolucion { get; set; }
    public string Codigo { get; set; } = null!;
    public int IdAdopcion { get; set; }
    public string Estado { get; set; } = null!;
    public string Organizacion { get; set; } = null!;

    public DevolucionAdoptanteDto Adoptante { get; set; } = null!;
    public DevolucionMascotaDto Mascota { get; set; } = null!;

    public List<string> Motivos { get; set; } = new();
    public string Explicacion { get; set; } = null!;

    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaRevision { get; set; }
    public string? ObservacionesOrganizacion { get; set; }
}

public class DevolucionAdoptanteDto
{
    public string Nombre { get; set; } = null!;
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? FotoUrl { get; set; }
}

public class DevolucionMascotaDto
{
    public int IdMascota { get; set; }
    public string Nombre { get; set; } = null!;
    public string Especie { get; set; } = null!;
    public string? Raza { get; set; }
    public string? EdadAproximada { get; set; }
    public string? FotoUrl { get; set; }
}

public class AceptarDevolucionRequestDto
{
    [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres.")]
    public string? Observaciones { get; set; }
}
