using TicketService.Domain;

namespace TicketService.DTOs.Requests
{
    public class AdminTicketFilter
    {
        public TicketStatus? Status { get; set; }
        public bool? Unassigned { get; set; }
        public bool? Mine { get; set; }
    }
}