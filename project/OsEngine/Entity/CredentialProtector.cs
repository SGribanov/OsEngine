using System;
using System.Security.Cryptography;
using System.Text;

namespace OsEngine.Entity
{
    /// <summary>
    /// Protects credentials using DPAPI (CurrentUser scope) and a version marker.
    /// </summary>
    public static class CredentialProtector
    {
        public const string Prefix = "dpapi:";

        public static string Protect(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] protectedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
                return Prefix + Convert.ToBase64String(protectedBytes);
            }
            catch
            {
                // Keep backward-compatible behavior on unsupported platforms/runtime errors.
                return plainText;
            }
        }

        public static bool TryUnprotect(string storedValue, out string plainText)
        {
            plainText = string.Empty;

            if (string.IsNullOrEmpty(storedValue))
            {
                return true;
            }

            if (!storedValue.StartsWith(Prefix, StringComparison.Ordinal))
            {
                plainText = storedValue;
                return false;
            }

            string payload = storedValue.Substring(Prefix.Length);

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(payload);
                byte[] plainBytes = ProtectedData.Unprotect(cipherBytes, null, DataProtectionScope.CurrentUser);
                plainText = Encoding.UTF8.GetString(plainBytes);
                return true;
            }
            catch
            {
                plainText = storedValue;
                return false;
            }
        }
    }
}
