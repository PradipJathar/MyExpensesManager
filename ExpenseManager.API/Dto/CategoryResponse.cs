using System;

namespace ExpenseManager.API.Dto
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public string ColorCode { get; set; } = "#3B82F6";
        public DateTime CreatedAt { get; set; }
    }
}
