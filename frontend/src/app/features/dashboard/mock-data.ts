// TODO: replace with /bookings API once the bookings module ships.
// History rows remain mocked because no booking endpoint exists yet.

export type ReservationStatus = 'confirmed' | 'cancelled';
export type DotColor = 'primary' | 'secondary' | 'tertiary';

export interface HistoryRow {
  readonly id: string;
  readonly resourceName: string;
  readonly groupName: string;
  readonly date: Date;
  readonly time: string;
  readonly amount: string;
  readonly status: ReservationStatus;
  readonly dot: DotColor;
}

export const HISTORY_ROWS: readonly HistoryRow[] = [
  {
    id: 'mock-1',
    resourceName: 'Server Rack A1',
    groupName: 'Datacenter Ops',
    date: new Date(2026, 4, 14),
    time: '09:00 — 11:00',
    amount: '€48.00',
    status: 'confirmed',
    dot: 'primary',
  },
  {
    id: 'mock-2',
    resourceName: 'Conference Room — Skylight',
    groupName: 'Product',
    date: new Date(2026, 4, 13),
    time: '14:30 — 16:00',
    amount: '€32.50',
    status: 'confirmed',
    dot: 'secondary',
  },
  {
    id: 'mock-3',
    resourceName: 'Fleet Car — Tesla Model 3',
    groupName: 'Field Sales',
    date: new Date(2026, 4, 12),
    time: '08:00 — 18:00',
    amount: '€120.00',
    status: 'cancelled',
    dot: 'tertiary',
  },
  {
    id: 'mock-4',
    resourceName: 'Projector — Sony VPL-X120',
    groupName: 'Marketing',
    date: new Date(2026, 4, 11),
    time: '10:00 — 12:00',
    amount: '€18.00',
    status: 'confirmed',
    dot: 'primary',
  },
  {
    id: 'mock-5',
    resourceName: 'Standing Desk — Block 4',
    groupName: 'Engineering',
    date: new Date(2026, 4, 10),
    time: '09:00 — 17:00',
    amount: '€12.00',
    status: 'confirmed',
    dot: 'secondary',
  },
  {
    id: 'mock-6',
    resourceName: 'Drone — DJI Mavic 3',
    groupName: 'Field Sales',
    date: new Date(2026, 4, 9),
    time: '13:00 — 15:30',
    amount: '€64.00',
    status: 'cancelled',
    dot: 'tertiary',
  },
];
