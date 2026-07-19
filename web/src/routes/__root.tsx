import * as React from 'react'
import { Link, Outlet, createRootRoute } from '@tanstack/react-router'
import { Badge } from '../components/ui/badge'

export const Route = createRootRoute({
  component: RootComponent,
})

function RootComponent() {
  return (
    <div className="min-h-screen bg-background text-foreground flex flex-col font-sans">
      <header className="border-b border-border bg-card px-6 py-4 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div>
            <h1 className="font-bold text-base flex items-center gap-2">
              homebot
              <Badge variant="secondary" className="text-xs font-mono">
                v2.0
              </Badge>
            </h1>
            <p className="text-xs text-muted-foreground hidden sm:block">
              Infrastructure Control Room
            </p>
          </div>
        </div>

        <nav className="flex items-center gap-4 text-sm font-medium">
          <Link
            to="/"
            activeProps={{
              className: 'text-foreground font-semibold underline underline-offset-4',
            }}
            activeOptions={{ exact: true }}
            className="text-muted-foreground hover:text-foreground transition-colors"
          >
            Dashboard
          </Link>
          <Link
            to="/about"
            activeProps={{
              className: 'text-foreground font-semibold underline underline-offset-4',
            }}
            className="text-muted-foreground hover:text-foreground transition-colors"
          >
            About
          </Link>

          <div className="h-4 w-[1px] bg-border hidden sm:block" />

          <Badge variant="outline" className="hidden sm:inline-flex gap-1.5 font-mono text-xs">
            <span className="w-1.5 h-1.5 rounded-full bg-emerald-500" />
            Connected
          </Badge>
        </nav>
      </header>

      <main className="flex-1 max-w-6xl w-full mx-auto p-6">
        <Outlet />
      </main>

      <footer className="py-4 px-6 border-t border-border text-center text-xs text-muted-foreground font-mono flex flex-col sm:flex-row items-center justify-between gap-2 max-w-6xl w-full mx-auto">
        <span>Powered by .NET 10 & TanStack Router</span>
        <span>Infrastructure Operations</span>
      </footer>
    </div>
  )
}
