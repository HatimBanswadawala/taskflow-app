import { useEffect } from 'react'
import { X } from 'lucide-react'

/**
 * Simple modal dialog.
 * Click outside or press Escape to close.
 */
export default function Modal({ isOpen, onClose, title, children }) {
  // Listen for Escape key to close modal
  useEffect(() => {
    if (!isOpen) return

    const handleEscape = (e) => {
      if (e.key === 'Escape') onClose()
    }
    document.addEventListener('keydown', handleEscape)
    return () => document.removeEventListener('keydown', handleEscape) // cleanup
  }, [isOpen, onClose])

  // Early return — if not open, render nothing
  if (!isOpen) return null

  return (
    // Backdrop — covers whole screen, click to close
    <div
      onClick={onClose}
      className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4"
    >
      {/* Modal content — stopPropagation prevents backdrop click bubbling up */}
      <div
        onClick={(e) => e.stopPropagation()}
        className="bg-white dark:bg-slate-800 rounded-xl shadow-2xl w-full max-w-md p-6 border border-slate-200 dark:border-slate-700 ring-1 ring-slate-900/5"
      >
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold">{title}</h2>
          <button
            onClick={onClose}
            className="text-slate-400 hover:text-slate-600 dark:hover:text-slate-200"
          >
            <X className="w-5 h-5" />
          </button>
        </div>
        {children}
      </div>
    </div>
  )
}
