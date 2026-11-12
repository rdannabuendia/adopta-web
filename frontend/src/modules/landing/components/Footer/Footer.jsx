import { useEffect, useRef, useState } from "react";
import "./Footer.css";

/* Pinta un SVG de /public con el color del texto */
const MaskIcon = ({ src, className = "" }) => (
  <span
    className={`mask-icon ${className}`}
    style={{ "--icon": `url(${src})` }}
    aria-hidden="true"
  />
);

const Svg = ({ children, className = "" }) => (
  <svg viewBox="0 0 24 24" className={className} aria-hidden="true">
    {children}
  </svg>
);

const icons = {
  mail: <Svg><path d="M20 4H4a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2zm0 4-8 5-8-5V6l8 5 8-5v2z" /></Svg>,
  phone: <Svg><path d="M6.6 10.8a15.1 15.1 0 0 0 6.6 6.6l2.2-2.2a1 1 0 0 1 1-.25c1.1.37 2.3.57 3.6.57a1 1 0 0 1 1 1V20a1 1 0 0 1-1 1A17 17 0 0 1 3 4a1 1 0 0 1 1-1h3.5a1 1 0 0 1 1 1c0 1.25.2 2.45.57 3.57a1 1 0 0 1-.25 1z" /></Svg>,
  pin: <Svg><path d="M12 2a7 7 0 0 0-7 7c0 5.25 7 13 7 13s7-7.75 7-13a7 7 0 0 0-7-7zm0 9.5A2.5 2.5 0 1 1 12 6.5a2.5 2.5 0 0 1 0 5z" /></Svg>,
  send: <Svg><path d="M2.01 21 23 12 2.01 3 2 10l15 2-15 2z" /></Svg>,
  up: <Svg><path d="M12 5l7 7-1.4 1.4L13 8.8V20h-2V8.8l-4.6 4.6L5 12z" /></Svg>,
  facebook: <Svg><path d="M13.5 22v-8h2.7l.5-3.3h-3.2V8.6c0-.9.3-1.6 1.7-1.6h1.6V4.1c-.3 0-1.3-.1-2.4-.1-2.4 0-4 1.5-4 4.1v2.6H7.2V14h2.7v8h3.6z" /></Svg>,
  instagram: (
    <Svg>
      <path d="M7.5 2h9A5.5 5.5 0 0 1 22 7.5v9a5.5 5.5 0 0 1-5.5 5.5h-9A5.5 5.5 0 0 1 2 16.5v-9A5.5 5.5 0 0 1 7.5 2zm0 2A3.5 3.5 0 0 0 4 7.5v9A3.5 3.5 0 0 0 7.5 20h9a3.5 3.5 0 0 0 3.5-3.5v-9A3.5 3.5 0 0 0 16.5 4h-9zM12 7a5 5 0 1 1 0 10 5 5 0 0 1 0-10zm0 2a3 3 0 1 0 0 6 3 3 0 0 0 0-6zm5.3-3.2a1.2 1.2 0 1 1 0 2.4 1.2 1.2 0 0 1 0-2.4z" />
    </Svg>
  ),
  x: <Svg><path d="M18.2 2.5h3.3l-7.2 8.2 8.5 10.8h-6.7l-5.2-6.8-6 6.8H1.6l7.7-8.8L1.2 2.5h6.8l4.7 6.2 5.5-6.2zm-1.2 17.3h1.8L7 4.4H5.1l11.9 15.4z" /></Svg>,
};

/* ✏️ Edita enlaces, redes y datos de contacto */
const columns = [
  {
    title: "Plataforma",
    links: [
      { label: "Inicio", href: "#inicio" },
      { label: "Ver mascotas", href: "/mascotas" },
      { label: "Sobre nosotros", href: "#sobre-nosotros" },
      { label: "¿Cómo funciona?", href: "#como-funciona" },
    ],
  },
  {
    title: "Comunidad",
    links: [
      { label: "Para adoptantes", href: "/registro" },
      { label: "Para organizaciones", href: "/organizaciones" },
      { label: "Preguntas frecuentes", href: "#faq" },
      { label: "Contáctanos", href: "#contacto" },
    ],
  },
];

