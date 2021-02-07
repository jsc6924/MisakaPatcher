using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;

namespace TranslatorLibrary
{
    public class ArtificialTranslator : ITranslator
    {
        private class CursorPriorityQueue
        {
            public class CPQComparator : IComparer<Tuple<int, double>>
            {
                // Call CaseInsensitiveComparer.Compare with the parameters reversed.
                int IComparer<Tuple<int, double>>.Compare(Tuple<int, double> x, Tuple<int, double> y)
                {
                    int r1 = x.Item2.CompareTo(y.Item2);
                    if (r1 != 0)
                    {
                        return r1;
                    }
                    else
                    {
                        return y.Item1.CompareTo(x.Item1);
                    }
                }
            }
            SortedSet<Tuple<int, double>> nextCursorsSet = new SortedSet<Tuple<int, double>>(new CPQComparator());
            int maxSize;
            public CursorPriorityQueue(int maxSize)
            {
                this.maxSize = maxSize;
            }
            public bool Add(int i, double p)
            {
                if (nextCursorsSet.Count < maxSize)
                {
                    bool result = nextCursorsSet.Add(new Tuple<int, double>(i, p));
                    return result;
                }
                var m = nextCursorsSet.Min;
                if (p > m.Item2)
                {
                    nextCursorsSet.Remove(m);
                    return nextCursorsSet.Add(new Tuple<int, double>(i, p));
                }
                return false;
            }
            public IEnumerable<int> Indices()
            {
                return nextCursorsSet.Select(i => i.Item1);
            }
            public IEnumerable<double> Values()
            {
                return nextCursorsSet.Select(i => i.Item2);
            }
        }

        private interface IDecrypt
        {
            string DecryptString(string cipherText);
        }

        private class AesDecrypt : IDecrypt
        {
            static byte[] iv = new byte[] { 6, 3, 2, 4, 4, 7, 2, 9, 0, 1, 0, 3, 6, 6, 8, 2 };
            static string Key = "7y41ca4o)pa2ea233bU^[0q4315a*+16";
            static Aes aes;
            static AesDecrypt()
            {
                aes = Aes.Create();
                aes.IV = iv;
                aes.Key = Encoding.UTF8.GetBytes(Key);
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
        private class XorDecrypt : IDecrypt
        {
            static byte[] key = new byte[] { 0x26, 0x3f, 0xa5, 0xbb, 0x4c, 0xf7, 0x9c, 0x19 };
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
                for (int j = 0; j < array.Length; j++)
                {
                    array[j] ^= key[i];
                    i = (i + 1) % key.Length;
                }
            }
        }

        private void initSettings(string mode)
        {
            switch (mode)
            {
                case "low":
                    pTransitionLongJump = 0.05;
                    pTransitionShortJump = 0.12;
                    break;
                default:
                case "medium":
                    pTransitionLongJump = 0.075;
                    pTransitionShortJump = 0.15;
                    break;
                case "high":
                    pTransitionLongJump = 0.15;
                    pTransitionShortJump = 0.25;
                    break;
            }
            pTransitionNext = 1 - pTransitionLongJump - pTransitionShortJump;
        }

        private int PatchPermission = 0;
        public int getPatchPermission()
        {
            return PatchPermission;
        }

