import StatCard from "../../../components/ui/StatCard";
import { useAsync } from "../../../hooks/useAsync";
import ReporteLayout from "../components/ReporteLayout";
import { obtenerReporteAdopciones } from "../services/reportesService";

export default function ReporteAdopcionesPage() {
  const { data: r, loading, error } = useAsync(obtenerReporteAdopciones, []);

  return (
    <ReporteLayout
      title="Reporte de adopciones"
      description="Analiza las adopciones realizadas durante un periodo."
      loading={loading}
      error={error}
      columns={3}
      chartTitle="Reporte de adopciones"
      chartNote="Adopciones completadas en los últimos 8 meses."
      unit=" adopciones"
      serie={r?.porMes}
      cards={
        r && [
          <StatCard key="t" label="Adopciones totales" value={r.total} hint="Desde el inicio" />,
          <StatCard key="m" label="Este mes" value={r.esteMes} hint="Adopciones completadas" />,
          <StatCard key="p" label="Promedio mensual" value={r.promedioMensual} hint="Últimos 12 meses" />,
        ]
      }
    />
  );
}
