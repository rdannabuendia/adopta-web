import { useEffect, useState } from "react";
import { useDebouncedValue } from "./useDebouncedValue";

export function useListFilters(emptyFilters) {
  const [filters, setFilters] = useState(emptyFilters);
  const [draft, setDraft] = useState(emptyFilters);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [showFilters, setShowFilters] = useState(false);

  const debounced = useDebouncedValue(search);

  useEffect(() => {
    if (debounced === filters.buscar) return;
    setFilters((f) => ({ ...f, buscar: debounced }));
    setPage(1);
  }, [debounced]);

  const openFilters = () => {
    setDraft(filters);
    setShowFilters(true);
  };
  const closeFilters = () => setShowFilters(false);

  const handleDraftChange = (e) => setDraft((d) => ({ ...d, [e.target.name]: e.target.value }));

  const apply = () => {
    setFilters(draft);
    setSearch(draft.buscar);
    setPage(1);
  };

  const clear = () => {
    setDraft(emptyFilters);
    setFilters(emptyFilters);
    setSearch("");
    setPage(1);
  };

  const activeCount = Object.entries(filters).filter(([k, v]) => k !== "buscar" && v).length;

  return {
    filters, draft, handleDraftChange, search, setSearch, page, setPage,
    showFilters, openFilters, closeFilters, apply, clear,
    activeCount, hasFilters: activeCount > 0 || Boolean(filters.buscar),
  };
}