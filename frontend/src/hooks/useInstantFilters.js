import { useMemo, useState } from "react";
import { useDebouncedValue } from "./useDebouncedValue";

export function useInstantFilters(emptyFilters) {
  const [filters, setFilters] = useState(emptyFilters);
  const [search, setSearchState] = useState("");
  const [page, setPage] = useState(1);
  const buscar = useDebouncedValue(search);

  const params = useMemo(() => ({ ...filters, buscar }), [filters, buscar]);

  const setSearch = (value) => {
    setSearchState(value);
    setPage(1);
  };
  const setFilter = (name, value) => {
    setFilters((f) => ({ ...f, [name]: value }));
    setPage(1);
  };
  const clear = () => {
    setFilters(emptyFilters);
    setSearchState("");
    setPage(1);
  };

  const hasFilters = Object.values(filters).some(Boolean) || Boolean(search);

  return { params, filters, setFilter, search, setSearch, page, setPage, clear, hasFilters };
}