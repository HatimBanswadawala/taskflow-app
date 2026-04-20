import { LayoutGrid, CheckCircle2, AlertTriangle, Clock } from 'lucide-react'
import { isOverdue } from '@/lib/taskHelpers'

/**
 * Shows key metrics at the top of the Dashboard.
 * Computed from the boards array we already fetched — no extra API call.
 */
export default function DashboardStats({ boards }) {
  // Flatten all tasks across all boards + columns
  const allTasks = boards.flatMap(b =>
    b.columns?.flatMap(c => c.tasks ?? []) ?? []
  )

  const totalBoards = boards.length
  const totalTasks = allTasks.length
  const doneTasks = allTasks.filter(t => t.status === 'Done').length
  const overdueTasks = allTasks.filter(
    t => t.status !== 'Done' && isOverdue(t.dueDate)
  ).length

  const stats = [
    { label: 'Boards', value: totalBoards, icon: LayoutGrid, color: 'cyan' },
    { label: 'Total Tasks', value: totalTasks, icon: Clock, color: 'blue' },
    { label: 'Completed', value: doneTasks, icon: CheckCircle2, color: 'green' },
    { label: 'Overdue', value: overdueTasks, icon: AlertTriangle, color: 'red' },
  ]

  // Color maps — hard-coded so Tailwind can detect classes at build time
  const colorClasses = {
    cyan: 'bg-cyan-100 dark:bg-cyan-950 text-cyan-700 dark:text-cyan-400',
    blue: 'bg-blue-100 dark:bg-blue-950 text-blue-700 dark:text-blue-400',
    green: 'bg-green-100 dark:bg-green-950 text-green-700 dark:text-green-400',
    red: 'bg-red-100 dark:bg-red-950 text-red-700 dark:text-red-400',
  }

  return (
    <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 mb-8">
      {stats.map((stat) => {
        const Icon = stat.icon
        return (
          <div
            key={stat.label}
            className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl p-4 shadow-sm"
          >
            <div className="flex items-center gap-3">
              <div className={`p-2 rounded-lg ${colorClasses[stat.color]}`}>
                <Icon className="w-5 h-5" />
              </div>
              <div>
                <p className="text-xs text-slate-500 dark:text-slate-400">{stat.label}</p>
                <p className="text-2xl font-bold">{stat.value}</p>
              </div>
            </div>
          </div>
        )
      })}
    </div>
  )
}
