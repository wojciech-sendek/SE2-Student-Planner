import { apiUrl } from '../config.js'
import { HttpError, readJsonResponse } from './httpError.js'
import { authHeaders } from '../lib/authStorage.js'

export async function fetchFaculties() {
  const res = await fetch(apiUrl('/api/Faculties'), {
    method: 'GET',
    headers: {
      Accept: 'application/json',
      ...authHeaders(),
    },
  })
  const data = await readJsonResponse(res)
  if (!res.ok) throw new HttpError(res.status, data)
  return data
}
