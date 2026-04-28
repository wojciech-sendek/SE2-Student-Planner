import React, { useId, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { resetPassword } from '../api/authApi.js'
import { HttpError, extractErrorMessages } from '../api/httpError.js'

export default function ResetPasswordPage() {
  const location = useLocation()
  const queryParams = new URLSearchParams(location.search)
  const initialEmail = queryParams.get('email') || ''

  const formId = useId()
  const emailId = `${formId}-email`
  const tokenId = `${formId}-token`
  const passwordId = `${formId}-password`
  const confirmPasswordId = `${formId}-confirm-password`

  const [email, setEmail] = useState(initialEmail)
  const [token, setToken] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)

  const [errors, setErrors] = useState({})
  const [apiError, setApiError] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSuccess, setIsSuccess] = useState(false)

  function validate() {
    const next = {}
    if (!email.trim()) next.email = 'Email is required.'
    if (!token.trim()) next.token = 'Reset token is required.'
    else if (token.trim().length !== 6) next.token = 'Token must be 6 digits.'
    if (!newPassword) next.newPassword = 'New password is required.'
    else if (newPassword.length < 8) next.newPassword = 'Use at least 8 characters.'
    
    if (newPassword !== confirmPassword) {
      next.confirmPassword = 'Passwords do not match.'
    }
    
    setErrors(next)
    return Object.keys(next).length === 0
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setApiError(null)
    if (!validate()) return

    setIsSubmitting(true)
    try {
      await resetPassword({
        email: email.trim(),
        token: token.trim(),
        newPassword
      })
      setIsSuccess(true)
    } catch (err) {
      if (err instanceof HttpError) {
        const msgs = extractErrorMessages(err.body)
        setApiError(msgs.join(' ') || 'Password reset failed.')
      } else {
        setApiError('Could not reach the API.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isSuccess) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-blue-50 to-purple-50 flex items-center justify-center p-4">
        <div className="w-full max-w-md text-center">
          <div className="rounded-2xl border border-slate-200/80 bg-white/95 shadow-xl p-8 backdrop-blur-sm">
            <div className="inline-flex items-center justify-center w-16 h-16 mb-6 rounded-full bg-emerald-100 text-emerald-600">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-slate-900 mb-2">Password reset successful</h2>
            <p className="text-slate-600 mb-8">
              Your password has been updated. You can now sign in with your new password.
            </p>
            <Link
              to="/login"
              className="inline-block w-full rounded-lg bg-indigo-600 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-indigo-500/30 transition-all hover:bg-indigo-500"
            >
              Go to sign in
            </Link>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-blue-50 to-purple-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <header className="text-center mb-8">
          <h1 className="text-4xl font-bold tracking-tight text-transparent bg-clip-text bg-gradient-to-r from-indigo-600 to-purple-600">
            Reset Password
          </h1>
          <p className="mt-3 text-sm text-slate-600 font-medium">
            Enter the token from your email to set a new password
          </p>
        </header>

        <div className="rounded-2xl border border-slate-200/80 bg-white/95 shadow-xl px-6 py-8 sm:px-8 backdrop-blur-sm">
          <form onSubmit={handleSubmit} className="space-y-5" noValidate>
            {apiError && (
              <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-900 shadow-sm" role="alert">
                {apiError}
              </div>
            )}

            <div>
              <label htmlFor={emailId} className="block text-sm font-semibold text-slate-700 mb-2">
                Email address
              </label>
              <input
                id={emailId}
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-slate-900 shadow-sm outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-500/10 transition-all"
                placeholder="you@university.edu"
              />
              {errors.email && <p className="mt-2 text-sm text-red-600">{errors.email}</p>}
            </div>

            <div>
              <label htmlFor={tokenId} className="block text-sm font-semibold text-slate-700 mb-2">
                6-digit Token
              </label>
              <input
                id={tokenId}
                type="text"
                value={token}
                onChange={(e) => setToken(e.target.value)}
                className="w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-slate-900 shadow-sm outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-500/10 transition-all"
                placeholder="Paste token from email"
              />
              {errors.token && <p className="mt-2 text-sm text-red-600">{errors.token}</p>}
            </div>

            <div>
              <label htmlFor={passwordId} className="block text-sm font-semibold text-slate-700 mb-2">
                New password
              </label>
              <div className="relative">
                <input
                  id={passwordId}
                  type={showPassword ? 'text' : 'password'}
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  className="w-full rounded-lg border border-slate-300 bg-white px-4 py-3 pr-20 text-slate-900 shadow-sm outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-500/10 transition-all"
                  placeholder="••••••••"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-2 top-1/2 -translate-y-1/2 rounded-md px-3 py-1.5 text-xs font-semibold text-slate-600 hover:bg-slate-100"
                >
                  {showPassword ? 'Hide' : 'Show'}
                </button>
              </div>
              {errors.newPassword && <p className="mt-2 text-sm text-red-600">{errors.newPassword}</p>}
            </div>

            <div>
              <label htmlFor={confirmPasswordId} className="block text-sm font-semibold text-slate-700 mb-2">
                Confirm new password
              </label>
              <input
                id={confirmPasswordId}
                type={showPassword ? 'text' : 'password'}
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-slate-900 shadow-sm outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-500/10 transition-all"
                placeholder="••••••••"
              />
              {errors.confirmPassword && <p className="mt-2 text-sm text-red-600">{errors.confirmPassword}</p>}
            </div>

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full rounded-lg bg-gradient-to-r from-indigo-600 to-purple-600 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-indigo-500/30 transition-all hover:shadow-xl hover:from-indigo-500 hover:to-purple-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-60 disabled:cursor-not-allowed"
            >
              {isSubmitting ? 'Resetting…' : 'Reset password'}
            </button>

            <p className="text-center text-sm text-slate-600">
              <Link to="/login" className="font-medium text-indigo-600 hover:text-indigo-500">
                Back to sign in
              </Link>
            </p>
          </form>
        </div>
      </div>
    </div>
  )
}
