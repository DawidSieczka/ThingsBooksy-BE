import { Component, DestroyRef, ElementRef, OnInit, computed, inject, input, signal } from '@angular/core';

@Component({
  selector: 'tb-animated-counter',
  standalone: true,
  templateUrl: './animated-counter.component.html',
  styleUrl: './animated-counter.component.scss',
})
export class AnimatedCounterComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly host = inject(ElementRef<HTMLElement>);

  readonly target = input.required<number>();
  readonly suffix = input<string>('');
  readonly durationMs = input<number>(1400);
  readonly delayMs = input<number>(600);

  private readonly current = signal(0);

  readonly display = computed(() => `${this.current().toLocaleString('en-US')}${this.suffix()}`);

  ngOnInit(): void {
    if (this.prefersReducedMotion()) {
      this.current.set(this.target());
      return;
    }

    let frameId = 0;
    let start: number | null = null;
    const target = this.target();
    const duration = this.durationMs();

    const tick = (ts: number): void => {
      if (start === null) start = ts;
      const progress = Math.min((ts - start) / duration, 1);
      const eased = 1 - Math.pow(1 - progress, 3);
      this.current.set(Math.round(eased * target));
      if (progress < 1) {
        frameId = requestAnimationFrame(tick);
      }
    };

    const delayId = window.setTimeout(() => {
      frameId = requestAnimationFrame(tick);
    }, this.delayMs());

    this.destroyRef.onDestroy(() => {
      window.clearTimeout(delayId);
      cancelAnimationFrame(frameId);
    });
  }

  private prefersReducedMotion(): boolean {
    return typeof window !== 'undefined'
      && typeof window.matchMedia === 'function'
      && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  }
}
