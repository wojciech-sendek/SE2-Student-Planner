import React from 'react'

function formatDateTime(isoStr) {
  if (!isoStr) return '—'
  return new Date(isoStr).toLocaleString([], {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export default function EventDetailsModal({ event, onClose, onEdit, onDelete }) {
  const { isPersonal, title, startTime, endTime, location, description, eventType } = event

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-md rounded-2xl bg-white shadow-2xl">
        <div className="flex items-center justify-between border-b border-slate-200 px-6 py-4">
          <h2 className="truncate pr-4 text-lg font-bold text-slate-900">{title}</h2>
          <button
            onClick={onClose}
            aria-label="Close"
            className="text-xl leading-none text-slate-400 transition-colors hover:text-slate-600"
          >
            ✕
          </button>
        </div>

        <div className="p-6">
          <dl className="space-y-3 text-sm">
            <div>
              <dt className="font-medium text-slate-500">Start</dt>
              <dd className="text-slate-900">{formatDateTime(startTime)}</dd>
            </div>
            <div>
              <dt className="font-medium text-slate-500">End</dt>
              <dd className="text-slate-900">{formatDateTime(endTime)}</dd>
            </div>
            {location && (
              <div>
                <dt className="font-medium text-slate-500">Location</dt>
                <dd className="text-slate-900">{location}</dd>
              </div>
            )}
            {description && (
              <div>
                <dt className="font-medium text-slate-500">Description</dt>
                <dd className="whitespace-pre-wrap text-slate-900">{description}</dd>
              </div>
            )}
            {!isPersonal && eventType && (
              <div>
                <dt className="font-medium text-slate-500">Type</dt>
                <dd className="capitalize text-slate-900">{eventType}</dd>
              </div>
            )}
          </dl>

          <div className="mt-6 flex gap-3">
            {isPersonal && (
              <>
                <button
                  onClick={onEdit}
                  className="flex-1 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-indigo-700"
                >
                  Edit
                </button>
                <button
                  onClick={onDelete}
                  className="flex-1 rounded-lg border border-red-300 px-4 py-2 text-sm font-semibold text-red-700 transition-colors hover:bg-red-50"
                >
                  Delete
                </button>
              </>
            )}
            <button
              onClick={onClose}
              className="flex-1 rounded-lg border border-slate-300 px-4 py-2 text-sm font-semibold text-slate-700 transition-colors hover:bg-slate-50"
            >
              Close
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
