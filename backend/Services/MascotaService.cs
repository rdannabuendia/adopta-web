using System.Globalization;
using Adopta.Api.Data;
using Adopta.Api.Data.Entities;
using Adopta.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Api.Services;

public interface IMascotaService
{
    Task<int> IdOrganizacionDeUsuarioAsync(int idUsuario);

    // Admin
    Task<PagedResultDto<OrganizacionMascotasDto>> OrganizacionesAsync(OrganizacionesQueryDto f);

    // Admin y organización
    Task<PagedResultDto<MascotaListaDto>> ListarAsync(int idOrganizacion, MascotasQueryDto f, bool ocultarNoDisponibles);
    Task<MascotaDetalleDto> DetalleAsync(int idMascota, int? idOrganizacion);

    // Organización
    Task<int> CrearAsync(int idOrganizacion, MascotaRequestDto dto);
    Task ActualizarAsync(int idMascota, int idOrganizacion, MascotaRequestDto dto);
    Task CambiarEstadoAsync(int idMascota, int idOrganizacion, CambiarEstadoMascotaRequestDto dto);
}

public class MascotaService : IMascotaService
{
    private const long MaxFotoBytes = 5 * 1024 * 1024;
    private const int MaxFotos = 6;
    private const int MaxCaracteristicas = 8;
    private const string Disponible = "Disponible";
    private const string NoDisponible = "No disponible";

    private static readonly string[] Especies = { "Perro", "Gato", "Otro" };
    private static readonly string[] Sexos = { "Macho", "Hembra", "Desconocido" };
    private static readonly string[] Tamanos = { "Pequeno", "Mediano", "Grande" };

