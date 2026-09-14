namespace TicketService.Models;

public enum ActionType
{
    Created,           
    StatusChanged,
    Assigned,        
    AssignedBySystem, 
    Updated,           
    Closed,          
    CommentAdded      
}