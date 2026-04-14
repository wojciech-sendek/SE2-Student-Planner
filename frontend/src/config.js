/**
 * Base URL for the Student Planner API (no trailing slash).
 * Leave unset in development to use the Vite proxy (/api → http://localhost:5289).
 * For production on another host, set VITE_API_BASE_URL (e.g. https://api.example.com).
 */
export function getApiBaseUrl() {
  const raw = import.meta.env.VITE_API_BASE_URL
  if (raw == null || String(raw).trim() === '') return ''
  return String(raw).replace(/\/$/, '')
}

export function apiUrl(path) {
  const p = path.startsWith('/') ? path : `/${path}`
  const base = getApiBaseUrl()
  return base ? `${base}${p}` : p
}
