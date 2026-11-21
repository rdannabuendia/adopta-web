import huella from "/public/icons/huellas/huella_g.svg";
import "../../styles/PawDecor.css";

export default function PawDecor({ className = "" }) {
  return (
    <img
      src={huella}
      alt=""
      aria-hidden="true"
      draggable="false"
      className={["paw-decor", className].filter(Boolean).join(" ")}
    />
  );
}