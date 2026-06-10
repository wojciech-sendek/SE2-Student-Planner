import '@testing-library/jest-dom'
import { cleanup } from '@testing-library/react'
import { afterEach } from 'vitest'
import { clearAllToasts } from '../lib/toastStore.js'

afterEach(() => {
  clearAllToasts()
  cleanup()
})
