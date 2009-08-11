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

        /// <summary>
        /// 过滤停用词
        /// </summary>
        public bool FilterStopWords = true;

        /// <summary>
        /// 忽略空格、回车、Tab
        /// </summary>
        public bool IgnoreSpace = true;

        /// <summary>
        /// 强制一元分词
        /// </summary>
        public bool ForceSingleWord = false;

        /// <summary>
        /// 繁体中文开关
        /// </summary>
        public bool TraditionalChineseEnabled = false;

        /// <summary>
        /// 同时输出简体和繁体
        /// </summary>
        public bool OutputSimplifiedTraditional = false;


    }
}
