using System;
using System.Security.Cryptography;
using System.Text;

namespace LoginProject.Helpers
{
    public static class EncryptionHelper
    {
        public static string EncryptString(string plainText)
        {
            // Example encryption logic (use a proper encryption method in production)
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(data);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
