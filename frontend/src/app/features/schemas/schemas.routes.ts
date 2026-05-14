import { Routes } from '@angular/router';
import { unsavedChangesGuard } from './guards/unsaved-changes.guard';

export const schemasRoutes: Routes = [
  {
    path: 'new',
    loadComponent: () =>
      import('./schema-designer-page/schema-designer-page.component').then(
        m => m.SchemaDesignerPageComponent,
      ),
    canDeactivate: [unsavedChangesGuard],
  },
  {
    path: ':schemaId',
    loadComponent: () =>
      import('./schema-designer-page/schema-designer-page.component').then(
        m => m.SchemaDesignerPageComponent,
      ),
    canDeactivate: [unsavedChangesGuard],
  },
];
