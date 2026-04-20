/**
 * Helpers for displaying tasks consistently across the app.
 * Centralizes UI styling logic so it's not duplicated in components.
 */

// Priority enum matching .NET backend (Priority.cs)
export const PRIORITY = {
  LOW: 'Low',
  MEDIUM: 'Medium',
  HIGH: 'High',
  URGENT: 'Urgent',
}

// Maps priority name → numeric value (the backend expects integers)
export const PRIORITY_TO_VALUE = {
  Low: 0,
  Medium: 1,
  High: 2,
  Urgent: 3,
}

// Color classes for each priority — used on badges
export const PRIORITY_STYLES = {
  Low: 'bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400',
  Medium: 'bg-blue-100 dark:bg-blue-950 text-blue-700 dark:text-blue-400',
  High: 'bg-orange-100 dark:bg-orange-950 text-orange-700 dark:text-orange-400',
  Urgent: 'bg-red-100 dark:bg-red-950 text-red-700 dark:text-red-400',
}

// Format a date for display (e.g., "May 20")
export function formatDueDate(isoString) {
  if (!isoString) return null
  const date = new Date(isoString)
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
}

// Check if due date is in the past
export function isOverdue(isoString) {
  if (!isoString) return false
  return new Date(isoString) < new Date()
}
