import { Injectable, signal } from '@angular/core';

export type ToastKind = 'success' | 'error' | 'info';

export interface Toast {
  readonly id: string;
  readonly kind: ToastKind;
  readonly message: string;
  readonly createdAt: number;
}

const MAX_TOASTS = 3;
const DEFAULT_TTL_MS = 5000;

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly _toasts = signal<Toast[]>([]);
  readonly toasts = this._toasts.asReadonly();

  success(message: string, ttlMs: number = DEFAULT_TTL_MS): string {
    return this.enqueue('success', message, ttlMs);
  }

  error(message: string, ttlMs: number = DEFAULT_TTL_MS): string {
    return this.enqueue('error', message, ttlMs);
  }

  info(message: string, ttlMs: number = DEFAULT_TTL_MS): string {
    return this.enqueue('info', message, ttlMs);
  }

  dismiss(id: string): void {
    this._toasts.update(items => items.filter(t => t.id !== id));
  }

  private enqueue(kind: ToastKind, message: string, ttlMs: number): string {
    const id = crypto.randomUUID();
    const toast: Toast = { id, kind, message, createdAt: Date.now() };
    this._toasts.update(items => {
      const next = [...items, toast];
      return next.length > MAX_TOASTS ? next.slice(next.length - MAX_TOASTS) : next;
    });
    if (ttlMs > 0) {
      setTimeout(() => this.dismiss(id), ttlMs);
    }
    return id;
  }
}
