using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
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
        static Exception getException(string f, int i, string msg)
        {
            return new Exception(String.Format("{0}第{1}行: {2}", f, i, msg));
        }
        static void checkInput(string fileName, string[] lines)
        {
            bool jp = false, first = true;
            int jpCount = 0, cnCount = 0;
            string line;
            int i = 0;
            void add()
            {
                if (!first)
                {
                    if (jp) jpCount++;
                    else cnCount++;
                }
                else
                {
                    first = false;
                }
            }
            while (i < lines.Length)
            {
                line = lines[i++];
                if (i == 1 && line.StartsWith("#!"))
                {
                    String regStr = @"#!useEnc=(True|False),enc=(aes|xor)";
                    Regex r = new Regex(regStr);
                    Match m = r.Match(line);
                    if (! m.Success)
                    {
                        throw getException(fileName, i, "#!后面的内容不正确，请参照" + regStr);
                    }
                    continue;
                }
                if (line == "\n" || line == "\r\n" || line.StartsWith("#"))
                {
                    //pass
                }
                else if (line.StartsWith("<j>"))
                {
                    if (jp)
                    {
                        throw getException(fileName, i, "检测到两句连续的原文");
                    }
                    add();
                    jp = true;
                }
                else if (line.StartsWith("<c>"))
                {
                    if (! jp)
                    {
                        throw getException(fileName, i, "检测到两句连续的译文");
                    }
                    add();
                    jp = false;
                }
            }
            add();
            if (jpCount != cnCount)
            {
                throw new Exception(String.Format("{2}: 原文与译文行数不一致，原文{0}，译文{1}",
                    jpCount, cnCount, fileName));
            }
        }
        static string Version = "1.1.0";
        static void Main(string[] args)
        {
            List<string> files = new List<string>();
            bool help = false;
            bool useEnc = false;
            IEncrypter encrypter = new NopEncrypter();
            string enc = "";
            bool check = false;
            bool preview = false;
            bool version = false;
            var p = new OptionSet()
            {
                { "f|file=", v => files.Add(v) },
                { "e|enc=", v => { enc = v; useEnc = true; } },
                { "c|check", v => { check = true; } },
                { "p|preview", v => preview = true },
                { "v|version", v => version = true },
                { "h|?|help", v => help = true },
            };
            List<string> extra = p.Parse(args);
            if (version)
            {
                Console.WriteLine("MisakaPatchPackager v{0}", Version);
                return;
            }
            if (help || extra.Count != 1)
            {
                Console.WriteLine("MisakaPatchPackager v{0}", Version);
                Console.WriteLine(@"
使用方法：
./MisakaPatchPackager.exe [option]... output
例：./MisakaPatchPackager.exe -f=""input1.txt"" -f=""input2.txt"" -e=xor -p output.msk
选项：
-f, --file [FILE]       指定输入文件（可以使用多个-f指定多个）
-e, --enc  [xor|aes]    指定加密方法（可选择xor或aes），默认不加密
-c, --check             检查语法
-p, --preview           预览前十行加密结果（只有在使用加密时才生效）
-v, --version           显示版本
-h, --help              帮助
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
                    string[] lines = File.ReadAllLines(f);
                    if (check)
                    {
                        Console.WriteLine("检查语法 {0}...", f);
                        try
                        {
                            checkInput(f, lines);
                        } 
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            outputFile.Close();
                            File.Delete(output);
                            return;
                        }
                    }
                    Console.WriteLine("打包 {0}...", f);
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
