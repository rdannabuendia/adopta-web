import { useEffect, useRef, useState } from "react";
import "./Contact.css";

/* Pinta un SVG de /public con el color del texto */
const MaskIcon = ({ src, className = "" }) => (
  <span
    className={`mask-icon ${className}`}
    style={{ "--icon": `url(${src})` }}
    aria-hidden="true"
  />
);

const Icon = ({ children }) => (
  <svg viewBox="0 0 24 24" aria-hidden="true">
    {children}
  </svg>
);

const icons = {
  mail: <Icon><path d="M20 4H4a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2zm0 4-8 5-8-5V6l8 5 8-5v2z" /></Icon>,
  phone: <Icon><path d="M6.6 10.8a15.1 15.1 0 0 0 6.6 6.6l2.2-2.2a1 1 0 0 1 1-.25c1.1.37 2.3.57 3.6.57a1 1 0 0 1 1 1V20a1 1 0 0 1-1 1A17 17 0 0 1 3 4a1 1 0 0 1 1-1h3.5a1 1 0 0 1 1 1c0 1.25.2 2.45.57 3.57a1 1 0 0 1-.25 1z" /></Icon>,
  pin: <Icon><path d="M12 2a7 7 0 0 0-7 7c0 5.25 7 13 7 13s7-7.75 7-13a7 7 0 0 0-7-7zm0 9.5A2.5 2.5 0 1 1 12 6.5a2.5 2.5 0 0 1 0 5z" /></Icon>,
  clock: <Icon><path d="M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zm4.2 13.2L11 12.4V7h1.5v4.6l4.4 2.4-.7 1.2z" /></Icon>,
  user: <Icon><path d="M12 12a5 5 0 1 0 0-10 5 5 0 0 0 0 10zm0 2c-3.3 0-10 1.7-10 5v3h20v-3c0-3.3-6.7-5-10-5z" /></Icon>,
  send: <Icon><path d="M2.01 21 23 12 2.01 3 2 10l15 2-15 2z" /></Icon>,
  check: <Icon><path d="M9 16.2 4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4z" /></Icon>,
};

/* ✏️ Reemplaza con los datos reales de tu plataforma */
const info = [
  { icon: icons.mail, title: "Correo", text: "contacto@adoptamas.com", href: "mailto:contacto@adoptamas.com" },
  { icon: icons.phone, title: "Teléfono", text: "+52 477 000 0000", href: "tel:+524770000000" },
  { icon: icons.pin, title: "Ubicación", text: "León, Guanajuato, México" },
  { icon: icons.clock, title: "Horario", text: "Lun a Vie · 9:00 – 18:00" },
];

const subjects = [
  "Quiero adoptar una mascota",
  "Soy una organización",
  "Seguimiento de mi adopción",
  "Soporte con la plataforma",
  "Otro tema",
];

const MAX = 500;

