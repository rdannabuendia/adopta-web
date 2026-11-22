import Avatar from "./Avatar";
import "../../styles/EntityCell.css";

export default function EntityCell({ name, imageUrl, subtitle }) {
  return (
    <div className="entity-cell">
      <Avatar name={name} src={imageUrl} size="md" />
      <div className="entity-cell__text">
        <strong>{name}</strong>
        {subtitle && <span>{subtitle}</span>}
      </div>
    </div>
  );
}