using System;
using System.Collections.Generic;
using System.Text;

namespace PanGu
{
    public enum WordType
    {
        None = 0,
        English = 1,
        SimpleChinese = 2,
        TraditionalChinese = 3,
        Numeric = 4,
        Symbol = 5,
        Space = 6,
    }


    public class WordInfo : WordAttribute
    {
        /// <summary>
        /// Current word type
        /// </summary>
        public WordType WordType;

        /// <summary>
        /// Original word type
        /// </summary>
        public WordType OriginalWordType;

        /// <summary>
        /// Word position
        /// </summary>
        public int Position;

        /// <summary>
        /// Rank for this word
        /// 单词权重
        /// </summary>
        public int Rank;

        public WordInfo()
        {
        }

        public WordInfo(string word, POS pos, double frequency)
            :base(word, pos, frequency)
        {
        }

        public WordInfo(WordAttribute wordAttr)
        {
            this.Word = wordAttr.Word;
            this.Pos = wordAttr.Pos;
            this.Frequency = wordAttr.Frequency;
        }

        public WordInfo(Dict.PositionLength pl, string oringinalText)
        {
            this.Word = oringinalText.Substring(pl.Position, pl.Length);
            this.Pos = pl.WordAttr.Pos;
            this.Frequency = pl.WordAttr.Frequency;
            this.WordType = WordType.SimpleChinese;
            this.Position = pl.Position;

            switch (pl.Level)
            {
                case 0:
                    this.Rank = Setting.PanGuSettings.Config.Parameters.BestRank;
                    break;
                case 1:
                    this.Rank = Setting.PanGuSettings.Config.Parameters.SecRank;
                    break;
                case 2:
                    this.Rank = Setting.PanGuSettings.Config.Parameters.ThirdRank;
                    break;
                case 3:
                    this.Rank = Setting.PanGuSettings.Config.Parameters.SingleRank;
                    break;
                default:
                    this.Rank = Setting.PanGuSettings.Config.Parameters.BestRank;
                    break;
            }

        }
    }
}
