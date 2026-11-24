import EmptyState from "../ui/EmptyState";
import PageHeader from "./PageHeader";

export default function PlaceholderPage({ title, description, breadcrumbs }) {
  return (
    <>
      <PageHeader
        title={title}
        description={description}
        breadcrumbs={breadcrumbs ?? [{ label: "Inicio", to: "/inicio" }, { label: title }]}
      />
      <EmptyState title="Módulo en construcción" description="Esta sección estará disponible pronto." />
    </>
  );
}