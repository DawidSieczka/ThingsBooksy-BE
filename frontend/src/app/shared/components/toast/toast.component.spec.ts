import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { ToastComponent } from './toast.component';
import { NotificationService } from '../../services/notification.service';

describe('ToastComponent', () => {
  let component: ToastComponent;
  let fixture: ComponentFixture<ToastComponent>;
  let notificationService: NotificationService;

  beforeEach(async () => {
    vi.useFakeTimers();

    await TestBed.configureTestingModule({
      imports: [ToastComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ToastComponent);
    component = fixture.componentInstance;
    notificationService = TestBed.inject(NotificationService);
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render one toast when NotificationService.success is called', () => {
    notificationService.success('hello');
    fixture.detectChanges();

    const toastEls = fixture.nativeElement.querySelectorAll('.toast');
    expect(toastEls.length).toBe(1);
    expect(toastEls[0].textContent).toContain('hello');
  });

  it('should apply the correct kind modifier class for success', () => {
    notificationService.success('all good');
    fixture.detectChanges();

    const toastEl = fixture.nativeElement.querySelector('.toast');
    expect(toastEl.classList.contains('toast--success')).toBe(true);
  });

  it('should apply the correct kind modifier class for error', () => {
    notificationService.error('something failed');
    fixture.detectChanges();

    const toastEl = fixture.nativeElement.querySelector('.toast');
    expect(toastEl.classList.contains('toast--error')).toBe(true);
  });

  it('should apply the correct kind modifier class for info', () => {
    notificationService.info('heads up');
    fixture.detectChanges();

    const toastEl = fixture.nativeElement.querySelector('.toast');
    expect(toastEl.classList.contains('toast--info')).toBe(true);
  });

  it('should remove the toast from the DOM when the close button is clicked', () => {
    notificationService.success('click to remove');
    fixture.detectChanges();

    const closeBtn = fixture.nativeElement.querySelector('.toast__close');
    expect(closeBtn).not.toBeNull();

    closeBtn.click();
    fixture.detectChanges();

    const toastEls = fixture.nativeElement.querySelectorAll('.toast');
    expect(toastEls.length).toBe(0);
  });

  it('should reflect signal updates when multiple toasts are added', () => {
    notificationService.success('first');
    notificationService.error('second');
    fixture.detectChanges();

    const toastEls = fixture.nativeElement.querySelectorAll('.toast');
    expect(toastEls.length).toBe(2);
  });

  it('should reflect signal updates when a toast is dismissed', () => {
    const id = notificationService.info('to dismiss');
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.toast').length).toBe(1);

    notificationService.dismiss(id);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.toast').length).toBe(0);
  });

  it('should set aria-label with kind and message on each toast', () => {
    notificationService.success('aria test');
    fixture.detectChanges();

    const toastEl = fixture.nativeElement.querySelector('[role="status"]');
    expect(toastEl.getAttribute('aria-label')).toBe('Success: aria test');
  });

  it('should have aria-label "Dismiss notification" on the close button', () => {
    notificationService.info('check aria');
    fixture.detectChanges();

    const closeBtn = fixture.nativeElement.querySelector('.toast__close');
    expect(closeBtn.getAttribute('aria-label')).toBe('Dismiss notification');
  });
});
