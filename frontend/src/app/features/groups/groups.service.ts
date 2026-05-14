import { Injectable, inject } from '@angular/core';
import { Observable, from, map } from 'rxjs';
import { ManagementGroups } from '../../api/ManagementGroups';

@Injectable({ providedIn: 'root' })
export class GroupsService {
  private readonly mgClient = inject(ManagementGroups);

  deleteGroup(groupId: string): Observable<void> {
    return from(this.mgClient.deleteManagementGroup(groupId)).pipe(map(() => void 0));
  }
}
