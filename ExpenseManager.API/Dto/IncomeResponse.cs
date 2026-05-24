using System;

namespace ExpenseManager.API.Dto
{
    public class IncomeResponse
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime IncomeDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
