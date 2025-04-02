using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace PocketCat.DefaultData
{
    public static class DataUtility
    {
        public static string GenerateCombinedData()
        {
            string combinedData = SerializeWithSettings(PocketCat.DefaultData.ActionData.DataModel);
            return combinedData;
        }


        public static bool ValidateCombinedData(string decryptedData)
        {
            try
            {
                var deserializedData = JsonConvert.DeserializeObject<ActionDataModel>(decryptedData);
                var originalData = PocketCat.DefaultData.ActionData.DataModel;

                // Validate by comparing serialized versions of the objects
                bool isValid = JsonConvert.SerializeObject(deserializedData) == JsonConvert.SerializeObject(originalData);
                Console.WriteLine(isValid ? "Validation successful! All data match." : "Validation failed. Data do not match.");
                return isValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during validation: {ex.Message}");
                return false;
            }
        }

        private static string SerializeWithSettings<T>(T data)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
            };
            return JsonConvert.SerializeObject(data, settings);
        }
    }


    public static class HashEncryptionUtility
    {
        public static byte[] EncryptHashesToMemory(string plainText, byte[] encryptionKey, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = encryptionKey;
                aes.IV = iv;

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        public static string DecryptHashesFromMemory(byte[] cipherText, byte[] encryptionKey, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = encryptionKey;
                aes.IV = iv;

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}

