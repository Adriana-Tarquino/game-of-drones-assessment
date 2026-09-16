import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';

import {
  Move,
  CreateGameRequest,
  GameResponse,
  PlayRoundRequest,
  RoundResult
} from '../models/game.models';

@Injectable({
  providedIn: 'root'
})
export class GameService {

  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getMoves(): Observable<Move[]> {
    return this.http.get<Move[]>(
      `${this.apiUrl}/moves`
    );
  }

  createGame(
    request: CreateGameRequest
  ): Observable<GameResponse> {

    return this.http.post<GameResponse>(
      `${this.apiUrl}/games`,
      request
    );
  }

  playRound(
    gameId: number,
    request: PlayRoundRequest
  ): Observable<RoundResult> {

    return this.http.post<RoundResult>(
      `${this.apiUrl}/games/${gameId}/rounds`,
      request
    );
  }
}
