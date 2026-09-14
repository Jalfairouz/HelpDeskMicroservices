using TicketService.Domain;
namespace TicketService.Models;


public class TicketHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TicketId { get; set; }
    public Guid PerformedByUserId { get; set; }
    public ActionType ActionType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}