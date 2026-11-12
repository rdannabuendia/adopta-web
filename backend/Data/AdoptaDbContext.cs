using System;
using System.Collections.Generic;
using Adopta.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Api.Data;

public partial class AdoptaDbContext : DbContext
{
    public AdoptaDbContext(DbContextOptions<AdoptaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Adopcione> Adopciones { get; set; }

    public virtual DbSet<ArchivosRequisito> ArchivosRequisitos { get; set; }

    public virtual DbSet<Comentario> Comentarios { get; set; }

    public virtual DbSet<DetalleMotivosDevolucion> DetalleMotivosDevolucions { get; set; }

    public virtual DbSet<EstadosAdaptacion> EstadosAdaptacions { get; set; }

    public virtual DbSet<EstadosMascotum> EstadosMascota { get; set; }

    public virtual DbSet<EstadosSolicitud> EstadosSolicituds { get; set; }

    public virtual DbSet<Favorito> Favoritos { get; set; }

    public virtual DbSet<FotosMascotum> FotosMascota { get; set; }

    public virtual DbSet<FotosSeguimiento> FotosSeguimientos { get; set; }

    public virtual DbSet<HistorialAdaptacion> HistorialAdaptacions { get; set; }

    public virtual DbSet<HistorialSolicitud> HistorialSolicituds { get; set; }

    public virtual DbSet<InformacionComplementariaSolicitud> InformacionComplementariaSolicituds { get; set; }

    public virtual DbSet<LikesPublicacion> LikesPublicacions { get; set; }

    public virtual DbSet<Mascota> Mascotas { get; set; }

    public virtual DbSet<MotivosDevolucion> MotivosDevolucions { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<Organizacione> Organizaciones { get; set; }

    public virtual DbSet<PerfilesAdoptante> PerfilesAdoptantes { get; set; }

    public virtual DbSet<PreferenciasUsuario> PreferenciasUsuarios { get; set; }

    public virtual DbSet<Publicacione> Publicaciones { get; set; }

    public virtual DbSet<ReferenciasPersonalesSolicitud> ReferenciasPersonalesSolicituds { get; set; }

    public virtual DbSet<ReportesSeguimiento> ReportesSeguimientos { get; set; }

    public virtual DbSet<RequisitosSolicitud> RequisitosSolicituds { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SolicitudesAdopcion> SolicitudesAdopcions { get; set; }

    public virtual DbSet<SolicitudesDevolucion> SolicitudesDevolucions { get; set; }

    public virtual DbSet<TiposDocumento> TiposDocumentos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VistaCatalogoMascota> VistaCatalogoMascotas { get; set; }

    public virtual DbSet<VistaReportesSeguimiento> VistaReportesSeguimientos { get; set; }

    public virtual DbSet<VistaSolicitudesAdopcion> VistaSolicitudesAdopcions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Adopcione>(entity =>
        {
            entity.HasKey(e => e.IdAdopcion).HasName("PRIMARY");

            entity.ToTable("adopciones");

            entity.HasIndex(e => e.IdSolicitud, "id_solicitud").IsUnique();

            entity.Property(e => e.IdAdopcion).HasColumnName("id_adopcion");
            entity.Property(e => e.ContratoUrl)
                .HasMaxLength(255)
                .HasColumnName("contrato_url");
            entity.Property(e => e.FechaAdopcion)
                .HasColumnType("date")
                .HasColumnName("fecha_adopcion");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");

            entity.HasOne(d => d.IdSolicitudNavigation).WithOne(p => p.Adopcione)
                .HasForeignKey<Adopcione>(d => d.IdSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_adopcion_solicitud");
        });

        modelBuilder.Entity<ArchivosRequisito>(entity =>
        {
            entity.HasKey(e => e.IdArchivo).HasName("PRIMARY");

            entity.ToTable("archivos_requisito");

            entity.HasIndex(e => e.IdUsuarioCarga, "fk_archivo_usuario");

            entity.HasIndex(e => new { e.IdRequisito, e.EsVigente }, "idx_archivos_requisito_vigente");

            entity.HasIndex(e => e.NombreAlmacenado, "nombre_almacenado").IsUnique();

            entity.Property(e => e.IdArchivo).HasColumnName("id_archivo");
            entity.Property(e => e.EsVigente)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("es_vigente");
            entity.Property(e => e.Extension)
                .HasMaxLength(10)
                .HasColumnName("extension");
            entity.Property(e => e.FechaCarga)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_carga");
            entity.Property(e => e.FechaEmisionDocumento)
                .HasColumnType("date")
                .HasColumnName("fecha_emision_documento");
            entity.Property(e => e.HashSha256)
                .HasMaxLength(64)
                .IsFixedLength()
                .HasColumnName("hash_sha256");
            entity.Property(e => e.IdRequisito).HasColumnName("id_requisito");
            entity.Property(e => e.IdUsuarioCarga).HasColumnName("id_usuario_carga");
            entity.Property(e => e.MimeType)
                .HasMaxLength(100)
                .HasColumnName("mime_type");
            entity.Property(e => e.NombreAlmacenado)
                .HasMaxLength(80)
                .HasColumnName("nombre_almacenado");
            entity.Property(e => e.NombreOriginal)
                .HasMaxLength(255)
                .HasColumnName("nombre_original");
            entity.Property(e => e.Origen)
                .HasColumnType("enum('Web','Movil')")
                .HasColumnName("origen");
            entity.Property(e => e.RutaRelativa)
                .HasMaxLength(255)
                .HasColumnName("ruta_relativa");
            entity.Property(e => e.TamanoBytes).HasColumnName("tamano_bytes");

            entity.HasOne(d => d.IdRequisitoNavigation).WithMany(p => p.ArchivosRequisitos)
                .HasForeignKey(d => d.IdRequisito)
                .HasConstraintName("fk_archivo_requisito");

            entity.HasOne(d => d.IdUsuarioCargaNavigation).WithMany(p => p.ArchivosRequisitos)
                .HasForeignKey(d => d.IdUsuarioCarga)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_archivo_usuario");
        });

        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.HasKey(e => e.IdComentario).HasName("PRIMARY");

            entity.ToTable("comentarios");

            entity.HasIndex(e => e.IdUsuario, "fk_comentario_usuario");

            entity.HasIndex(e => new { e.IdPublicacion, e.FechaComentario }, "idx_comentarios_publicacion_fecha");

            entity.Property(e => e.IdComentario).HasColumnName("id_comentario");
            entity.Property(e => e.Contenido)
                .HasColumnType("text")
                .HasColumnName("contenido");
            entity.Property(e => e.FechaComentario)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_comentario");
            entity.Property(e => e.IdPublicacion).HasColumnName("id_publicacion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdPublicacionNavigation).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.IdPublicacion)
                .HasConstraintName("fk_comentario_publicacion");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_comentario_usuario");
        });

        modelBuilder.Entity<DetalleMotivosDevolucion>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("PRIMARY");

            entity.ToTable("detalle_motivos_devolucion");

            entity.HasIndex(e => e.IdMotivo, "fk_detalle_motivo");

            entity.HasIndex(e => new { e.IdDevolucion, e.IdMotivo }, "uq_devolucion_motivo").IsUnique();

            entity.Property(e => e.IdDetalle).HasColumnName("id_detalle");
            entity.Property(e => e.IdDevolucion).HasColumnName("id_devolucion");
            entity.Property(e => e.IdMotivo).HasColumnName("id_motivo");

            entity.HasOne(d => d.IdDevolucionNavigation).WithMany(p => p.DetalleMotivosDevolucions)
                .HasForeignKey(d => d.IdDevolucion)
                .HasConstraintName("fk_detalle_devolucion");

            entity.HasOne(d => d.IdMotivoNavigation).WithMany(p => p.DetalleMotivosDevolucions)
                .HasForeignKey(d => d.IdMotivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_detalle_motivo");
        });

        modelBuilder.Entity<EstadosAdaptacion>(entity =>
        {
            entity.HasKey(e => e.IdEstadoAdaptacion).HasName("PRIMARY");

            entity.ToTable("estados_adaptacion");

            entity.HasIndex(e => e.Nombre, "nombre").IsUnique();

            entity.Property(e => e.IdEstadoAdaptacion).HasColumnName("id_estado_adaptacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(40)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<EstadosMascotum>(entity =>
        {
            entity.HasKey(e => e.IdEstadoMascota).HasName("PRIMARY");

            entity.ToTable("estados_mascota");

            entity.HasIndex(e => e.Nombre, "nombre").IsUnique();

            entity.Property(e => e.IdEstadoMascota).HasColumnName("id_estado_mascota");
            entity.Property(e => e.Nombre)
                .HasMaxLength(40)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<EstadosSolicitud>(entity =>
        {
            entity.HasKey(e => e.IdEstadoSolicitud).HasName("PRIMARY");

            entity.ToTable("estados_solicitud");

            entity.HasIndex(e => e.Nombre, "nombre").IsUnique();

            entity.Property(e => e.IdEstadoSolicitud).HasColumnName("id_estado_solicitud");
            entity.Property(e => e.Nombre)
                .HasMaxLength(40)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Favorito>(entity =>
        {
            entity.HasKey(e => e.IdFavorito).HasName("PRIMARY");

            entity.ToTable("favoritos");

            entity.HasIndex(e => e.IdMascota, "fk_favorito_mascota");

            entity.HasIndex(e => new { e.IdUsuario, e.IdMascota }, "uq_favorito_usuario_mascota").IsUnique();

            entity.Property(e => e.IdFavorito).HasColumnName("id_favorito");
            entity.Property(e => e.FechaAgregado)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_agregado");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdMascotaNavigation).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.IdMascota)
                .HasConstraintName("fk_favorito_mascota");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_favorito_usuario");
        });

        modelBuilder.Entity<FotosMascotum>(entity =>
        {
            entity.HasKey(e => e.IdFoto).HasName("PRIMARY");

            entity.ToTable("fotos_mascota");

            entity.HasIndex(e => e.IdMascota, "idx_fotos_mascota");

            entity.Property(e => e.IdFoto).HasColumnName("id_foto");
            entity.Property(e => e.EsPrincipal).HasColumnName("es_principal");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.RutaFoto)
                .HasMaxLength(255)
                .HasColumnName("ruta_foto");

            entity.HasOne(d => d.IdMascotaNavigation).WithMany(p => p.FotosMascota)
                .HasForeignKey(d => d.IdMascota)
                .HasConstraintName("fk_foto_mascota");
        });

        modelBuilder.Entity<FotosSeguimiento>(entity =>
        {
            entity.HasKey(e => e.IdFotoSeguimiento).HasName("PRIMARY");

            entity.ToTable("fotos_seguimiento");

            entity.HasIndex(e => e.IdReporte, "fk_foto_seguimiento");

            entity.Property(e => e.IdFotoSeguimiento).HasColumnName("id_foto_seguimiento");
            entity.Property(e => e.IdReporte).HasColumnName("id_reporte");
            entity.Property(e => e.RutaFoto)
                .HasMaxLength(255)
                .HasColumnName("ruta_foto");

            entity.HasOne(d => d.IdReporteNavigation).WithMany(p => p.FotosSeguimientos)
                .HasForeignKey(d => d.IdReporte)
                .HasConstraintName("fk_foto_seguimiento");
        });

        modelBuilder.Entity<HistorialAdaptacion>(entity =>
        {
            entity.HasKey(e => e.IdHistorialAdaptacion).HasName("PRIMARY");

            entity.ToTable("historial_adaptacion");

            entity.HasIndex(e => e.IdEstadoAdaptacion, "fk_hist_adapt_estado");

            entity.HasIndex(e => e.IdUsuarioResponsable, "fk_hist_adapt_usuario");

            entity.HasIndex(e => new { e.IdMascota, e.FechaCambio }, "idx_hist_adapt_mascota_fecha");

            entity.Property(e => e.IdHistorialAdaptacion).HasColumnName("id_historial_adaptacion");
            entity.Property(e => e.FechaCambio)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_cambio");
            entity.Property(e => e.IdEstadoAdaptacion).HasColumnName("id_estado_adaptacion");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.IdUsuarioResponsable).HasColumnName("id_usuario_responsable");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");

            entity.HasOne(d => d.IdEstadoAdaptacionNavigation).WithMany(p => p.HistorialAdaptacions)
                .HasForeignKey(d => d.IdEstadoAdaptacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_hist_adapt_estado");

            entity.HasOne(d => d.IdMascotaNavigation).WithMany(p => p.HistorialAdaptacions)
                .HasForeignKey(d => d.IdMascota)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_hist_adapt_mascota");

            entity.HasOne(d => d.IdUsuarioResponsableNavigation).WithMany(p => p.HistorialAdaptacions)
                .HasForeignKey(d => d.IdUsuarioResponsable)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_hist_adapt_usuario");
        });

        modelBuilder.Entity<HistorialSolicitud>(entity =>
        {
            entity.HasKey(e => e.IdHistorial).HasName("PRIMARY");

            entity.ToTable("historial_solicitud");

            entity.HasIndex(e => e.IdEstadoSolicitud, "fk_historial_estado");

            entity.HasIndex(e => new { e.IdSolicitud, e.FechaCambio }, "idx_historial_solicitud");

            entity.Property(e => e.IdHistorial).HasColumnName("id_historial");
            entity.Property(e => e.FechaCambio)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_cambio");
            entity.Property(e => e.IdEstadoSolicitud).HasColumnName("id_estado_solicitud");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.Observacion)
                .HasColumnType("text")
                .HasColumnName("observacion");

            entity.HasOne(d => d.IdEstadoSolicitudNavigation).WithMany(p => p.HistorialSolicituds)
                .HasForeignKey(d => d.IdEstadoSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_historial_estado");

            entity.HasOne(d => d.IdSolicitudNavigation).WithMany(p => p.HistorialSolicituds)
                .HasForeignKey(d => d.IdSolicitud)
                .HasConstraintName("fk_historial_solicitud");
        });

        modelBuilder.Entity<InformacionComplementariaSolicitud>(entity =>
        {
            entity.HasKey(e => e.IdInfo).HasName("PRIMARY");

            entity.ToTable("informacion_complementaria_solicitud");

            entity.HasIndex(e => e.IdSolicitud, "id_solicitud").IsUnique();

            entity.Property(e => e.IdInfo).HasColumnName("id_info");
            entity.Property(e => e.AceptaAvisoPrivacidad).HasColumnName("acepta_aviso_privacidad");
            entity.Property(e => e.AceptaSeguimiento).HasColumnName("acepta_seguimiento");
            entity.Property(e => e.AceptaVisitaDomiciliaria).HasColumnName("acepta_visita_domiciliaria");
            entity.Property(e => e.ArrendadorPermiteMascotas).HasColumnName("arrendador_permite_mascotas");
            entity.Property(e => e.CantidadOtrasMascotas).HasColumnName("cantidad_otras_mascotas");
            entity.Property(e => e.CompromisoEsterilizacion).HasColumnName("compromiso_esterilizacion");
            entity.Property(e => e.CuidadorAlterno)
                .HasMaxLength(120)
                .HasColumnName("cuidador_alterno");
            entity.Property(e => e.DeclaracionVeracidad).HasColumnName("declaracion_veracidad");
            entity.Property(e => e.EspacioExteriorCercado).HasColumnName("espacio_exterior_cercado");
            entity.Property(e => e.EspeciesOtrasMascotas)
                .HasMaxLength(120)
                .HasColumnName("especies_otras_mascotas");
            entity.Property(e => e.EstadoFormulario)
                .HasDefaultValueSql("'Borrador'")
                .HasColumnType("enum('Borrador','Enviado')")
                .HasColumnName("estado_formulario");
            entity.Property(e => e.FechaActualizacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFirma)
                .HasColumnType("datetime")
                .HasColumnName("fecha_firma");
            entity.Property(e => e.FirmaNombre)
                .HasMaxLength(160)
                .HasColumnName("firma_nombre");
            entity.Property(e => e.HayMenores).HasColumnName("hay_menores");
            entity.Property(e => e.HistorialMascotasPrevias)
                .HasColumnType("text")
                .HasColumnName("historial_mascotas_previas");
            entity.Property(e => e.HogarDeAcuerdo).HasColumnName("hogar_de_acuerdo");
            entity.Property(e => e.HorasSoloDia)
                .HasColumnType("enum('Menos de 4 horas','De 4 a 8 horas','Mas de 8 horas')")
                .HasColumnName("horas_solo_dia");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.LugarDormir)
                .HasColumnType("enum('Dentro de casa','Patio o exterior','Area techada exterior','Otro')")
                .HasColumnName("lugar_dormir");
            entity.Property(e => e.NumPersonasHogar).HasColumnName("num_personas_hogar");
            entity.Property(e => e.OtrasEsterilizadas).HasColumnName("otras_esterilizadas");
            entity.Property(e => e.OtrasVacunadas).HasColumnName("otras_vacunadas");
            entity.Property(e => e.PresupuestoMensual)
                .HasColumnType("enum('Menos de $500','$500 a $1,000','$1,001 a $2,000','Mas de $2,000')")
                .HasColumnName("presupuesto_mensual");
            entity.Property(e => e.RegimenVivienda)
                .HasColumnType("enum('Propia','Rentada','Familiar','Otro')")
                .HasColumnName("regimen_vivienda");
            entity.Property(e => e.SuperficieM2).HasColumnName("superficie_m2");
            entity.Property(e => e.TieneEspacioExterior).HasColumnName("tiene_espacio_exterior");
            entity.Property(e => e.TieneOtrasMascotas).HasColumnName("tiene_otras_mascotas");
            entity.Property(e => e.VeterinarioNombre)
                .HasMaxLength(120)
                .HasColumnName("veterinario_nombre");
            entity.Property(e => e.VeterinarioTelefono)
                .HasMaxLength(20)
                .HasColumnName("veterinario_telefono");

            entity.HasOne(d => d.IdSolicitudNavigation).WithOne(p => p.InformacionComplementariaSolicitud)
                .HasForeignKey<InformacionComplementariaSolicitud>(d => d.IdSolicitud)
                .HasConstraintName("fk_info_solicitud");
        });

        modelBuilder.Entity<LikesPublicacion>(entity =>
        {
            entity.HasKey(e => e.IdLike).HasName("PRIMARY");

            entity.ToTable("likes_publicacion");

            entity.HasIndex(e => e.IdUsuario, "fk_like_usuario");

            entity.HasIndex(e => new { e.IdPublicacion, e.IdUsuario }, "uq_like_publicacion_usuario").IsUnique();

            entity.Property(e => e.IdLike).HasColumnName("id_like");
            entity.Property(e => e.FechaLike)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_like");
            entity.Property(e => e.IdPublicacion).HasColumnName("id_publicacion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdPublicacionNavigation).WithMany(p => p.LikesPublicacions)
                .HasForeignKey(d => d.IdPublicacion)
                .HasConstraintName("fk_like_publicacion");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.LikesPublicacions)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_like_usuario");
        });

        modelBuilder.Entity<Mascota>(entity =>
        {
            entity.HasKey(e => e.IdMascota).HasName("PRIMARY");

            entity.ToTable("mascotas");

            entity.HasIndex(e => e.IdEstadoAdaptacion, "fk_mascota_adaptacion");

            entity.HasIndex(e => new { e.IdEstadoMascota, e.Especie, e.Tamano }, "idx_mascotas_catalogo");

            entity.HasIndex(e => e.IdOrganizacion, "idx_mascotas_organizacion");

            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.Color)
                .HasMaxLength(60)
                .HasColumnName("color");
            entity.Property(e => e.Comportamiento)
                .HasColumnType("text")
                .HasColumnName("comportamiento");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.EdadAproximada)
                .HasMaxLength(40)
                .HasColumnName("edad_aproximada");
            entity.Property(e => e.Especie)
                .HasMaxLength(50)
                .HasColumnName("especie");
            entity.Property(e => e.EstadoSalud)
                .HasColumnType("text")
                .HasColumnName("estado_salud");
            entity.Property(e => e.Esterilizado).HasColumnName("esterilizado");
            entity.Property(e => e.FechaNacimiento)
                .HasColumnType("date")
                .HasColumnName("fecha_nacimiento");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdEstadoAdaptacion)
                .HasDefaultValueSql("'1'")
                .HasColumnName("id_estado_adaptacion");
            entity.Property(e => e.IdEstadoMascota).HasColumnName("id_estado_mascota");
            entity.Property(e => e.IdOrganizacion).HasColumnName("id_organizacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
            entity.Property(e => e.Peso)
                .HasPrecision(6)
                .HasColumnName("peso");
            entity.Property(e => e.Raza)
                .HasMaxLength(80)
                .HasColumnName("raza");
            entity.Property(e => e.Sexo)
                .HasDefaultValueSql("'Desconocido'")
                .HasColumnType("enum('Macho','Hembra','Desconocido')")
                .HasColumnName("sexo");
            entity.Property(e => e.Tamano)
                .HasColumnType("enum('Pequeno','Mediano','Grande')")
                .HasColumnName("tamano");
            entity.Property(e => e.Vacunado).HasColumnName("vacunado");

            entity.HasOne(d => d.IdEstadoAdaptacionNavigation).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.IdEstadoAdaptacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_mascota_adaptacion");

            entity.HasOne(d => d.IdEstadoMascotaNavigation).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.IdEstadoMascota)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_mascota_estado");

            entity.HasOne(d => d.IdOrganizacionNavigation).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.IdOrganizacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_mascota_organizacion");
        });

        modelBuilder.Entity<MotivosDevolucion>(entity =>
        {
            entity.HasKey(e => e.IdMotivo).HasName("PRIMARY");

            entity.ToTable("motivos_devolucion");

            entity.HasIndex(e => e.Nombre, "nombre").IsUnique();

            entity.Property(e => e.IdMotivo).HasColumnName("id_motivo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Notificacione>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion).HasName("PRIMARY");

            entity.ToTable("notificaciones");

            entity.HasIndex(e => e.IdSolicitud, "fk_notificacion_solicitud");

            entity.HasIndex(e => new { e.IdUsuario, e.Leida, e.FechaEnvio }, "idx_notificaciones_usuario_fecha");

            entity.Property(e => e.IdNotificacion).HasColumnName("id_notificacion");
            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Leida).HasColumnName("leida");
            entity.Property(e => e.Mensaje)
                .HasColumnType("text")
                .HasColumnName("mensaje");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .HasColumnName("tipo");
            entity.Property(e => e.Titulo)
                .HasMaxLength(120)
                .HasColumnName("titulo");

            entity.HasOne(d => d.IdSolicitudNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdSolicitud)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_notificacion_solicitud");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_notificacion_usuario");
        });

        modelBuilder.Entity<Organizacione>(entity =>
        {
            entity.HasKey(e => e.IdOrganizacion).HasName("PRIMARY");

            entity.ToTable("organizaciones");

            entity.HasIndex(e => e.IdUsuario, "id_usuario").IsUnique();

            entity.Property(e => e.IdOrganizacion).HasColumnName("id_organizacion");
            entity.Property(e => e.Ciudad)
                .HasMaxLength(80)
                .HasColumnName("ciudad");
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(10)
                .HasColumnName("codigo_postal");
            entity.Property(e => e.CorreoContacto)
                .HasMaxLength(120)
                .HasColumnName("correo_contacto");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .HasColumnName("direccion");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'Pendiente'")
                .HasColumnType("enum('Pendiente','Activa','Inactiva')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Nombre)
                .HasMaxLength(120)
                .HasColumnName("nombre");
            entity.Property(e => e.SitioWeb)
                .HasMaxLength(255)
                .HasColumnName("sitio_web");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Organizacione)
                .HasForeignKey<Organizacione>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_organizacion_usuario");
        });

        modelBuilder.Entity<PerfilesAdoptante>(entity =>
        {
            entity.HasKey(e => e.IdPerfil).HasName("PRIMARY");

            entity.ToTable("perfiles_adoptantes");

            entity.HasIndex(e => e.IdUsuario, "id_usuario").IsUnique();

            entity.Property(e => e.IdPerfil).HasColumnName("id_perfil");
            entity.Property(e => e.Ciudad)
                .HasMaxLength(80)
                .HasColumnName("ciudad");
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(10)
                .HasColumnName("codigo_postal");
            entity.Property(e => e.ContactoEmergenciaNombre)
                .HasMaxLength(120)
                .HasColumnName("contacto_emergencia_nombre");
            entity.Property(e => e.ContactoEmergenciaParentesco)
                .HasMaxLength(40)
                .HasColumnName("contacto_emergencia_parentesco");
            entity.Property(e => e.ContactoEmergenciaTelefono)
                .HasMaxLength(20)
                .HasColumnName("contacto_emergencia_telefono");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .HasColumnName("direccion");
            entity.Property(e => e.ExperienciaMascotas)
                .HasColumnType("text")
                .HasColumnName("experiencia_mascotas");
            entity.Property(e => e.FechaNacimiento)
                .HasColumnType("date")
                .HasColumnName("fecha_nacimiento");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Ocupacion)
                .HasMaxLength(80)
                .HasColumnName("ocupacion");
            entity.Property(e => e.TieneMascotas).HasColumnName("tiene_mascotas");
            entity.Property(e => e.TipoVivienda)
                .HasMaxLength(50)
                .HasColumnName("tipo_vivienda");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.PerfilesAdoptante)
                .HasForeignKey<PerfilesAdoptante>(d => d.IdUsuario)
                .HasConstraintName("fk_perfil_usuario");
        });