const social = [
  { label: "Facebook", icon: icons.facebook, href: "https://facebook.com" },
  { label: "Instagram", icon: icons.instagram, href: "https://instagram.com" },
  { label: "X", icon: icons.x, href: "https://x.com" },
];

const legal = [
  { label: "Privacidad", href: "/privacidad" },
  { label: "Términos", href: "/terminos" },
  { label: "Cookies", href: "/cookies" },
];

export default function Footer() {
  const ref = useRef(null);
  const [isVisible, setIsVisible] = useState(false);
  const [email, setEmail] = useState("");
  const [subscribed, setSubscribed] = useState(false);

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true);
          observer.disconnect();
        }
      },
      { threshold: 0.1 }
    );
    if (ref.current) observer.observe(ref.current);
    return () => observer.disconnect();
  }, []);

  const handleSubscribe = (e) => {
    e.preventDefault();
    // 🔌 Conecta aquí tu servicio de newsletter
    setSubscribed(true);
    setEmail("");
  };

  const toTop = () => window.scrollTo({ top: 0, behavior: "smooth" });

  return (
    <footer ref={ref} className={`footer ${isVisible ? "footer--visible" : ""}`}>
      {/* Ola de transición desde la sección anterior */}
      <svg className="footer__wave" viewBox="0 0 1440 70" preserveAspectRatio="none" aria-hidden="true">
        <path d="M0 0h1440v20c-240 50-480 60-720 40S240 10 0 40V0z" />
      </svg>

      <div className="footer__container">
        {/* ---------- Columnas ---------- */}
        <div className="footer__grid">
          <div className="footer__brand reveal" style={{ "--i": 1 }}>
            <a href="#inicio" className="footer__logo" aria-label="Adopta+ — inicio">
              <span className="footer__logo-mark">
                <MaskIcon src="/icons/huellas/huella_g.svg" />
              </span>
              Adopta<span>+</span>
            </a>
            <p>
              Conectamos a personas que quieren dar un hogar con mascotas que lo
              necesitan, y te acompañamos después de la adopción.
            </p>

            <ul className="footer__social">
              {social.map((s) => (
                <li key={s.label}>
                  <a href={s.href} target="_blank" rel="noreferrer" aria-label={s.label}>
                    {s.icon}
                  </a>
                </li>
              ))}
            </ul>
          </div>

          {columns.map((col, i) => (
            <nav
              key={col.title}
              className="footer__col reveal"
              style={{ "--i": i + 2 }}
              aria-label={col.title}
            >
              <h4>{col.title}</h4>
              <ul>
                {col.links.map((l) => (
                  <li key={l.label}>
                    <a href={l.href}>{l.label}</a>
                  </li>
                ))}
              </ul>
            </nav>
          ))}

          <div className="footer__col reveal" style={{ "--i": 4 }}>
            <h4>Contacto</h4>
            <ul className="footer__contact">
              <li>
                {icons.mail}
                <a href="mailto:contacto@adoptamas.com">contacto@adoptamas.com</a>
              </li>
              <li>
                {icons.phone}
                <a href="tel:+524770000000">+52 477 000 0000</a>
              </li>
              <li>
                {icons.pin}
                <span>León, Guanajuato, México</span>
              </li>
            </ul>
          </div>
        </div>

        {/* ---------- Barra inferior ---------- */}
        <div className="footer__bottom reveal" style={{ "--i": 5 }}>
          <p>
            © {new Date().getFullYear()} Adopta+. Todos los derechos reservados.
          </p>

          <ul className="footer__legal">
            {legal.map((l) => (
              <li key={l.label}>
                <a href={l.href}>{l.label}</a>
              </li>
            ))}
          </ul>

          <button type="button" className="footer__top" onClick={toTop} aria-label="Volver arriba">
            Volver arriba
            {icons.up}
          </button>
        </div>
      </div>
    </footer>
  );
}