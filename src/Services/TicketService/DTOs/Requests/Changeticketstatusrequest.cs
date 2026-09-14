using System.ComponentModel.DataAnnotations;
using TicketService.Models;

namespace TicketService.DTOs.Requests;

public class ChangeTicketStatusRequest
{
    [Required]
    [EnumDataType(typeof(TicketStatus))]
    public TicketStatus Status { get; set; }
}
