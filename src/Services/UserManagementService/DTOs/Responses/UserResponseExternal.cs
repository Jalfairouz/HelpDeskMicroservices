namespace UserManagementService.DTOs.External;

public class UserResponseExternal
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string? Email { get; set; } = string.Empty;
}