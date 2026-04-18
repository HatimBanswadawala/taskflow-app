import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ArrowLeft, Loader2 } from 'lucide-react'
import { boardApi } from '@/services/boardApi'
import Button from '@/components/Button'
import ThemeToggle from '@/components/ThemeToggle'

export default function BoardDetail() {
  // useParams — reads URL parameters from the route (e.g., /boards/:id)
  const { id } = useParams()

  const { data: board, isLoading, error } = useQuery({
    queryKey: ['board', id],              // Unique key per board
    queryFn: () => boardApi.getById(id),
    enabled: !!id,                         // Only run query if id exists
  })

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="w-6 h-6 animate-spin text-slate-400" />
      </div>
    )
  }

  if (error || !board) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <p className="text-slate-500 mb-4">Board not found</p>
          <Link to="/dashboard">
            <Button variant="secondary">Back to Dashboard</Button>
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-50">
      <header className="border-b border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900">
        <div className="max-w-7xl mx-auto px-4 py-4 flex items-center justify-between gap-4">
          <div className="flex items-center gap-4 min-w-0">
            <Link to="/dashboard">
              <Button variant="ghost" size="sm">
                <ArrowLeft className="w-4 h-4" />
                Back
              </Button>
            </Link>
            <div className="min-w-0">
              <h1 className="text-xl font-bold truncate">{board.name}</h1>
              {board.description && (
                <p className="text-sm text-slate-500 truncate">{board.description}</p>
              )}
            </div>
          </div>
          <ThemeToggle />
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-4 py-8">
        <div className="text-center py-16 border-2 border-dashed border-slate-200 dark:border-slate-800 rounded-xl">
          <h3 className="font-semibold mb-2">Board view coming in Session 7</h3>
          <p className="text-sm text-slate-500">Will show columns + tasks with drag-and-drop</p>
          <p className="text-xs text-slate-400 mt-4">
            {board.columns?.length ?? 0} columns,{' '}
            {board.columns?.reduce((sum, c) => sum + (c.tasks?.length ?? 0), 0) ?? 0} tasks
          </p>
        </div>
      </main>
    </div>
  )
}
