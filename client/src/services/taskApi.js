import apiClient from './apiClient'

/**
 * All task-related API calls.
 * Mirrors the .NET API task endpoints from Session 2.
 */

export const taskApi = {
  // POST /api/tasks — create new task in a column
  create: async (payload) => {
    // payload = { title, description, priority, columnId, assignedToId, dueDate }
    const { data } = await apiClient.post('/tasks', payload)
    return data // { id: "new-task-guid" }
  },

  // PUT /api/tasks/:id — update task details
  update: async (id, payload) => {
    await apiClient.put(`/tasks/${id}`, payload)
  },

  // DELETE /api/tasks/:id — remove task
  delete: async (id) => {
    await apiClient.delete(`/tasks/${id}`)
  },

  // PUT /api/tasks/:id/move — move task between columns (drag-drop backend)
  move: async (id, payload) => {
    // payload = { taskId, targetColumnId, newPosition }
    await apiClient.put(`/tasks/${id}/move`, payload)
  },
}
