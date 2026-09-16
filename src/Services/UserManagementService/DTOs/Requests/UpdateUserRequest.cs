using System.ComponentModel.DataAnnotations;

namespace UserManagementService.DTOs.Requests;

public class UpdateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
}
