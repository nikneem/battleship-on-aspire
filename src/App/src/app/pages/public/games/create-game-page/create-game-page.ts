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
  selector: 'bat-create-game-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './create-game-page.html',
  styleUrl: './create-game-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'create-game-page-host'
  }
})
export class CreateGamePage {
  private readonly destroyRef = inject(DestroyRef);
  private readonly gamesApiService = inject(GamesApiService);
  private readonly identityService = inject(AnonymousPlayerIdentityService);
  private readonly router = inject(Router);

  readonly form = new FormGroup({
    playerName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(40)]
    }),
    joinSecret: new FormControl('', {
      nonNullable: true
    })
  });

  readonly profileError = signal<string | null>(null);
  readonly profileStatus = signal<ProfileStatus>('idle');
  readonly createError = signal<string | null>(null);
  readonly createPending = signal(false);
  readonly canCreateGame = computed(
    () => this.profileStatus() === 'ready' && !this.createPending()
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
            return of({
              error: null,
              status: 'idle' as ProfileStatus
            });
          }

          const currentSession = this.identityService.session();

          if (this.hasMatchingSession(currentSession, playerName)) {
            return of({
              error: null,
              status: 'ready' as ProfileStatus
            });
          }

          return this.identityService.establish(playerName).pipe(
            map(() => ({
              error: null,
              status: 'ready' as ProfileStatus
            })),
            startWith({
              error: null,
              status: 'pending' as ProfileStatus
            }),
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

  protected async createGame(): Promise<void> {
    if (!this.canCreateGame()) {
      return;
    }

    this.createPending.set(true);
    this.createError.set(null);

    try {
      const response = await this.gamesApiService
        .createGame(this.form.controls.joinSecret.value)
        .toPromise();

      if (response === undefined) {
        throw new Error('Game creation did not return a response.');
      }

      await this.router.navigate(['/games', response.gameCode]);
    } catch {
      this.createError.set('Unable to create a game lobby right now.');
    } finally {
      this.createPending.set(false);
    }
  }

  private hasMatchingSession(
    session: AnonymousPlayerIdentity | null,
    playerName: string
  ): boolean {
    return session !== null && session.playerName === playerName;
  }
}
