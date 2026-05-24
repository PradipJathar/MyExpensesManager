using System;
using System.ComponentModel.DataAnnotations;

namespace ExpenseManager.API.Dto
{
    public class CreateIncomeRequest
    {
        [Required]
        public int AccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Income amount must be a positive number greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(200)]
        public string Source { get; set; } = string.Empty;

        [Required]
        public DateTime IncomeDate { get; set; }
    }
}
