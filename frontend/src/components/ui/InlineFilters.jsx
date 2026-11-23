import BaseButton from "./BaseButton";
import BaseInput from "./BaseInput";
import BaseSelect from "./BaseSelect";
import SearchInput from "./SearchInput";
import "../../styles/InlineFilters.css";

/**
 * filters: [{ name, label, value, options?, type? }] — se aplican al cambiar cada control.
 *  type "select" (por defecto, con options) o "date" (selector de fecha).
 */
export default function InlineFilters({
  search, onSearchChange, searchPlaceholder = "Buscar…",
  filters = [], onFilterChange, onClear, canClear = false,
}) {
  return (
    <div className="inline-filters">
      <SearchInput
        className="inline-filters__search"
        value={search}
        onChange={onSearchChange}
        placeholder={searchPlaceholder}
      />
      {filters.map((f) =>
        f.type === "date" ? (
          <BaseInput
            key={f.name}
            type="date"
            className="inline-filters__select inline-filters__date"
            fullWidth={false}
            aria-label={f.label}
            title={f.label}
            name={f.name}
            value={f.value}
            onChange={(e) => onFilterChange(f.name, e.target.value)}
          />
        ) : (
          <BaseSelect
            key={f.name}
            className="inline-filters__select"
            fullWidth={false}
            aria-label={f.label}
            name={f.name}
            placeholder={f.label}
            options={f.options}
            value={f.value}
            onChange={(e) => onFilterChange(f.name, e.target.value)}
          />
        )
      )}
      {canClear && (
        <BaseButton variant="secondary" size="sm" onClick={onClear}>Limpiar</BaseButton>
      )}
    </div>
  );
}
