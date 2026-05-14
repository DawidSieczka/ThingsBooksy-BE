import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  ThingsBooksyModulesManagementGroupsCoreFeaturesGetGroupMembersGroupMemberDto as MemberDto,
  ThingsBooksyModulesManagementGroupsCoreFeaturesGetManagementGroupGetManagementGroupQueryResult as GroupDetailDto,
  ThingsBooksyModulesResourcesCoreFeaturesGetResourceInstancesResourceInstanceRowDto as ResourceRowDto,
} from '../../api/data-contracts';
import { ManagementGroups } from '../../api/ManagementGroups';
import { Resources } from '../../api/Resources';
import { AuthService } from '../auth/auth.service';

export interface SchemaSummary {
  readonly id: string;
  readonly name: string;
  readonly description: string | null;
  readonly instanceCount: number;
}

export interface CursorPage<T> {
  readonly items: T[];
  readonly nextCursor: string | null;
  readonly loading: boolean;
}

const PAGE_SIZE = 20;

@Injectable()
export class GroupContextStore {
  private readonly mgClient = inject(ManagementGroups);
  private readonly resourcesClient = inject(Resources);
  private readonly auth = inject(AuthService);

  private readonly _group = signal<GroupDetailDto | null>(null);
  private readonly _schemas = signal<SchemaSummary[]>([]);
  private readonly _members = signal<CursorPage<MemberDto>>({
    items: [],
    nextCursor: null,
    loading: false,
  });
  private readonly _resources = signal<CursorPage<ResourceRowDto>>({
    items: [],
    nextCursor: null,
    loading: false,
  });
  private readonly _initialLoading = signal(false);
  private readonly _initialError = signal<string | null>(null);

  readonly group = this._group.asReadonly();
  readonly schemas = this._schemas.asReadonly();
  readonly members = this._members.asReadonly();
  readonly resources = this._resources.asReadonly();
  readonly initialLoading = this._initialLoading.asReadonly();
  readonly initialError = this._initialError.asReadonly();

  readonly isOwner = computed(() => {
    const me = this.auth.currentUser();
    const owner = this._group()?.ownerId;
    return !!me && !!owner && me.id === owner;
  });

  async loadGroup(groupId: string): Promise<void> {
    this._initialLoading.set(true);
    this._initialError.set(null);
    try {
      const [group, schemas, members, resources] = await Promise.all([
        firstValueFrom(this.mgClient.getManagementGroup(groupId) as any),
        firstValueFrom(this.resourcesClient.getResourceTypes({ GroupId: groupId } as any) as any),
        firstValueFrom(
          this.mgClient.getGroupMembers(groupId, { take: PAGE_SIZE } as any) as any,
        ),
        firstValueFrom(
          this.resourcesClient.getResourceInstances({
            GroupId: groupId,
            Take: PAGE_SIZE,
          } as any) as any,
        ),
      ]);
      this._group.set(group as GroupDetailDto);
      this._schemas.set(this.mapSchemas((schemas as any[]) ?? []));
      this._members.set({
        items: ((members as any).items ?? []) as MemberDto[],
        nextCursor: (members as any).nextCursor ?? null,
        loading: false,
      });
      this._resources.set({
        items: ((resources as any).items ?? []) as ResourceRowDto[],
        nextCursor: (resources as any).nextCursor ?? null,
        loading: false,
      });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to load group.';
      this._initialError.set(message);
      this._group.set(null);
    } finally {
      this._initialLoading.set(false);
    }
  }

  async loadMoreMembers(groupId: string): Promise<void> {
    const current = this._members();
    if (current.loading || !current.nextCursor) {
      return;
    }
    this._members.set({ ...current, loading: true });
    try {
      const page = (await firstValueFrom(
        this.mgClient.getGroupMembers(groupId, {
          afterId: current.nextCursor,
          take: PAGE_SIZE,
        } as any) as any,
      )) as { items: MemberDto[]; nextCursor: string | null };
      this._members.set({
        items: [...current.items, ...(page.items ?? [])],
        nextCursor: page.nextCursor ?? null,
        loading: false,
      });
    } catch {
      this._members.set({ ...current, loading: false });
    }
  }

  async loadMoreResources(groupId: string): Promise<void> {
    const current = this._resources();
    if (current.loading || !current.nextCursor) {
      return;
    }
    this._resources.set({ ...current, loading: true });
    try {
      const page = (await firstValueFrom(
        this.resourcesClient.getResourceInstances({
          GroupId: groupId,
          AfterId: current.nextCursor,
          Take: PAGE_SIZE,
        } as any) as any,
      )) as { items: ResourceRowDto[]; nextCursor: string | null };
      this._resources.set({
        items: [...current.items, ...(page.items ?? [])],
        nextCursor: page.nextCursor ?? null,
        loading: false,
      });
    } catch {
      this._resources.set({ ...current, loading: false });
    }
  }

  replaceGroup(group: GroupDetailDto): void {
    this._group.set(group);
  }

  removeGroup(): void {
    this._group.set(null);
  }

  private mapSchemas(raw: any[]): SchemaSummary[] {
    return raw.map(r => ({
      id: r.id ?? r.Id,
      name: r.name ?? r.Name ?? '',
      description: r.description ?? r.Description ?? null,
      instanceCount: r.instanceCount ?? r.InstanceCount ?? 0,
    }));
  }
}
