import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import LoginPage from '../LoginPage'
import * as authApi from '../../api/authApi'

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
    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    )
    
    expect(screen.getByText(/Sign in to continue/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/Email address/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/Password/i)).toBeInTheDocument()
  })

  it('switches to forgot password mode when link is clicked', () => {
    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    )

    fireEvent.click(screen.getByText(/Forgot password\?/i))

    expect(screen.getByText(/Reset your password/i)).toBeInTheDocument()
    expect(screen.getByText(/Send reset token/i)).toBeInTheDocument()
  })

  it('validates email in forgot password mode', async () => {
    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    )

    fireEvent.click(screen.getByText(/Forgot password\?/i))
    fireEvent.click(screen.getByRole('button', { name: /Send reset token/i }))

    expect(await screen.findByText(/Email is required\./i)).toBeInTheDocument()
  })

  it('calls forgotPassword API with correct email', async () => {
    authApi.forgotPassword.mockResolvedValueOnce({})

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    )

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

  it('shows error if forgotPassword API fails', async () => {
    const errorBody = { errors: { '': ['API Error'] } }
    const httpError = { 
      body: errorBody,
      status: 400 
    }
    // We need to mock HttpError class or just use the structure that extractErrorMessages expects
    authApi.forgotPassword.mockRejectedValueOnce(new (class HttpError extends Error {
      constructor() {
        super('Failed')
        this.body = errorBody
        this.status = 400
      }
    })())

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    )

    fireEvent.click(screen.getByText(/Forgot password\?/i))
    fireEvent.change(screen.getByLabelText(/Email address/i), { target: { value: 'test@example.com' } })
    fireEvent.click(screen.getByRole('button', { name: /Send reset token/i }))

    expect(await screen.findByText(/API Error/i)).toBeInTheDocument()
  })
})
