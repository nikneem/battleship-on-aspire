import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { Subject, of } from 'rxjs';

import { GameRouteShell } from './game-route-shell';
import { GameSignalRService } from '../../../../game-signal-r.service';
import { GamesApiService, GameStateResponse } from '../../../../games-api.service';

const defaultGameState: GameStateResponse = {
  gameCode: 'SECTOR7',
  phase: 2,
  currentTurnPlayerId: null,
  winnerPlayerId: null,
  currentPlayer: { playerId: 'player-1', playerName: 'Commander', state: 2 },
  opponent: null,
  ownBoard: { isLocked: false, ships: [], incomingShots: [] },
  opponentBoard: { knownShots: [] }
};

const lockedGameState: GameStateResponse = {
  ...defaultGameState,
  ownBoard: { isLocked: true, ships: [], incomingShots: [] }
};

function makeSignalRStub() {
  return {
    connect: () => {},
    disconnect: () => {},
    shotFired$: new Subject(),
    gameStarted$: new Subject(),
    gameFinished$: new Subject(),
    gameAbandoned$: new Subject(),
    opponentConnectionLost$: new Subject(),
    fleetLocked$: new Subject()
  };
}

function makeGamesApiStub() {
  return {
    getGameState: () => of(defaultGameState),
    submitFleet: () => of(defaultGameState),
    lockFleet: () => of(lockedGameState),
    fireShot: () => of(defaultGameState),
    createGame: () => { throw new Error('Not expected in these tests'); }
  };
}

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
        { provide: GameSignalRService, useValue: makeSignalRStub() },
        { provide: GamesApiService, useValue: makeGamesApiStub() },
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
  it('should show outcome overlay with winner when gameFinished$ fires with own player ID', () => {
    const signalRStub = makeSignalRStub();
    TestBed.overrideProvider(GameSignalRService, { useValue: signalRStub });

    const fixture = TestBed.createComponent(GameRouteShell);
    fixture.detectChanges();

    signalRStub.gameStarted$.next('player-1');
    fixture.detectChanges();

    signalRStub.gameFinished$.next('player-1');
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    const overlay = host.querySelector('bat-game-outcome-overlay');
    expect(overlay).not.toBeNull();
    expect(host.textContent).toContain('WINNER!');
  });

  it('should show outcome overlay with loser when gameFinished$ fires with opponent player ID', () => {
    const signalRStub = makeSignalRStub();
    TestBed.overrideProvider(GameSignalRService, { useValue: signalRStub });

    const fixture = TestBed.createComponent(GameRouteShell);
    fixture.detectChanges();

    signalRStub.gameStarted$.next('player-2');
    fixture.detectChanges();

    signalRStub.gameFinished$.next('player-2');
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    const overlay = host.querySelector('bat-game-outcome-overlay');
    expect(overlay).not.toBeNull();
    expect(host.textContent).toContain('YOU LOST');
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
