using Adopta.Api.Data;
using Adopta.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Api.Services;

public interface IPublicoService
{
    /// <summary>Mascotas disponibles con foto, en orden aleatorio, para el carrusel de la landing.</summary>
    Task<List<MascotaPublicaDto>> MascotasAleatoriasAsync(int cantidad);
}

public class PublicoService : IPublicoService
{
    private const string Disponible = "Disponible";

    private readonly AdoptaDbContext _db;

    public PublicoService(AdoptaDbContext db) => _db = db;

    public async Task<List<MascotaPublicaDto>> MascotasAleatoriasAsync(int cantidad)
    {
        cantidad = Math.Clamp(cantidad, 1, 20);

        // Solo mascotas disponibles, con al menos una foto y de organizaciones activas.
        var ids = await _db.Mascotas.AsNoTracking()
            .Where(m => m.IdEstadoMascotaNavigation.Nombre == Disponible
                        && m.IdOrganizacionNavigation.Estado == "Activa"
                        && m.FotosMascota.Any())
            .Select(m => m.IdMascota)
            .ToListAsync();

        // El azar se hace en memoria (funciona en cualquier proveedor de BD).
        var elegidos = ids.OrderBy(_ => Random.Shared.Next()).Take(cantidad).ToList();
        if (elegidos.Count == 0) return new List<MascotaPublicaDto>();

        var items = await _db.Mascotas.AsNoTracking()
            .Where(m => elegidos.Contains(m.IdMascota))
            .Select(m => new MascotaPublicaDto
            {
                IdMascota = m.IdMascota,
                Nombre = m.Nombre,
                Especie = m.Especie,
                Raza = m.Raza,
                Sexo = m.Sexo,
                Tamano = m.Tamano,
                EdadAproximada = m.EdadAproximada,
                FotoUrl = m.FotosMascota
                    .OrderByDescending(x => x.EsPrincipal).ThenBy(x => x.IdFoto)
                    .Select(x => x.RutaFoto).FirstOrDefault(),
                Organizacion = m.IdOrganizacionNavigation.Nombre
            })
            .ToListAsync();

        foreach (var i in items) i.FotoUrl = PerfilService.UrlFoto(i.FotoUrl);

        // Conserva el orden aleatorio elegido
        return items.OrderBy(i => elegidos.IndexOf(i.IdMascota)).ToList();
    }
}
