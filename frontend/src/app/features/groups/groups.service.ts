import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GroupsApiService } from './services/groups-api.service';

@Injectable({ providedIn: 'root' })
export class GroupsService {
  private readonly api = inject(GroupsApiService);

  deleteGroup(groupId: string): Observable<void> {
    return this.api.deleteGroup(groupId);
  }
}
