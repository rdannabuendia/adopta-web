import BaseInput from "./BaseInput";
import { SearchIcon } from "./Icons";
import "../../styles/ListToolbar.css";

export default function SearchInput({ value, onChange, placeholder = "Buscar…", label = "Buscar", className = "", ...rest }) {
  return (
    <BaseInput
      type="search"
      aria-label={label}
      placeholder={placeholder}
      icon={<SearchIcon />}
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className={["search-input", className].filter(Boolean).join(" ")}
      {...rest}
    />
  );
}