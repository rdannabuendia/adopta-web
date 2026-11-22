import BaseButton from "../ui/BaseButton";
import "../../styles/Pagination.css";

export default function Pagination({ page, totalPages, total, pageSize, onChange }) {
  if (!total) return null;
  const from = (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, total);

  return (
    <nav className="pagination" aria-label="Paginación">
      <span className="pagination__info">Mostrando {from}–{to} de {total}</span>

      {totalPages > 1 && (
        <div className="pagination__controls">
          <BaseButton variant="secondary" size="xs" disabled={page <= 1} onClick={() => onChange(page - 1)}>
            Anterior
          </BaseButton>
          <span aria-current="page">Página {page} de {totalPages}</span>
          <BaseButton variant="secondary" size="xs" disabled={page >= totalPages} onClick={() => onChange(page + 1)}>
            Siguiente
          </BaseButton>
        </div>
      )}
    </nav>
  );
}