    private static readonly Dictionary<string, byte[]> FormatosFoto = new()
    {
        [".png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
        [".jpg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        [".jpeg"] = new byte[] { 0xFF, 0xD8, 0xFF },
    };

    private readonly AdoptaDbContext _db;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private readonly ILogger<MascotaService> _logger;

    public MascotaService(AdoptaDbContext db, IConfiguration config, IHostEnvironment env, ILogger<MascotaService> logger)
    {
        _db = db;
        _config = config;
        _env = env;
        _logger = logger;
    }

    private string PublicRoot => Path.Combine(_env.ContentRootPath, _config["Storage:PublicRoot"] ?? "Uploads");

    // ---------- Organización del usuario en sesión ----------
    public async Task<int> IdOrganizacionDeUsuarioAsync(int idUsuario)
    {
        var org = await _db.Organizaciones.AsNoTracking()
            .Where(o => o.IdUsuario == idUsuario)
            .Select(o => new { o.IdOrganizacion, o.Estado })
            .FirstOrDefaultAsync()
            ?? throw new NegocioException("No se encontró tu organización.", StatusCodes.Status404NotFound);

        if (org.Estado != "Activa")
            throw new NegocioException("Tu organización no está activa.", StatusCodes.Status403Forbidden);

        return org.IdOrganizacion;
    }

    // ---------- Admin: cards de organizaciones ----------
    public async Task<PagedResultDto<OrganizacionMascotasDto>> OrganizacionesAsync(OrganizacionesQueryDto f)
    {
        var buscar = f.Buscar?.Trim();

        var q = from o in _db.Organizaciones.AsNoTracking()
                join u in _db.Usuarios.AsNoTracking() on o.IdUsuario equals u.IdUsuario
                where o.Estado != "Pendiente"
                select new { o, u };

        if (!string.IsNullOrEmpty(buscar))
            q = q.Where(x => x.o.Nombre.Contains(buscar) || (x.o.Ciudad != null && x.o.Ciudad.Contains(buscar)));

        var (pagina, tamano) = Paginar(f.Pagina, f.Tamano, 24);
        var total = await q.CountAsync();

        var items = await q
            .OrderBy(x => x.o.Nombre)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(x => new OrganizacionMascotasDto
            {
                IdOrganizacion = x.o.IdOrganizacion,
                Nombre = x.o.Nombre,
                FotoUrl = x.u.FotoPerfil,
                Ciudad = x.o.Ciudad,
                TipoOrganizacion = x.o.TipoOrganizacion,
                Estado = x.o.Estado,
                TotalMascotas = _db.Mascotas.Count(m => m.IdOrganizacion == x.o.IdOrganizacion),
                Disponibles = _db.Mascotas.Count(m => m.IdOrganizacion == x.o.IdOrganizacion
                                                      && m.IdEstadoMascotaNavigation.Nombre == Disponible)
            })
            .ToListAsync();

        foreach (var i in items) i.FotoUrl = PerfilService.UrlFoto(i.FotoUrl);
        return Resultado(items, total, pagina, tamano);
    }

    // ---------- Listado de mascotas de una organización ----------
    public async Task<PagedResultDto<MascotaListaDto>> ListarAsync(int idOrganizacion, MascotasQueryDto f, bool ocultarNoDisponibles)
    {
        var buscar = f.Buscar?.Trim();
        var especie = f.Especie;
        var tamanoMascota = f.Tamano;
        var edad = f.Edad;
        var estado = f.Estado;

        var q = _db.Mascotas.AsNoTracking().Where(m => m.IdOrganizacion == idOrganizacion);

        if (!string.IsNullOrEmpty(buscar))
            q = q.Where(m => m.Nombre.Contains(buscar) || (m.Raza != null && m.Raza.Contains(buscar)));
        if (!string.IsNullOrEmpty(especie)) q = q.Where(m => m.Especie == especie);
        if (!string.IsNullOrEmpty(tamanoMascota)) q = q.Where(m => m.Tamano == tamanoMascota);
        if (!string.IsNullOrEmpty(edad)) q = q.Where(m => m.EdadAproximada == edad);

        if (!string.IsNullOrEmpty(estado))
            q = q.Where(m => m.IdEstadoMascotaNavigation.Nombre == estado);
        else if (ocultarNoDisponibles)
            q = q.Where(m => m.IdEstadoMascotaNavigation.Nombre != NoDisponible);

        var (pagina, porPagina) = Paginar(f.Pagina, f.PorPagina, 30);
        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(m => m.FechaRegistro).ThenBy(m => m.IdMascota)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(m => new MascotaListaDto
            {
                IdMascota = m.IdMascota,
                Nombre = m.Nombre,
                Especie = m.Especie,
                Raza = m.Raza,
                Sexo = m.Sexo,
                Tamano = m.Tamano,
                EdadAproximada = m.EdadAproximada,
                Estado = m.IdEstadoMascotaNavigation.Nombre,
                FotoUrl = m.FotosMascota
                    .OrderByDescending(x => x.EsPrincipal).ThenBy(x => x.IdFoto)
                    .Select(x => x.RutaFoto).FirstOrDefault(),
                IdOrganizacion = m.IdOrganizacion,
                Organizacion = m.IdOrganizacionNavigation.Nombre,
                OrganizacionFotoUrl = m.IdOrganizacionNavigation.IdUsuarioNavigation.FotoPerfil
            })
            .ToListAsync();

        foreach (var i in items)
        {
            i.FotoUrl = PerfilService.UrlFoto(i.FotoUrl);
            i.OrganizacionFotoUrl = PerfilService.UrlFoto(i.OrganizacionFotoUrl);
        }

        return Resultado(items, total, pagina, porPagina);
    }

    // ---------- Detalle (idOrganizacion null = admin, puede ver cualquiera) ----------
    public async Task<MascotaDetalleDto> DetalleAsync(int idMascota, int? idOrganizacion)
    {
        var m = await _db.Mascotas.AsNoTracking()
            .Include(x => x.IdEstadoMascotaNavigation)
            .Include(x => x.IdOrganizacionNavigation)
            .Include(x => x.FotosMascota)
            .FirstOrDefaultAsync(x => x.IdMascota == idMascota
                                      && (idOrganizacion == null || x.IdOrganizacion == idOrganizacion))
            ?? throw new NegocioException("Mascota no encontrada.", StatusCodes.Status404NotFound);

        var estado = m.IdEstadoMascotaNavigation.Nombre;

        return new MascotaDetalleDto
        {
            IdMascota = m.IdMascota,
            IdOrganizacion = m.IdOrganizacion,
            Organizacion = m.IdOrganizacionNavigation.Nombre,
            Nombre = m.Nombre,
            Especie = m.Especie,
            Raza = m.Raza,
            Sexo = m.Sexo,
            Tamano = m.Tamano,
            EdadAproximada = m.EdadAproximada,
            Color = m.Color,
            Peso = m.Peso,
            Descripcion = m.Descripcion,
            EstadoSalud = m.EstadoSalud,
            Vacunado = m.Vacunado,
            Esterilizado = m.Esterilizado,
            Caracteristicas = ParsearCaracteristicas(m.Comportamiento),
            Estado = estado,
            FechaRegistro = m.FechaRegistro,
            Fotos = m.FotosMascota
                .OrderByDescending(f => f.EsPrincipal).ThenBy(f => f.IdFoto)
                .Select(f => new FotoMascotaDto
                {
                    IdFoto = f.IdFoto,
                    Url = PerfilService.UrlFoto(f.RutaFoto)!,
                    EsPrincipal = f.EsPrincipal
                }).ToList(),
            EstadosDestino = DestinosPermitidos(estado)
        };
    }

    // ---------- Crear (Transacción atómica: Mascota + Fotos) ----------
    public async Task<int> CrearAsync(int idOrganizacion, MascotaRequestDto dto)
    {
        var datos = Validar(dto);

        var nuevas = dto.Fotos ?? new List<IFormFile>();
        if (nuevas.Count == 0) throw new NegocioException("Agrega al menos una fotografía.");
        if (nuevas.Count > MaxFotos) throw new NegocioException($"Máximo {MaxFotos} fotografías.");
        var extensiones = new List<string>();
        foreach (var f in nuevas) extensiones.Add(await ValidarFotoAsync(f));

        var idDisponible = await IdEstadoAsync(Disponible);
        var idSinSeguimiento = await _db.EstadosAdaptacions.AsNoTracking()
            .Where(e => e.Nombre == "Sin seguimiento").Select(e => e.IdEstadoAdaptacion).FirstAsync();

        var m = new Mascota
        {
            IdOrganizacion = idOrganizacion,
            IdEstadoMascota = idDisponible,
            IdEstadoAdaptacion = idSinSeguimiento
        };
        Aplicar(m, datos);

        var guardadas = new List<string>();
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Mascotas.Add(m);
            await _db.SaveChangesAsync();

            var principal = dto.IndicePrincipalNueva is int i && i >= 0 && i < nuevas.Count ? i : 0;
            for (var k = 0; k < nuevas.Count; k++)
            {
                var ruta = await GuardarFotoAsync(m.IdMascota, nuevas[k], extensiones[k]);
                guardadas.Add(ruta);
                _db.FotosMascota.Add(new FotosMascotum { IdMascota = m.IdMascota, RutaFoto = ruta, EsPrincipal = k == principal });
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            foreach (var r in guardadas) BorrarFoto(r);
            throw;
        }

        return m.IdMascota;
    }

    // ---------- Actualizar (Transacción atómica: Datos Mascota + Fotos) ----------
    public async Task ActualizarAsync(int idMascota, int idOrganizacion, MascotaRequestDto dto)
    {
        var m = await _db.Mascotas.Include(x => x.FotosMascota)
            .FirstOrDefaultAsync(x => x.IdMascota == idMascota && x.IdOrganizacion == idOrganizacion)
            ?? throw new NegocioException("Mascota no encontrada.", StatusCodes.Status404NotFound);

        var datos = Validar(dto);

        var eliminar = (dto.FotosEliminar ?? new List<int>()).ToHashSet();
        var quitar = m.FotosMascota.Where(f => eliminar.Contains(f.IdFoto)).ToList();
        var restantes = m.FotosMascota.Except(quitar).ToList();
        var nuevas = dto.Fotos ?? new List<IFormFile>();

        if (restantes.Count + nuevas.Count == 0)
            throw new NegocioException("La mascota debe tener al menos una fotografía.");
        if (restantes.Count + nuevas.Count > MaxFotos)
            throw new NegocioException($"Máximo {MaxFotos} fotografías.");

        var extensiones = new List<string>();
        foreach (var f in nuevas) extensiones.Add(await ValidarFotoAsync(f));

        var guardadas = new List<string>();
        var entidadesNuevas = new List<FotosMascotum>();

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            Aplicar(m, datos);

            var principalActual = restantes.FirstOrDefault(f => f.EsPrincipal);
            _db.FotosMascota.RemoveRange(quitar);

            for (var k = 0; k < nuevas.Count; k++)
            {
                var ruta = await GuardarFotoAsync(m.IdMascota, nuevas[k], extensiones[k]);
                guardadas.Add(ruta);
                var foto = new FotosMascotum { IdMascota = m.IdMascota, RutaFoto = ruta };
                entidadesNuevas.Add(foto);
                _db.FotosMascota.Add(foto);
            }

            FotosMascotum? principal = null;
            if (dto.IndicePrincipalNueva is int i && i >= 0 && i < entidadesNuevas.Count)
                principal = entidadesNuevas[i];
            else if (dto.IdFotoPrincipal is int idp)
                principal = restantes.FirstOrDefault(f => f.IdFoto == idp);
            principal ??= principalActual ?? restantes.FirstOrDefault() ?? entidadesNuevas.First();

            foreach (var f in restantes.Concat(entidadesNuevas)) f.EsPrincipal = f == principal;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            foreach (var r in guardadas) BorrarFoto(r);
            throw;
        }

        foreach (var f in quitar) BorrarFoto(f.RutaFoto);
    }

    // ---------- Cambiar estatus (también es "eliminar": No disponible) ----------
    public async Task CambiarEstadoAsync(int idMascota, int idOrganizacion, CambiarEstadoMascotaRequestDto dto)
    {
        var m = await _db.Mascotas.Include(x => x.IdEstadoMascotaNavigation)
            .FirstOrDefaultAsync(x => x.IdMascota == idMascota && x.IdOrganizacion == idOrganizacion)
            ?? throw new NegocioException("Mascota no encontrada.", StatusCodes.Status404NotFound);

        var actual = m.IdEstadoMascotaNavigation.Nombre;
        var destinos = DestinosPermitidos(actual);

        if (destinos.Count == 0)
            throw new NegocioException(
                "Esta mascota está en un proceso de adopción; su estatus cambia desde las solicitudes.",
                StatusCodes.Status409Conflict);
        if (!destinos.Contains(dto.Estado))
            throw new NegocioException("El estatus seleccionado no es válido para esta mascota.");

        var motivo = dto.Motivo?.Trim();
        if (motivo is { Length: > 500 }) throw new NegocioException("El motivo no puede exceder 500 caracteres.");

        m.IdEstadoMascota = await IdEstadoAsync(dto.Estado);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Mascota {Id} cambió de '{De}' a '{A}'. Motivo: {Motivo}",
            idMascota, actual, dto.Estado, string.IsNullOrEmpty(motivo) ? "(sin motivo)" : motivo);
    }

