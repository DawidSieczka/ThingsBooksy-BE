import { CdkDragHandle } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TypePillComponent, FieldType } from '../../../shared/components/type-pill/type-pill.component';
import { FieldDataType, FieldDraft } from '../types';

@Component({
  selector: 'tb-field-row',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CdkDragHandle, TypePillComponent],
  templateUrl: './field-row.component.html',
  styleUrl: './field-row.component.scss',
})
export class FieldRowComponent {
  readonly field = input.required<FieldDraft>();
  readonly index = input.required<number>();
  readonly nameError = input<string | null>(null);

  readonly nameChange = output<string>();
  readonly typeChange = output<FieldDataType>();
  readonly requiredChange = output<boolean>();
  readonly remove = output<void>();

  get pillValue(): FieldType {
    switch (this.field().dataType) {
      case 'Text':
        return 'text';
      case 'Number':
        return 'number';
      case 'Boolean':
        return 'boolean';
    }
  }

  onPillChange(next: FieldType): void {
    const mapped: FieldDataType =
      next === 'text' ? 'Text' : next === 'number' ? 'Number' : 'Boolean';
    this.typeChange.emit(mapped);
  }

  onNameInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.nameChange.emit(value);
  }

  onRequiredToggle(event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.requiredChange.emit(checked);
  }
}
