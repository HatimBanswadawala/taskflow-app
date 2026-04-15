import { useState, useEffect } from 'react'

function App() {
  // useState — creates a state variable. Returns [value, setter].
  // The initializer function runs ONCE on first render (lazy init).
  const [darkMode, setDarkMode] = useState(() => {
    const saved = localStorage.getItem('taskflow-theme')
    if (saved) return saved === 'dark'
    return window.matchMedia('(prefers-color-scheme: dark)').matches
  })

  // useEffect — runs AFTER render. The [darkMode] array means
  // "re-run this whenever darkMode changes".
  useEffect(() => {
    const root = document.documentElement
    if (darkMode) {
      root.classList.add('dark')
      localStorage.setItem('taskflow-theme', 'dark')
    } else {
      root.classList.remove('dark')
      localStorage.setItem('taskflow-theme', 'light')
    }
  }, [darkMode])

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
          <button
            onClick={() => setDarkMode(!darkMode)}
            className="px-4 py-2 rounded-lg bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 text-sm font-medium"
          >
            {darkMode ? '☀ Light' : '🌙 Dark'}
          </button>
        </div>
      </header>

      {/* Hero */}
      <main className="max-w-6xl mx-auto px-4 py-16">
        <div className="text-center mb-12">
          <h2 className="text-5xl font-bold mb-4 bg-gradient-to-r from-cyan-500 to-blue-600 bg-clip-text text-transparent">
            TaskFlow
          </h2>
          <p className="text-lg text-slate-600 dark:text-slate-400 max-w-2xl mx-auto">
            Real-time Kanban task management built with .NET 9 and React 18.
            Track your projects, collaborate in real-time, stay productive.
          </p>
        </div>

        {/* 3 Feature Cards — notice we call <FeatureCard /> 3 times with different props */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
          <FeatureCard
            icon="📋"
            title="Kanban Boards"
            description="Organize tasks into columns — To Do, In Progress, Done"
          />
          <FeatureCard
            icon="⚡"
            title="Real-Time Updates"
            description="See changes instantly across all users via SignalR"
          />
          <FeatureCard
            icon="🎯"
            title="Drag & Drop"
            description="Move tasks between columns with smooth animations"
          />
        </div>

        {/* Status Banner */}
        <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl p-6 text-center">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-green-100 dark:bg-green-950 text-green-700 dark:text-green-400 text-sm font-medium mb-3">
            <span className="w-2 h-2 rounded-full bg-green-500 animate-pulse"></span>
            Vite + React + Tailwind ready
          </div>
          <p className="text-slate-600 dark:text-slate-400 text-sm">
            Next: Authentication pages, Board view, Task management
          </p>
        </div>
      </main>

      <footer className="border-t border-slate-200 dark:border-slate-800 mt-12 py-6 text-center text-sm text-slate-500">
        TaskFlow &copy; 2026 — Built by Hatim Banswadawala
      </footer>
    </div>
  )
}

// A separate component — takes props and renders a card
// Notice: no TypeScript, just destructure props
function FeatureCard({ icon, title, description }) {
  return (
    <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl p-6 hover:shadow-lg">
      <div className="text-4xl mb-3">{icon}</div>
      <h3 className="text-lg font-semibold mb-2">{title}</h3>
      <p className="text-sm text-slate-600 dark:text-slate-400">{description}</p>
    </div>
  )
}

export default App
