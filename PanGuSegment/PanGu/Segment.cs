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

using PanGu.Framework;

namespace PanGu
{
    public class Segment
    {
    //    const string PATTERNS = @"[０-９\d]+\%|[０-９\d]{1,2}月|[０-９\d]{1,2}日|[０-９\d]{1,4}年|" +
    //@"[０-９\d]{1,4}-[０-９\d]{1,2}-[０-９\d]{1,2}|" +
    //@"\s+|" +
    //@"[０-９\d]+|[^ａ-ｚＡ-Ｚa-zA-Z0-9０-９\u4e00-\u9fa5]|[ａ-ｚＡ-Ｚa-zA-Z]+|[\u4e00-\u9fa5]+";

        const string PATTERNS = @"([０-９\d]+)|([ａ-ｚＡ-Ｚa-zA-Z_]+)";

        #region Private fields

        static object _LockObj = new object();
        static bool _Inited = false;

        internal static Dict.WordDictionary _WordDictionary = null;
        internal static Dict.ChsName _ChsName = null;
        internal static Dict.StopWord _StopWord = null;

        static Dict.DictionaryLoader _DictLoader;
        private Match.MatchOptions _Options;
        private Match.MatchParameter _Parameters;
        #endregion


        #region Merge functions

        /// <summary>
        /// 合并英文专用词。
        /// 如果字典中有英文专用词如U.S.A, C++.C#等
        /// 需要对初步分词后的英文和字母进行合并
        /// </summary>
        /// <param name="words"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        //private String MergeEnglishSpecialWord(CExtractWords extractWords, ArrayList words, int start, ref int end)
        //{
        //    StringBuilder str = new StringBuilder();

        //    int i;

        //    for (i = start; i < words.Count; i++)
        //    {
        //        string word = (string)words[i];

        //        //word 为空或者为空格回车换行等分割符号，中断扫描
        //        if (word.Trim() == "")
        //        {
        //            break;
        //        }

        //        //如果遇到中文，中断扫描
        //        if (word[0] >= 0x4e00 && word[0] <= 0x9fa5)
        //        {
        //            break;
        //        }

        //        str.Append(word);
        //    }

        //    String mergeString = str.ToString();
        //    List<T_WordInfo> exWords = extractWords.ExtractFullText(mergeString);

        //    if (exWords.Count == 1)
        //    {
        //        T_WordInfo info = (T_WordInfo)exWords[0];
        //        if (info.Word.Length == mergeString.Length)
        //        {
        //            end = i;
        //            return mergeString;
        //        }
        //    }

        //    return null;

        //}

        private bool MergeEnglishSpecialWord(string orginalText, SuperLinkedList<WordInfo> wordInfoList, ref SuperLinkedListNode<WordInfo> current)
        {
            SuperLinkedListNode<WordInfo> cur = current;

            cur = cur.Next;

            int last = -1;

            while (cur != null)
            {
                if (cur.Value.WordType == WordType.Symbol || cur.Value.WordType == WordType.English)
                {
                    last = cur.Value.Position + cur.Value.Word.Length;
                    cur = cur.Next;
                }
                else
                {
                    break;
                }
            }


            if (last >= 0)
            {
                int first = current.Value.Position;

                string newWord = orginalText.Substring(first, last - first);

                WordAttribute wa = _WordDictionary.GetWordAttr(newWord);

                if (wa == null)
                {
                    return false;
                }

                while (current != cur)
                {
                    SuperLinkedListNode<WordInfo> removeItem = current;
                    current = current.Next;
                    wordInfoList.Remove(removeItem);
                }

                WordInfo newWordInfo = new WordInfo(new PanGu.Dict.PositionLength(first, last - first, 
                    wa), orginalText, _Parameters);

                newWordInfo.WordType = WordType.English;
                newWordInfo.Rank = _Parameters.EnglishRank;

                if (current == null)
                {
                    wordInfoList.AddLast(newWordInfo);
                }
                else
                {
                    wordInfoList.AddBefore(current, newWordInfo);
                }

                return true;
            }


            return false;

        }

        #endregion

