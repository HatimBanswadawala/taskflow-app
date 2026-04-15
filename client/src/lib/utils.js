import { clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

/**
 * Combines class names intelligently.
 * Example: cn('px-4 py-2', isActive && 'bg-blue-500', className)
 */
export function cn(...inputs) {
  return twMerge(clsx(inputs))
}
