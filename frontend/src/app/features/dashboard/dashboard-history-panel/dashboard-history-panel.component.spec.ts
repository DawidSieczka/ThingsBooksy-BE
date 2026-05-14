import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { describe, it, expect, beforeEach } from 'vitest';
import { DashboardHistoryPanelComponent } from './dashboard-history-panel.component';
import { HistoryRow } from '../mock-data';

const MOCK_ROWS: readonly HistoryRow[] = [
  {
    id: 'test-1',
    resourceName: 'Server Rack A1',
    groupName: 'Datacenter Ops',
    date: new Date(2026, 4, 14),
    time: '09:00 — 11:00',
    amount: '€48.00',
    status: 'confirmed',
    dot: 'primary',
  },
  {
    id: 'test-2',
    resourceName: 'Conference Room',
    groupName: 'Product',
    date: new Date(2026, 4, 13),
    time: '14:30 — 16:00',
    amount: '€32.50',
    status: 'cancelled',
    dot: 'secondary',
  },
];

describe('DashboardHistoryPanelComponent', () => {
  let component: DashboardHistoryPanelComponent;
  let fixture: ComponentFixture<DashboardHistoryPanelComponent>;

  describe('with rows', () => {
    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [DashboardHistoryPanelComponent],
      }).compileComponents();

      fixture = TestBed.createComponent(DashboardHistoryPanelComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('rows', MOCK_ROWS);
      fixture.detectChanges();
    });

    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should render one row per HistoryRow input', () => {
      const rows = fixture.debugElement.queryAll(By.css('.history__row'));
      expect(rows.length).toBe(MOCK_ROWS.length);
    });

    it('should display resource name in each row', () => {
      const names = fixture.debugElement
        .queryAll(By.css('.history__resource-name'))
        .map(el => (el.nativeElement as HTMLElement).textContent?.trim());

      expect(names).toContain('Server Rack A1');
      expect(names).toContain('Conference Room');
    });

    it('should not render empty state when rows are present', () => {
      const empty = fixture.debugElement.query(By.css('.history__empty'));
      expect(empty).toBeNull();
    });

    it('should render a table container', () => {
      const table = fixture.debugElement.query(By.css('.history__table'));
      expect(table).not.toBeNull();
    });
  });

  describe('with empty rows', () => {
    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [DashboardHistoryPanelComponent],
      }).compileComponents();

      fixture = TestBed.createComponent(DashboardHistoryPanelComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('rows', []);
      fixture.detectChanges();
    });

    it('should show empty state message', () => {
      const empty = fixture.debugElement.query(By.css('.history__empty'));
      expect(empty).not.toBeNull();
      expect((empty.nativeElement as HTMLElement).textContent?.trim()).toBe(
        'No recent reservations.',
      );
    });

    it('should not render the table when rows are empty', () => {
      const table = fixture.debugElement.query(By.css('.history__table'));
      expect(table).toBeNull();
    });
  });

  describe('viewAllClicked output', () => {
    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [DashboardHistoryPanelComponent],
      }).compileComponents();

      fixture = TestBed.createComponent(DashboardHistoryPanelComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('rows', MOCK_ROWS);
      fixture.detectChanges();
    });

    it('should emit viewAllClicked when View all button is clicked', () => {
      let emitted = false;
      fixture.componentRef.instance.viewAllClicked.subscribe(() => {
        emitted = true;
      });

      const button = fixture.debugElement.query(By.css('.history__view-all'));
      (button.nativeElement as HTMLButtonElement).click();
      fixture.detectChanges();

      expect(emitted).toBe(true);
    });
  });
});
