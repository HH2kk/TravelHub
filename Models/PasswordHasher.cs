using System;

namespace TravelHub.Data
{
    public static class PasswordHasher
    {
        public static bool Verify(string stored, string input)
        {
            if (string.IsNullOrEmpty(stored) || input == null) return false;
            // Chỉ chấp nhận hash BCrypt; không có nhánh so sánh plaintext.
            if (!stored.StartsWith("$2", StringComparison.Ordinal)) return false;
            try { return global::BCrypt.Net.BCrypt.Verify(input, stored); }
            catch (Exception) { return false; }
        }

        public static string Hash(string input)
        {
            return global::BCrypt.Net.BCrypt.HashPassword(input);
        }
    }
}
