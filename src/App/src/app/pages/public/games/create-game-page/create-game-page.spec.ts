import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';

import { CreateGamePage } from './create-game-page';
import { authInterceptor } from '../../../../auth.interceptor';

describe('CreateGamePage', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateGamePage],
      providers: [provideRouter([]), provideHttpClient(withInterceptors([authInterceptor])), provideHttpClientTesting()]
    }).compileComponents();
  });

  it('should enable create game after the player profile is established', async () => {
    const fixture = TestBed.createComponent(CreateGamePage);
    const httpTestingController = TestBed.inject(HttpTestingController);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    const playerNameInput = host.querySelector('#player-name') as HTMLInputElement;
    const submitButton = host.querySelector('button[type="submit"]') as HTMLButtonElement;

    playerNameInput.value = 'Commander';
    playerNameInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    await new Promise((resolve) => window.setTimeout(resolve, 350));

    httpTestingController.expectOne('/api/profiles/anonymous-sessions').flush({
      playerId: 'player-1',
      playerName: 'Commander',
      accessToken: 'session-token',
      expiresAtUtc: '2026-03-30T12:00:00Z'
    });

    fixture.detectChanges();
    await fixture.whenStable();

    expect(submitButton.disabled).toBe(false);
  });

  it('should create a game and navigate to the game route', async () => {
    const fixture = TestBed.createComponent(CreateGamePage);
    const httpTestingController = TestBed.inject(HttpTestingController);
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    const playerNameInput = host.querySelector('#player-name') as HTMLInputElement;
    const submitButton = host.querySelector('button[type="submit"]') as HTMLButtonElement;

    playerNameInput.value = 'Captain';
    playerNameInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    await new Promise((resolve) => window.setTimeout(resolve, 350));

    httpTestingController.expectOne('/api/profiles/anonymous-sessions').flush({
      playerId: 'player-2',
      playerName: 'Captain',
      accessToken: 'captain-token',
      expiresAtUtc: '2026-03-30T12:00:00Z'
    });

    fixture.detectChanges();
    await fixture.whenStable();

    submitButton.click();
    fixture.detectChanges();

    const createGameRequest = httpTestingController.expectOne('/api/games');
    expect(createGameRequest.request.headers.get('Authorization')).toBe('Bearer captain-token');
    createGameRequest.flush({
      gameCode: '12345678'
    });

    await fixture.whenStable();

    expect(navigateSpy).toHaveBeenCalledWith(['/games', '12345678']);
  });
});
