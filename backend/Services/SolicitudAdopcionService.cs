using Adopta.Api.Data;
using Adopta.Api.Data.Entities;
using Adopta.Api.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static Adopta.Api.Services.ServiceHelpers;

namespace Adopta.Api.Services;

public record ArchivoSolicitud(string? Ruta, byte[]? Contenido, string Mime, string Nombre);

public interface ISolicitudAdopcionService
{
    Task<PagedResultDto<SolicitudListaDto>> ListarAsync(int idOrganizacion, SolicitudesQueryDto f);
    Task<List<EstadoSolicitudDto>> EstadosAsync();
    Task<SolicitudDetalleDto> DetalleAsync(int idSolicitud, int idOrganizacion);
    Task<List<SolicitudRequisitoDto>> DocumentosAsync(int idSolicitud, int idOrganizacion);
    Task<ArchivoSolicitud?> ObtenerArchivoAsync(int idArchivo, int idOrganizacion);

    Task SolicitarInformacionAsync(int idSolicitud, int idOrganizacion, int idUsuario, SolicitarInformacionRequestDto dto);
    Task ResolverAsync(int idSolicitud, int idOrganizacion, int idUsuario, ResolverSolicitudRequestDto dto);
}

public class SolicitudAdopcionService : ISolicitudAdopcionService
{
    private const string MascotaDisponible = "Disponible";
    private const string MascotaEnProceso = "En proceso de adopcion";
    private const string MascotaAdoptada = "Adoptada";

    private const string DevolucionAceptada = "Aceptada";

    private const int MaxTexto = 1000;
    private const int MinMotivo = 10;
    private static readonly HashSet<string> NombresAprobada = new() { "aprobada", "aprobado", "aceptada", "aceptado" };
    private static readonly HashSet<string> NombresRechazada = new() { "rechazada", "rechazado" };
    private static readonly HashSet<string> NombresCancelada = new() { "cancelada", "cancelado" };
    private static readonly HashSet<string> NombresDocumentacion = new() { "documentacion pendiente", "informacion pendiente" };

    private readonly AdoptaDbContext _db;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private readonly ILogger<SolicitudAdopcionService> _logger;
    private readonly IHttpClientFactory _http;

    public SolicitudAdopcionService(AdoptaDbContext db, IConfiguration config, IHostEnvironment env,
        ILogger<SolicitudAdopcionService> logger, IHttpClientFactory http)
    {
        _db = db;
        _config = config;
        _env = env;
        _logger = logger;
        _http = http;
    }

    private string StorageRoot => Path.Combine(_env.ContentRootPath, _config["Storage:Root"] ?? "Storage");
    private IQueryable<SolicitudesAdopcion> Solicitudes(int idOrganizacion) =>
        _db.SolicitudesAdopcions.AsNoTracking()
            .Where(s => s.IdMascotaNavigation.IdOrganizacion == idOrganizacion);

    // =================== Listado ===================

