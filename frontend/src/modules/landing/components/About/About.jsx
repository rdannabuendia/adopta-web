import "./About.css";
import iconInfo from "../../../../../public/icons/land-ing/escudo_landing.svg";
import iconSeguro from "../../../../../public/icons/land-ing/corazon_landing.svg";
import iconWeb from "../../../../../public/icons/land-ing/telefono_landing.svg";
import iconSeguimiento from "../../../../../public/icons/land-ing/manos_landing.svg";

const HuellaLogo = ({ className = "" }) => (
  <img src="../../../../../public/icons/huellas/huella_g.svg" alt="HuellaLogo" className={className} />
);

const features = [
  {
    icon: iconInfo,
    title: "Información confiable",
    text: "Datos actualizados de cada mascota y organización.",
  },
  {
    icon: iconSeguro,
    title: "Proceso seguro",
    text: "Tu información está protegida.",
  },
  {
    icon: iconWeb,
    title: "Disponible en web y móvil",
    text: "Accede desde cualquier dispositivo.",
  },
  {
    icon: iconSeguimiento,
    title: "Seguimiento post-adopción",
    text: "Mantente en contacto y comparte su evolución.",
  },
];

export default function About() {
  return (
    <section className="about" id="sobre-nosotros">
      <span className="about__circle about__circle--left" aria-hidden="true" />
      <span className="about__circle about__circle--right" aria-hidden="true" />
      <HuellaLogo className="about__paw about__paw--left" />
      <HuellaLogo className="about__paw about__paw--right" />

      <div className="about__container">
        <header className="about__header">
          <span className="about__label">
            <HuellaLogo className="about__label-icon" />
            Sobre nosotros
          </span>

          <h2 className="about__title">¿Qué es Adopta+?</h2>

          <p className="about__text">
            <strong>Adopta+</strong> es una plataforma web y móvil que facilita
            la adopción responsable de mascotas. Aquí puedes encontrar animales
            disponibles, conocer a las organizaciones, realizar tu solicitud de
            adopción y dar seguimiento al proceso, todo en un solo lugar.
          </p>
        </header>

        <ul className="about__features">
          {features.map((f) => (
            <li className="feature" key={f.title}>
              <div className="feature__icon">
                <img src={f.icon} alt="" />
              </div>
              <h3 className="feature__title">{f.title}</h3>
              <p className="feature__text">{f.text}</p>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}