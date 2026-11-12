import "./Hero.css";
import banner from "../../../../assets/img/banner_inicio.jpg";

const HuellaBlanca = ({ className = "" }) => (
  <img src="../../../../../public/icons/huellas/huella_blanca.svg" alt="Paw icon" className={className} />
);

export default function Hero() {
  return (
    <section className="hero" id="inicio">
      <img
        className="hero__img"
        src={banner}
        alt="Un perro y un gato juntos esperando un hogar"
      />

      <div className="hero__content">
        <span className="hero__badge">Más que una adopción</span>

        <h1 className="hero__title">
          Adopta, cambia
          <br />
          una vida
        </h1>

        <p className="hero__text">
          En Adopta+ conectamos a personas que quieren dar un hogar con
          mascotas que lo necesitan. También te acompañamos después de la
          adopción, para que sea un comienzo feliz y seguro.
        </p>

        <div className="hero__actions">
          <a href="/mascotas" className="btn btn--primary">
            <HuellaBlanca className="btn__icon" />
            Ver mascotas
          </a>
          <a href="/nosotros" className="btn btn--outline">
            Conoce más
          </a>
        </div>
      </div>
    </section>
  );
}