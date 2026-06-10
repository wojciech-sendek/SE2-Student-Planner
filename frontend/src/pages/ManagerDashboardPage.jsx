import React, { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { fetchCurrentUser } from '../api/authApi.js'
import { fetchEventRequests, createEventRequest } from '../api/eventsApi.js'
import { fetchFaculties } from '../api/facultiesApi.js'
import { HttpError, extractErrorMessages } from '../api/httpError.js'
import { clearAuth, getToken } from '../lib/authStorage.js'
import EventRequestFormModal from '../components/EventRequestFormModal.jsx'
import { showError, showInfo, showSuccess } from '../lib/toastStore.js'
import { normalizeRealtimeNotification } from '../lib/realtimeNotificationUtils.js'
import { areEventNotificationsEnabled } from '../lib/notificationPreferences.js'

function normalizeLabel(value) {
  return String(value ?? '').trim().toLowerCase()
}

function getRequestTypeLabel(req) {
  return req.requestType ?? req.RequestType ?? ''
}

function getRequestStatusLabel(req) {
  return req.requestStatus ?? req.status ?? req.Status ?? ''
}

function getTypeBadgeClass(type) {
  const normalized = normalizeLabel(type)
  if (normalized === 'create') return 'bg-emerald-100 text-emerald-800'
  if (normalized === 'update') return 'bg-amber-100 text-amber-800'
  if (normalized === 'delete') return 'bg-red-100 text-red-800'
  return 'bg-slate-100 text-slate-800'
}

function getStatusBadgeClass(status) {
  const normalized = normalizeLabel(status)
  if (normalized === 'approved') return 'bg-emerald-100 text-emerald-800'
  if (normalized === 'rejected') return 'bg-red-100 text-red-800'
  return 'bg-slate-100 text-slate-800'
}

function getRequestErrorMessage(error, fallbackMessage) {
  if (!(error instanceof HttpError)) return fallbackMessage
  const [message] = extractErrorMessages(error.body)
  return message ?? fallbackMessage
}

export default function ManagerDashboardPage() {
  const [user, setUser] = useState(null)
  const [requests, setRequests] = useState([])
  const [faculties, setFaculties] = useState([])
  const [loading, setLoading] = useState(true)
  const [globalError, setGlobalError] = useState(null)
  const [modalOpen, setModalOpen] = useState(false)

  useEffect(() => {
    if (!getToken()) return
    let cancelled = false
    ;(async () => {
      try {
        const [u, r, f] = await Promise.all([
          fetchCurrentUser(),
          fetchEventRequests().catch(() => []), // Provide empty list on fail to not break UI if endpoint missing
          fetchFaculties().catch(() => []),
        ])
        if (!cancelled) {
          setUser(u)
          setRequests(Array.isArray(r) ? r : [])
          setFaculties(Array.isArray(f) ? f : [])
        }
      } catch (e) {
        if (!cancelled && e instanceof HttpError && e.status === 401) {
          clearAuth()
          window.location.assign('/login')
        }
      } finally {
        if (!cancelled) setLoading(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    async function refreshRequests() {
      try {
        const r = await fetchEventRequests()
        setRequests(Array.isArray(r) ? r : [])
      } catch (e) {
        if (e instanceof HttpError && e.status === 401) return
        showError('Refresh failed', 'Could not reload event requests.')
      }
    }

    function handleEventRequestReviewed(event) {
      // ReceiveNotification already shows a toast when notifications are enabled.
      // Show a fallback toast here so the manager is always informed of the review outcome.
      if (!areEventNotificationsEnabled()) {
        const notification = normalizeRealtimeNotification(event?.detail)
        const status = String(notification?.requestStatus ?? '').toLowerCase()
        if (status === 'approved') {
          showSuccess('Request approved', 'Your event request was approved by the admin.')
        } else if (status === 'rejected') {
          showError('Request rejected', 'Your event request was rejected by the admin.')
        } else {
          showInfo('Request reviewed', 'An event request you submitted has been reviewed.')
        }
      }
      refreshRequests()
    }

    window.addEventListener('event-request-reviewed', handleEventRequestReviewed)
    return () => {
      window.removeEventListener('event-request-reviewed', handleEventRequestReviewed)
    }
  }, [])

  function handleLogout() {
    clearAuth()
    window.location.assign('/login')
  }

  async function handleCreateRequest(formData) {
    try {
      const res = await createEventRequest(formData)
      setModalOpen(false)
      setRequests(prev => [...prev, res])
      showSuccess('Request submitted', 'Your event request was sent for admin review.')
    } catch (e) {
      if (e instanceof HttpError && e.status === 401) {
        clearAuth()
        window.location.assign('/login')
        return
      }
      const message = getRequestErrorMessage(e, 'Could not submit the request')
      setGlobalError(message)
      showError('Request failed', message)
    }
  }

  const first = user?.firstName ?? user?.FirstName
  const last = user?.lastName ?? user?.LastName
  const email = user?.email ?? user?.Email

  return (
    <div className="flex min-h-screen flex-col bg-gradient-to-br from-indigo-50 via-blue-50 to-purple-50">
      <header className="border-b border-slate-200 bg-white/95 px-6 py-3 shadow-sm backdrop-blur">
        <div className="mx-auto flex max-w-7xl items-center justify-between">
          <div className="flex items-center gap-3">
            <h1 className="text-lg font-bold text-slate-900">Manager Dashboard</h1>
            {(first || email) && (
              <span className="text-sm text-slate-500">
                {first ? [first, last].filter(Boolean).join(' ') : email}
              </span>
            )}
          </div>
          <div className="flex items-center gap-2">
            <Link
              to="/app"
              className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-50"
            >
              Back to App
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

          <div className="mb-6 flex items-center justify-between">
            <h2 className="text-2xl font-bold text-slate-900">Event Requests</h2>
            <button
              onClick={() => setModalOpen(true)}
              className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-indigo-700 shadow-sm"
            >
              + New Request
            </button>
          </div>

          <div className="rounded-2xl border border-slate-200/80 bg-white/95 p-6 shadow-xl">
            {loading ? (
              <div className="flex h-32 items-center justify-center text-sm text-slate-500">
                Loading requests…
              </div>
            ) : requests.length === 0 ? (
              <div className="flex h-32 items-center justify-center text-sm text-slate-500">
                You have not submitted any event requests.
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm text-slate-600">
                  <thead className="border-b border-slate-200 text-xs font-semibold uppercase text-slate-500">
                    <tr>
                      <th className="px-4 py-3">Title</th>
                      <th className="px-4 py-3">Type</th>
                      <th className="px-4 py-3">Status</th>
                      <th className="px-4 py-3">Date</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {requests.map(req => {
                      const typeLabel = getRequestTypeLabel(req)
                      const statusLabel = getRequestStatusLabel(req)
                      const date = req.submissionDate ? new Date(req.submissionDate).toLocaleDateString() : ''
                      return (
                        <tr key={req.requestId || req.id} className="hover:bg-slate-50/50">
                          <td className="px-4 py-3 font-medium text-slate-900">
                            {req.details?.title ?? req.title ?? '(Unknown)'}
                          </td>
                          <td className="px-4 py-3">
                            <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-semibold ${getTypeBadgeClass(typeLabel)}`}>
                              {typeLabel}
                            </span>
                          </td>
                          <td className="px-4 py-3">
                            <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-semibold ${getStatusBadgeClass(statusLabel)}`}>
                              {statusLabel}
                            </span>
                          </td>
                          <td className="px-4 py-3">{date}</td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      </main>

      {modalOpen && (
        <EventRequestFormModal
          faculties={faculties}
          onSave={handleCreateRequest}
          onCancel={() => setModalOpen(false)}
        />
      )}
    </div>
  )
}
