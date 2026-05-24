using System;

namespace ExpenseManager.API.Dto
{
    public class BudgetResponse
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public int AlertThreshold { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
