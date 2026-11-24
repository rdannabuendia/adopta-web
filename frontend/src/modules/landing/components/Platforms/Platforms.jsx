import { useEffect, useRef, useState } from "react";
import BaseButton from "../../../../components/ui/BaseButton";
import "./Platforms.css";

// Enlace de descarga de la app móvil (APK en GitHub Releases)
const MOBILE_APP_URL =
  "https://github.com/IDGS-1002-23002074/Adopta/releases/download/v1.0.0/Adopta.apk";

// Código QR: coloca la imagen en src/assets/img/ con el nombre QR_Adopta (.png, .jpg, .svg o .webp)
const qrFiles = import.meta.glob("../../../../assets/img/QR_Adopta_v3.*", {
  eager: true,
  import: "default",
});
const QR_SRC = Object.values(qrFiles)[0];

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
  check: (
    <Svg>
      <path d="M9 16.2 4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4z" />
    </Svg>
  ),
  download: (
    <Svg>
      <path d="M12 3v12m0 0-4.5-4.5M12 15l4.5-4.5M4 20h16" fill="none" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  ),
  login: (
    <Svg>
      <path d="M10 17l5-5-5-5M15 12H3M14 4h5a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-5" fill="none" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  ),
  monitor: (
    <Svg>
      <path d="M3 5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2h-5l1 3h2v1H7v-1h2l1-3H5a2 2 0 0 1-2-2V5zm2 0v10h14V5H5z" />
    </Svg>
  ),
  phone: (
    <Svg>
      <path d="M8 2h8a2 2 0 0 1 2 2v16a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2zm0 2v14h8V4H8zm4 15.2a1 1 0 1 0 0 2 1 1 0 0 0 0-2z" />
    </Svg>
  ),
};

const platforms = [
  {
    key: "web",
    kind: "Para organizaciones",
    title: "Aplicación web",
    icon: icons.monitor,
    text: "El panel para refugios, asociaciones y rescatistas que gestionan sus adopciones desde la computadora.",
    features: [
      "Registra y administra a tus mascotas",
      "Recibe y resuelve solicitudes de adopción",
      "Da seguimiento a cada adopción y devolución",
      "Mantén al día el perfil de tu organización",
    ],
  },
  {
    key: "movil",
    kind: "Para quienes quieren adoptar",
    title: "App móvil",
    icon: icons.phone,
    text: "Para adoptantes: encuentra a tu compañero ideal y lleva todo el proceso en la palma de tu mano.",
    features: [
      "Explora las mascotas disponibles",
      "Envía tu solicitud de adopción",
      "Consulta el estado de tu solicitud",
      "Comparte la evolución de tu mascota",
    ],
  },
];

export default function Platforms() {
  const sectionRef = useRef(null);
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true);
          observer.disconnect();
        }
      },
      { threshold: 0.15 },
    );
    if (sectionRef.current) observer.observe(sectionRef.current);
    return () => observer.disconnect();
  }, []);

  const mobileLinkProps = { target: "_blank", rel: "noreferrer" };

  return (
    <section
      ref={sectionRef}
      className={`platforms ${isVisible ? "platforms--visible" : ""}`}
      id="plataformas"
    >
      <span className="platforms__circle platforms__circle--left" aria-hidden="true" />
      <span className="platforms__circle platforms__circle--right" aria-hidden="true" />

      <div className="platforms__container">
        <header className="platforms__header">
          <span className="platforms__label">
            <MaskIcon src="/icons/huellas/huella_g.svg" className="platforms__label-icon" />
            Web y móvil
          </span>
          <h2 className="platforms__title">Una plataforma, dos formas de usarla</h2>
          <span className="platforms__bar" aria-hidden="true" />
          <p className="platforms__text">
            Adopta+ está pensada para cada quien: las organizaciones administran desde la
            web y las personas que quieren adoptar lo hacen desde la app móvil.
          </p>
        </header>

        <div className="platforms__grid">
          {platforms.map((p, i) => (
            <article
              key={p.key}
              className={`platform platform--${p.key}`}
              style={{ "--i": i }}
            >
              <div className="platform__body">
                <span className="platform__kind">
                  <span className="platform__kind-icon">{p.icon}</span>
                  {p.kind}
                </span>
                <h3 className="platform__title">{p.title}</h3>
                <p className="platform__text">{p.text}</p>

                <ul className="platform__features">
                  {p.features.map((f) => (
                    <li key={f}>
                      <span className="platform__check">{icons.check}</span>
                      {f}
                    </li>
                  ))}
                </ul>

                {p.key === "web" ? (
                  <div className="platform__download platform__download--solo">
                    <div className="platform__download-info">
                      <strong>Accede a tu panel</strong>
                      <p>Inicia sesión o registra a tu organización para empezar.</p>
                      <div className="platform__actions">
                        <BaseButton to="/login" size="md" icon={icons.login}>
                          Iniciar sesión
                        </BaseButton>
                        <BaseButton to="/registro" size="md" variant="secondary">
                          Registrar organización
                        </BaseButton>
                      </div>
                    </div>
                  </div>
                ) : (
                  <div className={`platform__download ${QR_SRC ? "" : "platform__download--solo"}`}>
                    {QR_SRC && (
                      <img
                        className="platform__qr"
                        src={QR_SRC}
                        alt="Código QR para descargar la app móvil de Adopta+"
                      />
                    )}
                    <div className="platform__download-info">
                      <strong>Descarga la app</strong>
                      <p>
                        {QR_SRC
                          ? "Escanea el código con la cámara de tu celular o toca el botón."
                          : "Toca el botón para descargarla en tu celular."}
                      </p>
                      <BaseButton
                        href={MOBILE_APP_URL}
                        size="md"
                        icon={icons.download}
                        {...mobileLinkProps}
                      >
                        Descargar la app
                      </BaseButton>
                    </div>
                  </div>
                )}
              </div>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}