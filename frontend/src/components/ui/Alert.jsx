import "../../styles/Alert.css";

export default function Alert({ variant = "info", className = "", children }) {
  return (
    <div
      className={["alert", `alert--${variant}`, className].filter(Boolean).join(" ")}
      role={variant === "error" ? "alert" : "status"}
    >
      {children}
    </div>
  );
}