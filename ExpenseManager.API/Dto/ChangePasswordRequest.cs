using System.ComponentModel.DataAnnotations;

namespace ExpenseManager.API.Dto
{
    public class ChangePasswordRequest
    {
        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
