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
    public class ExpenseService
    {
        private readonly AppDbContext _context;

        public Task<ExpenseResponse> GetExpenseResponse(Expense expense) => Task.FromResult(new ExpenseResponse
        {
            Id = expense.Id,
            CategoryId = expense.CategoryId,
            CategoryName = expense.Category?.CategoryName ?? string.Empty,
            CategoryColor = expense.Category?.ColorCode ?? string.Empty,
            AccountId = expense.AccountId,
            AccountName = expense.Account?.AccountName ?? string.Empty,
            Amount = expense.Amount,
            Description = expense.Description,
            ExpenseDate = expense.ExpenseDate,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        });

        public ExpenseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExpenseResponse>> GetAllExpensesAsync(int userId)
        {
            var expenses = await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.Account)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.ExpenseDate)
                .ThenByDescending(e => e.CreatedAt)
                .ToListAsync();

            var response = new List<ExpenseResponse>();
            foreach (var expense in expenses)
            {
                response.Add(await GetExpenseResponse(expense));
            }
            return response;
        }

        public async Task<ExpenseResponse> GetExpenseByIdAsync(int id, int userId)
        {
            var expense = await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                throw new KeyNotFoundException("Expense not found or you do not have permission to access it.");
            }

            return await GetExpenseResponse(expense);
        }

        public async Task<ExpenseResponse> CreateExpenseAsync(CreateExpenseRequest request, int userId)
        {
            // Validation: amount must be positive
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Expense amount must be greater than zero.");
            }

            // Validation: date not too far in the future (allow 1 day for timezone buffer)
            if (request.ExpenseDate > DateTime.UtcNow.AddDays(1))
            {
                throw new ArgumentException("Expense date cannot be in the future.");
            }

            // Validation: category belongs to user
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId && c.UserId == userId);
            if (!categoryExists)
            {
                throw new ArgumentException("Selected category is invalid or does not belong to the user.");
            }

            // Validation: account belongs to user
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId && a.UserId == userId);
            if (account == null)
            {
                throw new ArgumentException("Selected account is invalid or does not belong to the user.");
            }

            var expense = new Expense
            {
                UserId = userId,
                AccountId = request.AccountId,
                CategoryId = request.CategoryId,
                Amount = request.Amount,
                Description = request.Description?.Trim() ?? string.Empty,
                ExpenseDate = request.ExpenseDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Update account balance (subtract expense amount)
            account.CurrentBalance -= expense.Amount;
            account.UpdatedAt = DateTime.UtcNow;

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            // Fetch fully populated expense to return
            var createdExpense = await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.Account)
                .FirstAsync(e => e.Id == expense.Id);

            return await GetExpenseResponse(createdExpense);
        }

        public async Task<ExpenseResponse> UpdateExpenseAsync(int id, UpdateExpenseRequest request, int userId)
        {
            var expense = await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                throw new KeyNotFoundException("Expense not found or you do not have permission to access it.");
            }

            if (request.Amount <= 0)
            {
                throw new ArgumentException("Expense amount must be greater than zero.");
            }

            if (request.ExpenseDate > DateTime.UtcNow.AddDays(1))
            {
                throw new ArgumentException("Expense date cannot be in the future.");
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId && c.UserId == userId);
            if (!categoryExists)
            {
                throw new ArgumentException("Selected category is invalid.");
            }

            // Handle account balance adjustments
            if (expense.AccountId == request.AccountId)
            {
                // Account hasn't changed. Just adjust current balance by the difference.
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == expense.AccountId && a.UserId == userId);
                if (account == null)
                {
                    throw new ArgumentException("Associated account is invalid.");
                }

                // Balance adjustment = old expense amount - new expense amount
                account.CurrentBalance += (expense.Amount - request.Amount);
                account.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Account has changed. Add old expense back to old account, subtract new expense from new account.
                var oldAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == expense.AccountId && a.UserId == userId);
                if (oldAccount != null)
                {
                    oldAccount.CurrentBalance += expense.Amount;
                    oldAccount.UpdatedAt = DateTime.UtcNow;
                }

                var newAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId && a.UserId == userId);
                if (newAccount == null)
                {
                    throw new ArgumentException("Selected new account is invalid.");
                }

                newAccount.CurrentBalance -= request.Amount;
                newAccount.UpdatedAt = DateTime.UtcNow;
            }

            // Update details
            expense.AccountId = request.AccountId;
            expense.CategoryId = request.CategoryId;
            expense.Amount = request.Amount;
            expense.Description = request.Description?.Trim() ?? string.Empty;
            expense.ExpenseDate = request.ExpenseDate;
            expense.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Fetch fully populated expense to return
            var updatedExpense = await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.Account)
                .FirstAsync(e => e.Id == expense.Id);

            return await GetExpenseResponse(updatedExpense);
        }

        public async Task<bool> DeleteExpenseAsync(int id, int userId)
        {
            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
            if (expense == null)
            {
                throw new KeyNotFoundException("Expense not found or you do not have permission to delete it.");
            }

            // Refund the account balance
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == expense.AccountId && a.UserId == userId);
            if (account != null)
            {
                account.CurrentBalance += expense.Amount;
                account.UpdatedAt = DateTime.UtcNow;
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ExpenseResponse>> FilterExpensesAsync(
            int userId,
            DateTime? startDate,
            DateTime? endDate,
            int? categoryId,
            decimal? minAmount,
            decimal? maxAmount)
        {
            var query = _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.Account)
                .Where(e => e.UserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate <= endDate.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            if (minAmount.HasValue)
            {
                query = query.Where(e => e.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                query = query.Where(e => e.Amount <= maxAmount.Value);
            }

            var expenses = await query
                .OrderByDescending(e => e.ExpenseDate)
                .ThenByDescending(e => e.CreatedAt)
                .ToListAsync();

            var response = new List<ExpenseResponse>();
            foreach (var expense in expenses)
            {
                response.Add(await GetExpenseResponse(expense));
            }
            return response;
        }
    }
}
