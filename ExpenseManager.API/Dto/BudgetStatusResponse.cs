namespace ExpenseManager.API.Dto
{
    public class BudgetStatusResponse
    {
        public int BudgetId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public decimal PercentageUsed { get; set; }
        public bool IsExceeded { get; set; }
        public bool IsAlert { get; set; }
    }
}
