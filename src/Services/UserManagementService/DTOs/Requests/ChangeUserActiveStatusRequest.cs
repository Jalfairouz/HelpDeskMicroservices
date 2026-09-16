using System.ComponentModel.DataAnnotations;

namespace UserManagementService.DTOs.Requests;

public class ChangeUserActiveStatusRequest
{
    [Required]
    public bool? IsActive { get; set; }
}