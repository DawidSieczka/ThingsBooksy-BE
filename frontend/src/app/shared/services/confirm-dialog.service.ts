import { Injectable, Signal, signal } from '@angular/core';

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  danger?: boolean;
}

export interface ConfirmState extends ConfirmOptions {
  resolve: (value: boolean) => void;
}

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private readonly _state = signal<ConfirmState | null>(null);

  readonly state: Signal<ConfirmState | null> = this._state.asReadonly();

  confirm(options: ConfirmOptions): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      this._state.set({
        title: options.title,
        message: options.message,
        confirmLabel: options.confirmLabel ?? 'Confirm',
        cancelLabel: options.cancelLabel ?? 'Cancel',
        danger: options.danger ?? false,
        resolve,
      });
    });
  }

  onConfirm(): void {
    const current = this._state();
    if (!current) return;
    current.resolve(true);
    this._state.set(null);
  }

  onCancel(): void {
    const current = this._state();
    if (!current) return;
    current.resolve(false);
    this._state.set(null);
  }
}