        modelBuilder.Entity<PreferenciasUsuario>(entity =>
        {
            entity.HasKey(e => e.IdPreferencia).HasName("PRIMARY");

            entity.ToTable("preferencias_usuario");

            entity.HasIndex(e => e.IdUsuario, "idx_preferencias_usuario");

            entity.Property(e => e.IdPreferencia).HasColumnName("id_preferencia");
            entity.Property(e => e.EdadPreferida)
                .HasMaxLength(30)
                .HasColumnName("edad_preferida");
            entity.Property(e => e.Especie)
                .HasMaxLength(50)
                .HasColumnName("especie");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Tamano)
                .HasMaxLength(30)
                .HasColumnName("tamano");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.PreferenciasUsuarios)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_preferencias_usuario");
        });

        modelBuilder.Entity<Publicacione>(entity =>
        {
            entity.HasKey(e => e.IdPublicacion).HasName("PRIMARY");

            entity.ToTable("publicaciones");

            entity.HasIndex(e => e.IdUsuario, "fk_publicacion_usuario");

            entity.HasIndex(e => e.FechaPublicacion, "idx_publicaciones_fecha");

            entity.Property(e => e.IdPublicacion).HasColumnName("id_publicacion");
            entity.Property(e => e.Contenido)
                .HasColumnType("text")
                .HasColumnName("contenido");
            entity.Property(e => e.FechaActualizacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaPublicacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_publicacion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Imagen)
                .HasMaxLength(255)
                .HasColumnName("imagen");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Publicaciones)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_publicacion_usuario");
        });

        modelBuilder.Entity<ReferenciasPersonalesSolicitud>(entity =>
        {
            entity.HasKey(e => e.IdReferencia).HasName("PRIMARY");

            entity.ToTable("referencias_personales_solicitud");

            entity.HasIndex(e => e.IdSolicitud, "idx_referencias_solicitud");

            entity.Property(e => e.IdReferencia).HasColumnName("id_referencia");
            entity.Property(e => e.AniosConocidos).HasColumnName("anios_conocidos");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(120)
                .HasColumnName("nombre_completo");
            entity.Property(e => e.Relacion)
                .HasMaxLength(60)
                .HasColumnName("relacion");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdSolicitudNavigation).WithMany(p => p.ReferenciasPersonalesSolicituds)
                .HasForeignKey(d => d.IdSolicitud)
                .HasConstraintName("fk_referencia_solicitud");
        });

        modelBuilder.Entity<ReportesSeguimiento>(entity =>
        {
            entity.HasKey(e => e.IdReporte).HasName("PRIMARY");

            entity.ToTable("reportes_seguimiento");

            entity.HasIndex(e => new { e.IdAdopcion, e.FechaReporte }, "idx_reportes_adopcion_fecha");

            entity.Property(e => e.IdReporte).HasColumnName("id_reporte");
            entity.Property(e => e.Adaptacion)
                .HasColumnType("text")
                .HasColumnName("adaptacion");
            entity.Property(e => e.Alimentacion)
                .HasColumnType("text")
                .HasColumnName("alimentacion");
            entity.Property(e => e.Comportamiento)
                .HasColumnType("text")
                .HasColumnName("comportamiento");
            entity.Property(e => e.FechaReporte)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_reporte");
            entity.Property(e => e.IdAdopcion).HasColumnName("id_adopcion");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.Salud)
                .HasColumnType("text")
                .HasColumnName("salud");

            entity.HasOne(d => d.IdAdopcionNavigation).WithMany(p => p.ReportesSeguimientos)
                .HasForeignKey(d => d.IdAdopcion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reporte_adopcion");
        });

        modelBuilder.Entity<RequisitosSolicitud>(entity =>
        {
            entity.HasKey(e => e.IdRequisito).HasName("PRIMARY");

            entity.ToTable("requisitos_solicitud");

            entity.HasIndex(e => e.IdUsuarioRevisor, "fk_requisito_revisor");

            entity.HasIndex(e => e.IdTipoDocumento, "fk_requisito_tipo");

            entity.HasIndex(e => new { e.IdSolicitud, e.IdTipoDocumento }, "uq_solicitud_tipo_documento").IsUnique();

            entity.Property(e => e.IdRequisito).HasColumnName("id_requisito");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'Pendiente'")
                .HasColumnType("enum('Pendiente','Cargado','Aprobado','Rechazado')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaRevision)
                .HasColumnType("datetime")
                .HasColumnName("fecha_revision");
            entity.Property(e => e.FechaSolicitud)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.IdTipoDocumento).HasColumnName("id_tipo_documento");
            entity.Property(e => e.IdUsuarioRevisor).HasColumnName("id_usuario_revisor");
            entity.Property(e => e.MotivoRechazo)
                .HasMaxLength(255)
                .HasColumnName("motivo_rechazo");
            entity.Property(e => e.Obligatorio)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("obligatorio");

            entity.HasOne(d => d.IdSolicitudNavigation).WithMany(p => p.RequisitosSolicituds)
                .HasForeignKey(d => d.IdSolicitud)
                .HasConstraintName("fk_requisito_solicitud");

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.RequisitosSolicituds)
                .HasForeignKey(d => d.IdTipoDocumento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_requisito_tipo");

            entity.HasOne(d => d.IdUsuarioRevisorNavigation).WithMany(p => p.RequisitosSolicituds)
                .HasForeignKey(d => d.IdUsuarioRevisor)
                .HasConstraintName("fk_requisito_revisor");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PRIMARY");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Nombre, "nombre").IsUnique();

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<SolicitudesAdopcion>(entity =>
        {
            entity.HasKey(e => e.IdSolicitud).HasName("PRIMARY");

            entity.ToTable("solicitudes_adopcion");

            entity.HasIndex(e => e.IdEstadoSolicitud, "fk_solicitud_estado");

            entity.HasIndex(e => new { e.IdAdoptante, e.IdEstadoSolicitud }, "idx_solicitud_adoptante_estado");

            entity.HasIndex(e => e.IdMascota, "idx_solicitud_mascota");

            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.CondicionesVivienda)
                .HasColumnType("text")
                .HasColumnName("condiciones_vivienda");
            entity.Property(e => e.DocumentacionCompleta).HasColumnName("documentacion_completa");
            entity.Property(e => e.ExperienciaPrevia)
                .HasColumnType("text")
                .HasColumnName("experiencia_previa");
            entity.Property(e => e.FechaEnvioDocumentos)
                .HasColumnType("datetime")
                .HasColumnName("fecha_envio_documentos");
            entity.Property(e => e.FechaLimiteDocumentos)
                .HasColumnType("date")
                .HasColumnName("fecha_limite_documentos");
            entity.Property(e => e.FechaResolucion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_resolucion");
            entity.Property(e => e.FechaSolicitud)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdAdoptante).HasColumnName("id_adoptante");
            entity.Property(e => e.IdEstadoSolicitud)
                .HasDefaultValueSql("'1'")
                .HasColumnName("id_estado_solicitud");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.MotivoAdopcion)
                .HasColumnType("text")
                .HasColumnName("motivo_adopcion");
            entity.Property(e => e.ObservacionesOrganizacion)
                .HasColumnType("text")
                .HasColumnName("observaciones_organizacion");

            entity.HasOne(d => d.IdAdoptanteNavigation).WithMany(p => p.SolicitudesAdopcions)
                .HasForeignKey(d => d.IdAdoptante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_solicitud_adoptante");

            entity.HasOne(d => d.IdEstadoSolicitudNavigation).WithMany(p => p.SolicitudesAdopcions)
                .HasForeignKey(d => d.IdEstadoSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_solicitud_estado");

            entity.HasOne(d => d.IdMascotaNavigation).WithMany(p => p.SolicitudesAdopcions)
                .HasForeignKey(d => d.IdMascota)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_solicitud_mascota");
        });

        modelBuilder.Entity<SolicitudesDevolucion>(entity =>
        {
            entity.HasKey(e => e.IdDevolucion).HasName("PRIMARY");

            entity.ToTable("solicitudes_devolucion");

            entity.HasIndex(e => e.IdUsuarioRevisor, "fk_devolucion_revisor");

            entity.HasIndex(e => e.IdAdopcion, "uq_devolucion_adopcion").IsUnique();

            entity.Property(e => e.IdDevolucion).HasColumnName("id_devolucion");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'Pendiente'")
                .HasColumnType("enum('Pendiente','Aceptada')")
                .HasColumnName("estado");
            entity.Property(e => e.Explicacion)
                .HasColumnType("text")
                .HasColumnName("explicacion");
            entity.Property(e => e.FechaRevision)
                .HasColumnType("datetime")
                .HasColumnName("fecha_revision");
            entity.Property(e => e.FechaSolicitud)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdAdopcion).HasColumnName("id_adopcion");
            entity.Property(e => e.IdUsuarioRevisor).HasColumnName("id_usuario_revisor");
            entity.Property(e => e.ObservacionesOrganizacion)
                .HasColumnType("text")
                .HasColumnName("observaciones_organizacion");

            entity.HasOne(d => d.IdAdopcionNavigation).WithOne(p => p.SolicitudesDevolucion)
                .HasForeignKey<SolicitudesDevolucion>(d => d.IdAdopcion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_devolucion_adopcion");

            entity.HasOne(d => d.IdUsuarioRevisorNavigation).WithMany(p => p.SolicitudesDevolucions)
                .HasForeignKey(d => d.IdUsuarioRevisor)
                .HasConstraintName("fk_devolucion_revisor");
        });

        modelBuilder.Entity<TiposDocumento>(entity =>
        {
            entity.HasKey(e => e.IdTipoDocumento).HasName("PRIMARY");

            entity.ToTable("tipos_documento");

            entity.HasIndex(e => e.Clave, "clave").IsUnique();

            entity.Property(e => e.IdTipoDocumento).HasColumnName("id_tipo_documento");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Clave)
                .HasMaxLength(40)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.FormatosPermitidos)
                .HasMaxLength(60)
                .HasColumnName("formatos_permitidos");
            entity.Property(e => e.MaxArchivos)
                .HasDefaultValueSql("'1'")
                .HasColumnName("max_archivos");
            entity.Property(e => e.MinArchivos)
                .HasDefaultValueSql("'1'")
                .HasColumnName("min_archivos");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.RequiereFechaEmision).HasColumnName("requiere_fecha_emision");
            entity.Property(e => e.TamanoMaxMb)
                .HasDefaultValueSql("'5'")
                .HasColumnName("tamano_max_mb");
            entity.Property(e => e.VigenciaMeses).HasColumnName("vigencia_meses");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Correo, "correo").IsUnique();

            entity.HasIndex(e => e.IdRol, "fk_usuarios_roles");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.ApellidoMaterno)
                .HasMaxLength(60)
                .HasColumnName("apellido_materno");
            entity.Property(e => e.ApellidoPaterno)
                .HasMaxLength(60)
                .HasColumnName("apellido_paterno");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .HasComment("Guardar hash BCrypt o Argon2, nunca texto plano")
                .HasColumnName("contrasena");
            entity.Property(e => e.Correo)
                .HasMaxLength(120)
                .HasColumnName("correo");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'Activo'")
                .HasColumnType("enum('Activo','Inactivo','Bloqueado')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.FotoPerfil)
                .HasMaxLength(255)
                .HasColumnName("foto_perfil");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_usuarios_roles");
        });

        modelBuilder.Entity<VistaCatalogoMascota>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vista_catalogo_mascotas");

            entity.Property(e => e.Color)
                .HasMaxLength(60)
                .HasColumnName("color");
            entity.Property(e => e.Comportamiento)
                .HasColumnType("text")
                .HasColumnName("comportamiento");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.EdadAproximada)
                .HasMaxLength(40)
                .HasColumnName("edad_aproximada");
            entity.Property(e => e.Especie)
                .HasMaxLength(50)
                .HasColumnName("especie");
            entity.Property(e => e.EstadoAdaptacion)
                .HasMaxLength(40)
                .HasColumnName("estado_adaptacion");
            entity.Property(e => e.EstadoMascota)
                .HasMaxLength(40)
                .HasColumnName("estado_mascota");
            entity.Property(e => e.EstadoSalud)
                .HasColumnType("text")
                .HasColumnName("estado_salud");
            entity.Property(e => e.Esterilizado).HasColumnName("esterilizado");
            entity.Property(e => e.FotoPrincipal)
                .HasMaxLength(255)
                .HasColumnName("foto_principal");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.IdOrganizacion).HasColumnName("id_organizacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
            entity.Property(e => e.Organizacion)
                .HasMaxLength(120)
                .HasColumnName("organizacion");
            entity.Property(e => e.Raza)
                .HasMaxLength(80)
                .HasColumnName("raza");
            entity.Property(e => e.Sexo)
                .HasDefaultValueSql("'Desconocido'")
                .HasColumnType("enum('Macho','Hembra','Desconocido')")
                .HasColumnName("sexo");
            entity.Property(e => e.Tamano)
                .HasColumnType("enum('Pequeno','Mediano','Grande')")
                .HasColumnName("tamano");
            entity.Property(e => e.Vacunado).HasColumnName("vacunado");
        });

        modelBuilder.Entity<VistaReportesSeguimiento>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vista_reportes_seguimiento");

            entity.Property(e => e.Adaptacion)
                .HasColumnType("text")
                .HasColumnName("adaptacion");
            entity.Property(e => e.Adoptante)
                .HasMaxLength(141)
                .HasColumnName("adoptante");
            entity.Property(e => e.Alimentacion)
                .HasColumnType("text")
                .HasColumnName("alimentacion");
            entity.Property(e => e.Comportamiento)
                .HasColumnType("text")
                .HasColumnName("comportamiento");
            entity.Property(e => e.FechaReporte)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_reporte");
            entity.Property(e => e.IdAdopcion).HasColumnName("id_adopcion");
            entity.Property(e => e.IdAdoptante).HasColumnName("id_adoptante");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.IdOrganizacion).HasColumnName("id_organizacion");
            entity.Property(e => e.IdReporte).HasColumnName("id_reporte");
            entity.Property(e => e.Mascota)
                .HasMaxLength(80)
                .HasColumnName("mascota");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.Organizacion)
                .HasMaxLength(120)
                .HasColumnName("organizacion");
            entity.Property(e => e.Salud)
                .HasColumnType("text")
                .HasColumnName("salud");
        });

        modelBuilder.Entity<VistaSolicitudesAdopcion>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vista_solicitudes_adopcion");

            entity.Property(e => e.Adoptante)
                .HasMaxLength(141)
                .HasColumnName("adoptante");
            entity.Property(e => e.Correo)
                .HasMaxLength(120)
                .HasColumnName("correo");
            entity.Property(e => e.DocumentacionCompleta).HasColumnName("documentacion_completa");
            entity.Property(e => e.EstadoSolicitud)
                .HasMaxLength(40)
                .HasColumnName("estado_solicitud");
            entity.Property(e => e.FechaLimiteDocumentos)
                .HasColumnType("date")
                .HasColumnName("fecha_limite_documentos");
            entity.Property(e => e.FechaSolicitud)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdAdoptante).HasColumnName("id_adoptante");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.IdOrganizacion).HasColumnName("id_organizacion");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.Mascota)
                .HasMaxLength(80)
                .HasColumnName("mascota");
            entity.Property(e => e.Organizacion)
                .HasMaxLength(120)
                .HasColumnName("organizacion");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
