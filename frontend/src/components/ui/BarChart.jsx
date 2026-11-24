import "../../styles/BarChart.css";

/**
 * Gráfica de barras verticales simple (sin librerías).
 *  data: [{ label, value }]    title: nombre accesible de la gráfica
 * La última barra (mes actual) va en el tono fuerte.
 */
export default function BarChart({ data = [], title, unit = "", height = 240 }) {
  const max = Math.max(1, ...data.map((d) => d.value));
  const resumen = data.map((d) => `${d.label}: ${d.value}`).join(", ");

  return (
    <div className="bar-chart" role="img" aria-label={`${title ?? "Gráfica"}. ${resumen}`}>
      <ul className="bar-chart__plot" style={{ height }}>
        {data.map((d, i) => (
          <li key={d.label + i} className="bar-chart__col" title={`${d.label}: ${d.value}${unit}`}>
            <span className="bar-chart__value">{d.value.toLocaleString("es-MX")}</span>
            <span
              className={`bar-chart__bar${i === data.length - 1 ? " is-current" : ""}`}
              style={{ height: `${Math.max((d.value / max) * 100, d.value > 0 ? 3 : 0.8)}%` }}
            />
          </li>
        ))}
      </ul>
      <ul className="bar-chart__axis" aria-hidden="true">
        {data.map((d, i) => <li key={d.label + i}>{d.label}</li>)}
      </ul>
    </div>
  );
}
