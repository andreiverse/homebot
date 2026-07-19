import * as React from 'react'
import { createFileRoute } from '@tanstack/react-router'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card'

export const Route = createFileRoute('/about')({
  component: AboutComponent,
})

function AboutComponent() {
  return (
    <div className="max-w-3xl mx-auto flex flex-col gap-6">
      <Card>
        <CardHeader>
          <CardTitle className="text-xl">About homebot</CardTitle>
          <CardDescription>
            The unified control room for your infrastructure. Built with .NET 10 & TanStack Router.
          </CardDescription>
        </CardHeader>
        <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="p-4 border rounded-lg bg-muted/40 flex flex-col gap-2">
            <h4 className="text-sm font-semibold">Platform-Agnostic UI Engine</h4>
            <p className="text-xs text-muted-foreground leading-relaxed">
              Your integrations (Jellyfin, Prometheus, qBittorrent) output a universal Card model. Whether rendered as Discord Embeds with native buttons or here as web cards, you write monitoring logic only once.
            </p>
          </div>

          <div className="p-4 border rounded-lg bg-muted/40 flex flex-col gap-2">
            <h4 className="text-sm font-semibold">Interactive Actions</h4>
            <p className="text-xs text-muted-foreground leading-relaxed">
              Actions with parameters open standard modal dialogs on web and mobile devices, keeping your workflow simple and stateless.
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
