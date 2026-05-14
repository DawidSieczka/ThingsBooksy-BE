import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
} from '@angular/core';
import { ThingsBooksyModulesManagementGroupsCoreFeaturesGetManagementGroupGetManagementGroupQueryResult as GroupDetailDto } from '../../../api/data-contracts';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';

function getGroupInitials(name: string | null | undefined): string {
  if (!name) {
    return '?';
  }
  const words = name.trim().split(/\s+/);
  if (words.length === 1) {
    return (words[0].slice(0, 2) ?? '').toUpperCase() || '?';
  }
  return ((words[0][0] ?? '') + (words[1][0] ?? '')).toUpperCase() || '?';
}

function formatCreatedAt(dateStr: string | null | undefined): string {
  if (!dateStr) {
    return '';
  }
  return new Intl.DateTimeFormat('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  }).format(new Date(dateStr));
}

@Component({
  selector: 'tb-group-header-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AvatarComponent],
  templateUrl: './group-header-panel.component.html',
  styleUrl: './group-header-panel.component.scss',
})
export class GroupHeaderPanelComponent {
  readonly group = input<GroupDetailDto | null>(null);
  readonly resourceCount = input<number>(0);
  readonly schemaCount = input<number>(0);
  readonly isOwner = input<boolean>(false);

  readonly edit = output<void>();
  readonly delete = output<void>();

  readonly avatarId = computed(() => this.group()?.id ?? 'group');
  readonly avatarInitials = computed(() => getGroupInitials(this.group()?.name));
  readonly groupName = computed(() => this.group()?.name ?? '');
  readonly groupDescription = computed(() => this.group()?.description ?? null);
  readonly createdAtFormatted = computed(() => formatCreatedAt(this.group()?.createdAt));
  readonly memberCount = computed(() => this.group()?.memberCount ?? 0);
  readonly membersLabel = computed(() => {
    const count = this.memberCount();
    return `${count} ${count === 1 ? 'member' : 'members'}`;
  });

  onEdit(): void {
    this.edit.emit();
  }

  onDelete(): void {
    this.delete.emit();
  }
}
