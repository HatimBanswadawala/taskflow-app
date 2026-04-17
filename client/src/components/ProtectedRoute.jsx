import { Navigate } from 'react-router-dom'
import { useAuth } from '@/context/AuthContext'

/**
 * Wraps pages that require authentication.
 * If user is not logged in, redirects to /login.
 *
 * Usage:
 *   <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
 */
export default function ProtectedRoute({ children }) {
  const { isAuthenticated, loading } = useAuth()

  // Don't redirect while we're still checking localStorage
  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-slate-500">Loading...</div>
      </div>
    )
  }

  // Not logged in? Send them to login
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  // Logged in — render the actual page
  return children
}
