using Isopoh.Cryptography.Argon2;
using System.Security.Cryptography;
using System.Text;

namespace QueTalMiAFPAoTAPI.Helpers {
    public static class CryptoHelper {
        public static string Hash(string password) {
            // byte[] salt = RandomNumberGenerator.GetBytes(16);
            // return Argon2.Hash(Encoding.UTF8.GetBytes(password), salt);
            return Argon2.Hash(password);
        }

        public static bool Verify(string password, string encoded) {
            return Argon2.Verify(encoded, password);
        }
    }
}
