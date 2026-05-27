import { apiUrl } from '../config.js'
import { HttpError, readJsonResponse } from './httpError.js'
import { authHeaders } from '../lib/authStorage.js'

const ADMIN_EVENT_REQUESTS_PATH = '/api/admin/event-requests'
const ADMIN_USERS_PATH = '/api/admin/users'

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

export async function fetchAdminEventRequests({ status, facultyId } = {}) {
  const params = new URLSearchParams()
  if (status) params.set('status', status)
  if (facultyId != null) params.set('facultyId', String(facultyId))
  const query = params.toString()
  const path = query ? `${ADMIN_EVENT_REQUESTS_PATH}?${query}` : ADMIN_EVENT_REQUESTS_PATH
  const data = await authedFetch(path)
  return Array.isArray(data) ? data : []
}

export async function approveEventRequest(id, reviewComment) {
  return authedFetch(`${ADMIN_EVENT_REQUESTS_PATH}/${id}/approve`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(reviewComment ? { reviewComment } : {}),
  })
}

export async function rejectEventRequest(id, reviewComment) {
  return authedFetch(`${ADMIN_EVENT_REQUESTS_PATH}/${id}/reject`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(reviewComment ? { reviewComment } : {}),
  })
}

export async function fetchAdminUsers() {
  const data = await authedFetch(ADMIN_USERS_PATH)
  return Array.isArray(data) ? data : []
}

export async function createManager(payload) {
  return authedFetch(`${ADMIN_USERS_PATH}/managers`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
}

export async function deleteAdminUser(id) {
  return authedFetch(`${ADMIN_USERS_PATH}/${id}`, {
    method: 'DELETE',
  })
}
