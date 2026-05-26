using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using ExpenseManager.API.Services;
using ExpenseManager.API.Models;
using ExpenseManager.API.Dto;
using ExpenseManager.API.Data;

namespace ExpenseManager.Tests.Services
{
    public class BudgetServiceTests
    {
        [Fact]
        public async Task CreateBudgetAsync_ShouldCreateBudgetSuccessfully_WhenCategoryIsValidAndNotDuplicate()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var service = new BudgetService(context);
            var userId = 1;
            var categoryId = 5;

            // Pre-seed category
            context.Categories.Add(new Category 
            { 
                Id = categoryId, 
                UserId = userId, 
                CategoryName = "Rent", 
                ColorCode = "#FFF", 
                CreatedAt = DateTime.UtcNow 
            });
            await context.SaveChangesAsync();

            var request = new CreateBudgetRequest
            {
                CategoryId = categoryId,
                BudgetAmount = 1200.00m,
                PeriodMonth = 6,
                PeriodYear = 2026,
                AlertThreshold = 80
            };

            // Act
            var result = await service.CreateBudgetAsync(request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.CategoryId);
            Assert.Equal("Rent", result.CategoryName);
            Assert.Equal(1200.00m, result.BudgetAmount);
            Assert.Equal(6, result.PeriodMonth);
            Assert.Equal(2026, result.PeriodYear);
            Assert.Equal(80, result.AlertThreshold);

            var dbBudget = await context.Budgets.FindAsync(result.Id);
            Assert.NotNull(dbBudget);
            Assert.Equal(userId, dbBudget.UserId);
        }

        [Fact]
        public async Task CreateBudgetAsync_ShouldThrowArgumentException_WhenCategoryDoesNotExistOrDoesNotBelongToUser()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var service = new BudgetService(context);
            var userId = 1;

            var request = new CreateBudgetRequest
            {
                CategoryId = 99, // Category doesn't exist
                BudgetAmount = 200m,
                PeriodMonth = 5,
                PeriodYear = 2026,
                AlertThreshold = 80
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateBudgetAsync(request, userId));
        }

        [Fact]
        public async Task CreateBudgetAsync_ShouldThrowArgumentException_WhenBudgetDuplicateExistsForSameCategoryMonthAndYear()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var service = new BudgetService(context);
            var userId = 1;
            var categoryId = 10;

            // Seed Category
            context.Categories.Add(new Category { Id = categoryId, UserId = userId, CategoryName = "Groceries", CreatedAt = DateTime.UtcNow });
            
            // Seed existing budget
            context.Budgets.Add(new Budget
            {
                UserId = userId,
                CategoryId = categoryId,
                BudgetAmount = 300m,
                PeriodMonth = 6,
                PeriodYear = 2026,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var request = new CreateBudgetRequest
            {
                CategoryId = categoryId,
                BudgetAmount = 400m,
                PeriodMonth = 6,
                PeriodYear = 2026,
                AlertThreshold = 90
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateBudgetAsync(request, userId));
        }

        [Fact]
        public async Task GetBudgetStatusAsync_ShouldCalculateCorrectSpentPercentagesAndFlags()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var service = new BudgetService(context);
            var userId = 1;
            var categoryId = 1;

            // Seed user category
            context.Categories.Add(new Category { Id = categoryId, UserId = userId, CategoryName = "Entertainment", ColorCode = "#123", CreatedAt = DateTime.UtcNow });
            
            // Seed budget ($500 limit, alert at 80%)
            context.Budgets.Add(new Budget
            {
                Id = 1,
                UserId = userId,
                CategoryId = categoryId,
                BudgetAmount = 500.00m,
                PeriodMonth = 5,
                PeriodYear = 2026,
                AlertThreshold = 80,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            // Seed expenses for this category in same month ($450 spent - which is 90% of budget)
            context.Expenses.AddRange(new List<Expense>
            {
                new() { Id = 1, UserId = userId, CategoryId = categoryId, AccountId = 1, Amount = 200.00m, ExpenseDate = new DateTime(2026, 5, 10), Description = "Movie", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { Id = 2, UserId = userId, CategoryId = categoryId, AccountId = 1, Amount = 250.00m, ExpenseDate = new DateTime(2026, 5, 15), Description = "Concert", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { Id = 3, UserId = userId, CategoryId = categoryId, AccountId = 1, Amount = 100.00m, ExpenseDate = new DateTime(2026, 6, 01), Description = "Next month movie", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow } // Excluded: next month
            });

            await context.SaveChangesAsync();

            // Act
            var results = await service.GetBudgetStatusAsync(userId);
            var status = results.FirstOrDefault(s => s.BudgetId == 1);

            // Assert
            Assert.NotNull(status);
            Assert.Equal(450.00m, status.SpentAmount);
            Assert.Equal(90.00m, status.PercentageUsed);
            Assert.True(status.IsAlert); // 90% >= 80%
            Assert.False(status.IsExceeded); // 450 <= 500
        }
    }
}
