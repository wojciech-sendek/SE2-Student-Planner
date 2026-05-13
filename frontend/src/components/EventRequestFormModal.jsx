import React, { useState } from 'react'

export default function EventRequestFormModal({ faculties, onSave, onCancel }) {
  const [requestType, setRequestType] = useState(0) // 0=CREATE, 1=UPDATE, 2=DELETE
  const [targetEventId, setTargetEventId] = useState('')
  const [title, setTitle] = useState('')
  const [startTime, setStartTime] = useState('')
  const [endTime, setEndTime] = useState('')
  const [location, setLocation] = useState('')
  const [description, setDescription] = useState('')
  const [facultyId, setFacultyId] = useState(faculties?.[0]?.id ?? '')

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
      payload.details = {
        title,
        startTime: startTime ? new Date(startTime).toISOString() : null,
        endTime: endTime ? new Date(endTime).toISOString() : null,
        location,
        description,
      }
    }

    onSave(payload)
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 p-4 backdrop-blur-sm">
      <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-2xl">
        <h2 className="mb-6 text-xl font-bold text-slate-900">New Event Request</h2>
        <form onSubmit={handleSubmit} className="space-y-4 text-sm">
          <div>
            <label className="mb-1.5 block font-medium text-slate-700">Faculty</label>
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
            <label className="mb-1.5 block font-medium text-slate-700">Request Type</label>
            <select
              value={requestType}
              onChange={e => setRequestType(e.target.value)}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            >
              <option value={0}>Create</option>
              <option value={1}>Update</option>
              <option value={2}>Delete</option>
            </select>
          </div>

          {(Number(requestType) === 1 || Number(requestType) === 2) && (
            <div>
              <label className="mb-1.5 block font-medium text-slate-700">Target Event ID</label>
              <input
                type="text"
                value={targetEventId}
                onChange={e => setTargetEventId(e.target.value)}
                required
                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
              />
            </div>
          )}

          {(Number(requestType) === 0 || Number(requestType) === 1) && (
            <>
              <div>
                <label className="mb-1.5 block font-medium text-slate-700">Title</label>
                <input
                  type="text"
                  value={title}
                  onChange={e => setTitle(e.target.value)}
                  required
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="mb-1.5 block font-medium text-slate-700">Start Time</label>
                  <input
                    type="datetime-local"
                    value={startTime}
                    onChange={e => setStartTime(e.target.value)}
                    required
                    className="w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                </div>
                <div>
                  <label className="mb-1.5 block font-medium text-slate-700">End Time</label>
                  <input
                    type="datetime-local"
                    value={endTime}
                    onChange={e => setEndTime(e.target.value)}
                    required
                    className="w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                </div>
              </div>

              <div>
                <label className="mb-1.5 block font-medium text-slate-700">Location</label>
                <input
                  type="text"
                  value={location}
                  onChange={e => setLocation(e.target.value)}
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
              </div>

              <div>
                <label className="mb-1.5 block font-medium text-slate-700">Description</label>
                <textarea
                  value={description}
                  onChange={e => setDescription(e.target.value)}
                  rows="3"
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
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
