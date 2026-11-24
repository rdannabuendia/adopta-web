-- ADOPTA+
DROP DATABASE IF EXISTS adopta_db;
CREATE DATABASE adopta_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE adopta_db;

-- 1. USUARIOS Y ROLES
CREATE TABLE roles (
 id_rol INT AUTO_INCREMENT PRIMARY KEY,
 nombre VARCHAR(30) NOT NULL UNIQUE
) ENGINE=InnoDB;
INSERT INTO roles(nombre) VALUES ('Adoptante'),('Organizacion'),('Administrador');

CREATE TABLE usuarios (
 id_usuario INT AUTO_INCREMENT PRIMARY KEY,
 id_rol INT NOT NULL,
 nombre VARCHAR(80) NOT NULL,
 apellido_paterno VARCHAR(60) NOT NULL,
 apellido_materno VARCHAR(60) NULL,
 correo VARCHAR(120) NOT NULL UNIQUE,
 contrasena VARCHAR(255) NOT NULL COMMENT 'Guardar hash BCrypt o Argon2, nunca texto plano',
 telefono VARCHAR(20) NULL,
 foto_perfil VARCHAR(255) NULL,
 fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 estado ENUM('Activo','Inactivo','Bloqueado') NOT NULL DEFAULT 'Activo',
 CONSTRAINT fk_usuarios_roles FOREIGN KEY(id_rol) REFERENCES roles(id_rol)
) ENGINE=InnoDB;

CREATE TABLE perfiles_adoptantes (
 id_perfil INT AUTO_INCREMENT PRIMARY KEY,
 id_usuario INT NOT NULL UNIQUE,
 fecha_nacimiento DATE NULL,
 direccion VARCHAR(200) NULL,
 ciudad VARCHAR(80) NULL,
 codigo_postal VARCHAR(10) NULL,
 tipo_vivienda VARCHAR(50) NULL,
 tiene_mascotas BOOLEAN NOT NULL DEFAULT FALSE,
 experiencia_mascotas TEXT NULL,
 ocupacion VARCHAR(80) NULL,
 contacto_emergencia_nombre VARCHAR(120) NULL,
 contacto_emergencia_telefono VARCHAR(20) NULL,
 contacto_emergencia_parentesco VARCHAR(40) NULL,
 CONSTRAINT fk_perfil_usuario FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE preferencias_usuario (
 id_preferencia INT AUTO_INCREMENT PRIMARY KEY,
 id_usuario INT NOT NULL,
 especie VARCHAR(50) NULL,
 tamano VARCHAR(30) NULL,
 edad_preferida VARCHAR(30) NULL,
 CONSTRAINT fk_preferencias_usuario FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario) ON DELETE CASCADE,
 INDEX idx_preferencias_usuario(id_usuario)
) ENGINE=InnoDB;

-- 2. ORGANIZACIONES
CREATE TABLE organizaciones (
 id_organizacion INT AUTO_INCREMENT PRIMARY KEY,
 id_usuario INT NOT NULL UNIQUE,
 nombre VARCHAR(120) NOT NULL,
 tipo_organizacion VARCHAR(60) NULL,
 descripcion TEXT NULL,
 direccion VARCHAR(200) NULL,
 ciudad VARCHAR(80) NULL,
 codigo_postal VARCHAR(10) NULL,
 telefono VARCHAR(20) NULL,
 correo_contacto VARCHAR(120) NULL,
 sitio_web VARCHAR(255) NULL,
 fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 estado ENUM('Pendiente','Activa','Inactiva') NOT NULL DEFAULT 'Pendiente',
 CONSTRAINT fk_organizacion_usuario FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario)
) ENGINE=InnoDB;

