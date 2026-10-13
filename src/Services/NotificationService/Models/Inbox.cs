using NotificationService.Models;

public class Inbox
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;

    public Notification Notification { get; set; } = null!;
}