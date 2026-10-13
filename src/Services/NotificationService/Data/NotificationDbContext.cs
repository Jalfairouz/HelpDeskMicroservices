
using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace HelpDesk.NotificationService.Data;

public sealed class NotificationDbContext : DbContext
{
    public NotificationDbContext(
        DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Inbox> Inboxes => Set<Inbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);

            entity.Property(n => n.Title)
                .IsRequired();

            entity.Property(n => n.Message)
                .IsRequired();

            entity.HasIndex(n => n.EventId)
                .IsUnique();
        });

        modelBuilder.Entity<Inbox>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.HasOne(i => i.Notification)
                .WithMany(n => n.Inboxes)
                .HasForeignKey(i => i.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(i => new
            {
                i.NotificationId,
                i.UserId
            }).IsUnique();

            entity.HasIndex(i => new
            {
                i.UserId,
                i.IsRead
            });
        });
    
}
}
