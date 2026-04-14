import { useId, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { register } from '../api/authApi.js'
import { HttpError, extractErrorMessages } from '../api/httpError.js'

function isValidEmail(value) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim())
}

function isUniversityEmail(value) {
  return value.trim().toLowerCase().endsWith('@pw.edu.pl')
}

function meetsPasswordPolicy(p) {
  if (p.length < 8) return false
  if (!/[0-9]/.test(p)) return false
  if (!/[A-Z]/.test(p)) return false
  if (!/[a-z]/.test(p)) return false
  return true
}

const inputClass =
  'w-full rounded-lg border border-slate-200 bg-white px-3 py-2.5 text-slate-900 shadow-sm outline-none ring-indigo-500/0 transition focus:border-indigo-400 focus:ring-2 focus:ring-indigo-500/20'

export default function RegisterPage() {
  const navigate = useNavigate()
  const id = useId()
  const firstNameId = `${id}-first`
  const lastNameId = `${id}-last`
  const emailId = `${id}-email`
  const passwordId = `${id}-password`
  const confirmId = `${id}-confirm`
  const termsId = `${id}-terms`

  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [acceptTerms, setAcceptTerms] = useState(false)
  const [showPassword, setShowPassword] = useState(false)

  const [errors, setErrors] = useState({})
  const [apiError, setApiError] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  function validate() {
    const next = {}
    if (!firstName.trim()) next.firstName = 'First name is required.'

    if (!lastName.trim()) next.lastName = 'Last name is required.'

    if (!email.trim()) next.email = 'Email is required.'
    else if (!isValidEmail(email)) next.email = 'Enter a valid email address.'
    else if (!isUniversityEmail(email)) {
      next.email = 'Registration requires a @pw.edu.pl university email.'
    }

    if (!password) next.password = 'Password is required.'
    else if (!meetsPasswordPolicy(password)) {
      next.password =
        'Use at least 8 characters with upper, lower, and a number (matches server rules).'
    }

    if (!confirmPassword) next.confirmPassword = 'Confirm your password.'
    else if (confirmPassword !== password) next.confirmPassword = 'Passwords do not match.'

    if (!acceptTerms) next.terms = 'You must accept the terms to continue.'

    setErrors(next)
    return Object.keys(next).length === 0
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setApiError(null)
    if (!validate()) return

    setIsSubmitting(true)
    try {
      await register({
        email: email.trim(),
        password,
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        facultyId: null,
      })
      navigate('/login', { replace: true, state: { registered: true } })
    } catch (err) {
      if (err instanceof HttpError) {
        const msgs = extractErrorMessages(err.body)
        setApiError(msgs.length ? msgs.join(' ') : err.message || 'Registration failed.')
      } else {
        setApiError(
          'Could not reach the API. Run the backend on port 5289 or set VITE_API_BASE_URL.',
        )
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-100 via-slate-50 to-indigo-100 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <header className="text-center mb-8">
          <h1 className="text-3xl font-semibold tracking-tight text-slate-900">
            Student Planner
          </h1>
          <p className="mt-2 text-sm text-slate-600">Create your account</p>
        </header>

        <div className="rounded-2xl border border-slate-200/80 bg-white/90 shadow-lg shadow-slate-200/60 backdrop-blur-sm px-6 py-8 sm:px-8">
          <form onSubmit={handleSubmit} className="space-y-5" noValidate>
            {apiError && (
              <div
                className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-900"
                role="alert"
              >
                {apiError}
              </div>
            )}

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label
                  htmlFor={firstNameId}
                  className="block text-sm font-medium text-slate-700 mb-1"
                >
                  First name
                </label>
                <input
                  id={firstNameId}
                  name="firstName"
                  type="text"
                  autoComplete="given-name"
                  value={firstName}
                  onChange={(e) => {
                    setFirstName(e.target.value)
                    if (errors.firstName)
                      setErrors((o) => ({ ...o, firstName: undefined }))
                  }}
                  className={inputClass}
                  placeholder="Alex"
                  aria-invalid={Boolean(errors.firstName)}
                  aria-describedby={
                    errors.firstName ? `${firstNameId}-err` : undefined
                  }
                />
                {errors.firstName && (
                  <p id={`${firstNameId}-err`} className="mt-1 text-sm text-red-600">
                    {errors.firstName}
                  </p>
                )}
              </div>
              <div>
                <label
                  htmlFor={lastNameId}
                  className="block text-sm font-medium text-slate-700 mb-1"
                >
                  Last name
                </label>
                <input
                  id={lastNameId}
                  name="lastName"
                  type="text"
                  autoComplete="family-name"
                  value={lastName}
                  onChange={(e) => {
                    setLastName(e.target.value)
                    if (errors.lastName)
                      setErrors((o) => ({ ...o, lastName: undefined }))
                  }}
                  className={inputClass}
                  placeholder="Kowalski"
                  aria-invalid={Boolean(errors.lastName)}
                  aria-describedby={
                    errors.lastName ? `${lastNameId}-err` : undefined
                  }
                />
                {errors.lastName && (
                  <p id={`${lastNameId}-err`} className="mt-1 text-sm text-red-600">
                    {errors.lastName}
                  </p>
                )}
              </div>
            </div>

            <div>
              <label
                htmlFor={emailId}
                className="block text-sm font-medium text-slate-700 mb-1"
              >
                University email
              </label>
              <input
                id={emailId}
                name="email"
                type="email"
                autoComplete="email"
                value={email}
                onChange={(e) => {
                  setEmail(e.target.value)
                  if (errors.email) setErrors((o) => ({ ...o, email: undefined }))
                }}
                className={inputClass}
                placeholder="you@pw.edu.pl"
                aria-invalid={Boolean(errors.email)}
                aria-describedby={errors.email ? `${emailId}-err` : undefined}
              />
              {errors.email && (
                <p id={`${emailId}-err`} className="mt-1 text-sm text-red-600">
                  {errors.email}
                </p>
              )}
              <p className="mt-1 text-xs text-slate-500">
                The API only accepts addresses ending in @pw.edu.pl.
              </p>
            </div>

            <div>
              <div className="flex items-center justify-between gap-2 mb-1">
                <label
                  htmlFor={passwordId}
                  className="text-sm font-medium text-slate-700"
                >
                  Password
                </label>
                <button
                  type="button"
                  onClick={() => setShowPassword((v) => !v)}
                  className="text-xs font-medium text-slate-600 hover:text-slate-800 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500/40 rounded-md px-2 py-1"
                >
                  {showPassword ? 'Hide passwords' : 'Show passwords'}
                </button>
              </div>
              <input
                id={passwordId}
                name="password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="new-password"
                value={password}
                onChange={(e) => {
                  setPassword(e.target.value)
                  if (errors.password)
                    setErrors((o) => ({ ...o, password: undefined }))
                }}
                className={inputClass}
                placeholder="••••••••"
                aria-invalid={Boolean(errors.password)}
                aria-describedby={
                  errors.password ? `${passwordId}-err` : undefined
                }
              />
              {errors.password && (
                <p id={`${passwordId}-err`} className="mt-1 text-sm text-red-600">
                  {errors.password}
                </p>
              )}
            </div>

            <div>
              <label
                htmlFor={confirmId}
                className="block text-sm font-medium text-slate-700 mb-1"
              >
                Confirm password
              </label>
              <input
                id={confirmId}
                name="confirmPassword"
                type={showPassword ? 'text' : 'password'}
                autoComplete="new-password"
                value={confirmPassword}
                onChange={(e) => {
                  setConfirmPassword(e.target.value)
                  if (errors.confirmPassword)
                    setErrors((o) => ({ ...o, confirmPassword: undefined }))
                }}
                className={inputClass}
                placeholder="••••••••"
                aria-invalid={Boolean(errors.confirmPassword)}
                aria-describedby={
                  errors.confirmPassword ? `${confirmId}-err` : undefined
                }
              />
              {errors.confirmPassword && (
                <p id={`${confirmId}-err`} className="mt-1 text-sm text-red-600">
                  {errors.confirmPassword}
                </p>
              )}
            </div>

            <div>
              <div className="flex items-start gap-2">
                <input
                  id={termsId}
                  name="terms"
                  type="checkbox"
                  checked={acceptTerms}
                  onChange={(e) => {
                    setAcceptTerms(e.target.checked)
                    if (errors.terms) setErrors((o) => ({ ...o, terms: undefined }))
                  }}
                  className="mt-0.5 size-4 shrink-0 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500/30"
                  aria-invalid={Boolean(errors.terms)}
                  aria-describedby={errors.terms ? `${termsId}-err` : undefined}
                />
                <label htmlFor={termsId} className="text-sm text-slate-700">
                  I agree to the Terms of Service and Privacy Policy.
                </label>
              </div>
              {errors.terms && (
                <p id={`${termsId}-err`} className="mt-1 text-sm text-red-600">
                  {errors.terms}
                </p>
              )}
            </div>

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSubmitting ? 'Creating account…' : 'Create account'}
            </button>

            <p className="text-center text-sm text-slate-600">
              Already have an account?{' '}
              <Link
                to="/login"
                className="font-medium text-indigo-600 hover:text-indigo-500 focus:outline-none focus-visible:underline"
              >
                Sign in
              </Link>
            </p>
          </form>
        </div>
      </div>
    </div>
  )
}