-- Documentos de la solicitud de organización
CREATE TABLE documentos_organizacion (
 id_documento INT AUTO_INCREMENT PRIMARY KEY,
 id_organizacion INT NOT NULL,
 tipo_documento ENUM('ExistenciaLegal','ComprobanteDomicilio','IdentificacionResponsable') NOT NULL,
 nombre_original VARCHAR(255) NOT NULL,
 ruta_relativa VARCHAR(255) NOT NULL,
 mime_type VARCHAR(100) NOT NULL,
 tamano_bytes INT UNSIGNED NOT NULL,
 hash_sha256 CHAR(64) NOT NULL,
 fecha_carga DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 CONSTRAINT fk_doc_org FOREIGN KEY(id_organizacion) REFERENCES organizaciones(id_organizacion) ON DELETE CASCADE,
 UNIQUE KEY uq_doc_org_tipo(id_organizacion,tipo_documento)
) ENGINE=InnoDB;

CREATE TABLE motivos_baja (
 id_motivo_baja INT AUTO_INCREMENT PRIMARY KEY,
 nombre VARCHAR(100) NOT NULL UNIQUE
) ENGINE=InnoDB;
INSERT INTO motivos_baja(nombre) VALUES
('Incumplimiento en el seguimiento de adopciones'),
('Información o documentos falsos'),
('Maltrato o negligencia hacia los animales'),
('Cobros indebidos por adopción'),
('Inactividad prolongada'),
('Otro');

CREATE TABLE bajas_organizacion (
 id_baja INT AUTO_INCREMENT PRIMARY KEY,
 id_organizacion INT NOT NULL,
 id_motivo_baja INT NOT NULL,
 id_usuario_admin INT NOT NULL,
 descripcion TEXT NOT NULL,
 fecha_revision DATETIME NOT NULL,
 fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 CONSTRAINT fk_baja_organizacion FOREIGN KEY(id_organizacion) REFERENCES organizaciones(id_organizacion),
 CONSTRAINT fk_baja_motivo FOREIGN KEY(id_motivo_baja) REFERENCES motivos_baja(id_motivo_baja),
 CONSTRAINT fk_baja_admin FOREIGN KEY(id_usuario_admin) REFERENCES usuarios(id_usuario),
 INDEX idx_baja_organizacion(id_organizacion, fecha_registro)
) ENGINE=InnoDB;

-- Enlaces de recuperación de contraseña (se guarda solo el hash SHA-256 del token, fechas en UTC)
CREATE TABLE tokens_recuperacion (
 id_token INT AUTO_INCREMENT PRIMARY KEY,
 id_usuario INT NOT NULL,
 token_hash CHAR(64) NOT NULL UNIQUE,
 fecha_creacion DATETIME NOT NULL,
 fecha_expiracion DATETIME NOT NULL,
 usado BOOLEAN NOT NULL DEFAULT FALSE,
 CONSTRAINT fk_token_usuario FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario) ON DELETE CASCADE,
 INDEX idx_token_usuario(id_usuario,fecha_creacion)
) ENGINE=InnoDB;

-- 3. MASCOTAS
CREATE TABLE estados_mascota (
 id_estado_mascota INT AUTO_INCREMENT PRIMARY KEY,
 nombre VARCHAR(40) NOT NULL UNIQUE
) ENGINE=InnoDB;
INSERT INTO estados_mascota(nombre) VALUES
('Disponible'),('En proceso de adopcion'),('Adoptada'),('En seguimiento'),('En proceso de devolucion'),('No disponible');

CREATE TABLE estados_adaptacion (
 id_estado_adaptacion INT AUTO_INCREMENT PRIMARY KEY,
 nombre VARCHAR(40) NOT NULL UNIQUE
) ENGINE=InnoDB;
INSERT INTO estados_adaptacion(nombre) VALUES
('Sin seguimiento'),('En adaptacion'),('Adaptacion favorable'),('Requiere atencion'),('Adaptacion completada');

