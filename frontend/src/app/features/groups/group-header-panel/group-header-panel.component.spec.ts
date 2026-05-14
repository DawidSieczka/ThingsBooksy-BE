import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { By } from '@angular/platform-browser';
import { GroupHeaderPanelComponent } from './group-header-panel.component';
import { ThingsBooksyModulesManagementGroupsCoreFeaturesGetManagementGroupGetManagementGroupQueryResult as GroupDetailDto } from '../../../api/data-contracts';

const SAMPLE_GROUP: GroupDetailDto = {
  id: 'group-1',
  name: 'Engineering Team',
  description: 'The main engineering group.',
  ownerId: 'owner-1',
  createdAt: '2024-03-15T10:00:00Z',
  memberCount: 5,
  members: [],
};

describe('GroupHeaderPanelComponent', () => {
  let component: GroupHeaderPanelComponent;
  let fixture: ComponentFixture<GroupHeaderPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GroupHeaderPanelComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(GroupHeaderPanelComponent);
    component = fixture.componentInstance;

    fixture.componentRef.setInput('group', SAMPLE_GROUP);
    fixture.componentRef.setInput('resourceCount', 3);
    fixture.componentRef.setInput('schemaCount', 2);
    fixture.componentRef.setInput('isOwner', true);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('renders the group name in the h1 heading', () => {
    const heading = fixture.debugElement.query(By.css('.group-header-panel__name'));
    expect(heading).toBeTruthy();
    expect(heading.nativeElement.textContent.trim()).toBe('Engineering Team');
  });

  it('renders the group description', () => {
    const description = fixture.debugElement.query(By.css('.group-header-panel__description'));
    expect(description).toBeTruthy();
    expect(description.nativeElement.textContent.trim()).toBe('The main engineering group.');
  });

  it('does not render description element when description is absent', () => {
    fixture.componentRef.setInput('group', { ...SAMPLE_GROUP, description: null });
    fixture.detectChanges();

    const description = fixture.debugElement.query(By.css('.group-header-panel__description'));
    expect(description).toBeNull();
  });

  it('hides Edit and Delete buttons when isOwner is false', () => {
    fixture.componentRef.setInput('isOwner', false);
    fixture.detectChanges();

    const actions = fixture.debugElement.query(By.css('.group-header-panel__actions'));
    expect(actions).toBeNull();
  });

  it('shows Edit and Delete buttons when isOwner is true', () => {
    const editBtn = fixture.debugElement.query(By.css('.group-header-panel__btn--edit'));
    const deleteBtn = fixture.debugElement.query(By.css('.group-header-panel__btn--delete'));
    expect(editBtn).toBeTruthy();
    expect(deleteBtn).toBeTruthy();
  });

  it('emits edit event when Edit button is clicked', () => {
    const emitted: void[] = [];
    component.edit.subscribe(() => emitted.push(undefined));

    const editBtn = fixture.debugElement.query(By.css('.group-header-panel__btn--edit'));
    editBtn.nativeElement.click();

    expect(emitted.length).toBe(1);
  });

  it('emits delete event when Delete button is clicked', () => {
    const emitted: void[] = [];
    component.delete.subscribe(() => emitted.push(undefined));

    const deleteBtn = fixture.debugElement.query(By.css('.group-header-panel__btn--delete'));
    deleteBtn.nativeElement.click();

    expect(emitted.length).toBe(1);
  });

  it('renders "5 members" label when memberCount is 5', () => {
    const chips = fixture.debugElement.queryAll(By.css('.group-header-panel__chip'));
    const membersChip = chips.find(c => c.nativeElement.textContent.includes('member'));
    expect(membersChip).toBeTruthy();
    expect(membersChip!.nativeElement.textContent).toContain('5 members');
  });

  it('renders "1 member" (singular) when memberCount is 1', () => {
    fixture.componentRef.setInput('group', { ...SAMPLE_GROUP, memberCount: 1 });
    fixture.detectChanges();

    const chips = fixture.debugElement.queryAll(By.css('.group-header-panel__chip'));
    const membersChip = chips.find(c => c.nativeElement.textContent.includes('member'));
    expect(membersChip).toBeTruthy();
    expect(membersChip!.nativeElement.textContent).toContain('1 member');
    expect(membersChip!.nativeElement.textContent).not.toContain('1 members');
  });

  it('renders the resource count in the meta chips', () => {
    const chips = fixture.debugElement.queryAll(By.css('.group-header-panel__chip'));
    const resourceChip = chips.find(c => c.nativeElement.textContent.includes('resources'));
    expect(resourceChip).toBeTruthy();
    expect(resourceChip!.nativeElement.textContent).toContain('3 resources');
  });

  it('renders the created date chip', () => {
    const chips = fixture.debugElement.queryAll(By.css('.group-header-panel__chip'));
    const dateChip = chips.find(c => c.nativeElement.textContent.includes('Created'));
    expect(dateChip).toBeTruthy();
  });

  it('renders tb-avatar with group id and initials', () => {
    const avatar = fixture.debugElement.query(By.css('tb-avatar'));
    expect(avatar).toBeTruthy();
    expect(component.avatarId()).toBe('group-1');
    expect(component.avatarInitials()).toBe('ET');
  });
});
