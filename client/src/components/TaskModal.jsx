import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Save, Plus, Loader2 } from 'lucide-react'
import { taskApi } from '@/services/taskApi'
import { PRIORITY, PRIORITY_TO_VALUE } from '@/lib/taskHelpers'
import Modal from './Modal'
import Button from './Button'

/**
 * One modal handles both CREATE and EDIT.
 * - If `task` prop is null → Create mode
 * - If `task` prop has a value → Edit mode
 */
export default function TaskModal({ isOpen, onClose, boardId, columnId, task = null }) {
  const queryClient = useQueryClient()
  const isEditMode = !!task

  // Form state — initialize from task when editing
  const [form, setForm] = useState({
    title: '',
    description: '',
    priority: PRIORITY.MEDIUM,
    dueDate: '',
  })
  const [error, setError] = useState('')

  // When modal opens, prefill from task (edit) or reset (create)
  useEffect(() => {
    if (isOpen) {
      if (task) {
        setForm({
          title: task.title || '',
          description: task.description || '',
          priority: task.priority || PRIORITY.MEDIUM,
          dueDate: task.dueDate ? task.dueDate.split('T')[0] : '',
        })
      } else {
        setForm({ title: '', description: '', priority: PRIORITY.MEDIUM, dueDate: '' })
      }
      setError('')
    }
  }, [isOpen, task])

  // Create mutation
  const createTask = useMutation({
    mutationFn: (payload) => taskApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] })
      onClose()
    },
    onError: (err) => {
      setError(err.response?.data?.errors?.[0]?.errorMessage || 'Failed to create task')
    },
  })

  // Update mutation
  const updateTask = useMutation({
    mutationFn: ({ id, payload }) => taskApi.update(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] })
      onClose()
    },
    onError: (err) => {
      setError(err.response?.data?.errors?.[0]?.errorMessage || 'Failed to update task')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    if (!form.title.trim()) return

    const payload = {
      title: form.title,
      description: form.description || null,
      priority: PRIORITY_TO_VALUE[form.priority],
      dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : null,
      assignedToId: null,
    }

    if (isEditMode) {
      // Update — include id
      updateTask.mutate({ id: task.id, payload: { ...payload, id: task.id } })
    } else {
      // Create — include columnId
      createTask.mutate({ ...payload, columnId })
    }
  }

  const isPending = createTask.isPending || updateTask.isPending

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={isEditMode ? 'Edit Task' : 'New Task'}>
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && (
          <div className="bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 text-red-700 dark:text-red-400 text-sm rounded-lg p-3">
            {error}
          </div>
        )}

        {/* Title */}
        <div>
          <label className="block text-sm font-medium mb-1">Title *</label>
          <input
            type="text"
            value={form.title}
            onChange={(e) => setForm({ ...form, title: e.target.value })}
            autoFocus
            required
            maxLength={200}
            placeholder="What needs to be done?"
            className="w-full px-3 py-2 rounded-lg border border-slate-300 dark:border-slate-600 bg-slate-50 dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-cyan-500"
          />
        </div>

        {/* Description */}
        <div>
          <label className="block text-sm font-medium mb-1">Description</label>
          <textarea
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            rows={3}
            maxLength={1000}
            placeholder="Additional details..."
            className="w-full px-3 py-2 rounded-lg border border-slate-300 dark:border-slate-600 bg-slate-50 dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-cyan-500 resize-none"
          />
        </div>

        {/* Priority + Due Date side-by-side */}
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium mb-1">Priority</label>
            <select
              value={form.priority}
              onChange={(e) => setForm({ ...form, priority: e.target.value })}
              className="w-full px-3 py-2 rounded-lg border border-slate-300 dark:border-slate-600 bg-slate-50 dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-cyan-500"
            >
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
              <option value="Urgent">Urgent</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">Due Date</label>
            <input
              type="date"
              value={form.dueDate}
              onChange={(e) => setForm({ ...form, dueDate: e.target.value })}
              className="w-full px-3 py-2 rounded-lg border border-slate-300 dark:border-slate-600 bg-slate-50 dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-cyan-500"
            />
          </div>
        </div>

        {/* Action buttons */}
        <div className="flex gap-2 justify-end pt-2">
          <Button type="button" variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" disabled={isPending}>
            {isPending ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                Saving...
              </>
            ) : isEditMode ? (
              <>
                <Save className="w-4 h-4" />
                Save Changes
              </>
            ) : (
              <>
                <Plus className="w-4 h-4" />
                Create Task
              </>
            )}
          </Button>
        </div>
      </form>
    </Modal>
  )
}
