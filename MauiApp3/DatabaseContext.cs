using Microsoft.EntityFrameworkCore;

namespace MauiApp3;

public class DatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<SharedList> SharedLists { get; set; }
    public DbSet<SharedListAccess> SharedListAccesses { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка индексов
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<SharedList>()
            .HasIndex(sl => sl.ListId);

        // Настройка отношений
        modelBuilder.Entity<SharedList>()
            .HasOne(sl => sl.Owner)
            .WithMany(u => u.OwnedLists)
            .HasForeignKey(sl => sl.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SharedListAccess>()
            .HasOne(sla => sla.SharedList)
            .WithMany(sl => sl.SharedAccesses)
            .HasForeignKey(sla => sla.ListId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SharedListAccess>()
            .HasOne(sla => sla.User)
            .WithMany(u => u.SharedAccesses)
            .HasForeignKey(sla => sla.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Составной индекс для предотвращения дублирования доступа
        modelBuilder.Entity<SharedListAccess>()
            .HasIndex(sla => new { sla.ListId, sla.UserId })
            .IsUnique();
    }
}