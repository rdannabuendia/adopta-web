namespace Adopta.Api.DTOs;

public class NotificacionDto
{
    public int IdNotificacion { get; set; }
    public string Titulo { get; set; } = null!;
    public string Mensaje { get; set; } = null!;
    public string? Tipo { get; set; }
    public bool Leida { get; set; }
    public DateTime Fecha { get; set; }
    public int? IdSolicitud { get; set; }
}

public class NotificacionesResumenDto
{
    public int NoLeidas { get; set; }
    public List<NotificacionDto> Items { get; set; } = new();
}

public class NotificacionesContadorDto
{
    public int NoLeidas { get; set; }
}
