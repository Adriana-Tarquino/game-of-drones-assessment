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
            .OrderByDescending(player => player.GamesWon)
            .Select(player => new
            {
                player.Id,
                player.Name,
                player.GamesWon
            })
            .ToListAsync();

        return Ok(players);
    }
}