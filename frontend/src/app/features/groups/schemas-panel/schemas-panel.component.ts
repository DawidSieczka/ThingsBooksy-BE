import {
  ChangeDetectionStrategy,
  Component,
  InputSignal,
  OutputEmitterRef,
  input,
  output,
} from '@angular/core';
import { SchemaSummary } from '../group-context.store';
import { CountChipComponent } from '../../../shared/components/count-chip/count-chip.component';
import { IconPlusComponent } from '../../../shared/components/icon-plus/icon-plus.component';
import { IconChevronComponent } from '../../../shared/components/icon-chevron/icon-chevron.component';

@Component({
  selector: 'tb-schemas-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CountChipComponent, IconPlusComponent, IconChevronComponent],
  templateUrl: './schemas-panel.component.html',
  styleUrl: './schemas-panel.component.scss',
})
export class SchemasPanelComponent {
  readonly schemas: InputSignal<SchemaSummary[]> = input<SchemaSummary[]>([]);
  readonly isOwner: InputSignal<boolean> = input<boolean>(false);

  readonly addSchema: OutputEmitterRef<void> = output<void>();
  readonly selectSchema: OutputEmitterRef<string> = output<string>();
  readonly addResourceForSchema: OutputEmitterRef<string> = output<string>();
  readonly deleteSchema: OutputEmitterRef<SchemaSummary> = output<SchemaSummary>();
}
