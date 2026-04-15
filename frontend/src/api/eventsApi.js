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

export function fetchAllEvents() {
  return authedFetch('/api/personalevents')
}

export function createPersonalEvent(details) {
  return authedFetch('/api/personalevents', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(details),
  })
}

export function updatePersonalEvent(id, details) {
  return authedFetch(`/api/personalevents/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(details),
  })
}

export async function deletePersonalEvent(id) {
  const res = await fetch(apiUrl(`/api/personalevents/${id}`), {
    method: 'DELETE',
    headers: {
      Accept: 'application/json',
      ...authHeaders(),
    },
  })
  if (res.ok || res.status === 204) return
  const data = await readJsonResponse(res)
  throw new HttpError(res.status, data)
}
