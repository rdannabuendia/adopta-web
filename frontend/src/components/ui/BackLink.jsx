import { Link } from "react-router-dom";
import { ArrowLeftIcon } from "./Icons";
import "../../styles/BackLink.css";

export default function BackLink({ to, children = "Volver", className = "" }) {
  return (
    <Link to={to} className={["back-link", className].filter(Boolean).join(" ")}>
      <ArrowLeftIcon />
      {children}
    </Link>
  );
}