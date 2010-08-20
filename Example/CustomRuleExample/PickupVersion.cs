using System;
using System.Collections.Generic;
using System.Text;
using PanGu;

namespace CustomRuleExample
{
    /// <summary>
    /// 这个规则用于将文章中的版本号单独提出来
    /// V1.2.3.4 分词结果为
    /// v/1.2/1.2.3/1.2.3.4
    /// 这个规则要工作正常，需要将 EnglishMultiDimensionality 开关打开
    /// </summary>
    public class PickupVersion : ICustomRule
    {
        private string _Text;

        #region ICustomRule Members

        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value;
            }
        }

        /// <summary>
        /// 提取版本号
        /// </summary>
        /// <param name="result">盘古分词的结果</param>
        /// <param name="vWordNode">V 这个字符的第一个出现位置</param>
        /// <param name="lastNode">版本号的最后一个词</param>
        /// <param name="versionBeginPosition">版本号第一个词的起始位置</param>
        private void Pickup(SuperLinkedList<WordInfo> result, SuperLinkedListNode<WordInfo> vWordNode,
            SuperLinkedListNode<WordInfo> lastNode, int versionBeginPosition)
        {
            SuperLinkedListNode<WordInfo> node = vWordNode.Next;
            int lastPosition = lastNode.Value.Position + lastNode.Value.Word.Length;

            SuperLinkedListNode<WordInfo> end = lastNode.Next;

            while (node != end)
            {
                result.Remove(node);
                node = vWordNode.Next;
            }

            if (vWordNode.Value.Word == "V")
            {
                vWordNode.Value.Word = "v";
            }

            string version = _Text.Substring(versionBeginPosition, lastPosition - versionBeginPosition);

            int dotPosition = 0;
            int dotCount = 0;

            WordInfo verWord = null;
            dotPosition = version.IndexOf('.', dotPosition);

            while (dotPosition > 0)
            {
                verWord = null;

                if (dotCount > 0) //第一个点之前的版本号不提取
                {
                    //提取前n个子版本号
                    verWord = new WordInfo(version.Substring(0, dotPosition), POS.POS_D_K, 0);
                    verWord.Rank = 1; //这里设置子版本号的权重
                    verWord.Position = versionBeginPosition;
                    verWord.WordType = WordType.None;
                }

                dotCount++;

                dotPosition = version.IndexOf('.', dotPosition + 1);

                if (verWord != null)
                {
                    result.AddAfter(vWordNode, verWord);
                }
            }

            //提取完整版本号
            verWord = new WordInfo(version, POS.POS_D_K, 0);
            verWord.Rank = 5; //这里设置完整版本号的权重
            verWord.Position = versionBeginPosition;
            verWord.WordType = WordType.None;
            result.AddAfter(vWordNode, verWord);

        }

        public void AfterSegment(SuperLinkedList<WordInfo> result)
        {
            SuperLinkedListNode<WordInfo> node = result.First;

            SuperLinkedListNode<WordInfo> vWordNode = null;
            SuperLinkedListNode<WordInfo> lastNode = null;
            bool isVersion = false;
            int versionBeginPosition = -1;

            while (node != null)
            {
                if (vWordNode == null)
                {
                    if (node.Value.WordType == WordType.English)
                    {
                        //匹配 V 这个字符，作为版本号的开始
                        if (node.Value.Word.Length == 1)
                        {
                            if (node.Value.Word[0] == 'v' || node.Value.Word[0] == 'V')
                            {
                                vWordNode = node;
                                lastNode = node;
                            }
                        }
                    }
                }
                else if (vWordNode != null)
                {
                    //如果V有多元分词情况，忽略，跳到下一个
                    if (node.Value.Position == vWordNode.Value.Position)
                    {
                        node = node.Next;
                        continue;
                    }

                    //匹配数字或点
                    if (node.Value.WordType == WordType.Numeric ||
                        node.Value.Word == ".")
                    {
                        if (node.Value.Position - (lastNode.Value.Position + lastNode.Value.Word.Length) <= 1)
                        {
                            if (versionBeginPosition < 0)
                            {
                                versionBeginPosition = node.Value.Position;
                            }

                            isVersion = true;
                            lastNode = node;

                            node = node.Next;
                            continue;
                        }
                    }

                    if (isVersion)
                    {
                        //如果是版本号，提取版本号
                        Pickup(result, vWordNode, lastNode, versionBeginPosition);
                        vWordNode = null;
                        lastNode = null;
                        versionBeginPosition = -1;
                        isVersion = false;
                        continue;
                    }
                }

                node = node.Next;
            }

            if (isVersion)
            {
                //如果是版本号，提取版本号
                Pickup(result, vWordNode, lastNode, versionBeginPosition);
            }
        }

        #endregion

    }
}
