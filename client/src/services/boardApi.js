import apiClient from './apiClient'

/**
 * All board-related API calls live here.
 * Each function returns a promise. TanStack Query will handle loading/error states.
 */

export const boardApi = {
  // GET /api/boards — fetch all boards for current user
  getAll: async () => {
    const { data } = await apiClient.get('/boards')
    return data
  },

  // GET /api/boards/:id — fetch single board with columns + tasks
  getById: async (id) => {
    const { data } = await apiClient.get(`/boards/${id}`)
    return data
  },

  // POST /api/boards — create new board (auto-adds 3 default columns)
  create: async (payload) => {
    // payload = { name, description, userId }
    const { data } = await apiClient.post('/boards', payload)
    return data // { id: "new-board-guid" }
  },

  // PUT /api/boards/:id — update name/description
  update: async (id, payload) => {
    await apiClient.put(`/boards/${id}`, payload)
  },

  // DELETE /api/boards/:id — cascade deletes columns + tasks
  delete: async (id) => {
    await apiClient.delete(`/boards/${id}`)
  },
}
