import { useId, useState } from 'react'
import { Link } from 'react-router-dom'

function isValidEmail(value) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim())
}

const inputClass =
  'w-full rounded-lg border border-slate-200 bg-white px-3 py-2.5 text-slate-900 shadow-sm outline-none ring-indigo-500/0 transition focus:border-indigo-400 focus:ring-2 focus:ring-indigo-500/20'

export default function RegisterPage() {
  const id = useId()
  const nameId = `${id}-name`
  const emailId = `${id}-email`
  const passwordId = `${id}-password`
  const confirmId = `${id}-confirm`
  const termsId = `${id}-terms`

  const [displayName, setDisplayName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [acceptTerms, setAcceptTerms] = useState(false)
  const [showPassword, setShowPassword] = useState(false)

  const [errors, setErrors] = useState({})
  const [registerSuccess, setRegisterSuccess] = useState(false)

  function validate() {
    const next = {}
    const nameTrim = displayName.trim()
    if (!nameTrim) next.displayName = 'Name is required.'
    else if (nameTrim.length < 2) next.displayName = 'Use at least 2 characters.'

    if (!email.trim()) next.email = 'Email is required.'
    else if (!isValidEmail(email)) next.email = 'Enter a valid email address.'

    if (!password) next.password = 'Password is required.'
    else if (password.length < 8) next.password = 'Use at least 8 characters.'

    if (!confirmPassword) next.confirmPassword = 'Confirm your password.'
    else if (confirmPassword !== password) next.confirmPassword = 'Passwords do not match.'

    if (!acceptTerms) next.terms = 'You must accept the terms to continue.'

    setErrors(next)
    return Object.keys(next).length === 0
  }

  function handleSubmit(e) {
    e.preventDefault()
    setRegisterSuccess(false)
    if (!validate()) return

    setPassword('')
    setConfirmPassword('')
    setRegisterSuccess(true)
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
            {registerSuccess && (
              <div
                className="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-900"
                role="status"
              >
                Registration details look valid. Connect registration API.
              </div>
            )}

            <div>
              <label
                htmlFor={nameId}
                className="block text-sm font-medium text-slate-700 mb-1"
              >
                Full name
              </label>
              <input
                id={nameId}
                name="displayName"
                type="text"
                autoComplete="name"
                value={displayName}
                onChange={(e) => {
                  setDisplayName(e.target.value)
                  if (errors.displayName)
                    setErrors((o) => ({ ...o, displayName: undefined }))
                }}
                className={inputClass}
                placeholder="Alex Student"
                aria-invalid={Boolean(errors.displayName)}
                aria-describedby={
                  errors.displayName ? `${nameId}-err` : undefined
                }
              />
              {errors.displayName && (
                <p id={`${nameId}-err`} className="mt-1 text-sm text-red-600">
                  {errors.displayName}
                </p>
              )}
            </div>

            <div>
              <label
                htmlFor={emailId}
                className="block text-sm font-medium text-slate-700 mb-1"
              >
                Email
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
                placeholder="you@university.edu"
                aria-invalid={Boolean(errors.email)}
                aria-describedby={errors.email ? `${emailId}-err` : undefined}
              />
              {errors.email && (
                <p id={`${emailId}-err`} className="mt-1 text-sm text-red-600">
                  {errors.email}
                </p>
              )}
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
                  I agree to the{' '}
                  <span className="text-slate-900 font-medium">
                    Terms of Service
                  </span>{' '}
                  and{' '}
                  <span className="text-slate-900 font-medium">
                    Privacy Policy
                  </span>
                  . (Frontend placeholder  link real documents later.)
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
              className="w-full rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 focus-visible:ring-offset-2"
            >
              Create account
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
