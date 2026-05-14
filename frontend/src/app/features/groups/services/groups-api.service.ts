import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import {
  ThingsBooksyModulesManagementGroupsCoreFeaturesGetGroupMembersGetGroupMembersQueryResult as MembersPage,
  ThingsBooksyModulesManagementGroupsCoreFeaturesGetManagementGroupGetManagementGroupQueryResult as GroupDetail,
} from '../../../api/data-contracts';

export type GroupDetailDto = GroupDetail;
export type GroupMembersPageDto = MembersPage;

export interface CursorParams {
  afterId?: string | null;
  take?: number | null;
}

export interface GroupListItemDto {
  readonly id: string;
  readonly name: string;
  readonly description: string | null;
  readonly ownerId: string;
  readonly createdAt: string;
  readonly memberCount: number;
}

@Injectable({ providedIn: 'root' })
export class GroupsApiService {
  private readonly http = inject(HttpClient);

  getMyGroups(): Observable<GroupListItemDto[]> {
    return this.http.get<GroupListItemDto[]>('/management-groups');
  }

  getGroup(id: string): Observable<GroupDetailDto> {
    return this.http.get<GroupDetailDto>(`/management-groups/${id}`);
  }

  getGroupMembers(id: string, params: CursorParams = {}): Observable<GroupMembersPageDto> {
    const query: Record<string, string> = {};
    if (params.afterId) query['afterId'] = params.afterId;
    if (params.take != null) query['take'] = String(params.take);
    return this.http.get<GroupMembersPageDto>(`/management-groups/${id}/members`, {
      params: query,
    });
  }

  deleteGroup(id: string): Observable<void> {
    return this.http.delete<void>(`/management-groups/${id}`).pipe(map(() => void 0));
  }

  isNameAvailable(name: string): Observable<boolean> {
    return this.http
      .get<{ available?: boolean }>('/management-groups/name-available', {
        params: { name },
        headers: { 'x-silent-errors': 'true' },
      })
      .pipe(map(res => res.available ?? false));
  }
}
