import React from 'react'
import { render, screen, act } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import ToastContainer from '../ToastContainer.jsx'
import { showSuccess } from '../../lib/toastStore.js'

function ToastTrigger() {
  return (
    <button type="button" onClick={() => showSuccess('Signed in', 'Welcome back')}>
      Trigger toast
    </button>
  )
}

describe('ToastContainer', () => {
  it('renders a toast when showSuccess is called', async () => {
    render(
      <>
        <ToastTrigger />
        <ToastContainer />
      </>,
    )

    await act(async () => {
      screen.getByRole('button', { name: 'Trigger toast' }).click()
    })

    expect(screen.getByText('Signed in')).toBeInTheDocument()
    expect(screen.getByText('Welcome back')).toBeInTheDocument()
  })
})
