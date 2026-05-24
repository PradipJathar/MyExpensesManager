using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ExpenseManager.API.Data;
using ExpenseManager.API.Dto;
using ExpenseManager.API.Helpers;
using ExpenseManager.API.Models;

namespace ExpenseManager.API.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthService(AppDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Normalize email
            var emailNormalized = request.Email.Trim().ToLower();

            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == emailNormalized))
            {
                throw new InvalidOperationException("A user with this email address already exists.");
            }

            // Create new user
            var user = new User
            {
                Email = emailNormalized,
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Seed default categories for new user
            SeedDefaultCategories(user.Id);
            await _context.SaveChangesAsync();

            // Generate tokens
            var accessToken = _jwtHelper.GenerateToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            // Save refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var emailNormalized = request.Email.Trim().ToLower();

            // Find user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailNormalized);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // Verify password
            if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // Generate tokens
            var accessToken = _jwtHelper.GenerateToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            // Save refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
            {
                throw new KeyNotFoundException("User not found.");
            }

            // Verify old password
            if (!PasswordHelper.VerifyPassword(oldPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Incorrect current password.");
            }

            // Set new password
            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            ClaimsPrincipal principal;
            try
            {
                principal = _jwtHelper.GetPrincipalFromExpiredToken(request.Token);
            }
            catch (Exception)
            {
                throw new SecurityException("Invalid access token.");
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new SecurityException("Invalid token claims.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                throw new SecurityException("Invalid or expired refresh token.");
            }

            // Generate new tokens
            var newAccessToken = _jwtHelper.GenerateToken(user);
            var newRefreshToken = _jwtHelper.GenerateRefreshToken();

            // Update user refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };
        }

        private void SeedDefaultCategories(int userId)
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
        }
    }

    public class SecurityException : Exception
    {
        public SecurityException(string message) : base(message) { }
    }
}
