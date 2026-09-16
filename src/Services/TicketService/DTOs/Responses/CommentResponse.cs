namespace TicketService.DTOs.Responses;

public class CommentResponse
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string Content { get; set; } = "";
    public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
}