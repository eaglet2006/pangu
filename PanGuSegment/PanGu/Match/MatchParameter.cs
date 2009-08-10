using System;
using System.Collections.Generic;
using System.Text;

namespace PanGu.Match
{
    [Serializable]
    public class MatchParameter
    {
 
        private int _Redundancy;

        /// <summary>
        /// 多元分词冗余度
        /// </summary>
        public int Redundancy
        {
            get
            {
                return _Redundancy;
            }

            set
            {
                if (value < 0)
                {
                    _Redundancy = 0;
                }
                else if (value >= 3)
                {
                    _Redundancy = 2;
                }
                else
                {
                    _Redundancy = value;
                }
            }
        }

        /// <summary>
        /// 未登录词权值
        /// </summary>
        public int UnknowRank = 1;

        /// <summary>
        /// 最匹配词权值
        /// </summary>
        public int BestRank = 5;

        /// <summary>
        /// 次匹配词权值
        /// </summary>
        public int SecRank = 3;

        /// <summary>
        /// 再次匹配词权值
        /// </summary>
        public int ThirdRank = 2;

        /// <summary>
        /// 强行输出的单字的权值
        /// </summary>
        public int SingleRank = 1;

        /// <summary>
        /// 数字的权值
        /// </summary>
        public int NumericRank = 1;

        /// <summary>
        /// 英文词汇权值
        /// </summary>
        public int EnglishRank = 5;

        /// <summary>
        /// 符号的权值
        /// </summary>
        public int SymbolRank = 1;
    }
}
