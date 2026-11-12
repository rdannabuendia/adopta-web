import { useEffect, useRef, useState } from "react";
import "./Faq.css";
// Cambia la ruta por la de tu gatito (PNG con fondo transparente)
import gatito from "../../../../assets/img/image.png";

const MaskIcon = ({ src, className = "" }) => (
  <span
    className={`mask-icon ${className}`}
    style={{ "--icon": `url(${src})` }}
    aria-hidden="true"
  />
);

const Heart = ({ className = "" }) => (
  <svg className={`faq__heart ${className}`} viewBox="0 0 48 44" aria-hidden="true">
    <path
      d="M24 40C10 30 4 22 4 14.5 4 9 8.4 5 13.5 5c4 0 7.5 2.4 10.5 7 3-4.6 6.5-7 10.5-7C39.6 5 44 9 44 14.5 44 22 38 30 24 40Z"
      fill="none"
      strokeWidth="4"
      strokeLinejoin="round"
    />
  </svg>
);

const Arrow = () => (
  <svg className="arrow" viewBox="0 0 24 24" aria-hidden="true">
    <path d="M5 12h14M13 6l6 6-6 6" fill="none" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
);

const faqs = [
  {
    q: "¿Tiene algún costo adoptar una mascota?",
    a: "Registrarte en Adopta+ es gratis. Algunas organizaciones piden una cuota de recuperación para cubrir vacunas o esterilización; cada una la indica en el perfil de la mascota.",
  },
  {
    q: "¿Qué requisitos necesito para adoptar?",
    a: "Ser mayor de edad, crear tu cuenta y completar el formulario de adopción. Cada organización puede pedir información adicional, como tu tipo de vivienda o tu experiencia con mascotas.",
  },
  {
    q: "¿Cuánto tarda la revisión de mi solicitud?",
    a: "Depende de cada organización. Recibirás una notificación cuando tu solicitud cambie de estado y podrás darle seguimiento desde tu cuenta.",
  },
  {
    q: "¿Puedo adoptar si vivo en un departamento?",
    a: "Sí, en muchos casos. Lo importante es que la mascota tenga el espacio, el tiempo y los cuidados que necesita. Cada perfil indica qué tipo de hogar es ideal.",
  },
  {
    q: "¿Hay seguimiento después de la adopción?",
    a: "Sí. Puedes mantenerte en contacto con la organización y compartir la evolución de tu mascota para que el comienzo sea feliz y seguro.",
  },
];

export default function Faq() {
  const sectionRef = useRef(null);
  const [isVisible, setIsVisible] = useState(false);
  const [open, setOpen] = useState(0);

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

  return (
    <section
      ref={sectionRef}
      className={`faq ${isVisible ? "faq--visible" : ""}`}
      id="faq"
    >
      <div className="faq__container">
        <div className="faq__cta">
          <div className="faq__pet">
            <Heart className="faq__heart--1" />
            <Heart className="faq__heart--2" />
            <div className="faq__blob">
              <img src={gatito} alt="Gatito esperando un hogar" />
            </div>
          </div>

          <div className="faq__cta-text">
            <span className="faq__label">
              <MaskIcon src="/icons/huellas/huella_g.svg" className="faq__label-icon" />
              Únete a nuestra comunidad
            </span>
            <h2 className="faq__title">
              Juntos por más
              <br />
              hogares felices
            </h2>
            <p className="faq__text">
              Ya sea que quieras adoptar, apoyar a una organización o
              simplemente conocer más, ¡te esperamos!
            </p>
            <a href="/registro" className="faq__btn">
              Regístrate ahora
              <Arrow />
            </a>
          </div>
        </div>

        {/* ---------- Derecha: panel de dudas ---------- */}
        <div className="faq__panel">
          <MaskIcon src="/icons/huellas/huella_g.svg" className="faq__panel-paw" />

          <header className="faq__panel-header">
            <MaskIcon src="/icons/huellas/huella_g.svg" className="faq__panel-icon" />
            <div>
              <h3 className="faq__panel-title">¿Tienes dudas?</h3>
              <p className="faq__panel-text">
                Consulta nuestras preguntas frecuentes o contáctanos directamente.
                Estamos para ayudarte.
              </p>
            </div>
          </header>

          <ul className="faq__list">
            {faqs.map((item, i) => {
              const isOpen = open === i;
              return (
                <li
                  key={item.q}
                  className={`faq__item ${isOpen ? "faq__item--open" : ""}`}
                  style={{ "--i": i }}
                >
                  <button
                    type="button"
                    className="faq__question"
                    aria-expanded={isOpen}
                    aria-controls={`faq-a-${i}`}
                    onClick={() => setOpen(isOpen ? -1 : i)}
                  >
                    <span>{item.q}</span>
                    <span className="faq__chevron" aria-hidden="true" />
                  </button>
                  <div className="faq__answer" id={`faq-a-${i}`} role="region">
                    <div className="faq__answer-inner">
                      <p>{item.a}</p>
                    </div>
                  </div>
                </li>
              );
            })}
          </ul>

          <a href="/contacto" className="faq__contact">
            <span className="faq__contact-icon">
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <path d="M20 4H4a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2zm0 4-8 5-8-5V6l8 5 8-5v2z" />
              </svg>
            </span>
            <span className="faq__contact-body">
              <strong>¿No encuentras tu respuesta?</strong>
              <small>Escríbenos y te ayudamos.</small>
            </span>
            <span className="faq__contact-link">
              Enviar mensaje
              <Arrow />
            </span>
          </a>
        </div>
      </div>
    </section>
  );
}