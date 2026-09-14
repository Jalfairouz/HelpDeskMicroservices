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
    }
}