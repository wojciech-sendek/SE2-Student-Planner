export function normalizeRealtimeNotification(payload) {
  if (!payload || typeof payload !== 'object') return null

  return {
    id: payload.id ?? payload.Id ?? crypto.randomUUID(),
    type: payload.type ?? payload.Type ?? 'notification',
    title: payload.title ?? payload.Title ?? 'Notification',
    message: payload.message ?? payload.Message ?? '',
    severity: payload.severity ?? payload.Severity ?? 'info',
    eventRequestId: payload.eventRequestId ?? payload.EventRequestId ?? null,
    academicEventId: payload.academicEventId ?? payload.AcademicEventId ?? null,
    facultyId: payload.facultyId ?? payload.FacultyId ?? null,
    facultyName: payload.facultyName ?? payload.FacultyName ?? null,
    facultyDisplayName: payload.facultyDisplayName ?? payload.FacultyDisplayName ?? null,
    requestType: payload.requestType ?? payload.RequestType ?? null,
    requestStatus: payload.requestStatus ?? payload.RequestStatus ?? null,
    createdAtUtc: payload.createdAtUtc ?? payload.CreatedAtUtc ?? null,
  }
}

export function severityToToastType(severity) {
  const normalized = String(severity ?? 'info').toLowerCase()
  if (normalized === 'success') return 'success'
  if (normalized === 'warning') return 'warning'
  if (normalized === 'error') return 'error'
  return 'info'
}
