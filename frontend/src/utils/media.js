const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000";

export const mediaUrl = (path) => {
  if (!path) return null;
  return /^https?:\/\//i.test(path) ? path : `${API_URL}${path}`;
};