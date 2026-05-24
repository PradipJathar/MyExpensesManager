namespace ExpenseManager.API.Dto
{
    public class MonthlyComparisonResponse
    {
        public decimal CurrentMonthExpenses { get; set; }
        public decimal PreviousMonthExpenses { get; set; }
        public decimal ExpenseChangePercentage { get; set; }
        public decimal CurrentMonthIncome { get; set; }
        public decimal PreviousMonthIncome { get; set; }
        public decimal IncomeChangePercentage { get; set; }
    }
}
