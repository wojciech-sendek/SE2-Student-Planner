import React, { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  approveEventRequest,
  createManager,
  deleteAdminUser,
  fetchAdminEventRequests,
  fetchAdminUsers,
  rejectEventRequest,
} from '../api/adminApi.js'
import { fetchCurrentUser } from '../api/authApi.js'
import { fetchFaculties } from '../api/facultiesApi.js'
import { HttpError, extractErrorMessages } from '../api/httpError.js'
import ReviewEventRequestModal from '../components/ReviewEventRequestModal.jsx'
import { clearAuth, getToken } from '../lib/authStorage.js'
import {
  formatEventWhen,
  getRequestFacultyLabel,
  getRequestId,
  getRequestStatusLabel,
  getRequestTitle,
  getRequestTypeLabel,
  getStatusBadgeClass,
  getTypeBadgeClass,
  normalizeLabel,
} from '../lib/eventRequestDisplay.js'

const STATUS_FILTERS = [
  { value: '', label: 'All statuses' },
  { value: 'Pending', label: 'Pending' },
  { value: 'Approved', label: 'Approved' },
  { value: 'Rejected', label: 'Rejected' },
]

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

function getUserRoles(user) {
  return user.roles ?? user.Roles ?? []
}

function getUserEmail(user) {
  return user.email ?? user.Email ?? ''
}

function getUserId(user) {
  return user.id ?? user.Id
}

