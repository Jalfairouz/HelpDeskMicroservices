using TicketService.Models;

namespace TicketService.DTOs.Responses;

public class TicketResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketType Type { get; set; }

    public TicketCategory Category { get; set; }

    public TicketPriority Priority { get; set; }

    public TicketStatus Status { get; set; }

    public Guid CreatedByUserId { get; set; }

    public Guid? AssignedTechnicianId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }
}