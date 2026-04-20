import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Loader2 } from 'lucide-react'
import { boardApi } from '@/services/boardApi'
import { useAuth } from '@/context/AuthContext'
import Modal from './Modal'
import Button from './Button'

export default function CreateBoardModal({ isOpen, onClose }) {
  const { user } = useAuth()
  const queryClient = useQueryClient()

  const [form, setForm] = useState({ name: '', description: '' })
  const [error, setError] = useState('')

  // useMutation — for POST/PUT/DELETE (anything that changes server state)
  const createBoard = useMutation({
    mutationFn: (payload) => boardApi.create(payload),
    onSuccess: () => {
      // Refetch the boards list automatically
      queryClient.invalidateQueries({ queryKey: ['boards'] })
      // Reset form + close modal
      setForm({ name: '', description: '' })
      setError('')
      onClose()
    },
    onError: (err) => {
      const msg = err.response?.data?.errors?.[0]?.errorMessage || 'Failed to create board'
      setError(msg)
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    if (!form.name.trim()) return

    createBoard.mutate({
      name: form.name,
      description: form.description || null,
      userId: user.userId,
    })
  }

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Create New Board">
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && (
          <div className="bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 text-red-700 dark:text-red-400 text-sm rounded-lg p-3">
            {error}
          </div>
        )}

        <div>
          <label className="block text-sm font-medium mb-1">Board Name *</label>
          <input
            type="text"
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            autoFocus
            required
            maxLength={100}
            placeholder="e.g., Sprint 1, Personal Tasks"
            className="w-full px-3 py-2 rounded-lg border border-slate-300 dark:border-slate-600 bg-slate-50 dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-cyan-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Description (optional)</label>
          <textarea
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            rows={3}
            maxLength={500}
            placeholder="What's this board about?"
            className="w-full px-3 py-2 rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-950 focus:outline-none focus:ring-2 focus:ring-cyan-500 resize-none"
          />
        </div>

        <div className="flex gap-2 justify-end pt-2">
          <Button type="button" variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" disabled={createBoard.isPending}>
            {createBoard.isPending ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                Creating...
              </>
            ) : (
              <>
                <Plus className="w-4 h-4" />
                Create Board
              </>
            )}
          </Button>
        </div>
      </form>
    </Modal>
  )
}
