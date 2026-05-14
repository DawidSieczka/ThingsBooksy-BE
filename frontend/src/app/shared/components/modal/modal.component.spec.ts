import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { Component } from '@angular/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ModalComponent } from './modal.component';

// Host component to drive signal inputs
@Component({
  standalone: true,
  imports: [ModalComponent],
  template: `
    <tb-modal [open]="isOpen" [title]="modalTitle" (close)="onClose()">
      <p>Modal content</p>
    </tb-modal>
  `,
})
class TestHostComponent {
  isOpen = false;
  modalTitle = 'Test Title';
  closeCalled = false;
  onClose(): void {
    this.closeCalled = true;
  }
}

describe('ModalComponent', () => {
  let hostFixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent, NoopAnimationsModule],
    }).compileComponents();

    hostFixture = TestBed.createComponent(TestHostComponent);
    host = hostFixture.componentInstance;
    hostFixture.detectChanges();
  });

  it('should create', () => {
    expect(host).toBeTruthy();
  });

  it('should render the modal title when title input is set', () => {
    host.isOpen = true;
    hostFixture.detectChanges();
    const title = hostFixture.nativeElement.querySelector('.modal__title');
    expect(title?.textContent?.trim()).toBe('Test Title');
  });

  it('should not render header when title is empty', async () => {
    host.modalTitle = '';
    host.isOpen = true;
    hostFixture.detectChanges();
    const header = hostFixture.nativeElement.querySelector('.modal__header');
    expect(header).toBeNull();
  });

  it('should emit close when onCancel is called', () => {
    const modalEl = hostFixture.nativeElement.querySelector('tb-modal');
    const componentRef = hostFixture.debugElement.query(
      (el) => el.componentInstance instanceof ModalComponent,
    );
    const modal: ModalComponent = componentRef?.componentInstance;

    if (modal) {
      const event = new Event('cancel');
      const preventDefaultSpy = vi.spyOn(event, 'preventDefault');
      let emitted = false;
      modal.close.subscribe(() => (emitted = true));
      modal.onCancel(event);
      expect(preventDefaultSpy).toHaveBeenCalled();
      expect(emitted).toBe(true);
    }
    expect(modalEl).toBeTruthy();
  });
});
