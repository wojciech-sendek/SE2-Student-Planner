import React from 'react'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import ResetPasswordPage from '../ResetPasswordPage'
import * as authApi from '../../api/authApi'

// Mock the API calls
vi.mock('../../api/authApi', () => ({
  resetPassword: vi.fn(),
}))

describe('ResetPasswordPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders reset password form with email from query param', () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPasswordPage />} />
        </Routes>
      </MemoryRouter>
    )
    
    expect(screen.getByLabelText(/Email address/i)).toHaveValue('test@example.com')
    expect(screen.getByLabelText(/6-digit token/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^New password$/i)).toBeInTheDocument()
  })

  it('validates 6-digit token format', async () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPasswordPage />} />
        </Routes>
      </MemoryRouter>
    )

    fireEvent.change(screen.getByLabelText(/6-digit token/i), { target: { value: '123' } })
    fireEvent.change(screen.getByLabelText(/^New password$/i), { target: { value: 'password123' } })
    fireEvent.change(screen.getByLabelText(/Confirm new password/i), { target: { value: 'password123' } })
    
    fireEvent.click(screen.getByRole('button', { name: /Reset password/i }))

    expect(await screen.findByText(/Token must be 6 digits\./i)).toBeInTheDocument()
  })

  it('calls resetPassword API with correct data', async () => {
    authApi.resetPassword.mockResolvedValueOnce({})

    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPasswordPage />} />
        </Routes>
      </MemoryRouter>
    )

    fireEvent.change(screen.getByLabelText(/6-digit token/i), { target: { value: '123456' } })
    fireEvent.change(screen.getByLabelText(/^New password$/i), { target: { value: 'newpassword123' } })
    fireEvent.change(screen.getByLabelText(/Confirm new password/i), { target: { value: 'newpassword123' } })
    
    fireEvent.click(screen.getByRole('button', { name: /Reset password/i }))

    await waitFor(() => {
      expect(authApi.resetPassword).toHaveBeenCalledWith({
        email: 'test@example.com',
        token: '123456',
        newPassword: 'newpassword123'
      })
    })

    expect(screen.getByText(/Password reset successful/i)).toBeInTheDocument()
  })

  it('validates matching passwords', async () => {
    render(
      <MemoryRouter initialEntries={['/reset-password?email=test@example.com']}>
        <Routes>
          <Route path="/reset-password" element={<ResetPasswordPage />} />
        </Routes>
      </MemoryRouter>
    )

    fireEvent.change(screen.getByLabelText(/6-digit token/i), { target: { value: '123456' } })
    fireEvent.change(screen.getByLabelText(/^New password$/i), { target: { value: 'pass1' } })
    fireEvent.change(screen.getByLabelText(/Confirm new password/i), { target: { value: 'pass2' } })
    
    fireEvent.click(screen.getByRole('button', { name: /Reset password/i }))

    expect(await screen.findByText(/Passwords do not match\./i)).toBeInTheDocument()
  })
})
