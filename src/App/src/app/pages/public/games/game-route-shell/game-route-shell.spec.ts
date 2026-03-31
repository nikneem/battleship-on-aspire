import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';

import { GameRouteShell } from './game-route-shell';

describe('GameRouteShell', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  beforeEach(async () => {
    localStorage.setItem('battle-ops-access-token', 'gameplay-token');
    localStorage.setItem('battle-ops-access-token-expires-at', '2026-03-30T12:00:00Z');
    localStorage.setItem('battle-ops-player-id', 'player-1');
    localStorage.setItem('battle-ops-player-name', 'Commander');

    await TestBed.configureTestingModule({
      imports: [GameRouteShell],
      providers: [
        provideHttpClient(),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ gameCode: 'SECTOR7' })
            }
          }
        }
      ]
    }).compileComponents();
  });

  it('should render the setup board and full ship inventory before readiness', () => {
    const fixture = TestBed.createComponent(GameRouteShell);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;

    expect(host.textContent).toContain('GAME SECTOR7');
    expect(host.textContent).toContain('COMMANDER');
    expect(host.querySelectorAll('.setup-board__cell').length).toBe(100);
    expect(host.querySelectorAll('.ship-card').length).toBe(5);
    expect(host.querySelector('[data-ready-action]')).toBeNull();
  });

  it('should place ships, reveal ready, and support contextual rotation', () => {
    const fixture = TestBed.createComponent(GameRouteShell);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;

    placeShip(host, fixture, 'carrier', '0-0');
    placeShip(host, fixture, 'battleship', '6-0');
    placeShip(host, fixture, 'cruiser', '7-0');
    placeShip(host, fixture, 'submarine', '8-0');
    placeShip(host, fixture, 'destroyer', '9-0');

    const carrier = host.querySelector('[data-ship-id="carrier"].setup-board__ship') as HTMLButtonElement | null;
    expect(carrier?.getAttribute('data-orientation')).toBe('horizontal');

    carrier?.click();
    fixture.detectChanges();

    const rotateButton = host.querySelector('[data-rotate-action]') as HTMLButtonElement | null;
    expect(rotateButton).not.toBeNull();

    rotateButton?.click();
    fixture.detectChanges();

    expect(carrier?.getAttribute('data-orientation')).toBe('vertical');
    expect(host.querySelector('[data-ready-action]')).not.toBeNull();
  });

  it('should keep rotated ships within the board bounds', () => {
    const fixture = TestBed.createComponent(GameRouteShell);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;

    placeShip(host, fixture, 'carrier', '0-8');

    const carrier = host.querySelector('[data-ship-id="carrier"].setup-board__ship') as HTMLButtonElement | null;
    expect(carrier?.style.gridColumn).toContain('6 / span 5');

    carrier?.click();
    fixture.detectChanges();

    const rotateButton = host.querySelector('[data-rotate-action]') as HTMLButtonElement | null;
    rotateButton?.click();
    fixture.detectChanges();

    expect(carrier?.getAttribute('data-orientation')).toBe('vertical');
    expect(carrier?.style.gridColumn).toContain('6 / span 1');
    expect(carrier?.style.gridRow).toContain('1 / span 5');
  });

  it('should lock ship movement and rotation after readiness is confirmed', () => {
    const fixture = TestBed.createComponent(GameRouteShell);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;

    placeShip(host, fixture, 'carrier', '0-0');
    placeShip(host, fixture, 'battleship', '1-0');
    placeShip(host, fixture, 'cruiser', '2-0');
    placeShip(host, fixture, 'submarine', '3-0');
    placeShip(host, fixture, 'destroyer', '4-0');

    const carrier = host.querySelector('[data-ship-id="carrier"].setup-board__ship') as HTMLButtonElement | null;
    const originalGridColumn = carrier?.style.gridColumn;

    const readyButton = host.querySelector('[data-ready-action]') as HTMLButtonElement | null;
    readyButton?.click();
    fixture.detectChanges();

    expect(host.querySelector('[data-ready-action]')).toBeNull();
    expect(host.querySelector('[data-rotate-action]')).toBeNull();
    expect(carrier?.draggable).toBe(false);

    carrier?.click();
    fixture.detectChanges();

    const alternateCell = host.querySelector('[data-cell="8-8"]') as HTMLButtonElement | null;
    alternateCell?.click();
    fixture.detectChanges();

    expect(carrier?.style.gridColumn).toBe(originalGridColumn);
  });
});

function placeShip(host: HTMLElement, fixture: { detectChanges(): void }, shipId: string, cellKey: string): void {
  const shipButton = host.querySelector(`.ship-card[data-ship-id="${shipId}"]`) as HTMLButtonElement | null;
  const cellButton = host.querySelector(`[data-cell="${cellKey}"]`) as HTMLButtonElement | null;

  shipButton?.click();
  fixture.detectChanges();
  cellButton?.click();
  fixture.detectChanges();
}
