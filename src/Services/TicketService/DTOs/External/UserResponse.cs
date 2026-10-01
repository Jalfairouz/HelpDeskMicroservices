namespace TicketService.DTOs.External;

public class UserResponse
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string FullName => $"{FirstName} {LastName}".Trim();
}