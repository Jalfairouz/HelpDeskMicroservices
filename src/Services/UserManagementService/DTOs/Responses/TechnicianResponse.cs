namespace UserManagementService.DTOs.Responses;

public class TechnicianResponse
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}