CREATE TABLE mascotas (
 id_mascota INT AUTO_INCREMENT PRIMARY KEY,
 id_organizacion INT NOT NULL,
 id_estado_mascota INT NOT NULL,
 id_estado_adaptacion INT NOT NULL DEFAULT 1,
 nombre VARCHAR(80) NOT NULL,
 especie VARCHAR(50) NOT NULL,
 raza VARCHAR(80) NULL,
 sexo ENUM('Macho','Hembra','Desconocido') NOT NULL DEFAULT 'Desconocido',
 fecha_nacimiento DATE NULL,
 edad_aproximada VARCHAR(40) NULL,
 tamano ENUM('Pequeno','Mediano','Grande') NULL,
 color VARCHAR(60) NULL,
 peso DECIMAL(6,2) NULL,
 descripcion TEXT NULL,
 comportamiento TEXT NULL,
 estado_salud TEXT NULL,
 vacunado BOOLEAN NOT NULL DEFAULT FALSE,
 esterilizado BOOLEAN NOT NULL DEFAULT FALSE,
 fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 CONSTRAINT fk_mascota_organizacion FOREIGN KEY(id_organizacion) REFERENCES organizaciones(id_organizacion),
 CONSTRAINT fk_mascota_estado FOREIGN KEY(id_estado_mascota) REFERENCES estados_mascota(id_estado_mascota),
 CONSTRAINT fk_mascota_adaptacion FOREIGN KEY(id_estado_adaptacion) REFERENCES estados_adaptacion(id_estado_adaptacion),
 INDEX idx_mascotas_catalogo(id_estado_mascota,especie,tamano),
 INDEX idx_mascotas_organizacion(id_organizacion)
) ENGINE=InnoDB;

CREATE TABLE fotos_mascota (
 id_foto INT AUTO_INCREMENT PRIMARY KEY,
 id_mascota INT NOT NULL,
 ruta_foto VARCHAR(255) NOT NULL,
 es_principal BOOLEAN NOT NULL DEFAULT FALSE,
 CONSTRAINT fk_foto_mascota FOREIGN KEY(id_mascota) REFERENCES mascotas(id_mascota) ON DELETE CASCADE,
 INDEX idx_fotos_mascota(id_mascota)
) ENGINE=InnoDB;

-- 4. SOLICITUDES DE ADOPCION
CREATE TABLE estados_solicitud (
 id_estado_solicitud INT AUTO_INCREMENT PRIMARY KEY,
 nombre VARCHAR(40) NOT NULL UNIQUE
) ENGINE=InnoDB;
INSERT INTO estados_solicitud(nombre) VALUES
('Pendiente'),('En revision'),('Aprobada'),('Rechazada'),('Cancelada'),('Documentacion pendiente');

CREATE TABLE solicitudes_adopcion (
 id_solicitud INT AUTO_INCREMENT PRIMARY KEY,
 id_mascota INT NOT NULL,
 id_adoptante INT NOT NULL,
 fecha_solicitud DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 motivo_adopcion TEXT NULL,
 experiencia_previa TEXT NULL,
 condiciones_vivienda TEXT NULL,
 id_estado_solicitud INT NOT NULL DEFAULT 1,
 observaciones_organizacion TEXT NULL,
 fecha_resolucion DATETIME NULL,
 fecha_limite_documentos DATE NULL,
 fecha_envio_documentos DATETIME NULL,
 documentacion_completa BOOLEAN NOT NULL DEFAULT FALSE,
 CONSTRAINT fk_solicitud_mascota FOREIGN KEY(id_mascota) REFERENCES mascotas(id_mascota),
 CONSTRAINT fk_solicitud_adoptante FOREIGN KEY(id_adoptante) REFERENCES usuarios(id_usuario),
 CONSTRAINT fk_solicitud_estado FOREIGN KEY(id_estado_solicitud) REFERENCES estados_solicitud(id_estado_solicitud),
 INDEX idx_solicitud_adoptante_estado(id_adoptante,id_estado_solicitud),
 INDEX idx_solicitud_mascota(id_mascota)
) ENGINE=InnoDB;

