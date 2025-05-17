using DocuManagerAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace DocuManagerAPI.Data
{
    public static class FakeUserRepository
    {
        private static readonly PasswordHasher<object> _hasher = new();

        private static List<UserModel> _users = new()
        {
            new UserModel
            {
                Email = "admin@docu.com",
                SenhaHash = _hasher.HashPassword(null, "123456")
            }
        };

        public static UserModel GetUserByEmail(string email)
        {
            return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}
