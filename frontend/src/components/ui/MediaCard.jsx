import "../../styles/MediaCard.css";

/**
 * Tarjeta con imagen arriba (mascotas, organizaciones, etc.)
 *  image / imageAlt / fit ("cover" | "contain") / placeholder: nodo si no hay imagen
 *  badge: nodo sobre la imagen · title · subtitle · meta: nodo · footer: botones
 */
export default function MediaCard({
  image, imageAlt = "", fit = "cover", placeholder, badge,
  title, subtitle, meta, footer, className = "",
}) {
  return (
    <article className={["media-card", className].filter(Boolean).join(" ")}>
      <div className={`media-card__media media-card__media--${fit}`}>
        {image ? (
          <img src={image} alt={imageAlt} loading="lazy" />
        ) : (
          <span className="media-card__placeholder" aria-hidden="true">{placeholder}</span>
        )}
        {badge && <span className="media-card__badge">{badge}</span>}
      </div>

      <div className="media-card__body">
        <h3 className="media-card__title">{title}</h3>
        {subtitle && <p className="media-card__subtitle">{subtitle}</p>}
        {meta && <div className="media-card__meta">{meta}</div>}
        {footer && <div className="media-card__footer">{footer}</div>}
      </div>
    </article>
  );
}