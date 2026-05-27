import React, { useEffect, useState } from 'react'
import { Navigate } from 'react-router-dom'
import { fetchCurrentUser } from '../api/authApi.js'
import { HttpError } from '../api/httpError.js'
import { clearAuth, getToken } from '../lib/authStorage.js'

export default function RoleProtectedRoute({ children, role }) {
  const [access, setAccess] = useState('loading')

  useEffect(() => {
    if (!getToken()) {
      setAccess('unauthorized')
      return
    }

    let cancelled = false
    ;(async () => {
      try {
        const user = await fetchCurrentUser()
        const roles = user?.roles ?? user?.Roles ?? []
        if (!cancelled) {
          setAccess(roles.includes(role) ? 'allowed' : 'forbidden')
        }
      } catch (e) {
        if (!cancelled) {
          if (e instanceof HttpError && e.status === 401) {
            clearAuth()
            setAccess('unauthorized')
          } else {
            setAccess('forbidden')
          }
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [role])

  if (access === 'loading') {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-indigo-50 via-blue-50 to-purple-50 text-sm text-slate-500">
        Loading…
      </div>
    )
  }

  if (access === 'unauthorized') {
    return <Navigate to="/login" replace />
  }

  if (access === 'forbidden') {
    return <Navigate to="/app" replace />
  }

  return children
}