export default function Contact() {
  const sectionRef = useRef(null);
  const [isVisible, setIsVisible] = useState(false);
  const [status, setStatus] = useState("idle"); // idle | sending | sent | error
  const [message, setMessage] = useState("");

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true);
          observer.disconnect();
        }
      },
      { threshold: 0.15 }
    );
    if (sectionRef.current) observer.observe(sectionRef.current);
    return () => observer.disconnect();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    const data = Object.fromEntries(new FormData(e.currentTarget));
    setStatus("sending");
    try {
      // 🔌 Conecta aquí tu API, por ejemplo:
      // await fetch("/api/contacto", {
      //   method: "POST",
      //   headers: { "Content-Type": "application/json" },
      //   body: JSON.stringify(data),
      // });
      console.log("Mensaje:", data);
      await new Promise((r) => setTimeout(r, 1200)); // simulación, bórrala
      setStatus("sent");
    } catch {
      setStatus("error");
    }
  };

  const reset = () => {
    setMessage("");
    setStatus("idle");
  };

  return (
    <section
      ref={sectionRef}
      className={`contact ${isVisible ? "contact--visible" : ""}`}
      id="contacto"
    >
      <span className="contact__circle contact__circle--left" aria-hidden="true" />
      <span className="contact__circle contact__circle--right" aria-hidden="true" />
      <MaskIcon src="/icons/huellas/huella_g.svg" className="contact__bg-paw contact__bg-paw--1" />
      <MaskIcon src="/icons/huellas/huella_g.svg" className="contact__bg-paw contact__bg-paw--2" />

      <div className="contact__container">
        {/* ---------- Izquierda: información ---------- */}
        <div className="contact__info">
          <span className="contact__label">
            <MaskIcon src="/icons/huellas/huella_g.svg" className="contact__label-icon" />
            Contáctanos
          </span>

          <h2 className="contact__title">
            Hablemos, estamos
            <br />
            para ayudarte
          </h2>
          <span className="contact__bar" aria-hidden="true" />

          <p className="contact__text">
            ¿Tienes preguntas sobre una adopción, quieres sumar a tu
            organización o necesitas ayuda con la plataforma? Escríbenos y te
            responderemos lo antes posible.
          </p>

          <ul className="contact__cards">
            {info.map((item, i) => {
              const Wrapper = item.href ? "a" : "div";
              return (
                <li key={item.title} style={{ "--i": i }}>
                  <Wrapper className="info-card" href={item.href}>
                    <span className="info-card__icon">{item.icon}</span>
                    <span className="info-card__body">
                      <strong>{item.title}</strong>
                      <small>{item.text}</small>
                    </span>
                  </Wrapper>
                </li>
              );
            })}
          </ul>
        </div>

        {/* ---------- Derecha: formulario ---------- */}
        <div className="contact__panel">
          <MaskIcon src="/icons/huellas/huella_g.svg" className="contact__panel-paw" />

          {status === "sent" ? (
            <div className="contact__success" role="status">
              <span className="contact__success-icon">{icons.check}</span>
              <h3>¡Mensaje enviado!</h3>
              <p>
                Gracias por escribirnos. Revisaremos tu mensaje y te
                responderemos muy pronto.
              </p>
              <button type="button" className="btn btn--outline" onClick={reset}>
                Enviar otro mensaje
              </button>
            </div>
          ) : (
            <form className="form" onSubmit={handleSubmit}>
              <header className="form__header">
                <h3>Envíanos un mensaje</h3>
                <p>Completa el formulario y te contactaremos.</p>
              </header>

              <div className="form__row">
                <label className="field">
                  <span className="field__label">Nombre</span>
                  <span className="field__control">
                    {icons.user}
                    <input
                      type="text"
                      name="nombre"
                      placeholder="Tu nombre completo"
                      autoComplete="name"
                      required
                    />
                  </span>
                </label>

                <label className="field">
                  <span className="field__label">Correo electrónico</span>
                  <span className="field__control">
                    {icons.mail}
                    <input
                      type="email"
                      name="correo"
                      placeholder="tucorreo@ejemplo.com"
                      autoComplete="email"
                      required
                    />
                  </span>
                </label>
              </div>

              <label className="field">
                <span className="field__label">Asunto</span>
                <span className="field__control field__control--select">
                  <select name="asunto" defaultValue="" required>
                    <option value="" disabled>
                      Selecciona un tema
                    </option>
                    {subjects.map((s) => (
                      <option key={s} value={s}>
                        {s}
                      </option>
                    ))}
                  </select>
                </span>
              </label>

              <label className="field">
                <span className="field__label">
                  Mensaje
                  <em className={message.length > MAX - 40 ? "is-near" : ""}>
                    {message.length}/{MAX}
                  </em>
                </span>
                <span className="field__control field__control--area">
                  <textarea
                    name="mensaje"
                    rows="5"
                    maxLength={MAX}
                    placeholder="Cuéntanos en qué podemos ayudarte…"
                    value={message}
                    onChange={(e) => setMessage(e.target.value)}
                    required
                  />
                </span>
              </label>

              {status === "error" && (
                <p className="form__error" role="alert">
                  No pudimos enviar tu mensaje. Inténtalo de nuevo en unos minutos.
                </p>
              )}

              <button
                type="submit"
                className="btn btn--primary"
                disabled={status === "sending"}
              >
                {status === "sending" ? (
                  <>
                    <span className="spinner" aria-hidden="true" />
                    Enviando…
                  </>
                ) : (
                  <>
                    Enviar mensaje
                    {icons.send}
                  </>
                )}
              </button>
            </form>
          )}
        </div>
      </div>
    </section>
  );
}
