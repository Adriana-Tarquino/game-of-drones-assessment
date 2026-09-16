import { Component, afterNextRender, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import { GameService } from './services/game.service';
import { GameResponse, Move, RoundResult } from './models/game.models';

@Component({
  selector: 'app-root',
  imports: [FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly gameService = inject(GameService);

  readonly moves = signal<Move[]>([]);
  readonly game = signal<GameResponse | null>(null);
  readonly latestRound = signal<RoundResult | null>(null);
  readonly loadingMoves = signal(true);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  player1Name = '';
  player2Name = '';
  player1MoveId: number | null = null;
  player2MoveId: number | null = null;

  constructor() {
    // Avoid an API call while Angular renders on the server.
    afterNextRender(() => this.loadMoves());
  }

  createGame(): void {
    const player1Name = this.player1Name.trim();
    const player2Name = this.player2Name.trim();

    if (!player1Name || !player2Name) {
      this.error.set('Escribe el nombre de los dos jugadores.');
      return;
    }

    this.error.set(null);
    this.submitting.set(true);
    this.gameService.createGame({ player1Name, player2Name })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (game) => {
          this.game.set(game);
          this.latestRound.set(null);
          this.player1MoveId = null;
          this.player2MoveId = null;
        },
        error: (error) => this.showApiError(error, 'No se pudo crear la partida.')
      });
  }

  playRound(): void {
    const game = this.game();
    if (!game || this.player1MoveId === null || this.player2MoveId === null) {
      this.error.set('Cada jugador debe elegir un movimiento.');
      return;
    }

    this.error.set(null);
    this.submitting.set(true);
    this.gameService.playRound(game.id, {
      player1MoveId: this.player1MoveId,
      player2MoveId: this.player2MoveId
    })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (round) => {
          this.latestRound.set(round);
          this.game.set({
            ...game,
            player1Score: round.player1Score,
            player2Score: round.player2Score,
            isFinished: round.gameFinished
          });
          this.player1MoveId = null;
          this.player2MoveId = null;
        },
        error: (error) => this.showApiError(error, 'No se pudo registrar la ronda.')
      });
  }

  newGame(): void {
    this.game.set(null);
    this.latestRound.set(null);
    this.error.set(null);
    this.player1MoveId = null;
    this.player2MoveId = null;
  }

  private loadMoves(): void {
    this.error.set(null);
    this.loadingMoves.set(true);
    this.gameService.getMoves()
      .pipe(finalize(() => this.loadingMoves.set(false)))
      .subscribe({
        next: (moves) => this.moves.set(moves),
        error: (error) => this.showApiError(error, 'No se pudieron cargar los movimientos. Comprueba que el backend esté iniciado en http://localhost:5212.')
      });
  }

  private showApiError(error: { error?: unknown }, fallback: string): void {
    const message = typeof error.error === 'string' ? error.error : fallback;
    this.error.set(message);
  }
}
