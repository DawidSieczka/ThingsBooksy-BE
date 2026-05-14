import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

export type FieldType = 'text' | 'number' | 'boolean';

const CYCLE_ORDER: FieldType[] = ['text', 'number', 'boolean'];

@Component({
  selector: 'tb-type-pill',
  standalone: true,
  imports: [],
  templateUrl: './type-pill.component.html',
  styleUrl: './type-pill.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TypePillComponent {
  readonly value = input.required<FieldType>();
  readonly readonly = input<boolean>(false);

  readonly valueChange = output<FieldType>();

  get label(): string {
    switch (this.value()) {
      case 'text':    return 'Text';
      case 'number':  return 'Number';
      case 'boolean': return 'Yes / No';
    }
  }

  get ariaLabel(): string {
    return `Field type: ${this.label}. Click to change.`;
  }

  onCycle(): void {
    if (this.readonly()) return;
    const current = this.value();
    const idx = CYCLE_ORDER.indexOf(current);
    const next = CYCLE_ORDER[(idx + 1) % CYCLE_ORDER.length];
    this.valueChange.emit(next);
  }
}
