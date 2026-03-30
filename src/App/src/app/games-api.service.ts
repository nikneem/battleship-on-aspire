import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { AnonymousPlayerIdentityService } from './anonymous-player-identity.service';

export interface CreateGameResponse {
  readonly gameCode: string;
}

@Injectable({ providedIn: 'root' })
export class GamesApiService {
  private readonly httpClient = inject(HttpClient);
  private readonly identityService = inject(AnonymousPlayerIdentityService);

  createGame(joinSecret: string): Observable<CreateGameResponse> {
    const session = this.identityService.session();

    if (session === null) {
      throw new Error('An anonymous player session must exist before creating a game.');
    }

    return this.httpClient.post<CreateGameResponse>(
      '/api/games',
      {
        joinSecret: joinSecret.trim() === '' ? null : joinSecret.trim()
      },
      {
        headers: new HttpHeaders({
          Authorization: `Bearer ${session.accessToken}`
        })
      }
    );
  }
}
