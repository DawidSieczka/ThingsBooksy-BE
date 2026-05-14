import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { firstValueFrom } from 'rxjs';
import { GroupContextStore } from '../group-context.store';
import { GroupsService } from '../groups.service';
import { AnimatedBackgroundComponent } from '../../../shared/components/animated-background/animated-background.component';
import { CreateOrEditGroupModalComponent } from '../../dashboard/create-group-modal/create-group-modal.component';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';
import { NotificationService } from '../../../shared/services/notification.service';
import { GroupHeaderPanelComponent } from '../group-header-panel/group-header-panel.component';
import { SchemasPanelComponent } from '../schemas-panel/schemas-panel.component';
import { ResourcesPanelComponent } from '../resources-panel/resources-panel.component';
import { MembersPanelComponent } from '../members-panel/members-panel.component';

@Component({
  selector: 'tb-group-detail-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [GroupContextStore],
  imports: [
    RouterModule,
    AnimatedBackgroundComponent,
    CreateOrEditGroupModalComponent,
    GroupHeaderPanelComponent,
    SchemasPanelComponent,
    ResourcesPanelComponent,
    MembersPanelComponent,
  ],
  templateUrl: './group-detail-page.component.html',
  styleUrl: './group-detail-page.component.scss',
})
export class GroupDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly store = inject(GroupContextStore);
  private readonly groupsService = inject(GroupsService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly notifications = inject(NotificationService);

  readonly editOpen = signal(false);
  readonly deleteLoading = signal(false);

  private readonly paramMap = toSignal(this.route.paramMap);
  readonly groupId = computed(() => this.paramMap()?.get('groupId') ?? null);

  readonly groupName = computed(() => this.store.group()?.name ?? '');
  readonly initialLoading = this.store.initialLoading;
  readonly initialError = this.store.initialError;
  readonly group = this.store.group;
  readonly schemas = this.store.schemas;
  readonly resources = this.store.resources;
  readonly members = this.store.members;

  constructor() {
    effect(() => {
      const id = this.groupId();
      if (!id) {
        void this.router.navigate(['/dashboard']);
        return;
      }
      void this.store.loadGroup(id);
    });
  }

  ngOnInit(): void {
    // Intentionally empty — data loading is driven by the effect() in constructor.
  }

  onEdit(): void {
    this.editOpen.set(true);
  }

  onEditModalClose(): void {
    this.editOpen.set(false);
  }

  onEditSubmitted(): void {
    const id = this.groupId();
    if (id) {
      void this.store.loadGroup(id);
    }
    this.notifications.success('Group updated');
    this.editOpen.set(false);
  }

  async onDelete(): Promise<void> {
    const id = this.groupId();
    if (!id) return;

    const schemaCount = this.schemas().length;
    const resourceCount = this.resources().items.length;

    const confirmed = await this.confirmDialog.confirm({
      title: 'Delete group',
      message: `Are you sure you want to delete this group? It contains ${schemaCount} schema(s) and ${resourceCount} resource(s). This action cannot be undone.`,
      confirmLabel: 'Delete',
      cancelLabel: 'Cancel',
      danger: true,
    });

    if (!confirmed) return;

    this.deleteLoading.set(true);
    try {
      await firstValueFrom(this.groupsService.deleteGroup(id));
      this.notifications.success('Group deleted');
      void this.router.navigate(['/dashboard']);
    } catch {
      this.notifications.error('Failed to delete group. Please try again.');
    } finally {
      this.deleteLoading.set(false);
    }
  }

  onAddSchema(): void {
    const id = this.groupId();
    if (id) {
      void this.router.navigate(['/groups', id, 'schemas', 'new']);
    }
  }

  onSelectSchema(schemaId: string): void {
    const id = this.groupId();
    if (id) {
      void this.router.navigate(['/groups', id, 'schemas', schemaId]);
    }
  }

  onAddResource(): void {
    // US3 (T049) will wire this to the create-resource modal.
  }

  onRetry(): void {
    const id = this.groupId();
    if (id) {
      void this.store.loadGroup(id);
    }
  }
}
