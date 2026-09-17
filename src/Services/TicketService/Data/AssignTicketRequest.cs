using System.ComponentModel.DataAnnotations;

namespace TicketService.DTOs.Requests;

public class AssignTicketRequest
{
    [Required]
    public Guid? TechnicianId { get; set; }
}