import React, { useState, useRef } from 'react'
import { combineDateTime, TIME_OPTIONS } from '../lib/formUtils'

export default function EventRequestFormModal({ faculties, onSave, onCancel }) {
  const [requestType, setRequestType] = useState(0) // 0=CREATE, 1=UPDATE, 2=DELETE
  const [targetEventId, setTargetEventId] = useState('')
  const [facultyId, setFacultyId] = useState(faculties?.[0]?.id ?? '')
  
  const [form, setForm] = useState({
    title: '',
    startDate: '',
    startTime: '',
    endDate: '',
    endTime: '',
    location: '',
    description: '',
  })

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
        return false
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

  function handleSubmit(e) {
    e.preventDefault()
    
    const payload = {
      requestType: Number(requestType),
      facultyId: Number(facultyId),
    }

    if (payload.requestType === 1 || payload.requestType === 2) {
      payload.targetEventId = targetEventId
    }

    if (payload.requestType === 0 || payload.requestType === 1) {
      const startDT = combineDateTime(form.startDate, form.startTime)
      const endDT = combineDateTime(form.endDate, form.endTime)

      payload.details = {
        title: form.title,
        startTime: startDT ? new Date(startDT).toISOString() : null,
        endTime: endDT ? new Date(endDT).toISOString() : null,
        location: form.location,
        description: form.description,
      }
    }

    onSave(payload)
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4 backdrop-blur-sm">
      <div className="w-full max-w-md rounded-2xl bg-white shadow-2xl">
        <div className="border-b border-slate-200 px-6 py-4">
          <h2 className="text-lg font-bold text-slate-900">New Event Request</h2>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="mb-1 block text-sm font-medium text-slate-700">Faculty</label>
              <select
                value={facultyId}
                onChange={e => setFacultyId(e.target.value)}
                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                required
              >
                {faculties?.map(f => (
                  <option key={f.id} value={f.id}>{f.displayName}</option>
                ))}
              </select>
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium text-slate-700">Request Type</label>
              <select
                value={requestType}
                onChange={e => setRequestType(e.target.value)}
                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
              >
                <option value={0}>Create</option>
                <option value={1}>Update</option>
                <option value={2}>Delete</option>
              </select>
            </div>
          </div>

          {(Number(requestType) === 1 || Number(requestType) === 2) && (
            <div>
              <label className="mb-1 block text-sm font-medium text-slate-700">Target Event ID</label>
              <input
                type="text"
                value={targetEventId}
                onChange={e => setTargetEventId(e.target.value)}
                required
                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
              />
            </div>
          )}

          {(Number(requestType) === 0 || Number(requestType) === 1) && (
            <>
              <div>
                <label className="mb-1 block text-sm font-medium text-slate-700">
                  Title <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  name="title"
                  value={form.title}
                  onChange={handleChange}
                  required
                  placeholder="Event title"
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
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
                      required
                      className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                    />
                    <div className="relative">
                      <input
                        ref={startTimeInputRef}
                        type="time"
                        name="startTime"
                        value={form.startTime}
                        onChange={handleChange}
                        required
                        className="w-full rounded-lg border border-slate-300 px-3 py-2 pr-10 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                      />
                      <button
                        type="button"
                        onClick={() => openTimePicker(startTimeInputRef, 'start')}
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
                      required
                      className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                    />
                    <div className="relative">
                      <input
                        ref={endTimeInputRef}
                        type="time"
                        name="endTime"
                        value={form.endTime}
                        onChange={handleChange}
                        required
                        className="w-full rounded-lg border border-slate-300 px-3 py-2 pr-10 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                      />
                      <button
                        type="button"
                        onClick={() => openTimePicker(endTimeInputRef, 'end')}
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
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-medium text-slate-700">Description</label>
                <textarea
                  name="description"
                  value={form.description}
                  onChange={handleChange}
                  rows="3"
                  placeholder="Optional description"
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
              </div>
            </>
          )}

          <div className="flex gap-3 pt-2">
            <button
              type="submit"
              className="flex-1 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-indigo-700"
            >
              Submit Request
            </button>
            <button
              type="button"
              onClick={onCancel}
              className="flex-1 rounded-lg border border-slate-300 px-4 py-2 text-sm font-semibold text-slate-700 transition-colors hover:bg-slate-50"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
