using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ExpenseManager.API.Data;
using ExpenseManager.API.Dto;
using ExpenseManager.API.Models;

namespace ExpenseManager.API.Services
{
    public class BudgetService
    {
        private readonly AppDbContext _context;

        public BudgetService(AppDbContext context)
        {
            _context = context;
        }

        private static BudgetResponse MapToResponse(Budget budget) => new()
        {
            Id = budget.Id,
            CategoryId = budget.CategoryId,
            CategoryName = budget.Category?.CategoryName ?? string.Empty,
            BudgetAmount = budget.BudgetAmount,
            PeriodMonth = budget.PeriodMonth,
            PeriodYear = budget.PeriodYear,
            AlertThreshold = budget.AlertThreshold,
            CreatedAt = budget.CreatedAt,
            UpdatedAt = budget.UpdatedAt
        };

        public async Task<IEnumerable<BudgetResponse>> GetAllBudgetsAsync(int userId)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.PeriodYear)
                .ThenByDescending(b => b.PeriodMonth)
                .ToListAsync();

            return budgets.Select(MapToResponse);
        }

        public async Task<BudgetResponse> GetBudgetByIdAsync(int id, int userId)
        {
            var budget = await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                throw new KeyNotFoundException("Budget not found or you do not have access to it.");
            }

            return MapToResponse(budget);
        }

        public async Task<BudgetResponse> CreateBudgetAsync(CreateBudgetRequest request, int userId)
        {
            // Verify category exists and belongs to user
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == userId);
            if (category == null)
            {
                throw new ArgumentException("Selected category is invalid.");
            }

            // Check if budget already exists for this category in the same month/year
            var duplicateExists = await _context.Budgets
                .AnyAsync(b => b.UserId == userId &&
                               b.CategoryId == request.CategoryId &&
                               b.PeriodMonth == request.PeriodMonth &&
                               b.PeriodYear == request.PeriodYear);

            if (duplicateExists)
            {
                throw new ArgumentException($"A budget already exists for category '{category.CategoryName}' in {request.PeriodMonth}/{request.PeriodYear}.");
            }

            var budget = new Budget
            {
                UserId = userId,
                CategoryId = request.CategoryId,
                BudgetAmount = request.BudgetAmount,
                PeriodMonth = request.PeriodMonth,
                PeriodYear = request.PeriodYear,
                AlertThreshold = request.AlertThreshold,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            // Populate Category details for response
            budget.Category = category;

            return MapToResponse(budget);
        }

        public async Task<BudgetResponse> UpdateBudgetAsync(int id, UpdateBudgetRequest request, int userId)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                throw new KeyNotFoundException("Budget not found or you do not have access to it.");
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == userId);
            if (category == null)
            {
                throw new ArgumentException("Selected category is invalid.");
            }

            // Check duplicate budgets excluding current one
            var duplicateExists = await _context.Budgets
                .AnyAsync(b => b.UserId == userId &&
                               b.Id != id &&
                               b.CategoryId == request.CategoryId &&
                               b.PeriodMonth == request.PeriodMonth &&
                               b.PeriodYear == request.PeriodYear);

            if (duplicateExists)
            {
                throw new ArgumentException($"A budget already exists for category '{category.CategoryName}' in {request.PeriodMonth}/{request.PeriodYear}.");
            }

            budget.CategoryId = request.CategoryId;
            budget.BudgetAmount = request.BudgetAmount;
            budget.PeriodMonth = request.PeriodMonth;
            budget.PeriodYear = request.PeriodYear;
            budget.AlertThreshold = request.AlertThreshold;
            budget.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            budget.Category = category;

            return MapToResponse(budget);
        }

        public async Task<bool> DeleteBudgetAsync(int id, int userId)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                throw new KeyNotFoundException("Budget not found or you do not have access to it.");
            }

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<BudgetStatusResponse>> GetBudgetStatusAsync(int userId)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.PeriodYear)
                .ThenByDescending(b => b.PeriodMonth)
                .ToListAsync();

            var statusList = new List<BudgetStatusResponse>();

            foreach (var budget in budgets)
            {
                var spentAmount = await GetCategorySpendingAsync(budget.CategoryId, budget.PeriodMonth, budget.PeriodYear, userId);
                var percentage = budget.BudgetAmount > 0 ? (spentAmount / budget.BudgetAmount) * 100 : 0;

                statusList.Add(new BudgetStatusResponse
                {
                    BudgetId = budget.Id,
                    CategoryId = budget.CategoryId,
                    CategoryName = budget.Category?.CategoryName ?? string.Empty,
                    CategoryColor = budget.Category?.ColorCode ?? string.Empty,
                    BudgetAmount = budget.BudgetAmount,
                    SpentAmount = spentAmount,
                    PeriodMonth = budget.PeriodMonth,
                    PeriodYear = budget.PeriodYear,
                    PercentageUsed = Math.Round(percentage, 2),
                    IsExceeded = spentAmount > budget.BudgetAmount,
                    IsAlert = percentage >= budget.AlertThreshold
                });
            }

            return statusList;
        }

        public async Task<decimal> GetCategorySpendingAsync(int categoryId, int month, int year, int userId)
        {
            return await _context.Expenses
                .Where(e => e.CategoryId == categoryId &&
                            e.UserId == userId &&
                            e.ExpenseDate.Month == month &&
                            e.ExpenseDate.Year == year)
                .SumAsync(e => e.Amount);
        }
    }
}