    public async Task<PagedResultDto<SolicitudListaDto>> ListarAsync(int idOrganizacion, SolicitudesQueryDto f)
    {
        var q = Solicitudes(idOrganizacion);

        foreach (var palabra in Palabras(f.Buscar))
        {
            var p = palabra;
            q = q.Where(s => s.IdMascotaNavigation.Nombre.Contains(p)
                             || s.IdAdoptanteNavigation.Nombre.Contains(p)
                             || s.IdAdoptanteNavigation.ApellidoPaterno.Contains(p)
                             || (s.IdAdoptanteNavigation.ApellidoMaterno != null && s.IdAdoptanteNavigation.ApellidoMaterno.Contains(p)));
        }

        if (f.IdEstadoSolicitud is int idEstado)
            q = q.Where(s => s.IdEstadoSolicitud == idEstado);

        if (f.Fecha is DateTime fecha)
        {
            var inicio = fecha.Date;
            var finExclusivo = inicio.AddDays(1);
            q = q.Where(s => s.FechaSolicitud >= inicio && s.FechaSolicitud < finExclusivo);
        }

        var (pagina, tamano) = Paginar(f.Pagina, f.Tamano, 50);
        var total = await q.CountAsync();

        var filas = await q
            .OrderByDescending(s => s.FechaSolicitud).ThenByDescending(s => s.IdSolicitud)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(s => new
            {
                s.IdSolicitud,
                s.FechaSolicitud,
                s.IdMascota,
                Mascota = s.IdMascotaNavigation.Nombre,
                s.IdEstadoSolicitud,
                Estado = s.IdEstadoSolicitudNavigation.Nombre,
                AdNombre = s.IdAdoptanteNavigation.Nombre,
                AdPaterno = s.IdAdoptanteNavigation.ApellidoPaterno,
                AdMaterno = s.IdAdoptanteNavigation.ApellidoMaterno,
                AdFoto = s.IdAdoptanteNavigation.FotoPerfil
            })
            .ToListAsync();

        var items = filas.Select(r => new SolicitudListaDto
        {
            IdSolicitud = r.IdSolicitud,
            Adoptante = NombreCompleto(r.AdNombre, r.AdPaterno, r.AdMaterno),
            AdoptanteFotoUrl = PerfilService.UrlFoto(r.AdFoto),
            IdMascota = r.IdMascota,
            Mascota = r.Mascota,
            FechaSolicitud = r.FechaSolicitud,
            IdEstadoSolicitud = r.IdEstadoSolicitud,
            Estado = r.Estado
        }).ToList();

        return Resultado(items, total, pagina, tamano);
    }

    public async Task<List<EstadoSolicitudDto>> EstadosAsync() =>
        await _db.EstadosSolicituds.AsNoTracking()
            .OrderBy(e => e.IdEstadoSolicitud)
            .Select(e => new EstadoSolicitudDto { IdEstadoSolicitud = e.IdEstadoSolicitud, Nombre = e.Nombre })
            .ToListAsync();

