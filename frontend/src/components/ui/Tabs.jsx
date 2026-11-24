import { useId, useState } from "react";
import "../../styles/Tabs.css";

export default function Tabs({ tabs, initialTab, className = "" }) {
  const [active, setActive] = useState(initialTab ?? tabs[0]?.id);
  const baseId = useId();
  const tabId = (id) => `${baseId}-tab-${id}`;
  const panelId = (id) => `${baseId}-panel-${id}`;

  const current = tabs.some((t) => t.id === active) ? active : tabs[0]?.id;

  const onKeyDown = (e) => {
    const index = tabs.findIndex((t) => t.id === current);
    let next;
    if (e.key === "ArrowRight") next = (index + 1) % tabs.length;
    else if (e.key === "ArrowLeft") next = (index - 1 + tabs.length) % tabs.length;
    else if (e.key === "Home") next = 0;
    else if (e.key === "End") next = tabs.length - 1;
    else return;

    e.preventDefault();
    setActive(tabs[next].id);
    document.getElementById(tabId(tabs[next].id))?.focus();
  };

  return (
    <div className={["tabs", className].filter(Boolean).join(" ")}>
      <div className="tabs__list" role="tablist" onKeyDown={onKeyDown}>
        {tabs.map((t) => (
          <button
            key={t.id}
            type="button"
            role="tab"
            id={tabId(t.id)}
            aria-selected={t.id === current}
            aria-controls={panelId(t.id)}
            tabIndex={t.id === current ? 0 : -1}
            className={`tabs__tab${t.id === current ? " is-active" : ""}`}
            onClick={() => setActive(t.id)}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tabs.map((t) => (
        <div
          key={t.id}
          role="tabpanel"
          id={panelId(t.id)}
          aria-labelledby={tabId(t.id)}
          hidden={t.id !== current}
          className="tabs__panel"
        >
          {t.id === current && t.content}
        </div>
      ))}
    </div>
  );
}