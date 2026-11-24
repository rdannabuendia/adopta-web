import { useCallback, useEffect, useRef, useState } from "react";
import BaseButton from "../../../../components/ui/BaseButton";
import landingService from "../../services/landing.service";
import { mediaUrl } from "../../../../utils/media";
import { scrollToSection } from "../../../../utils/scroll";
import "./PetsCarousel.css";

const TOTAL = 10;
const AUTOPLAY_MS = 4500;

const MaskIcon = ({ src, className = "" }) => (
  <span
    className={`mask-icon ${className}`}
    style={{ "--icon": `url(${src})` }}
    aria-hidden="true"
  />
);

const Svg = ({ children, className = "" }) => (
  <svg className={className} viewBox="0 0 24 24" aria-hidden="true">
    {children}
  </svg>
);

const icons = {
  prev: (
    <Svg>
      <path d="M15 5l-7 7 7 7" fill="none" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  ),
  next: (
    <Svg>
      <path d="M9 5l7 7-7 7" fill="none" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  ),
  shuffle: (
    <Svg>
      <path
        d="M4 4v6h6M20 20v-6h-6M20 10A8 8 0 0 0 6.3 6.3L4 8.5M4 14a8 8 0 0 0 13.7 3.7L20 15.5"
        fill="none"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </Svg>
  ),
  pin: (
    <Svg>
      <path d="M12 2a7 7 0 0 0-7 7c0 5.25 7 13 7 13s7-7.75 7-13a7 7 0 0 0-7-7zm0 9.5A2.5 2.5 0 1 1 12 6.5a2.5 2.5 0 0 1 0 5z" />
    </Svg>
  ),
  arrow: (
    <Svg>
      <path d="M5 12h14M13 6l6 6-6 6" fill="none" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  ),
};

const TAMANOS = { Pequeno: "Pequeño", Mediano: "Mediano", Grande: "Grande" };

function PetPhoto({ pet }) {
  const [failed, setFailed] = useState(false);
  const src = mediaUrl(pet.fotoUrl);

  if (!src || failed) {
    return (
      <div className="pets__photo pets__photo--empty">
        <MaskIcon src="/icons/huellas/huella_g.svg" className="pets__photo-paw" />
      </div>
    );
  }

  return (
    <div className="pets__photo">
      <img
        src={src}
        alt={`${pet.nombre}, ${pet.especie?.toLowerCase() ?? "mascota"} en adopción`}
        loading="lazy"
        onError={() => setFailed(true)}
      />
    </div>
  );
}

function PetCard({ pet }) {
  const tags = [pet.edadAproximada, TAMANOS[pet.tamano] ?? pet.tamano].filter(Boolean);

  return (
    <article className="pet-card">
      <div className="pet-card__media">
        <PetPhoto pet={pet} />
        <span className="pet-card__badge">{pet.especie}</span>
      </div>

      <div className="pet-card__body">
        <div className="pet-card__head">
          <h3 className="pet-card__name">{pet.nombre}</h3>
          <span className="pet-card__sex">{pet.sexo}</span>
        </div>

        <p className="pet-card__breed">{pet.raza || "Mestizo"}</p>

        {tags.length > 0 && (
          <ul className="pet-card__tags">
            {tags.map((t) => (
              <li key={t}>{t}</li>
            ))}
          </ul>
        )}

        <p className="pet-card__org">
          {icons.pin}
          <span>{pet.organizacion}</span>
        </p>

        <a
          href="#plataformas"
          className="pet-card__cta"
          onClick={(e) => scrollToSection("plataformas", e)}
        >
          Quiero adoptarlo
          {icons.arrow}
        </a>
      </div>
    </article>
  );
}

function Skeletons() {
  return Array.from({ length: 4 }, (_, i) => (
    <div className="pet-card pet-card--skeleton" key={i} aria-hidden="true">
      <div className="pet-card__media" />
      <div className="pet-card__body">
        <span className="sk sk--lg" />
        <span className="sk" />
        <span className="sk sk--sm" />
      </div>
    </div>
  ));
}

