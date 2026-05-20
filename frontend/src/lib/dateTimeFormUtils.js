export function toDatetimeLocal(isoStr) {
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

export function splitDatetimeLocal(datetimeLocal) {
  if (!datetimeLocal) return { date: '', time: '' }
  const [datePart = '', timePart = ''] = datetimeLocal.split('T')
  return { date: datePart, time: timePart.slice(0, 5) }
}

export function combineDateTime(date, time) {
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

export const TIME_OPTIONS = buildTimeOptions()
