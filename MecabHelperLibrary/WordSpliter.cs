using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MecabHelperLibrary
{
    public interface WordSpliter
    {
        List<WordInfo> SentenceHandle(string sentence);
    }

    public struct WordInfo
    {

        /// <summary>
        /// 单词
        /// </summary>
        public string Word;

        /// <summary>
        /// 词性
        /// </summary>
        public string PartOfSpeech;

        /// <summary>
        /// 词性说明
        /// </summary>
        public string Description;

        /// <summary>
        /// 假名
        /// </summary>
        public string Kana;

        /// <summary>
        /// Mecab能提供的关于这个词的详细信息 CSV表示
        /// </summary>
        public string Feature;
    }
}
