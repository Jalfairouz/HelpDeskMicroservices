using System.ComponentModel.DataAnnotations;

namespace TicketService.DTOs.Requests;

public class AddCommentRequest
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = "";
}