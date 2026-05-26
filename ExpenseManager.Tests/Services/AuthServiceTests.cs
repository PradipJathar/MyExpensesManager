using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ExpenseManager.API.Services;
using ExpenseManager.API.Helpers;
using ExpenseManager.API.Models;
using ExpenseManager.API.Dto;
using ExpenseManager.API.Data;

namespace ExpenseManager.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly JwtHelper _jwtHelper;

        public AuthServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            // Mock config for JwtHelper
            _configMock.Setup(c => c["JwtSettings:Secret"]).Returns("super_secret_test_key_with_sufficient_length_like_32_bytes_long");
            _configMock.Setup(c => c["JwtSettings:Issuer"]).Returns("test_issuer");
            _configMock.Setup(c => c["JwtSettings:Audience"]).Returns("test_audience");
            _configMock.Setup(c => c["JwtSettings:ExpiryInMinutes"]).Returns("60");

            _jwtHelper = new JwtHelper(_configMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldRegisterUserSuccessfully_AndSeedDefaultCategories()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var categoryService = new CategoryService(context);
            var authService = new AuthService(context, _jwtHelper, categoryService);

            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123!",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var response = await authService.RegisterAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Token);
            Assert.NotNull(response.RefreshToken);
            Assert.Equal("test@example.com", response.User.Email);
            Assert.Equal("John", response.User.FirstName);
            Assert.Equal("Doe", response.User.LastName);

            // Verify user stored in DB
            var dbUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            Assert.NotNull(dbUser);
            Assert.True(PasswordHelper.VerifyPassword("Password123!", dbUser.PasswordHash));

            // Verify default categories seeded (Food, Travel, etc. - 7 categories)
            var userCategories = await context.Categories.Where(c => c.UserId == dbUser.Id).ToListAsync();
            Assert.Equal(7, userCategories.Count);
            Assert.Contains(userCategories, c => c.CategoryName == "Food" && c.IsDefault);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowInvalidOperationException_WhenEmailAlreadyExists()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var categoryService = new CategoryService(context);
            var authService = new AuthService(context, _jwtHelper, categoryService);

            // Add an existing user
            context.Users.Add(new User
            {
                Email = "existing@example.com",
                PasswordHash = PasswordHelper.HashPassword("OldPass123!"),
                FirstName = "Existing",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var request = new RegisterRequest
            {
                Email = "existing@example.com",
                Password = "NewPassword123!",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => authService.RegisterAsync(request));
        }

        [Fact]
        public async Task LoginAsync_ShouldLoginSuccessfully_WithCorrectCredentials()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var categoryService = new CategoryService(context);
            var authService = new AuthService(context, _jwtHelper, categoryService);

            var passwordHash = PasswordHelper.HashPassword("SecretPassword123!");
            var user = new User
            {
                Email = "login@example.com",
                PasswordHash = passwordHash,
                FirstName = "Login",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var request = new LoginRequest
            {
                Email = "login@example.com",
                Password = "SecretPassword123!"
            };

            // Act
            var response = await authService.LoginAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Token);
            Assert.NotNull(response.RefreshToken);
            Assert.Equal("login@example.com", response.User.Email);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WithWrongPassword()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var categoryService = new CategoryService(context);
            var authService = new AuthService(context, _jwtHelper, categoryService);

            var user = new User
            {
                Email = "login@example.com",
                PasswordHash = PasswordHelper.HashPassword("CorrectPassword123!"),
                FirstName = "Login",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var request = new LoginRequest
            {
                Email = "login@example.com",
                Password = "WrongPassword123!"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.LoginAsync(request));
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldChangePasswordSuccessfully_WhenOldPasswordIsValid()
        {
            // Arrange
            using var context = TestDatabaseFixture.CreateContext();
            var categoryService = new CategoryService(context);
            var authService = new AuthService(context, _jwtHelper, categoryService);

            var user = new User
            {
                Id = 1,
                Email = "user@example.com",
                PasswordHash = PasswordHelper.HashPassword("OldPass123!"),
                FirstName = "User",
                LastName = "Test",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await authService.ChangePasswordAsync(userId: 1, oldPassword: "OldPass123!", newPassword: "NewPass123!");

            // Assert
            Assert.True(result);
            var dbUser = await context.Users.FindAsync(1);
            Assert.NotNull(dbUser);
            Assert.True(PasswordHelper.VerifyPassword("NewPass123!", dbUser.PasswordHash));
        }
    }
}