CREATE TABLE historial_solicitud (
 id_historial INT AUTO_INCREMENT PRIMARY KEY,
 id_solicitud INT NOT NULL,
 id_estado_solicitud INT NOT NULL,
 observacion TEXT NULL,
 fecha_cambio DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 CONSTRAINT fk_historial_solicitud FOREIGN KEY(id_solicitud) REFERENCES solicitudes_adopcion(id_solicitud) ON DELETE CASCADE,
 CONSTRAINT fk_historial_estado FOREIGN KEY(id_estado_solicitud) REFERENCES estados_solicitud(id_estado_solicitud),
 INDEX idx_historial_solicitud(id_solicitud,fecha_cambio)
) ENGINE=InnoDB;

CREATE TABLE adopciones (
 id_adopcion INT AUTO_INCREMENT PRIMARY KEY,
 id_solicitud INT NOT NULL UNIQUE,
 fecha_adopcion DATE NOT NULL,
 contrato_url VARCHAR(255) NULL,
 observaciones TEXT NULL,
 CONSTRAINT fk_adopcion_solicitud FOREIGN KEY(id_solicitud) REFERENCES solicitudes_adopcion(id_solicitud)
) ENGINE=InnoDB;

-- 5. DOCUMENTACION DE ADOPTANTES
CREATE TABLE tipos_documento (
 id_tipo_documento INT AUTO_INCREMENT PRIMARY KEY,
 clave VARCHAR(40) NOT NULL UNIQUE,
 nombre VARCHAR(100) NOT NULL,
 descripcion VARCHAR(255) NULL,
 formatos_permitidos VARCHAR(60) NOT NULL,
 tamano_max_mb TINYINT UNSIGNED NOT NULL DEFAULT 5,
 min_archivos TINYINT UNSIGNED NOT NULL DEFAULT 1,
 max_archivos TINYINT UNSIGNED NOT NULL DEFAULT 1,
 requiere_fecha_emision BOOLEAN NOT NULL DEFAULT FALSE,
 vigencia_meses TINYINT UNSIGNED NULL,
 activo BOOLEAN NOT NULL DEFAULT TRUE,
 CONSTRAINT chk_tipo_documento_archivos CHECK(max_archivos >= min_archivos)
) ENGINE=InnoDB;

INSERT INTO tipos_documento
(clave,nombre,descripcion,formatos_permitidos,tamano_max_mb,min_archivos,max_archivos,requiere_fecha_emision,vigencia_meses,activo)
VALUES
('IDENTIFICACION_FRENTE','Identificacion oficial (frente)','Identificacion por el frente','pdf,jpg,png',5,1,1,FALSE,NULL,TRUE),
('IDENTIFICACION_REVERSO','Identificacion oficial (reverso)','Reverso de INE o licencia','pdf,jpg,png',5,1,1,FALSE,NULL,TRUE),
('COMPROBANTE_DOMICILIO','Comprobante de domicilio','Antiguedad maxima de 3 meses','pdf,jpg,png',5,1,1,TRUE,3,TRUE),
('FOTOS_HOGAR','Fotografias del hogar','Imagenes de espacios donde vivira la mascota','jpg,png',5,2,5,FALSE,NULL,TRUE),
('CARTA_ARRENDADOR','Carta de autorizacion del arrendador','Para vivienda rentada','pdf,jpg,png',5,1,1,FALSE,NULL,TRUE),
('CARTILLA_VACUNACION','Cartilla de vacunacion','Cartilla de otras mascotas','pdf,jpg,png',5,1,3,FALSE,NULL,TRUE),
('COMPROBANTE_ESTERILIZACION','Comprobante de esterilizacion','Comprobante de otras mascotas','pdf,jpg,png',5,1,3,FALSE,NULL,TRUE);

