import { useId, useState } from 'react'

const REMEMBER_EMAIL_KEY = 'student-planner-remember-email'

function isValidEmail(value) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim())
}

export default function LoginPage() {
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
  const [loginSuccess, setLoginSuccess] = useState(false)

  function validateLogin() {
    const next = {}
    if (!email.trim()) next.email = 'Email is required.'
    else if (!isValidEmail(email)) next.email = 'Enter a valid email address.'
    if (!password) next.password = 'Password is required.'
    else if (password.length < 8) next.password = 'Use at least 8 characters.'
    setErrors(next)
    return Object.keys(next).length === 0
  }

  function handleLoginSubmit(e) {
    e.preventDefault()
    setLoginSuccess(false)
    if (!validateLogin()) return

    if (rememberMe) localStorage.setItem(REMEMBER_EMAIL_KEY, email.trim())
    else localStorage.removeItem(REMEMBER_EMAIL_KEY)

    setPassword('')
    setLoginSuccess(true)
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
    setLoginSuccess(false)
  }

  function backToLogin() {
    setMode('login')
    setForgotSent(false)
    setErrors({})
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-100 via-slate-50 to-indigo-100 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <header className="text-center mb-8">
          <h1 className="text-3xl font-semibold tracking-tight text-slate-900">
            Student Planner
          </h1>
          <p className="mt-2 text-sm text-slate-600">
            {mode === 'login'
              ? 'Sign in to continue'
              : 'Reset your password'}
          </p>
        </header>

        <div className="rounded-2xl border border-slate-200/80 bg-white/90 shadow-lg shadow-slate-200/60 backdrop-blur-sm px-6 py-8 sm:px-8">
          {mode === 'login' ? (
            <form onSubmit={handleLoginSubmit} className="space-y-5" noValidate>
              {loginSuccess && (
                <div
                  className="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-900"
                  role="status"
                >
                  Credentials look valid. Connect auth API here to finish
                  sign-in.
                </div>
              )}

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
                  className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2.5 text-slate-900 shadow-sm outline-none ring-indigo-500/0 transition focus:border-indigo-400 focus:ring-2 focus:ring-indigo-500/20"
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
                    onClick={openForgot}
                    className="text-sm font-medium text-indigo-600 hover:text-indigo-500 focus:outline-none focus-visible:underline"
                  >
                    Forgot password?
                  </button>
                </div>
                <div className="relative">
                  <input
                    id={passwordId}
                    name="password"
                    type={showPassword ? 'text' : 'password'}
                    autoComplete="current-password"
                    value={password}
                    onChange={(e) => {
                      setPassword(e.target.value)
                      if (errors.password)
                        setErrors((o) => ({ ...o, password: undefined }))
                    }}
                    className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2.5 pr-24 text-slate-900 shadow-sm outline-none ring-indigo-500/0 transition focus:border-indigo-400 focus:ring-2 focus:ring-indigo-500/20"
                    placeholder="••••••••"
                    aria-invalid={Boolean(errors.password)}
                    aria-describedby={
                      errors.password ? `${passwordId}-err` : undefined
                    }
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((v) => !v)}
                    className="absolute right-2 top-1/2 -translate-y-1/2 rounded-md px-2 py-1 text-xs font-medium text-slate-600 hover:bg-slate-100 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500/40"
                  >
                    {showPassword ? 'Hide' : 'Show'}
                  </button>
                </div>
                {errors.password && (
                  <p id={`${passwordId}-err`} className="mt-1 text-sm text-red-600">
                    {errors.password}
                  </p>
                )}
              </div>

              <div className="flex items-center gap-2">
                <input
                  id={rememberId}
                  name="remember"
                  type="checkbox"
                  checked={rememberMe}
                  onChange={(e) => setRememberMe(e.target.checked)}
                  className="size-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500/30"
                />
                <label htmlFor={rememberId} className="text-sm text-slate-700">
                  Remember me on this device
                </label>
              </div>

              <button
                type="submit"
                className="w-full rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 focus-visible:ring-offset-2"
              >
                Sign in
              </button>

              <p className="text-center text-sm text-slate-600">
                No account yet?{' '}
                <span className="text-slate-500" title="Register page — next step">
                  Register (coming next)
                </span>
              </p>
            </form>
          ) : (
            <div className="space-y-5">
              <button
                type="button"
                onClick={backToLogin}
                className="text-sm font-medium text-indigo-600 hover:text-indigo-500 focus:outline-none focus-visible:underline"
              >
                 Back to sign in
              </button>

              {forgotSent ? (
                <div
                  className="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-3 text-sm text-emerald-900"
                  role="status"
                >
                  If an account exists for{' '}
                  <span className="font-medium">{forgotEmail.trim()}</span>, you
                  will receive reset instructions. This is a frontend-only preview;
                  no email is sent until the backend is connected.
                </div>
              ) : (
                <form onSubmit={handleForgotSubmit} className="space-y-5" noValidate>
                  <p className="text-sm text-slate-600">
                    Enter the email for your account. We will send reset
                    instructions when the server is ready.
                  </p>
                  <div>
                    <label
                      htmlFor={`${formId}-forgot-email`}
                      className="block text-sm font-medium text-slate-700 mb-1"
                    >
                      Email
                    </label>
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
                      className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2.5 text-slate-900 shadow-sm outline-none ring-indigo-500/0 transition focus:border-indigo-400 focus:ring-2 focus:ring-indigo-500/20"
                      placeholder="you@university.edu"
                      aria-invalid={Boolean(errors.forgotEmail)}
                      aria-describedby={
                        errors.forgotEmail ? `${formId}-forgot-err` : undefined
                      }
                    />
                    {errors.forgotEmail && (
                      <p
                        id={`${formId}-forgot-err`}
                        className="mt-1 text-sm text-red-600"
                      >
                        {errors.forgotEmail}
                      </p>
                    )}
                  </div>
                  <button
                    type="submit"
                    className="w-full rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 focus-visible:ring-offset-2"
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
