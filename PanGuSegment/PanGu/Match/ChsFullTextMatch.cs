/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace PanGu.Match
{
    public class ChsFullTextMatch: IChsFullTextMatch
    {
        class NodeComparer : IComparer<Node>
        {
            #region IComparer<Node> Members

            public int Compare(Node x, Node y)
            {
                if (x.SpaceCount < y.SpaceCount)
                {
                    return -1;
                }
                else if (x.SpaceCount > y.SpaceCount)
                {
                    return 1;
                }
                else
                {
                    if (x.AboveCount < y.AboveCount)
                    {
                        return -1;
                    }
                    else if (x.AboveCount > y.AboveCount)
                    {
                        return 1;
                    }
                    else
                    {
                        if (x.SingleWordCount < y.SingleWordCount)
                        {
                            return -1;
                        }
                        else if (x.SingleWordCount > y.SingleWordCount)
                        {
                            return 1;
                        }
                        else
                        {
                            if (x.FreqSum > y.FreqSum)
                            {
                                return -1;
                            }
                            else if (x.FreqSum < y.FreqSum)
                            {
                                return 1;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    }
                }

            }

            #endregion
        }

        class Node : IComparable<Node>
        {
            public int AboveCount;
            public int SpaceCount;
            public double FreqSum;
            public int SingleWordCount;
            
            public Dict.PositionLength PositionLength;
            public Node Parent;

            public Node()
            {
                AboveCount = 0;
            }

            public Node(Dict.PositionLength pl, Node parent, int aboveCount, 
                int spaceCount, int singleWordCount, double freqSum)
            {
                PositionLength = pl;
                Parent = parent;
                AboveCount = aboveCount;
                SpaceCount = spaceCount;
                SingleWordCount = singleWordCount;
                FreqSum = freqSum;
            }


            #region IComparable<Node> Members

            public int CompareTo(Node other)
            {
                if (this.SpaceCount < other.SpaceCount)
                {
                    return -1;
                }
                else if (this.SpaceCount > other.SpaceCount)
                {
                    return 1;
                }
                else
                {
                    if (this.AboveCount < other.AboveCount)
                    {
                        return -1;
                    }
                    else if (this.AboveCount > other.AboveCount)
                    {
                        return 1;
                    }
                    else
                    {
                        if (this.FreqSum > other.FreqSum)
                        {
                            return -1;
                        }
                        else if (this.FreqSum < other.FreqSum)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }

            #endregion
        }

        Node _Root = new Node();

        Framework.AppendList<Node> _LeafNodeList = new PanGu.Framework.AppendList<Node>();
        List<Dict.PositionLength[]> _AllCombinations = new List<PanGu.Dict.PositionLength[]>();
        Dict.WordDictionary _WordDict;

        const int TopRecord = 3;
        const POS SingleWordMask = POS.POS_D_C | POS.POS_D_P | POS.POS_D_R | POS.POS_D_U;

        /// <summary>
        /// Build tree 
        /// </summary>
        /// <param name="pl">position length list</param>
        /// <param name="count">position length list count</param>
        /// <param name="parent">parent node</param>
        /// <param name="curIndex">current index of position length list</param>
        private void BuildTree(Dict.PositionLength[] pl, int stringLength, int count, Node parent, int curIndex)
        {
            //嵌套太多的情况一般很少发生，如果发生，强行中断，以免造成博弈树遍历层次过多
            //降低系统效率
            if (_LeafNodeList.Count > 8192)
            {
                return;
            }

            if (curIndex < count - 1)
            {
                if (pl[curIndex + 1].Position == pl[curIndex].Position)
                {
                    BuildTree(pl, stringLength, count, parent, curIndex + 1);
                }
            }

            int spaceCount = parent.SpaceCount + pl[curIndex].Position - (parent.PositionLength.Position + parent.PositionLength.Length);
            int singleWordCount = parent.SingleWordCount + (pl[curIndex].Length == 1 ? 1 : 0);
            double freqSum = 0;

            if (_Options != null)
            {
                if (_Options.FrequencyFirst)
                {
                    freqSum = parent.FreqSum + pl[curIndex].WordAttr.Frequency;
                }
            }

            Node curNode = new Node(pl[curIndex], parent, parent.AboveCount + 1, spaceCount, singleWordCount, freqSum);

            int cur = curIndex + 1;
            while (cur < count)
            {
                if (pl[cur].Position >= pl[curIndex].Position + pl[curIndex].Length)
                {
                    BuildTree(pl, stringLength, count, curNode, cur);
                    break;
                }

                cur++;
            }

            if (cur >= count)
            {
                curNode.SpaceCount += stringLength - curNode.PositionLength.Position - curNode.PositionLength.Length;
                _LeafNodeList.Add(curNode);
            }

        }

        #region IChsFullTextMatch Members

        private MatchOptions _Options = null;
        public MatchOptions Options
        {
            get
            {
                return _Options;
            }
            set
            {
                _Options = value;
            }
        }


        private MatchParameter _Parameters = null;
        public MatchParameter Parameters
        {
            get
            {
                return _Parameters;
            }
            set
            {
                _Parameters = value;
            }
        }

        private ICollection<Dict.PositionLength> MergeAllCombinations(int redundancy)
        {
            LinkedList<Dict.PositionLength> result = new LinkedList<PanGu.Dict.PositionLength>();

            if ((redundancy == 0 || !_Options.MultiDimensionality) && !_Options.ForceSingleWord)
            {
                return _AllCombinations[0];
            }

            int i = 0;

            LinkedListNode<Dict.PositionLength> cur;

            bool forceOnce = false;

            Loop:

            while (i <= redundancy && i < _AllCombinations.Count)
            {
                cur = result.First;

                for (int j = 0; j < _AllCombinations[i].Length; j++)
                {
                    _AllCombinations[i][j].Level = i;

                    if (cur != null)
                    {
                        while (cur.Value.Position < _AllCombinations[i][j].Position)
                        {
                            cur = cur.Next;

                            if (cur == null)
                            {
                                break;
                            }
                        }

                        if (cur != null)
                        {
                            if (cur.Value.Position != _AllCombinations[i][j].Position ||
                                cur.Value.Length != _AllCombinations[i][j].Length)
                            {
                                result.AddBefore(cur, _AllCombinations[i][j]);
                            }
                        }
                        else
                        {
                            result.AddLast(_AllCombinations[i][j]);
                        }
                    }
                    else
                    {
                        result.AddLast(_AllCombinations[i][j]);
                    }
                }

                i++;
            }

            if (_Options.ForceSingleWord && !forceOnce)
            {
                i = _AllCombinations.Count - 1;
                redundancy = i;
                forceOnce = true;
                goto Loop;
            }

            return result;
        }

        private bool IsKnownSingleWord(int[] masks, int index, string orginalText)
        {

            int state = masks[index];
            if (state == 2)
            {
                return false;
            }

            if (state == 1)
            {
                if (!_Options.UnknownWordIdentify)
                {
                    return false;
                }

                //如果单字是连词、助词、介词、代词
                WordAttribute wa = _WordDict.GetWordAttr(orginalText[index].ToString());

                if (wa != null)
                {
                    if ((wa.Pos & SingleWordMask) != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        private List<WordInfo> GetUnknowWords(int[] masks, string orginalText, out bool needRemoveSingleWord)
        {
            List<WordInfo> unknownWords = new List<WordInfo>();

            //找到所有未登录词
            needRemoveSingleWord = false;

            int j = 0;
            bool begin = false;
            int beginPosition = 0;
            while (j < masks.Length)
            {
                if (_Options.UnknownWordIdentify)
                {

                    if (!begin)
                    {
                        if (IsKnownSingleWord(masks, j, orginalText))
                        {
                            begin = true;
                            beginPosition = j;
                        }
                    }
                    else
                    {
                        bool mergeUnknownWord = true;

                        if (!IsKnownSingleWord(masks, j, orginalText))
                        {
                            if (j - beginPosition <= 2)
                            {
                                for (int k = beginPosition; k < j; k++)
                                {
                                    mergeUnknownWord = false;

                                    if (masks[k] != 1)
                                    {
                                        string word = orginalText.Substring(k, 1);
                                        WordInfo wi = new WordInfo();
                                        wi.Word = word;
                                        wi.Position = k;
                                        wi.WordType = WordType.None;
                                        wi.Rank = _Parameters.UnknowRank;
                                        unknownWords.Add(wi);
                                    }
                                }
                            }
                            else
                            {
                                for (int k = beginPosition; k < j; k++)
                                {
                                    if (masks[k] == 1)
                                    {
                                        masks[k] = 11;
                                        needRemoveSingleWord = true;
                                    }
                                }
                            }

                            begin = false;

                            if (mergeUnknownWord)
                            {
                                string word = orginalText.Substring(beginPosition,
                                    j - beginPosition);
                                WordInfo wi = new WordInfo();
                                wi.Word = word;
                                wi.Position = beginPosition;
                                wi.WordType = WordType.None;
                                wi.Rank = _Parameters.UnknowRank;
                                unknownWords.Add(wi);
                            }
                        }
                    }
                }
                else
                {
                    if (IsKnownSingleWord(masks, j, orginalText))
                    {
                        WordInfo wi = new WordInfo();
                        wi.Word = orginalText[j].ToString();
                        wi.Position = j;
                        wi.WordType = WordType.None;
                        wi.Rank = _Parameters.UnknowRank;
                        unknownWords.Add(wi);
                    }
                }

                j++;
            }

            if (begin && _Options.UnknownWordIdentify)
            {
                bool mergeUnknownWord = true;

                if (j - beginPosition <= 2)
                {
                    for (int k = beginPosition; k < j; k++)
                    {
                        mergeUnknownWord = false;

                        if (masks[k] != 1)
                        {
                            string word = orginalText.Substring(k, 1);
                            WordInfo wi = new WordInfo();
                            wi.Word = word;
                            wi.Position = k;
                            wi.WordType = WordType.None;
                            wi.Rank = _Parameters.UnknowRank;
                            unknownWords.Add(wi);
                        }
                    }
                }
                else
                {
                    for (int k = beginPosition; k < j; k++)
                    {
                        if (masks[k] == 1)
                        {
                            masks[k] = 11;
                            needRemoveSingleWord = true;
                        }
                    }
                }

                begin = false;

                if (mergeUnknownWord)
                {

                    string word = orginalText.Substring(beginPosition,
                        j - beginPosition);
                    WordInfo wi = new WordInfo();
                    wi.Word = word;
                    wi.Position = beginPosition;
                    wi.WordType = WordType.None;
                    wi.Rank = _Parameters.UnknowRank;
                    unknownWords.Add(wi);
                }
            }

            return unknownWords;
        }

        public ChsFullTextMatch(Dict.WordDictionary wordDict)
        {
            _WordDict = wordDict;
        }

        public SuperLinkedList<WordInfo> Match(PanGu.Dict.PositionLength[] positionLenArr, string orginalText, int count)
        {
            if (_Options == null)
            {
                _Options = Setting.PanGuSettings.Config.MatchOptions;
            }

            if (_Parameters == null)
            {
                _Parameters = Setting.PanGuSettings.Config.Parameters;
            }

            int[] masks = new int[orginalText.Length];
            int redundancy = _Parameters.Redundancy;

            SuperLinkedList<WordInfo> result = new SuperLinkedList<WordInfo>();

            if (count == 0)
            {
                if (_Options.UnknownWordIdentify)
                {
                    WordInfo wi = new WordInfo();
                    wi.Word = orginalText;
                    wi.Position = 0;
                    wi.WordType = WordType.None;
                    wi.Rank = 1;
                    result.AddFirst(wi);
                    return result;
                }
                else
                {
                    int position = 0;
                    foreach (char c in orginalText)
                    {
                        WordInfo wi = new WordInfo();
                        wi.Word = c.ToString();
                        wi.Position = position++;
                        wi.WordType = WordType.None;
                        wi.Rank = 1;
                        result.AddLast(wi);
                    }

                    return result;
                }
            }

            BuildTree(positionLenArr, orginalText.Length, count, _Root, 0);

            Node[] leafNodeArray = _LeafNodeList.Items;

            Framework.QuickSort<Node>.TopSort(leafNodeArray,
                _LeafNodeList.Count, (int)Math.Min(TopRecord, _LeafNodeList.Count), new NodeComparer());

            int j = 0;
            // 获取前TopRecord个单词序列
            foreach (Node node in leafNodeArray)
            {
                if (j >= TopRecord || j >= _LeafNodeList.Count)
                {
                    break;
                }

                Dict.PositionLength[] comb = new PanGu.Dict.PositionLength[node.AboveCount];

                int i = node.AboveCount - 1;
                Node cur = node;

                while (i >= 0)
                {
                    comb[i] = cur.PositionLength;
                    cur = cur.Parent;
                    i--;
                }

                _AllCombinations.Add(comb);

                j++;
            }

            //Force single word
            //强制一元分词
            if (_Options.ForceSingleWord)
            {
                Dict.PositionLength[] comb = new PanGu.Dict.PositionLength[orginalText.Length];

                for (int i = 0; i < comb.Length; i++)
                {
                    PanGu.Dict.PositionLength pl = new PanGu.Dict.PositionLength(i, 1, new WordAttribute(orginalText[i].ToString(), POS.POS_UNK, 0));
                    pl.Level = 3;
                    comb[i] = pl;
                }

                _AllCombinations.Add(comb);
            }

            if (_AllCombinations.Count > 0)
            {
                ICollection<Dict.PositionLength> positionCollection = MergeAllCombinations(redundancy);

                foreach (Dict.PositionLength pl in positionCollection)
                //for (int i = 0; i < _AllCombinations[0].Length; i++)
                {
                    //result.AddLast(new WordInfo(_AllCombinations[0][i], orginalText));
                    result.AddLast(new WordInfo(pl, orginalText, _Parameters));
                    if (pl.Length > 1)
                    {
                        for (int k = pl.Position;
                            k < pl.Position + pl.Length; k++)
                        {
                            masks[k] = 2;
                        }
                    }
                    else
                    {
                        masks[pl.Position] = 1;
                    }
                }
            }

            #region 合并未登录词

            bool needRemoveSingleWord;
            List<WordInfo> unknownWords = GetUnknowWords(masks, orginalText, out needRemoveSingleWord);

            //合并到结果序列的对应位置中
            if (unknownWords.Count > 0)
            {
                SuperLinkedListNode<WordInfo> cur = result.First;

                if (needRemoveSingleWord && !_Options.ForceSingleWord)
                {
                    //Remove single word need be remvoed

                    while (cur != null)
                    {
                        if (cur.Value.Word.Length == 1)
                        {
                            if (masks[cur.Value.Position] == 11)
                            {
                                SuperLinkedListNode<WordInfo> removeItem = cur;

                                cur = cur.Next;

                                result.Remove(removeItem);

                                continue;
                            }
                        }

                        cur = cur.Next;
                    }
                }

                cur = result.First;

                j = 0;

                while (cur != null)
                {
                    if (cur.Value.Position >= unknownWords[j].Position)
                    {
                        result.AddBefore(cur, unknownWords[j]);
                        j++;
                        if (j >= unknownWords.Count)
                        {
                            break;
                        }
                    }

                    if (cur.Value.Position < unknownWords[j].Position)
                    {
                        cur = cur.Next;
                    }
                }

                while (j < unknownWords.Count)
                {
                    result.AddLast(unknownWords[j]);
                    j++;
                }
            }


            #endregion



            return result;
        }

        #endregion
    }
}
