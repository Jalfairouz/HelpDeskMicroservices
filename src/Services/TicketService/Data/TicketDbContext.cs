using Microsoft.EntityFrameworkCore;
using TicketService.Models;

namespace TicketService.Data;

public class TicketDbContext : DbContext
{
    public TicketDbContext(
        DbContextOptions<TicketDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<TicketHistory> TicketHistories => Set<TicketHistory>();
    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(ticket => ticket.Id);

            entity.Property(ticket => ticket.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(ticket => ticket.Description)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(ticket => ticket.Type)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(ticket => ticket.Category)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(ticket => ticket.Priority)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(ticket => ticket.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasIndex(ticket => ticket.CreatedByUserId);

            entity.HasIndex(ticket => new
            {
                ticket.AssignedTechnicianId,
                ticket.Status
            });
        });
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(comment => comment.Id);

            entity.Property(comment => comment.Content)
                .IsRequired()
                .HasMaxLength(2000);

            entity.HasIndex(comment => comment.TicketId);

            entity.HasIndex(comment => comment.AuthorUserId);
        });
        
        modelBuilder.Entity<TicketHistory>(entity =>  
        {
            entity.HasKey(history => history.Id);

            entity.Property(history => history.ActionType)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasIndex(history => history.TicketId);
        });
    }
}