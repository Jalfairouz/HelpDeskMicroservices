using TicketService.Domain;

namespace TicketService.DTOs.Responses;

public class TicketHistoryResponse
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public string PerformedByUserId { get; set; } = ""; 

    public ActionType ActionType { get; set; }

    public DateTime CreatedAt { get; set; }
}