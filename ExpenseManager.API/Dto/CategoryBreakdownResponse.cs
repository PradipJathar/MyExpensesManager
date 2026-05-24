namespace ExpenseManager.API.Dto
{
    public class CategoryBreakdownResponse
    {
        public string CategoryName { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }
}
