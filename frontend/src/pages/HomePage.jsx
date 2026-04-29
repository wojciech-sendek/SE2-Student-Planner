import React, { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { fetchCurrentUser } from '../api/authApi.js'
import { HttpError } from '../api/httpError.js'
import { clearAuth, getToken } from '../lib/authStorage.js'
import Calendar from '../components/Calendar.jsx'

export default function HomePage() {
  const [user, setUser] = useState(null)

  useEffect(() => {
    if (!getToken()) return
    let cancelled = false
    ;(async () => {
      try {
        const data = await fetchCurrentUser()
        if (!cancelled) setUser(data)
      } catch (e) {
        if (!cancelled && e instanceof HttpError && e.status === 401) {
          clearAuth()
          window.location.assign('/login')
        }
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

  const first = user?.firstName ?? user?.FirstName
  const last = user?.lastName ?? user?.LastName
  const email = user?.email ?? user?.Email

  return (
    <div className="flex min-h-screen flex-col bg-gradient-to-br from-indigo-50 via-blue-50 to-purple-50">
      <header className="border-b border-slate-200 bg-white/95 px-6 py-3 shadow-sm backdrop-blur">
        <div className="mx-auto flex max-w-7xl items-center justify-between">
          <div className="flex items-center gap-3">
            <h1 className="text-lg font-bold text-slate-900">Student Planner</h1>
            {(first || email) && (
              <span className="text-sm text-slate-500">
                {first ? [first, last].filter(Boolean).join(' ') : email}
              </span>
            )}
          </div>
          <div className="flex items-center gap-2">
            <Link
              to="/settings"
              className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-50"
            >
              Settings
            </Link>
            <button
              type="button"
              onClick={handleLogout}
              className="rounded-lg bg-slate-800 px-3 py-1.5 text-sm font-semibold text-white transition-colors hover:bg-slate-700"
            >
              Sign out
            </button>
          </div>
        </div>
      </header>

      <main className="flex-1 p-6">
        <div className="mx-auto max-w-7xl rounded-2xl border border-slate-200/80 bg-white/95 p-6 shadow-xl">
          <Calendar />
        </div>
      </main>
    </div>
  )
}
