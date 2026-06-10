const DEFAULT_DURATION_MS = 6000

let toasts = []
const listeners = new Set()
const timers = new Map()

function emit() {
  const snapshot = [...toasts]
  listeners.forEach((listener) => listener(snapshot))
}

function createToastId() {
  if (typeof crypto !== 'undefined' && crypto.randomUUID) {
    return crypto.randomUUID()
  }
  return `toast-${Date.now()}-${Math.random().toString(16).slice(2)}`
}

export function clearAllToasts() {
  timers.forEach((timer) => clearTimeout(timer))
  timers.clear()
  toasts = []
  emit()
}

export function dismissToast(id) {
  const timer = timers.get(id)
  if (timer) {
    clearTimeout(timer)
    timers.delete(id)
  }
  toasts = toasts.filter((toast) => toast.id !== id)
  emit()
}

export function addToast({
  title,
  message,
  type = 'info',
  duration = DEFAULT_DURATION_MS,
  id,
}) {
  const toastId = id ?? createToastId()
  const nextToast = {
    id: toastId,
    title: title ?? 'Notification',
    message: message ?? '',
    type,
  }

  toasts = [...toasts.filter((toast) => toast.id !== toastId), nextToast].slice(-5)
  emit()

  if (duration > 0) {
    const existingTimer = timers.get(toastId)
    if (existingTimer) clearTimeout(existingTimer)

    const timer = setTimeout(() => {
      dismissToast(toastId)
    }, duration)
    timers.set(toastId, timer)
  }

  return toastId
}

export function subscribeToasts(listener) {
  listeners.add(listener)
  listener([...toasts])
  return () => listeners.delete(listener)
}

export function showSuccess(title, message, options) {
  return addToast({ title, message, type: 'success', ...options })
}

export function showInfo(title, message, options) {
  return addToast({ title, message, type: 'info', ...options })
}

export function showWarning(title, message, options) {
  return addToast({ title, message, type: 'warning', ...options })
}

export function showError(title, message, options) {
  return addToast({ title, message, type: 'error', ...options })
}
