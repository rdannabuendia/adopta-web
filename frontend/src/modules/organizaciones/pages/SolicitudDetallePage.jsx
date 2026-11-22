import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import BaseTextarea from "../../../components/ui/BaseTextarea";
import Modal from "../../../components/ui/Modal";
import PageHeader from "../../../components/layout/PageHeader";
import { useAsync } from "../../../hooks/useAsync";
import {
  aprobarSolicitud,
  obtenerOrganizacion,
  rechazarSolicitud,
} from "../services/organizacionesService";
import OrganizacionDetalleView from "./OrganizacionDetalleView";
import "../../../styles/Organizaciones.css";

export default function SolicitudDetallePage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data: org, loading, error } = useAsync(() => obtenerOrganizacion(id), [id]);

  const [modal, setModal] = useState(null); // "aprobar" | "rechazar" | null
  const [motivo, setMotivo] = useState("");
  const [motivoError, setMotivoError] = useState("");
  const [busy, setBusy] = useState(false);
  const [actionError, setActionError] = useState("");

  const cerrar = () => {
    if (busy) return;
    setModal(null);
    setMotivo("");
    setMotivoError("");
  };

  const terminar = (res) =>
    navigate("/organizaciones/solicitudes", { state: { mensaje: res.mensaje } });

  const aprobar = async () => {
    setBusy(true);
    setActionError("");
    try {
      terminar(await aprobarSolicitud(id));
    } catch (err) {
      setActionError(err.message);
      setBusy(false);
      setModal(null);
    }
  };

  const rechazar = async () => {
    const texto = motivo.trim();
    if (texto.length < 10) return setMotivoError("Explica el motivo (mínimo 10 caracteres).");
    setBusy(true);
    setActionError("");
    try {
      terminar(await rechazarSolicitud(id, texto));
    } catch (err) {
      setActionError(err.message);
      setBusy(false);
      setModal(null);
    }
  };

  const pendiente = org?.estado === "Pendiente";

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Organizaciones", to: "/organizaciones" },
          { label: "Solicitudes de registro", to: "/organizaciones/solicitudes" },
          { label: "Detalle" },
        ]}
        title="Detalle de solicitud"
        description="Revisa la información y los documentos antes de resolver."
      />

      {error && <Alert variant="error" className="org-alert">{error}</Alert>}
      {actionError && <Alert variant="error" className="org-alert">{actionError}</Alert>}
      {loading && !org && <p className="org-state">Cargando…</p>}
      {org && !pendiente && (
        <Alert variant="info" className="org-alert">Esta solicitud ya fue resuelta.</Alert>
      )}

      {org && (
        <OrganizacionDetalleView
          org={org}
          actions={
            pendiente && (
              <>
                <BaseButton onClick={() => setModal("aprobar")}>Aprobar solicitud</BaseButton>
                <BaseButton variant="secondary" onClick={() => setModal("rechazar")}>Rechazar</BaseButton>
              </>
            )
          }
        />
      )}

      <Modal
        open={modal === "aprobar"}
        title="Aprobar solicitud"
        onClose={cerrar}
        footer={
          <>
            <BaseButton variant="secondary" onClick={cerrar} disabled={busy}>Cancelar</BaseButton>
            <BaseButton onClick={aprobar} disabled={busy}>{busy ? "Aprobando…" : "Aprobar"}</BaseButton>
          </>
        }
      >
        <p>
          Se activará la cuenta de <strong>{org?.nombre}</strong> y se enviará un correo para que cree su contraseña.
        </p>
      </Modal>

      <Modal
        open={modal === "rechazar"}
        title="Rechazar solicitud"
        onClose={cerrar}
        footer={
          <>
            <BaseButton variant="secondary" onClick={cerrar} disabled={busy}>Cancelar</BaseButton>
            <BaseButton onClick={rechazar} disabled={busy}>{busy ? "Rechazando…" : "Rechazar"}</BaseButton>
          </>
        }
      >
        <BaseTextarea
          label="Motivo del rechazo"
          placeholder="Se enviará al solicitante por correo"
          maxLength={500}
          showCount
          value={motivo}
          onChange={(e) => {
            setMotivo(e.target.value);
            setMotivoError("");
          }}
          error={motivoError}
          required
        />
        <p className="org-state">La solicitud y sus documentos se eliminarán.</p>
      </Modal>
    </>
  );
}