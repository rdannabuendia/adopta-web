import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";

export function useFlashMessage() {
  const location = useLocation();
  const navigate = useNavigate();
  const [mensaje] = useState(location.state?.mensaje ?? "");

  useEffect(() => {
    if (location.state?.mensaje) navigate(location.pathname + location.search, { replace: true, state: null });
  }, []);

  return mensaje;
}