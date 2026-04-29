import { useRef, useState } from 'react'

function toDatetimeLocal(isoStr) {
  if (!isoStr) return ''
  try {
    const d = new Date(isoStr)
    const pad = n => String(n).padStart(2, '0')
    return (
      `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}` +
      `T${pad(d.getHours())}:${pad(d.getMinutes())}`
    )
  } catch {
    return ''
  }
}

function splitDatetimeLocal(datetimeLocal) {
  if (!datetimeLocal) return { date: '', time: '' }
  const [datePart = '', timePart = ''] = datetimeLocal.split('T')
  return { date: datePart, time: timePart.slice(0, 5) }
}

function combineDateTime(date, time) {
  if (!date || !time) return ''
  return `${date}T${time}`
}

function buildTimeOptions() {
  const options = []
  for (let hour = 0; hour < 24; hour++) {
    for (let minute = 0; minute < 60; minute += 15) {
      options.push(`${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`)
    }
  }
  return options
}

const TIME_OPTIONS = buildTimeOptions()

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
  const [openTimeMenu, setOpenTimeMenu] = useState(null)
  const startTimeInputRef = useRef(null)
  const endTimeInputRef = useRef(null)

  function handleChange(e) {
    const { name, value } = e.target
    setForm(prev => ({ ...prev, [name]: value }))
  }

  function openNativeTimePicker(input) {
    if (!input) return false
    input.focus()
    if (typeof input.showPicker === 'function') {
      try {
        input.showPicker()
        return true
      } catch {
        // Ignore and fall back to custom menu.
      }
    }
    return false
  }

  function handleDateChange(e, nextPickerRef, menuKey) {
    handleChange(e)
    setOpenTimeMenu(menuKey)
    openNativeTimePicker(nextPickerRef.current)
  }

  function openTimePicker(ref, menuKey) {
    setOpenTimeMenu(prev => (prev === menuKey ? null : menuKey))
    openNativeTimePicker(ref.current)
  }

  function setTimeValue(name, value) {
    setForm(prev => ({ ...prev, [name]: value }))
    setOpenTimeMenu(null)
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
              onChange={handleChange}
              placeholder="Event title"
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="mb-1 block text-sm font-medium text-slate-700">
                Start <span className="text-red-500">*</span>
              </label>
              <div className="space-y-2">
                <input
                  type="date"
                  name="startDate"
                  value={form.startDate}
                  onChange={e => handleDateChange(e, startTimeInputRef, 'start')}
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
                <div className="relative">
                  <input
                    ref={startTimeInputRef}
                    type="time"
                    name="startTime"
                    value={form.startTime}
                    onChange={handleChange}
                    className="w-full rounded-lg border border-slate-300 px-3 py-2 pr-10 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                  <button
                    type="button"
                    onClick={() => openTimePicker(startTimeInputRef, 'start')}
                    aria-label="Open start time picker"
                    className="absolute right-2 top-1/2 -translate-y-1/2 rounded p-1 text-slate-500 transition-colors hover:bg-slate-100 hover:text-slate-700"
                  >
                    🕒
                  </button>
                  {openTimeMenu === 'start' && (
                    <div className="absolute left-0 top-full z-30 mt-1 max-h-44 w-full overflow-y-auto rounded-lg border border-slate-200 bg-white shadow-lg">
                      {TIME_OPTIONS.map(option => (
                        <button
                          key={option}
                          type="button"
                          onMouseDown={e => {
                            e.preventDefault()
                            setTimeValue('startTime', option)
                          }}
                          className="block w-full px-3 py-1.5 text-left text-sm text-slate-700 hover:bg-slate-100"
                        >
                          {option}
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-slate-700">
                End <span className="text-red-500">*</span>
              </label>
              <div className="space-y-2">
                <input
                  type="date"
                  name="endDate"
                  value={form.endDate}
                  onChange={e => handleDateChange(e, endTimeInputRef, 'end')}
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
                <div className="relative">
                  <input
                    ref={endTimeInputRef}
                    type="time"
                    name="endTime"
                    value={form.endTime}
                    onChange={handleChange}
                    className="w-full rounded-lg border border-slate-300 px-3 py-2 pr-10 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                  <button
                    type="button"
                    onClick={() => openTimePicker(endTimeInputRef, 'end')}
                    aria-label="Open end time picker"
                    className="absolute right-2 top-1/2 -translate-y-1/2 rounded p-1 text-slate-500 transition-colors hover:bg-slate-100 hover:text-slate-700"
                  >
                    🕒
                  </button>
                  {openTimeMenu === 'end' && (
                    <div className="absolute left-0 top-full z-30 mt-1 max-h-44 w-full overflow-y-auto rounded-lg border border-slate-200 bg-white shadow-lg">
                      {TIME_OPTIONS.map(option => (
                        <button
                          key={option}
                          type="button"
                          onMouseDown={e => {
                            e.preventDefault()
                            setTimeValue('endTime', option)
                          }}
                          className="block w-full px-3 py-1.5 text-left text-sm text-slate-700 hover:bg-slate-100"
                        >
                          {option}
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">Location</label>
            <input
              type="text"
              name="location"
              value={form.location}
              onChange={handleChange}
              placeholder="Optional location"
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">Description</label>
            <textarea
              name="description"
              value={form.description}
              onChange={handleChange}
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
