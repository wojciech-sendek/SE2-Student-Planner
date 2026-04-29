import { apiUrl } from '../config.js'
import { HttpError, readJsonResponse } from './httpError.js'
import { authHeaders } from '../lib/authStorage.js'

async function authedFetch(path, options = {}) {
  const { headers: extraHeaders, ...rest } = options
  const res = await fetch(apiUrl(path), {
    ...rest,
    headers: {
      Accept: 'application/json',
      ...authHeaders(),
      ...extraHeaders,
    },
  })
  const data = await readJsonResponse(res)
  if (!res.ok) throw new HttpError(res.status, data)
  return data
}

export function fetchUsosStatus() {
  return authedFetch('/api/usos/status')
}

export function fetchUsosAuthorizationUrl() {
  return authedFetch('/api/usos/authorization-url')
}

export function syncUsosSchedule() {
  return authedFetch('/api/usos/sync', {
    method: 'POST',
  })
}
