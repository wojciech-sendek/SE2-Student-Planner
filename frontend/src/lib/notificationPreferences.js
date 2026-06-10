const NOTIFICATIONS_ENABLED_KEY = 'student-planner-event-notifications-enabled'

export function areEventNotificationsEnabled() {
  if (typeof window === 'undefined') return true
  const stored = localStorage.getItem(NOTIFICATIONS_ENABLED_KEY)
  if (stored === null) return true
  return stored === 'true'
}

export function setEventNotificationsEnabled(enabled) {
  if (typeof window === 'undefined') return
  localStorage.setItem(NOTIFICATIONS_ENABLED_KEY, enabled ? 'true' : 'false')
  window.dispatchEvent(
    new CustomEvent('event-notifications-preference-changed', { detail: { enabled } }),
  )
}
