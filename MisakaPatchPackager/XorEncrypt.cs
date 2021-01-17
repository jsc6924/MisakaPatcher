using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;

namespace MisakaPatchPackager
{
    class XorEncrypt : IEncrypter
    {
        static byte[] key = new byte[] { 0x26, 0x3f, 0xa5, 0xbb, 0x4c, 0xf7, 0x9c, 0x19 };
        public string EncryptString(string plainText)
        {
            byte[] array;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                {
                    streamWriter.Write(plainText);
                }
                array = memoryStream.ToArray();
                Xor(array);
            }
            return Convert.ToBase64String(array);
        }

        public string DecryptString(string cipherText)
        {
            byte[] buffer = Convert.FromBase64String(cipherText);
            Xor(buffer);
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (StreamReader streamReader = new StreamReader(memoryStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        private static void Xor(byte[] array)
        {
            int i = 3;
            for(int j = 0; j < array.Length; j++)
            {
                array[j] ^= key[i];
                i = (i + 1) % key.Length;
            }
        }
    }
}