CREATE TABLE requisitos_solicitud (
 id_requisito INT AUTO_INCREMENT PRIMARY KEY,
 id_solicitud INT NOT NULL,
 id_tipo_documento INT NOT NULL,
 obligatorio BOOLEAN NOT NULL DEFAULT TRUE,
 estado ENUM('Pendiente','Cargado','Aprobado','Rechazado') NOT NULL DEFAULT 'Pendiente',
 motivo_rechazo VARCHAR(255) NULL,
 id_usuario_revisor INT NULL,
 fecha_solicitud DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 fecha_revision DATETIME NULL,
 CONSTRAINT fk_requisito_solicitud FOREIGN KEY(id_solicitud) REFERENCES solicitudes_adopcion(id_solicitud) ON DELETE CASCADE,
 CONSTRAINT fk_requisito_tipo FOREIGN KEY(id_tipo_documento) REFERENCES tipos_documento(id_tipo_documento),
 CONSTRAINT fk_requisito_revisor FOREIGN KEY(id_usuario_revisor) REFERENCES usuarios(id_usuario),
 UNIQUE KEY uq_solicitud_tipo_documento(id_solicitud,id_tipo_documento)
) ENGINE=InnoDB;

CREATE TABLE archivos_requisito (
 id_archivo INT AUTO_INCREMENT PRIMARY KEY,
 id_requisito INT NOT NULL,
 id_usuario_carga INT NOT NULL,
 nombre_original VARCHAR(255) NOT NULL,
 nombre_almacenado VARCHAR(80) NOT NULL UNIQUE,
 ruta_relativa VARCHAR(255) NOT NULL,
 mime_type VARCHAR(100) NOT NULL,
 extension VARCHAR(10) NOT NULL,
 tamano_bytes INT UNSIGNED NOT NULL,
 hash_sha256 CHAR(64) NOT NULL,
 fecha_emision_documento DATE NULL,
 origen ENUM('Web','Movil') NOT NULL,
 es_vigente BOOLEAN NOT NULL DEFAULT TRUE,
 fecha_carga DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 CONSTRAINT fk_archivo_requisito FOREIGN KEY(id_requisito) REFERENCES requisitos_solicitud(id_requisito) ON DELETE CASCADE,
 CONSTRAINT fk_archivo_usuario FOREIGN KEY(id_usuario_carga) REFERENCES usuarios(id_usuario),
 INDEX idx_archivos_requisito_vigente(id_requisito,es_vigente)
) ENGINE=InnoDB;

