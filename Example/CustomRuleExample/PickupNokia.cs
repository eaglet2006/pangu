using System;
using System.Collections.Generic;
using System.Text;
using PanGu;

namespace CustomRuleExample
{
    /// <summary>
    /// 这个规则提取单词中的Nokia
    /// </summary>
    public class PickupNokia : ICustomRule
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

        public void AfterSegment(SuperLinkedList<WordInfo> result)
        {
            SuperLinkedListNode<WordInfo> node = result.First;

            while (node != null)
            {
                if (node.Value.WordType == WordType.English)
                {
                    int position = node.Value.Word.IndexOf("Nokia", 0, StringComparison.CurrentCultureIgnoreCase);
                    if (position >= 0 && 
                        !node.Value.Word.Equals("Nokia", StringComparison.CurrentCultureIgnoreCase))
                    {
                        WordInfo wordinfo = new WordInfo("Nokia", node.Value.Position + position, node.Value.Pos,
                            node.Value.Frequency, node.Value.Rank, node.Value.WordType, node.Value.OriginalWordType);
                        node = result.AddAfter(node, wordinfo);
                    }
                }

                node = node.Next;
            }
        }

        #endregion
    }
}
