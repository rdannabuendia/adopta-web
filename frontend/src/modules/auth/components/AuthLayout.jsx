import { PawIcon } from "../../../components/ui/Icons";
import "../../../styles/LoginPage.css";
import heroImage from "../../../assets/img/hero_login.jpg"; 

export default function AuthLayout({ children }) {
  return (
    <main className="login-page">
      <section className="login-page__panel">
        <PawIcon className="login-page__deco login-page__deco--sm" />
        <PawIcon className="login-page__deco login-page__deco--lg" />

        <div className="login-page__content">
          <div className="login-page__brand">
            <PawIcon className="login-page__brand-icon" />
            <span>Adopta+</span>
          </div>

          {children}
        </div>
      </section>

      <aside
        className="login-page__hero"
        style={{ backgroundImage: `url(${heroImage})` }}
        role="img"
        aria-label="Un perro y un gato juntos"
      />
    </main>
  );
}