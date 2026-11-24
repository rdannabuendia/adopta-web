import Card from "../ui/Card";
import EmptyState from "../ui/EmptyState";
import "../../styles/DataTable.css";

export default function DataTable({
  columns,
  rows = [],
  rowKey = "id",
  loading = false,
  empty = { title: "Sin resultados" },
  footer,
  caption,
  className = "",
}) {
  return (
    <Card className={["data-table-card", className].filter(Boolean).join(" ")}>
      <div className="data-table__scroll">
        <table className="data-table" aria-busy={loading || undefined}>
          {caption && <caption className="sr-only">{caption}</caption>}
          <thead>
            <tr>
              {columns.map((c) => (
                <th key={c.key} scope="col" style={{ width: c.width, textAlign: c.align }}>
                  {c.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className={loading ? "is-loading" : undefined}>
            {rows.map((row) => (
              <tr key={typeof rowKey === "function" ? rowKey(row) : row[rowKey]}>
                {columns.map((c) => (
                  <td key={c.key} style={{ textAlign: c.align }}>
                    {c.render ? c.render(row) : row[c.key] ?? "—"}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {loading && rows.length === 0 && <p className="data-table__state">Cargando…</p>}
      {!loading && rows.length === 0 && <EmptyState {...empty} className="data-table__empty" />}
      {footer && <div className="data-table__footer">{footer}</div>}
    </Card>
  );
}