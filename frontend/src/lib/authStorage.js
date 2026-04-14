const TOKEN_KEY = 'student-planner-auth-token'
const EXPIRES_KEY = 'student-planner-auth-expires'

export function getToken() {
  if (typeof window === 'undefined') return null
  return localStorage.getItem(TOKEN_KEY)
}

export function saveAuthFromResponse(data) {
  const token = data.token ?? data.Token
  const expiresAtUtc = data.expiresAtUtc ?? data.ExpiresAtUtc
  if (token) localStorage.setItem(TOKEN_KEY, token)
  if (expiresAtUtc) localStorage.setItem(EXPIRES_KEY, String(expiresAtUtc))
}

export function clearAuth() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(EXPIRES_KEY)
}

export function authHeaders() {
  const t = getToken()
  return t ? { Authorization: `Bearer ${t}` } : {}
}
