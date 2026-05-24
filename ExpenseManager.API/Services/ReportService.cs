using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ExpenseManager.API.Data;
using ExpenseManager.API.Dto;
using ExpenseManager.API.Models;

namespace ExpenseManager.API.Services
{
    public class ReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        private static decimal CalculateChangePercentage(decimal previous, decimal current)
        {
            if (previous == 0)
            {
                return current > 0 ? 100 : 0;
            }
            return Math.Round(((current - previous) / previous) * 100, 2);
        }

        public async Task<SummaryResponse> GetMonthlySummaryAsync(int userId, int month, int year)
        {
            var totalIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.IncomeDate.Month == month && i.IncomeDate.Year == year)
                .SumAsync(i => i.Amount);

            var totalExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.ExpenseDate.Month == month && e.ExpenseDate.Year == year)
                .SumAsync(e => e.Amount);

            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId && b.PeriodMonth == month && b.PeriodYear == year)
                .ToListAsync();

            var totalBudgeted = budgets.Sum(b => b.BudgetAmount);
            decimal totalBudgetSpent = 0;

            foreach (var budget in budgets)
            {
                var spentForCategory = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                                e.CategoryId == budget.CategoryId &&
                                e.ExpenseDate.Month == month &&
                                e.ExpenseDate.Year == year)
                    .SumAsync(e => e.Amount);

                totalBudgetSpent += spentForCategory;
            }

            return new SummaryResponse
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetAmount = totalIncome - totalExpenses,
                TotalBudgeted = totalBudgeted,
                TotalBudgetSpent = totalBudgetSpent
            };
        }

        public async Task<IEnumerable<TrendResponse>> GetYearlySummaryAsync(int userId, int year)
        {
            var yearlyData = new List<TrendResponse>();

            for (int month = 1; month <= 12; month++)
            {
                var totalIncome = await _context.Incomes
                    .Where(i => i.UserId == userId && i.IncomeDate.Month == month && i.IncomeDate.Year == year)
                    .SumAsync(i => i.Amount);

                var totalExpenses = await _context.Expenses
                    .Where(e => e.UserId == userId && e.ExpenseDate.Month == month && e.ExpenseDate.Year == year)
                    .SumAsync(e => e.Amount);

                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

                yearlyData.Add(new TrendResponse
                {
                    Label = $"{monthName.Substring(0, 3)} {year}",
                    Month = month,
                    Year = year,
                    Income = totalIncome,
                    Expense = totalExpenses,
                    Net = totalIncome - totalExpenses
                });
            }

            return yearlyData;
        }

        public async Task<IEnumerable<CategoryBreakdownResponse>> GetCategoryBreakdownAsync(int userId, int month, int year)
        {
            var totalExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.ExpenseDate.Month == month && e.ExpenseDate.Year == year)
                .SumAsync(e => e.Amount);

            if (totalExpenses == 0)
            {
                return Enumerable.Empty<CategoryBreakdownResponse>();
            }

            var groupedExpenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && e.ExpenseDate.Month == month && e.ExpenseDate.Year == year)
                .GroupBy(e => new { e.CategoryId, CategoryName = e.Category != null ? e.Category.CategoryName : "Uncategorized", ColorCode = e.Category != null ? e.Category.ColorCode : "#6B7280" })
                .Select(g => new
                {
                    g.Key.CategoryName,
                    g.Key.ColorCode,
                    TotalAmount = g.Sum(e => e.Amount)
                })
                .ToListAsync();

            return groupedExpenses.Select(g => new CategoryBreakdownResponse
            {
                CategoryName = g.CategoryName,
                ColorCode = g.ColorCode,
                Amount = g.TotalAmount,
                Percentage = Math.Round((g.TotalAmount / totalExpenses) * 100, 2)
            }).OrderByDescending(c => c.Amount);
        }

        public async Task<MonthlyComparisonResponse> GetMonthlyComparisonAsync(int userId, int month, int year)
        {
            // Current month data
            var curIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.IncomeDate.Month == month && i.IncomeDate.Year == year)
                .SumAsync(i => i.Amount);

            var curExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.ExpenseDate.Month == month && e.ExpenseDate.Year == year)
                .SumAsync(e => e.Amount);

            // Previous month calculation
            var prevDate = new DateTime(year, month, 1).AddMonths(-1);
            var prevMonth = prevDate.Month;
            var prevYear = prevDate.Year;

            var prevIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.IncomeDate.Month == prevMonth && i.IncomeDate.Year == prevYear)
                .SumAsync(i => i.Amount);

            var prevExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.ExpenseDate.Month == prevMonth && e.ExpenseDate.Year == prevYear)
                .SumAsync(e => e.Amount);

            return new MonthlyComparisonResponse
            {
                CurrentMonthExpenses = curExpenses,
                PreviousMonthExpenses = prevExpenses,
                ExpenseChangePercentage = CalculateChangePercentage(prevExpenses, curExpenses),
                CurrentMonthIncome = curIncome,
                PreviousMonthIncome = prevIncome,
                IncomeChangePercentage = CalculateChangePercentage(prevIncome, curIncome)
            };
        }

        public async Task<IEnumerable<TrendResponse>> GetSpendingTrendsAsync(int userId, int monthsCount)
        {
            var trends = new List<TrendResponse>();
            var today = DateTime.UtcNow;
            
            // Loop backwards N months and calculate
            for (int i = monthsCount - 1; i >= 0; i--)
            {
                var targetDate = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
                var m = targetDate.Month;
                var y = targetDate.Year;

                var totalIncome = await _context.Incomes
                    .Where(i => i.UserId == userId && i.IncomeDate.Month == m && i.IncomeDate.Year == y)
                    .SumAsync(i => i.Amount);

                var totalExpenses = await _context.Expenses
                    .Where(e => e.UserId == userId && e.ExpenseDate.Month == m && e.ExpenseDate.Year == y)
                    .SumAsync(e => e.Amount);

                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m);

                trends.Add(new TrendResponse
                {
                    Label = $"{monthName.Substring(0, 3)} {y}",
                    Month = m,
                    Year = y,
                    Income = totalIncome,
                    Expense = totalExpenses,
                    Net = totalIncome - totalExpenses
                });
            }

            return trends;
        }

        public async Task<IEnumerable<BudgetVsActualResponse>> GetBudgetVsActualAsync(int userId, int month, int year)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId && b.PeriodMonth == month && b.PeriodYear == year)
                .ToListAsync();

            var list = new List<BudgetVsActualResponse>();

            foreach (var budget in budgets)
            {
                var spentAmount = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                                e.CategoryId == budget.CategoryId &&
                                e.ExpenseDate.Month == month &&
                                e.ExpenseDate.Year == year)
                    .SumAsync(e => e.Amount);

                var percentage = budget.BudgetAmount > 0 ? (spentAmount / budget.BudgetAmount) * 100 : 0;

                list.Add(new BudgetVsActualResponse
                {
                    CategoryId = budget.CategoryId,
                    CategoryName = budget.Category?.CategoryName ?? "Unknown",
                    CategoryColor = budget.Category?.ColorCode ?? "#3B82F6",
                    BudgetAmount = budget.BudgetAmount,
                    SpentAmount = spentAmount,
                    PercentageUsed = Math.Round(percentage, 2),
                    IsExceeded = spentAmount > budget.BudgetAmount
                });
            }

            return list;
        }
    }
}
