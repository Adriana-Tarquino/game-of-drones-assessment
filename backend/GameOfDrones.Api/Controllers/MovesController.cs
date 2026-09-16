using GameOfDrones.Api.Data;
using GameOfDrones.Api.DTOs;
using GameOfDrones.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameOfDrones.Api.Controllers;

[ApiController]
[Route("api/moves")]
public class MovesController : ControllerBase
{
    private readonly GameDbContext _context;

    public MovesController(GameDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Move>>> GetMoves()
    {
        var moves = await _context.Moves
            .AsNoTracking()
            .OrderBy(move => move.Id)
            .ToListAsync();

        return Ok(moves);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Move>> GetMove(int id)
    {
        var move = await _context.Moves.AsNoTracking().FirstOrDefaultAsync(move => move.Id == id);
        return move is null ? NotFound("Move not found.") : Ok(move);
    }

    [HttpPost]
    public async Task<ActionResult<Move>> CreateMove(CreateMoveDto dto)
    {
        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Move name is required.");
        }

        if (await NameExists(name))
        {
            return Conflict("A move with this name already exists.");
        }

        var move = new Move { Name = name };
        _context.Moves.Add(move);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMove), new { id = move.Id }, move);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Move>> UpdateMove(int id, UpdateMoveDto dto)
    {
        var move = await _context.Moves.FindAsync(id);
        if (move is null)
        {
            return NotFound("Move not found.");
        }

        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Move name is required.");
        }

        if (await NameExists(name, id))
        {
            return Conflict("A move with this name already exists.");
        }

        move.Name = name;
        await _context.SaveChangesAsync();
        return Ok(move);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMove(int id)
    {
        var move = await _context.Moves.FindAsync(id);
        if (move is null)
        {
            return NotFound("Move not found.");
        }

        var isUsed = await _context.MoveRules.AnyAsync(rule =>
                rule.MoveId == id || rule.BeatsMoveId == id)
            || await _context.Rounds.AnyAsync(round =>
                round.Player1MoveId == id || round.Player2MoveId == id);

        if (isUsed)
        {
            return Conflict("This move cannot be deleted because it is used by a rule or a played round.");
        }

        _context.Moves.Remove(move);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private Task<bool> NameExists(string name, int? excludedId = null) =>
        _context.Moves.AnyAsync(move =>
            move.Name.ToLower() == name.ToLower() &&
            (!excludedId.HasValue || move.Id != excludedId.Value));
}
