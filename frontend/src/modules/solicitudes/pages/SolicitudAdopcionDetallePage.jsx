import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import Avatar from "../../../components/ui/Avatar";
import BaseButton from "../../../components/ui/BaseButton";
import Card from "../../../components/ui/Card";
import DetailRows from "../../../components/ui/DetailRows";
import StatusBadge, { statusLabel } from "../../../components/ui/StatusBadge";
import Timeline from "../../../components/ui/Timeline";
import PageHeader from "../../../components/layout/PageHeader";
import { siNo } from "../../../constants/solicitud";
import { useAsync } from "../../../hooks/useAsync";
import { useFlashMessage } from "../../../hooks/useFlashMessage";
import { formatDate } from "../../../utils/format";
import { mediaUrl } from "../../../utils/media";
import DocumentosSolicitudModal from "../components/DocumentosSolicitudModal";
import { obtenerSolicitud } from "../services/solicitudesService";
import "../../../styles/Solicitudes.css";

const LISTA = "/solicitudes";

const timelineItems = (historial) =>
  historial.map((h, i) => ({
    id: `${h.estado}-${h.fecha}-${i}`,
    // El primer movimiento siempre es el envío de la solicitud
    label: i === 0 ? "Solicitud enviada" : statusLabel(h.estado),
    date: formatDate(h.fecha),
    detail: h.observacion,
  }));

const variantResolucion = (estado) => (/rechaz/i.test(estado) ? "error" : /cancel/i.test(estado) ? "info" : "success");

function filasHogar(d) {
  const h = d.hogar;
  return [
    { label: "Experiencia previa", value: d.experienciaPrevia },
    { label: "Condiciones de vivienda", value: d.condicionesVivienda },
    ...(h
      ? [
          { label: "Régimen de vivienda", value: h.regimenVivienda },
          { label: "Arrendador permite mascotas", value: siNo(h.arrendadorPermiteMascotas) },
          { label: "Espacio exterior", value: h.tieneEspacioExterior ? `Sí${h.espacioExteriorCercado ? " · cercado" : ""}` : "No" },
          { label: "Personas en el hogar", value: `${h.numPersonasHogar}${h.hayMenores ? " · hay menores" : ""}` },
          { label: "Hogar de acuerdo", value: siNo(h.hogarDeAcuerdo) },
          { label: "Horas solo al día", value: h.horasSoloDia },
          { label: "Lugar para dormir", value: h.lugarDormir },
          { label: "Cuidador alterno", value: h.cuidadorAlterno },
          { label: "Presupuesto mensual", value: h.presupuestoMensual },
          { label: "Otras mascotas", value: h.tieneOtrasMascotas ? h.especiesOtrasMascotas || "Sí" : "No" },
          { label: "Acepta visita domiciliaria", value: siNo(h.aceptaVisitaDomiciliaria) },
          { label: "Acepta seguimiento", value: siNo(h.aceptaSeguimiento) },
        ]
      : []),
  ].filter((r) => r.value !== null && r.value !== undefined && r.value !== "");
}

export default function SolicitudAdopcionDetallePage() {
  const { id } = useParams();
  const mensaje = useFlashMessage();
  const { data: d, loading, error } = useAsync(() => obtenerSolicitud(id), [id]);
  const [verDocumentos, setVerDocumentos] = useState(false);

  const hogar = d ? filasHogar(d) : [];

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Adopción", to: LISTA },
          { label: "Detalle Solicitud" },
        ]}
        title="Detalle Solicitud de Adopción"
        description="Revisa la información del adoptante, los documentos adjuntos y el estado actual del trámite."
      />

      {mensaje && <Alert variant="success" className="sol-alert">{mensaje}</Alert>}
      {error && <Alert variant="error" className="sol-alert">{error}</Alert>}
      {loading && !d && <p className="sol-state">Cargando…</p>}

      {d && (
        <>
          {d.esFinal && (
            <Alert variant={variantResolucion(d.estado)} className="sol-alert">
              Solicitud {statusLabel(d.estado).toLowerCase()}
              {d.fechaResolucion && ` el ${formatDate(d.fechaResolucion)}`}.
              {d.observacionesOrganizacion && ` Observaciones: ${d.observacionesOrganizacion}`}
            </Alert>
          )}

          <Card padded className="sol-detail">
            <div className="sol-detail__grid">
              <div className="sol-detail__main">
                <div className="sol-detail__person">
                  <div className="sol-detail__contact">
                    <h2>{d.adoptante.nombre}</h2>
                    {d.adoptante.correo && <a href={`mailto:${d.adoptante.correo}`}>{d.adoptante.correo}</a>}
                    {d.adoptante.telefono && <span>{d.adoptante.telefono}</span>}
                  </div>
                  <Avatar name={d.adoptante.nombre} src={mediaUrl(d.adoptante.fotoUrl)} size="xl" />
                </div>

                <section>
                  <h3 className="sol-section-title">Información de la solicitud</h3>
                  <DetailRows
                    items={[
                      { label: "Mascota", value: <Link to={`/mascotas/${d.mascota.idMascota}`} className="sol-link">{d.mascota.nombre}</Link> },
                      { label: "Fecha de Solicitud", value: formatDate(d.fechaSolicitud) },
                      { label: "Estatus", value: <StatusBadge status={d.estado} /> },
                      ...(!d.esFinal && d.fechaLimiteDocumentos
                        ? [{ label: "Entrega de documentos", value: `Hasta el ${formatDate(d.fechaLimiteDocumentos)}` }]
                        : []),
                    ]}
                  />
                </section>

                <section>
                  <h3 className="sol-section-title">Mensaje del Adoptante</h3>
                  {d.motivoAdopcion ? (
                    <blockquote className="sol-quote">“{d.motivoAdopcion}”</blockquote>
                  ) : (
                    <p className="sol-state">El adoptante no dejó un mensaje.</p>
                  )}
                </section>
              </div>

              <aside className="sol-detail__side">
                <h3 className="sol-detail__side-title">Historial Solicitud</h3>
                <Timeline items={timelineItems(d.historial)} />

                <div className="sol-detail__side-actions">
                  <BaseButton fullWidth onClick={() => setVerDocumentos(true)}>Ver Documentos</BaseButton>
                  {!d.esFinal && (
                    <BaseButton fullWidth variant="secondary" to={`${LISTA}/${d.idSolicitud}/informacion`}>
                      Solicitar Información Adicional
                    </BaseButton>
                  )}
                </div>
              </aside>
            </div>

            <div className="sol-detail__footer">
              {!d.esFinal && (
                <BaseButton to={`${LISTA}/${d.idSolicitud}/resolver`}>Resolver Solicitud</BaseButton>
              )}
              <BaseButton variant="secondary" to={LISTA}>Cerrar</BaseButton>
            </div>
          </Card>

          {hogar.length > 0 && (
            <Card padded className="sol-detail sol-extra">
              <h3 className="sol-section-title">Información del adoptante</h3>
              <DetailRows items={hogar} />

              {d.referencias.length > 0 && (
                <>
                  <h3 className="sol-section-title sol-extra__refs">Referencias personales</h3>
                  <DetailRows
                    items={d.referencias.map((r) => ({
                      label: r.nombre,
                      value: [r.relacion, r.telefono, r.aniosConocidos != null && `${r.aniosConocidos} años de conocerse`]
                        .filter(Boolean)
                        .join(" · "),
                    }))}
                  />
                </>
              )}
            </Card>
          )}

          <DocumentosSolicitudModal open={verDocumentos} idSolicitud={d.idSolicitud} onClose={() => setVerDocumentos(false)} />
        </>
      )}
    </>
  );
}