CREATE TABLE informacion_complementaria_solicitud (
 id_info INT AUTO_INCREMENT PRIMARY KEY,
 id_solicitud INT NOT NULL UNIQUE,
 regimen_vivienda ENUM('Propia','Rentada','Familiar','Otro') NOT NULL,
 arrendador_permite_mascotas BOOLEAN NULL,
 tiene_espacio_exterior BOOLEAN NOT NULL,
 espacio_exterior_cercado BOOLEAN NULL,
 superficie_m2 SMALLINT UNSIGNED NULL,
 num_personas_hogar TINYINT UNSIGNED NOT NULL,
 hay_menores BOOLEAN NOT NULL,
 hogar_de_acuerdo BOOLEAN NOT NULL,
 horas_solo_dia ENUM('Menos de 4 horas','De 4 a 8 horas','Mas de 8 horas') NOT NULL,
 lugar_dormir ENUM('Dentro de casa','Patio o exterior','Area techada exterior','Otro') NOT NULL,
 cuidador_alterno VARCHAR(120) NULL,
 presupuesto_mensual ENUM('Menos de $500','$500 a $1,000','$1,001 a $2,000','Mas de $2,000') NOT NULL,
 tiene_otras_mascotas BOOLEAN NOT NULL,
 cantidad_otras_mascotas TINYINT UNSIGNED NULL,
 especies_otras_mascotas VARCHAR(120) NULL,
 otras_esterilizadas BOOLEAN NULL,
 otras_vacunadas BOOLEAN NULL,
 historial_mascotas_previas TEXT NULL,
 veterinario_nombre VARCHAR(120) NULL,
 veterinario_telefono VARCHAR(20) NULL,
 acepta_visita_domiciliaria BOOLEAN NOT NULL,
 acepta_seguimiento BOOLEAN NOT NULL,
 compromiso_esterilizacion BOOLEAN NULL,
 acepta_aviso_privacidad BOOLEAN NOT NULL,
 declaracion_veracidad BOOLEAN NOT NULL,
 firma_nombre VARCHAR(160) NULL,
 fecha_firma DATETIME NULL,
 estado_formulario ENUM('Borrador','Enviado') NOT NULL DEFAULT 'Borrador',
 fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 fecha_actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
 CONSTRAINT fk_info_solicitud FOREIGN KEY(id_solicitud) REFERENCES solicitudes_adopcion(id_solicitud) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE referencias_personales_solicitud (
 id_referencia INT AUTO_INCREMENT PRIMARY KEY,
 id_solicitud INT NOT NULL,
 nombre_completo VARCHAR(120) NOT NULL,
 telefono VARCHAR(20) NOT NULL,
 relacion VARCHAR(60) NOT NULL,
 anios_conocidos TINYINT UNSIGNED NULL,
 CONSTRAINT fk_referencia_solicitud FOREIGN KEY(id_solicitud) REFERENCES solicitudes_adopcion(id_solicitud) ON DELETE CASCADE,
 INDEX idx_referencias_solicitud(id_solicitud)
) ENGINE=InnoDB;

-- 6. SEGUIMIENTO Y ADAPTACION
CREATE TABLE reportes_seguimiento (
 id_reporte INT AUTO_INCREMENT PRIMARY KEY,
 id_adopcion INT NOT NULL,
 comportamiento TEXT NULL,
 salud TEXT NULL,
 alimentacion TEXT NULL,
 adaptacion TEXT NULL,
 observaciones TEXT NULL,
 fecha_reporte DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 CONSTRAINT fk_reporte_adopcion FOREIGN KEY(id_adopcion) REFERENCES adopciones(id_adopcion),
 INDEX idx_reportes_adopcion_fecha(id_adopcion,fecha_reporte)
) ENGINE=InnoDB;

CREATE TABLE fotos_seguimiento (
 id_foto_seguimiento INT AUTO_INCREMENT PRIMARY KEY,
 id_reporte INT NOT NULL,
 ruta_foto VARCHAR(255) NOT NULL,
 CONSTRAINT fk_foto_seguimiento FOREIGN KEY(id_reporte) REFERENCES reportes_seguimiento(id_reporte) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE historial_adaptacion (
 id_historial_adaptacion INT AUTO_INCREMENT PRIMARY KEY,
 id_mascota INT NOT NULL,
 id_estado_adaptacion INT NOT NULL,
 id_usuario_responsable INT NOT NULL,
 observaciones TEXT NULL,
 fecha_cambio DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 CONSTRAINT fk_hist_adapt_mascota FOREIGN KEY(id_mascota) REFERENCES mascotas(id_mascota),
 CONSTRAINT fk_hist_adapt_estado FOREIGN KEY(id_estado_adaptacion) REFERENCES estados_adaptacion(id_estado_adaptacion),
 CONSTRAINT fk_hist_adapt_usuario FOREIGN KEY(id_usuario_responsable) REFERENCES usuarios(id_usuario),
 INDEX idx_hist_adapt_mascota_fecha(id_mascota,fecha_cambio)
) ENGINE=InnoDB;

-- 7. DEVOLUCIONES
CREATE TABLE motivos_devolucion (
 id_motivo INT AUTO_INCREMENT PRIMARY KEY,
 nombre VARCHAR(100) NOT NULL UNIQUE
) ENGINE=InnoDB;
INSERT INTO motivos_devolucion(nombre) VALUES
('Problemas economicos'),('Cambio de domicilio'),('Falta de tiempo'),
('Problemas de comportamiento'),('Problemas de salud'),
('Problemas de convivencia'),('Otros');

CREATE TABLE solicitudes_devolucion (
 id_devolucion INT AUTO_INCREMENT PRIMARY KEY,
 id_adopcion INT NOT NULL,
 explicacion TEXT NOT NULL,
 fecha_solicitud DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 estado ENUM('Pendiente','Aceptada') NOT NULL DEFAULT 'Pendiente',
 fecha_revision DATETIME NULL,
 id_usuario_revisor INT NULL,
 observaciones_organizacion TEXT NULL,
 CONSTRAINT fk_devolucion_adopcion FOREIGN KEY(id_adopcion) REFERENCES adopciones(id_adopcion),
 CONSTRAINT fk_devolucion_revisor FOREIGN KEY(id_usuario_revisor) REFERENCES usuarios(id_usuario),
 UNIQUE KEY uq_devolucion_adopcion(id_adopcion)
) ENGINE=InnoDB;

CREATE TABLE detalle_motivos_devolucion (
 id_detalle INT AUTO_INCREMENT PRIMARY KEY,
 id_devolucion INT NOT NULL,
 id_motivo INT NOT NULL,
 CONSTRAINT fk_detalle_devolucion FOREIGN KEY(id_devolucion) REFERENCES solicitudes_devolucion(id_devolucion) ON DELETE CASCADE,
 CONSTRAINT fk_detalle_motivo FOREIGN KEY(id_motivo) REFERENCES motivos_devolucion(id_motivo),
 UNIQUE KEY uq_devolucion_motivo(id_devolucion,id_motivo)
) ENGINE=InnoDB;

-- 8. FAVORITOS
CREATE TABLE favoritos (
 id_favorito INT AUTO_INCREMENT PRIMARY KEY,
 id_usuario INT NOT NULL,
 id_mascota INT NOT NULL,
 fecha_agregado DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 CONSTRAINT fk_favorito_usuario FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario) ON DELETE CASCADE,
 CONSTRAINT fk_favorito_mascota FOREIGN KEY(id_mascota) REFERENCES mascotas(id_mascota) ON DELETE CASCADE,
 UNIQUE KEY uq_favorito_usuario_mascota(id_usuario,id_mascota)
) ENGINE=InnoDB;

-- 9. COMUNIDAD
CREATE TABLE publicaciones (
 id_publicacion INT AUTO_INCREMENT PRIMARY KEY,
 id_usuario INT NOT NULL,
 contenido TEXT NOT NULL,
 imagen VARCHAR(255) NULL,
 fecha_publicacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 fecha_actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
 CONSTRAINT fk_publicacion_usuario FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario),
 INDEX idx_publicaciones_fecha(fecha_publicacion)
) ENGINE=InnoDB;

