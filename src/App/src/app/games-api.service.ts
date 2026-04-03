import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface CreateGameResponse {
  readonly gameCode: string;
}

export interface GameLobbyResponse {
  readonly gameCode: string;
  readonly phase: number; // 0=LobbyOpen, 1=LobbyFull
  readonly protection: number; // 0=Open, 1=Protected
  readonly isJoinable: boolean;
  readonly host: GameParticipant;
  readonly guest: GameParticipant | null;
}

export interface ShotRecord {
  readonly coordinate: { readonly row: number; readonly column: number };
  readonly outcome: number; // 0=Miss, 1=Hit, 2=Sunk
}

export interface ShipPlacement {
  readonly length: number;
  readonly start: { readonly row: number; readonly column: number };
  readonly orientation: number; // 0=Horizontal, 1=Vertical
}

export interface PlayerBoardState {
  readonly isLocked: boolean;
  readonly ships: readonly ShipPlacement[];
  readonly incomingShots: readonly ShotRecord[];
}

export interface OpponentBoardState {
  readonly knownShots: readonly ShotRecord[];
}

export interface GameParticipant {
  readonly playerId: string;
  readonly playerName: string;
  readonly state: number;
}

export interface GameStateResponse {
  readonly gameCode: string;
  readonly phase: number; // 0=LobbyOpen, 1=LobbyFull, 2=Setup, 3=InProgress, 4=Finished, 5=Cancelled, 6=Abandoned
  readonly currentTurnPlayerId: string | null;
  readonly winnerPlayerId: string | null;
  readonly currentPlayer: GameParticipant;
  readonly opponent: GameParticipant | null;
  readonly ownBoard: PlayerBoardState;
  readonly opponentBoard: OpponentBoardState;
}

export interface ShipPlacementRequest {
  readonly length: number;
  readonly start: { readonly row: number; readonly column: number };
  readonly orientation: number; // 0=Horizontal, 1=Vertical
}

@Injectable({ providedIn: 'root' })
export class GamesApiService {
  private readonly httpClient = inject(HttpClient);

  createGame(joinSecret: string): Observable<CreateGameResponse> {
    return this.httpClient.post<CreateGameResponse>('/api/games', {
      joinSecret: joinSecret.trim() === '' ? null : joinSecret.trim()
    });
  }

  joinGame(gameCode: string, joinSecret?: string): Observable<GameLobbyResponse> {
    return this.httpClient.post<GameLobbyResponse>('/api/games/join', {
      gameCode: gameCode.trim(),
      joinSecret: joinSecret?.trim() || null
    });
  }

  getGameState(gameCode: string): Observable<GameStateResponse> {
    return this.httpClient.get<GameStateResponse>(`/api/games/${gameCode}`);
  }

  submitFleet(gameCode: string, ships: readonly ShipPlacementRequest[]): Observable<GameStateResponse> {
    return this.httpClient.put<GameStateResponse>(`/api/games/${gameCode}/fleet`, { ships });
  }

  lockFleet(gameCode: string): Observable<GameStateResponse> {
    return this.httpClient.post<GameStateResponse>(`/api/games/${gameCode}/lock`, {});
  }

  fireShot(gameCode: string, row: number, column: number): Observable<GameStateResponse> {
    return this.httpClient.post<GameStateResponse>(`/api/games/${gameCode}/shots`, {
      target: { row, column }
    });
  }
}
