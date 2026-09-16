namespace GameOfDrones.Api.Models;

public class Game
{
    public int Id { get; set; }

    public int Player1Id { get; set; }

    public int Player2Id { get; set; }

    public int Player1Score { get; set; }

    public int Player2Score { get; set; }

    public int? WinnerId { get; set; }

    public bool IsFinished { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}