import { cn } from '@/lib/utils'

/**
 * Reusable Button component with variants.
 * Uses `cn()` to merge default classes with optional custom className.
 */
export default function Button({
  children,
  variant = 'primary',   // default value
  size = 'md',
  className,
  ...rest                // ...rest collects any other props (onClick, type, disabled, etc.)
}) {
  const base = 'inline-flex items-center justify-center gap-2 font-medium rounded-lg disabled:opacity-50 disabled:cursor-not-allowed'

  const variants = {
    primary: 'bg-gradient-to-r from-cyan-500 to-blue-600 text-white hover:opacity-90',
    secondary: 'bg-slate-100 dark:bg-slate-700 hover:bg-slate-200 dark:hover:bg-slate-600',
    danger: 'bg-red-500 text-white hover:bg-red-600',
    ghost: 'hover:bg-slate-100 dark:hover:bg-slate-700',
  }

  const sizes = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2 text-sm',
    lg: 'px-5 py-2.5 text-base',
  }

  return (
    <button
      className={cn(base, variants[variant], sizes[size], className)}
      {...rest}
    >
      {children}
    </button>
  )
}
