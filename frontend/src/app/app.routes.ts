import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () =>
      import('./features/auth/auth.routes').then(m => m.authRoutes),
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/dashboard/dashboard.routes').then(m => m.dashboardRoutes),
  },
  {
    path: 'groups',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/groups/groups.routes').then(m => m.groupsRoutes),
  },
];
