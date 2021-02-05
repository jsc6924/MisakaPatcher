using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MecabHelperLibrary
{

    public class NoWordSplit : WordSpliter
    {
        public List<WordInfo> SentenceHandle(string sentence)
        {
            List<WordInfo> ret = new List<WordInfo>(1);
            WordInfo info = new WordInfo();
            info.Word = sentence;
            ret.Add(info);
            return ret;
        }
    }
}