CREATE TABLE comentarios (
 id_comentario INT AUTO_INCREMENT PRIMARY KEY,
 id_publicacion INT NOT NULL,
 id_usuario INT NOT NULL,
 contenido TEXT NOT NULL,
 fecha_comentario DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 CONSTRAINT fk_comentario_publicacion FOREIGN KEY(id_publicacion) REFERENCES publicaciones(id_publicacion) ON DELETE CASCADE,
 CONSTRAINT fk_comentario_usuario FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario),
 INDEX idx_comentarios_publicacion_fecha(id_publicacion,fecha_comentario)
) ENGINE=InnoDB;

CREATE TABLE likes_publicacion (
 id_like INT AUTO_INCREMENT PRIMARY KEY,
 id_publicacion INT NOT NULL,
 id_usuario INT NOT NULL,
 fecha_like DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 CONSTRAINT fk_like_publicacion FOREIGN KEY(id_publicacion) REFERENCES publicaciones(id_publicacion) ON DELETE CASCADE,
 CONSTRAINT fk_like_usuario FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario) ON DELETE CASCADE,
 UNIQUE KEY uq_like_publicacion_usuario(id_publicacion,id_usuario)
) ENGINE=InnoDB;

-- 10. NOTIFICACIONES
CREATE TABLE notificaciones (
 id_notificacion INT AUTO_INCREMENT PRIMARY KEY,
 id_usuario INT NOT NULL,
 titulo VARCHAR(120) NOT NULL,
 mensaje TEXT NOT NULL,
 tipo VARCHAR(50) NULL,
 leida BOOLEAN NOT NULL DEFAULT FALSE,
 fecha_envio DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
 id_solicitud INT NULL,
 CONSTRAINT fk_notificacion_usuario FOREIGN KEY(id_usuario) REFERENCES usuarios(id_usuario) ON DELETE CASCADE,
 CONSTRAINT fk_notificacion_solicitud FOREIGN KEY(id_solicitud) REFERENCES solicitudes_adopcion(id_solicitud) ON DELETE SET NULL,
 INDEX idx_notificaciones_usuario_fecha(id_usuario,leida,fecha_envio)
) ENGINE=InnoDB;

