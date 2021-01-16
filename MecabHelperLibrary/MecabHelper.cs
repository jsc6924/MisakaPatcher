using MeCab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MecabHelperLibrary
{
    public class MecabHelper : WordSpliter
    {
        private MeCabParam Parameter;
        private MeCabTagger Tagger;

        public MecabHelper() {
            Parameter = new MeCabParam();
            Tagger = MeCabTagger.Create(Parameter);
        }

        public void Dispose()
        {
            Tagger.Dispose();
        }

        /// <summary>
        /// 处理句子，对句子进行分词，得到结果
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public List<WordInfo> SentenceHandle(string sentence) {

            List<WordInfo> ret = new List<WordInfo>();

            foreach (var node in Tagger.ParseToNodes(sentence))
            {
                if (node.CharType > 0)
                {
                    var features = node.Feature.Split(',');

                    WordInfo mwi = new WordInfo {
                        Word = node.Surface,
                        PartOfSpeech = features[0],
                        Description = features[1],
                        Feature = node.Feature
                    };

                    //加这一步是为了防止乱码进入分词导致无法读取假名
                    if (features.Length >= 8)
                    {
                        mwi.Kana = features[7];
                    }
                    
                    ret.Add(mwi);
                }
            }

            return ret;
        }


    }
}
