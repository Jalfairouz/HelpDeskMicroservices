namespace TicketService.Models;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TicketId { get; set; }           
    public Guid AuthorUserId { get; set; }       
    public string Content { get; set; } = "";   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}