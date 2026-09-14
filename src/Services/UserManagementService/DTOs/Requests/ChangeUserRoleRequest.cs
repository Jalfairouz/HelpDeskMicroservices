using System.ComponentModel.DataAnnotations;

namespace UserManagementService.DTOs.Requests;

public class ChangeUserRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}