import { useMemo } from "react";
import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import DocumentList from "../../../components/ui/DocumentList";
import Modal from "../../../components/ui/Modal";
import StatusBadge from "../../../components/ui/StatusBadge";
import { useAsync } from "../../../hooks/useAsync";
import { abrirArchivoSolicitud, listarDocumentosSolicitud } from "../services/solicitudesService";

/** Documentos que el adoptante adjuntó a la solicitud (y los que aún faltan). */
export default function DocumentosSolicitudModal({ open, idSolicitud, onClose }) {
  const { data: requisitos, loading, error } = useAsync(
    () => (open ? listarDocumentosSolicitud(idSolicitud) : Promise.resolve(null)),
    [open, idSolicitud]
  );

  // DocumentList espera { idDocumento, tipo, nombreOriginal }: un renglón por archivo
  const documentos = useMemo(
    () =>
      (requisitos ?? []).flatMap((r) =>
        r.archivos.map((a) => ({ idDocumento: a.idArchivo, tipo: r.tipo, nombreOriginal: a.nombreOriginal }))
      ),
    [requisitos]
  );
  const pendientes = (requisitos ?? []).filter((r) => r.archivos.length === 0);

  return (
    <Modal
      open={open}
      title="Documentos adjuntos"
      onClose={onClose}
      className="sol-docs-modal"
      footer={<BaseButton variant="secondary" onClick={onClose}>Cerrar</BaseButton>}
    >
      {error && <Alert variant="error">{error}</Alert>}
      {loading && !requisitos && <p>Cargando…</p>}

      {requisitos && requisitos.length === 0 && <p>Esta solicitud no tiene documentos requeridos.</p>}

      <DocumentList
        title="Archivos cargados"
        documents={documentos}
        onView={(doc) => abrirArchivoSolicitud(doc.idDocumento)}
      />

      {pendientes.length > 0 && (
        <section className="sol-docs-modal__pending">
          <h3>Aún sin cargar</h3>
          <ul>
            {pendientes.map((r) => (
              <li key={r.idRequisito}>
                <span>
                  {r.tipo}
                  {!r.obligatorio && <small> (opcional)</small>}
                  {r.motivoRechazo && <small> · {r.motivoRechazo}</small>}
                </span>
                <StatusBadge status={r.estado} />
              </li>
            ))}
          </ul>
        </section>
      )}
    </Modal>
  );
}
