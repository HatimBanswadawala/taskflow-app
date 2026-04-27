import { useState, useMemo } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Loader2, AlertCircle, Search, X } from 'lucide-react'
import {
  DndContext,
  DragOverlay,
  PointerSensor,
  useSensor,
  useSensors,
  closestCorners,
} from '@dnd-kit/core'
import { boardApi } from '@/services/boardApi'
import { taskApi } from '@/services/taskApi'
import { useDebounce } from '@/hooks/useDebounce'
import Button from '@/components/Button'
import ThemeToggle from '@/components/ThemeToggle'
import Column from '@/components/Column'
import TaskCard from '@/components/TaskCard'
import TaskModal from '@/components/TaskModal'

export default function BoardDetail() {
  const { id } = useParams()
  const queryClient = useQueryClient()

  // Search state — value updates on every keystroke
  const [searchInput, setSearchInput] = useState('')
  // Debounced value — updates 300ms after last keystroke (used for actual filtering)
  const debouncedSearch = useDebounce(searchInput, 300)

  // Modal state — controls which modal is shown
  const [taskModalState, setTaskModalState] = useState({
    isOpen: false,
    columnId: null,  // for create
    task: null,      // for edit (null = create mode)
  })

  // Drag state — track which task is being dragged for the DragOverlay
  const [activeTask, setActiveTask] = useState(null)

  // Sensors — require pointer to move 5px before drag starts (prevents accidental drag on click)
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } })
  )

  // Fetch board with all its columns and tasks
  // refetchInterval: auto-refresh every 5 seconds for collaborative live updates
  const { data: board, isLoading, error } = useQuery({
    queryKey: ['board', id],
    queryFn: () => boardApi.getById(id),
    enabled: !!id,
    refetchInterval: 5000,         // poll every 5s for collaborative awareness
    refetchIntervalInBackground: false, // stop polling when tab is hidden (saves bandwidth)
  })

  // Filter tasks by search — useMemo caches the result, only recomputes when dependencies change
  const filteredColumns = useMemo(() => {
    if (!board?.columns) return []
    if (!debouncedSearch.trim()) return board.columns  // no filter → return all

    const query = debouncedSearch.toLowerCase()
    return board.columns.map(col => ({
      ...col,
      tasks: col.tasks?.filter(task =>
        task.title.toLowerCase().includes(query) ||
        task.description?.toLowerCase().includes(query)
      ) ?? []
    }))
  }, [board, debouncedSearch])

  // Count how many tasks match (for UI feedback)
  const matchCount = filteredColumns.reduce(
    (sum, col) => sum + (col.tasks?.length ?? 0),
    0
  )

  // Delete task mutation
  const deleteTask = useMutation({
    mutationFn: (taskId) => taskApi.delete(taskId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', id] })
    },
  })

  // Move task mutation (drag-drop backend)
  const moveTask = useMutation({
    mutationFn: ({ taskId, targetColumnId, newPosition }) =>
      taskApi.move(taskId, { taskId, targetColumnId, newPosition }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', id] })
    },
  })

  // Helper — find a task by ID across all columns
  const findTaskById = (taskId) => {
    for (const col of board?.columns ?? []) {
      const task = col.tasks?.find((t) => t.id === taskId)
      if (task) return { task, columnId: col.id }
    }
    return null
  }

  // Drag START — user picked up a card
  const handleDragStart = (event) => {
    const found = findTaskById(event.active.id)
    if (found) setActiveTask(found.task)
  }

  // Drag END — user dropped the card
  const handleDragEnd = (event) => {
    const { active, over } = event
    setActiveTask(null)
    if (!over) return  // dropped outside a valid zone

    const activeId = active.id  // task being dragged
    const overId = over.id       // what it was dropped on (task OR column)

    // Find source column
    const source = findTaskById(activeId)
    if (!source) return

    // Determine target column
    // If dropped ON another task → target column = that task's column
    // If dropped ON a column directly → target column = that column
    let targetColumnId
    let newPosition

    const droppedOnTask = findTaskById(overId)
    if (droppedOnTask) {
      targetColumnId = droppedOnTask.columnId
      // Position = position of the task we dropped on
      const targetColumn = board.columns.find((c) => c.id === targetColumnId)
      newPosition = targetColumn.tasks.findIndex((t) => t.id === overId)
    } else {
      // Dropped on a column directly (usually empty column)
      targetColumnId = overId
      const targetColumn = board.columns.find((c) => c.id === targetColumnId)
      newPosition = targetColumn?.tasks?.length ?? 0  // add to bottom
    }

    // Skip if dropped in same spot
    if (source.columnId === targetColumnId) {
      const sourceColumn = board.columns.find((c) => c.id === source.columnId)
      const sourcePosition = sourceColumn.tasks.findIndex((t) => t.id === activeId)
      if (sourcePosition === newPosition) return
    }

    moveTask.mutate({ taskId: activeId, targetColumnId, newPosition })
  }

  // Handlers passed down to Column → TaskCard
  const handleAddTask = (columnId) => {
    setTaskModalState({ isOpen: true, columnId, task: null })
  }

  const handleEditTask = (task) => {
    setTaskModalState({ isOpen: true, columnId: null, task })
  }

  const handleDeleteTask = (task) => {
    if (confirm(`Delete task "${task.title}"?`)) {
      deleteTask.mutate(task.id)
    }
  }

  const closeModal = () => {
    setTaskModalState({ isOpen: false, columnId: null, task: null })
  }

  // Loading state
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="w-6 h-6 animate-spin text-slate-400" />
      </div>
    )
  }

  // Error state
  if (error || !board) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <AlertCircle className="w-12 h-12 mx-auto text-slate-400 mb-3" />
          <p className="text-slate-500 mb-4">Board not found</p>
          <Link to="/dashboard">
            <Button variant="secondary">Back to Dashboard</Button>
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-sky-100/50 to-cyan-50 dark:from-slate-950 dark:via-slate-950 dark:to-slate-950 text-slate-900 dark:text-slate-50 flex flex-col">
      {/* Header */}
      <header className="border-b border-slate-200 dark:border-slate-800 bg-white/80 dark:bg-slate-900/80 backdrop-blur-md shadow-sm sticky top-0 z-10">
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

      {/* Search bar */}
      <div className="max-w-7xl mx-auto w-full px-4 pt-4">
        <div className="relative max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 pointer-events-none" />
          <input
            type="text"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            placeholder="Search tasks..."
            className="w-full pl-10 pr-10 py-2 rounded-lg border border-slate-300 dark:border-slate-600 bg-white/80 dark:bg-slate-900 backdrop-blur-sm focus:outline-none focus:ring-2 focus:ring-cyan-500"
          />
          {searchInput && (
            <button
              onClick={() => setSearchInput('')}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
              title="Clear search"
            >
              <X className="w-4 h-4" />
            </button>
          )}
        </div>
        {debouncedSearch && (
          <p className="text-xs text-slate-500 mt-2">
            {matchCount} {matchCount === 1 ? 'task' : 'tasks'} match "{debouncedSearch}"
          </p>
        )}
      </div>

      {/* Board content wrapped in DndContext for drag-and-drop */}
      <DndContext
        sensors={sensors}
        collisionDetection={closestCorners}
        onDragStart={handleDragStart}
        onDragEnd={handleDragEnd}
      >
        <main className="flex-1 overflow-x-auto">
          <div className="px-4 py-6">
            <div className="flex gap-4 min-h-full">
              {filteredColumns.map((column) => (
                <Column
                  key={column.id}
                  column={column}
                  onAddTask={handleAddTask}
                  onEditTask={handleEditTask}
                  onDeleteTask={handleDeleteTask}
                />
              ))}
            </div>
          </div>
        </main>

        {/* DragOverlay — shows floating card at cursor while dragging */}
        <DragOverlay>
          {activeTask ? (
            <TaskCard task={activeTask} onEdit={() => {}} onDelete={() => {}} />
          ) : null}
        </DragOverlay>
      </DndContext>

      {/* Task modal — handles both create and edit */}
      <TaskModal
        isOpen={taskModalState.isOpen}
        onClose={closeModal}
        boardId={id}
        columnId={taskModalState.columnId}
        task={taskModalState.task}
      />
    </div>
  )
}
