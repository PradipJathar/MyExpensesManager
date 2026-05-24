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
    public class IncomeService
    {
        private readonly AppDbContext _context;

        public IncomeService(AppDbContext context)
        {
            _context = context;
        }

        private static IncomeResponse MapToResponse(Income income) => new()
        {
            Id = income.Id,
            AccountId = income.AccountId,
            AccountName = income.Account?.AccountName ?? string.Empty,
            Amount = income.Amount,
            Source = income.Source,
            IncomeDate = income.IncomeDate,
            CreatedAt = income.CreatedAt,
            UpdatedAt = income.UpdatedAt
        };

        public async Task<IEnumerable<IncomeResponse>> GetAllIncomeAsync(int userId)
        {
            var incomes = await _context.Incomes
                .Include(i => i.Account)
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.IncomeDate)
                .ThenByDescending(i => i.CreatedAt)
                .ToListAsync();

            return incomes.Select(MapToResponse);
        }

        public async Task<IncomeResponse> GetIncomeByIdAsync(int id, int userId)
        {
            var income = await _context.Incomes
                .Include(i => i.Account)
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
            {
                throw new KeyNotFoundException("Income record not found or you do not have access to it.");
            }

            return MapToResponse(income);
        }

        public async Task<IncomeResponse> CreateIncomeAsync(CreateIncomeRequest request, int userId)
        {
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Income amount must be greater than zero.");
            }

            if (request.IncomeDate > DateTime.UtcNow.AddDays(1))
            {
                throw new ArgumentException("Income date cannot be in the future.");
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId && a.UserId == userId);
            if (account == null)
            {
                throw new ArgumentException("Selected account is invalid or does not belong to the user.");
            }

            var income = new Income
            {
                UserId = userId,
                AccountId = request.AccountId,
                Amount = request.Amount,
                Source = request.Source.Trim(),
                IncomeDate = request.IncomeDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Update account balance (add income amount)
            account.CurrentBalance += income.Amount;
            account.UpdatedAt = DateTime.UtcNow;

            _context.Incomes.Add(income);
            await _context.SaveChangesAsync();

            // Populate account details for response
            income.Account = account;

            return MapToResponse(income);
        }

        public async Task<IncomeResponse> UpdateIncomeAsync(int id, UpdateIncomeRequest request, int userId)
        {
            var income = await _context.Incomes
                .Include(i => i.Account)
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
            {
                throw new KeyNotFoundException("Income record not found or you do not have access to it.");
            }

            if (request.Amount <= 0)
            {
                throw new ArgumentException("Income amount must be greater than zero.");
            }

            if (request.IncomeDate > DateTime.UtcNow.AddDays(1))
            {
                throw new ArgumentException("Income date cannot be in the future.");
            }

            if (income.AccountId == request.AccountId)
            {
                // Account hasn't changed. Adjust balance by the difference
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == income.AccountId && a.UserId == userId);
                if (account == null)
                {
                    throw new ArgumentException("Associated account is invalid.");
                }

                // Balance adjustment = new income amount - old income amount
                account.CurrentBalance += (request.Amount - income.Amount);
                account.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Account changed. Subtract old income from old account, add new income to new account
                var oldAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == income.AccountId && a.UserId == userId);
                if (oldAccount != null)
                {
                    oldAccount.CurrentBalance -= income.Amount;
                    oldAccount.UpdatedAt = DateTime.UtcNow;
                }

                var newAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId && a.UserId == userId);
                if (newAccount == null)
                {
                    throw new ArgumentException("Selected new account is invalid.");
                }

                newAccount.CurrentBalance += request.Amount;
                newAccount.UpdatedAt = DateTime.UtcNow;
            }

            income.AccountId = request.AccountId;
            income.Amount = request.Amount;
            income.Source = request.Source.Trim();
            income.IncomeDate = request.IncomeDate;
            income.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Refetch with populated Account details
            var updatedIncome = await _context.Incomes
                .Include(i => i.Account)
                .FirstAsync(i => i.Id == income.Id);

            return MapToResponse(updatedIncome);
        }

        public async Task<bool> DeleteIncomeAsync(int id, int userId)
        {
            var income = await _context.Incomes.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (income == null)
            {
                throw new KeyNotFoundException("Income record not found or you do not have access to it.");
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == income.AccountId && a.UserId == userId);
            if (account != null)
            {
                account.CurrentBalance -= income.Amount;
                account.UpdatedAt = DateTime.UtcNow;
            }

            _context.Incomes.Remove(income);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> GetMonthlyIncomeAsync(int userId, int month, int year)
        {
            return await _context.Incomes
                .Where(i => i.UserId == userId && i.IncomeDate.Month == month && i.IncomeDate.Year == year)
                .SumAsync(i => i.Amount);
        }

        public async Task<decimal> GetTotalIncomeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Incomes
                .Where(i => i.UserId == userId && i.IncomeDate >= startDate && i.IncomeDate <= endDate)
                .SumAsync(i => i.Amount);
        }
    }
}
