import { Component, ChangeDetectionStrategy, computed, input } from '@angular/core';

function fnv1a32(input: string): number {
  let hash = 0x811c9dc5;
  for (let i = 0; i < input.length; i++) {
    hash ^= input.charCodeAt(i);
    hash = (hash * 0x01000193) >>> 0;
  }
  return hash >>> 0;
}

const BUCKET_MAP: Record<number, 'primary' | 'secondary' | 'tertiary'> = {
  0: 'primary',
  1: 'secondary',
  2: 'tertiary',
};

const SIZE_CONFIG: Record<
  'sm' | 'md' | 'lg',
  { dimension: string; fontSize: string; borderRadius: string }
> = {
  sm: { dimension: '2.125rem', fontSize: 'var(--font-size-sm)', borderRadius: '0.5625rem' },
  md: { dimension: '2.75rem',  fontSize: 'var(--font-size-base)', borderRadius: '0.6875rem' },
  lg: { dimension: '3.875rem', fontSize: 'var(--font-size-2xl)', borderRadius: '0.9375rem' },
};

@Component({
  selector: 'tb-avatar',
  standalone: true,
  imports: [],
  templateUrl: './avatar.component.html',
  styleUrl: './avatar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AvatarComponent {
  readonly id = input.required<string>();
  readonly initials = input.required<string>();
  readonly size = input<'sm' | 'md' | 'lg'>('md');
  readonly colorOverride = input<'primary' | 'secondary' | 'tertiary' | undefined>(undefined);

  readonly resolvedColor = computed<'primary' | 'secondary' | 'tertiary'>(() => {
    const override = this.colorOverride();
    if (override) {
      return override;
    }
    const bucket = fnv1a32(this.id()) % 3;
    return BUCKET_MAP[bucket];
  });

  readonly hostStyles = computed(() => {
    const cfg = SIZE_CONFIG[this.size()];
    const color = this.resolvedColor();
    return {
      '--avatar-size': cfg.dimension,
      '--avatar-font-size': cfg.fontSize,
      '--avatar-radius': cfg.borderRadius,
      '--avatar-accent': `var(--color-accent-${color})`,
    };
  });

  readonly ariaLabel = computed(() => `${this.initials()} avatar`);
}