    // =================== Helpers ===================
    private static List<string> DestinosPermitidos(string actual) => actual switch
    {
        Disponible => new List<string> { NoDisponible },
        NoDisponible => new List<string> { Disponible },
        _ => new List<string>()
    };

    private Task<int> IdEstadoAsync(string nombre) =>
        _db.EstadosMascota.AsNoTracking().Where(e => e.Nombre == nombre).Select(e => e.IdEstadoMascota).FirstAsync();

    private static List<string> ParsearCaracteristicas(string? texto) =>
        string.IsNullOrWhiteSpace(texto)
            ? new List<string>()
            : texto.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private sealed record DatosMascota(
        string Nombre, string Especie, string Raza, string Sexo, string Tamano, string Edad,
        string? Color, decimal? Peso, string Descripcion, string? EstadoSalud,
        bool Vacunado, bool Esterilizado, string? Comportamiento);

    private static DatosMascota Validar(MascotaRequestDto dto)
    {
        var nombre = Requerido(dto.Nombre, "el nombre", 80);

        var especie = Requerido(dto.Especie, "la especie", 50);
        if (!Especies.Contains(especie)) throw new NegocioException("La especie no es válida.");

        var raza = Requerido(dto.Raza, "la raza", 80);

        var sexo = string.IsNullOrWhiteSpace(dto.Sexo) ? "Desconocido" : dto.Sexo.Trim();
        if (!Sexos.Contains(sexo)) throw new NegocioException("El sexo no es válido.");

        var tamano = Requerido(dto.Tamano, "el tamaño", 20);
        if (!Tamanos.Contains(tamano)) throw new NegocioException("El tamaño no es válido.");

        var edad = Requerido(dto.EdadAproximada, "la edad", 40);
        var color = Opcional(dto.Color, "el color", 60);
        var descripcion = Requerido(dto.Descripcion, "la descripción", 2000);
        var salud = Opcional(dto.EstadoSalud, "el estado de salud", 1000);

        decimal? peso = null;
        if (!string.IsNullOrWhiteSpace(dto.Peso))
        {
            if (!decimal.TryParse(dto.Peso.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var p)
                || p <= 0 || p > 150)
                throw new NegocioException("El peso debe estar entre 0.1 y 150 kg.");
            peso = Math.Round(p, 2);
        }

        var tags = (dto.Caracteristicas ?? new List<string>())
            .Select(t => t.Replace(',', ' ').Trim())
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (tags.Count > MaxCaracteristicas) throw new NegocioException($"Máximo {MaxCaracteristicas} características.");
        if (tags.Any(t => t.Length > 30)) throw new NegocioException("Cada característica admite hasta 30 caracteres.");

        return new DatosMascota(nombre, especie, raza, sexo, tamano, edad, color, peso, descripcion, salud,
            dto.Vacunado, dto.Esterilizado, tags.Count == 0 ? null : string.Join(", ", tags));
    }

