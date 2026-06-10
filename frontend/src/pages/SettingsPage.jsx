import React, { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { apiUrl } from '../config.js'
import { HttpError, extractErrorMessages, readJsonResponse } from '../api/httpError.js'
import { authHeaders, clearAuth } from '../lib/authStorage.js'
import {
  fetchUsosAuthorizationUrl,
  fetchUsosStatus,
  syncUsosSchedule,
} from '../api/usosApi.js'
import { fetchCurrentUser, updateNotificationPreference } from '../api/authApi.js'
import { showError, showSuccess } from '../lib/toastStore.js'
import {
  areEventNotificationsEnabled,
  setEventNotificationsEnabled,
} from '../lib/notificationPreferences.js'

export default function SettingsPage() {
  const [eventNotificationsEnabled, setEventNotificationsEnabledState] = useState(
    () => areEventNotificationsEnabled(),
  )
  const [isDeleting, setIsDeleting] = useState(false)
  const [error, setError] = useState(null)
  const [usosStatus, setUsosStatus] = useState(null)
  const [isLoadingUsos, setIsLoadingUsos] = useState(true)
  const [isConnectingUsos, setIsConnectingUsos] = useState(false)
  const [isSyncingUsos, setIsSyncingUsos] = useState(false)
  const [usosMessage, setUsosMessage] = useState(null)
  const [usosError, setUsosError] = useState(null)
  const [notificationsEnabled, setNotificationsEnabled] = useState(true)
  const [isUpdatingNotifications, setIsUpdatingNotifications] = useState(false)
  const [isLoadingPrefs, setIsLoadingPrefs] = useState(true)
  const navigate = useNavigate()

  const loadUsosStatus = useCallback(async () => {
    setIsLoadingUsos(true)
    setUsosError(null)
    try {
      const data = await fetchUsosStatus()
      setUsosStatus(data ?? null)
    } catch (e) {
      if (e instanceof HttpError && e.status === 401) {
        clearAuth()
        navigate('/login', { replace: true })
        return
      }
      const [message] = e instanceof HttpError ? extractErrorMessages(e.body) : []
      setUsosError(message ?? 'Could not load USOS settings.')
    } finally {
      setIsLoadingUsos(false)
    }
  }, [navigate])

  const loadPreferences = useCallback(async () => {
    setIsLoadingPrefs(true)
    try {
      const user = await fetchCurrentUser()
      setNotificationsEnabled(user.notificationsEnabled ?? user.NotificationsEnabled ?? true)
    } catch (e) {
      if (e instanceof HttpError && e.status === 401) {
        clearAuth()
        navigate('/login', { replace: true })
      }
    } finally {
      setIsLoadingPrefs(false)
    }
  }, [navigate])

  useEffect(() => {
    loadUsosStatus()
    loadPreferences()
  }, [loadUsosStatus, loadPreferences])

  useEffect(() => {
    function handlePreferenceChange(event) {
      setEventNotificationsEnabledState(event.detail?.enabled ?? areEventNotificationsEnabled())
    }

    window.addEventListener('event-notifications-preference-changed', handlePreferenceChange)
    return () => {
      window.removeEventListener('event-notifications-preference-changed', handlePreferenceChange)
    }
  }, [])

  function handleToggleEventNotifications(enabled) {
    setEventNotificationsEnabled(enabled)
    setEventNotificationsEnabledState(enabled)
  }

  async function handleStartUsosAuthorization() {
    setIsConnectingUsos(true)
    setUsosMessage(null)
    setUsosError(null)
    try {
      const data = await fetchUsosAuthorizationUrl()
      const authorizationUrl = data?.authorizationUrl ?? data?.AuthorizationUrl
      const message = data?.message ?? data?.Message

      if (!authorizationUrl) {
        const err = message ?? 'USOS OAuth is not configured on the backend.'
        setUsosError(err)
        showError('USOS connection failed', err)
        return
      }

      const popup = window.open(authorizationUrl, '_blank', 'noopener,noreferrer')
      if (!popup) {
        window.location.assign(authorizationUrl)
        return
      }

      setUsosMessage('USOS authorization opened in a new tab. After approval, click Refresh status.')
    } catch (e) {
      if (e instanceof HttpError && e.status === 401) {
        clearAuth()
        navigate('/login', { replace: true })
        return
      }
      const [message] = e instanceof HttpError ? extractErrorMessages(e.body) : []
      const err = message ?? 'Could not start USOS authorization.'
      setUsosError(err)
      showError('USOS connection failed', err)
    } finally {
      setIsConnectingUsos(false)
    }
  }

  async function handleSyncUsos() {
    setIsSyncingUsos(true)
    setUsosMessage(null)
    setUsosError(null)
    try {
      const events = await syncUsosSchedule()
      const count = Array.isArray(events) ? events.length : 0
      const msg = `USOS schedule synchronized (${count} events).`
      setUsosMessage(msg)
      showSuccess('Schedule synced', msg)
      await loadUsosStatus()
    } catch (e) {
      if (e instanceof HttpError && e.status === 401) {
        clearAuth()
        navigate('/login', { replace: true })
        return
      }
      const [message] = e instanceof HttpError ? extractErrorMessages(e.body) : []
      const err = message ?? 'Could not synchronize USOS schedule.'
      setUsosError(err)
      showError('Sync failed', err)
    } finally {
      setIsSyncingUsos(false)
    }
  }

  async function handleDeleteAccount() {
    if (
      !window.confirm(
        'Are you sure you want to delete your account? This cannot be undone.',
      )
    ) {
      return
    }

    setIsDeleting(true)
    setError(null)

    try {
      const res = await fetch(apiUrl('/api/Auth/delete-account'), {
        method: 'DELETE',
        headers: {
          Accept: 'application/json',
          ...authHeaders(),
        },
      })

      if (res.ok || res.status === 204) {
        clearAuth()
        navigate('/login', { replace: true })
        return
      }

      const data = await readJsonResponse(res)
      const message =
        data?.message ??
        data?.Message ??
        'Failed to delete account.'
      setError(message)
      showError('Account deletion failed', message)
    } catch {
      const message = 'Network error. Is the API running?'
      setError(message)
      showError('Account deletion failed', message)
    } finally {
      setIsDeleting(false)
    }
  }

  async function handleToggleNotifications() {
    const newValue = !notificationsEnabled
    setIsUpdatingNotifications(true)
    try {
      await updateNotificationPreference(newValue)
      setNotificationsEnabled(newValue)
    } catch (e) {
      const [message] = e instanceof HttpError ? extractErrorMessages(e.body) : []
      setError(message ?? 'Failed to update notification preferences.')
    } finally {
      setIsUpdatingNotifications(false)
    }
  }

  return (
    <div className="min-h-screen bg-slate-50 p-6">
      <div className="mx-auto max-w-xl">
        <div className="mb-6 flex items-center justify-between gap-4">
          <h1 className="text-2xl font-bold text-slate-900">User settings</h1>
          <Link
            to="/app"
            className="text-sm font-medium text-indigo-600 hover:text-indigo-500"
          >
            ← Home
          </Link>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="text-lg font-semibold text-slate-900">Notifications</h2>
          <p className="mt-1 text-sm text-slate-600">
            Choose whether you want to receive real-time notifications about event changes.
          </p>
          <div className="mt-4 flex items-center justify-between">
            <div className="flex flex-col">
              <span className="text-sm font-medium text-slate-900">Enable real-time notifications</span>
              <span className="text-xs text-slate-500">Toast notifications and browser alerts</span>
            </div>
            <button
              type="button"
              onClick={handleToggleNotifications}
              disabled={isUpdatingNotifications || isLoadingPrefs}
              className={`relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-indigo-600 focus:ring-offset-2 ${
                notificationsEnabled ? 'bg-indigo-600' : 'bg-slate-200'
              }`}
            >
              <span
                className={`pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out ${
                  notificationsEnabled ? 'translate-x-5' : 'translate-x-0'
                }`}
              />
            </button>
          </div>
        </div>

        <div className="mt-6 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="text-lg font-semibold text-slate-900">Notifications</h2>
          <p className="mt-2 text-sm text-slate-600">
            Control whether live toast notifications appear for faculty events and request updates.
          </p>
          <label className="mt-4 flex items-center justify-between gap-4 rounded-lg border border-slate-200 px-4 py-3">
            <span className="text-sm font-medium text-slate-800">Event notifications</span>
            <button
              type="button"
              role="switch"
              aria-checked={eventNotificationsEnabled}
              onClick={() => handleToggleEventNotifications(!eventNotificationsEnabled)}
              className={`relative h-7 w-12 rounded-full transition-colors ${
                eventNotificationsEnabled ? 'bg-indigo-600' : 'bg-slate-300'
              }`}
            >
              <span
                className={`absolute top-0.5 h-6 w-6 rounded-full bg-white shadow transition-transform ${
                  eventNotificationsEnabled ? 'left-5' : 'left-0.5'
                }`}
              />
            </button>
          </label>
        </div>

        <div className="mt-6 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="text-lg font-semibold text-slate-900">USOS integration</h2>
          {isLoadingUsos ? (
            <p className="mt-2 text-sm text-slate-600">Loading USOS status…</p>
          ) : (
            <p className="mt-2 text-sm text-slate-600">
              Status:{' '}
              <span className={usosStatus?.isConnected || usosStatus?.IsConnected ? 'font-semibold text-emerald-700' : 'font-semibold text-amber-700'}>
                {usosStatus?.isConnected || usosStatus?.IsConnected ? 'Connected' : 'Not connected'}
              </span>{' '}
              • Synced classes:{' '}
              <span className="font-semibold text-slate-900">
                {usosStatus?.syncedEventsCount ?? usosStatus?.SyncedEventsCount ?? 0}
              </span>
            </p>
          )}
          {(usosError || usosMessage) && (
            <p className={`mt-3 text-sm ${usosError ? 'text-red-600' : 'text-emerald-700'}`} role={usosError ? 'alert' : 'status'}>
              {usosError ?? usosMessage}
            </p>
          )}
          <div className="mt-4 flex flex-wrap gap-2">
            <button
              type="button"
              onClick={handleStartUsosAuthorization}
              disabled={isConnectingUsos || isLoadingUsos}
              className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isConnectingUsos ? 'Opening USOS…' : 'Connect / reconnect USOS'}
            </button>
            <button
              type="button"
              onClick={handleSyncUsos}
              disabled={isSyncingUsos || isLoadingUsos}
              className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSyncingUsos ? 'Syncing…' : 'Sync now'}
            </button>
            <button
              type="button"
              onClick={loadUsosStatus}
              disabled={isLoadingUsos}
              className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            >
              Refresh status
            </button>
          </div>
        </div>

        <div className="mt-6 rounded-xl border border-red-200 bg-white p-6 shadow-sm">
          <h2 className="text-lg font-semibold text-red-700">Danger zone</h2>
          <p className="mt-2 text-sm text-slate-600">
            Deleting your account removes your access permanently.
          </p>
          {error && (
            <p className="mt-3 text-sm text-red-600" role="alert">
              {error}
            </p>
          )}
          <button
            type="button"
            onClick={handleDeleteAccount}
            disabled={isDeleting}
            className="mt-4 rounded-lg bg-red-600 px-4 py-2 text-sm font-semibold text-white hover:bg-red-500 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isDeleting ? 'Deleting…' : 'Delete my account'}
          </button>
        </div>
      </div>
    </div>
  )
}
