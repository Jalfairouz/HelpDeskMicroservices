namespace UserManagementService.DTOs.Requests;

public class UserLookupRequest
{
    public List<Guid> Ids { get; set; } = new();
}