    private static void Aplicar(Mascota m, DatosMascota d)
    {
        m.Nombre = d.Nombre;
        m.Especie = d.Especie;
        m.Raza = d.Raza;
        m.Sexo = d.Sexo;
        m.Tamano = d.Tamano;
        m.EdadAproximada = d.Edad;
        m.Color = d.Color;
        m.Peso = d.Peso;
        m.Descripcion = d.Descripcion;
        m.EstadoSalud = d.EstadoSalud;
        m.Vacunado = d.Vacunado;
        m.Esterilizado = d.Esterilizado;
        m.Comportamiento = d.Comportamiento;
    }

    private static string Requerido(string? valor, string campo, int max)
    {
        var v = valor?.Trim();
        if (string.IsNullOrEmpty(v)) throw new NegocioException($"Ingresa {campo}.");
        if (v.Length > max) throw new NegocioException($"Máximo {max} caracteres en {campo}.");
        return v;
    }

    private static string? Opcional(string? valor, string campo, int max)
    {
        var v = valor?.Trim();
        if (string.IsNullOrEmpty(v)) return null;
        if (v.Length > max) throw new NegocioException($"Máximo {max} caracteres en {campo}.");
        return v;
    }

    private static (int Pagina, int Tamano) Paginar(int pagina, int tamano, int max) =>
        (Math.Max(pagina, 1), Math.Clamp(tamano, 1, max));

