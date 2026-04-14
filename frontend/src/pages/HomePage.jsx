import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { fetchCurrentUser } from '../api/authApi.js'
import { HttpError, extractErrorMessages } from '../api/httpError.js'
import { clearAuth, getToken } from '../lib/authStorage.js'

export default function HomePage() {
  const [user, setUser] = useState(null)
  const [error, setError] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!getToken()) {
      setLoading(false)
      return
    }
    let cancelled = false
    ;(async () => {
      try {
        const data = await fetchCurrentUser()
        if (!cancelled) setUser(data)
      } catch (e) {
        if (!cancelled) {
          if (e instanceof HttpError && e.status === 401) {
            clearAuth()
            setError('Session expired. Please sign in again.')
          } else {
            const msgs = e instanceof HttpError ? extractErrorMessages(e.body) : []
            setError(msgs.length ? msgs.join(' ') : 'Could not load your profile.')
          }
        }
      } finally {
        if (!cancelled) setLoading(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [])

  function handleLogout() {
    clearAuth()
    window.location.assign('/login')
  }

  const email = user?.email ?? user?.Email
  const first = user?.firstName ?? user?.FirstName
  const last = user?.lastName ?? user?.LastName
  const roles = user?.roles ?? user?.Roles ?? []

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-blue-50 to-purple-50 p-6">
      <div className="mx-auto max-w-lg rounded-2xl border border-slate-200/80 bg-white/95 p-8 shadow-xl">
        <h1 className="text-2xl font-bold text-slate-900">Student Planner</h1>
        <p className="mt-1 text-sm text-slate-600">You are connected to the API.</p>

        {loading && <p className="mt-6 text-slate-600">Loading your account…</p>}

        {!loading && error && (
          <div className="mt-6 rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-900">
            {error}{' '}
            <Link to="/login" className="font-medium text-indigo-600 underline">
              Sign in
            </Link>
          </div>
        )}

        {!loading && !error && user && (
          <dl className="mt-6 space-y-2 text-sm">
            <div>
              <dt className="font-medium text-slate-500">Email</dt>
              <dd className="text-slate-900">{email}</dd>
            </div>
            {(first || last) && (
              <div>
                <dt className="font-medium text-slate-500">Name</dt>
                <dd className="text-slate-900">
                  {[first, last].filter(Boolean).join(' ')}
                </dd>
              </div>
            )}
            {Array.isArray(roles) && roles.length > 0 && (
              <div>
                <dt className="font-medium text-slate-500">Roles</dt>
                <dd className="text-slate-900">{roles.join(', ')}</dd>
              </div>
            )}
          </dl>
        )}

        <div className="mt-8 flex flex-wrap gap-3">
          <button
            type="button"
            onClick={handleLogout}
            className="rounded-lg bg-slate-800 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700"
          >
            Sign out
          </button>
          <Link
            to="/settings"
            className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50"
          >
            Settings
          </Link>
          <Link
            to="/login"
            className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50"
          >
            Back to login
          </Link>
        </div>
      </div>
    </div>
  )
}
