using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace TelonaiWebApi.Helpers
{

    public class EncryptionSettings
    {
        public string Key { get; set; } 
        public string IV { get; set; } 
    }
    public interface IEncryption
    {
        string Encrypt(string plainText);

        string Decrypt(string encryptedText);
    }
    public class EncryptionHelper : IEncryption
    {
        public EncryptionHelper(IOptions<EncryptionSettings> encryptionSettings)
        {
            _encryptionSettings = encryptionSettings.Value;
        }
        private readonly EncryptionSettings _encryptionSettings;

        public string Encrypt(string plainText)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_encryptionSettings.Key);
                    aes.IV = Encoding.UTF8.GetBytes(_encryptionSettings.IV);

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var writer = new StreamWriter(cs))
                        {
                            writer.Write(plainText);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Encryption error: {ex.Message}");
                return plainText;
            }
        }
        public string Decrypt(string encryptedText)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_encryptionSettings.Key);
                    aes.IV = Encoding.UTF8.GetBytes(_encryptionSettings.IV);

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream(Convert.FromBase64String(encryptedText)))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var reader = new StreamReader(cs))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Decryption error: {ex.Message}");
                return encryptedText;
            }
        }
    }
}