    private static PagedResultDto<T> Resultado<T>(List<T> items, int total, int pagina, int tamano) => new()
    {
        Items = items,
        Total = total,
        Pagina = pagina,
        Tamano = tamano,
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamano))
    };

    // ---------- Fotos ----------
    private static async Task<string> ValidarFotoAsync(IFormFile file)
    {
        if (file.Length == 0) throw new NegocioException("Una de las imágenes está vacía.");
        if (file.Length > MaxFotoBytes) throw new NegocioException("Cada imagen debe pesar máximo 5 MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FormatosFoto.TryGetValue(ext, out var firma))
            throw new NegocioException("Las imágenes deben ser JPG o PNG.");

        var cabecera = new byte[firma.Length];
        await using var s = file.OpenReadStream();
        var leidos = await s.ReadAsync(cabecera.AsMemory(0, cabecera.Length));
        if (leidos < cabecera.Length || !cabecera.SequenceEqual(firma))
            throw new NegocioException("Una imagen está dañada o no coincide con su formato.");

        return ext == ".jpeg" ? ".jpg" : ext;
    }

    private async Task<string> GuardarFotoAsync(int idMascota, IFormFile file, string ext)
    {
        var carpeta = Path.Combine(PublicRoot, "mascotas", idMascota.ToString());
        Directory.CreateDirectory(carpeta);

        var nombre = $"{Guid.NewGuid():N}{ext}";
        await using var output = File.Create(Path.Combine(carpeta, nombre));
        await file.CopyToAsync(output);

        return $"mascotas/{idMascota}/{nombre}";
    }

    private void BorrarFoto(string rutaRelativa)
    {
        try
        {
            var raiz = Path.GetFullPath(PublicRoot);
            var ruta = Path.GetFullPath(Path.Combine(raiz, rutaRelativa));
            if (ruta.StartsWith(raiz + Path.DirectorySeparatorChar) && File.Exists(ruta))
                File.Delete(ruta);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo borrar la foto {Ruta}", rutaRelativa);
        }
    }
}