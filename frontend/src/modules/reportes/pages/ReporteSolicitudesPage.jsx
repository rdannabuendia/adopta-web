import StatCard from "../../../components/ui/StatCard";
import { useAsync } from "../../../hooks/useAsync";
import ReporteLayout from "../components/ReporteLayout";
import { obtenerReporteSolicitudes } from "../services/reportesService";

export default function ReporteSolicitudesPage() {
  const { data: r, loading, error } = useAsync(obtenerReporteSolicitudes, []);

  return (
    <ReporteLayout
      title="Reporte de solicitudes"
      description="Consulta el estado y volumen de solicitudes."
      loading={loading}
      error={error}
      columns={3}
      chartTitle="Reporte de solicitudes"
      chartNote="Solicitudes recibidas en los últimos 8 meses."
      unit=" solicitudes"
      serie={r?.porMes}
      cards={
        r && [
          <StatCard key="a" label="Aprobadas" value={r.aprobadas} hint={`${r.porcentajeAprobadas}% del total`} />,
          <StatCard key="p" label="Pendientes" value={r.pendientes} hint="Por revisar" />,
          <StatCard key="r" label="Rechazadas" value={r.rechazadas} hint={`${r.porcentajeRechazadas}% del total`} />,
        ]
      }
    />
  );
}
