using System.ComponentModel.DataAnnotations;

namespace Adopta.Api.DTOs;

public class SolicitudesQueryDto
{
    public string? Buscar { get; set; }
    public int? IdEstadoSolicitud { get; set; }
    public DateTime? Fecha { get; set; }
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 10;
}

public class SolicitudListaDto
{
    public int IdSolicitud { get; set; }
    public string Adoptante { get; set; } = null!;
    public string? AdoptanteFotoUrl { get; set; }
    public int IdMascota { get; set; }
    public string Mascota { get; set; } = null!;
    public DateTime FechaSolicitud { get; set; }
    public int IdEstadoSolicitud { get; set; }
    public string Estado { get; set; } = null!;
}

public class EstadoSolicitudDto
{
    public int IdEstadoSolicitud { get; set; }
    public string Nombre { get; set; } = null!;
}

public class SolicitudDetalleDto
{
    public int IdSolicitud { get; set; }
    public int IdEstadoSolicitud { get; set; }
    public string Estado { get; set; } = null!;
    public bool EsFinal { get; set; }

    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public string? ObservacionesOrganizacion { get; set; }
    public DateTime? FechaLimiteDocumentos { get; set; }
    public bool DocumentacionCompleta { get; set; }

    public string? MotivoAdopcion { get; set; }
    public string? ExperienciaPrevia { get; set; }
    public string? CondicionesVivienda { get; set; }

    public SolicitudAdoptanteDto Adoptante { get; set; } = null!;
    public SolicitudMascotaDto Mascota { get; set; } = null!;
    public SolicitudHogarDto? Hogar { get; set; }
    public List<SolicitudReferenciaDto> Referencias { get; set; } = new();
    public List<SolicitudHistorialDto> Historial { get; set; } = new();
}

public class SolicitudAdoptanteDto
{
    public string Nombre { get; set; } = null!;
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? FotoUrl { get; set; }
}

public class SolicitudMascotaDto
{
    public int IdMascota { get; set; }
    public string Nombre { get; set; } = null!;
    public string Especie { get; set; } = null!;
    public string? Raza { get; set; }
    public string? Tamano { get; set; }
    public string? EdadAproximada { get; set; }
    public string? FotoUrl { get; set; }
}

public class SolicitudHogarDto
{
    public string RegimenVivienda { get; set; } = null!;
    public bool? ArrendadorPermiteMascotas { get; set; }
    public bool TieneEspacioExterior { get; set; }
    public bool? EspacioExteriorCercado { get; set; }
    public int NumPersonasHogar { get; set; }
    public bool HayMenores { get; set; }
    public bool HogarDeAcuerdo { get; set; }
    public string HorasSoloDia { get; set; } = null!;
    public string LugarDormir { get; set; } = null!;
    public string? CuidadorAlterno { get; set; }
    public string PresupuestoMensual { get; set; } = null!;
    public bool TieneOtrasMascotas { get; set; }
    public string? EspeciesOtrasMascotas { get; set; }
    public bool AceptaVisitaDomiciliaria { get; set; }
    public bool AceptaSeguimiento { get; set; }
}

public class SolicitudReferenciaDto
{
    public string Nombre { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public string Relacion { get; set; } = null!;
    public int? AniosConocidos { get; set; }
}

public class SolicitudHistorialDto
{
    public string Estado { get; set; } = null!;
    public string? Observacion { get; set; }
    public DateTime Fecha { get; set; }
}

public class SolicitudRequisitoDto
{
    public int IdRequisito { get; set; }
    public string Tipo { get; set; } = null!;
    public bool Obligatorio { get; set; }
    public string Estado { get; set; } = null!;
    public string? MotivoRechazo { get; set; }
    public List<SolicitudArchivoDto> Archivos { get; set; } = new();
}

public class SolicitudArchivoDto
{
    public int IdArchivo { get; set; }
    public string NombreOriginal { get; set; } = null!;
}

public class SolicitarInformacionRequestDto
{
    [Required(ErrorMessage = "Escribe el mensaje para el adoptante.")]
    public string? Mensaje { get; set; }
}

public class ResolverSolicitudRequestDto
{
    [Required(ErrorMessage = "Selecciona el dictamen de la solicitud.")]
    public string? Decision { get; set; }
    public string? Observaciones { get; set; }
}
