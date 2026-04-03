import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

export interface ShotFiredEvent {
  readonly firingPlayerId: string;
  readonly targetRow: number;
  readonly targetColumn: number;
  readonly outcome: number; // 0=Miss, 1=Hit, 2=Sunk
}

@Injectable({ providedIn: 'root' })
export class GameSignalRService implements OnDestroy {
  private connection: signalR.HubConnection | null = null;

  readonly shotFired$ = new Subject<ShotFiredEvent>();
  readonly gameStarted$ = new Subject<string>();         // firstTurnPlayerId
  readonly gameFinished$ = new Subject<string>();        // winnerPlayerId
  readonly gameAbandoned$ = new Subject<string>();       // abandoningPlayerId
  readonly opponentConnectionLost$ = new Subject<string>(); // playerId
  readonly fleetLocked$ = new Subject<string>();         // playerId
  readonly playerReady$ = new Subject<string>();         // playerId
  readonly playerJoined$ = new Subject<string>();        // guestPlayerId

  connect(gameCode: string, playerId: string, token: string): void {
    this.disconnect();

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/game', { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    this.connection.on('ShotFired', (firingPlayerId: string, targetRow: number, targetColumn: number, outcome: number) =>
      this.shotFired$.next({ firingPlayerId, targetRow, targetColumn, outcome })
    );
    this.connection.on('GameStarted', (firstTurnPlayerId: string) =>
      this.gameStarted$.next(firstTurnPlayerId)
    );
    this.connection.on('GameFinished', (winnerPlayerId: string) =>
      this.gameFinished$.next(winnerPlayerId)
    );
    this.connection.on('GameAbandoned', (abandoningPlayerId: string) =>
      this.gameAbandoned$.next(abandoningPlayerId)
    );
    this.connection.on('OpponentConnectionLost', (pid: string) =>
      this.opponentConnectionLost$.next(pid)
    );
    this.connection.on('FleetLocked', (pid: string) =>
      this.fleetLocked$.next(pid)
    );
    this.connection.on('PlayerReady', (pid: string) =>
      this.playerReady$.next(pid)
    );
    this.connection.on('PlayerJoined', (guestPlayerId: string) =>
      this.playerJoined$.next(guestPlayerId)
    );

    this.connection
      .start()
      .then(() => this.connection!.invoke('JoinGame', gameCode, playerId))
      .catch((err) => console.error('SignalR connection error:', err));
  }

  disconnect(): void {
    this.connection?.stop();
    this.connection = null;
  }

  ngOnDestroy(): void {
    this.disconnect();
  }
}
