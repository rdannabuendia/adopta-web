import BaseButton from "./BaseButton";
import Card from "./Card";
import IconButton from "./IconButton";
import { CloseIcon } from "./Icons";
import "../../styles/FilterPanel.css";

export default function FilterPanel({
  title = "Criterios de búsqueda",
  applyLabel = "Aplicar filtros",
  clearLabel = "Limpiar",
  onApply,
  onClear,
  onClose,
  children,
}) {
  const handleSubmit = (e) => {
    e.preventDefault();
    onApply?.();
  };

  return (
    <form className="filter-panel" onSubmit={handleSubmit} noValidate>
      <Card padded>
        <div className="filter-panel__head">
          <h2>{title}</h2>
          {onClose && (
            <IconButton label="Cerrar filtros" size="sm" onClick={onClose}>
              <CloseIcon />
            </IconButton>
          )}
        </div>
        <div className="filter-panel__grid">{children}</div>
      </Card>

      <div className="filter-panel__actions">
        <BaseButton variant="secondary" onClick={onClear}>{clearLabel}</BaseButton>
        <BaseButton type="submit">{applyLabel}</BaseButton>
      </div>
    </form>
  );
}