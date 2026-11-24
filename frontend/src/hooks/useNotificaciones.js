import { useCallback, useEffect, useState } from "react";
import {
  listarNotificaciones,
  marcarLeida,
  marcarTodasLeidas,
} from "../modules/notificaciones/services/notificacionesService";

const INTERVALO_MS = 60_000;

/**
 * Notificaciones del usuario en sesión: se consultan al montar, cada minuto
 * (solo con la pestaña visible) y al volver a la pestaña.
 */
export function useNotificaciones() {
  const [items, setItems] = useState([]);
  const [noLeidas, setNoLeidas] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const cargar = useCallback(async () => {
    try {
      const res = await listarNotificaciones({ limite: 20 });
      setItems(res.items);
      setNoLeidas(res.noLeidas);
      setError("");
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    cargar();
    const timer = setInterval(() => !document.hidden && cargar(), INTERVALO_MS);
    const onVisible = () => !document.hidden && cargar();
    document.addEventListener("visibilitychange", onVisible);
    return () => {
      clearInterval(timer);
      document.removeEventListener("visibilitychange", onVisible);
    };
  }, [cargar]);

  const leer = useCallback(async (n) => {
    if (n.leida) return;
    setItems((prev) => prev.map((x) => (x.idNotificacion === n.idNotificacion ? { ...x, leida: true } : x)));
    setNoLeidas((c) => Math.max(0, c - 1));
    try {
      await marcarLeida(n.idNotificacion);
    } catch {
      cargar(); // si falló, se vuelve a sincronizar con el servidor
    }
  }, [cargar]);

  const leerTodas = useCallback(async () => {
    setItems((prev) => prev.map((x) => ({ ...x, leida: true })));
    setNoLeidas(0);
    try {
      await marcarTodasLeidas();
    } catch {
      cargar();
    }
  }, [cargar]);

  return { items, noLeidas, loading, error, leer, leerTodas, recargar: cargar };
}
