using BCryptNet = BCrypt.Net.BCrypt;

namespace ExpenseManager.API.Helpers
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hashes a plain-text password using BCrypt.
        /// </summary>
        public static string HashPassword(string password)
        {
            return BCryptNet.HashPassword(password);
        }

        /// <summary>
        /// Verifies a plain-text password against a BCrypt hash.
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            return BCryptNet.Verify(password, hash);
        }
    }
}
