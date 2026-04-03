import { DOCUMENT } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { map, Observable, tap } from 'rxjs';

interface AnonymousPlayerSessionResponse {
  readonly playerId: string;
  readonly playerName: string;
  readonly accessToken: string;
  readonly expiresAtUtc: string;
}

export interface AnonymousPlayerIdentity {
  readonly playerId: string;
  readonly playerName: string;
  readonly accessToken: string;
  readonly expiresAtUtc: string;
}

const accessTokenStorageKey = 'battle-ops-access-token';
const expiresAtStorageKey = 'battle-ops-access-token-expires-at';
const playerIdStorageKey = 'battle-ops-player-id';
const playerNameStorageKey = 'battle-ops-player-name';

@Injectable({ providedIn: 'root' })
export class AnonymousPlayerIdentityService {
  private readonly document = inject(DOCUMENT);
  private readonly httpClient = inject(HttpClient);
  private readonly storage = this.document.defaultView?.localStorage ?? null;

  readonly session = signal<AnonymousPlayerIdentity | null>(this.readStoredSession());

  renewSession(accessToken: string): Observable<AnonymousPlayerIdentity> {
    return this.httpClient
      .post<AnonymousPlayerSessionResponse>('/api/profiles/anonymous-sessions/renew', { accessToken })
      .pipe(
        map((response) => ({
          playerId: response.playerId,
          playerName: response.playerName,
          accessToken: response.accessToken,
          expiresAtUtc: response.expiresAtUtc
        })),
        tap((session) => this.persistSession(session))
      );
  }

  establish(playerName: string): Observable<AnonymousPlayerIdentity> {
    const trimmedPlayerName = playerName.trim();

    return this.httpClient
      .post<AnonymousPlayerSessionResponse>('/api/profiles/anonymous-sessions', {
        playerName: trimmedPlayerName
      })
      .pipe(
        map((response) => ({
          playerId: response.playerId,
          playerName: response.playerName,
          accessToken: response.accessToken,
          expiresAtUtc: response.expiresAtUtc
        })),
        tap((session) => this.persistSession(session))
      );
  }

  private persistSession(session: AnonymousPlayerIdentity): void {
    this.session.set(session);

    if (this.storage === null) {
      return;
    }

    this.storage.setItem(accessTokenStorageKey, session.accessToken);
    this.storage.setItem(expiresAtStorageKey, session.expiresAtUtc);
    this.storage.setItem(playerIdStorageKey, session.playerId);
    this.storage.setItem(playerNameStorageKey, session.playerName);
  }

  private readStoredSession(): AnonymousPlayerIdentity | null {
    if (this.storage === null) {
      return null;
    }

    const accessToken = this.storage.getItem(accessTokenStorageKey);
    const expiresAtUtc = this.storage.getItem(expiresAtStorageKey);
    const playerId = this.storage.getItem(playerIdStorageKey);
    const playerName = this.storage.getItem(playerNameStorageKey);

    if (
      accessToken === null ||
      expiresAtUtc === null ||
      playerId === null ||
      playerName === null
    ) {
      return null;
    }

    return {
      playerId,
      playerName,
      accessToken,
      expiresAtUtc
    };
  }
}
