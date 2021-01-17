using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MisakaPatchPackager
{
    public interface IEncrypter
    {
        string EncryptString(string plainText);
        string DecryptString(string cipherText);

    }
}
