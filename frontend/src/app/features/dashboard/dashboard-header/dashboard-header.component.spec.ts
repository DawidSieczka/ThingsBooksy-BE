import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideAnimations } from '@angular/platform-browser/animations';
import { describe, it, expect, beforeEach } from 'vitest';
import { DashboardHeaderComponent } from './dashboard-header.component';

describe('DashboardHeaderComponent', () => {
  let component: DashboardHeaderComponent;
  let fixture: ComponentFixture<DashboardHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardHeaderComponent],
      providers: [provideAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardHeaderComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('userName', 'Alice Smith');
    fixture.componentRef.setInput('userInitials', 'AS');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render all four nav links', () => {
    const links = fixture.debugElement.queryAll(By.css('.dashboard-header__nav a'));
    expect(links.length).toBe(4);
    const texts = links.map(l => (l.nativeElement as HTMLAnchorElement).textContent?.trim());
    expect(texts).toContain('Dashboard');
    expect(texts).toContain('Browse resources');
    expect(texts).toContain('My bookings');
    expect(texts).toContain('Admin panel');
  });

  it('should mark the Dashboard link as active', () => {
    const activeLink = fixture.debugElement.query(By.css('.dashboard-header__nav a.active'));
    expect(activeLink).toBeTruthy();
    expect((activeLink.nativeElement as HTMLAnchorElement).textContent?.trim()).toBe('Dashboard');
  });

  it('should render the logo text', () => {
    const logo = fixture.debugElement.query(By.css('.dashboard-header__logo'));
    expect(logo.nativeElement.textContent.trim()).toBe('ThingsBooksy');
  });

  it('should emit settingsClicked when user-menu settings output fires', () => {
    let emitted = false;
    component.settingsClicked.subscribe(() => { emitted = true; });

    const userMenu = fixture.debugElement.query(By.css('tb-user-menu'));
    userMenu.triggerEventHandler('settings', undefined);

    expect(emitted).toBe(true);
  });

  it('should emit logoutClicked when user-menu logout output fires', () => {
    let emitted = false;
    component.logoutClicked.subscribe(() => { emitted = true; });

    const userMenu = fixture.debugElement.query(By.css('tb-user-menu'));
    userMenu.triggerEventHandler('logout', undefined);

    expect(emitted).toBe(true);
  });

  it('should pass userName and userInitials inputs to tb-user-menu', () => {
    const userMenu = fixture.debugElement.query(By.css('tb-user-menu'));
    expect(userMenu).toBeTruthy();
    const userMenuInstance = userMenu.componentInstance;
    expect(userMenuInstance.name()).toBe('Alice Smith');
    expect(userMenuInstance.initials()).toBe('AS');
  });
});
