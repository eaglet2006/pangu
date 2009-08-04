using System;
using System.Collections.Generic;
using System.Text;

namespace PanGu
{
    public enum Language
    {
        None = 0,
        English = 1,
        SimpleChinese = 2,
        TraditionalChinese = 3,
    }


    public class WordInfo : WordAttribute
    {
        /// <summary>
        /// Current language
        /// </summary>
        public Language Language;

        /// <summary>
        /// Original language
        /// </summary>
        public Language OriginalLanguage;

        public WordInfo(string word, int pos, double frequency)
            :base(word, pos, frequency)
        {
        }

        public WordInfo(WordAttribute wordAttr)
        {
            this.Word = wordAttr.Word;
            this.Pos = wordAttr.Pos;
            this.Frequency = wordAttr.Frequency;
        }
    }
}
