import {
  CdkDrag,
  CdkDragDrop,
  CdkDropList,
  moveItemInArray,
} from '@angular/cdk/drag-drop';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
} from '@angular/core';
import { FieldRowComponent } from '../field-row/field-row.component';
import { FieldDataType, FieldDraft, createEmptyField } from '../types';

@Component({
  selector: 'tb-schema-form-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CdkDropList, CdkDrag, FieldRowComponent],
  templateUrl: './schema-form-panel.component.html',
  styleUrl: './schema-form-panel.component.scss',
})
export class SchemaFormPanelComponent {
  readonly name = input.required<string>();
  readonly description = input.required<string | null>();
  readonly fields = input.required<readonly FieldDraft[]>();
  readonly nameError = input<string | null>(null);

  readonly nameChange = output<string>();
  readonly descriptionChange = output<string | null>();
  readonly fieldsChange = output<FieldDraft[]>();

  readonly fieldNameErrors = computed<Record<string, string>>(() => {
    const errors: Record<string, string> = {};
    const seen = new Map<string, string>();
    for (const f of this.fields()) {
      const key = (f.name ?? '').trim().toLowerCase();
      if (!key) continue;
      if (seen.has(key)) {
        errors[f.id] = 'Duplicate field name';
        errors[seen.get(key)!] = 'Duplicate field name';
      } else {
        seen.set(key, f.id);
      }
    }
    return errors;
  });

  onNameInput(event: Event): void {
    this.nameChange.emit((event.target as HTMLInputElement).value);
  }

  onDescriptionInput(event: Event): void {
    const value = (event.target as HTMLTextAreaElement).value;
    this.descriptionChange.emit(value.length === 0 ? null : value);
  }

  onAddField(): void {
    this.fieldsChange.emit([...this.fields(), createEmptyField()]);
  }

  onDrop(event: CdkDragDrop<FieldDraft[]>): void {
    const next = [...this.fields()];
    moveItemInArray(next, event.previousIndex, event.currentIndex);
    this.fieldsChange.emit(next);
  }

  patchField(id: string, patch: Partial<FieldDraft>): void {
    this.fieldsChange.emit(
      this.fields().map(f => (f.id === id ? { ...f, ...patch } : f)),
    );
  }

  onFieldName(id: string, name: string): void {
    this.patchField(id, { name });
  }

  onFieldType(id: string, dataType: FieldDataType): void {
    this.patchField(id, { dataType });
  }

  onFieldRequired(id: string, isRequired: boolean): void {
    this.patchField(id, { isRequired });
  }

  onFieldRemove(id: string): void {
    this.fieldsChange.emit(this.fields().filter(f => f.id !== id));
  }
}
