using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace MisakaPatchPackager
{
    class AesEncrypt : IEncrypter
    {
        static byte[] iv = new byte[] { 6, 3, 2, 4, 4, 7, 2, 9, 0, 1, 0, 3, 6, 6, 8, 2 };
        static string Key = "7y41ca4o)pa2ea233bU^[0q4315a*+16";
        static Aes aes;
        static AesEncrypt()
        {
            aes = Aes.Create();
            aes.IV = iv;
            aes.Key = Encoding.UTF8.GetBytes(Key);
        }
        public string EncryptString(string plainText)
        {
            byte[] array;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }

                    array = memoryStream.ToArray();
                }
            }

            return Convert.ToBase64String(array);
        }

        public string DecryptString(string cipherText)
        {
            byte[] buffer = Convert.FromBase64String(cipherText);

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }
}