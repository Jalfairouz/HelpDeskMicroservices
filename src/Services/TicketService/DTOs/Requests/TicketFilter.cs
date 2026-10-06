using TicketService.Domain;

namespace TicketService.DTOs.Requests;

public class TicketFilter
{
    public TicketStatus? Status { get; set; }
    public bool? Unassigned { get; set; }
}