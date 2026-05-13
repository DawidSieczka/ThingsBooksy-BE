import { ComponentFixture, TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { StatusMessageComponent } from './status-message.component';

describe('StatusMessageComponent', () => {
  let fixture: ComponentFixture<StatusMessageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatusMessageComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(StatusMessageComponent);
    fixture.componentRef.setInput('tone', 'error');
    fixture.componentRef.setInput('message', 'Boom');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });
});
