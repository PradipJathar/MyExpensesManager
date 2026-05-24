namespace ExpenseManager.API.Dto
{
    public class SummaryResponse
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalBudgeted { get; set; }
        public decimal TotalBudgetSpent { get; set; }
    }
}
