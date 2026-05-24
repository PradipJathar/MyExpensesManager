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
    public class CategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        private static CategoryResponse MapToResponse(Category category) => new()
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            IsDefault = category.IsDefault,
            ColorCode = category.ColorCode,
            CreatedAt = category.CreatedAt
        };

        public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync(int userId)
        {
            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.IsDefault ? 0 : 1)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

            return categories.Select(MapToResponse);
        }

        public async Task<CategoryResponse> GetCategoryByIdAsync(int id, int userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found or you do not have access to it.");
            }

            return MapToResponse(category);
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, int userId)
        {
            var nameNormalized = request.CategoryName.Trim();

            // Check duplicate name (case insensitive)
            var duplicateExists = await _context.Categories
                .AnyAsync(c => c.UserId == userId && c.CategoryName.ToLower() == nameNormalized.ToLower());

            if (duplicateExists)
            {
                throw new ArgumentException($"A category with the name '{nameNormalized}' already exists.");
            }

            var category = new Category
            {
                UserId = userId,
                CategoryName = nameNormalized,
                ColorCode = string.IsNullOrWhiteSpace(request.ColorCode) ? "#3B82F6" : request.ColorCode.Trim(),
                IsDefault = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return MapToResponse(category);
        }

        public async Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request, int userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found or you do not have access to it.");
            }

            if (category.IsDefault)
            {
                throw new InvalidOperationException("Default categories cannot be modified.");
            }

            var nameNormalized = request.CategoryName.Trim();

            // Check duplicate name excluding current category
            var duplicateExists = await _context.Categories
                .AnyAsync(c => c.UserId == userId && c.Id != id && c.CategoryName.ToLower() == nameNormalized.ToLower());

            if (duplicateExists)
            {
                throw new ArgumentException($"A category with the name '{nameNormalized}' already exists.");
            }

            category.CategoryName = nameNormalized;
            category.ColorCode = request.ColorCode.Trim();

            await _context.SaveChangesAsync();

            return MapToResponse(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id, int userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found or you do not have access to it.");
            }

            if (category.IsDefault)
            {
                throw new InvalidOperationException("Default categories cannot be deleted.");
            }

            // Check if there are expenses associated with this category
            var hasExpenses = await _context.Expenses
                .AnyAsync(e => e.CategoryId == id && e.UserId == userId);

            if (hasExpenses)
            {
                throw new InvalidOperationException("Cannot delete category as it is currently assigned to one or more expenses.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task SeedDefaultCategoriesAsync(int userId)
        {
            var defaultCategories = new[]
            {
                new Category { UserId = userId, CategoryName = "Food", IsDefault = true, ColorCode = "#EF4444", CreatedAt = DateTime.UtcNow },
                new Category { UserId = userId, CategoryName = "Travel", IsDefault = true, ColorCode = "#3B82F6", CreatedAt = DateTime.UtcNow },
                new Category { UserId = userId, CategoryName = "Entertainment", IsDefault = true, ColorCode = "#10B981", CreatedAt = DateTime.UtcNow },
                new Category { UserId = userId, CategoryName = "Shopping", IsDefault = true, ColorCode = "#F59E0B", CreatedAt = DateTime.UtcNow },
                new Category { UserId = userId, CategoryName = "Utilities", IsDefault = true, ColorCode = "#6366F1", CreatedAt = DateTime.UtcNow },
                new Category { UserId = userId, CategoryName = "Healthcare", IsDefault = true, ColorCode = "#EC4899", CreatedAt = DateTime.UtcNow },
                new Category { UserId = userId, CategoryName = "Other", IsDefault = true, ColorCode = "#6B7280", CreatedAt = DateTime.UtcNow }
            };

            _context.Categories.AddRange(defaultCategories);
            await Task.CompletedTask;
        }
    }
}
