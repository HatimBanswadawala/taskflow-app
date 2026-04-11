using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Data;

/// <summary>
/// EF Core DbContext — this is the bridge between our C# entities and the database.
/// We're using InMemory provider so no real DB needed. Data resets on app restart.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Each DbSet = one "table" in the database
    public DbSet<User> Users => Set<User>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Column> Columns => Set<Column>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User — email must be unique
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.Property(u => u.PasswordHash).IsRequired();
        });

        // Board — belongs to a User, cascade delete (delete user = delete their boards)
        modelBuilder.Entity<Board>(entity =>
        {
            entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
            entity.HasOne(b => b.User)
                  .WithMany(u => u.Boards)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Column — belongs to a Board, ordered by Position
        modelBuilder.Entity<Column>(entity =>
        {
            entity.Property(c => c.Name).IsRequired().HasMaxLength(50);
            entity.HasOne(c => c.Board)
                  .WithMany(b => b.Columns)
                  .HasForeignKey(c => c.BoardId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TaskItem — belongs to a Column, optionally assigned to a User
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).HasMaxLength(1000);

            entity.HasOne(t => t.Column)
                  .WithMany(c => c.Tasks)
                  .HasForeignKey(t => t.ColumnId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.AssignedTo)
                  .WithMany(u => u.AssignedTasks)
                  .HasForeignKey(t => t.AssignedToId)
                  .OnDelete(DeleteBehavior.SetNull); // Don't delete task if user is deleted
        });
    }
}
