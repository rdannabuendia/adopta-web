import StatCard from "../../../components/ui/StatCard";
import { useAsync } from "../../../hooks/useAsync";
import ReporteLayout from "../components/ReporteLayout";
import { obtenerReporteMascotas } from "../services/reportesService";

export default function ReporteMascotasPage() {
  const { data: r, loading, error } = useAsync(obtenerReporteMascotas, []);

  return (
    <ReporteLayout
      title="Estadísticas de mascotas"
      description="Distribución por disponibilidad y estado."
      loading={loading}
      error={error}
      columns={3}
      chartTitle="Estadísticas de mascotas"
      chartNote="Mascotas registradas en los últimos 8 meses."
      unit=" mascotas"
      serie={r?.porMes}
      cards={
        r && [
          <StatCard key="d" label="Disponibles" value={r.disponibles} hint={`${r.porcentajeDisponibles}% del total`} />,
          <StatCard key="e" label="En proceso" value={r.enProceso} hint={`${r.porcentajeEnProceso}% del total`} />,
          <StatCard key="a" label="Adoptadas" value={r.adoptadas} hint={`${r.porcentajeAdoptadas}% del total`} />,
        ]
      }
    />
  );
}
