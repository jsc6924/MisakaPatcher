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
        static void Main(string[] args)
        {
            List<string> files = new List<string>();
            bool help = false;
            bool enc = false;
            bool preview = false;
            var p = new OptionSet()
            {
                { "f|file=", v => files.Add(v) },
                { "e|enc", v => enc = true },
                { "p|preview", v => preview = true },
                { "h|?|help", v => help = true },
            };
            List<string> extra = p.Parse(args);
            if (help || extra.Count != 1)
            {
                Console.WriteLine(@"
使用方法：
./MisakaPatchPackager.exe [option]... output
例：./MisakaPatchPackager.exe -f=""input1.txt"" -f=""input2.txt"" -ep output.msk
选项：
-f, --file      指定输入文件（可以指定多个）
-e, --enc       加密
-p, --preview   预览前十行加密结果（只有在使用加密时才生效）
-h, --help      帮助
");
                return;
            }
            string output = extra[0];
            if (enc && !output.EndsWith(".msk"))
            {
                output += ".msk";
            }
            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter(output))
            {
                foreach (var f in files)
                {
                    Console.WriteLine("打包 {0}...", f);
                    string[] lines = System.IO.File.ReadAllLines(f);
                    foreach (var line in lines)
                    {
                        if (enc)
                        {
                            outputFile.WriteLine(Encrypt.EncryptString(line));
                        }
                        else
                        {
                            outputFile.WriteLine(line);
                        }
                    }
                }
                Console.WriteLine("打包完成");
            }
            if (enc && preview)
            {
                Console.WriteLine("显示前十行加密结果...");
                // Open the text file using a stream reader.
                using (var sr = new StreamReader(output))
                {
                    string line;
                    var counter = 0;
                    while ((line = sr.ReadLine()) != null && counter < 10)
                    {
                        System.Console.WriteLine(line);
                        System.Console.WriteLine(Encrypt.DecryptString(line));
                        counter++;
                    }
                }
            }
            return;
        }
    }
}
