import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import logo from "../../../../assets/img/adopta_logo.svg"; 
import "./Navbar.css";

const LINKS = [
  { label: "Inicio", id: "inicio" },
  { label: "Sobre Nosotros", id: "sobre-nosotros" },
  { label: "¿Cómo funciona?", id: "como-funciona" },
  { label: "FAQ", id: "faq" },
  { label: "Contacto", id: "contacto" },
];

export default function Navbar() {
  const [abierto, setAbierto] = useState(false);
  const [scrolled, setScrolled] = useState(false);
  const [activo, setActivo] = useState("inicio");
  const cerrar = () => setAbierto(false);

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 8);
    onScroll();
    window.addEventListener("scroll", onScroll, { passive: true });
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) =>
        entries.forEach((e) => e.isIntersecting && setActivo(e.target.id)),
      { rootMargin: "-40% 0px -55% 0px" }
    );
    LINKS.forEach(({ id }) => {
      const el = document.getElementById(id);
      if (el) observer.observe(el);
    });
    return () => observer.disconnect();
  }, []);

  return (
    <header className={`header ${scrolled ? "is-scrolled" : ""}`}>
      <div className="header__container">
        <a href="#inicio" className="header__logo" onClick={cerrar}>
          <img src={logo} alt="Adopta+" />
        </a>

        <button
          className="header__toggle"
          aria-label="Abrir menú"
          aria-expanded={abierto}
          onClick={() => setAbierto(!abierto)}
        >
          <span />
          <span />
          <span />
        </button>

        <div className={`header__menu ${abierto ? "is-open" : ""}`}>
          <nav className="header__nav">
            {LINKS.map(({ label, id }) => (
              <a
                key={id}
                href={`#${id}`}
                className={activo === id ? "is-active" : ""}
                onClick={cerrar}
              >
                {label}
              </a>
            ))}
            <button className="header__search" aria-label="Buscar">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none"
                   stroke="currentColor" strokeWidth="2" strokeLinecap="round">
                <circle cx="11" cy="11" r="7" />
                <line x1="16.5" y1="16.5" x2="21" y2="21" />
              </svg>
            </button>
          </nav>

          <div className="header__actions">
            <Link to="/login" className="btn btn--outline" onClick={cerrar}>
              Iniciar Sesión
            </Link>
            <Link to="/register" className="btn btn--solid" onClick={cerrar}>
              Registrarse
            </Link>
          </div>
        </div>
      </div>
    </header>
  );
}