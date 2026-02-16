#pragma warning disable SYSLIB0060 // Rfc2898DeriveBytes constructor is obsolete
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using OsEngine.Entity;

namespace OsEngine.OsTrader.Gui.BlockInterface
{
    public static class BlockMaster
    {
        public static string Password
        {
            get
            {
                try
                {
                    EncryptedValueSettingsDto settings = SettingsManager.Load(
                        GetPasswordPath(),
                        defaultValue: null,
                        legacyLoader: ParseLegacyEncryptedValueSettings);

                    if (settings == null || string.IsNullOrWhiteSpace(settings.EncryptedValue))
                    {
                        return string.Empty;
                    }

                    string decrypted = Decrypt(settings.EncryptedValue);
                    return decrypted ?? string.Empty;
                }
                catch (Exception)
                {
                    // ignore
                }

                return "";
            }
            set
            {
                try
                {
                    string saveStr = Encrypt(value);

                    SettingsManager.Save(
                        GetPasswordPath(),
                        new EncryptedValueSettingsDto
                        {
                            EncryptedValue = saveStr
                        });
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        public static bool IsBlocked
        {
            get
            {
                try
                {
                    EncryptedValueSettingsDto settings = SettingsManager.Load(
                        GetIsBlockedPath(),
                        defaultValue: null,
                        legacyLoader: ParseLegacyEncryptedValueSettings);

                    if (settings == null || string.IsNullOrWhiteSpace(settings.EncryptedValue))
                    {
                        return false;
                    }

                    string decrypted = Decrypt(settings.EncryptedValue);

                    if (string.IsNullOrWhiteSpace(decrypted))
                    {
                        return false;
                    }

                    if (bool.TryParse(decrypted, out bool isBlocked))
                    {
                        return isBlocked;
                    }

                    return false;
                }
                catch (Exception)
                {
                    // ignore
                }

                return false;
            }
            set
            {
                try
                {
                    string saveStr = Encrypt(value.ToString());

                    SettingsManager.Save(
                        GetIsBlockedPath(),
                        new EncryptedValueSettingsDto
                        {
                            EncryptedValue = saveStr
                        });
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        private static string GetPasswordPath()
        {
            return @"Engine\PrimeSettingss.txt";
        }

        private static string GetIsBlockedPath()
        {
            return @"Engine\PrimeSettingsss.txt";
        }

        private static EncryptedValueSettingsDto ParseLegacyEncryptedValueSettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string normalized = content.Replace("\r", string.Empty);
            string[] lines = normalized.Split('\n');

            if (lines.Length > 0 && lines[lines.Length - 1] == string.Empty)
            {
                Array.Resize(ref lines, lines.Length - 1);
            }

            return new EncryptedValueSettingsDto
            {
                EncryptedValue = lines.Length > 0 ? lines[0] : null
            };
        }

        private sealed class EncryptedValueSettingsDto
        {
            public string EncryptedValue { get; set; }
        }

        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "dfg2335";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 },1,HashAlgorithmName.SHA256);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText)
        {
            if(cipherText == null)
            {
                return null;
            }

            string EncryptionKey = "dfg2335";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }, 1, HashAlgorithmName.SHA256);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}
