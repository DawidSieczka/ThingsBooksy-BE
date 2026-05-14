import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

export interface CreateGroupPayload {
  name: string;
  description: string | null;
}

export interface UpdateGroupPayload {
  id: string;
  name: string;
  description: string | null;
}

export interface GroupResult {
  id: string;
  name: string;
  description: string | null;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);

  isGroupNameAvailable(name: string): Observable<boolean> {
    return this.http
      .get<{ available?: boolean }>('/management-groups/name-available', {
        params: { name },
        headers: { 'x-silent-errors': 'true' },
      })
      .pipe(map(res => res.available ?? false));
  }

  createGroup(payload: CreateGroupPayload): Observable<GroupResult> {
    return this.http
      .post<{ id: string }>('/management-groups', {
        name: payload.name,
        description: payload.description,
      })
      .pipe(
        map(response => ({
          id: response.id,
          name: payload.name,
          description: payload.description,
        })),
      );
  }

  updateGroup(payload: UpdateGroupPayload): Observable<GroupResult> {
    return this.http
      .put<void>(`/management-groups/${payload.id}`, {
        name: payload.name,
        description: payload.description,
      })
      .pipe(
        map(() => ({
          id: payload.id,
          name: payload.name,
          description: payload.description,
        })),
      );
  }
}
