import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { catchError, debounceTime, distinctUntilChanged, map, of, startWith, switchMap } from 'rxjs';

import {
  AnonymousPlayerIdentityService,
  type AnonymousPlayerIdentity
} from '../../../../anonymous-player-identity.service';
import { GamesApiService } from '../../../../games-api.service';

type ProfileStatus = 'idle' | 'pending' | 'ready' | 'error';

@Component({
  selector: 'bat-join-game-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './join-game-page.html',
  styleUrl: './join-game-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'join-game-page-host'
  }
})
export class JoinGamePage {
  private readonly destroyRef = inject(DestroyRef);
  private readonly gamesApiService = inject(GamesApiService);
  private readonly identityService = inject(AnonymousPlayerIdentityService);
  private readonly router = inject(Router);

  readonly form = new FormGroup({
    playerName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(40)]
    }),
    gameCode: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required]
    }),
    joinSecret: new FormControl('', {
      nonNullable: true
    })
  });

  readonly profileError = signal<string | null>(null);
  readonly profileStatus = signal<ProfileStatus>('idle');
  readonly joinError = signal<string | null>(null);
  readonly joinPending = signal(false);
  readonly canJoinGame = computed(
    () =>
      this.profileStatus() === 'ready' &&
      !this.joinPending() &&
      this.form.controls.gameCode.value.trim() !== ''
  );

  constructor() {
    this.form.controls.playerName.valueChanges
      .pipe(
        startWith(this.form.controls.playerName.value),
        map((value) => value.trim()),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((playerName) => {
          if (playerName === '') {
            return of({ error: null, status: 'idle' as ProfileStatus });
          }

          const currentSession = this.identityService.session();

          if (this.hasMatchingSession(currentSession, playerName)) {
            return of({ error: null, status: 'ready' as ProfileStatus });
          }

          return this.identityService.establish(playerName).pipe(
            map(() => ({ error: null, status: 'ready' as ProfileStatus })),
            startWith({ error: null, status: 'pending' as ProfileStatus }),
            catchError(() =>
              of({
                error: 'Unable to establish the anonymous player profile.',
                status: 'error' as ProfileStatus
              })
            )
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(({ error, status }) => {
        this.profileError.set(error);
        this.profileStatus.set(status);
      });
  }

  protected async joinGame(): Promise<void> {
    if (!this.canJoinGame()) {
      return;
    }

    this.joinPending.set(true);
    this.joinError.set(null);

    const gameCode = this.form.controls.gameCode.value.trim();
    const joinSecret = this.form.controls.joinSecret.value.trim() || undefined;

    try {
      await this.gamesApiService.joinGame(gameCode, joinSecret).toPromise();
      await this.router.navigate(['/games', gameCode]);
    } catch {
      this.joinError.set('Unable to join the game. Check the code and try again.');
    } finally {
      this.joinPending.set(false);
    }
  }

  private hasMatchingSession(
    session: AnonymousPlayerIdentity | null,
    playerName: string
  ): boolean {
    return session !== null && session.playerName === playerName;
  }
}
