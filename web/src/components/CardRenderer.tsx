import * as React from 'react'
import type { Card, ContentBlock, ActionItem } from '../types'
import { Card as ShadcnCard, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from './ui/card'
import { Badge } from './ui/badge'
import { Button } from './ui/button'
import { Separator } from './ui/separator'

interface CardRendererProps {
  card: Card
  onActionClick?: (action: ActionItem) => void
}

export function CardRenderer({ card, onActionClick }: CardRendererProps) {
  return (
    <ShadcnCard className="flex flex-col justify-between overflow-hidden">
      <div>
        {/* Header */}
        {(card.heading || card.summary) && (
          <CardHeader className="pb-3">
            <div className="flex items-center justify-between gap-2">
              {card.heading && (
                <CardTitle className="text-base font-semibold">
                  {card.heading}
                </CardTitle>
              )}
              {card.link && (
                <a
                  href={card.link}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <Badge variant="outline" className="text-xs font-normal hover:bg-muted">
                    Open →
                  </Badge>
                </a>
              )}
            </div>
            {card.summary && (
              <CardDescription className="text-xs">
                {card.summary}
              </CardDescription>
            )}
          </CardHeader>
        )}

        {/* Content Blocks */}
        <CardContent className="flex flex-col gap-3 pt-0">
          {(card.content ?? []).map((block, idx) => (
            <BlockRenderer key={idx} block={block} />
          ))}
        </CardContent>
      </div>

      {/* Actions / Footer */}
      {(((card.actions ?? []).length > 0) || card.metadata) && (
        <CardFooter className="flex flex-wrap items-center justify-between gap-3 pt-3 border-t bg-muted/20">
          {(card.actions ?? []).length > 0 ? (
            <div className="flex flex-wrap items-center gap-2">
              {(card.actions ?? []).map((action, idx) => (
                <Button
                  key={idx}
                  variant="outline"
                  size="sm"
                  onClick={() => {
                    if (action.url) {
                      window.open(action.url, '_blank')
                    } else if (onActionClick) {
                      onActionClick(action)
                    }
                  }}
                  className="text-xs h-8"
                >
                  {action.label}
                </Button>
              ))}
            </div>
          ) : (
            <div />
          )}

          {card.metadata && (
            <div className="text-[11px] text-muted-foreground font-mono flex items-center gap-3 ml-auto">
              {card.metadata.author && <span>👤 {card.metadata.author}</span>}
              {card.metadata.footer && <span>{card.metadata.footer}</span>}
            </div>
          )}
        </CardFooter>
      )}
    </ShadcnCard>
  )
}

function BlockRenderer({ block }: { block: ContentBlock }) {
  switch (block.type) {
    case 'keyValue':
      return (
        <div className="grid grid-cols-2 sm:grid-cols-3 gap-2 my-1">
          {(block.items ?? []).map((item, idx) => (
            <div
              key={idx}
              className="border rounded-md p-2.5 bg-muted/30 flex flex-col justify-between"
            >
              <span className="text-[11px] font-medium text-muted-foreground uppercase tracking-wider truncate">
                {item.key}
              </span>
              <span className="text-sm font-semibold truncate mt-1">
                {item.value}
              </span>
            </div>
          ))}
        </div>
      )

    case 'text':
      return (
        <div className="flex flex-col gap-1 my-0.5">
          {block.heading && (
            <h4 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
              {block.heading}
            </h4>
          )}
          <p className="text-sm text-foreground/90 leading-relaxed whitespace-pre-wrap">
            {block.text}
          </p>
        </div>
      )

    case 'list':
      return (
        <ul className="flex flex-col gap-1 my-1 pl-4 list-disc text-sm text-foreground/90">
          {(block.items ?? []).map((item, idx) => (
            <li key={idx} className="leading-normal">{item}</li>
          ))}
        </ul>
      )

    case 'code':
      return (
        <div className="my-1 rounded-md bg-muted border p-3 overflow-x-auto">
          {block.language && (
            <div className="text-[10px] font-mono uppercase text-muted-foreground mb-1">
              {block.language}
            </div>
          )}
          <pre className="font-mono text-xs leading-relaxed">
            <code>{block.code}</code>
          </pre>
        </div>
      )

    case 'media':
      return (
        <div className="my-1 rounded-md overflow-hidden border">
          <img
            src={block.source}
            alt={block.description || 'Media'}
            className="w-full h-auto object-cover max-h-64"
          />
          {block.description && (
            <div className="p-2 text-xs text-center text-muted-foreground bg-muted/50 border-t">
              {block.description}
            </div>
          )}
        </div>
      )

    case 'divider':
      return <Separator className="my-2" />

    case 'graph':
      return <MiniSvgGraph graph={block} />

    default:
      return null
  }
}

function MiniSvgGraph({ graph }: { graph: any }) {
  const series = graph.series?.[0]
  if (!series || !series.xs?.length || !series.ys?.length) {
    return (
      <div className="p-4 bg-muted/40 rounded-md border text-center text-xs text-muted-foreground">
        No graph data available for {graph.title}
      </div>
    )
  }

  const xs: number[] = series.xs
  const ys: number[] = series.ys
  const minX = Math.min(...xs)
  const maxX = Math.max(...xs)
  const minY = Math.min(...ys)
  const maxY = Math.max(...ys)

  const width = 400
  const height = 120
  const pad = 12

  const points = xs.map((x, i) => {
    const y = ys[i]
    const px = pad + ((x - minX) / (maxX - minX || 1)) * (width - pad * 2)
    const py = height - pad - ((y - minY) / (maxY - minY || 1)) * (height - pad * 2)
    return `${px.toFixed(1)},${py.toFixed(1)}`
  })

  const pathD = points.length ? `M ${points.join(' L ')}` : ''
  const areaD = points.length
    ? `${pathD} L ${points[points.length - 1].split(',')[0]},${height - pad} L ${pad},${height - pad} Z`
    : ''

  return (
    <div className="my-1 border rounded-md p-3 bg-muted/20">
      <div className="flex items-center justify-between mb-2">
        <span className="text-xs font-semibold">{graph.title}</span>
        <span className="text-[10px] text-muted-foreground font-mono">
          {ys[ys.length - 1]?.toFixed(1)} {graph.yLabel}
        </span>
      </div>
      <div className="w-full overflow-hidden">
        <svg viewBox={`0 0 ${width} ${height}`} className="w-full h-24 overflow-visible">
          {areaD && (
            <path
              d={areaD}
              fill="currentColor"
              className="text-primary/10"
            />
          )}
          {pathD && (
            <path
              d={pathD}
              fill="none"
              stroke="currentColor"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
              className="text-primary"
            />
          )}
        </svg>
      </div>
      <div className="flex items-center justify-between mt-1 text-[10px] font-mono text-muted-foreground">
        <span>{graph.xLabel}</span>
      </div>
    </div>
  )
}