    // =================== Detalle ===================
    public async Task<SolicitudDetalleDto> DetalleAsync(int idSolicitud, int idOrganizacion)
    {
        var r = await Solicitudes(idOrganizacion)
            .Where(s => s.IdSolicitud == idSolicitud)
            .Select(s => new
            {
                s.IdSolicitud,
                s.IdEstadoSolicitud,
                Estado = s.IdEstadoSolicitudNavigation.Nombre,
                s.FechaSolicitud,
                s.FechaResolucion,
                s.ObservacionesOrganizacion,
                s.FechaLimiteDocumentos,
                s.DocumentacionCompleta,
                s.MotivoAdopcion,
                s.ExperienciaPrevia,
                s.CondicionesVivienda,

                AdNombre = s.IdAdoptanteNavigation.Nombre,
                AdPaterno = s.IdAdoptanteNavigation.ApellidoPaterno,
                AdMaterno = s.IdAdoptanteNavigation.ApellidoMaterno,
                AdCorreo = s.IdAdoptanteNavigation.Correo,
                AdTelefono = s.IdAdoptanteNavigation.Telefono,
                AdFoto = s.IdAdoptanteNavigation.FotoPerfil,

                s.IdMascota,
                MascotaNombre = s.IdMascotaNavigation.Nombre,
                s.IdMascotaNavigation.Especie,
                s.IdMascotaNavigation.Raza,
                s.IdMascotaNavigation.Tamano,
                s.IdMascotaNavigation.EdadAproximada,
                MascotaFoto = s.IdMascotaNavigation.FotosMascota
                    .OrderByDescending(ft => ft.EsPrincipal).ThenBy(ft => ft.IdFoto)
                    .Select(ft => ft.RutaFoto).FirstOrDefault()
            })
            .FirstOrDefaultAsync()
            ?? throw new NegocioException("Solicitud no encontrada.", StatusCodes.Status404NotFound);

        var hogar = await _db.InformacionComplementariaSolicituds.AsNoTracking()
            .Where(i => i.IdSolicitud == idSolicitud)
            .FirstOrDefaultAsync();

        var referencias = await _db.ReferenciasPersonalesSolicituds.AsNoTracking()
            .Where(x => x.IdSolicitud == idSolicitud)
            .OrderBy(x => x.IdReferencia)
            .ToListAsync();

        var historial = await _db.HistorialSolicituds.AsNoTracking()
            .Where(h => h.IdSolicitud == idSolicitud)
            .OrderBy(h => h.FechaCambio).ThenBy(h => h.IdHistorial)
            .Select(h => new SolicitudHistorialDto
            {
                Estado = h.IdEstadoSolicitudNavigation.Nombre,
                Observacion = h.Observacion,
                Fecha = h.FechaCambio
            })
            .ToListAsync();

        // Solicitudes sin bitácora (creadas antes de llevar historial): al menos el envío
        if (historial.Count == 0)
            historial.Add(new SolicitudHistorialDto { Estado = r.Estado, Fecha = r.FechaSolicitud });

        return new SolicitudDetalleDto
        {
            IdSolicitud = r.IdSolicitud,
            IdEstadoSolicitud = r.IdEstadoSolicitud,
            Estado = r.Estado,
            EsFinal = EsFinal(r.Estado),
            FechaSolicitud = r.FechaSolicitud,
            FechaResolucion = r.FechaResolucion,
            ObservacionesOrganizacion = r.ObservacionesOrganizacion,
            FechaLimiteDocumentos = r.FechaLimiteDocumentos,
            DocumentacionCompleta = r.DocumentacionCompleta,
            MotivoAdopcion = r.MotivoAdopcion,
            ExperienciaPrevia = r.ExperienciaPrevia,
            CondicionesVivienda = r.CondicionesVivienda,
            Adoptante = new SolicitudAdoptanteDto
            {
                Nombre = NombreCompleto(r.AdNombre, r.AdPaterno, r.AdMaterno),
                Correo = r.AdCorreo,
                Telefono = r.AdTelefono,
                FotoUrl = PerfilService.UrlFoto(r.AdFoto)
            },
            Mascota = new SolicitudMascotaDto
            {
                IdMascota = r.IdMascota,
                Nombre = r.MascotaNombre,
                Especie = r.Especie,
                Raza = r.Raza,
                Tamano = r.Tamano,
                EdadAproximada = r.EdadAproximada,
                FotoUrl = PerfilService.UrlFoto(r.MascotaFoto)
            },
            Hogar = hogar is null ? null : new SolicitudHogarDto
            {
                RegimenVivienda = hogar.RegimenVivienda,
                ArrendadorPermiteMascotas = hogar.ArrendadorPermiteMascotas,
                TieneEspacioExterior = hogar.TieneEspacioExterior,
                EspacioExteriorCercado = hogar.EspacioExteriorCercado,
                NumPersonasHogar = hogar.NumPersonasHogar,
                HayMenores = hogar.HayMenores,
                HogarDeAcuerdo = hogar.HogarDeAcuerdo,
                HorasSoloDia = hogar.HorasSoloDia,
                LugarDormir = hogar.LugarDormir,
                CuidadorAlterno = hogar.CuidadorAlterno,
                PresupuestoMensual = hogar.PresupuestoMensual,
                TieneOtrasMascotas = hogar.TieneOtrasMascotas,
                EspeciesOtrasMascotas = hogar.EspeciesOtrasMascotas,
                AceptaVisitaDomiciliaria = hogar.AceptaVisitaDomiciliaria,
                AceptaSeguimiento = hogar.AceptaSeguimiento
            },
            Referencias = referencias.Select(x => new SolicitudReferenciaDto
            {
                Nombre = x.NombreCompleto,
                Telefono = x.Telefono,
                Relacion = x.Relacion,
                AniosConocidos = x.AniosConocidos
            }).ToList(),
            Historial = historial
        };
    }

