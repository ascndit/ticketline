using Microsoft.EntityFrameworkCore;
using Ticketline.Api.Models;

namespace Ticketline.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Name).HasMaxLength(100);
            entity.Property(user => user.Email).HasMaxLength(255);
            entity.Property(user => user.PasswordHash).HasMaxLength(500);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.Property(ticket => ticket.Title).HasMaxLength(150);
            entity.Property(ticket => ticket.Status).HasMaxLength(20);
            entity.HasOne(ticket => ticket.Owner)
                .WithMany(user => user.Tickets)
                .HasForeignKey(ticket => ticket.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
