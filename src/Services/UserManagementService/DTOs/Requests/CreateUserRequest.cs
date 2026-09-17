using System.ComponentModel.DataAnnotations;

public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string Role { get; set; } = string.Empty;
}