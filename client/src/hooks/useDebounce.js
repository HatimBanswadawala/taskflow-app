import { useState, useEffect } from 'react'

/**
 * Custom hook: returns a "delayed" version of a value.
 * Useful for search inputs — wait until user stops typing before triggering expensive operations.
 *
 * Example:
 *   const search = useState('')
 *   const debouncedSearch = useDebounce(search, 300)
 *   useEffect(() => runQuery(debouncedSearch), [debouncedSearch])
 *
 * @param value   - the fast-changing value (e.g., search input text)
 * @param delayMs - milliseconds to wait after the last change
 * @returns the value, but updated only after `delayMs` of inactivity
 */
export function useDebounce(value, delayMs = 300) {
  const [debounced, setDebounced] = useState(value)

  useEffect(() => {
    // Schedule an update for `delayMs` later
    const timer = setTimeout(() => setDebounced(value), delayMs)

    // Cleanup: if `value` changes again before the timer fires, cancel the old one
    return () => clearTimeout(timer)
  }, [value, delayMs])

  return debounced
}
