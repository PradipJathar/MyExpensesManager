using System.ComponentModel.DataAnnotations;

namespace ExpenseManager.API.Dto
{
    public class UpdateBudgetRequest
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be a positive number greater than zero.")]
        public decimal BudgetAmount { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
        public int PeriodMonth { get; set; }

        [Required]
        [Range(2000, 2100, ErrorMessage = "Year must be a valid four-digit year (between 2000 and 2100).")]
        public int PeriodYear { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Alert threshold must be between 0 and 100 percent.")]
        public int AlertThreshold { get; set; } = 90;
    }
}