    // =================== Documentos ===================

    public async Task<List<SolicitudRequisitoDto>> DocumentosAsync(int idSolicitud, int idOrganizacion)
    {
        if (!await Solicitudes(idOrganizacion).AnyAsync(s => s.IdSolicitud == idSolicitud))
            throw new NegocioException("Solicitud no encontrada.", StatusCodes.Status404NotFound);

        var requisitos = await _db.RequisitosSolicituds.AsNoTracking()
            .Where(r => r.IdSolicitud == idSolicitud)
            .OrderBy(r => r.IdRequisito)
            .Select(r => new
            {
                r.IdRequisito,
                Tipo = r.IdTipoDocumentoNavigation.Nombre,
                r.Obligatorio,
                r.Estado,
                r.MotivoRechazo
            })
            .ToListAsync();

        var ids = requisitos.Select(r => r.IdRequisito).ToList();
        var archivos = await _db.ArchivosRequisitos.AsNoTracking()
            .Where(a => ids.Contains(a.IdRequisito) && (a.EsVigente == null || a.EsVigente == true))
            .OrderBy(a => a.IdArchivo)
            .Select(a => new { a.IdRequisito, a.IdArchivo, a.NombreOriginal })
            .ToListAsync();

        return requisitos.Select(r => new SolicitudRequisitoDto
        {
            IdRequisito = r.IdRequisito,
            Tipo = r.Tipo,
            Obligatorio = r.Obligatorio ?? true,
            Estado = r.Estado,
            MotivoRechazo = r.MotivoRechazo,
            Archivos = archivos
                .Where(a => a.IdRequisito == r.IdRequisito)
                .Select(a => new SolicitudArchivoDto { IdArchivo = a.IdArchivo, NombreOriginal = a.NombreOriginal })
                .ToList()
        }).ToList();
    }

    public async Task<ArchivoSolicitud?> ObtenerArchivoAsync(int idArchivo, int idOrganizacion)
    {
        // Solo archivos de solicitudes de mascotas de la organización en sesión
        var a = await _db.ArchivosRequisitos.AsNoTracking()
            .Where(x => x.IdArchivo == idArchivo
                        && x.IdRequisitoNavigation.IdSolicitudNavigation.IdMascotaNavigation.IdOrganizacion == idOrganizacion)
            .Select(x => new { x.RutaRelativa, x.MimeType, x.NombreOriginal,
                IdAdoptante = x.IdRequisitoNavigation.IdSolicitudNavigation.IdAdoptante })
            .FirstOrDefaultAsync();
        if (a is null) return null;

        // Se busca en la carpeta privada de la API y, si existe, en la carpeta donde guarda la app móvil
        var relativa = a.RutaRelativa.TrimStart('/', '\\');
        var intentos = new List<string>();
        foreach (var raiz in RaicesDeArchivos())
        {
            var ruta = Path.GetFullPath(Path.Combine(raiz, relativa));
            intentos.Add(ruta);

            if (!ruta.StartsWith(raiz + Path.DirectorySeparatorChar)) continue;
            if (File.Exists(ruta)) return new ArchivoSolicitud(ruta, null, a.MimeType, a.NombreOriginal);
        }

        var remoto = await DescargarDeMovilAsync(idArchivo, a.IdAdoptante, a.MimeType, a.NombreOriginal);
        if (remoto is not null) return remoto;

        _logger.LogWarning("Archivo {Id} no encontrado en disco ni en la API móvil. Rutas probadas: {Rutas}", idArchivo, string.Join(" | ", intentos));
        return null;
    }

