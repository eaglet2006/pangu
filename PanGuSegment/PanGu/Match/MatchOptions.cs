using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace PanGu.Match
{
    [Serializable]
    public class MatchOptions
    {
        /// <summary>
        /// 中文人名识别
        /// </summary>
        public bool ChineseNameIdentify = false;

        /// <summary>
        /// 词频优先
        /// </summary>
        public bool FrequencyFirst = false;

        /// <summary>
        /// 多元分词
        /// </summary>
        public bool MultiDimensionality = true;
    }
}
