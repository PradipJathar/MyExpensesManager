using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseManager.API.Models
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BudgetAmount { get; set; }

        [Required]
        public int PeriodMonth { get; set; } // 1-12

        [Required]
        public int PeriodYear { get; set; }

        [Required]
        public int AlertThreshold { get; set; } = 90; // percentage (e.g. 90)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
