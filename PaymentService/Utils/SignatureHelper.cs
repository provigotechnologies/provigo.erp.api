using System.Security.Cryptography;
using System.Text;

namespace PaymentService.Utils
{
    public static class SignatureHelper
    {
        public static string CreateSignature(string data, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hash = hmac.ComputeHash(dataBytes);
                return Convert.ToHexString(hash).ToLower(); // hex lowercase
            }
        }

        public static bool VerifySignature(string data, string signature, string key)
        {
            var generated = CreateSignature(data, key);
            return string.Equals(generated, signature, StringComparison.OrdinalIgnoreCase);
        }
    }
}
