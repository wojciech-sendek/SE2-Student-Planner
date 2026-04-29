import { apiUrl } from '../config.js'
import { HttpError, readJsonResponse } from './httpError.js'
import { authHeaders } from '../lib/authStorage.js'

async function postJson(path, body) {
  const res = await fetch(apiUrl(path), {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
    body: JSON.stringify(body),
  })
  const data = await readJsonResponse(res)
  if (!res.ok) throw new HttpError(res.status, data)
  return data
}

export function login({ email, password }) {
  return postJson('/api/Auth/login', { email, password })
}

export function register(payload) {
  return postJson('/api/Auth/register', payload)
}

export function forgotPassword(email) {
  return postJson('/api/Auth/forgot-password', { email })
}

export function resetPassword(payload) {
  return postJson('/api/Auth/reset-password', payload)
}

export async function fetchCurrentUser() {
  const res = await fetch(apiUrl('/api/Auth/me'), {
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
