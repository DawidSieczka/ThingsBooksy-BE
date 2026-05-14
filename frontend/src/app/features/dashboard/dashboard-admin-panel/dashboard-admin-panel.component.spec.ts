import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { By } from '@angular/platform-browser';
import { DashboardAdminPanelComponent } from './dashboard-admin-panel.component';
import { GroupItem } from '../mock-data';

const MEMBER_GROUPS_STUB: readonly GroupItem[] = [
  { id: 'g1', name: 'Engineering', memberCount: 10, initials: 'EN', accent: 'primary', isAdmin: false },
  { id: 'g2', name: 'Product', memberCount: 5, initials: 'PR', accent: 'secondary', isAdmin: false },
];

const ADMIN_GROUPS_STUB: readonly GroupItem[] = [
  { id: 'g3', name: 'Design', memberCount: 4, initials: 'DS', accent: 'tertiary', isAdmin: true },
];

describe('DashboardAdminPanelComponent', () => {
  let component: DashboardAdminPanelComponent;
  let fixture: ComponentFixture<DashboardAdminPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardAdminPanelComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardAdminPanelComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('memberGroups', MEMBER_GROUPS_STUB);
    fixture.componentRef.setInput('adminGroups', ADMIN_GROUPS_STUB);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the Admin panel heading', () => {
    const h2 = fixture.debugElement.query(By.css('.admin__title'));
    expect(h2.nativeElement.textContent.trim()).toBe('Admin panel');
  });

  it('should render the correct number of member groups', () => {
    const items = fixture.debugElement.queryAll(By.css('.admin__section:first-of-type .admin__group'));
    expect(items.length).toBe(MEMBER_GROUPS_STUB.length);
  });

  it('should render the correct number of admin groups', () => {
    const items = fixture.debugElement.queryAll(By.css('.admin__section:last-of-type .admin__group'));
    expect(items.length).toBe(ADMIN_GROUPS_STUB.length);
  });

  it('should display group names in the member list', () => {
    const names = fixture.debugElement
      .queryAll(By.css('.admin__section:first-of-type .admin__group-name'))
      .map(el => el.nativeElement.textContent.trim());
    expect(names).toContain('Engineering');
    expect(names).toContain('Product');
  });

  it('should display the Admin role badge for admin groups', () => {
    const badges = fixture.debugElement.queryAll(By.css('.admin__role-badge'));
    expect(badges.length).toBe(ADMIN_GROUPS_STUB.length);
    expect(badges[0].nativeElement.textContent.trim()).toBe('Admin');
  });

  it('should emit createGroupClicked when the primary button is clicked', () => {
    const emitSpy = vi.spyOn(component.createGroupClicked, 'emit');
    const btn = fixture.debugElement.query(By.css('.admin__primary-btn'));
    btn.nativeElement.click();
    expect(emitSpy).toHaveBeenCalledTimes(1);
  });

  it('should show empty message when memberGroups is empty', async () => {
    fixture.componentRef.setInput('memberGroups', []);
    fixture.detectChanges();
    const empty = fixture.debugElement.query(By.css('.admin__section:first-of-type .admin__empty'));
    expect(empty).toBeTruthy();
    expect(empty.nativeElement.textContent.trim()).toBe('No groups yet.');
  });

  it('should show empty message when adminGroups is empty', async () => {
    fixture.componentRef.setInput('adminGroups', []);
    fixture.detectChanges();
    const empty = fixture.debugElement.query(By.css('.admin__section:last-of-type .admin__empty'));
    expect(empty).toBeTruthy();
    expect(empty.nativeElement.textContent.trim()).toBe('No groups yet.');
  });
});
