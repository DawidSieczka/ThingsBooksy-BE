import { Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

let nextId = 0;

@Component({
  selector: 'tb-agree-checkbox',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './agree-checkbox.component.html',
  styleUrl: './agree-checkbox.component.scss',
})
export class AgreeCheckboxComponent {
  readonly control = input.required<FormControl<boolean>>();
  readonly termsHref = input<string>('#');
  readonly privacyHref = input<string>('#');
  readonly inputId = `tb-agree-${nextId++}`;
}
