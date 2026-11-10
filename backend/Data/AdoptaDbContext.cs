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

    public virtual DbSet<LikesPublicacion> LikesPublicacions { get; set; }

    public virtual DbSet<Mascota> Mascotas { get; set; }

    public virtual DbSet<MotivosDevolucion> MotivosDevolucions { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<Organizacione> Organizaciones { get; set; }

    public virtual DbSet<PerfilesAdoptante> PerfilesAdoptantes { get; set; }

    public virtual DbSet<PreferenciasUsuario> PreferenciasUsuarios { get; set; }

    public virtual DbSet<Publicacione> Publicaciones { get; set; }

    public virtual DbSet<ReportesSeguimiento> ReportesSeguimientos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SolicitudesAdopcion> SolicitudesAdopcions { get; set; }

    public virtual DbSet<SolicitudesDevolucion> SolicitudesDevolucions { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

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
                .HasConstraintName("adopciones_ibfk_1");
        });

        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.HasKey(e => e.IdComentario).HasName("PRIMARY");

            entity.ToTable("comentarios");

            entity.HasIndex(e => e.IdPublicacion, "id_publicacion");

            entity.HasIndex(e => e.IdUsuario, "id_usuario");

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
                .HasConstraintName("comentarios_ibfk_1");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comentarios_ibfk_2");
        });

        modelBuilder.Entity<DetalleMotivosDevolucion>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("PRIMARY");

            entity.ToTable("detalle_motivos_devolucion");

            entity.HasIndex(e => new { e.IdDevolucion, e.IdMotivo }, "id_devolucion").IsUnique();

            entity.HasIndex(e => e.IdMotivo, "id_motivo");

            entity.Property(e => e.IdDetalle).HasColumnName("id_detalle");
            entity.Property(e => e.IdDevolucion).HasColumnName("id_devolucion");
            entity.Property(e => e.IdMotivo).HasColumnName("id_motivo");

            entity.HasOne(d => d.IdDevolucionNavigation).WithMany(p => p.DetalleMotivosDevolucions)
                .HasForeignKey(d => d.IdDevolucion)
                .HasConstraintName("detalle_motivos_devolucion_ibfk_1");

            entity.HasOne(d => d.IdMotivoNavigation).WithMany(p => p.DetalleMotivosDevolucions)
                .HasForeignKey(d => d.IdMotivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detalle_motivos_devolucion_ibfk_2");
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

            entity.HasIndex(e => e.IdMascota, "id_mascota");

            entity.HasIndex(e => new { e.IdUsuario, e.IdMascota }, "id_usuario").IsUnique();

            entity.Property(e => e.IdFavorito).HasColumnName("id_favorito");
            entity.Property(e => e.FechaAgregado)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_agregado");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdMascotaNavigation).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.IdMascota)
                .HasConstraintName("favoritos_ibfk_2");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("favoritos_ibfk_1");
        });

        modelBuilder.Entity<FotosMascotum>(entity =>
        {
            entity.HasKey(e => e.IdFoto).HasName("PRIMARY");

            entity.ToTable("fotos_mascota");

            entity.HasIndex(e => e.IdMascota, "id_mascota");

            entity.Property(e => e.IdFoto).HasColumnName("id_foto");
            entity.Property(e => e.EsPrincipal)
                .HasDefaultValueSql("'0'")
                .HasColumnName("es_principal");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.RutaFoto)
                .HasMaxLength(255)
                .HasColumnName("ruta_foto");

            entity.HasOne(d => d.IdMascotaNavigation).WithMany(p => p.FotosMascota)
                .HasForeignKey(d => d.IdMascota)
                .HasConstraintName("fotos_mascota_ibfk_1");
        });

        modelBuilder.Entity<FotosSeguimiento>(entity =>
        {
            entity.HasKey(e => e.IdFotoSeguimiento).HasName("PRIMARY");

            entity.ToTable("fotos_seguimiento");

            entity.HasIndex(e => e.IdReporte, "id_reporte");

            entity.Property(e => e.IdFotoSeguimiento).HasColumnName("id_foto_seguimiento");
            entity.Property(e => e.IdReporte).HasColumnName("id_reporte");
            entity.Property(e => e.RutaFoto)
                .HasMaxLength(255)
                .HasColumnName("ruta_foto");

            entity.HasOne(d => d.IdReporteNavigation).WithMany(p => p.FotosSeguimientos)
                .HasForeignKey(d => d.IdReporte)
                .HasConstraintName("fotos_seguimiento_ibfk_1");
        });

        modelBuilder.Entity<HistorialAdaptacion>(entity =>
        {
            entity.HasKey(e => e.IdHistorialAdaptacion).HasName("PRIMARY");

            entity.ToTable("historial_adaptacion");

            entity.HasIndex(e => e.IdEstadoAdaptacion, "id_estado_adaptacion");

            entity.HasIndex(e => e.IdMascota, "id_mascota");

            entity.HasIndex(e => e.IdUsuarioResponsable, "id_usuario_responsable");

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
                .HasConstraintName("historial_adaptacion_ibfk_2");

            entity.HasOne(d => d.IdMascotaNavigation).WithMany(p => p.HistorialAdaptacions)
                .HasForeignKey(d => d.IdMascota)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("historial_adaptacion_ibfk_1");

            entity.HasOne(d => d.IdUsuarioResponsableNavigation).WithMany(p => p.HistorialAdaptacions)
                .HasForeignKey(d => d.IdUsuarioResponsable)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("historial_adaptacion_ibfk_3");
        });

        modelBuilder.Entity<HistorialSolicitud>(entity =>
        {
            entity.HasKey(e => e.IdHistorial).HasName("PRIMARY");

            entity.ToTable("historial_solicitud");

            entity.HasIndex(e => e.IdEstadoSolicitud, "id_estado_solicitud");

            entity.HasIndex(e => e.IdSolicitud, "id_solicitud");

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
                .HasConstraintName("historial_solicitud_ibfk_2");

            entity.HasOne(d => d.IdSolicitudNavigation).WithMany(p => p.HistorialSolicituds)
                .HasForeignKey(d => d.IdSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("historial_solicitud_ibfk_1");
        });

        modelBuilder.Entity<LikesPublicacion>(entity =>
        {
            entity.HasKey(e => e.IdLike).HasName("PRIMARY");

            entity.ToTable("likes_publicacion");

            entity.HasIndex(e => new { e.IdPublicacion, e.IdUsuario }, "id_publicacion").IsUnique();

            entity.HasIndex(e => e.IdUsuario, "id_usuario");

            entity.Property(e => e.IdLike).HasColumnName("id_like");
            entity.Property(e => e.FechaLike)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_like");
            entity.Property(e => e.IdPublicacion).HasColumnName("id_publicacion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdPublicacionNavigation).WithMany(p => p.LikesPublicacions)
                .HasForeignKey(d => d.IdPublicacion)
                .HasConstraintName("likes_publicacion_ibfk_1");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.LikesPublicacions)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("likes_publicacion_ibfk_2");
        });

        modelBuilder.Entity<Mascota>(entity =>
        {
            entity.HasKey(e => e.IdMascota).HasName("PRIMARY");

            entity.ToTable("mascotas");

            entity.HasIndex(e => e.IdEstadoAdaptacion, "id_estado_adaptacion");

            entity.HasIndex(e => e.IdEstadoMascota, "id_estado_mascota");

            entity.HasIndex(e => e.IdOrganizacion, "id_organizacion");

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
            entity.Property(e => e.Esterilizado)
                .HasDefaultValueSql("'0'")
                .HasColumnName("esterilizado");
            entity.Property(e => e.FechaNacimiento)
                .HasColumnType("date")
                .HasColumnName("fecha_nacimiento");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdEstadoAdaptacion).HasColumnName("id_estado_adaptacion");
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
            entity.Property(e => e.Vacunado)
                .HasDefaultValueSql("'0'")
                .HasColumnName("vacunado");

            entity.HasOne(d => d.IdEstadoAdaptacionNavigation).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.IdEstadoAdaptacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mascotas_ibfk_3");

            entity.HasOne(d => d.IdEstadoMascotaNavigation).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.IdEstadoMascota)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mascotas_ibfk_2");

            entity.HasOne(d => d.IdOrganizacionNavigation).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.IdOrganizacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mascotas_ibfk_1");
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

            entity.HasIndex(e => e.IdUsuario, "id_usuario");

            entity.Property(e => e.IdNotificacion).HasColumnName("id_notificacion");
            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Leida)
                .HasDefaultValueSql("'0'")
                .HasColumnName("leida");
            entity.Property(e => e.Mensaje)
                .HasColumnType("text")
                .HasColumnName("mensaje");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .HasColumnName("tipo");
            entity.Property(e => e.Titulo)
                .HasMaxLength(120)
                .HasColumnName("titulo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("notificaciones_ibfk_1");
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
                .HasConstraintName("organizaciones_ibfk_1");
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
            entity.Property(e => e.TieneMascotas)
                .HasDefaultValueSql("'0'")
                .HasColumnName("tiene_mascotas");
            entity.Property(e => e.TipoVivienda)
                .HasMaxLength(50)
                .HasColumnName("tipo_vivienda");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.PerfilesAdoptante)
                .HasForeignKey<PerfilesAdoptante>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("perfiles_adoptantes_ibfk_1");
        });

        modelBuilder.Entity<PreferenciasUsuario>(entity =>
        {
            entity.HasKey(e => e.IdPreferencia).HasName("PRIMARY");

            entity.ToTable("preferencias_usuario");

            entity.HasIndex(e => e.IdUsuario, "id_usuario");

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
                .HasConstraintName("preferencias_usuario_ibfk_1");
        });

        modelBuilder.Entity<Publicacione>(entity =>
        {
            entity.HasKey(e => e.IdPublicacion).HasName("PRIMARY");

            entity.ToTable("publicaciones");

            entity.HasIndex(e => e.IdUsuario, "id_usuario");

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
                .HasConstraintName("publicaciones_ibfk_1");
        });

        modelBuilder.Entity<ReportesSeguimiento>(entity =>
        {
            entity.HasKey(e => e.IdReporte).HasName("PRIMARY");

            entity.ToTable("reportes_seguimiento");

            entity.HasIndex(e => e.IdAdopcion, "id_adopcion");

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
                .HasConstraintName("reportes_seguimiento_ibfk_1");
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

            entity.HasIndex(e => e.IdAdoptante, "id_adoptante");

            entity.HasIndex(e => e.IdEstadoSolicitud, "id_estado_solicitud");

            entity.HasIndex(e => e.IdMascota, "id_mascota");

            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.CondicionesVivienda)
                .HasColumnType("text")
                .HasColumnName("condiciones_vivienda");
            entity.Property(e => e.ExperienciaPrevia)
                .HasColumnType("text")
                .HasColumnName("experiencia_previa");
            entity.Property(e => e.FechaResolucion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_resolucion");
            entity.Property(e => e.FechaSolicitud)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdAdoptante).HasColumnName("id_adoptante");
            entity.Property(e => e.IdEstadoSolicitud).HasColumnName("id_estado_solicitud");
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
                .HasConstraintName("solicitudes_adopcion_ibfk_2");

            entity.HasOne(d => d.IdEstadoSolicitudNavigation).WithMany(p => p.SolicitudesAdopcions)
                .HasForeignKey(d => d.IdEstadoSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("solicitudes_adopcion_ibfk_3");

            entity.HasOne(d => d.IdMascotaNavigation).WithMany(p => p.SolicitudesAdopcions)
                .HasForeignKey(d => d.IdMascota)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("solicitudes_adopcion_ibfk_1");
        });

        modelBuilder.Entity<SolicitudesDevolucion>(entity =>
        {
            entity.HasKey(e => e.IdDevolucion).HasName("PRIMARY");

            entity.ToTable("solicitudes_devolucion");

            entity.HasIndex(e => e.IdAdopcion, "id_adopcion");

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
            entity.Property(e => e.ObservacionesOrganizacion)
                .HasColumnType("text")
                .HasColumnName("observaciones_organizacion");

            entity.HasOne(d => d.IdAdopcionNavigation).WithMany(p => p.SolicitudesDevolucions)
                .HasForeignKey(d => d.IdAdopcion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("solicitudes_devolucion_ibfk_1");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Correo, "correo").IsUnique();

            entity.HasIndex(e => e.IdRol, "id_rol");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.ApellidoMaterno)
                .HasMaxLength(60)
                .HasColumnName("apellido_materno");
            entity.Property(e => e.ApellidoPaterno)
                .HasMaxLength(60)
                .HasColumnName("apellido_paterno");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
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
                .HasConstraintName("usuarios_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
