namespace TicketService.Domain;

public enum ActionType
{
    Created,
    Updated,
    StatusChanged,
    Assigned,
    AssignedBySystem,
    Closed,
    CommentAdded
}