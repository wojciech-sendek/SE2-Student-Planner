import { useId, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { login } from '../api/authApi.js'
import { HttpError, extractErrorMessages } from '../api/httpError.js'
import { saveAuthFromResponse } from '../lib/authStorage.js'

const REMEMBER_EMAIL_KEY = 'student-planner-remember-email'

function isValidEmail(value) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim())
}

export default function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const justRegistered = Boolean(location.state?.registered)
  const formId = useId()
  const emailId = `${formId}-email`
  const passwordId = `${formId}-password`
  const rememberId = `${formId}-remember`

  const [mode, setMode] = useState('login') // 'login' | 'forgot'

  const [email, setEmail] = useState(() => {
    if (typeof window === 'undefined') return ''
    return localStorage.getItem(REMEMBER_EMAIL_KEY) ?? ''
  })
  const [password, setPassword] = useState('')
  const [rememberMe, setRememberMe] = useState(() => {
    if (typeof window === 'undefined') return false
    return Boolean(localStorage.getItem(REMEMBER_EMAIL_KEY))
  })
  const [showPassword, setShowPassword] = useState(false)

  const [forgotEmail, setForgotEmail] = useState('')
  const [forgotSent, setForgotSent] = useState(false)

  const [errors, setErrors] = useState({})
  const [apiError, setApiError] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  function validateLogin() {
    const next = {}
    if (!email.trim()) next.email = 'Email is required.'
    else if (!isValidEmail(email)) next.email = 'Enter a valid email address.'
    if (!password) next.password = 'Password is required.'
    else if (password.length < 8) next.password = 'Use at least 8 characters.'
    setErrors(next)
    return Object.keys(next).length === 0
  }

  async function handleLoginSubmit(e) {
    e.preventDefault()
    setApiError(null)
    if (!validateLogin()) return

    setIsSubmitting(true)
    try {
      const data = await login({
        email: email.trim(),
        password,
      })
      saveAuthFromResponse(data)
      if (rememberMe) localStorage.setItem(REMEMBER_EMAIL_KEY, email.trim())
      else localStorage.removeItem(REMEMBER_EMAIL_KEY)
      setPassword('')
      navigate('/app', { replace: true })
    } catch (err) {
      if (err instanceof HttpError) {
        const msgs = extractErrorMessages(err.body)
        if (err.status === 401) {
          setApiError('Invalid email or password.')
        } else if (msgs.length) {
          setApiError(msgs.join(' '))
        } else {
          setApiError(err.message || 'Sign-in failed.')
        }
      } else {
        setApiError(
          'Could not reach the API. Run the backend on port 5289 or set VITE_API_BASE_URL.',
        )
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  function validateForgot() {
    const next = {}
    if (!forgotEmail.trim()) next.forgotEmail = 'Email is required.'
    else if (!isValidEmail(forgotEmail)) next.forgotEmail = 'Enter a valid email address.'
    setErrors(next)
    return Object.keys(next).length === 0
  }

  function handleForgotSubmit(e) {
    e.preventDefault()
    setForgotSent(false)
    if (!validateForgot()) return
    setForgotSent(true)
  }

  function openForgot() {
    setMode('forgot')
    setForgotEmail(email.trim())
    setForgotSent(false)
    setErrors({})
    setApiError(null)
  }

  function backToLogin() {
    setMode('login')
    setForgotSent(false)
    setErrors({})
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-blue-50 to-purple-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <header className="text-center mb-8 animate-fade-in">
          <div className="inline-flex items-center justify-center w-16 h-16 mb-4 rounded-2xl bg-gradient-to-br from-indigo-500 to-purple-600 shadow-lg shadow-indigo-500/30">
            <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
            </svg>
          </div>
          <h1 className="text-4xl font-bold tracking-tight text-transparent bg-clip-text bg-gradient-to-r from-indigo-600 to-purple-600">
            Student Planner
          </h1>
          <p className="mt-3 text-sm text-slate-600 font-medium">
            {mode === 'login'
              ? 'Sign in to continue'
              : 'Reset your password'}
          </p>
        </header>

        <div className="rounded-2xl border border-slate-200/80 bg-white/95 shadow-xl shadow-slate-900/10 backdrop-blur-sm px-6 py-8 sm:px-8 transition-all duration-300 hover:shadow-2xl hover:shadow-slate-900/15">
          <style>{`
            @keyframes fade-in {
              from {
                opacity: 0;
                transform: translateY(-10px);
              }
              to {
                opacity: 1;
                transform: translateY(0);
              }
            }
            .animate-fade-in {
              animation: fade-in 0.6s ease-out;
            }
          `}</style>
          {mode === 'login' ? (
            <form onSubmit={handleLoginSubmit} className="space-y-5" noValidate>
              {justRegistered && (
                <div
                  className="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-900 shadow-sm"
                  role="status"
                >
                  Account created. You can sign in now.
                </div>
              )}

              {apiError && (
                <div
                  className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-900 shadow-sm"
                  role="alert"
                >
                  {apiError}
                </div>
              )}

              <div>
                <label
                  htmlFor={emailId}
                  className="block text-sm font-semibold text-slate-700 mb-2"
                >
                  Email address
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <svg className="h-5 w-5 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 12a4 4 0 10-8 0 4 4 0 008 0zm0 0v1.5a2.5 2.5 0 005 0V12a9 9 0 10-9 9m4.5-1.206a8.959 8.959 0 01-4.5 1.207" />
                    </svg>
                  </div>
                  <input
                    id={emailId}
                    name="email"
                    type="email"
                    autoComplete="email"
                    value={email}
                    onChange={(e) => {
                      setEmail(e.target.value)
                      setApiError(null)
                      if (errors.email) setErrors((o) => ({ ...o, email: undefined }))
                    }}
                    className="w-full rounded-lg border border-slate-300 bg-white pl-10 pr-3 py-3 text-slate-900 shadow-sm outline-none ring-indigo-500/0 transition-all focus:border-indigo-500 focus:ring-4 focus:ring-indigo-500/10 hover:border-slate-400"
                    placeholder="you@university.edu"
                    aria-invalid={Boolean(errors.email)}
                    aria-describedby={errors.email ? `${emailId}-err` : undefined}
                  />
                </div>
                {errors.email && (
                  <p id={`${emailId}-err`} className="mt-2 text-sm text-red-600 flex items-center gap-1">
                    <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                    </svg>
                    {errors.email}
                  </p>
                )}
              </div>

              <div>
                <div className="flex items-center justify-between gap-2 mb-2">
                  <label
                    htmlFor={passwordId}
                    className="text-sm font-semibold text-slate-700"
                  >
                    Password
                  </label>
                  <button
                    type="button"
                    onClick={openForgot}
                    className="text-sm font-semibold text-indigo-600 hover:text-indigo-700 focus:outline-none focus-visible:underline underline-offset-2 transition-colors"
                  >
                    Forgot password?
                  </button>
                </div>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <svg className="h-5 w-5 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                    </svg>
                  </div>
                  <input
                    id={passwordId}
                    name="password"
                    type={showPassword ? 'text' : 'password'}
                    autoComplete="current-password"
                    value={password}
                    onChange={(e) => {
                      setPassword(e.target.value)
                      setApiError(null)
                      if (errors.password)
                        setErrors((o) => ({ ...o, password: undefined }))
                    }}
                    className="w-full rounded-lg border border-slate-300 bg-white pl-10 pr-20 py-3 text-slate-900 shadow-sm outline-none ring-indigo-500/0 transition-all focus:border-indigo-500 focus:ring-4 focus:ring-indigo-500/10 hover:border-slate-400"
                    placeholder="••••••••"
                    aria-invalid={Boolean(errors.password)}
                    aria-describedby={
                      errors.password ? `${passwordId}-err` : undefined
                    }
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((v) => !v)}
                    className="absolute right-2 top-1/2 -translate-y-1/2 rounded-md px-3 py-1.5 text-xs font-semibold text-slate-600 hover:bg-slate-100 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500/40 transition-colors"
                  >
                    {showPassword ? 'Hide' : 'Show'}
                  </button>
                </div>
                {errors.password && (
                  <p id={`${passwordId}-err`} className="mt-2 text-sm text-red-600 flex items-center gap-1">
                    <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                    </svg>
                    {errors.password}
                  </p>
                )}
              </div>

              <div className="flex items-center gap-2.5">
                <input
                  id={rememberId}
                  name="remember"
                  type="checkbox"
                  checked={rememberMe}
                  onChange={(e) => setRememberMe(e.target.checked)}
                  className="size-4 rounded border-slate-300 text-indigo-600 focus:ring-2 focus:ring-indigo-500/30 transition-all cursor-pointer"
                />
                <label htmlFor={rememberId} className="text-sm text-slate-700 font-medium cursor-pointer select-none">
                  Remember me on this device
                </label>
              </div>

              <button
                type="submit"
                disabled={isSubmitting}
                className="w-full rounded-lg bg-gradient-to-r from-indigo-600 to-purple-600 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-indigo-500/30 transition-all hover:shadow-xl hover:shadow-indigo-500/40 hover:from-indigo-500 hover:to-purple-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 focus-visible:ring-offset-2 active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isSubmitting ? 'Signing in…' : 'Sign in'}
              </button>

              <p className="text-center text-sm text-slate-600">
                No account yet?{' '}
                <Link
                  to="/register"
                  className="font-medium text-indigo-600 hover:text-indigo-500 focus:outline-none focus-visible:underline"
                >
                  Register
                </Link>
              </p>
            </form>
          ) : (
            <div className="space-y-5">
              <button
                type="button"
                onClick={backToLogin}
                className="flex items-center gap-1 text-sm font-semibold text-indigo-600 hover:text-indigo-700 focus:outline-none focus-visible:underline underline-offset-2 transition-colors"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
                Back to sign in
              </button>

              {forgotSent ? (
                <div
                  className="rounded-lg border border-emerald-300/60 bg-gradient-to-r from-emerald-50 to-green-50 px-4 py-3.5 text-sm text-emerald-900 shadow-sm"
                  role="status"
                >
                  <div className="flex gap-3">
                    <svg className="w-5 h-5 text-emerald-600 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                    </svg>
                    <div>
                      <p className="font-semibold mb-1">Check your email</p>
                      <p>
                        If an account exists for{' '}
                        <span className="font-semibold">{forgotEmail.trim()}</span>, you
                        will receive reset instructions. This is a frontend-only preview;
                        no email is sent until the backend is connected.
                      </p>
                    </div>
                  </div>
                </div>
              ) : (
                <form onSubmit={handleForgotSubmit} className="space-y-5" noValidate>
                  <p className="text-sm text-slate-600 leading-relaxed">
                    Enter the email for your account. We will send reset
                    instructions when the server is ready.
                  </p>
                  <div>
                    <label
                      htmlFor={`${formId}-forgot-email`}
                      className="block text-sm font-semibold text-slate-700 mb-2"
                    >
                      Email address
                    </label>
                    <div className="relative">
                      <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                        <svg className="h-5 w-5 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 12a4 4 0 10-8 0 4 4 0 008 0zm0 0v1.5a2.5 2.5 0 005 0V12a9 9 0 10-9 9m4.5-1.206a8.959 8.959 0 01-4.5 1.207" />
                        </svg>
                      </div>
                      <input
                        id={`${formId}-forgot-email`}
                        name="forgot-email"
                        type="email"
                        autoComplete="email"
                        value={forgotEmail}
                        onChange={(e) => {
                          setForgotEmail(e.target.value)
                          if (errors.forgotEmail)
                            setErrors((o) => ({ ...o, forgotEmail: undefined }))
                        }}
                        className="w-full rounded-lg border border-slate-300 bg-white pl-10 pr-3 py-3 text-slate-900 shadow-sm outline-none ring-indigo-500/0 transition-all focus:border-indigo-500 focus:ring-4 focus:ring-indigo-500/10 hover:border-slate-400"
                        placeholder="you@university.edu"
                        aria-invalid={Boolean(errors.forgotEmail)}
                        aria-describedby={
                          errors.forgotEmail ? `${formId}-forgot-err` : undefined
                        }
                      />
                    </div>
                    {errors.forgotEmail && (
                      <p
                        id={`${formId}-forgot-err`}
                        className="mt-2 text-sm text-red-600 flex items-center gap-1"
                      >
                        <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                          <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                        </svg>
                        {errors.forgotEmail}
                      </p>
                    )}
                  </div>
                  <button
                    type="submit"
                    className="w-full rounded-lg bg-gradient-to-r from-indigo-600 to-purple-600 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-indigo-500/30 transition-all hover:shadow-xl hover:shadow-indigo-500/40 hover:from-indigo-500 hover:to-purple-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 focus-visible:ring-offset-2 active:scale-[0.98]"
                  >
                    Send reset instructions
                  </button>
                </form>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
