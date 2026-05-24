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
    public class AccountService
    {
        private readonly AppDbContext _context;

        public AccountService(AppDbContext context)
        {
            _context = context;
        }

        private static AccountResponse MapToResponse(Account account) => new()
        {
            Id = account.Id,
            AccountName = account.AccountName,
            AccountType = account.AccountType,
            AccountNumber = account.AccountNumber,
            InitialBalance = account.InitialBalance,
            CurrentBalance = account.CurrentBalance,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        };

        public async Task<IEnumerable<AccountResponse>> GetAllAccountsAsync(int userId)
        {
            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.AccountName)
                .ToListAsync();

            return accounts.Select(MapToResponse);
        }

        public async Task<AccountResponse> GetAccountByIdAsync(int id, int userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (account == null)
            {
                throw new KeyNotFoundException("Account not found or you do not have access to it.");
            }

            return MapToResponse(account);
        }

        public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, int userId)
        {
            var account = new Account
            {
                UserId = userId,
                AccountName = request.AccountName.Trim(),
                AccountType = request.AccountType,
                AccountNumber = request.AccountNumber?.Trim(),
                InitialBalance = request.CurrentBalance,
                CurrentBalance = request.CurrentBalance,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return MapToResponse(account);
        }

        public async Task<AccountResponse> UpdateAccountAsync(int id, UpdateAccountRequest request, int userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (account == null)
            {
                throw new KeyNotFoundException("Account not found or you do not have access to it.");
            }

            // Fetch dynamic totals to correct the CurrentBalance based on the new InitialBalance
            var totalExpenses = await _context.Expenses
                .Where(e => e.AccountId == id)
                .SumAsync(e => e.Amount);

            var totalIncome = await _context.Incomes
                .Where(i => i.AccountId == id)
                .SumAsync(i => i.Amount);

            account.AccountName = request.AccountName.Trim();
            account.AccountType = request.AccountType;
            account.AccountNumber = request.AccountNumber?.Trim();
            account.InitialBalance = request.InitialBalance;
            account.CurrentBalance = request.InitialBalance + totalIncome - totalExpenses;
            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(account);
        }

        public async Task<bool> DeleteAccountAsync(int id, int userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (account == null)
            {
                throw new KeyNotFoundException("Account not found or you do not have access to it.");
            }

            // Safety check: is the account used in any expenses?
            var hasExpenses = await _context.Expenses.AnyAsync(e => e.AccountId == id);
            if (hasExpenses)
            {
                throw new InvalidOperationException("Cannot delete account as it is currently associated with one or more expenses.");
            }

            // Safety check: is the account used in any incomes?
            var hasIncomes = await _context.Incomes.AnyAsync(i => i.AccountId == id);
            if (hasIncomes)
            {
                throw new InvalidOperationException("Cannot delete account as it is currently associated with one or more incomes.");
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<AccountBalanceResponse> GetAccountBalanceAsync(int id, int userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (account == null)
            {
                throw new KeyNotFoundException("Account not found or you do not have access to it.");
            }

            var totalExpenses = await _context.Expenses
                .Where(e => e.AccountId == id)
                .SumAsync(e => e.Amount);

            var totalIncome = await _context.Incomes
                .Where(i => i.AccountId == id)
                .SumAsync(i => i.Amount);

            var calculatedBalance = account.InitialBalance + totalIncome - totalExpenses;

            // Optional: keep CurrentBalance in sync in case of drift
            if (account.CurrentBalance != calculatedBalance)
            {
                account.CurrentBalance = calculatedBalance;
                await _context.SaveChangesAsync();
            }

            return new AccountBalanceResponse
            {
                AccountId = account.Id,
                AccountName = account.AccountName,
                InitialBalance = account.InitialBalance,
                TotalExpenses = totalExpenses,
                TotalIncome = totalIncome,
                CalculatedBalance = calculatedBalance
            };
        }
    }
}
