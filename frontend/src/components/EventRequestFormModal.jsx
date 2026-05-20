import { useEffect, useState } from 'react'
import { fetchManagerAcademicEvents } from '../api/eventsApi.js'
import DateTimeRangeFields from './DateTimeRangeFields.jsx'
import { combineDateTime, splitDatetimeLocal, toDatetimeLocal } from '../lib/dateTimeFormUtils.js'

function formatEventWhen(startTime, endTime) {
  const start = new Date(startTime)
  const end = new Date(endTime)
  if (Number.isNaN(start.getTime())) return ''
  const datePart = start.toLocaleDateString()
  const timePart = `${start.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} – ${end.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`
  return `${datePart}, ${timePart}`
}

function applyEventToForm(event, setForm) {
  const start = splitDatetimeLocal(toDatetimeLocal(event.startTime))
  const end = splitDatetimeLocal(toDatetimeLocal(event.endTime))
  setForm(prev => ({
    ...prev,
    title: event.title ?? '',
    startDate: start.date,
    startTime: start.time,
    endDate: end.date,
    endTime: end.time,
    location: event.location ?? '',
  }))
}

export default function EventRequestFormModal({ faculties, onSave, onCancel }) {
  const [requestType, setRequestType] = useState(0)
  const [targetEventId, setTargetEventId] = useState(null)
  const [academicEvents, setAcademicEvents] = useState([])
  const [eventsLoading, setEventsLoading] = useState(false)
  const [eventsError, setEventsError] = useState(null)
  const [validationError, setValidationError] = useState(null)
  const [form, setForm] = useState({
    title: '',
    startDate: '',
    startTime: '',
    endDate: '',
    endTime: '',
    location: '',
    description: '',
  })
  const [facultyId, setFacultyId] = useState(faculties?.[0]?.id ?? '')

  const needsTargetEvent = Number(requestType) === 1 || Number(requestType) === 2
  const needsEventDetails = Number(requestType) === 0 || Number(requestType) === 1

  useEffect(() => {
    if (!needsTargetEvent) return

    let cancelled = false
    setEventsLoading(true)
    setEventsError(null)

    ;(async () => {
      try {
        const events = await fetchManagerAcademicEvents()
        if (!cancelled) setAcademicEvents(events)
      } catch {
        if (!cancelled) {
          setAcademicEvents([])
          setEventsError('Could not load academic events.')
        }
      } finally {
        if (!cancelled) setEventsLoading(false)
      }
    })()

    return () => {
      cancelled = true
    }
  }, [needsTargetEvent])

  function handleFieldChange(name, value) {
    setForm(prev => ({ ...prev, [name]: value }))
  }

  function handleRequestTypeChange(value) {
    setRequestType(value)
    setTargetEventId(null)
    setValidationError(null)
    if (Number(value) === 0) {
      setForm({
        title: '',
        startDate: '',
        startTime: '',
        endDate: '',
        endTime: '',
        location: '',
        description: '',
      })
    }
  }

  function handleSelectTargetEvent(event) {
    const id = event.id ?? event.Id
    setTargetEventId(id)
    setValidationError(null)
    if (Number(requestType) === 1) {
      applyEventToForm(event, setForm)
    }
  }

  function handleSubmit(e) {
    e.preventDefault()
    setValidationError(null)

    const payload = {
      requestType: Number(requestType),
      facultyId: Number(facultyId),
    }

    if (needsTargetEvent) {
      if (targetEventId == null) {
        setValidationError('Select an event to update or delete.')
        return
      }
      payload.targetEventId = Number(targetEventId)
    }

    if (needsEventDetails) {
      const startDateTime = combineDateTime(form.startDate, form.startTime)
      const endDateTime = combineDateTime(form.endDate, form.endTime)
      if (!form.title.trim()) {
        setValidationError('Title is required.')
        return
      }
      if (!startDateTime || !endDateTime) {
        setValidationError('Start and end date and time are required.')
        return
      }
      const startDate = new Date(startDateTime)
      const endDate = new Date(endDateTime)
      if (Number.isNaN(startDate.getTime()) || Number.isNaN(endDate.getTime())) {
        setValidationError('Please provide valid start and end times.')
        return
      }
      if (endDate <= startDate) {
        setValidationError('End time must be after start time.')
        return
      }

      payload.details = {
        title: form.title.trim(),
        startTime: startDate.toISOString(),
        endTime: endDate.toISOString(),
        location: form.location.trim(),
        description: form.description.trim(),
      }
    }

    onSave(payload)
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 p-4 backdrop-blur-sm">
      <div className="max-h-[90vh] w-full max-w-md overflow-y-auto rounded-2xl bg-white p-6 shadow-2xl">
        <h2 className="mb-6 text-xl font-bold text-slate-900">New Event Request</h2>
        <form onSubmit={handleSubmit} className="space-y-4 text-sm">
          {validationError && (
            <p className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-800">
              {validationError}
            </p>
          )}

          <div>
            <label className="mb-1 block font-medium text-slate-700">Faculty</label>
            <select
              value={facultyId}
              onChange={e => setFacultyId(e.target.value)}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
              required
            >
              {faculties?.map(f => (
                <option key={f.id} value={f.id}>{f.displayName}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="mb-1 block font-medium text-slate-700">Request Type</label>
            <select
              value={requestType}
              onChange={e => handleRequestTypeChange(e.target.value)}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            >
              <option value={0}>Create</option>
              <option value={1}>Update</option>
              <option value={2}>Delete</option>
            </select>
          </div>

          {needsTargetEvent && (
            <div>
              <label className="mb-1.5 block font-medium text-slate-700">
                Target event <span className="text-red-500">*</span>
              </label>
              {eventsLoading ? (
                <p className="text-sm text-slate-500">Loading events…</p>
              ) : eventsError ? (
                <p className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-800">
                  {eventsError}
                </p>
              ) : academicEvents.length === 0 ? (
                <p className="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-600">
                  No academic events available for your faculty.
                </p>
              ) : (
                <div className="max-h-48 overflow-y-auto rounded-lg border border-slate-200 divide-y divide-slate-100">
                  {academicEvents.map(event => {
                    const id = event.id ?? event.Id
                    const selected = targetEventId === id
                    return (
                      <button
                        key={id}
                        type="button"
                        onClick={() => handleSelectTargetEvent(event)}
                        className={`w-full px-3 py-2.5 text-left transition-colors hover:bg-slate-50 ${
                          selected ? 'bg-indigo-50 ring-1 ring-inset ring-indigo-500' : ''
                        }`}
                      >
                        <div className="font-medium text-slate-900">{event.title}</div>
                        <div className="mt-0.5 text-xs text-slate-500">
                          {formatEventWhen(event.startTime, event.endTime)}
                          {event.location ? ` · ${event.location}` : ''}
                          {event.facultyDisplayName ? ` · ${event.facultyDisplayName}` : ''}
                        </div>
                      </button>
                    )
                  })}
                </div>
              )}
            </div>
          )}

          {needsEventDetails && (
            <>
              <div>
                <label className="mb-1 block font-medium text-slate-700">
                  Title <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  name="title"
                  value={form.title}
                  onChange={e => handleFieldChange('title', e.target.value)}
                  required
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
              </div>

              <DateTimeRangeFields
                startDate={form.startDate}
                startTime={form.startTime}
                endDate={form.endDate}
                endTime={form.endTime}
                onFieldChange={handleFieldChange}
              />

              <div>
                <label className="mb-1 block font-medium text-slate-700">Location</label>
                <input
                  type="text"
                  name="location"
                  value={form.location}
                  onChange={e => handleFieldChange('location', e.target.value)}
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
              </div>

              <div>
                <label className="mb-1 block font-medium text-slate-700">Description</label>
                <textarea
                  name="description"
                  value={form.description}
                  onChange={e => handleFieldChange('description', e.target.value)}
                  rows={3}
                  className="w-full resize-none rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
              </div>
            </>
          )}

          <div className="mt-6 flex justify-end gap-3 pt-2">
            <button
              type="button"
              onClick={onCancel}
              className="rounded-lg px-4 py-2 font-medium text-slate-600 transition-colors hover:bg-slate-100 hover:text-slate-900"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="rounded-lg bg-indigo-600 px-4 py-2 font-medium text-white transition-colors hover:bg-indigo-700"
            >
              Submit Request
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
