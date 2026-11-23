import DetailRows from "../../../components/ui/DetailRows";
import { formatDate } from "../../../utils/format";
import { mediaUrl } from "../../../utils/media";

const CAMPOS = [
  ["comportamiento", "Comportamiento"],
  ["salud", "Salud"],
  ["alimentacion", "Alimentación"],
  ["adaptacion", "Adaptación"],
  ["observaciones", "Observaciones"],
];

/** Un reporte de seguimiento enviado por el adoptante desde la app móvil. */
export default function ReporteCard({ reporte }) {
  const filas = CAMPOS.filter(([k]) => reporte[k]?.trim()).map(([k, label]) => ({ label, value: reporte[k] }));

  return (
    <article className="report-card">
      <h3 className="report-card__date">Reporte del {formatDate(reporte.fecha)}</h3>

      {filas.length > 0 ? (
        <DetailRows items={filas} />
      ) : (
        <p className="report-card__empty">El adoptante no escribió detalles en este reporte.</p>
      )}

      {reporte.fotos.length > 0 && (
        <ul className="report-card__photos">
          {reporte.fotos.map((url) => (
            <li key={url}>
              <a href={mediaUrl(url)} target="_blank" rel="noreferrer">
                <img src={mediaUrl(url)} alt={`Foto del reporte del ${formatDate(reporte.fecha)}`} loading="lazy" />
              </a>
            </li>
          ))}
        </ul>
      )}
    </article>
  );
}
