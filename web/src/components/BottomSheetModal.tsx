import * as React from 'react'
import type { ActionItem } from '../types'
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from './ui/dialog'
import { Input } from './ui/input'
import { Button } from './ui/button'

interface BottomSheetModalProps {
  isOpen: boolean
  onClose: () => void
  action?: ActionItem | null
  onConfirm?: (action: ActionItem, inputVal?: string) => void
}

export function BottomSheetModal({
  isOpen,
  onClose,
  action,
  onConfirm,
}: BottomSheetModalProps) {
  const [inputValue, setInputValue] = React.useState('')

  React.useEffect(() => {
    if (isOpen) {
      setInputValue('')
    }
  }, [isOpen, action])

  if (!action) return null

  return (
    <Dialog open={isOpen} onOpenChange={(open) => { if (!open) onClose() }}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Execute Action</DialogTitle>
          <DialogDescription>
            Provide any optional parameters for action: <span className="font-semibold text-foreground">{action.label}</span>
          </DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-4 py-2">
          <div className="flex flex-col gap-2">
            <label className="text-xs font-medium text-muted-foreground">
              Parameter / Note (Optional)
            </label>
            <Input
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              placeholder="Enter value..."
            />
          </div>
        </div>

        <DialogFooter className="gap-2 sm:gap-0">
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button
            onClick={() => {
              if (onConfirm) onConfirm(action, inputValue)
              onClose()
            }}
          >
            Confirm & Run
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