export default function PetsCarousel() {
  const sectionRef = useRef(null);
  const trackRef = useRef(null);

  const [isVisible, setIsVisible] = useState(false);
  const [inView, setInView] = useState(false);
  const [paused, setPaused] = useState(false);
  const [status, setStatus] = useState("loading"); // loading | ready | empty | error
  const [pets, setPets] = useState([]);
  const [active, setActive] = useState(0);

  // ---------- Datos ----------
  const load = useCallback(async () => {
    setStatus("loading");
    try {
      const data = await landingService.getFeaturedPets(TOTAL);
      const list = Array.isArray(data) ? data : [];
      setPets(list);
      setActive(0);
      setStatus(list.length ? "ready" : "empty");
      trackRef.current?.scrollTo({ left: 0 });
    } catch {
      setPets([]);
      setStatus("error");
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  // ---------- Animación de entrada + autoplay solo cuando se ve ----------
  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        setInView(entry.isIntersecting);
        if (entry.isIntersecting) setIsVisible(true);
      },
      { threshold: 0.2 },
    );
    if (sectionRef.current) observer.observe(sectionRef.current);
    return () => observer.disconnect();
  }, []);

  // ---------- Navegación ----------
  const step = () => {
    const track = trackRef.current;
    const card = track?.querySelector(".pet-card");
    if (!track || !card) return 0;
    const gap = parseFloat(getComputedStyle(track).columnGap) || 0;
    return card.offsetWidth + gap;
  };

  const goTo = useCallback((index) => {
    const track = trackRef.current;
    const card = track?.querySelector(".pet-card");
    if (!track || !card) return;
    const gap = parseFloat(getComputedStyle(track).columnGap) || 0;
    track.scrollTo({ left: index * (card.offsetWidth + gap), behavior: "smooth" });
  }, []);

  const move = useCallback(
    (dir) => {
      const track = trackRef.current;
      if (!track) return;
      const atEnd = track.scrollLeft + track.clientWidth >= track.scrollWidth - 8;
      const atStart = track.scrollLeft <= 8;

      if (dir > 0 && atEnd) return goTo(0);
      if (dir < 0 && atStart) {
        return track.scrollTo({ left: track.scrollWidth, behavior: "smooth" });
      }
      track.scrollBy({ left: dir * step(), behavior: "smooth" });
    },
    [goTo],
  );

  const onScroll = () => {
    const s = step();
    if (!s) return;
    setActive(Math.round((trackRef.current?.scrollLeft ?? 0) / s));
  };

  useEffect(() => {
    const reduce = window.matchMedia?.("(prefers-reduced-motion: reduce)").matches;
    if (status !== "ready" || pets.length < 2 || paused || !inView || reduce) return;

    const id = setInterval(() => move(1), AUTOPLAY_MS);
    return () => clearInterval(id);
  }, [status, pets.length, paused, inView, move]);

  return (
    <section
      ref={sectionRef}
      className={`pets ${isVisible ? "pets--visible" : ""}`}
      id="mascotas"
    >
      <span className="pets__circle pets__circle--left" aria-hidden="true" />
      <span className="pets__circle pets__circle--right" aria-hidden="true" />
      <MaskIcon src="/icons/huellas/huella_g.svg" className="pets__bg-paw pets__bg-paw--1" />
      <MaskIcon src="/icons/huellas/huella_g.svg" className="pets__bg-paw pets__bg-paw--2" />

      <div className="pets__container">
        <header className="pets__header">
          <span className="pets__label">
            <MaskIcon src="/icons/huellas/huella_g.svg" className="pets__label-icon" />
            Mascotas en adopción
          </span>
          <h2 className="pets__title">Ellos esperan un hogar</h2>
          <span className="pets__bar" aria-hidden="true" />
          <p className="pets__text">
            Conoce a algunos de los peluditos que hoy buscan una familia. Cada vez que
            entras te mostramos mascotas distintas.
          </p>
        </header>

        {status === "error" || status === "empty" ? (
          <div className="pets__state" role="status">
            <MaskIcon src="/icons/huellas/huella_g.svg" className="pets__state-icon" />
            <h3>
              {status === "error"
                ? "No pudimos cargar las mascotas"
                : "Pronto habrá nuevos amigos por conocer"}
            </h3>
            <p>
              {status === "error"
                ? "Revisa tu conexión e inténtalo de nuevo."
                : "Por ahora no hay mascotas para mostrar. Vuelve en unos días o descarga la app para enterarte primero."}
            </p>
            <div className="pets__state-actions">
              {status === "error" && (
                <BaseButton size="md" onClick={load}>
                  Reintentar
                </BaseButton>
              )}
              <BaseButton
                size="md"
                variant="secondary"
                href="#plataformas"
                onClick={(e) => scrollToSection("plataformas", e)}
              >
                Descargar la app
              </BaseButton>
            </div>
          </div>
        ) : (
          <div
            className="pets__carousel"
            onMouseEnter={() => setPaused(true)}
            onMouseLeave={() => setPaused(false)}
            onFocus={() => setPaused(true)}
            onBlur={() => setPaused(false)}
            onTouchStart={() => setPaused(true)}
            role="region"
            aria-roledescription="carrusel"
            aria-label="Mascotas disponibles para adopción"
          >
            <button
              type="button"
              className="pets__nav pets__nav--prev"
              aria-label="Mascota anterior"
              onClick={() => move(-1)}
              disabled={status === "loading"}
            >
              {icons.prev}
            </button>

            <div className="pets__track" ref={trackRef} onScroll={onScroll} tabIndex={-1}>
              {status === "loading" ? (
                <Skeletons />
              ) : (
                pets.map((pet) => <PetCard key={pet.idMascota} pet={pet} />)
              )}
            </div>

            <button
              type="button"
              className="pets__nav pets__nav--next"
              aria-label="Siguiente mascota"
              onClick={() => move(1)}
              disabled={status === "loading"}
            >
              {icons.next}
            </button>
          </div>
        )}

        {status === "ready" && (
          <div className="pets__footer">
            <ul className="pets__dots" aria-label="Posición del carrusel">
              {pets.map((p, i) => (
                <li key={p.idMascota}>
                  <button
                    type="button"
                    className={i === active ? "is-active" : ""}
                    aria-label={`Ir a ${p.nombre}`}
                    aria-current={i === active}
                    onClick={() => goTo(i)}
                  />
                </li>
              ))}
            </ul>

            <button type="button" className="pets__shuffle" onClick={load}>
              {icons.shuffle}
              Ver otras mascotas
            </button>
          </div>
        )}
      </div>
    </section>
  );
}
