import React, { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { apiUrl } from '../config.js'
import { readJsonResponse } from '../api/httpError.js'
import { authHeaders, clearAuth } from '../lib/authStorage.js'

export default function SettingsPage() {
  const [isDeleting, setIsDeleting] = useState(false)
  const [error, setError] = useState(null)
  const navigate = useNavigate()

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
