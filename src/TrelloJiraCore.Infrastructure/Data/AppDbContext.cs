using Microsoft.EntityFrameworkCore;
using TrelloJiraCore.Core.Entities;

namespace TrelloJiraCore.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Board> Boards { get; set; }
    public DbSet<List> Lists { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<BoardMember> BoardMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Board>()
            .HasMany(b => b.Lists)
            .WithOne(l => l.Board)
            .HasForeignKey(l => l.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<List>()
            .HasMany(l => l.Cards)
            .WithOne(c => c.List)
            .HasForeignKey(c => c.ListId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ActivityLog>()
            .HasOne(a => a.Board)
            .WithMany()
            .HasForeignKey(a => a.BoardId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<BoardMember>()
            .HasOne(bm => bm.Board)
            .WithMany(b => b.Members)
            .HasForeignKey(bm => bm.BoardId);

        modelBuilder.Entity<BoardMember>()
            .HasOne(bm => bm.User)
            .WithMany()
            .HasForeignKey(bm => bm.UserId);
    }
}
