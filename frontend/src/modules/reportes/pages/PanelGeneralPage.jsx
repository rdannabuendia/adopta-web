import StatCard from "../../../components/ui/StatCard";
import { useAsync } from "../../../hooks/useAsync";
import ReporteLayout from "../components/ReporteLayout";
import { obtenerPanel } from "../services/reportesService";

export default function PanelGeneralPage() {
  const { data: p, loading, error } = useAsync(obtenerPanel, []);

  return (
    <ReporteLayout
      title="Panel de estadísticas generales"
      description="Resumen de la actividad de Adopta+ en un solo lugar."
      loading={loading}
      error={error}
      chartTitle="Actividad mensual"
      chartNote="Solicitudes de adopción recibidas en los últimos 8 meses."
      unit=" solicitudes"
      serie={p?.actividadMensual}
      cards={
        p && [
          <StatCard key="org" to="/organizaciones" label="Organizaciones" value={p.organizaciones} hint={`+${p.organizacionesNuevasMes} este mes`} />,
          <StatCard key="mas" to="/reportes/mascotas" label="Mascotas registradas" value={p.mascotasRegistradas} hint={`${p.mascotasDisponibles} disponibles`} />,
          <StatCard key="ado" to="/reportes/adopciones" label="Adopciones realizadas" value={p.adopcionesRealizadas} hint={`+${p.adopcionesMes} este mes`} />,
          <StatCard key="sol" to="/reportes/solicitudes" label="Solicitudes pendientes" value={p.solicitudesPendientes} hint={`${p.solicitudesPorRevisar} por revisar`} />,
        ]
      }
    />
  );
}
