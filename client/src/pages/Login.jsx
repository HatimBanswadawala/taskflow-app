import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Mail, Lock, LogIn, Loader2 } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()

  // Single state object for form fields — simpler than multiple useState calls
  const [form, setForm] = useState({ email: 'demo@taskflow.app', password: 'Demo123!' })
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  // Controlled input change — updates state when user types
  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value })
  }

  const handleSubmit = async (e) => {
    e.preventDefault() // prevent default form POST + page reload
    setError('')
    setSubmitting(true)

    try {
      await login(form.email, form.password)
      navigate('/dashboard') // redirect after successful login
    } catch (err) {
      // Extract error message from API response
      setError(err.response?.data?.errors?.[0]?.errorMessage || 'Login failed. Try again.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center gap-2">
            <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-cyan-500 to-blue-600 flex items-center justify-center text-white font-bold">
              TF
            </div>
            <h1 className="text-2xl font-bold">TaskFlow</h1>
          </div>
          <p className="text-slate-500 dark:text-slate-400 text-sm mt-2">Welcome back. Sign in to continue.</p>
        </div>

        {/* Form Card */}
        <form onSubmit={handleSubmit} className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl p-6 space-y-4">
          <h2 className="text-xl font-semibold mb-2">Sign In</h2>

          {/* Error message */}
          {error && (
            <div className="bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 text-red-700 dark:text-red-400 text-sm rounded-lg p-3">
              {error}
            </div>
          )}

          {/* Email field */}
          <div>
            <label className="block text-sm font-medium mb-1">Email</label>
            <div className="relative">
              <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
              <input
                type="email"
                name="email"
                value={form.email}
                onChange={handleChange}
                required
                className="w-full pl-10 pr-4 py-2 rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-950 focus:outline-none focus:ring-2 focus:ring-cyan-500"
                placeholder="you@example.com"
              />
            </div>
          </div>

          {/* Password field */}
          <div>
            <label className="block text-sm font-medium mb-1">Password</label>
            <div className="relative">
              <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
              <input
                type="password"
                name="password"
                value={form.password}
                onChange={handleChange}
                required
                className="w-full pl-10 pr-4 py-2 rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-950 focus:outline-none focus:ring-2 focus:ring-cyan-500"
                placeholder="Enter your password"
              />
            </div>
          </div>

          {/* Submit button */}
          <button
            type="submit"
            disabled={submitting}
            className="w-full flex items-center justify-center gap-2 py-2.5 rounded-lg bg-gradient-to-r from-cyan-500 to-blue-600 text-white font-medium hover:opacity-90 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {submitting ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                Signing in...
              </>
            ) : (
              <>
                <LogIn className="w-4 h-4" />
                Sign In
              </>
            )}
          </button>

          {/* Register link */}
          <p className="text-center text-sm text-slate-600 dark:text-slate-400">
            Don't have an account?{' '}
            <Link to="/register" className="text-cyan-600 dark:text-cyan-400 font-medium hover:underline">
              Create one
            </Link>
          </p>
        </form>

        {/* Demo credentials hint */}
        <div className="mt-4 text-center text-xs text-slate-500">
          Demo: demo@taskflow.app / Demo123!
        </div>
      </div>
    </div>
  )
}
