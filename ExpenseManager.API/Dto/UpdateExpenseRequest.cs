using System;
using System.ComponentModel.DataAnnotations;

namespace ExpenseManager.API.Dto
{
    public class UpdateExpenseRequest
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int AccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be a positive number greater than zero.")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime ExpenseDate { get; set; }
    }
}
