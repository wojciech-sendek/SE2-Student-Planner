import { useRef, useState } from 'react'
import { TIME_OPTIONS } from '../lib/dateTimeFormUtils.js'

export default function DateTimeRangeFields({
  startDate,
  startTime,
  endDate,
  endTime,
  onFieldChange,
}) {
  const [openTimeMenu, setOpenTimeMenu] = useState(null)
  const startTimeInputRef = useRef(null)
  const endTimeInputRef = useRef(null)

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
    onFieldChange(e.target.name, e.target.value)
    setOpenTimeMenu(menuKey)
    openNativeTimePicker(nextPickerRef.current)
  }

  function openTimePicker(ref, menuKey) {
    setOpenTimeMenu(prev => (prev === menuKey ? null : menuKey))
    openNativeTimePicker(ref.current)
  }

  function setTimeValue(name, value) {
    onFieldChange(name, value)
    setOpenTimeMenu(null)
  }

  return (
    <div className="grid grid-cols-2 gap-3">
      <div>
        <label className="mb-1 block text-sm font-medium text-slate-700">
          Start <span className="text-red-500">*</span>
        </label>
        <div className="space-y-2">
          <input
            type="date"
            name="startDate"
            value={startDate}
            onChange={e => handleDateChange(e, startTimeInputRef, 'start')}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
          />
          <div className="relative">
            <input
              ref={startTimeInputRef}
              type="time"
              name="startTime"
              value={startTime}
              onChange={e => onFieldChange(e.target.name, e.target.value)}
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
            value={endDate}
            onChange={e => handleDateChange(e, endTimeInputRef, 'end')}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
          />
          <div className="relative">
            <input
              ref={endTimeInputRef}
              type="time"
              name="endTime"
              value={endTime}
              onChange={e => onFieldChange(e.target.name, e.target.value)}
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
  )
}
