import * as React from 'react'
import { createFileRoute } from '@tanstack/react-router'
import type { ActionItem } from '../types'
import { useGetWidgetCards, useExecuteWidgetAction } from '../api/generated/endpoints'
import { CardRenderer } from '../components/CardRenderer'
import { BottomSheetModal } from '../components/BottomSheetModal'
import { Button } from '../components/ui/button'

export const Route = createFileRoute('/')({
  component: DashboardComponent,
})

function DashboardComponent() {
  const {
    data: response,
    isLoading: loading,
    error,
    refetch,
    dataUpdatedAt,
  } = useGetWidgetCards({
    query: {
      refetchInterval: 15000,
    },
  })

  const snapshots = response?.data ?? []

  const { mutateAsync: executeAction } = useExecuteWidgetAction()

  // Action modal state
  const [selectedAction, setSelectedAction] = React.useState<ActionItem | null>(null)
  const [isModalOpen, setIsModalOpen] = React.useState<boolean>(false)

  const handleActionClick = (action: ActionItem) => {
    setSelectedAction(action)
    setIsModalOpen(true)
  }

  const handleActionConfirm = async (action: ActionItem, inputVal?: string) => {
    try {
      await executeAction({
        data: {
          actionId: action.id ?? '',
          label: action.label ?? null,
          parameter: inputVal ?? null,
        },
      })
      // Refresh cards right after executing
      refetch()
    } catch (err) {
      console.error('Action execution failed:', err)
    }
  }

  return (
    <div className="flex flex-col gap-6">
      {/* Top bar with stats & manual refresh */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 border rounded-lg p-4 bg-card">
        <div>
          <h2 className="text-lg font-semibold tracking-tight">
            Live Infrastructure Grid
          </h2>
          <p className="text-xs text-muted-foreground mt-0.5">
            Auto-refreshing snapshots via TanStack Query.
          </p>
        </div>

        <div className="flex items-center gap-3 w-full sm:w-auto">
          {dataUpdatedAt > 0 && (
            <span className="text-xs font-mono text-muted-foreground hidden md:block">
              Updated: {new Date(dataUpdatedAt).toLocaleTimeString()}
            </span>
          )}

          <Button
            variant="outline"
            size="sm"
            onClick={() => refetch()}
            disabled={loading && snapshots.length === 0}
            className="w-full sm:w-auto"
          >
            {loading && snapshots.length === 0 ? 'Refreshing...' : 'Refresh Grid'}
          </Button>
        </div>
      </div>

      {/* Error state */}
      {error ? (() => {
        const errorMessage = error instanceof Error ? error.message : typeof error === 'string' ? error : 'Unknown error';
        return (
          <div className="p-4 border border-destructive/50 rounded-lg bg-destructive/10 text-destructive flex flex-col sm:flex-row items-center justify-between gap-4">
            <div className="flex items-center gap-3">
              <div>
                <h4 className="font-semibold text-sm">Connection to .NET Backend Failed</h4>
                <p className="text-xs mt-0.5 font-mono">{errorMessage}</p>
              </div>
            </div>
            <Button
              variant="destructive"
              size="sm"
              onClick={() => refetch()}
            >
              Retry Now
            </Button>
          </div>
        );
      })() : null}

      {/* Loading state skeleton */}
      {loading && snapshots.length === 0 && !error && (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
          {[1, 2, 3].map((n) => (
            <div
              key={n}
              className="h-64 rounded-lg border bg-muted/40 p-6 flex flex-col justify-between animate-pulse"
            >
              <div className="flex flex-col gap-3">
                <div className="w-1/3 h-5 bg-muted rounded" />
                <div className="w-2/3 h-4 bg-muted rounded" />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div className="h-16 bg-muted rounded" />
                <div className="h-16 bg-muted rounded" />
              </div>
              <div className="w-1/4 h-8 bg-muted rounded" />
            </div>
          ))}
        </div>
      )}

      {/* Empty state */}
      {!loading && snapshots.length === 0 && !error && (
        <div className="p-12 text-center border rounded-lg bg-card flex flex-col items-center justify-center max-w-xl mx-auto my-8">
          <h3 className="text-base font-semibold">No Active Widgets Registered</h3>
          <p className="text-xs text-muted-foreground max-w-sm mt-1 leading-relaxed">
            Your ASP.NET Core backend responded, but no integrations currently expose an IWidget. Ensure Jellyfin, Prometheus, or qBittorrent options are enabled in appsettings.json.
          </p>
        </div>
      )}

      {/* Cards Grid */}
      {snapshots.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
          {snapshots.map((snapshot) => (
            <CardRenderer
              key={snapshot.widgetId}
              card={snapshot.card}
              onActionClick={handleActionClick}
            />
          ))}
        </div>
      )}

      {/* Action Dialog Modal */}
      <BottomSheetModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        action={selectedAction}
        onConfirm={handleActionConfirm}
      />
    </div>
  )
}
