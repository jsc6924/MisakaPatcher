using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MisakaPatchPackager
{
    class NopEncrypter: IEncrypter
    {
        public string EncryptString(string plainText)
        {
            return plainText;
        }
        public string DecryptString(string cipherText)
        {
            return cipherText;
        }
    }
}
