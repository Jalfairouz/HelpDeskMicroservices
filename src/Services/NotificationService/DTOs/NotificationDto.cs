namespace NotificationService.DTOs;

public sealed record NotificationDto(
    Guid Id,            
    Guid NotificationId,
    Guid TicketId,
    string Title,
    string Message,
    DateTime CreatedAt,
    bool IsRead,
    DateTime? ReadAt
    );