export function normalizeLabel(value) {
  return String(value ?? '').trim().toLowerCase()
}

export function getRequestId(req) {
  return req.requestId ?? req.id ?? req.Id
}

export function getRequestTypeLabel(req) {
  return req.requestType ?? req.RequestType ?? ''
}

export function getRequestStatusLabel(req) {
  return req.requestStatus ?? req.status ?? req.Status ?? ''
}

export function getRequestTitle(req) {
  return req.details?.title ?? req.title ?? req.Title ?? '(Unknown)'
}

export function getRequestFacultyLabel(req) {
  return req.facultyDisplayName ?? req.FacultyDisplayName ?? req.facultyName ?? req.FacultyName ?? ''
}

export function getTypeBadgeClass(type) {
  const normalized = normalizeLabel(type)
  if (normalized === 'create') return 'bg-emerald-100 text-emerald-800'
  if (normalized === 'update') return 'bg-amber-100 text-amber-800'
  if (normalized === 'delete') return 'bg-red-100 text-red-800'
  return 'bg-slate-100 text-slate-800'
}

export function getStatusBadgeClass(status) {
  const normalized = normalizeLabel(status)
  if (normalized === 'approved') return 'bg-emerald-100 text-emerald-800'
  if (normalized === 'rejected') return 'bg-red-100 text-red-800'
  return 'bg-slate-100 text-slate-800'
}

export function formatEventWhen(startTime, endTime) {
  const start = new Date(startTime)
  const end = new Date(endTime)
  if (Number.isNaN(start.getTime())) return ''
  const datePart = start.toLocaleDateString()
  const startTimePart = start.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  const endTimePart = end.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  return `${datePart}, ${startTimePart} – ${endTimePart}`
}
