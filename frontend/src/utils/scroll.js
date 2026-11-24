// Desplazamiento suave a una sección de la landing, descontando el alto del navbar fijo.
export function scrollToSection(id, event) {
  const el = document.getElementById(id);
  if (!el) return; // si no existe, deja que el navegador haga su comportamiento normal

  event?.preventDefault();

  const header = document.querySelector(".header");
  const offset = header ? header.offsetHeight : 0;
  const top = el.getBoundingClientRect().top + window.scrollY - offset;

  window.scrollTo({ top: Math.max(top, 0), behavior: "smooth" });

  try {
    window.history.replaceState(null, "", `#${id}`);
  } catch {
    /* no es crítico */
  }
}
