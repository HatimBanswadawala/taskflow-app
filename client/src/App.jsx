import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from '@/context/AuthContext'
import ProtectedRoute from '@/components/ProtectedRoute'
import Login from '@/pages/Login'
import Register from '@/pages/Register'
import Dashboard from '@/pages/Dashboard'
import BoardDetail from '@/pages/BoardDetail'

export default function App() {
  return (
    // AuthProvider wraps everything — all pages can use useAuth()
    <AuthProvider>
      {/* BrowserRouter enables client-side routing */}
      <BrowserRouter>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />

          {/* Protected route — wrapped in ProtectedRoute */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            }
          />

          <Route
            path="/boards/:id"
            element={
              <ProtectedRoute>
                <BoardDetail />
              </ProtectedRoute>
            }
          />

          {/* Default — redirect root to dashboard (which redirects to login if not authed) */}
          <Route path="/" element={<Navigate to="/dashboard" replace />} />

          {/* Catch-all — any other URL goes to dashboard */}
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
