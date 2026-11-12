import { useEffect, useRef, useState } from "react";
import "./HowItWorks.css";

/*
  Los SVG de /public se sirven desde la raíz: usa "/icons/..." (no "../../public/...").
  MaskIcon pinta el SVG con el color del texto (currentColor), así se puede
  cambiar de color en hover/activo y animarlo igual que un SVG en línea.
*/
const MaskIcon = ({ src, className = "" }) => (
  <span
    className={`mask-icon ${className}`}
    style={{ "--icon": `url(${src})` }}
    aria-hidden="true"
  />
);

const steps = [
  {
    title: "Regístrate",
    text: "Crea tu cuenta como organización o adoptante desde la móvil.",
    icon: <MaskIcon src="/icons/user_icon.svg" />,
  },
  {
    title: "Explora mascotas",
    text: "Conoce a los animales disponibles y encuentra el que más te guste.",
    icon: <MaskIcon src="/icons/huellas/huella_g.svg" />,
  },
  {
    title: "Envía tu solicitud",
    text: "Completa el formulario de adopción con tus datos y responde algunas preguntas.",
    icon: (
      <svg viewBox="0 0 24 24" fill="currentColor">
        <path d="M14 2H6c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V8l-6-6zm2 16H8v-2h8v2zm0-4H8v-2h8v2zm-3-5V3.5L18.5 9H13z" />
      </svg>
    ),
  },
  {
    title: "Revisión de la organización",
    text: "La organización evaluará tu solicitud y se pondrá en contacto contigo.",
    icon: (
      <svg viewBox="0 0 24 24" fill="currentColor">
        <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" />
      </svg>
    ),
  },
  {
    title: "¡Haz la diferencia!",
    text: "Si todo está en orden, serás parte de una nueva historia.",
    icon: (
      <svg viewBox="0 0 24 24" fill="currentColor">
        <path d="M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z" />
      </svg>
    ),
  },
];

export default function HowItWorks() {
  const sectionRef = useRef(null);
  const [isVisible, setIsVisible] = useState(false);
  const [active, setActive] = useState(-1); // paso resaltado
  const [paused, setPaused] = useState(false);

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true);
          observer.disconnect();
        }
      },
      { threshold: 0.2 }
    );
    if (sectionRef.current) observer.observe(sectionRef.current);
    return () => observer.disconnect();
  }, []);

  useEffect(() => {
    const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (!isVisible || paused || reduce) return;
    const id = setInterval(() => setActive((a) => (a + 1) % steps.length), 2200);
    return () => clearInterval(id);
  }, [isVisible, paused]);

  return (
    <section
      ref={sectionRef}
      className={`how ${isVisible ? "how--visible" : ""}`}
      id="como-funciona"
    >
      <MaskIcon src="/icons/huellas/huella_g.svg" className="how__bg-paw" />

      <div className="how__container">
        <header className="how__header">
          <MaskIcon src="/icons/huellas/huella_g.svg" className="how__header-paw" />
          <div>
            <h2 className="how__title">¿Cómo funciona?</h2>
            <p className="how__subtitle">
              El proceso es sencillo y seguro. Solo necesitas unos minutos para comenzar.
            </p>
            <span className="how__bar" aria-hidden="true" />
          </div>
        </header>

        <div className="how__body">
          <ol
            className="how__steps"
            onMouseEnter={() => setPaused(true)}
            onMouseLeave={() => setPaused(false)}
          >
            {steps.map((step, index) => (
              <li
                key={step.title}
                className={`step ${active === index ? "step--active" : ""}`}
                style={{ "--i": index }}
                onMouseEnter={() => setActive(index)}
              >
                <div className="step__circle">
                  <span className="step__badge">{index + 1}</span>
                  <div className="step__icon">{step.icon}</div>
                </div>

                <h3 className="step__title">{step.title}</h3>
                <p className="step__text">{step.text}</p>

                {index < steps.length - 1 && (
                  <span className="step__line" aria-hidden="true" />
                )}
              </li>
            ))}
          </ol>

          <div className="how__note" aria-hidden="true">
            <span>Adoptar</span>
            <span>también es</span>
            <span>
              amar <span className="how__heart">♥</span>
            </span>
          </div>
        </div>
      </div>
    </section>
  );
}