import React, { useEffect, useRef } from 'react'
import {
  createNotificationConnection,
  startNotificationConnection,
  stopNotificationConnection,
} from '../api/signalRNotifications.js'
import { getToken } from '../lib/authStorage.js'
import { areEventNotificationsEnabled } from '../lib/notificationPreferences.js'
import {
  normalizeRealtimeNotification,
  severityToToastType,
} from '../lib/realtimeNotificationUtils.js'
import { addToast } from '../lib/toastStore.js'

function dispatchRealtimeEvent(name, detail) {
  if (typeof window === 'undefined') return
  window.dispatchEvent(new CustomEvent(name, { detail }))
}

export default function NotificationProvider({ children }) {
  const connectionRef = useRef(null)
  const startingRef = useRef(false)

  useEffect(() => {
    let cancelled = false

    async function ensureConnection() {
      const token = getToken()
      if (!token) {
        if (connectionRef.current) {
          await stopNotificationConnection(connectionRef.current)
          connectionRef.current = null
        }
        return
      }

      if (connectionRef.current || startingRef.current) return

      startingRef.current = true
      const connection = createNotificationConnection({
        onReceiveNotification: (payload) => {
          if (!areEventNotificationsEnabled()) return

          const notification = normalizeRealtimeNotification(payload)
          if (!notification) return

          addToast({
            id: notification.id,
            title: notification.title,
            message: notification.message,
            type: severityToToastType(notification.severity),
          })
        },
        onEventRequestReviewed: (payload) => {
          dispatchRealtimeEvent('event-request-reviewed', payload)
        },
        onAcademicEventChanged: (payload) => {
          dispatchRealtimeEvent('academic-event-changed', payload)
        },
        onReconnected: () => {
          addToast({
            title: 'Reconnected',
            message: 'Live notifications are active again.',
            type: 'info',
            duration: 4000,
          })
        },
        onDisconnected: (error) => {
          if (error && getToken()) {
            addToast({
              title: 'Connection lost',
              message: 'Real-time notifications are temporarily unavailable.',
              type: 'warning',
              duration: 5000,
            })
          }
        },
      })

      try {
        await startNotificationConnection(connection)
        if (cancelled) {
          await stopNotificationConnection(connection)
          return
        }
        connectionRef.current = connection
      } catch (error) {
        if (!cancelled && getToken() && areEventNotificationsEnabled()) {
          console.warn('SignalR notification connection failed:', error)
        }
      } finally {
        startingRef.current = false
      }
    }

    ensureConnection()

    function handleAuthChange() {
      if (!getToken() && connectionRef.current) {
        stopNotificationConnection(connectionRef.current).finally(() => {
          connectionRef.current = null
        })
        return
      }
      if (getToken() && !connectionRef.current) {
        ensureConnection()
      }
    }

    function handleVisibilityChange() {
      if (document.visibilityState === 'visible') {
        ensureConnection()
      }
    }

    window.addEventListener('auth-changed', handleAuthChange)
    window.addEventListener('storage', handleAuthChange)
    window.addEventListener('focus', handleAuthChange)
    document.addEventListener('visibilitychange', handleVisibilityChange)

    return () => {
      cancelled = true
      window.removeEventListener('auth-changed', handleAuthChange)
      window.removeEventListener('storage', handleAuthChange)
      window.removeEventListener('focus', handleAuthChange)
      document.removeEventListener('visibilitychange', handleVisibilityChange)
      if (connectionRef.current) {
        stopNotificationConnection(connectionRef.current)
        connectionRef.current = null
      }
    }
  }, [])

  return children
}
