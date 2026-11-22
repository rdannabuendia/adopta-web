import BaseButton from "./BaseButton";
import BaseSelect from "./BaseSelect";
import SearchInput from "./SearchInput";
import "../../styles/InlineFilters.css";

/** filters: [{ name, label, options, value }] — se aplican al cambiar cada select */
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
      {filters.map((f) => (
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
      ))}
      {canClear && (
        <BaseButton variant="secondary" size="sm" onClick={onClear}>Limpiar</BaseButton>
      )}
    </div>
  );
}