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

    public MoveRulesController(GameDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetRules()
    {
        var rules = await _context.MoveRules
            .AsNoTracking()
            .Include(rule => rule.Move)
            .Include(rule => rule.BeatsMove)
            .OrderBy(rule => rule.Id)
            .Select(rule => new
            {
                rule.Id,
                Move = rule.Move.Name,
                Beats = rule.BeatsMove.Name,
                rule.MoveId,
                rule.BeatsMoveId
            })
            .ToListAsync();
        return Ok(rules);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRule(int id)
    {
        var rule = await _context.MoveRules
            .AsNoTracking()
            .Include(rule => rule.Move)
            .Include(rule => rule.BeatsMove)
            .Where(rule => rule.Id == id)
            .Select(rule => new
            {
                rule.Id,
                Move = rule.Move.Name,
                Beats = rule.BeatsMove.Name,
                rule.MoveId,
                rule.BeatsMoveId
            })
            .FirstOrDefaultAsync();
        return rule is null ? NotFound("Rule not found.") : Ok(rule);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRule(CreateMoveRuleDto dto)
    {
        var validationError = await ValidateRule(dto.MoveId, dto.BeatsMoveId);
        if (validationError is not null)
        {
            return validationError;
        }

        var rule = new MoveRule { MoveId = dto.MoveId, BeatsMoveId = dto.BeatsMoveId };
        _context.MoveRules.Add(rule);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRule), new { id = rule.Id }, new
        {
            rule.Id,
            MoveId = rule.MoveId,
            BeatsMoveId = rule.BeatsMoveId
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateRule(int id, UpdateMoveRuleDto dto)
    {
        var rule = await _context.MoveRules.FindAsync(id);
        if (rule is null)
        {
            return NotFound("Rule not found.");
        }

        var validationError = await ValidateRule(dto.MoveId, dto.BeatsMoveId, id);
        if (validationError is not null)
        {
            return validationError;
        }

        rule.MoveId = dto.MoveId;
        rule.BeatsMoveId = dto.BeatsMoveId;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRule(int id)
    {
        var rule = await _context.MoveRules.FindAsync(id);
        if (rule is null)
        {
            return NotFound("Rule not found.");
        }

        _context.MoveRules.Remove(rule);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<IActionResult?> ValidateRule(int moveId, int beatsMoveId, int? excludedId = null)
    {
        if (moveId == beatsMoveId)
        {
            return BadRequest("A move cannot beat itself.");
        }

        var movesExist = await _context.Moves.CountAsync(move =>
            move.Id == moveId || move.Id == beatsMoveId) == 2;
        if (!movesExist)
        {
            return BadRequest("One or both moves do not exist.");
        }

        var duplicateExists = await _context.MoveRules.AnyAsync(rule =>
            rule.MoveId == moveId && rule.BeatsMoveId == beatsMoveId &&
            (!excludedId.HasValue || rule.Id != excludedId.Value));
        return duplicateExists ? Conflict("This rule already exists.") : null;
    }

}
