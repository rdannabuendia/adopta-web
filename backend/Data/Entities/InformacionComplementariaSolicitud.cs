using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class InformacionComplementariaSolicitud
{
    public int IdInfo { get; set; }

    public int IdSolicitud { get; set; }

    public string RegimenVivienda { get; set; } = null!;

    public bool? ArrendadorPermiteMascotas { get; set; }

    public bool TieneEspacioExterior { get; set; }

    public bool? EspacioExteriorCercado { get; set; }

    public ushort? SuperficieM2 { get; set; }

    public byte NumPersonasHogar { get; set; }

    public bool HayMenores { get; set; }

    public bool HogarDeAcuerdo { get; set; }

    public string HorasSoloDia { get; set; } = null!;

    public string LugarDormir { get; set; } = null!;

    public string? CuidadorAlterno { get; set; }

    public string PresupuestoMensual { get; set; } = null!;

    public bool TieneOtrasMascotas { get; set; }

    public byte? CantidadOtrasMascotas { get; set; }

    public string? EspeciesOtrasMascotas { get; set; }

    public bool? OtrasEsterilizadas { get; set; }

    public bool? OtrasVacunadas { get; set; }

    public string? HistorialMascotasPrevias { get; set; }

    public string? VeterinarioNombre { get; set; }

    public string? VeterinarioTelefono { get; set; }

    public bool AceptaVisitaDomiciliaria { get; set; }

    public bool AceptaSeguimiento { get; set; }

    public bool? CompromisoEsterilizacion { get; set; }

    public bool AceptaAvisoPrivacidad { get; set; }

    public bool DeclaracionVeracidad { get; set; }

    public string? FirmaNombre { get; set; }

    public DateTime? FechaFirma { get; set; }

    public string EstadoFormulario { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual SolicitudesAdopcion IdSolicitudNavigation { get; set; } = null!;
}
