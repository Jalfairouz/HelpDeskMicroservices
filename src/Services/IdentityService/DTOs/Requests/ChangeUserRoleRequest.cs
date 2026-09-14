using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs.Requests;

public class ChangeUserRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}