        private SuperLinkedList<WordInfo> GetInitSegment(string text)
        {
            SuperLinkedList<WordInfo> result = new SuperLinkedList<WordInfo>();

            Framework.Lexical lexical = new PanGu.Framework.Lexical(text);

            DFAResult dfaResult;

            for (int i = 0; i < text.Length; i++)
            {
                dfaResult = lexical.Input(text[i], i);

                switch (dfaResult)
                {
                    case DFAResult.Continue:
                        continue;
                    case DFAResult.Quit:
                        result.AddLast(lexical.OutputToken);
                        break;
                    case DFAResult.ElseQuit:
                        result.AddLast(lexical.OutputToken);
                        if (lexical.OldState != 255)
                        {
                            i--;
                        }

                        break;
                }

            }

            dfaResult = lexical.Input(0, text.Length);

            switch (dfaResult)
            {
                case DFAResult.Continue:
                    break;
                case DFAResult.Quit:
                    result.AddLast(lexical.OutputToken);
                    break;
                case DFAResult.ElseQuit:
                    result.AddLast(lexical.OutputToken);
                    break;
            }

            return result;
        }

        private SuperLinkedList<WordInfo> PreSegment(String text)
        {
            SuperLinkedList<WordInfo> result = GetInitSegment(text);

            SuperLinkedListNode<WordInfo> cur = result.First;

            while (cur != null)
            {
                if (_Options.IgnoreSpace)
                {
                    if (cur.Value.WordType == WordType.Space)
                    {
                        SuperLinkedListNode<WordInfo> lst = cur;
                        cur = cur.Next;
                        result.Remove(lst);
                        continue;
                    }
                }

                switch (cur.Value.WordType)
                {
                    case WordType.SimplifiedChinese:

                        string inputText = cur.Value.Word;

                        WordType originalWordType = WordType.SimplifiedChinese;

                        if (_Options.TraditionalChineseEnabled)
                        {
                            string simplified = Microsoft.VisualBasic.Strings.StrConv(cur.Value.Word, Microsoft.VisualBasic.VbStrConv.SimplifiedChinese, 0);

                            if (simplified != cur.Value.Word)
                            {
                                originalWordType = WordType.TraditionalChinese;
                                inputText = simplified;
                            }
                        }

                        PanGu.Framework.AppendList<Dict.PositionLength> pls = _WordDictionary.GetAllMatchs(inputText, _Options.ChineseNameIdentify);
                        PanGu.Match.ChsFullTextMatch chsMatch = new PanGu.Match.ChsFullTextMatch(_WordDictionary);
                        chsMatch.Options = _Options;
                        chsMatch.Parameters = _Parameters;
                        SuperLinkedList<WordInfo> chsMatchWords = chsMatch.Match(pls.Items, cur.Value.Word, pls.Count);

                        SuperLinkedListNode<WordInfo> curChsMatch = chsMatchWords.First;
                        while (curChsMatch != null)
                        {
                            WordInfo wi = curChsMatch.Value;

                            wi.Position += cur.Value.Position;
                            wi.OriginalWordType = originalWordType;
                            wi.WordType = originalWordType;

                            if (_Options.OutputSimplifiedTraditional)
                            {
                                if (_Options.TraditionalChineseEnabled)
                                {
                                    string newWord;
                                    WordType wt;

                                    if (originalWordType == WordType.SimplifiedChinese)
                                    {
                                        newWord = Microsoft.VisualBasic.Strings.StrConv(wi.Word, 
                                            Microsoft.VisualBasic.VbStrConv.TraditionalChinese, 0);
                                        wt = WordType.TraditionalChinese;
                                    }
                                    else
                                    {
                                        newWord = Microsoft.VisualBasic.Strings.StrConv(wi.Word, 
                                            Microsoft.VisualBasic.VbStrConv.SimplifiedChinese, 0);
                                        wt = WordType.SimplifiedChinese;
                                    }

                                    if (newWord != wi.Word)
                                    {
                                        WordInfo newWordInfo = new WordInfo(wi);
                                        newWordInfo.Word = newWord;
                                        newWordInfo.OriginalWordType = originalWordType;
                                        newWordInfo.WordType = wt;
                                        newWordInfo.Rank = _Parameters.SimplifiedTraditionalRank;
                                        newWordInfo.Position = wi.Position;
                                        chsMatchWords.AddBefore(curChsMatch, newWordInfo);
                                    }
                                }
                            }

                            curChsMatch = curChsMatch.Next;
                        }

                        SuperLinkedListNode<WordInfo> lst = result.AddAfter(cur, chsMatchWords);
                        SuperLinkedListNode<WordInfo> removeItem = cur;
                        cur = lst.Next;
                        result.Remove(removeItem);
                        break;
                    case WordType.English:
                        cur.Value.Rank = _Parameters.EnglishRank;
                        List<string> output;

                        if (_Options.MultiDimensionality)
                        {
                            if (Framework.Regex.GetMatchStrings(cur.Value.Word, PATTERNS, true, out output))
                            {
                                if (output.Count > 1)
                                {
                                    int position = cur.Value.Position;

                                    foreach (string splitWord in output)
                                    {
                                        if (string.IsNullOrEmpty(splitWord))
                                        {
                                            continue;
                                        }

                                        WordInfo wi;

                                        if (splitWord[0] >= '0' && splitWord[0] <= '9')
                                        {
                                            wi = new WordInfo(splitWord, POS.POS_A_M, 1);
                                            wi.Position = position;
                                            wi.Rank = _Parameters.NumericRank;
                                        }
                                        else
                                        {
                                            wi = new WordInfo(splitWord, POS.POS_A_NX, 1);
                                            wi.Position = position;
                                            wi.Rank = _Parameters.EnglishRank;
                                        }

                                        result.AddBefore(cur, wi);
                                        position += splitWord.Length;
                                    }
                                }
                            }
                        }

                        if (!MergeEnglishSpecialWord(text, result, ref cur))
                        {
                            cur = cur.Next;
                        }

                        break;
                    case WordType.Numeric:
                        cur.Value.Rank = _Parameters.NumericRank;
                        cur = cur.Next;
                        break;
                    case WordType.Symbol:
                        cur.Value.Rank = _Parameters.SymbolRank;
                        cur = cur.Next;
                        break;
                    default:
                        cur = cur.Next;
                        break;
                }

            }


            return result;

        }

