using System.ComponentModel.DataAnnotations;

namespace ExpenseManager.API.Dto
{
    public class UpdateAccountRequest
    {
        [Required]
        [MaxLength(100)]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(Bank|CreditCard|Cash)$", ErrorMessage = "Account type must be one of: Bank, CreditCard, Cash.")]
        public string AccountType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? AccountNumber { get; set; }

        [Required]
        public decimal InitialBalance { get; set; } // Representing the updated initial balance
    }
}
