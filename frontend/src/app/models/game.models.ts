export interface Move {
  id: number;
  name: string;
}

export interface CreateGameRequest {
  player1Name: string;
  player2Name: string;
}

export interface GameResponse {
  id: number;
  player1: string;
  player2: string;
  player1Score: number;
  player2Score: number;
  isFinished: boolean;
}

export interface PlayRoundRequest {
  player1MoveId: number;
  player2MoveId: number;
}

export interface RoundResult {
  round: number;
  player1Move: string;
  player2Move: string;
  roundWinner: string | null;
  player1Score: number;
  player2Score: number;
  gameFinished: boolean;
  gameWinner: string | null;
}
