using GameOfDrones.Api.Data;
using GameOfDrones.Api.DTOs;
using GameOfDrones.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameOfDrones.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovesController : ControllerBase
{
    private readonly GameDbContext _context;

    public MovesController(GameDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Move>>> GetMoves()
    {
        var moves = await _context.Moves
            .OrderBy(move => move.Id)
            .ToListAsync();

        return Ok(moves);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<Move>> GetMove(int id)
    {
        var move = await _context.Moves.FindAsync(id);

        if (move == null)
        {
            return NotFound();
        }

        return Ok(move);
    }


    [HttpPost]
    public async Task<ActionResult<Move>> CreateMove(CreateMoveDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Move name is required.");
        }

        var exists = await _context.Moves
            .AnyAsync(move =>
                move.Name.ToLower() == dto.Name.ToLower());

        if (exists)
        {
            return Conflict(
                "A move with this name already exists."
            );
        }

        var move = new Move
        {
            Name = dto.Name.Trim()
        };

        _context.Moves.Add(move);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMove),
            new { id = move.Id },
            move
        );
    }
}