        /// <summary>
        /// 翻译API初始化
        /// </summary>
        /// <param name="param1">参数一 汉化补丁路径</param>
        /// <param name="param2">参数二 不使用</param>
        public void TranslatorInit(string patchPath, string mode)
        {
            initSettings(mode);
            bool enc = false;
            IDecrypt decrypt = null;
            var patch = new StreamReader(patchPath);
            string temp = "";
            bool jp = true, first = true;
            void add()
            {
                if (!first)
                {
                    if (jp) jp_text.Add(temp);
                    else cn_text.Add(temp);
                }
                else
                {
                    first = false;
                }
                temp = "";
            }
            string line;
            while ((line = patch.ReadLine()) != null)
            {
                if (line.StartsWith("#!"))
                {
                    Regex r = new Regex(@"#!useEnc=(.*),enc=(.*)", RegexOptions.IgnoreCase);
                    Match m = r.Match(line);
                    if (m.Success)
                    {
                        enc = m.Groups[1].Value == "True";
                        PatchPermission = enc ? 0 : 1;
                        switch (m.Groups[2].Value)
                        {
                            case "xor":
                                decrypt = new XorDecrypt();
                                break;
                            case "aes":
                                decrypt = new AesDecrypt();
                                break;
                        }
                    }
                    continue;
                }
                if (enc)
                {
                    line = decrypt.DecryptString(line);
                }
                if (line == "\n" || line == "\r\n" || line.StartsWith("#"))
                {
                    //pass
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
            {
                throw new Exception(String.Format("原文与译文行数不一致，原文{0}，译文{1}，请检查补丁",
                    jp_text.Count, cn_text.Count));
            }
        }

        /// <summary>
        /// 翻译一条语句
        /// </summary>
        /// <param name="sourceText">源文本</param>
        /// <param name="desLang">目标语言</param>
        /// <param name="srcLang">源语言</param>
        /// <returns>翻译后的语句,如果翻译有错误会返回空，可以通过GetLastError来获取错误</returns>
        #region Explanation
        /*
         * An Approximated Viterbi Algorithm for sparse model
         * Author: jsc723
         * 
         * Our problem can be modeled as an HMM and we can apply Viterbi algorithm to decode the best 
         * match for each timestep. 
         * Let T[i, t] store the probability at time t that the most likely sequence so far ends at i. (i.e. most likely seq at t = (x_1, x_2, ... , x_t=i)
         * Assume there are K possible sentences (states).
         * 
         * We use the following transition model:
         *     P(transition from state i to j) = pNext * v       if j == i + 1  // go to next sentence
         *                                     = pShortJump * v  if j != i + 1 and abs(j - i - 1) < 10 // jump to somewhere nearby
         *                                     = pLongJump * v   otherwise  // jump to somewhere faraway (e.g. load game data)
         *          where pNext*v + 20*pShortJump*v + (K-21)*pLongJump*v == 1, v is a normalization factor
         *     (Assume K >> 20)
         *     Notice that we can choose any (pNext, pShortJump, pLongJump) due to normalization.
         *     
         *     
         * Initial probabilities P(i) = 1.0 / K
         *     which is same for all states, so we can use 1.0 due to normalization
         * 
         * P(state = i | observation at t) = ComputeSimilarity(jp_text[i], sourceText)
         *     see the implementation below for details
         * 
         * A forward step in Viterbi algorithm at time t can be described as 
         * for each state i = {1, 2, ..., K} do
         *     T[i, t] <- max(k)(T[i, t-1] * P(transition from state k to i) * P(state = i | observation at t))
         *     
         * This requires O(K^2), but our K > 30000, so it will be too slow for our case.
         * So we need to approximate:
         *     We only consider the case when at least one of {T[i, t-1], P(transition from state k to i)} are large.
         *     Let possibleCursors be a list of large T[i, t-1], and sum(possibleCursors) == 0.8,
         *     which means I'm 80% sure the previous state is one of the state in possibleCursors
         *     For each T[i, t-1] in possibleCursors , we consider 
         *       [Case 1]
         *          T[i, t-1]*P(i to i+1)*P(i+1 | o_t) //transition to the next line
         *       [Case 2]
         *          T[i, t-1]*P(i to somewhere u within [i-10, i+10])*P(u | o_t)  [Case 2] //transition to somewhere nearby
         *     
         *     At this point, if we've already find a confident solution (indicated by some threshold), we can stop here.
         *     Notice that the runtime is O(M), where M is the maximum size of possibleCursors and is a constant!
         *     
         *     If we don't find a confident solution after case 1 and 2, we have to search for every sentence: 
         *     For all k that P(k | o_t) is large, consider: 
         *        [Case 3]
         *          max(T[*, t-1])*P(i to k)*P(k | o_t) // a long jump from the most likely cursor
         *           
         *     The runtime is now O(MK). (linear)
         * 
         */
        #endregion
        public string Translate(string sourceText, string desLang, string srcLang)
        {
            //sourceText = addNoise(addNoise2(sourceText)); //The translator is able to find the correct match on hook mode under a high noise
            Console.WriteLine(String.Format("Input:{0}", sourceText));
            if (jp_text.Count == 0)
            {
                return "补丁为空";
            }

            if (sourceText.Length >= R_MAX_LEN)
            {
                sourceText = sourceText.Substring(0, R_MAX_LEN - 1);
            }

            double pMostLikelyPrevCursor = possibleCursors.Count == 0 ? 1.0 / pTransitionNext : possibleCursors.Max(i => i.Value);
            CursorPriorityQueue nextCursorsPQ = new CursorPriorityQueue(MAX_CURSOR);

            var maxPSeq = 0.0;
            var maxPSkip = 0.0;
            if (possibleCursors.Count > 0)
            {
                foreach (int k in possibleCursors.Keys)
                {
                    int pivot = k + 1;
                    for (int u = pivot - shortJumpMaxDistance; u < pivot + shortJumpMaxDistance; u++)
                    {
                        if (u >= 0 && u < jp_text.Count)
                        {
                            double s = ComputeSimilarity(sourceText, jp_text[u]);
                            int d = Math.Abs(u - pivot);
                            // [Case 1] or [Case 2]
                            double pTransition = u == pivot ? pTransitionNext : pTransitionShortJump;
                            double pSequantial = possibleCursors[k] * pTransition * s;
                            maxPSeq = Math.Max(maxPSeq, pSequantial);
                            nextCursorsPQ.Add(u, pSequantial);
                        }
                    }
                }
            }
            // do a full search only if we are not confident after case 1 and 2
            if (maxPSeq < pFullSearchThresh) 
            {
                for (int i = 0; i < jp_text.Count; i++)
                {
                    double s = ComputeSimilarity(sourceText, jp_text[i]);
                    double pSkip = pMostLikelyPrevCursor * pTransitionLongJump * s; //[Case 3]
                    maxPSkip = Math.Max(maxPSkip, pSkip);
                    nextCursorsPQ.Add(i, pSkip);
                }
            }
            Console.WriteLine("maxPSeq = {0}", maxPSeq);
            Console.WriteLine("maxPSkip = {0}", maxPSkip);
            //Softmax on next cursors
            List<int> nextCursorsIdx = nextCursorsPQ.Indices().ToList();
            var z = nextCursorsPQ.Values();
            double z_sum = z.Sum();
            var z_norm = z.Select(i => i / z_sum);  //take an extra normalization
            var z_exp = z_norm.Select(i => Math.Exp(SoftmaxCoeff * i));
            double sum_z_exp = z_exp.Sum();
            List<double> z_softmax = z_exp.Select(i => i / sum_z_exp).ToList();

            possibleCursors.Clear();

            for (int i = 0; i < z_softmax.Count; i++)
            {
                int j = nextCursorsIdx[i];
                if (possibleCursors.ContainsKey(j))
                {
                    possibleCursors[j] = Math.Max(possibleCursors[j], z_softmax[i]);
                }
                else
                {
                    possibleCursors[j] = z_softmax[i];
                }
            }
            int maxI = 0;
            double maxP = 0.0;
            foreach (int k in possibleCursors.Keys)
            {
                if (maxP < possibleCursors[k])
                {
                    maxP = possibleCursors[k];
                    maxI = k;
                }
                //For debug
                Console.WriteLine(String.Format("{0}:{1}", k, jp_text[k]));
                Console.WriteLine(possibleCursors[k]);
            }
            if (possibleCursors.Count == 0)
                return "无匹配文本";
            Console.WriteLine(String.Format("[{0}:{1}]", maxI, jp_text[maxI]));
            Console.WriteLine("------");
            return cn_text[maxI] == "" ? jp_text[maxI] : cn_text[maxI];
        }

        int addNoiseState = 0;
        //for test
        private string addNoise(string input)
        {
            if (addNoiseState++ % 2 == 0)
                return input;
            return "";
        }
        //for test
        private string addNoise2(string input)
        {
            StringBuilder result = new StringBuilder(input);
            for (int i = 0; i < input.Length; i++)
            {
                if (random.NextDouble() < 0.5)
                    result[i] = 'A';
            }
            return result.ToString();
        }


        /// <summary>
        /// 返回最后一次错误的ID或原因
        /// </summary>
        /// <returns></returns>
        public string GetLastError()
        {
            return "last error";
        }

        /// <summary>
        /// 返回两个string的相似度, 返回值在0到1之间
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private double ComputeSimilarity(string first, string second)
        {
            if (first.Length >= R_MAX_LEN)
            {
                first = first.Substring(0, R_MAX_LEN - 1);
            }

            if (second.Length >= R_MAX_LEN)
            {
                second = second.Substring(0, R_MAX_LEN - 1);
            }
            int d = ComputeDistance(first, second);
            double m = first.Length + second.Length + 1e-9;

            return Sigmoid(1.0 - (double)d / m) * LengthAdjust(m);
        }

        /// <summary>
        /// 返回两个string的edit distance, string的长度需小于R_MAX_LEN
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private int ComputeDistance(string first, string second)
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

        private static double Sigmoid(double x)
        {
            //Boost x > 0.7, suppress x < 0.7, please plot the curve to visualize
            double k = System.Math.Exp(-15.0 * (x - 0.7));
            return 1.0 / (1.0 + k);
        }

        private static double LengthAdjust(double x)
        {
            const double offset = 0.00055277; //k(0)
            double k = System.Math.Exp(-0.5 * (x - 15));
            return (1.0 / (1.0 + k) - offset) / (1.0 - offset);
        }


        private List<String> jp_text = new List<string>();
        private List<String> cn_text = new List<string>();
        Random random = new Random();
        private int[,] r = new int[2, R_MAX_LEN];
        private const int R_MAX_LEN = 64;
        private const int MAX_CURSOR = 8;
        private const double SoftmaxCoeff = 10;
        private double pTransitionLongJump;
        private double pTransitionShortJump;
        private double pTransitionNext;
        private const int shortJumpMaxDistance = 10;
        private const double pFullSearchThresh = 0.2;
        private Dictionary<int, double> possibleCursors = new Dictionary<int, double>();
    }
}
