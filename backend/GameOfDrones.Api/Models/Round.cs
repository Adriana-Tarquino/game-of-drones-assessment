namespace GameOfDrones.Api.Models;

public class Round
{
    public int Id { get; set; }

    public int GameId { get; set; }

    public int RoundNumber { get; set; }

    public int Player1MoveId { get; set; }

    public int Player2MoveId { get; set; }

    public int? WinnerId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}