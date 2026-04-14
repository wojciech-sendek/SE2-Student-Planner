export class HttpError extends Error {
  constructor(status, body) {
    super(typeof body?.message === 'string' ? body.message : `Request failed (${status})`)
    this.name = 'HttpError'
    this.status = status
    this.body = body
  }
}

export async function readJsonResponse(res) {
  const text = await res.text()
  if (!text) return null
  try {
    return JSON.parse(text)
  } catch {
    return { message: text }
  }
}

/** Normalizes ASP.NET validation / BadRequest payloads for display. */
export function extractErrorMessages(body) {
  if (!body) return []
  const errs = body.errors ?? body.Errors
  if (Array.isArray(errs)) return errs.map(String).filter(Boolean)
  if (errs && typeof errs === 'object') {
    return Object.values(errs)
      .flatMap((v) => (Array.isArray(v) ? v : [v]))
      .filter(Boolean)
      .map(String)
  }
  const m = body.message ?? body.Message ?? body.title ?? body.Title
  if (typeof m === 'string' && m.trim()) return [m.trim()]
  return []
}
