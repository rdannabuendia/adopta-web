import "../../styles/Card.css";

export default function Card({ padded = false, className = "", children, ...rest }) {
  const classes = ["card", padded && "card--padded", className].filter(Boolean).join(" ");
  return (
    <section className={classes} {...rest}>
      {children}
    </section>
  );
}