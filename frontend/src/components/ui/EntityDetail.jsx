import Avatar from "./Avatar";
import Card from "./Card";
import DetailRows from "./DetailRows";
import InfoItem from "./InfoItem";
import "../../styles/EntityDetail.css";

/**
 * Props
 *  title:      título de la tarjeta principal
 *  avatar:     { name, src } (opcional)
 *  rows:       [{ label, value }]  -> tabla de la tarjeta principal
 *  mainFooter: nodo bajo la tabla (documentos, etc.)
 *  sideTitle / sideItems: [{ label, value }] -> tarjeta lateral
 *  sideFooter: nodo bajo la tarjeta lateral (alertas)
 *  actions:    botones de la pantalla
 */

export default function EntityDetail({
  title = "Información general",
  avatar,
  rows = [],
  mainFooter,
  sideTitle = "Información adicional",
  sideItems = [],
  sideFooter,
  actions,
}) {
  return (
    <div className="entity-detail">
      <Card padded className="entity-detail__main">
        <h2 className="entity-detail__title">{title}</h2>

        {avatar && (
          <div className="entity-detail__brand">
            <Avatar name={avatar.name} src={avatar.src} size="lg" />
            <strong>{avatar.name}</strong>
          </div>
        )}

        <DetailRows items={rows} />
        {mainFooter}
      </Card>

      <div className="entity-detail__side">
        {sideItems.length > 0 && (
          <Card padded>
            <h2 className="entity-detail__title">{sideTitle}</h2>
            <dl className="info-list">
              {sideItems.map((i) => (
                <InfoItem key={i.label} label={i.label}>{i.value}</InfoItem>
              ))}
            </dl>
          </Card>
        )}
        {sideFooter}
        {actions && <div className="entity-detail__actions">{actions}</div>}
      </div>
    </div>
  );
}