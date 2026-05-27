import { apiUrl } from '../config.js'
import { HttpError, readJsonResponse } from './httpError.js'
import { authHeaders } from '../lib/authStorage.js'

const FACULTY_EVENTS_PATH = '/api/faculty-events'

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

function buildQuery({ facultyId, from, to } = {}) {
  const params = new URLSearchParams()
  if (facultyId != null) params.set('facultyId', String(facultyId))
  if (from) params.set('from', from)
  if (to) params.set('to', to)
  const query = params.toString()
  return query ? `?${query}` : ''
}

export async function fetchAvailableFacultyEvents(filters = {}) {
  const data = await authedFetch(`${FACULTY_EVENTS_PATH}${buildQuery(filters)}`)
  return Array.isArray(data) ? data : []
}

export async function fetchSubscribedFacultyEvents(filters = {}) {
  const data = await authedFetch(`${FACULTY_EVENTS_PATH}/subscribed${buildQuery(filters)}`)
  return Array.isArray(data) ? data : []
}

export async function subscribeToFacultyEvent(id) {
  return authedFetch(`${FACULTY_EVENTS_PATH}/${id}/subscribe`, {
    method: 'POST',
  })
}

export async function unsubscribeFromFacultyEvent(id) {
  return authedFetch(`${FACULTY_EVENTS_PATH}/${id}/subscribe`, {
    method: 'DELETE',
  })
}
