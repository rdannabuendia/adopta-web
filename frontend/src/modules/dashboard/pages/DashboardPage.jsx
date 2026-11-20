import PageHeader from "../../../components/layout/PageHeader";
import { getVisibleNav } from "../../../components/layout/navigation";
import LinkCard from "../../../components/ui/LinkCard";
import { getUser } from "../../auth/services/authService";

export default function DashboardPage() {
  const user = getUser();
  const firstName = user?.nombre?.trim().split(/\s+/)[0] ?? "";

  const shortcuts = getVisibleNav(user?.rol)
    .filter((i) => i.description)
    .map((i) => ({ ...i, to: i.children ? i.children[0].to : i.to }));

  return (
    <>
      <PageHeader
        breadcrumbs={[{ label: "Inicio" }]}
        title={`¡Hola, ${firstName}!`}
        description="Estos son tus accesos rápidos."
      />

      <div className="quick-grid">
        {shortcuts.map((s) => (
          <LinkCard key={s.to} to={s.to} icon={s.icon} title={s.label} description={s.description} />
        ))}
      </div>
    </>
  );
}