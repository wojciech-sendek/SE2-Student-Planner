import { apiUrl } from '../config.js'
import { HttpError, readJsonResponse } from './httpError.js'
import { authHeaders } from '../lib/authStorage.js'

const PERSONAL_EVENTS_PATH = '/api/PersonalEvents'
const ACADEMIC_EVENTS_PATH = '/api/AcademicEvents'

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

function ensurePersonalEventShape(event) {
  if (!event || typeof event !== 'object') return event

  const hasEventType = event.eventType != null || event.EventType != null
  const hasIsPersonal = event.isPersonal != null || event.IsPersonal != null

  if (hasEventType && hasIsPersonal) return event

  return {
    ...event,
    eventType: hasEventType ? event.eventType ?? event.EventType : 'personal',
    isPersonal: hasIsPersonal ? event.isPersonal ?? event.IsPersonal : true,
  }
}

export async function fetchPersonalEvents() {
  const data = await authedFetch(PERSONAL_EVENTS_PATH)
  return Array.isArray(data) ? data.map(ensurePersonalEventShape) : ensurePersonalEventShape(data)
}

export async function fetchAcademicEvents() {
  return authedFetch(ACADEMIC_EVENTS_PATH)
}

export async function fetchAllEvents() {
  const [personal, academic] = await Promise.all([
    fetchPersonalEvents(),
    fetchAcademicEvents(),
  ])
  const personalList = Array.isArray(personal) ? personal : personal ? [personal] : []
  const academicList = Array.isArray(academic) ? academic : academic ? [academic] : []
  return [...personalList, ...academicList]
}

export async function createPersonalEvent(details) {
  const data = await authedFetch(PERSONAL_EVENTS_PATH, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(details),
  })
  return ensurePersonalEventShape(data)
}

export async function updatePersonalEvent(id, details) {
  const data = await authedFetch(`${PERSONAL_EVENTS_PATH}/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(details),
  })
  return ensurePersonalEventShape(data)
}

export function deletePersonalEvent(id) {
  return authedFetch(`${PERSONAL_EVENTS_PATH}/${id}`, {
    method: 'DELETE',
  })
}
