import Alert from "../../../components/ui/Alert";
import DocumentList from "../../../components/ui/DocumentList";
import EntityDetail from "../../../components/ui/EntityDetail";
import SocialLinks from "../../../components/ui/SocialLinks";
import StatusBadge from "../../../components/ui/StatusBadge";
import { formatDate, plural } from "../../../utils/format";
import { mediaUrl } from "../../../utils/media";
import { abrirDocumento } from "../services/organizacionesService";

const TIPO_DOCUMENTO = {
  ExistenciaLegal: "Comprobante de existencia legal",
  ComprobanteDomicilio: "Comprobante de domicilio",
  IdentificacionResponsable: "Identificación del responsable",
};

/** Sirve para organizaciones y solicitudes. actions: botones de la pantalla. */
export default function OrganizacionDetalleView({ org, actions }) {
  const direccion = [org.direccion, org.ciudad].filter(Boolean).join(", ");
  const redes = [
    { type: "facebook", url: org.facebook },
    { type: "instagram", url: org.instagram },
    { type: "youtube", url: org.youtube },
  ].filter((r) => r.url);

  return (
    <EntityDetail
      avatar={{ name: org.nombre, src: mediaUrl(org.fotoUrl) }}
      rows={[
        { label: "Nombre", value: org.nombre },
        { label: "Correo", value: org.correo },
        { label: "Teléfono", value: org.telefono },
        { label: "Dirección", value: direccion },
        { label: "Estado", value: <StatusBadge status={org.estado} /> },
      ]}
      mainFooter={
        <DocumentList
          documents={org.documentos}
          labels={TIPO_DOCUMENTO}
          onView={(d) => abrirDocumento(d.idDocumento)}
        />
      }
      sideItems={[
        { label: "Descripción", value: org.descripcion },
        { label: "Tipo de organización", value: org.tipoOrganizacion },
        { label: "Redes sociales", value: redes.length ? <SocialLinks links={redes} /> : null },
        {
          label: "Sitio web",
          value: org.sitioWeb ? (
            <a href={org.sitioWeb} target="_blank" rel="noopener noreferrer">{org.sitioWeb}</a>
          ) : null,
        },
        { label: "Horario de atención", value: org.horarioAtencion },
        { label: "Registrada el", value: formatDate(org.fechaRegistro) },
        {
          label: "Mascotas registradas",
          value: <span className="entity-detail__count">{plural(org.totalMascotas, "mascota")}</span>,
        },
      ]}
      sideFooter={
        org.baja && (
          <Alert variant="error">
            <strong>Dada de baja el {formatDate(org.baja.fechaRevision)}</strong>
            <br />
            Motivo: {org.baja.motivo}
            <br />
            {org.baja.descripcion}
          </Alert>
        )
      }
      actions={actions}
    />
  );
}