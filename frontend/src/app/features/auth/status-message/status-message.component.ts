import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'tb-status-message',
  standalone: true,
  templateUrl: './status-message.component.html',
  styleUrl: './status-message.component.scss',
})
export class StatusMessageComponent {
  readonly tone = input.required<'error' | 'success'>();
  readonly message = input.required<string>();

  readonly role = computed(() => (this.tone() === 'error' ? 'alert' : 'status'));
}
