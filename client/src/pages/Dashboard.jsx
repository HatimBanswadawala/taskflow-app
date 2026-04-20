import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { LogOut, Plus, LayoutGrid, Trash2, Loader2, AlertCircle } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'
import { boardApi } from '@/services/boardApi'
import Button from '@/components/Button'
import CreateBoardModal from '@/components/CreateBoardModal'
import ThemeToggle from '@/components/ThemeToggle'
import DashboardStats from '@/components/DashboardStats'

export default function Dashboard() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const [showCreateModal, setShowCreateModal] = useState(false)

  // useQuery — for GET requests (reads server data)
  // Returns: data, isLoading, error, and more
  const { data: boards, isLoading, error } = useQuery({
    queryKey: ['boards'],           // Cache key — identifies this query
    queryFn: boardApi.getAll,       // Function that fetches data
  })

  // useMutation for deleting
  const deleteBoard = useMutation({
    mutationFn: (id) => boardApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['boards'] })
    },
  })

  const handleDelete = (e, boardId, boardName) => {
    e.preventDefault()     // Prevent Link navigation
    e.stopPropagation()    // Don't bubble to card
    if (confirm(`Delete "${boardName}"? This will delete all columns and tasks.`)) {
      deleteBoard.mutate(boardId)
    }
  }

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-sky-100/50 to-cyan-50 dark:from-slate-950 dark:via-slate-950 dark:to-slate-950 text-slate-900 dark:text-slate-50">
      {/* Header */}
      <header className="border-b border-slate-200 dark:border-slate-800 bg-white/80 dark:bg-slate-900/80 sticky top-0 z-10 backdrop-blur-md shadow-sm">
        <div className="max-w-6xl mx-auto px-4 py-4 flex items-center justify-between">
          <Link to="/dashboard" className="flex items-center gap-2">
            <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-cyan-500 to-blue-600 flex items-center justify-center text-white font-bold text-sm">
              TF
            </div>
            <h1 className="text-xl font-bold">TaskFlow</h1>
          </Link>
          <div className="flex items-center gap-3">
            <span className="text-sm text-slate-600 dark:text-slate-400 hidden sm:block">
              Hi, <span className="font-medium">{user?.fullName}</span>
            </span>
            <ThemeToggle />
            <Button variant="secondary" size="sm" onClick={handleLogout}>
              <LogOut className="w-4 h-4" />
              Logout
            </Button>
          </div>
        </div>
      </header>

      {/* Content */}
      <main className="max-w-6xl mx-auto px-4 py-8">
        {/* Stats row — only show when boards have loaded */}
        {boards && boards.length > 0 && <DashboardStats boards={boards} />}

        {/* Title + Create button */}
        <div className="flex items-center justify-between mb-8">
          <div>
            <h2 className="text-2xl font-bold mb-1">My Boards</h2>
            <p className="text-sm text-slate-500 dark:text-slate-400">
              {boards?.length ?? 0} {boards?.length === 1 ? 'board' : 'boards'}
            </p>
          </div>
          <Button onClick={() => setShowCreateModal(true)}>
            <Plus className="w-4 h-4" />
            New Board
          </Button>
        </div>

        {/* Loading state */}
        {isLoading && (
          <div className="flex items-center justify-center py-16 text-slate-500">
            <Loader2 className="w-6 h-6 animate-spin mr-2" />
            Loading boards...
          </div>
        )}

        {/* Error state */}
        {error && (
          <div className="bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 text-red-700 dark:text-red-400 rounded-lg p-4 flex items-start gap-3">
            <AlertCircle className="w-5 h-5 flex-shrink-0 mt-0.5" />
            <div>
              <p className="font-medium">Failed to load boards</p>
              <p className="text-sm opacity-80">{error.message}</p>
            </div>
          </div>
        )}

        {/* Empty state */}
        {!isLoading && boards?.length === 0 && (
          <div className="text-center py-16 border-2 border-dashed border-slate-200 dark:border-slate-800 rounded-xl">
            <LayoutGrid className="w-12 h-12 mx-auto text-slate-400 mb-3" />
            <h3 className="font-semibold mb-1">No boards yet</h3>
            <p className="text-sm text-slate-500 mb-4">Create your first board to get started</p>
            <Button onClick={() => setShowCreateModal(true)}>
              <Plus className="w-4 h-4" />
              Create Board
            </Button>
          </div>
        )}

        {/* Board grid */}
        {boards?.length > 0 && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {boards.map((board) => (
              <Link
                key={board.id}
                to={`/boards/${board.id}`}
                className="group relative bg-white dark:bg-slate-800 border border-white dark:border-slate-700 rounded-xl p-5 shadow-lg shadow-blue-200/40 dark:shadow-slate-900/40 hover:shadow-xl hover:shadow-cyan-300/50 dark:hover:shadow-cyan-500/30 hover:-translate-y-1 hover:border-cyan-400 dark:hover:border-cyan-500 transition-all"
              >
                <div className="flex items-start justify-between mb-3">
                  <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-cyan-500 to-blue-600 flex items-center justify-center">
                    <LayoutGrid className="w-5 h-5 text-white" />
                  </div>
                  <button
                    onClick={(e) => handleDelete(e, board.id, board.name)}
                    className="opacity-0 group-hover:opacity-100 transition-opacity p-1.5 rounded hover:bg-red-50 dark:hover:bg-red-950 text-slate-400 hover:text-red-600"
                    title="Delete board"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
                <h3 className="font-semibold mb-1 truncate">{board.name}</h3>
                {board.description && (
                  <p className="text-sm text-slate-500 dark:text-slate-400 line-clamp-2 mb-3">
                    {board.description}
                  </p>
                )}
                <div className="flex gap-3 text-xs text-slate-500">
                  <span>{board.columns?.length ?? 0} columns</span>
                  <span>•</span>
                  <span>
                    {board.columns?.reduce((sum, c) => sum + (c.tasks?.length ?? 0), 0) ?? 0} tasks
                  </span>
                </div>
              </Link>
            ))}
          </div>
        )}
      </main>

      {/* Create Board Modal */}
      <CreateBoardModal
        isOpen={showCreateModal}
        onClose={() => setShowCreateModal(false)}
      />
    </div>
  )
}
