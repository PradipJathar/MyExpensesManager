using System.ComponentModel.DataAnnotations;

namespace ExpenseManager.API.Dto
{
    public class CreateCategoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(7)]
        [RegularExpression(@"^#[0-9a-fA-F]{6}$", ErrorMessage = "Color code must be a valid hex color starting with # followed by 6 hex characters (e.g. #3B82F6).")]
        public string? ColorCode { get; set; }
    }
}
