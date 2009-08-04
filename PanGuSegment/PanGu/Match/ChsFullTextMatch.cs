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

        const int TopRecord = 3;

        /// <summary>
        /// Build tree 
        /// </summary>
        /// <param name="pl">position length list</param>
        /// <param name="count">position length list count</param>
        /// <param name="parent">parent node</param>
        /// <param name="curIndex">current index of position length list</param>
        private void BuildTree(Dict.PositionLength[] pl, int stringLength, int count, Node parent, int curIndex)
        {
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

        public LinkedList<WordInfo> Match(PanGu.Dict.PositionLength[] positionLenArr, int stringLength, int count)
        {
            LinkedList<WordInfo> result = new LinkedList<WordInfo>();

            if (count == 0)
            {
                return result;
            }

            BuildTree(positionLenArr, stringLength, count, _Root, 0);

            Node[] leafNodeArray = _LeafNodeList.Items;

            Framework.QuickSort<Node>.TopSort(leafNodeArray,
                _LeafNodeList.Count, (int)Math.Min(TopRecord, _LeafNodeList.Count), new NodeComparer());

            int j = 0;
            foreach (Node node in leafNodeArray)
            {
                if (j >= TopRecord)
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

            return result;
        }

        #endregion
    }
}
