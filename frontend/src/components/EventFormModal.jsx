import { useState } from 'react'
import DateTimeRangeFields from './DateTimeRangeFields.jsx'
import { combineDateTime, splitDatetimeLocal, toDatetimeLocal } from '../lib/dateTimeFormUtils.js'

export default function EventFormModal({ title, initialValues, onSave, onCancel }) {
  const initialStart = splitDatetimeLocal(toDatetimeLocal(initialValues?.startTime))
  const initialEnd = splitDatetimeLocal(toDatetimeLocal(initialValues?.endTime))

  const [form, setForm] = useState({
    title: initialValues?.title ?? '',
    startDate: initialStart.date,
    startTime: initialStart.time,
    endDate: initialEnd.date,
    endTime: initialEnd.time,
    location: initialValues?.location ?? '',
    description: initialValues?.description ?? '',
  })
  const [saving, setSaving] = useState(false)
  const [validationError, setValidationError] = useState(null)

  function handleFieldChange(name, value) {
    setForm(prev => ({ ...prev, [name]: value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    const startDateTime = combineDateTime(form.startDate, form.startTime)
    const endDateTime = combineDateTime(form.endDate, form.endTime)
    if (!form.title.trim()) { setValidationError('Title is required.'); return }
    if (!startDateTime) { setValidationError('Start date and time are required.'); return }
    if (!endDateTime) { setValidationError('End date and time are required.'); return }

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

    setSaving(true)
    setValidationError(null)
    try {
      await onSave({
        title: form.title.trim(),
        startTime: startDate.toISOString(),
        endTime: endDate.toISOString(),
        location: form.location.trim(),
        description: form.description.trim(),
      })
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-md rounded-2xl bg-white shadow-2xl">
        <div className="border-b border-slate-200 px-6 py-4">
          <h2 className="text-lg font-bold text-slate-900">{title}</h2>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {validationError && (
            <p className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-800">
              {validationError}
            </p>
          )}

          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">
              Title <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              name="title"
              value={form.title}
              onChange={e => handleFieldChange('title', e.target.value)}
              placeholder="Event title"
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
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
            <label className="mb-1 block text-sm font-medium text-slate-700">Location</label>
            <input
              type="text"
              name="location"
              value={form.location}
              onChange={e => handleFieldChange('location', e.target.value)}
              placeholder="Optional location"
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">Description</label>
            <textarea
              name="description"
              value={form.description}
              onChange={e => handleFieldChange('description', e.target.value)}
              rows={3}
              placeholder="Optional description"
              className="w-full resize-none rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            />
          </div>

          <div className="flex gap-3 pt-2">
            <button
              type="submit"
              disabled={saving}
              className="flex-1 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-indigo-700 disabled:opacity-60"
            >
              {saving ? 'Saving…' : 'Save'}
            </button>
            <button
              type="button"
              onClick={onCancel}
              disabled={saving}
              className="flex-1 rounded-lg border border-slate-300 px-4 py-2 text-sm font-semibold text-slate-700 transition-colors hover:bg-slate-50 disabled:opacity-60"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
