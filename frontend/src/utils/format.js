export const formatDate = (iso) =>
  iso ? new Date(iso).toLocaleDateString("es-MX", { day: "numeric", month: "short", year: "numeric" }) : null;

export const plural = (n, singular, pluralForm) => `${n} ${n === 1 ? singular : pluralForm ?? `${singular}s`}`;