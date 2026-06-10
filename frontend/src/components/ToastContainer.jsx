import React, { useEffect, useState } from 'react'
import { createPortal } from 'react-dom'
import { dismissToast, subscribeToasts } from '../lib/toastStore.js'

const TYPE_STYLES = {
  success: {
    backgroundColor: '#ecfdf5',
    borderColor: '#a7f3d0',
    color: '#022c22',
    iconColor: '#059669',
    iconPath:
      'M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z',
  },
  info: {
    backgroundColor: '#eef2ff',
    borderColor: '#c7d2fe',
    color: '#1e1b4b',
    iconColor: '#4f46e5',
    iconPath:
      'M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z',
  },
  warning: {
    backgroundColor: '#fffbeb',
    borderColor: '#fde68a',
    color: '#451a03',
    iconColor: '#d97706',
    iconPath:
      'M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z',
  },
  error: {
    backgroundColor: '#fef2f2',
    borderColor: '#fecaca',
    color: '#450a0a',
    iconColor: '#dc2626',
    iconPath:
      'M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z',
  },
}

const containerStyle = {
  position: 'fixed',
  top: '1rem',
  right: '1rem',
  zIndex: 2147483647,
  display: 'flex',
  flexDirection: 'column',
  gap: '0.75rem',
  maxWidth: '24rem',
  width: 'calc(100vw - 2rem)',
  pointerEvents: 'none',
}

export default function ToastContainer() {
  const [toasts, setToasts] = useState([])

  useEffect(() => subscribeToasts(setToasts), [])

  if (typeof document === 'undefined' || toasts.length === 0) {
    return null
  }

  return createPortal(
    <div aria-live="polite" aria-relevant="additions" style={containerStyle}>
      {toasts.map((toast) => {
        const styles = TYPE_STYLES[toast.type] ?? TYPE_STYLES.info
        return (
          <div
            key={toast.id}
            role="status"
            style={{
              pointerEvents: 'auto',
              opacity: 1,
              border: `1px solid ${styles.borderColor}`,
              backgroundColor: styles.backgroundColor,
              color: styles.color,
              borderRadius: '0.75rem',
              padding: '0.75rem 1rem',
              boxShadow: '0 10px 15px -3px rgba(15, 23, 42, 0.1)',
            }}
          >
            <div style={{ display: 'flex', alignItems: 'flex-start', gap: '0.75rem' }}>
              <svg
                style={{
                  marginTop: '0.125rem',
                  height: '1.25rem',
                  width: '1.25rem',
                  flexShrink: 0,
                  color: styles.iconColor,
                }}
                fill="currentColor"
                viewBox="0 0 20 20"
                aria-hidden="true"
              >
                <path fillRule="evenodd" d={styles.iconPath} clipRule="evenodd" />
              </svg>
              <div style={{ minWidth: 0, flex: 1 }}>
                <p style={{ fontSize: '0.875rem', fontWeight: 600, margin: 0 }}>
                  {toast.title}
                </p>
                {toast.message ? (
                  <p style={{ margin: '0.25rem 0 0', fontSize: '0.875rem', opacity: 0.9 }}>
                    {toast.message}
                  </p>
                ) : null}
              </div>
              <button
                type="button"
                onClick={() => dismissToast(toast.id)}
                aria-label="Dismiss notification"
                style={{
                  border: 'none',
                  background: 'transparent',
                  color: 'inherit',
                  cursor: 'pointer',
                  fontSize: '1rem',
                  fontWeight: 700,
                  opacity: 0.6,
                  padding: '0 0.25rem',
                }}
              >
                ×
              </button>
            </div>
          </div>
        )
      })}
    </div>,
    document.body,
  )
}
