
namespace NotificationService.Models;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EventId { get; set; }
    public string Audience { get; set; } = string.Empty;


    public Guid TicketId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Inbox> Inboxes { get; set; }
        = new List<Inbox>();
}