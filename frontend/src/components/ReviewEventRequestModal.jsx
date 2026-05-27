import React, { useState } from 'react'
import { formatEventWhen, getRequestFacultyLabel, getRequestTitle, getRequestTypeLabel } from '../lib/eventRequestDisplay.js'

export default function ReviewEventRequestModal({ request, action, onConfirm, onCancel, busy }) {
  const [reviewComment, setReviewComment] = useState('')
  const isApprove = action === 'approve'

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 p-4">
      <div
        className="w-full max-w-lg rounded-2xl border border-slate-200 bg-white p-6 shadow-2xl"
        role="dialog"
        aria-modal="true"
        aria-labelledby="review-request-title"
      >
        <h2 id="review-request-title" className="text-lg font-bold text-slate-900">
          {isApprove ? 'Approve request' : 'Reject request'}
        </h2>
        <p className="mt-1 text-sm text-slate-500">
          {getRequestTypeLabel(request)} · {getRequestFacultyLabel(request)}
        </p>

        <dl className="mt-4 space-y-2 rounded-lg bg-slate-50 p-4 text-sm">
          <div>
            <dt className="font-medium text-slate-500">Title</dt>
            <dd className="text-slate-900">{getRequestTitle(request)}</dd>
          </div>
          <div>
            <dt className="font-medium text-slate-500">When</dt>
            <dd className="text-slate-900">
              {formatEventWhen(
                request.details?.startTime ?? request.startTime,
                request.details?.endTime ?? request.endTime
              )}
            </dd>
          </div>
          {(request.details?.location ?? request.location) && (
            <div>
              <dt className="font-medium text-slate-500">Location</dt>
              <dd className="text-slate-900">{request.details?.location ?? request.location}</dd>
            </div>
          )}
        </dl>

        <label className="mt-4 block text-sm font-medium text-slate-700" htmlFor="review-comment">
          Review comment (optional)
        </label>
        <textarea
          id="review-comment"
          rows={3}
          value={reviewComment}
          onChange={e => setReviewComment(e.target.value)}
          className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200"
          placeholder="Add a note for the manager…"
        />

        <div className="mt-6 flex justify-end gap-2">
          <button
            type="button"
            onClick={onCancel}
            disabled={busy}
            className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            type="button"
            disabled={busy}
            onClick={() => onConfirm(reviewComment.trim() || undefined)}
            className={`rounded-lg px-4 py-2 text-sm font-semibold text-white disabled:opacity-50 ${
              isApprove
                ? 'bg-emerald-600 hover:bg-emerald-700'
                : 'bg-red-600 hover:bg-red-700'
            }`}
          >
            {busy ? 'Saving…' : isApprove ? 'Approve' : 'Reject'}
          </button>
        </div>
      </div>
    </div>
  )
}
