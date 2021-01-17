using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options;

namespace MisakaPatchPackager
{
    class Program
    {
        static IEncrypter getEncrypter(string enc)
        {
            switch(enc)
            {
                case "aes":
                    return new AesEncrypt();
                case "xor":
                case "":
                    return new XorEncrypt();
                default:
                    throw new Exception(String.Format("无法识别加密方法：{0}", enc));
            }
        }
        static void Main(string[] args)
        {
            List<string> files = new List<string>();
            bool help = false;
            bool useEnc = false;
            IEncrypter encrypter = new NopEncrypter();
            string enc = "";
            bool preview = false;
            var p = new OptionSet()
            {
                { "f|file=", v => files.Add(v) },
                { "e|enc=", v => { enc = v; useEnc = true; } },
                { "p|preview", v => preview = true },
                { "h|?|help", v => help = true },
            };
            List<string> extra = p.Parse(args);
            if (help || extra.Count != 1)
            {
                Console.WriteLine(@"
使用方法：
./MisakaPatchPackager.exe [option]... output
例：./MisakaPatchPackager.exe -f=""input1.txt"" -f=""input2.txt"" -e=xor -p output.msk
选项：
-f, --file      指定输入文件（可以指定多个）
-e, --enc       指定加密方法（可选择xor或aes），默认不加密
-p, --preview   预览前十行加密结果（只有在使用加密时才生效）
-h, --help      帮助
");
                return;
            }
            string output = extra[0];
            if (useEnc)
            {
                if (!output.EndsWith(".msk"))
                {
                    output += ".msk";
                }
                encrypter = getEncrypter(enc);
            }
            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter(output))
            {
                outputFile.WriteLine("#!useEnc={0},enc={1}", useEnc, enc);
                foreach (var f in files)
                {
                    Console.WriteLine("打包 {0}...", f);
                    string[] lines = System.IO.File.ReadAllLines(f);
                    foreach (var line in lines)
                    {
                        if (useEnc)
                        {
                            outputFile.WriteLine(encrypter.EncryptString(line));
                        }
                        else
                        {
                            outputFile.WriteLine(line);
                        }
                    }
                }
                Console.WriteLine("打包完成");
            }
            if (useEnc && preview)
            {
                Console.WriteLine("显示前十行加密结果...");
                // Open the text file using a stream reader.
                using (var sr = new StreamReader(output))
                {
                    string line;
                    var counter = 0;
                    while ((line = sr.ReadLine()) != null && counter < 10)
                    {
                        if (line.StartsWith("#!"))
                        {
                            continue;
                        }
                        System.Console.WriteLine(line);
                        System.Console.WriteLine(encrypter.DecryptString(line));
                        counter++;
                    }
                }
            }
            return;
        }
    }
}
