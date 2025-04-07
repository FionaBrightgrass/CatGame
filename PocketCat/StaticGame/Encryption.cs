using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PocketCat.StaticGame
{
    public static class Encryption
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("zY8Q21mV#x5RNp*wkT4Hd9^cJ$oLbF!E"); // Replace with a secure key
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("16PCat44ForAES!1");

        public static void SaveSecureFile(string jwtToken, string filePath)
        {
            try
            {
                // Encrypt the JWT
                byte[] encryptedData = EncryptData(jwtToken);

                // Write encrypted data to file
                File.WriteAllBytes(filePath, encryptedData);

                Console.WriteLine("JWT saved securely.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving secure file: {ex.Message}");
            }
        }

        private static byte[] EncryptData(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return ms.ToArray();
                }
            }
        }
        public static string LoadSecureFile(string filePath)
        {
            try
            {
                // Read encrypted data from file
                byte[] encryptedData = File.ReadAllBytes(filePath);

                // Decrypt the data
                string decryptedData = DecryptData(encryptedData);

                Console.WriteLine("JWT loaded securely.");
                return decryptedData; // Return the original JWT
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading secure file: {ex.Message}");
                return null;
            }
        }

        private static string DecryptData(byte[] encryptedData)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                using (MemoryStream ms = new MemoryStream(encryptedData))
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
