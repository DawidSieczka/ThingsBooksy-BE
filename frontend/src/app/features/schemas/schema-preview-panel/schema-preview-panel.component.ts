import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FieldDraft } from '../types';

@Component({
  selector: 'tb-schema-preview-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [],
  templateUrl: './schema-preview-panel.component.html',
  styleUrl: './schema-preview-panel.component.scss',
})
export class SchemaPreviewPanelComponent {
  readonly name = input.required<string>();
  readonly description = input.required<string | null>();
  readonly fields = input.required<readonly FieldDraft[]>();
}
