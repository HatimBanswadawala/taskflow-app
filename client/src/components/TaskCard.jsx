import { Calendar, AlertCircle, Pencil, Trash2 } from 'lucide-react'
import { cn } from '@/lib/utils'
import { PRIORITY_STYLES, formatDueDate, isOverdue } from '@/lib/taskHelpers'

/**
 * Single task card displayed in a column.
 * Shows title, description, priority badge, due date.
 * Hover reveals edit/delete buttons.
 */
export default function TaskCard({ task, onEdit, onDelete }) {
  const dueDateText = formatDueDate(task.dueDate)
  const overdue = isOverdue(task.dueDate)

  return (
    <div className="group bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg p-3 shadow-sm hover:shadow-md hover:border-cyan-500 dark:hover:border-cyan-500 transition-all cursor-pointer">
      {/* Title row + actions */}
      <div className="flex items-start justify-between gap-2 mb-2">
        <h4 className="font-medium text-sm leading-snug">{task.title}</h4>
        <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity flex-shrink-0">
          <button
            onClick={(e) => {
              e.stopPropagation()
              onEdit(task)
            }}
            className="p-1 rounded hover:bg-slate-100 dark:hover:bg-slate-700 text-slate-400 hover:text-cyan-600"
            title="Edit task"
          >
            <Pencil className="w-3.5 h-3.5" />
          </button>
          <button
            onClick={(e) => {
              e.stopPropagation()
              onDelete(task)
            }}
            className="p-1 rounded hover:bg-red-50 dark:hover:bg-red-950 text-slate-400 hover:text-red-600"
            title="Delete task"
          >
            <Trash2 className="w-3.5 h-3.5" />
          </button>
        </div>
      </div>

      {/* Description (if exists) */}
      {task.description && (
        <p className="text-xs text-slate-500 dark:text-slate-400 line-clamp-2 mb-2">
          {task.description}
        </p>
      )}

      {/* Footer — priority + due date */}
      <div className="flex items-center justify-between gap-2 mt-2">
        <span className={cn(
          'inline-flex items-center px-2 py-0.5 rounded text-xs font-medium',
          PRIORITY_STYLES[task.priority]
        )}>
          {task.priority}
        </span>

        {dueDateText && (
          <span className={cn(
            'inline-flex items-center gap-1 text-xs',
            overdue ? 'text-red-600 dark:text-red-400 font-medium' : 'text-slate-500'
          )}>
            {overdue ? (
              <AlertCircle className="w-3 h-3" />
            ) : (
              <Calendar className="w-3 h-3" />
            )}
            {dueDateText}
          </span>
        )}
      </div>
    </div>
  )
}
