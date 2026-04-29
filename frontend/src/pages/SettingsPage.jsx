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

export default function SettingsPage() {
  const [isDeleting, setIsDeleting] = useState(false)
  const [error, setError] = useState(null)
  const [usosStatus, setUsosStatus] = useState(null)
  const [isLoadingUsos, setIsLoadingUsos] = useState(true)
  const [isConnectingUsos, setIsConnectingUsos] = useState(false)
  const [isSyncingUsos, setIsSyncingUsos] = useState(false)
  const [usosMessage, setUsosMessage] = useState(null)
  const [usosError, setUsosError] = useState(null)
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

  useEffect(() => {
    loadUsosStatus()
  }, [loadUsosStatus])

  async function handleStartUsosAuthorization() {
    setIsConnectingUsos(true)
    setUsosMessage(null)
    setUsosError(null)
    try {
      const data = await fetchUsosAuthorizationUrl()
      const authorizationUrl = data?.authorizationUrl ?? data?.AuthorizationUrl
      const message = data?.message ?? data?.Message

      if (!authorizationUrl) {
        setUsosError(message ?? 'USOS OAuth is not configured on the backend.')
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
      setUsosError(message ?? 'Could not start USOS authorization.')
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
      setUsosMessage(`USOS schedule synchronized (${count} events).`)
      await loadUsosStatus()
    } catch (e) {
      if (e instanceof HttpError && e.status === 401) {
        clearAuth()
        navigate('/login', { replace: true })
        return
      }
      const [message] = e instanceof HttpError ? extractErrorMessages(e.body) : []
      setUsosError(message ?? 'Could not synchronize USOS schedule.')
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
      setError(
        data?.message ??
          data?.Message ??
          'Failed to delete account.',
      )
    } catch {
      setError('Network error. Is the API running?')
    } finally {
      setIsDeleting(false)
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

        <div className="rounded-xl border border-red-200 bg-white p-6 shadow-sm">
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
