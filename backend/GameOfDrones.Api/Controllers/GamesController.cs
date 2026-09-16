using GameOfDrones.Api.Data;
using GameOfDrones.Api.DTOs;
using GameOfDrones.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameOfDrones.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly GameDbContext _context;

    public GamesController(GameDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetGames()
    {
        var games = await (
            from game in _context.Games.AsNoTracking()
            join player1 in _context.Players.AsNoTracking() on game.Player1Id equals player1.Id
            join player2 in _context.Players.AsNoTracking() on game.Player2Id equals player2.Id
            orderby game.CreatedAt descending
            select new
            {
                game.Id,
                Player1 = player1.Name,
                Player2 = player2.Name,
                game.Player1Score,
                game.Player2Score,
                game.IsFinished,
                game.CreatedAt
            }).ToListAsync();

        return Ok(games);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGame(CreateGameDto dto)
    {
        var player1Name = dto.Player1Name.Trim();
        var player2Name = dto.Player2Name.Trim();

        if (string.IsNullOrWhiteSpace(player1Name) ||
            string.IsNullOrWhiteSpace(player2Name))
        {
            return BadRequest("Both player names are required.");
        }

        if (player1Name.Equals(
            player2Name,
            StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Players must have different names.");
        }

        var player1 = await GetOrCreatePlayer(player1Name);
        var player2 = await GetOrCreatePlayer(player2Name);

        var game = new Game
        {
            Player1Id = player1.Id,
            Player2Id = player2.Id,
            Player1Score = 0,
            Player2Score = 0,
            IsFinished = false
        };

        _context.Games.Add(game);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetGame),
            new { id = game.Id },
            new
            {
                game.Id,
                Player1 = player1.Name,
                Player2 = player2.Name,
                game.Player1Score,
                game.Player2Score,
                game.IsFinished
            }
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGame(int id)
    {
        var game = await _context.Games.FindAsync(id);

        if (game == null)
        {
            return NotFound("Game not found.");
        }

        var player1 = await _context.Players.FindAsync(game.Player1Id);
        var player2 = await _context.Players.FindAsync(game.Player2Id);

        var rounds = await _context.Rounds
            .Where(r => r.GameId == id)
            .OrderBy(r => r.RoundNumber)
            .ToListAsync();

        string? winner = null;

        if (game.WinnerId.HasValue)
        {
            var winnerPlayer =
                await _context.Players.FindAsync(game.WinnerId.Value);

            winner = winnerPlayer?.Name;
        }

        return Ok(new
        {
            game.Id,
            Player1 = player1?.Name,
            Player2 = player2?.Name,
            game.Player1Score,
            game.Player2Score,
            Winner = winner,
            game.IsFinished,
            Rounds = rounds
        });
        
    }

    [HttpPost("{id}/rounds")]
public async Task<IActionResult> PlayRound(
    int id,
    PlayRoundDto dto)
{
    var game = await _context.Games.FindAsync(id);

    if (game == null)
    {
        return NotFound("Game not found.");
    }

    if (game.IsFinished)
    {
        return BadRequest(
            "This game has already finished."
        );
    }

    var player1Move =
        await _context.Moves.FindAsync(dto.Player1MoveId);

    var player2Move =
        await _context.Moves.FindAsync(dto.Player2MoveId);

    if (player1Move == null || player2Move == null)
    {
        return BadRequest(
            "One or both moves do not exist."
        );
    }

    var player1Wins = await _context.MoveRules
        .AnyAsync(rule =>
            rule.MoveId == dto.Player1MoveId &&
            rule.BeatsMoveId == dto.Player2MoveId);

    var player2Wins = await _context.MoveRules
        .AnyAsync(rule =>
            rule.MoveId == dto.Player2MoveId &&
            rule.BeatsMoveId == dto.Player1MoveId);

    int? roundWinnerId = null;

    if (player1Wins && !player2Wins)
    {
        game.Player1Score++;
        roundWinnerId = game.Player1Id;
    }
    else if (player2Wins && !player1Wins)
    {
        game.Player2Score++;
        roundWinnerId = game.Player2Id;
    }

    var roundNumber =
        await _context.Rounds
            .CountAsync(r => r.GameId == id)
        + 1;

    var round = new Round
    {
        GameId = game.Id,
        RoundNumber = roundNumber,
        Player1MoveId = dto.Player1MoveId,
        Player2MoveId = dto.Player2MoveId,
        WinnerId = roundWinnerId
    };

    _context.Rounds.Add(round);

    if (game.Player1Score >= 3)
    {
        await FinishGame(game, game.Player1Id);
    }
    else if (game.Player2Score >= 3)
    {
        await FinishGame(game, game.Player2Id);
    }

    await _context.SaveChangesAsync();

    string? roundWinner = null;

    if (roundWinnerId.HasValue)
    {
        var player =
            await _context.Players.FindAsync(roundWinnerId.Value);

        roundWinner = player?.Name;
    }

    string? gameWinner = null;

    if (game.WinnerId.HasValue)
    {
        var player =
            await _context.Players.FindAsync(game.WinnerId.Value);

        gameWinner = player?.Name;
    }

    return Ok(new
    {
        Round = roundNumber,
        Player1Move = player1Move.Name,
        Player2Move = player2Move.Name,
        RoundWinner = roundWinner,
        Player1Score = game.Player1Score,
        Player2Score = game.Player2Score,
        GameFinished = game.IsFinished,
        GameWinner = gameWinner
    });
}
private async Task FinishGame(Game game, int winnerId)
{
    game.IsFinished = true;
    game.WinnerId = winnerId;

    var winner =
        await _context.Players.FindAsync(winnerId);

    if (winner != null)
    {
        winner.GamesWon++;
    }
}

    private async Task<Player> GetOrCreatePlayer(string name)
    {
        var player = await _context.Players
            .FirstOrDefaultAsync(p =>
                p.Name.ToLower() == name.ToLower());

        if (player != null)
        {
            return player;
        }

        player = new Player
        {
            Name = name,
            GamesWon = 0
        };

        _context.Players.Add(player);

        await _context.SaveChangesAsync();

        return player;
    }
    
}