        private void FilterStopWord(SuperLinkedList<WordInfo> wordInfoList)
        {
            if (wordInfoList == null)
            {
                return;
            }

            SuperLinkedListNode<WordInfo> cur = wordInfoList.First;

            while (cur != null)
            {
                if (_StopWord.IsStopWord(cur.Value.Word))
                {
                    SuperLinkedListNode<WordInfo> removeItem = cur;
                    cur = cur.Next;
                    wordInfoList.Remove(removeItem);
                }
                else
                {
                    cur = cur.Next;
                }
            }
        }

        #region Public methods
        public ICollection<WordInfo> DoSegment(string text)
        {
            return DoSegment(text, null, null);
        }

        public ICollection<WordInfo> DoSegment(string text, Match.MatchOptions options)
        {
            return DoSegment(text, options, null);
        }

        public ICollection<WordInfo> DoSegment(string text, Match.MatchOptions options, Match.MatchParameter parameters)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new SuperLinkedList<WordInfo>();
            }

            try
            {
                Dict.DictionaryLoader.Lock.Enter(PanGu.Framework.Lock.Mode.Share);
                _Options = options;
                _Parameters = parameters;

                if (_Options == null)
                {
                    _Options = Setting.PanGuSettings.Config.MatchOptions;
                }

                if (_Parameters == null)
                {
                    _Parameters = Setting.PanGuSettings.Config.Parameters;
                }

                Init();

                SuperLinkedList<WordInfo> result = PreSegment(text);

                if (_Options.FilterStopWords)
                {
                    FilterStopWord(result);
                }

                return result;
            }
            finally
            {
                Dict.DictionaryLoader.Lock.Leave();
            }
        }

        #endregion

        #region Initialization

        static private void LoadDictionary()
        {
            _WordDictionary = new PanGu.Dict.WordDictionary();
            string dir = Setting.PanGuSettings.Config.GetDictionaryPath();
            _WordDictionary.Load(dir + "Dict.Dct");

            _ChsName = new PanGu.Dict.ChsName();
            _ChsName.LoadChsName(Setting.PanGuSettings.Config.GetDictionaryPath());

            _WordDictionary.ChineseName = _ChsName;

            _StopWord = new PanGu.Dict.StopWord();
            _StopWord.LoadStopwordsDict(dir + "Stopword.txt");

            _DictLoader = new PanGu.Dict.DictionaryLoader(Setting.PanGuSettings.Config.GetDictionaryPath());
        }

        public static void Init()
        {
            Init(null);
        }

        public static void Init(string fileName)
        {
            lock (_LockObj)
            {
                if (_Inited)
                {
                    return;
                }

                if (fileName == null)
                {
                    Setting.SettingLoader loader = new PanGu.Setting.SettingLoader();
                }
                else
                {
                    Setting.SettingLoader loader = new PanGu.Setting.SettingLoader(fileName);
                }

                LoadDictionary();

                _Inited = true;
            }
        }



        #endregion
    }
}
