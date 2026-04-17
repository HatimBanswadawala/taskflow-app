import { createContext, useContext, useState, useEffect } from 'react'
import apiClient from '@/services/apiClient'

// 1. Create a Context — like a global "channel" for auth data
const AuthContext = createContext(null)

// 2. Provider component — wraps the app and supplies auth data to any child
export function AuthProvider({ children }) {
  // user = current logged-in user (null if not logged in)
  const [user, setUser] = useState(null)
  // loading = true while we check localStorage on first render
  const [loading, setLoading] = useState(true)

  // On app load, check if there's a saved user in localStorage
  useEffect(() => {
    const savedUser = localStorage.getItem('taskflow-user')
    if (savedUser) {
      try {
        setUser(JSON.parse(savedUser))
      } catch {
        localStorage.removeItem('taskflow-user')
      }
    }
    setLoading(false)
  }, [])

  // Login function — called from Login page
  const login = async (email, password) => {
    const response = await apiClient.post('/auth/login', { email, password })
    const { token, userId, fullName, email: userEmail } = response.data

    const userData = { userId, fullName, email: userEmail }
    localStorage.setItem('taskflow-token', token)
    localStorage.setItem('taskflow-user', JSON.stringify(userData))
    setUser(userData)

    return userData
  }

  // Register function — called from Register page
  const register = async (fullName, email, password) => {
    const response = await apiClient.post('/auth/register', { fullName, email, password })
    const { token, userId, fullName: name, email: userEmail } = response.data

    const userData = { userId, fullName: name, email: userEmail }
    localStorage.setItem('taskflow-token', token)
    localStorage.setItem('taskflow-user', JSON.stringify(userData))
    setUser(userData)

    return userData
  }

  // Logout — clears token and user
  const logout = () => {
    localStorage.removeItem('taskflow-token')
    localStorage.removeItem('taskflow-user')
    setUser(null)
  }

  // The "value" is what child components receive when they useAuth()
  const value = {
    user,
    loading,
    isAuthenticated: !!user, // !! converts to boolean
    login,
    register,
    logout,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// 3. Custom hook — shorthand so components write useAuth() instead of useContext(AuthContext)
export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider')
  }
  return context
}
