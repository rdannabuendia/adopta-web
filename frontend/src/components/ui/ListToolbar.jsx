import BaseButton from "./BaseButton";
import SearchInput from "./SearchInput";
import "../../styles/ListToolbar.css";

export default function ListToolbar({
  search,
  onSearchChange,
  searchPlaceholder = "Buscar…",
  onToggleFilters,
  filtersOpen = false,
  activeFilters = 0,
  filterLabel = "Filtros",
}) {
  return (
    <div className="list-toolbar">
      <SearchInput
        className="list-toolbar__search"
        value={search}
        onChange={onSearchChange}
        placeholder={searchPlaceholder}
      />
      <BaseButton className="list-toolbar__filter" onClick={onToggleFilters} aria-expanded={filtersOpen}>
        {filterLabel}
        {activeFilters > 0 && ` (${activeFilters})`}
      </BaseButton>
    </div>
  );
}