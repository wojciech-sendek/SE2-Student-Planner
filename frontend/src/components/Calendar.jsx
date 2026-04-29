import React, { useState, useEffect, useCallback } from 'react'
import {
  fetchAllEvents,
  createPersonalEvent,
  updatePersonalEvent,
  deletePersonalEvent,
} from '../api/eventsApi.js'
import { extractErrorMessages, HttpError } from '../api/httpError.js'
import { clearAuth } from '../lib/authStorage.js'
import EventFormModal from './EventFormModal.jsx'
import EventDetailsModal from './EventDetailsModal.jsx'
import ConfirmDeleteModal from './ConfirmDeleteModal.jsx'

const WEEKDAYS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']
const MONTHS = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
]
const MAX_VISIBLE = 3

function isPersonalEvent(event) {
  if (!event) return false
  if (event.isPersonal === true || event.IsPersonal === true) return true
  const type = String(
    event.eventType ?? event.EventType ?? event.type ?? event.Type ?? ''
  ).toLowerCase()
  return type === 'personal' || type === 'personalevent' || type === 'personal_event'
}

function getField(event, ...keys) {
  for (const key of keys) {
    if (event[key] !== undefined) return event[key]
    const pascal = key[0].toUpperCase() + key.slice(1)
    if (event[pascal] !== undefined) return event[pascal]
  }
  return undefined
}

function normalizeEvent(event) {
  return {
    id: getField(event, 'id'),
    title: getField(event, 'title') ?? '(No title)',
    startTime: getField(event, 'startTime'),
    endTime: getField(event, 'endTime'),
    location: getField(event, 'location') ?? '',
    description: getField(event, 'description') ?? '',
    eventType: String(getField(event, 'eventType', 'type') ?? ''),
    isPersonal: isPersonalEvent(event),
  }
}

function getCalendarDays(year, month) {
  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)
  const startDow = firstDay.getDay()
  const days = []

  for (let i = startDow; i > 0; i--) {
    days.push({ date: new Date(year, month, 1 - i), inMonth: false })
  }
  for (let d = 1; d <= lastDay.getDate(); d++) {
    days.push({ date: new Date(year, month, d), inMonth: true })
  }
  const remaining = (7 - (days.length % 7)) % 7
  for (let i = 1; i <= remaining; i++) {
    days.push({ date: new Date(year, month + 1, i), inMonth: false })
  }

  return days
}

function isSameDay(a, b) {
  return (
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate()
  )
}

function getEventsForDay(events, date) {
  return events.filter(e => {
    const start = e.startTime ? new Date(e.startTime) : null
    return start && isSameDay(start, date)
  })
}

function eventChipClass(event) {
  if (event.isPersonal) return 'bg-indigo-500 hover:bg-indigo-600'
  const t = event.eventType.toLowerCase()
  if (t.includes('faculty')) return 'bg-emerald-500 hover:bg-emerald-600'
  if (t.includes('usos') || t.includes('class')) return 'bg-amber-500 hover:bg-amber-600'
  return 'bg-slate-400 hover:bg-slate-500'
}

