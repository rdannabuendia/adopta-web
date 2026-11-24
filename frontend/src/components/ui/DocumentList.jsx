import { useState } from "react";
import Alert from "./Alert";
import BaseButton from "./BaseButton";
import "../../styles/DocumentList.css";

export default function DocumentList({ documents = [], labels = {}, onView, title = "Documentos" }) {
  const [error, setError] = useState("");
  if (documents.length === 0) return null;

  const ver = async (doc) => {
    setError("");
    try {
      await onView(doc);
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <section className="document-list">
      <h3>{title}</h3>
      {error && <Alert variant="error">{error}</Alert>}
      <ul>
        {documents.map((d) => (
          <li key={d.idDocumento}>
            <span>
              <strong>{labels[d.tipo] ?? d.tipo}</strong>
              <small>{d.nombreOriginal}</small>
            </span>
            <BaseButton variant="secondary" size="xs" onClick={() => ver(d)}>Ver</BaseButton>
          </li>
        ))}
      </ul>
    </section>
  );
}