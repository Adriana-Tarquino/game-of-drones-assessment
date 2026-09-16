# Game of Drones

A full-stack implementation of the Paper, Rock, Scissors assessment. Two players share one computer, take turns selecting a move, and the first player to win three rounds wins the game.

## Live demo

- Frontend: https://game-of-drones-bqp65cxqg-adriana-tarquinos-projects.vercel.app/
- API: https://game-of-drones-assessment.onrender.com/

## Features

- Angular user interface with a sequential, private turn flow: Player 1 chooses first, then Player 2.
- Live scoreboard and the result of the latest round.
- Game and player-win persistence in SQLite through Entity Framework Core.
- Configurable moves and rules at runtime through the REST API.
- Default rules: Paper beats Rock, Rock beats Scissors, and Scissors beat Paper.
- A round is a draw when neither selected move beats the other.

## Tech stack

- Angular 21
- ASP.NET Core 10
- Entity Framework Core and SQLite

## Prerequisites

- .NET SDK 10
- Node.js 20.19+ or 22.12+
- npm

## Run locally

### 1. Start the API

From the repository root:

```powershell
dotnet run --project backend/GameOfDrones.Api
```

The API starts at `http://localhost:5212`. On startup it applies the Entity Framework migrations automatically and creates the SQLite database if it does not exist.

You can also open `GameOfDrones.slnx` in Visual Studio and start the `GameOfDrones.Api` project.

### 2. Start the Angular application

In a second terminal:

```powershell
cd frontend
npm install
npm start
```

Open `http://localhost:4200`.

During development Angular proxies `/api` to `http://localhost:5212`, so the application code uses a relative API path rather than a hard-coded backend URL.

## API reference

| Area | Endpoints |
| --- | --- |
| Moves | `GET /api/moves`, `GET /api/moves/{id}`, `POST /api/moves`, `PUT /api/moves/{id}`, `DELETE /api/moves/{id}` |
| Move rules | `GET /api/move-rules`, `GET /api/move-rules/{id}`, `POST /api/move-rules`, `PUT /api/move-rules/{id}`, `DELETE /api/move-rules/{id}` |
| Players | `GET /api/players`, `GET /api/players/{id}` |
| Games | `GET /api/games`, `GET /api/games/{id}`, `POST /api/games`, `POST /api/games/{id}/rounds` |

## Test the API with PowerShell

Use a second terminal while the API is running:

```powershell
# Default moves and rules
Invoke-RestMethod http://localhost:5212/api/moves
Invoke-RestMethod http://localhost:5212/api/move-rules

# Create a game
$game = Invoke-RestMethod -Method Post `
  -Uri http://localhost:5212/api/games `
  -ContentType 'application/json' `
  -Body '{"player1Name":"Ana","player2Name":"Luis"}'

# Player 1 chooses Paper (2) and Player 2 chooses Rock (1).
# Repeat three times to make Ana win the game.
Invoke-RestMethod -Method Post `
  -Uri "http://localhost:5212/api/games/$($game.id)/rounds" `
  -ContentType 'application/json' `
  -Body '{"player1MoveId":2,"player2MoveId":1}'

# Check the game and player ranking
Invoke-RestMethod "http://localhost:5212/api/games/$($game.id)"
Invoke-RestMethod http://localhost:5212/api/players
```

To verify runtime configuration, add an unused test move and delete it afterwards:

```powershell
$move = Invoke-RestMethod -Method Post `
  -Uri http://localhost:5212/api/moves `
  -ContentType 'application/json' `
  -Body '{"name":"Lizard"}'

Invoke-RestMethod -Method Delete `
  -Uri "http://localhost:5212/api/moves/$($move.id)"
```

Moves that are already used by a rule or played round cannot be deleted. Remove their dependent rule first, and never delete a move referenced by historical rounds.

## Delivery checklist

- Push the repository to GitHub.
- Deploy the frontend and backend, then add the public URL here.
- Do not commit `node_modules`, `bin`, `obj`, or local SQLite database files.
- Build both projects before submitting:

```powershell
dotnet build backend/GameOfDrones.Api/GameOfDrones.Api.csproj
cd frontend
npm run build
```
