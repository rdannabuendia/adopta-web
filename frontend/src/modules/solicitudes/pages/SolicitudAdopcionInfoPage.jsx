import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import BaseTextarea from "../../../components/ui/BaseTextarea";
import Card from "../../../components/ui/Card";
import PageHeader from "../../../components/layout/PageHeader";
import { MAX_TEXTO, MIN_TEXTO } from "../../../constants/solicitud";
import { useAsync } from "../../../hooks/useAsync";
import { obtenerSolicitud, solicitarInformacion } from "../services/solicitudesService";
import "../../../styles/Solicitudes.css";

const LISTA = "/solicitudes";

/** Antes de resolver: pide al adoptante un dato o documento que falta. */
export default function SolicitudAdopcionInfoPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const detalle = `${LISTA}/${id}`;

  const { data: d, error: loadError } = useAsync(() => obtenerSolicitud(id), [id]);

  const [mensaje, setMensaje] = useState("");
  const [fieldError, setFieldError] = useState("");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    const texto = mensaje.trim();
    if (texto.length < MIN_TEXTO) return setFieldError(`Escribe el mensaje para el adoptante (mínimo ${MIN_TEXTO} caracteres).`);

    setSaving(true);
    try {
      const res = await solicitarInformacion(id, { mensaje: texto });
      navigate(detalle, { state: { mensaje: res.mensaje } });
    } catch (err) {
      setError(err.message);
      setSaving(false);
    }
  };

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Adopción", to: LISTA },
          { label: "Detalle Solicitud", to: detalle },
          { label: "Información Adicional" },
        ]}
        title="Solicitud de Información Adicional"
        description="Indica los documentos o datos faltantes que el adoptante debe adjuntar para continuar con el proceso."
      />

      {loadError && <Alert variant="error" className="sol-alert">{loadError}</Alert>}
      {error && <Alert variant="error" className="sol-alert">{error}</Alert>}

      {d?.esFinal && (
        <Alert variant="info" className="sol-alert">
          Esta solicitud ya fue resuelta; no se puede pedir más información. <Link to={detalle}>Ver detalle</Link>
        </Alert>
      )}

      {d && !d.esFinal && (
        <Card padded className="sol-info">
          <h2 className="sol-info__title">Información Adicional</h2>
          <p className="sol-info__lead">
            Puedes solicitar más información al adoptante antes de cambiar el estado de la solicitud.
          </p>
          <p className="sol-info__context">
            Solicitud de <strong>{d.adoptante.nombre}</strong> para adoptar a <strong>{d.mascota.nombre}</strong>.
          </p>

          <form className="sol-info__form" onSubmit={handleSubmit} noValidate>
            <BaseTextarea
              label="Mensaje"
              required
              placeholder="Escribe aquí..."
              rows={6}
              maxLength={MAX_TEXTO}
              showCount
              value={mensaje}
              onChange={(e) => { setMensaje(e.target.value); setFieldError(""); }}
              error={fieldError}
            />

            <div className="sol-form-actions sol-form-actions--center">
              <BaseButton type="submit" loading={saving}>Enviar</BaseButton>
              <BaseButton variant="secondary" to={detalle} disabled={saving}>Cancelar</BaseButton>
            </div>
          </form>
        </Card>
      )}
    </>
  );
}
