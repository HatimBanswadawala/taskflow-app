import { Calendar, AlertCircle, Pencil, Trash2 } from 'lucide-react'
import { useSortable } from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { cn } from '@/lib/utils'
import { PRIORITY_STYLES, formatDueDate, isOverdue } from '@/lib/taskHelpers'

/**
 * Task card — entire card is draggable.
 * Edit/delete buttons stop propagation so clicks still work.
 */
export default function TaskCard({ task, onEdit, onDelete }) {
  const dueDateText = formatDueDate(task.dueDate)
  const overdue = isOverdue(task.dueDate)

  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({
    id: task.id,
    data: { type: 'task', task },
  })

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.4 : 1,
  }

  return (
    <div
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      className={cn(
        'group bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg p-3 shadow-sm hover:shadow-md hover:border-cyan-500 dark:hover:border-cyan-500 transition-all cursor-grab active:cursor-grabbing',
        isDragging && 'ring-2 ring-cyan-500 shadow-xl'
      )}
    >
      {/* Title row + actions */}
      <div className="flex items-start justify-between gap-2 mb-2">
        <h4 className="font-medium text-sm leading-snug">{task.title}</h4>
        <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity flex-shrink-0">
          <button
            onPointerDown={(e) => e.stopPropagation()}
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
            onPointerDown={(e) => e.stopPropagation()}
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

      {/* Description */}
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
