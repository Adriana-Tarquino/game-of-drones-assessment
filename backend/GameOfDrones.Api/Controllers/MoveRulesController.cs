using GameOfDrones.Api.Data;
using GameOfDrones.Api.DTOs;
using GameOfDrones.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameOfDrones.Api.Controllers;

[ApiController]
[Route("api/move-rules")]
public class MoveRulesController : ControllerBase
{
    private readonly GameDbContext _context;

    public MoveRulesController(GameDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetRules()
    {
        var rules = await _context.MoveRules
            .Include(rule => rule.Move)
            .Include(rule => rule.BeatsMove)
            .Select(rule => new
            {
                rule.Id,
                Move = rule.Move.Name,
                Beats = rule.BeatsMove.Name
            })
            .ToListAsync();

        return Ok(rules);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRule(CreateMoveRuleDto dto)
    {
        if (dto.MoveId == dto.BeatsMoveId)
        {
            return BadRequest("A move cannot beat itself.");
        }

        var moveExists = await _context.Moves
            .AnyAsync(m => m.Id == dto.MoveId);

        var beatsMoveExists = await _context.Moves
            .AnyAsync(m => m.Id == dto.BeatsMoveId);

        if (!moveExists || !beatsMoveExists)
        {
            return BadRequest("One or both moves do not exist.");
        }

        var ruleExists = await _context.MoveRules
            .AnyAsync(r =>
                r.MoveId == dto.MoveId &&
                r.BeatsMoveId == dto.BeatsMoveId);

        if (ruleExists)
        {
            return Conflict("This rule already exists.");
        }

        var rule = new MoveRule
        {
            MoveId = dto.MoveId,
            BeatsMoveId = dto.BeatsMoveId
        };

        _context.MoveRules.Add(rule);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetRules),
            new { id = rule.Id },
            rule
        );
    }
}