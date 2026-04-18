import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import './index.css'
import App from './App.jsx'

// QueryClient — the "brain" of TanStack Query.
// Holds the cache, handles refetching, retries, etc.
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30 * 1000,      // Data is "fresh" for 30 seconds — no refetch during this time
      retry: 1,                   // Retry failed requests once (default is 3)
      refetchOnWindowFocus: false, // Don't auto-refetch when user switches tabs
    },
  },
})

createRoot(document.getElementById('root')).render(
  <StrictMode>
    {/* QueryClientProvider makes the client available everywhere */}
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </StrictMode>,
)
