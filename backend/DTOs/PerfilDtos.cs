namespace Adopta.Api.DTOs;

public class PerfilOrganizacionDto
{
    public int IdOrganizacion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? TipoOrganizacion { get; set; }
    public string? Descripcion { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Telefono { get; set; }
    public string? CorreoContacto { get; set; }
    public string? SitioWeb { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class PerfilDto
{
    public int IdUsuario { get; set; }
    public string Rol { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;       
    public string NombreCompleto { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Telefono { get; set; }
    public string? FotoUrl { get; set; }                    
    public DateTime FechaRegistro { get; set; }
    public PerfilOrganizacionDto? Organizacion { get; set; } 
}

public class ActualizarPerfilRequestDto
{
    public string? Nombre { get; set; }
    public string? Telefono { get; set; }
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? TipoOrganizacion { get; set; }
    public string? Descripcion { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? CodigoPostal { get; set; }
    public string? CorreoContacto { get; set; }
    public string? SitioWeb { get; set; }
    public IFormFile? Foto { get; set; }
    public bool EliminarFoto { get; set; }
}