import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideAnimations } from '@angular/platform-browser/animations';
import { By } from '@angular/platform-browser';
import { describe, it, expect, beforeEach } from 'vitest';
import { UserMenuComponent } from './user-menu.component';

describe('UserMenuComponent', () => {
  let component: UserMenuComponent;
  let fixture: ComponentFixture<UserMenuComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserMenuComponent],
      providers: [provideAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(UserMenuComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('name', 'John Doe');
    fixture.componentRef.setInput('initials', 'JD');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render initials in the avatar', () => {
    const avatar = fixture.debugElement.query(By.css('.user-avatar'));
    expect(avatar.nativeElement.textContent.trim()).toBe('JD');
  });

  it('should render the user name', () => {
    const name = fixture.debugElement.query(By.css('.user-name'));
    expect(name.nativeElement.textContent.trim()).toBe('John Doe');
  });

  it('should start with dropdown closed', () => {
    expect(component.isOpen()).toBe(false);
    const dropdown = fixture.debugElement.query(By.css('.user-dropdown'));
    expect(dropdown).toBeNull();
  });

  it('should open dropdown when button is clicked', () => {
    const btn = fixture.debugElement.query(By.css('.user-btn'));
    btn.nativeElement.click();
    fixture.detectChanges();

    expect(component.isOpen()).toBe(true);
    const dropdown = fixture.debugElement.query(By.css('.user-dropdown'));
    expect(dropdown).not.toBeNull();
  });

  it('should close dropdown on second button click (toggle)', () => {
    component.isOpen.set(true);
    fixture.detectChanges();

    const btn = fixture.debugElement.query(By.css('.user-btn'));
    btn.nativeElement.click();
    fixture.detectChanges();

    expect(component.isOpen()).toBe(false);
  });

  it('should set aria-expanded to true when open', () => {
    component.isOpen.set(true);
    fixture.detectChanges();

    const btn = fixture.debugElement.query(By.css('.user-btn'));
    expect(btn.nativeElement.getAttribute('aria-expanded')).toBe('true');
  });

  it('should close dropdown on ESC key', () => {
    component.isOpen.set(true);
    fixture.detectChanges();

    component.onEscKey();
    fixture.detectChanges();

    expect(component.isOpen()).toBe(false);
  });

  it('should emit settings output and close dropdown when settings item clicked', () => {
    component.isOpen.set(true);
    fixture.detectChanges();

    let emitted = false;
    component.settings.subscribe(() => (emitted = true));

    component.onSettings();
    fixture.detectChanges();

    expect(emitted).toBe(true);
    expect(component.isOpen()).toBe(false);
  });

  it('should emit logout output and close dropdown when logout item clicked', () => {
    component.isOpen.set(true);
    fixture.detectChanges();

    let emitted = false;
    component.logout.subscribe(() => (emitted = true));

    component.onLogout();
    fixture.detectChanges();

    expect(emitted).toBe(true);
    expect(component.isOpen()).toBe(false);
  });

  it('should close dropdown when clicking outside the host element', () => {
    component.isOpen.set(true);
    fixture.detectChanges();

    const outsideElement = document.createElement('div');
    document.body.appendChild(outsideElement);

    const event = new MouseEvent('click', { bubbles: true });
    Object.defineProperty(event, 'target', { value: outsideElement });
    component.onDocumentClick(event);
    fixture.detectChanges();

    expect(component.isOpen()).toBe(false);

    document.body.removeChild(outsideElement);
  });
});