function formatTime(isoStr) {
  if (!isoStr) return ''
  return new Date(isoStr).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

function getRequestErrorMessage(error, fallbackMessage) {
  if (!(error instanceof HttpError)) return fallbackMessage
  const [message] = extractErrorMessages(error.body)
  return message ?? fallbackMessage
}

export default function Calendar() {
  const today = new Date()
  const [currentMonth, setCurrentMonth] = useState(
    new Date(today.getFullYear(), today.getMonth(), 1)
  )
  const [events, setEvents] = useState([])
  const [loading, setLoading] = useState(true)
  const [globalError, setGlobalError] = useState(null)
  const [modal, setModal] = useState(null)

  const loadEvents = useCallback(async () => {
    setLoading(true)
    try {
      const data = await fetchAllEvents()
      const list = Array.isArray(data) ? data : (data?.events ?? data?.Events ?? [])
      setEvents(list.map(normalizeEvent))
      setGlobalError(null)
    } catch (e) {
      if (e instanceof HttpError && e.status === 401) {
        clearAuth()
        window.location.assign('/login')
        return
      }
      setGlobalError(getRequestErrorMessage(e, 'Could not load events'))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    loadEvents()
  }, [loadEvents])

  async function handleCreate(formData) {
    try {
      const res = await createPersonalEvent(formData)
      setModal(null)
      const created = res ? normalizeEvent(res) : null
      if (created?.id) {
        setEvents(prev => [...prev, created])
      } else {
        await loadEvents()
      }
    } catch (e) {
      if (e instanceof HttpError && e.status === 401) {
        clearAuth()
        window.location.assign('/login')
        return
      }
      setModal(null)
      setGlobalError(getRequestErrorMessage(e, 'Could not create the event'))
    }
  }

  async function handleUpdate(id, formData) {
    try {
      const res = await updatePersonalEvent(id, formData)
      setModal(null)
      const updated = res ? normalizeEvent(res) : null
      if (updated?.id) {
        setEvents(prev => prev.map(e => (e.id === id ? updated : e)))
      } else {
        await loadEvents()
      }
    } catch (e) {
      if (e instanceof HttpError && e.status === 401) {
        clearAuth()
        window.location.assign('/login')
        return
      }
      setModal(null)
      setGlobalError(getRequestErrorMessage(e, 'Could not update the event'))
    }
  }

  async function handleDelete(id) {
    try {
      await deletePersonalEvent(id)
      setModal(null)
      setEvents(prev => prev.filter(e => e.id !== id))
    } catch (e) {
      if (e instanceof HttpError && e.status === 401) {
        clearAuth()
        window.location.assign('/login')
        return
      }
      setModal(null)
      setGlobalError(getRequestErrorMessage(e, 'Could not delete the event'))
    }
  }

  const days = getCalendarDays(currentMonth.getFullYear(), currentMonth.getMonth())

  return (
    <div className="flex flex-col">
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

      {/* Header */}
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <button
            onClick={() =>
              setCurrentMonth(d => new Date(d.getFullYear(), d.getMonth() - 1, 1))
            }
            aria-label="Previous month"
            className="rounded-lg border border-slate-200 p-2 text-slate-600 transition-colors hover:bg-slate-50"
          >
            &#8592;
          </button>
          <h2 className="w-48 text-center text-xl font-bold text-slate-900">
            {MONTHS[currentMonth.getMonth()]} {currentMonth.getFullYear()}
          </h2>
          <button
            onClick={() =>
              setCurrentMonth(d => new Date(d.getFullYear(), d.getMonth() + 1, 1))
            }
            aria-label="Next month"
            className="rounded-lg border border-slate-200 p-2 text-slate-600 transition-colors hover:bg-slate-50"
          >
            &#8594;
          </button>
          <button
            onClick={() =>
              setCurrentMonth(new Date(today.getFullYear(), today.getMonth(), 1))
            }
            className="rounded-lg border border-slate-200 px-3 py-1.5 text-sm text-slate-600 transition-colors hover:bg-slate-50"
          >
            Today
          </button>
        </div>
        <button
          onClick={() => setModal({ type: 'create' })}
          className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-indigo-700"
        >
          + Create Event
        </button>
      </div>

      {/* Legend */}
      <div className="mb-3 flex flex-wrap items-center gap-3 text-xs text-slate-500">
        <span className="flex items-center gap-1.5">
          <span className="inline-block h-2.5 w-2.5 rounded-full bg-indigo-500" />
          Personal
        </span>
        <span className="flex items-center gap-1.5">
          <span className="inline-block h-2.5 w-2.5 rounded-full bg-emerald-500" />
          Faculty
        </span>
        <span className="flex items-center gap-1.5">
          <span className="inline-block h-2.5 w-2.5 rounded-full bg-amber-500" />
          Classes
        </span>
        <span className="flex items-center gap-1.5">
          <span className="inline-block h-2.5 w-2.5 rounded-full bg-slate-400" />
          Other
        </span>
      </div>

      {/* Weekday labels */}
      <div className="grid grid-cols-7">
        {WEEKDAYS.map(d => (
          <div
            key={d}
            className="py-2 text-center text-xs font-semibold uppercase tracking-wide text-slate-500"
          >
            {d}
          </div>
        ))}
      </div>

      {/* Grid */}
      {loading ? (
        <div className="flex h-64 items-center justify-center text-sm text-slate-500">
          Loading events…
        </div>
      ) : (
        <div className="grid grid-cols-7 border-l border-t border-slate-200">
          {days.map(({ date, inMonth }) => {
            const dayEvents = getEventsForDay(events, date)
            const visible = dayEvents.slice(0, MAX_VISIBLE)
            const overflow = dayEvents.length - visible.length
            const isToday = isSameDay(date, today)

            return (
              <div
                key={date.toISOString()}
                className={`min-h-24 border-b border-r border-slate-200 p-1.5 ${
                  inMonth ? 'bg-white' : 'bg-slate-50/60'
                }`}
              >
                <div
                  className={`mb-1 flex h-7 w-7 items-center justify-center rounded-full text-sm font-medium ${
                    isToday
                      ? 'bg-indigo-600 text-white'
                      : inMonth
                        ? 'text-slate-900'
                        : 'text-slate-400'
                  }`}
                >
                  {date.getDate()}
                </div>
                <div className="space-y-0.5">
                  {visible.map(event => (
                    <button
                      key={event.id ?? `${event.title}-${event.startTime}`}
                      onClick={() => setModal({ type: 'details', event })}
                      title={event.title}
                      className={`w-full truncate rounded px-1.5 py-0.5 text-left text-xs text-white transition-colors ${eventChipClass(event)}`}
                    >
                      {event.startTime && (
                        <span className="mr-1 opacity-80">{formatTime(event.startTime)}</span>
                      )}
                      {event.title}
                    </button>
                  ))}
                  {overflow > 0 && (
                    <p className="px-1 text-xs text-slate-500">+{overflow} more</p>
                  )}
                </div>
              </div>
            )
          })}
        </div>
      )}

      {/* Modals */}
      {modal?.type === 'create' && (
        <EventFormModal
          title="Create Event"
          onSave={handleCreate}
          onCancel={() => setModal(null)}
        />
      )}
      {modal?.type === 'details' && (
        <EventDetailsModal
          event={modal.event}
          onClose={() => setModal(null)}
          onEdit={() => setModal({ type: 'edit', event: modal.event })}
          onDelete={() => setModal({ type: 'confirmDelete', event: modal.event })}
        />
      )}
      {modal?.type === 'edit' && (
        <EventFormModal
          title="Edit Event"
          initialValues={modal.event}
          onSave={formData => handleUpdate(modal.event.id, formData)}
          onCancel={() => setModal(null)}
        />
      )}
      {modal?.type === 'confirmDelete' && (
        <ConfirmDeleteModal
          onConfirm={() => handleDelete(modal.event.id)}
          onCancel={() => setModal(null)}
        />
      )}
    </div>
  )
}
