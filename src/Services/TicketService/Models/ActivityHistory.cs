namespace TicketService.Models;

public class ActivityHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TicketId { get; set; }              
    public Guid PerformedByUserId { get; set; }     
    public string ActivityType { get; set; } = "";  
    public string? OldValue { get; set; }          
    public string? NewValue { get; set; }          
    public DateTime CreatedAt { get; set; } =       
        DateTime.UtcNow;
}