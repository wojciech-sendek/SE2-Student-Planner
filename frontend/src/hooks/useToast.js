import { useCallback, useEffect, useState } from 'react'
import {
  addToast,
  dismissToast,
  showError,
  showInfo,
  showSuccess,
  showWarning,
  subscribeToasts,
} from '../lib/toastStore.js'

export function useToast() {
  const [toastList, setToastList] = useState([])

  useEffect(() => subscribeToasts(setToastList), [])

  return {
    toasts: toastList,
    addToast,
    dismissToast,
    showSuccess: useCallback(
      (title, message, options) => showSuccess(title, message, options),
      [],
    ),
    showInfo: useCallback(
      (title, message, options) => showInfo(title, message, options),
      [],
    ),
    showWarning: useCallback(
      (title, message, options) => showWarning(title, message, options),
      [],
    ),
    showError: useCallback(
      (title, message, options) => showError(title, message, options),
      [],
    ),
  }
}
