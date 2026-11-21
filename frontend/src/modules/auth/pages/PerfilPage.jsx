import { useEffect, useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import Avatar from "../../../components/ui/Avatar";
import Badge from "../../../components/ui/Badge";
import Breadcrumbs from "../../../components/ui/Breadcrumbs";
import Card from "../../../components/ui/Card";
import EmptyState from "../../../components/ui/EmptyState";
import IconButton from "../../../components/ui/IconButton";
import { BuildingIcon, EditIcon, UserIcon } from "../../../components/ui/Icons";
import InfoItem from "../../../components/ui/InfoItem";
import Tabs from "../../../components/ui/Tabs";
import { mediaUrl } from "../../../utils/media";
import { obtenerPerfil } from "../services/perfilService";
import "../../../styles/Perfil.css";

const fechaLarga = (iso) =>
  iso
    ? new Date(iso).toLocaleDateString("es-MX", {
        year: "numeric",
        month: "long",
        day: "numeric",
      })
    : "No especificado";

function buildTabs(perfil) {
  const org = perfil.organizacion;

  if (org) {
    const ubicacionCompleta = [org.direccion, org.ciudad, org.codigoPostal]
      .filter(Boolean)
      .join(", ");

    return [
      {
        id: "info",
        label: "Información General",
        content: (
          <div className="perfil-tab-panel">
            <dl className="info-list perfil-info-grid">
              <InfoItem label="Nombre de la organización">{org.nombre}</InfoItem>
              <InfoItem label="Tipo de organización">{org.tipoOrganizacion}</InfoItem>
              <InfoItem label="Miembro desde">{fechaLarga(perfil.fechaRegistro)}</InfoItem>
              <InfoItem label="Correo de acceso">{perfil.correo}</InfoItem>
              <InfoItem label="Descripción" className="perfil-info-item--full">
                {org.descripcion || "Sin descripción registrada."}
              </InfoItem>
            </dl>
          </div>
        ),
      },
      {
        id: "ubicacion",
        label: "Ubicación",
        content: (
          <div className="perfil-tab-panel">
            <dl className="info-list perfil-info-grid">
              <InfoItem label="Dirección">{org.direccion}</InfoItem>
              <InfoItem label="Ciudad">{org.ciudad}</InfoItem>
              <InfoItem label="Código postal">{org.codigoPostal || "No especificado"}</InfoItem>
              <InfoItem label="Referencia completa">{ubicacionCompleta || "No disponible"}</InfoItem>
            </dl>
          </div>
        ),
      },
      {
        id: "contacto",
        label: "Contacto",
        content: (
          <div className="perfil-tab-panel">
            <dl className="info-list perfil-info-grid">
              <InfoItem label="Teléfono">{org.telefono}</InfoItem>
              <InfoItem label="Correo de contacto">
                {org.correoContacto || perfil.correo}
              </InfoItem>
              <InfoItem label="Sitio web" className="perfil-info-item--full">
                {org.sitioWeb ? (
                  <a
                    href={org.sitioWeb}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="perfil-link"
                  >
                    {org.sitioWeb}
                  </a>
                ) : (
                  "No registrado"
                )}
              </InfoItem>
            </dl>
          </div>
        ),
      },
    ];
  }

  return [
    {
      id: "info",
      label: "Información Personal",
      content: (
        <div className="perfil-tab-panel">
          <dl className="info-list perfil-info-grid">
            <InfoItem label="Nombre">{perfil.nombre}</InfoItem>
            <InfoItem label="Apellido paterno">{perfil.apellidoPaterno}</InfoItem>
            <InfoItem label="Apellido materno">
              {perfil.apellidoMaterno || "No especificado"}
            </InfoItem>
            <InfoItem label="Miembro desde">{fechaLarga(perfil.fechaRegistro)}</InfoItem>
          </dl>
        </div>
      ),
    },
    {
      id: "contacto",
      label: "Contacto",
      content: (
        <div className="perfil-tab-panel">
          <dl className="info-list perfil-info-grid">
            <InfoItem label="Teléfono">{perfil.telefono || "No especificado"}</InfoItem>
            <InfoItem label="Correo electrónico">{perfil.correo}</InfoItem>
          </dl>
        </div>
      ),
    },
  ];
}

export default function PerfilPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const [perfil, setPerfil] = useState(null);
  const [error, setError] = useState("");
  const [mensaje] = useState(location.state?.mensaje ?? "");

  useEffect(() => {
    if (location.state?.mensaje) {
      navigate(location.pathname, { replace: true, state: null });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    let cancelled = false;
    obtenerPerfil()
      .then((data) => !cancelled && setPerfil(data))
      .catch((err) => !cancelled && setError(err.message));
    return () => {
      cancelled = true;
    };
  }, []);

  const crumbs = [{ label: "Inicio", to: "/inicio" }, { label: "Ver perfil" }];
  const esOrg = Boolean(perfil?.organizacion);
  const tabs = useMemo(() => (perfil ? buildTabs(perfil) : []), [perfil]);

  if (error) {
    return (
      <div className="perfil-page">
        <Breadcrumbs items={crumbs} className="perfil-crumbs" />
        <Alert variant="error">{error}</Alert>
      </div>
    );
  }

  if (!perfil) {
    return (
      <div className="perfil-page">
        <Breadcrumbs items={crumbs} className="perfil-crumbs" />
        <EmptyState title="Cargando perfil…" />
      </div>
    );
  }

  return (
    <div className="perfil-page">
      <Breadcrumbs items={crumbs} className="perfil-crumbs" />

      {mensaje && (
        <Alert variant="success" className="perfil-alert">
          {mensaje}
        </Alert>
      )}

      <Card className="perfil-card">
        <div className="perfil-banner" role="img" aria-label="Portada del perfil de Adopta+">
          <div className="perfil-banner__overlay" />
        </div>

        <div className="perfil-head">
          <Avatar
            name={perfil.nombreCompleto}
            src={mediaUrl(perfil.fotoUrl)}
            size="xl"
            className="perfil-head__avatar"
          />

          <div className="perfil-head__info">
            <div className="perfil-head__title-row">
              <h1 className="perfil-head__name">{perfil.nombreCompleto}</h1>
              <IconButton
                label="Editar perfil"
                size="sm"
                className="perfil-head__edit-btn"
                onClick={() => navigate("/perfil/editar")}
              >
                <EditIcon />
              </IconButton>
            </div>
            <p className="perfil-head__email">{perfil.correo}</p>
            {esOrg && perfil.organizacion?.ciudad && (
              <p className="perfil-head__meta">
                {perfil.organizacion.tipoOrganizacion} · {perfil.organizacion.ciudad}
              </p>
            )}
          </div>

          <Badge
            icon={esOrg ? <BuildingIcon /> : <UserIcon />}
            className="perfil-head__badge"
          >
            {esOrg ? "Organización" : perfil.rol}
          </Badge>
        </div>

        <div className="perfil-meta-strip">
          <div className="perfil-meta-strip__item">
            <span className="perfil-meta-strip__label">Rol en plataforma</span>
            <strong className="perfil-meta-strip__value">
              {esOrg ? "Organización" : perfil.rol}
            </strong>
          </div>
          <div className="perfil-meta-strip__item">
            <span className="perfil-meta-strip__label">Registro</span>
            <strong className="perfil-meta-strip__value">
              {fechaLarga(perfil.fechaRegistro)}
            </strong>
          </div>
          <div className="perfil-meta-strip__item">
            <span className="perfil-meta-strip__label">Teléfono principal</span>
            <strong className="perfil-meta-strip__value">
              {esOrg
                ? perfil.organizacion?.telefono || "No registrado"
                : perfil.telefono || "No registrado"}
            </strong>
          </div>
        </div>

        <Tabs tabs={tabs} className="perfil-tabs" />
      </Card>
    </div>
  );
}