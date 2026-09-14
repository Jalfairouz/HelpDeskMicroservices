using TicketService.Domain;

namespace TicketService.Models;

public class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketType Type { get; set; }

    public TicketCategory Category { get; set; }

    public TicketPriority Priority { get; set; }

    public TicketStatus Status { get; set; } =
        TicketStatus.Open;

    public Guid CreatedByUserId { get; set; }

    public Guid? AssignedTechnicianId { get; set; }

    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }
}