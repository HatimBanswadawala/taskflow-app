import { LogOut } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'
import { useNavigate } from 'react-router-dom'

export default function Dashboard() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-50">
      {/* Header */}
      <header className="border-b border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900">
        <div className="max-w-6xl mx-auto px-4 py-4 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-cyan-500 to-blue-600 flex items-center justify-center text-white font-bold text-sm">
              TF
            </div>
            <h1 className="text-xl font-bold">TaskFlow</h1>
          </div>
          <div className="flex items-center gap-4">
            <span className="text-sm text-slate-600 dark:text-slate-400">
              Hi, <span className="font-medium">{user?.fullName}</span>
            </span>
            <button
              onClick={handleLogout}
              className="flex items-center gap-2 px-4 py-2 rounded-lg bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 text-sm font-medium"
            >
              <LogOut className="w-4 h-4" />
              Logout
            </button>
          </div>
        </div>
      </header>

      {/* Content */}
      <main className="max-w-6xl mx-auto px-4 py-12">
        <h2 className="text-3xl font-bold mb-4">Welcome, {user?.fullName}!</h2>
        <p className="text-slate-600 dark:text-slate-400 mb-8">
          You're logged in. Boards and tasks coming in next session.
        </p>

        <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl p-6">
          <h3 className="font-semibold mb-2">Session Info</h3>
          <div className="text-sm text-slate-600 dark:text-slate-400 space-y-1">
            <p>User ID: <code className="bg-slate-100 dark:bg-slate-800 px-2 py-0.5 rounded text-xs">{user?.userId}</code></p>
            <p>Email: {user?.email}</p>
          </div>
        </div>
      </main>
    </div>
  )
}
