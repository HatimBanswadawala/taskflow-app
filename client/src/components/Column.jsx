import { Plus } from 'lucide-react'
import { useDroppable } from '@dnd-kit/core'
import { SortableContext, verticalListSortingStrategy } from '@dnd-kit/sortable'
import { cn } from '@/lib/utils'
import TaskCard from './TaskCard'
import Button from './Button'

/**
 * Column — now a drop zone AND a sortable container.
 * - useDroppable: makes the column area accept task drops (even empty column)
 * - SortableContext: wraps children so they can reorder within this column
 */
export default function Column({ column, onAddTask, onEditTask, onDeleteTask }) {
  const taskCount = column.tasks?.length ?? 0
  const taskIds = column.tasks?.map((t) => t.id) ?? []

  // useDroppable — registers this column as a drop target
  const { setNodeRef, isOver } = useDroppable({
    id: column.id,
    data: { type: 'column', columnId: column.id },
  })

  return (
    <div
      ref={setNodeRef}
      className={cn(
        'bg-white/70 dark:bg-slate-900 rounded-xl p-3 flex flex-col w-full sm:w-80 flex-shrink-0 border border-slate-200 dark:border-slate-800 shadow-sm backdrop-blur-sm transition-colors',
        isOver && 'bg-cyan-50 dark:bg-slate-800 border-cyan-400 border-2'  // highlight when dragging over
      )}
    >
      {/* Column header */}
      <div className="flex items-center justify-between mb-3 px-1">
        <div className="flex items-center gap-2">
          <h3 className="font-semibold text-sm">{column.name}</h3>
          <span className="text-xs text-slate-500 bg-slate-200 dark:bg-slate-700 px-2 py-0.5 rounded-full">
            {taskCount}
          </span>
        </div>
      </div>

      {/* Task list — SortableContext enables reordering within this column */}
      <SortableContext items={taskIds} strategy={verticalListSortingStrategy}>
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
            <div className="text-center py-6 text-xs text-slate-400 border-2 border-dashed border-slate-200 dark:border-slate-700 rounded-lg">
              Drop tasks here
            </div>
          )}
        </div>
      </SortableContext>

      {/* Add task button */}
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