export default function AdminDashboardPage() {
  const [user, setUser] = useState(null)
  const [faculties, setFaculties] = useState([])
  const [requests, setRequests] = useState([])
  const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(true)
  const [usersLoading, setUsersLoading] = useState(false)
  const [globalError, setGlobalError] = useState(null)
  const [activeTab, setActiveTab] = useState('moderation')
  const [facultyFilter, setFacultyFilter] = useState(null)
  const [statusFilter, setStatusFilter] = useState('')
  const [reviewTarget, setReviewTarget] = useState(null)
  const [reviewBusy, setReviewBusy] = useState(false)
  const [managerForm, setManagerForm] = useState({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    facultyId: '',
  })
  const [managerBusy, setManagerBusy] = useState(false)
  const [managerFormError, setManagerFormError] = useState(null)

  function validateManagerForm() {
    const email = managerForm.email.trim()
    if (!email) {
      return 'Email is required.'
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      return 'Enter a valid email address (e.g. name@pw.edu.pl).'
    }
    if (!managerForm.password || managerForm.password.length < 8) {
      return 'Password must be at least 8 characters.'
    }
    if (!managerForm.facultyId) {
      return 'Select a faculty for the manager.'
    }
    return null
  }

  const loadRequests = useCallback(async () => {
    const data = await fetchAdminEventRequests({
      status: statusFilter || undefined,
      facultyId: facultyFilter ?? undefined,
    })
    setRequests(data)
  }, [facultyFilter, statusFilter])

  const loadUsers = useCallback(async () => {
    setUsersLoading(true)
    try {
      const data = await fetchAdminUsers()
      setUsers(data)
    } finally {
      setUsersLoading(false)
    }
  }, [])

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
        if (!cancelled) await loadUsers()
      } catch (e) {
        if (!cancelled && e instanceof HttpError && e.status === 401) {
          clearAuth()
          window.location.assign('/login')
        } else if (!cancelled) {
          setGlobalError(getRequestErrorMessage(e, 'Could not load admin dashboard'))
        }
      }
    })()
    return () => {
      cancelled = true
    }
  }, [loadUsers])

  useEffect(() => {
    if (!getToken()) return
    let cancelled = false
    setLoading(true)
    ;(async () => {
      try {
        await loadRequests()
      } catch (e) {
        if (!cancelled && e instanceof HttpError && e.status === 401) {
          clearAuth()
          window.location.assign('/login')
        } else if (!cancelled) {
          setGlobalError(getRequestErrorMessage(e, 'Could not load event requests'))
        }
      } finally {
        if (!cancelled) setLoading(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [loadRequests])

  function handleLogout() {
    clearAuth()
    window.location.assign('/login')
  }

  async function handleReviewConfirm(reviewComment) {
    if (!reviewTarget) return
    setReviewBusy(true)
    setGlobalError(null)
    try {
      const id = getRequestId(reviewTarget.request)
      const updated =
        reviewTarget.action === 'approve'
          ? await approveEventRequest(id, reviewComment)
          : await rejectEventRequest(id, reviewComment)
      setRequests(prev =>
        prev.map(r => (getRequestId(r) === getRequestId(updated) ? updated : r))
      )
      setReviewTarget(null)
    } catch (e) {
      if (e instanceof HttpError && e.status === 401) {
        clearAuth()
        window.location.assign('/login')
        return
      }
      setGlobalError(getRequestErrorMessage(e, 'Could not review the request'))
    } finally {
      setReviewBusy(false)
    }
  }

  async function handleCreateManager(e) {
    e.preventDefault()
    const validationError = validateManagerForm()
    if (validationError) {
      setManagerFormError(validationError)
      return
    }

    setManagerBusy(true)
    setManagerFormError(null)
    setGlobalError(null)
    try {
      const payload = {
        email: managerForm.email.trim(),
        password: managerForm.password,
        firstName: managerForm.firstName.trim() || undefined,
        lastName: managerForm.lastName.trim() || undefined,
        facultyId: Number(managerForm.facultyId),
      }
      const created = await createManager(payload)
      setUsers(prev => [...prev, created])
      setManagerForm({
        email: '',
        password: '',
        firstName: '',
        lastName: '',
        facultyId: '',
      })
    } catch (err) {
      if (err instanceof HttpError && err.status === 401) {
        clearAuth()
        window.location.assign('/login')
        return
      }
      const message = getRequestErrorMessage(err, 'Could not create manager')
      setManagerFormError(message)
    } finally {
      setManagerBusy(false)
    }
  }

  async function handleDeleteUser(targetUser) {
    const email = getUserEmail(targetUser)
    const id = getUserId(targetUser)
    if (!window.confirm(`Delete user ${email || id}? This cannot be undone.`)) return

    setGlobalError(null)
    try {
      await deleteAdminUser(id)
      setUsers(prev => prev.filter(u => getUserId(u) !== id))
    } catch (e) {
      if (e instanceof HttpError && e.status === 401) {
        clearAuth()
        window.location.assign('/login')
        return
      }
      setGlobalError(getRequestErrorMessage(e, 'Could not delete user'))
    }
  }

  const email = user?.email ?? user?.Email

  return (
    <div className="flex min-h-screen flex-col bg-gradient-to-br from-indigo-50 via-blue-50 to-purple-50">
      <header className="border-b border-slate-200 bg-white/95 px-6 py-3 shadow-sm backdrop-blur">
        <div className="mx-auto flex max-w-7xl items-center justify-between">
          <div className="flex items-center gap-3">
            <h1 className="text-lg font-bold text-slate-900">Admin Dashboard</h1>
            {email && <span className="text-sm text-slate-500">{email}</span>}
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

          <div className="mb-6 flex gap-2 border-b border-slate-200">
            <button
              type="button"
              onClick={() => setActiveTab('moderation')}
              className={`border-b-2 px-4 py-2 text-sm font-semibold transition-colors ${
                activeTab === 'moderation'
                  ? 'border-indigo-600 text-indigo-700'
                  : 'border-transparent text-slate-500 hover:text-slate-700'
              }`}
            >
              Event moderation
            </button>
            <button
              type="button"
              onClick={() => setActiveTab('users')}
              className={`border-b-2 px-4 py-2 text-sm font-semibold transition-colors ${
                activeTab === 'users'
                  ? 'border-indigo-600 text-indigo-700'
                  : 'border-transparent text-slate-500 hover:text-slate-700'
              }`}
            >
              User management
            </button>
          </div>

          {activeTab === 'moderation' && (
            <>
              <div className="mb-4">
                <h2 className="text-2xl font-bold text-slate-900">Event requests</h2>
                <p className="mt-1 text-sm text-slate-500">
                  Review manager submissions and approve or reject changes to the faculty schedule.
                </p>
              </div>

              <div className="mb-4 flex flex-wrap items-center gap-2">
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

              <div className="mb-6 flex flex-wrap items-center gap-2">
                <span className="text-xs font-semibold uppercase tracking-wide text-slate-500">Status</span>
                {STATUS_FILTERS.map(({ value, label }) => (
                  <button
                    key={value || 'all'}
                    type="button"
                    onClick={() => setStatusFilter(value)}
                    className={`rounded-full px-3 py-1 text-xs font-semibold transition-colors ${
                      statusFilter === value
                        ? 'bg-slate-800 text-white'
                        : 'bg-white text-slate-600 ring-1 ring-slate-200 hover:bg-slate-50'
                    }`}
                  >
                    {label}
                  </button>
                ))}
              </div>

              <div className="rounded-2xl border border-slate-200/80 bg-white/95 p-6 shadow-xl">
                {loading ? (
                  <div className="flex h-32 items-center justify-center text-sm text-slate-500">
                    Loading requests…
                  </div>
                ) : requests.length === 0 ? (
                  <div className="flex h-32 items-center justify-center text-sm text-slate-500">
                    No event requests match the current filters.
                  </div>
                ) : (
                  <div className="overflow-x-auto">
                    <table className="w-full text-left text-sm text-slate-600">
                      <thead className="border-b border-slate-200 text-xs font-semibold uppercase text-slate-500">
                        <tr>
                          <th className="px-4 py-3">Title</th>
                          <th className="px-4 py-3">Faculty</th>
                          <th className="px-4 py-3">Type</th>
                          <th className="px-4 py-3">Status</th>
                          <th className="px-4 py-3">When</th>
                          <th className="px-4 py-3">Actions</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-slate-100">
                        {requests.map(req => {
                          const typeLabel = getRequestTypeLabel(req)
                          const statusLabel = getRequestStatusLabel(req)
                          const isPending = normalizeLabel(statusLabel) === 'pending'
                          return (
                            <tr key={getRequestId(req)} className="hover:bg-slate-50/50">
                              <td className="px-4 py-3 font-medium text-slate-900">{getRequestTitle(req)}</td>
                              <td className="px-4 py-3">{getRequestFacultyLabel(req)}</td>
                              <td className="px-4 py-3">
                                <span
                                  className={`inline-flex rounded-full px-2 py-0.5 text-xs font-semibold ${getTypeBadgeClass(typeLabel)}`}
                                >
                                  {typeLabel}
                                </span>
                              </td>
                              <td className="px-4 py-3">
                                <span
                                  className={`inline-flex rounded-full px-2 py-0.5 text-xs font-semibold ${getStatusBadgeClass(statusLabel)}`}
                                >
                                  {statusLabel}
                                </span>
                              </td>
                              <td className="px-4 py-3">
                                {formatEventWhen(
                                  req.details?.startTime ?? req.startTime,
                                  req.details?.endTime ?? req.endTime
                                )}
                              </td>
                              <td className="px-4 py-3">
                                {isPending ? (
                                  <div className="flex flex-wrap gap-2">
                                    <button
                                      type="button"
                                      onClick={() => setReviewTarget({ request: req, action: 'approve' })}
                                      className="rounded-lg bg-emerald-600 px-3 py-1 text-xs font-semibold text-white hover:bg-emerald-700"
                                    >
                                      Approve
                                    </button>
                                    <button
                                      type="button"
                                      onClick={() => setReviewTarget({ request: req, action: 'reject' })}
                                      className="rounded-lg bg-red-600 px-3 py-1 text-xs font-semibold text-white hover:bg-red-700"
                                    >
                                      Reject
                                    </button>
                                  </div>
                                ) : (
                                  <span className="text-xs text-slate-400">—</span>
                                )}
                              </td>
                            </tr>
                          )
                        })}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            </>
          )}

          {activeTab === 'users' && (
            <div className="grid gap-6 lg:grid-cols-2">
              <div className="rounded-2xl border border-slate-200/80 bg-white/95 p-6 shadow-xl">
                <h2 className="text-lg font-bold text-slate-900">Create manager</h2>
                <form className="mt-4 space-y-3" noValidate onSubmit={handleCreateManager}>
                  {managerFormError && (
                    <div
                      className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-800"
                      role="alert"
                    >
                      {managerFormError}
                    </div>
                  )}
                  <div>
                    <label className="text-sm font-medium text-slate-700" htmlFor="mgr-email">
                      Email
                    </label>
                    <input
                      id="mgr-email"
                      type="email"
                      autoComplete="email"
                      value={managerForm.email}
                      onChange={e => {
                        setManagerForm(prev => ({ ...prev, email: e.target.value }))
                        if (managerFormError) setManagerFormError(null)
                      }}
                      aria-invalid={managerFormError ? 'true' : undefined}
                      aria-describedby={managerFormError ? 'mgr-form-error' : undefined}
                      className={`mt-1 w-full rounded-lg border px-3 py-2 text-sm ${
                        managerFormError
                          ? 'border-red-300 focus:border-red-500 focus:ring-red-200'
                          : 'border-slate-300'
                      }`}
                    />
                  </div>
                  <div>
                    <label className="text-sm font-medium text-slate-700" htmlFor="mgr-password">
                      Password
                    </label>
                    <input
                      id="mgr-password"
                      type="password"
                      required
                      minLength={8}
                      value={managerForm.password}
                      onChange={e => setManagerForm(prev => ({ ...prev, password: e.target.value }))}
                      className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                    />
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="text-sm font-medium text-slate-700" htmlFor="mgr-first">
                        First name
                      </label>
                      <input
                        id="mgr-first"
                        type="text"
                        value={managerForm.firstName}
                        onChange={e => setManagerForm(prev => ({ ...prev, firstName: e.target.value }))}
                        className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                      />
                    </div>
                    <div>
                      <label className="text-sm font-medium text-slate-700" htmlFor="mgr-last">
                        Last name
                      </label>
                      <input
                        id="mgr-last"
                        type="text"
                        value={managerForm.lastName}
                        onChange={e => setManagerForm(prev => ({ ...prev, lastName: e.target.value }))}
                        className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                      />
                    </div>
                  </div>
                  <div>
                    <label className="text-sm font-medium text-slate-700" htmlFor="mgr-faculty">
                      Faculty
                    </label>
                    <select
                      id="mgr-faculty"
                      value={managerForm.facultyId}
                      onChange={e => {
                        setManagerForm(prev => ({ ...prev, facultyId: e.target.value }))
                        if (managerFormError) setManagerFormError(null)
                      }}
                      className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                    >
                      <option value="">— Select faculty —</option>
                      {faculties.map(faculty => (
                        <option key={getFacultyId(faculty)} value={getFacultyId(faculty)}>
                          {getFacultyLabel(faculty)}
                        </option>
                      ))}
                    </select>
                  </div>
                  <button
                    type="submit"
                    disabled={managerBusy}
                    className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-700 disabled:opacity-50"
                  >
                    {managerBusy ? 'Creating…' : 'Create manager'}
                  </button>
                </form>
              </div>

              <div className="rounded-2xl border border-slate-200/80 bg-white/95 p-6 shadow-xl">
                <h2 className="text-lg font-bold text-slate-900">Users</h2>
                {usersLoading ? (
                  <p className="mt-4 text-sm text-slate-500">Loading users…</p>
                ) : users.length === 0 ? (
                  <p className="mt-4 text-sm text-slate-500">No users found.</p>
                ) : (
                  <ul className="mt-4 divide-y divide-slate-100">
                    {users.map(u => {
                      const id = getUserId(u)
                      const roles = getUserRoles(u).join(', ')
                      const facultyNames = (u.facultyNames ?? u.FacultyNames ?? []).join(', ')
                      return (
                        <li key={id} className="flex items-start justify-between gap-3 py-3">
                          <div>
                            <p className="font-medium text-slate-900">{getUserEmail(u)}</p>
                            <p className="text-xs text-slate-500">{roles}</p>
                            {facultyNames && (
                              <p className="text-xs text-slate-400">{facultyNames}</p>
                            )}
                          </div>
                          <button
                            type="button"
                            onClick={() => handleDeleteUser(u)}
                            className="shrink-0 rounded-lg border border-red-200 px-2 py-1 text-xs font-semibold text-red-700 hover:bg-red-50"
                          >
                            Delete
                          </button>
                        </li>
                      )
                    })}
                  </ul>
                )}
              </div>
            </div>
          )}
        </div>
      </main>

      {reviewTarget && (
        <ReviewEventRequestModal
          request={reviewTarget.request}
          action={reviewTarget.action}
          busy={reviewBusy}
          onCancel={() => !reviewBusy && setReviewTarget(null)}
          onConfirm={handleReviewConfirm}
        />
      )}
    </div>
  )
}