    private async Task<ArchivoSolicitud?> DescargarDeMovilAsync(int idArchivo, int idAdoptante, string mime, string nombre)
    {
        var baseUrl = _config["Mobile:BaseUrl"]?.TrimEnd('/');
        var secret = _config["Mobile:JwtSecret"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(secret))
            throw new NegocioException("Falta configurar Mobile:BaseUrl y Mobile:JwtSecret en la API web.", StatusCodes.Status502BadGateway);

        try
        {
            // La API móvil solo entrega el documento a su dueño: se firma un token corto del adoptante
            // con el mismo secreto JWT que usa la API móvil (sub = id_usuario, rol = Adoptante).
            var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                claims: new[] { new Claim("sub", idAdoptante.ToString()), new Claim("rol", "Adoptante") },
                expires: DateTime.UtcNow.AddMinutes(2),
                signingCredentials: creds);
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            var client = _http.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            using var req = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/archivos/{idArchivo}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var resp = await client.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                var cuerpo = await resp.Content.ReadAsStringAsync();
                _logger.LogWarning("API móvil respondió {Status} para el archivo {Id}: {Cuerpo}", (int)resp.StatusCode, idArchivo, cuerpo);
                throw new NegocioException(
                    $"La API móvil respondió {(int)resp.StatusCode} al pedir el archivo {idArchivo}: {cuerpo}",
                    StatusCodes.Status502BadGateway);
            }
            return new ArchivoSolicitud(null, await resp.Content.ReadAsByteArrayAsync(), mime, nombre);
        }
        catch (NegocioException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo descargar el archivo {Id} de la API móvil", idArchivo);
            throw new NegocioException($"No se pudo conectar con la API móvil ({baseUrl}): {ex.Message}", StatusCodes.Status502BadGateway);
        }
    }

    private IEnumerable<string> RaicesDeArchivos()
    {
        yield return Path.GetFullPath(StorageRoot);

        var movil = _config["Storage:MobileRoot"];
        if (!string.IsNullOrWhiteSpace(movil))
            yield return Path.GetFullPath(movil, _env.ContentRootPath);
    }

    // =================== Solicitar información adicional ===================

    public async Task SolicitarInformacionAsync(int idSolicitud, int idOrganizacion, int idUsuario, SolicitarInformacionRequestDto dto)
    {
        var mensaje = (dto.Mensaje ?? "").Trim();
        if (mensaje.Length < MinMotivo)
            throw new NegocioException($"Escribe el mensaje para el adoptante (mínimo {MinMotivo} caracteres).");
        if (mensaje.Length > MaxTexto)
            throw new NegocioException($"El mensaje no puede exceder {MaxTexto} caracteres.");

        var s = await CargarParaEscrituraAsync(idSolicitud, idOrganizacion);
        if (EsFinal(s.IdEstadoSolicitudNavigation.Nombre))
            throw new NegocioException("Esta solicitud ya fue resuelta; no se puede pedir más información.", StatusCodes.Status409Conflict);

        var estados = await _db.EstadosSolicituds.AsNoTracking().ToListAsync();
        var nuevo = estados.FirstOrDefault(e => NombresDocumentacion.Contains(Normalizar(e.Nombre)))
                    ?? s.IdEstadoSolicitudNavigation;

        await using var tx = await _db.Database.BeginTransactionAsync();

        s.IdEstadoSolicitud = nuevo.IdEstadoSolicitud;
        s.DocumentacionCompleta = false;

        _db.HistorialSolicituds.Add(new HistorialSolicitud
        {
            IdSolicitud = s.IdSolicitud,
            IdEstadoSolicitud = nuevo.IdEstadoSolicitud,
            Observacion = mensaje
        });

        _db.Notificaciones.Add(new Notificacione
        {
            IdUsuario = s.IdAdoptante,
            IdSolicitud = s.IdSolicitud,
            Tipo = "SolicitudInformacion",
            Titulo = "Información adicional solicitada",
            Mensaje = $"{s.IdMascotaNavigation.IdOrganizacionNavigation.Nombre} necesita más información sobre tu solicitud para {s.IdMascotaNavigation.Nombre}: {mensaje}"
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        _logger.LogInformation("Solicitud {Id}: usuario {Usuario} pidió información adicional al adoptante", idSolicitud, idUsuario);
    }

    public async Task ResolverAsync(int idSolicitud, int idOrganizacion, int idUsuario, ResolverSolicitudRequestDto dto)
    {
        var decision = Normalizar(dto.Decision);
        var acepta = NombresAprobada.Contains(decision);
        var rechaza = NombresRechazada.Contains(decision);
        if (!acepta && !rechaza)
            throw new NegocioException("El dictamen seleccionado no es válido.");

        var observaciones = Opcional(dto.Observaciones, "las observaciones", MaxTexto);
        if (rechaza && (observaciones is null || observaciones.Length < MinMotivo))
            throw new NegocioException($"Explica al adoptante el motivo del rechazo (mínimo {MinMotivo} caracteres).");

        var s = await CargarParaEscrituraAsync(idSolicitud, idOrganizacion);
        if (EsFinal(s.IdEstadoSolicitudNavigation.Nombre))
            throw new NegocioException("Esta solicitud ya fue resuelta.", StatusCodes.Status409Conflict);

        var mascota = s.IdMascotaNavigation;
        var estados = await _db.EstadosSolicituds.AsNoTracking().ToListAsync();
        var nuevo = estados.FirstOrDefault(e => acepta ? NombresAprobada.Contains(Normalizar(e.Nombre)) : NombresRechazada.Contains(Normalizar(e.Nombre)))
                    ?? throw new NegocioException(
                        $"Falta el estado \"{(acepta ? "Aprobada" : "Rechazada")}\" en el catálogo de estados de solicitud.",
                        StatusCodes.Status500InternalServerError);

        // Las demás solicitudes de la misma mascota (para cerrarlas o liberar a la mascota)
        var otras = await _db.SolicitudesAdopcions
            .Include(x => x.IdEstadoSolicitudNavigation)
            .Where(x => x.IdMascota == s.IdMascota && x.IdSolicitud != s.IdSolicitud)
            .ToListAsync();
        var abiertas = otras.Where(x => !EsFinal(x.IdEstadoSolicitudNavigation.Nombre)).ToList();

        var ahora = DateTime.Now;
        await using var tx = await _db.Database.BeginTransactionAsync();

        if (acepta)
        {
            // Una mascota no puede tener dos adopciones vigentes (si la anterior se devolvió, puede adoptarse otra vez)
            var yaAdoptada = await _db.SolicitudesAdopcions.AnyAsync(x =>
                x.IdMascota == s.IdMascota
                && x.IdSolicitud != s.IdSolicitud
                && x.Adopcione != null
                && (x.Adopcione.SolicitudesDevolucion == null || x.Adopcione.SolicitudesDevolucion.Estado != DevolucionAceptada));
            if (yaAdoptada)
                throw new NegocioException("Esta mascota ya fue adoptada mediante otra solicitud.", StatusCodes.Status409Conflict);

            var idAdoptada = await IdEstadoMascotaAsync(MascotaAdoptada);

            Cerrar(s, nuevo, observaciones, ahora);
            mascota.IdEstadoMascota = idAdoptada;

            _db.Adopciones.Add(new Adopcione
            {
                IdSolicitud = s.IdSolicitud,
                FechaAdopcion = ahora.Date,
                Observaciones = observaciones
            });

            Notificar(s, "SolicitudAceptada", "Solicitud de adopción aceptada",
                $"¡Buenas noticias! {s.IdMascotaNavigation.IdOrganizacionNavigation.Nombre} aceptó tu solicitud para adoptar a {mascota.Nombre}."
                + (observaciones is null ? "" : $" Observaciones: {observaciones}"));

            // Quienes también la pedían ya no pueden adoptarla
            var rechazo = estados.FirstOrDefault(e => NombresRechazada.Contains(Normalizar(e.Nombre)));
            if (rechazo is not null)
            {
                foreach (var otra in abiertas)
                {
                    const string motivo = "La mascota fue adoptada por otro solicitante.";
                    Cerrar(otra, rechazo, motivo, ahora);
                    Notificar(otra, "SolicitudRechazada", "Solicitud de adopción rechazada",
                        $"Tu solicitud para adoptar a {mascota.Nombre} fue cerrada: {motivo}");
                }
            }
        }
        else
        {
            Cerrar(s, nuevo, observaciones, ahora);

            Notificar(s, "SolicitudRechazada", "Solicitud de adopción rechazada",
                $"{s.IdMascotaNavigation.IdOrganizacionNavigation.Nombre} no aceptó tu solicitud para adoptar a {mascota.Nombre}. Motivo: {observaciones}");

            // Si nadie más la está solicitando, la mascota vuelve a estar disponible
            if (abiertas.Count == 0
                && Normalizar(mascota.IdEstadoMascotaNavigation.Nombre) == Normalizar(MascotaEnProceso))
            {
                mascota.IdEstadoMascota = await IdEstadoMascotaAsync(MascotaDisponible);
            }
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        _logger.LogInformation("Solicitud {Id} {Resultado} por usuario {Usuario} (mascota {Mascota})",
            idSolicitud, acepta ? "aceptada" : "rechazada", idUsuario, mascota.IdMascota);
    }

    // =================== Helpers ===================
    private async Task<SolicitudesAdopcion> CargarParaEscrituraAsync(int idSolicitud, int idOrganizacion) =>
        await _db.SolicitudesAdopcions
            .Include(x => x.IdEstadoSolicitudNavigation)
            .Include(x => x.IdMascotaNavigation).ThenInclude(m => m.IdOrganizacionNavigation)
            .Include(x => x.IdMascotaNavigation).ThenInclude(m => m.IdEstadoMascotaNavigation)
            .FirstOrDefaultAsync(x => x.IdSolicitud == idSolicitud && x.IdMascotaNavigation.IdOrganizacion == idOrganizacion)
        ?? throw new NegocioException("Solicitud no encontrada.", StatusCodes.Status404NotFound);

    private void Cerrar(SolicitudesAdopcion s, EstadosSolicitud estado, string? observaciones, DateTime fecha)
    {
        s.IdEstadoSolicitud = estado.IdEstadoSolicitud;
        s.FechaResolucion = fecha;
        s.ObservacionesOrganizacion = observaciones;

        _db.HistorialSolicituds.Add(new HistorialSolicitud
        {
            IdSolicitud = s.IdSolicitud,
            IdEstadoSolicitud = estado.IdEstadoSolicitud,
            Observacion = observaciones
        });
    }

    private void Notificar(SolicitudesAdopcion s, string tipo, string titulo, string mensaje) =>
        _db.Notificaciones.Add(new Notificacione
        {
            IdUsuario = s.IdAdoptante,
            IdSolicitud = s.IdSolicitud,
            Tipo = tipo,
            Titulo = titulo,
            Mensaje = mensaje
        });

    private async Task<int> IdEstadoMascotaAsync(string nombre)
    {
        var estados = await _db.EstadosMascota.AsNoTracking().ToListAsync();
        return estados.FirstOrDefault(e => Normalizar(e.Nombre) == Normalizar(nombre))?.IdEstadoMascota
               ?? throw new NegocioException(
                   $"Falta el estado \"{nombre}\" en el catálogo de estados de mascota.",
                   StatusCodes.Status500InternalServerError);
    }

    private static bool EsFinal(string estado)
    {
        var n = Normalizar(estado);
        return NombresAprobada.Contains(n) || NombresRechazada.Contains(n) || NombresCancelada.Contains(n);
    }
}