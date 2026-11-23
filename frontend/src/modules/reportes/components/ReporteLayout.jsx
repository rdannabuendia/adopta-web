import Alert from "../../../components/ui/Alert";
import BarChart from "../../../components/ui/BarChart";
import Card from "../../../components/ui/Card";
import { StatGrid } from "../../../components/ui/StatCard";
import PageHeader from "../../../components/layout/PageHeader";
import { REPORTES_BREADCRUMB } from "../../../constants/reportes";
import ReportesNav from "./ReportesNav";
import "../../../styles/Reportes.css";

/**
 * Estructura común de los reportes: encabezado, menú de reportes, tarjetas de indicadores y gráfica mensual.
 * Las pantallas solo aportan título, textos y los datos.
 */
export default function ReporteLayout({ title, description, loading, error, columns = 4, cards, chartTitle, chartNote, serie, unit, children }) {
  return (
    <>
      <PageHeader breadcrumbs={REPORTES_BREADCRUMB} title={title} description={description} />
      <ReportesNav />

      {error && <Alert variant="error" className="reportes-alert">{error}</Alert>}
      {loading && !cards && <p className="reportes-state">Cargando…</p>}

      {cards && (
        <>
          <StatGrid columns={columns}>{cards}</StatGrid>

          <Card padded className="reportes-chart">
            <h2 className="reportes-chart__title">{chartTitle}</h2>
            {chartNote && <p className="reportes-chart__note">{chartNote}</p>}
            <BarChart
              title={chartTitle}
              unit={unit}
              data={(serie ?? []).map((s) => ({ label: s.etiqueta, value: s.valor }))}
            />
          </Card>

          {children}
        </>
      )}
    </>
  );
}
