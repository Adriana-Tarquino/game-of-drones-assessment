namespace GameOfDrones.Api.Models;

public class MoveRule
{
    public int Id { get; set; }

    public int MoveId { get; set; }

    public int BeatsMoveId { get; set; }

    public Move Move { get; set; } = null!;

    public Move BeatsMove { get; set; } = null!;
}