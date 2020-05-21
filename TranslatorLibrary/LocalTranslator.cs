using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace TranslatorLibrary
{
    public class LocalTranslator :ITranslator
{
        /// <summary>
        /// 翻译API初始化
        /// </summary>
        /// <param name="param1">参数一 汉化补丁路径</param>
        /// <param name="param2">参数二 不使用</param>
        public void TranslatorInit(string patchPath, string param2 = "")
        {
            string[] lines = System.IO.File.ReadAllLines(patchPath);
            string temp = "";
            bool jp = true;
            void add()
            {
                if (temp != "")
                {
                    if (jp) jp_text.Add(temp);
                    else cn_text.Add(temp);
                }
                temp = "";
            }

            foreach(string line in lines)
            {
                if(line == "\n" || line == "\r\n")
                {
                    add();
                } 
                else if (line.StartsWith("<j>"))
                {
                    add();
                    jp = true;
                }
                else if (line.StartsWith("<c>"))
                {
                    add();
                    jp = false;
                }
                else
                {
                    temp += line;
                }
            }
            add();
            if (jp_text.Count != cn_text.Count)
                throw new Exception("Total sentence number not match, please check your patch.");
        }

        /// <summary>
        /// 翻译一条语句
        /// </summary>
        /// <param name="sourceText">源文本</param>
        /// <param name="desLang">目标语言</param>
        /// <param name="srcLang">源语言</param>
        /// <returns>翻译后的语句,如果翻译有错误会返回空，可以通过GetLastError来获取错误</returns>
        public string Translate(string sourceText, string desLang, string srcLang)
        {
            double maxSim = 0.0;
            int maxI = 0;
            if (jp_text.Count == 0)
                return "No translation available";
            for(int i = 0; i < jp_text.Count; i++)
            {
                double s = ComputeSimilarity(sourceText, jp_text[i]);
                Console.WriteLine(jp_text[i]);
                Console.WriteLine(s);
                if(s > maxSim)
                {
                    maxSim = s;
                    maxI = i;
                }
            }

            return cn_text[maxI];
        }

        /// <summary>
        /// 返回最后一次错误的ID或原因
        /// </summary>
        /// <returns></returns>
        public string GetLastError()
        {
            return "last error";
        }

        private static double ComputeSimilarity(string first, string second)
        {
            int d = ComputeDistance(first, second);
            return 1.0 - (double)d / (first.Length + second.Length);
        }

        private static int ComputeDistance(string first, string second)
        {
            if (first.Length == 0)
            {
                return second.Length;
            }

            if (second.Length == 0)
            {
                return first.Length;
            }

            var current = 1;
            var previous = 0;
            var r = new int[2, second.Length + 1];
            for (var i = 0; i <= second.Length; i++)
            {
                r[previous, i] = i;
            }

            for (var i = 0; i < first.Length; i++)
            {
                r[current, 0] = i + 1;

                for (var j = 1; j <= second.Length; j++)
                {
                    var cost = (second[j - 1] == first[i]) ? 0 : 1;
                    r[current, j] = Min(
                        r[previous, j] + 1,
                        r[current, j - 1] + 1,
                        r[previous, j - 1] + cost);
                }
                previous = (previous + 1) % 2;
                current = (current + 1) % 2;
            }
            return r[previous, second.Length];
        }

        private static int Min(int e1, int e2, int e3) =>
            Math.Min(Math.Min(e1, e2), e3);


        private List<String> jp_text = new List<string>();
        private List<String> cn_text = new List<string>();
    }
}

