import Breadcrumbs from "../ui/Breadcrumbs";

export default function PageHeader({ title, description, breadcrumbs = [], actions }) {
  return (
    <header className="page-header">
      <div className="page-header__text">
        <Breadcrumbs items={breadcrumbs} className="page-header__crumbs" />
        <h1>{title}</h1>
        {description && <p>{description}</p>}
      </div>
      {actions && <div className="page-header__actions">{actions}</div>}
    </header>
  );
}