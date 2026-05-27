import React, { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { fetchCurrentUser } from '../api/authApi.js'
import { fetchFaculties } from '../api/facultiesApi.js'
import {
  fetchAvailableFacultyEvents,
  subscribeToFacultyEvent,
  unsubscribeFromFacultyEvent,
} from '../api/facultyEventsApi.js'
import { HttpError, extractErrorMessages } from '../api/httpError.js'
import { clearAuth, getToken } from '../lib/authStorage.js'
import { formatEventWhen } from '../lib/eventRequestDisplay.js'

function getRequestErrorMessage(error, fallbackMessage) {
  if (!(error instanceof HttpError)) return fallbackMessage
  const [message] = extractErrorMessages(error.body)
  return message ?? fallbackMessage
}

function getFacultyId(faculty) {
  return faculty.id ?? faculty.Id
}

function getFacultyLabel(faculty) {
  return faculty.displayName ?? faculty.DisplayName ?? faculty.name ?? faculty.Name ?? 'Faculty'
}

function getEventId(event) {
  return event.id ?? event.Id
}

function getEventField(event, camel, pascal) {
  return event[camel] ?? event[pascal]
}

export default function FacultyEventsPage() {
  const [user, setUser] = useState(null)
  const [faculties, setFaculties] = useState([])
  const [events, setEvents] = useState([])
  const [facultyFilter, setFacultyFilter] = useState(null)
  const [loading, setLoading] = useState(true)
  const [actionId, setActionId] = useState(null)
  const [globalError, setGlobalError] = useState(null)

  const loadEvents = useCallback(async () => {
    const data = await fetchAvailableFacultyEvents({
      facultyId: facultyFilter ?? undefined,
    })
    setEvents(data)
  }, [facultyFilter])

  useEffect(() => {
    if (!getToken()) return
    let cancelled = false
    ;(async () => {
      try {
        const [u, f] = await Promise.all([fetchCurrentUser(), fetchFaculties()])
        if (!cancelled) {
          setUser(u)
          setFaculties(Array.isArray(f) ? f : [])
        }
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

  useEffect(() => {
    if (!getToken()) return
    let cancelled = false
    setLoading(true)
    ;(async () => {
      try {
        await loadEvents()
      } catch (e) {
        if (!cancelled && e instanceof HttpError && e.status === 401) {
          clearAuth()
          window.location.assign('/login')
        } else if (!cancelled) {
          setGlobalError(getRequestErrorMessage(e, 'Could not load faculty events'))
        }
      } finally {
        if (!cancelled) setLoading(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [loadEvents])

  function handleLogout() {
    clearAuth()
    window.location.assign('/login')
  }

  async function handleToggleSubscription(event) {
    const id = getEventId(event)
    const subscribed = getEventField(event, 'isSubscribed', 'IsSubscribed')
    setActionId(id)
    setGlobalError(null)
    try {
      const updated = subscribed
        ? await unsubscribeFromFacultyEvent(id)
        : await subscribeToFacultyEvent(id)
      setEvents(prev => prev.map(e => (getEventId(e) === id ? updated : e)))
    } catch (e) {
      if (e instanceof HttpError && e.status === 401) {
        clearAuth()
        window.location.assign('/login')
        return
      }
      setGlobalError(
        getRequestErrorMessage(
          e,
          subscribed ? 'Could not unsubscribe' : 'Could not subscribe'
        )
      )
    } finally {
      setActionId(null)
    }
  }

  const email = user?.email ?? user?.Email

  return (
    <div className="flex min-h-screen flex-col bg-gradient-to-br from-indigo-50 via-blue-50 to-purple-50">
      <header className="border-b border-slate-200 bg-white/95 px-6 py-3 shadow-sm backdrop-blur">
        <div className="mx-auto flex max-w-7xl items-center justify-between">
          <div className="flex items-center gap-3">
            <h1 className="text-lg font-bold text-slate-900">Faculty Events</h1>
            {email && <span className="text-sm text-slate-500">{email}</span>}
          </div>
          <div className="flex items-center gap-2">
            <Link
              to="/app"
              className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-50"
            >
              Back to calendar
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
        <div className="mx-auto max-w-7xl">
          {globalError && (
            <div className="mb-4 flex items-center justify-between rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
              <span>{globalError}</span>
              <button
                onClick={() => setGlobalError(null)}
                aria-label="Dismiss"
                className="ml-4 font-bold text-red-400 transition-colors hover:text-red-600"
              >
                ✕
              </button>
            </div>
          )}

          <div className="mb-4">
            <h2 className="text-2xl font-bold text-slate-900">Browse & subscribe</h2>
            <p className="mt-1 text-sm text-slate-500">
              Approved faculty events appear on your calendar after you subscribe.
            </p>
          </div>

          <div className="mb-6 flex flex-wrap items-center gap-2">
            <span className="text-xs font-semibold uppercase tracking-wide text-slate-500">Faculty</span>
            <button
              type="button"
              onClick={() => setFacultyFilter(null)}
              className={`rounded-full px-3 py-1 text-xs font-semibold transition-colors ${
                facultyFilter === null
                  ? 'bg-indigo-600 text-white'
                  : 'bg-white text-slate-600 ring-1 ring-slate-200 hover:bg-slate-50'
              }`}
            >
              All
            </button>
            {faculties.map(faculty => {
              const id = getFacultyId(faculty)
              return (
                <button
                  key={id}
                  type="button"
                  onClick={() => setFacultyFilter(id)}
                  className={`rounded-full px-3 py-1 text-xs font-semibold transition-colors ${
                    facultyFilter === id
                      ? 'bg-indigo-600 text-white'
                      : 'bg-white text-slate-600 ring-1 ring-slate-200 hover:bg-slate-50'
                  }`}
                >
                  {getFacultyLabel(faculty)}
                </button>
              )
            })}
          </div>

          <div className="rounded-2xl border border-slate-200/80 bg-white/95 p-6 shadow-xl">
            {loading ? (
              <div className="flex h-32 items-center justify-center text-sm text-slate-500">
                Loading events…
              </div>
            ) : events.length === 0 ? (
              <div className="flex h-32 items-center justify-center text-sm text-slate-500">
                No faculty events available for the selected filters.
              </div>
            ) : (
              <ul className="divide-y divide-slate-100">
                {events.map(event => {
                  const id = getEventId(event)
                  const subscribed = getEventField(event, 'isSubscribed', 'IsSubscribed')
                  const subscriberCount =
                    getEventField(event, 'subscriberCount', 'SubscriberCount') ?? 0
                  const facultyLabel =
                    getEventField(event, 'facultyDisplayName', 'FacultyDisplayName') ??
                    getEventField(event, 'facultyName', 'FacultyName') ??
                    ''
                  const busy = actionId === id

                  return (
                    <li
                      key={id}
                      className="flex flex-col gap-3 py-4 sm:flex-row sm:items-center sm:justify-between"
                    >
                      <div>
                        <h3 className="font-semibold text-slate-900">
                          {getEventField(event, 'title', 'Title')}
                        </h3>
                        <p className="text-sm text-slate-500">{facultyLabel}</p>
                        <p className="mt-1 text-sm text-slate-600">
                          {formatEventWhen(
                            getEventField(event, 'startTime', 'StartTime'),
                            getEventField(event, 'endTime', 'EndTime')
                          )}
                        </p>
                        {getEventField(event, 'location', 'Location') && (
                          <p className="text-sm text-slate-500">
                            {getEventField(event, 'location', 'Location')}
                          </p>
                        )}
                        <p className="mt-1 text-xs text-slate-400">
                          {subscriberCount} subscriber{subscriberCount === 1 ? '' : 's'}
                        </p>
                      </div>
                      <button
                        type="button"
                        disabled={busy}
                        onClick={() => handleToggleSubscription(event)}
                        className={`shrink-0 rounded-lg px-4 py-2 text-sm font-semibold transition-colors disabled:opacity-50 ${
                          subscribed
                            ? 'border border-slate-300 text-slate-700 hover:bg-slate-50'
                            : 'bg-emerald-600 text-white hover:bg-emerald-700'
                        }`}
                      >
                        {busy ? 'Saving…' : subscribed ? 'Unsubscribe' : 'Subscribe'}
                      </button>
                    </li>
                  )
                })}
              </ul>
            )}
          </div>
        </div>
      </main>
    </div>
  )
}