-- VISTAS AUXILIARES
CREATE OR REPLACE VIEW vista_catalogo_mascotas AS
SELECT m.id_mascota,m.nombre,m.especie,m.raza,m.sexo,m.edad_aproximada,m.tamano,
 m.color,m.descripcion,m.comportamiento,m.estado_salud,m.vacunado,m.esterilizado,
 em.nombre AS estado_mascota,ea.nombre AS estado_adaptacion,
 o.id_organizacion,o.nombre AS organizacion,
 (SELECT fm.ruta_foto FROM fotos_mascota fm
  WHERE fm.id_mascota=m.id_mascota AND fm.es_principal=TRUE
  ORDER BY fm.id_foto LIMIT 1) AS foto_principal
FROM mascotas m
JOIN estados_mascota em ON em.id_estado_mascota=m.id_estado_mascota
JOIN estados_adaptacion ea ON ea.id_estado_adaptacion=m.id_estado_adaptacion
JOIN organizaciones o ON o.id_organizacion=m.id_organizacion;

CREATE OR REPLACE VIEW vista_solicitudes_adopcion AS
SELECT s.id_solicitud,s.fecha_solicitud,s.id_mascota,m.nombre AS mascota,
 s.id_adoptante,CONCAT(u.nombre,' ',u.apellido_paterno) AS adoptante,
 u.correo,es.nombre AS estado_solicitud,s.documentacion_completa,
 s.fecha_limite_documentos,o.id_organizacion,o.nombre AS organizacion
FROM solicitudes_adopcion s
JOIN mascotas m ON m.id_mascota=s.id_mascota
JOIN usuarios u ON u.id_usuario=s.id_adoptante
JOIN estados_solicitud es ON es.id_estado_solicitud=s.id_estado_solicitud
JOIN organizaciones o ON o.id_organizacion=m.id_organizacion;

CREATE OR REPLACE VIEW vista_reportes_seguimiento AS
SELECT r.id_reporte,r.fecha_reporte,r.comportamiento,r.salud,r.alimentacion,
 r.adaptacion,r.observaciones,a.id_adopcion,m.id_mascota,m.nombre AS mascota,
 org.id_organizacion,org.nombre AS organizacion,u.id_usuario AS id_adoptante,
 CONCAT(u.nombre,' ',u.apellido_paterno) AS adoptante
FROM reportes_seguimiento r
JOIN adopciones a ON a.id_adopcion=r.id_adopcion
JOIN solicitudes_adopcion s ON s.id_solicitud=a.id_solicitud
JOIN mascotas m ON m.id_mascota=s.id_mascota
JOIN organizaciones org ON org.id_organizacion=m.id_organizacion
JOIN usuarios u ON u.id_usuario=s.id_adoptante;
