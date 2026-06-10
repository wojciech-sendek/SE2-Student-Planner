import React from 'react'
import { render, screen, fireEvent, waitFor, act } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import LoginPage from '../LoginPage'
import * as authApi from '../../api/authApi'
import { HttpError } from '../../api/httpError.js'
import ToastContainer from '../../components/ToastContainer.jsx'

function renderLoginPage() {
  return render(
    <MemoryRouter>
      <LoginPage />
      <ToastContainer />
    </MemoryRouter>,
  )
}

// Mock the API calls
vi.mock('../../api/authApi', () => ({
  login: vi.fn(),
  forgotPassword: vi.fn(),
}))

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
  })

  it('renders login form by default', () => {
    renderLoginPage()
    
    expect(screen.getByText(/Sign in to continue/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/Email address/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/Password/i)).toBeInTheDocument()
  })

  it('switches to forgot password mode when link is clicked', () => {
    renderLoginPage()

    fireEvent.click(screen.getByText(/Forgot password\?/i))

    expect(screen.getByText(/Reset your password/i)).toBeInTheDocument()
    expect(screen.getByText(/Send reset token/i)).toBeInTheDocument()
  })

  it('validates email in forgot password mode', async () => {
    renderLoginPage()

    fireEvent.click(screen.getByText(/Forgot password\?/i))
    fireEvent.click(screen.getByRole('button', { name: /Send reset token/i }))

    expect(await screen.findByText(/Email is required\./i)).toBeInTheDocument()
  })

  it('calls forgotPassword API with correct email', async () => {
    authApi.forgotPassword.mockResolvedValueOnce({})

    renderLoginPage()

    fireEvent.click(screen.getByText(/Forgot password\?/i))
    
    const emailInput = screen.getByLabelText(/Email address/i)
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } })
    
    fireEvent.click(screen.getByRole('button', { name: /Send reset token/i }))

    await waitFor(() => {
      expect(authApi.forgotPassword).toHaveBeenCalledWith('test@example.com')
    })

    expect(screen.getByText(/Check your email/i)).toBeInTheDocument()
    expect(screen.getByText(/test@example.com/i)).toBeInTheDocument()
  })

  it('shows a top-right toast when login credentials are invalid', async () => {
    authApi.login.mockRejectedValueOnce(new HttpError(401, {}))

    renderLoginPage()

    fireEvent.change(screen.getByLabelText(/Email address/i), {
      target: { value: 'test@example.com' },
    })
    fireEvent.change(screen.getByLabelText(/Password/i), {
      target: { value: 'wrongpassword' },
    })
    await act(async () => {
      fireEvent.click(screen.getByRole('button', { name: /Sign in/i }))
      await new Promise((resolve) => setTimeout(resolve, 0))
    })

    const toast = await screen.findByRole('status')
    expect(toast).toHaveTextContent('Sign-in failed')
    expect(toast).toHaveTextContent('Invalid email or password.')
  })

  it('shows error if forgotPassword API fails', async () => {
    const errorBody = { errors: { '': ['API Error'] } }
    authApi.forgotPassword.mockRejectedValueOnce(new HttpError(400, errorBody))

    renderLoginPage()

    fireEvent.click(screen.getByText(/Forgot password\?/i))
    fireEvent.change(screen.getByLabelText(/Email address/i), { target: { value: 'test@example.com' } })
    fireEvent.click(screen.getByRole('button', { name: /Send reset token/i }))

    expect(await screen.findByText(/API Error/i)).toBeInTheDocument()
  })
})
