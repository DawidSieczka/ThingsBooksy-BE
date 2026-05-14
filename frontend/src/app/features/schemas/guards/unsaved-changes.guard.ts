import { inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';
import { SchemaDesignerPageComponent } from '../schema-designer-page/schema-designer-page.component';

export const unsavedChangesGuard: CanDeactivateFn<SchemaDesignerPageComponent> = component => {
  if (!component.dirty()) {
    return true;
  }
  const confirmDialog = inject(ConfirmDialogService);
  return confirmDialog.confirm({
    title: 'Discard unsaved changes?',
    message: 'You have unsaved changes that will be lost if you leave now.',
    confirmLabel: 'Discard',
    cancelLabel: 'Stay',
    danger: true,
  });
};
