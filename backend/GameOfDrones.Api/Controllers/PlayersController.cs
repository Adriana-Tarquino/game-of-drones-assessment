using GameOfDrones.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameOfDrones.Api.Controllers;

[ApiController]
[Route("api/players")]
public class PlayersController : ControllerBase
{
    private readonly GameDbContext _context;

    public PlayersController(GameDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPlayers()
    {
        var players = await _context.Players
            .AsNoTracking()
            .OrderByDescending(player => player.GamesWon)
            .ThenBy(player => player.Name)
            .Select(player => new
            {
                player.Id,
                player.Name,
                player.GamesWon
            })
            .ToListAsync();

        return Ok(players);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPlayer(int id)
    {
        var player = await _context.Players
            .AsNoTracking()
            .Where(player => player.Id == id)
            .Select(player => new
            {
                player.Id,
                player.Name,
                player.GamesWon
            })
            .FirstOrDefaultAsync();

        return player is null ? NotFound("Player not found.") : Ok(player);
    }
}
