using GameOfDrones.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameOfDrones.Api.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();

    public DbSet<Move> Moves => Set<Move>();

    public DbSet<MoveRule> MoveRules => Set<MoveRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Move>().HasData(
            new Move
            {
                Id = 1,
                Name = "Rock"
            },
            new Move
            {
                Id = 2,
                Name = "Paper"
            },
            new Move
            {
                Id = 3,
                Name = "Scissors"
            }
        );

        modelBuilder.Entity<MoveRule>().HasData(
            new MoveRule
            {
                Id = 1,
                MoveId = 2,
                BeatsMoveId = 1
            },
            new MoveRule
            {
                Id = 2,
                MoveId = 1,
                BeatsMoveId = 3
            },
            new MoveRule
            {
                Id = 3,
                MoveId = 3,
                BeatsMoveId = 2
            }
        );
    }
}