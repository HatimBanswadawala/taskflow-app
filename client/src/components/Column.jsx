import { Plus } from 'lucide-react'
import TaskCard from './TaskCard'
import Button from './Button'

/**
 * A column in the Kanban board (e.g., "To Do", "In Progress").
 * Renders all task cards belonging to this column.
 */
export default function Column({ column, onAddTask, onEditTask, onDeleteTask }) {
  const taskCount = column.tasks?.length ?? 0

  return (
    <div className="bg-white/70 dark:bg-slate-900 rounded-xl p-3 flex flex-col w-full sm:w-80 flex-shrink-0 border border-slate-200 dark:border-slate-800 shadow-sm backdrop-blur-sm">
      {/* Column header */}
      <div className="flex items-center justify-between mb-3 px-1">
        <div className="flex items-center gap-2">
          <h3 className="font-semibold text-sm">{column.name}</h3>
          <span className="text-xs text-slate-500 bg-slate-200 dark:bg-slate-700 px-2 py-0.5 rounded-full">
            {taskCount}
          </span>
        </div>
      </div>

      {/* Task list */}
      <div className="space-y-2 flex-1 min-h-[100px]">
        {column.tasks?.map((task) => (
          <TaskCard
            key={task.id}
            task={task}
            onEdit={onEditTask}
            onDelete={onDeleteTask}
          />
        ))}

        {/* Empty state inside column */}
        {taskCount === 0 && (
          <div className="text-center py-6 text-xs text-slate-400">
            No tasks yet
          </div>
        )}
      </div>

      {/* Add task button at bottom */}
      <Button
        variant="ghost"
        size="sm"
        onClick={() => onAddTask(column.id)}
        className="mt-2 w-full justify-start text-slate-500"
      >
        <Plus className="w-4 h-4" />
        Add task
      </Button>
    </div>
  )
}
