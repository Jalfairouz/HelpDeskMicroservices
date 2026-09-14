using System.ComponentModel.DataAnnotations;
using TicketService.Domain;

namespace TicketService.DTOs.Requests;

public class CreateTicketRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TicketType Type { get; set; }

    [Required]
    public TicketCategory Category { get; set; }

    [Required]
    public TicketPriority Priority { get; set; }
}