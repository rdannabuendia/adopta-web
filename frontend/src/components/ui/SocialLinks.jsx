import "../../styles/SocialLinks.css";

const ICONS = {
  facebook: <path d="M14 8h2V5h-2.5C11.6 5 10.5 6.2 10.5 8v2h-2v3h2v6h3v-6h2.2l.3-3h-2.5V8.4c0-.3.2-.4.5-.4Z" fill="currentColor" stroke="none" />,
  instagram: (
    <>
      <rect x="5" y="5" width="14" height="14" rx="4" />
      <circle cx="12" cy="12" r="3.2" />
      <circle cx="16.2" cy="7.8" r=".6" fill="currentColor" />
    </>
  ),
  youtube: (
    <>
      <rect x="4" y="7" width="16" height="10" rx="3" />
      <path d="m10.5 9.8 3.5 2.2-3.5 2.2Z" fill="currentColor" />
    </>
  ),
};

/** links: [{ type: "facebook" | "instagram" | "youtube", url }] */
export default function SocialLinks({ links = [] }) {
  return (
    <ul className="social-links">
      {links.map(({ type, url }) => (
        <li key={type}>
          <a href={url} target="_blank" rel="noopener noreferrer" aria-label={type}>
            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" strokeWidth="1.6" aria-hidden="true">
              {ICONS[type]}
            </svg>
          </a>
        </li>
      ))}
    </ul>
  );
}