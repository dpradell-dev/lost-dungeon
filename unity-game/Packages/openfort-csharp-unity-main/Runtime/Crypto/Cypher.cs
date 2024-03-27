using System;
using System.Security.Cryptography;
using System.Text;

namespace Openfort.Crypto
{
    public class Cypher
    {
        public static byte[] DeriveKey(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public static string Encrypt(string key, string data)
        {
            var aes = Aes.Create();
            aes.Key = DeriveKey(key);
            aes.GenerateIV();
        
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var encrypted = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(data), 0, data.Length);

            var result = new byte[aes.IV.Length + encrypted.Length];
            Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
            Array.Copy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

            return Convert.ToBase64String(result);
        }

        public static string Decrypt(string key, string data)
        {
            var combined = Convert.FromBase64String(data);
            var aes = Aes.Create();
            aes.Key = DeriveKey(key);

            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] encrypted = new byte[combined.Length - iv.Length];

            Array.Copy(combined, iv, iv.Length);
            Array.Copy(combined, iv.Length, encrypted, 0, encrypted.Length);

            aes.IV = iv;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            var decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
            return Encoding.UTF8.GetString(decrypted);
        }    
    }
}