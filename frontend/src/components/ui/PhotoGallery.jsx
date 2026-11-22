import { useState } from "react";
import { PawIcon } from "./Icons";
import "../../styles/PhotoGallery.css";

/** photos: lista de URLs (la primera es la principal) */
export default function PhotoGallery({ photos = [], alt = "" }) {
  const [index, setIndex] = useState(0);
  const actual = photos[Math.min(index, photos.length - 1)];

  return (
    <div className="photo-gallery">
      <div className="photo-gallery__main">
        {actual ? <img src={actual} alt={alt} /> : <PawIcon aria-hidden="true" />}
      </div>

      {photos.length > 1 && (
        <ul className="photo-gallery__thumbs">
          {photos.map((src, i) => (
            <li key={src}>
              <button
                type="button"
                className={i === index ? "is-active" : ""}
                aria-label={`Ver foto ${i + 1}`}
                onClick={() => setIndex(i)}
              >
                <img src={src} alt="" />
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}