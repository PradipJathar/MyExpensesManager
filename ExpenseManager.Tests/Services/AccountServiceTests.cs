using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ExpenseManager.API.Services;
using ExpenseManager.API.Models;
using ExpenseManager.API.Dto;
using ExpenseManager.API.Data;

namespace ExpenseManager.Tests.Services
{
    public class AccountServiceTests
    {
        [Fact]
        public async Task CreateAccountAsync_ShouldCreateAccountSuccessfully()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var service = new AccountService(context);
            var userId = 1;
            var request = new CreateAccountRequest
            {
                AccountName = "Savings Account",
                AccountType = "Bank",
                AccountNumber = "123456",
                CurrentBalance = 1000.00m
            };

            // Act
            var result = await service.CreateAccountAsync(request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Savings Account", result.AccountName);
            Assert.Equal("Bank", result.AccountType);
            Assert.Equal("123456", result.AccountNumber);
            Assert.Equal(1000.00m, result.InitialBalance);
            Assert.Equal(1000.00m, result.CurrentBalance);

            var dbAccount = context.Accounts.FirstOrDefault(a => a.Id == result.Id);
            Assert.NotNull(dbAccount);
            Assert.Equal(userId, dbAccount.UserId);
        }

        [Fact]
        public async Task GetAllAccountsAsync_ShouldOnlyReturnUsersOwnAccounts()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var service = new AccountService(context);
            
            context.Accounts.AddRange(new List<Account>
            {
                new() { Id = 1, UserId = 1, AccountName = "User 1 Account A", AccountType = "Bank", InitialBalance = 500, CurrentBalance = 500, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { Id = 2, UserId = 1, AccountName = "User 1 Account B", AccountType = "Cash", InitialBalance = 100, CurrentBalance = 100, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { Id = 3, UserId = 2, AccountName = "User 2 Account A", AccountType = "Bank", InitialBalance = 1500, CurrentBalance = 1500, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAllAccountsAsync(userId: 1);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, a => Assert.Contains("User 1", a.AccountName));
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldThrowKeyNotFoundException_WhenAccountDoesNotExistOrBelongsToOtherUser()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var service = new AccountService(context);

            context.Accounts.Add(new Account 
            { 
                Id = 1, 
                UserId = 1, 
                AccountName = "My Account", 
                AccountType = "Bank", 
                InitialBalance = 100, 
                CurrentBalance = 100, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            });
            await context.SaveChangesAsync();

            // Act & Assert
            // 1. Account belongs to user 1, but requested by user 2
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetAccountByIdAsync(id: 1, userId: 2));
            
            // 2. Account does not exist
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetAccountByIdAsync(id: 99, userId: 1));
        }

        [Fact]
        public async Task DeleteAccountAsync_ShouldThrowInvalidOperationException_WhenAccountHasLinkedExpensesOrIncomes()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var service = new AccountService(context);
            var userId = 1;
            
            var account = new Account { Id = 1, UserId = userId, AccountName = "Active Account", AccountType = "Bank", InitialBalance = 500, CurrentBalance = 500, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.Accounts.Add(account);

            // Add an expense linking to this account
            context.Expenses.Add(new Expense
            {
                Id = 1,
                UserId = userId,
                AccountId = 1,
                Amount = 50.00m,
                Description = "Coffee",
                ExpenseDate = DateTime.UtcNow,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAccountAsync(id: 1, userId: userId));
        }

        [Fact]
        public async Task GetAccountBalanceAsync_ShouldCalculateCorrectBalance()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var service = new AccountService(context);
            var userId = 1;
            var accountId = 1;

            context.Accounts.Add(new Account 
            { 
                Id = accountId, 
                UserId = userId, 
                AccountName = "Checkings", 
                AccountType = "Bank", 
                InitialBalance = 1000.00m, 
                CurrentBalance = 1000.00m, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            });

            // Add expenses (- $150 total)
            context.Expenses.AddRange(new List<Expense>
            {
                new() { Id = 1, UserId = userId, AccountId = accountId, Amount = 50.00m, Description = "Item 1", ExpenseDate = DateTime.UtcNow, CategoryId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { Id = 2, UserId = userId, AccountId = accountId, Amount = 100.00m, Description = "Item 2", ExpenseDate = DateTime.UtcNow, CategoryId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });

            // Add income (+ $300 total)
            context.Incomes.Add(new Income
            {
                Id = 1,
                UserId = userId,
                AccountId = accountId,
                Amount = 300.00m,
                Source = "Salary",
                IncomeDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAccountBalanceAsync(accountId, userId);

            // Assert
            // 1000 (initial) + 300 (income) - 150 (expenses) = 1150
            Assert.Equal(1150.00m, result.CalculatedBalance);
            Assert.Equal(150.00m, result.TotalExpenses);
            Assert.Equal(300.00m, result.TotalIncome);
        }
    }
}
