namespace ExpenseManager.API.Dto
{
    public class TrendResponse
    {
        public string Label { get; set; } = string.Empty; // e.g. "May 2026"
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Net { get; set; }
    }
}
