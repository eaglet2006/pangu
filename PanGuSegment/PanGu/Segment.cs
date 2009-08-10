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
        const string PATTERNS = @"[０-９\d]+\%|[０-９\d]{1,2}月|[０-９\d]{1,2}日|[０-９\d]{1,4}年|" +
    @"[０-９\d]{1,4}-[０-９\d]{1,2}-[０-９\d]{1,2}|" +
    @"\s+|" +
    @"[０-９\d]+|[^ａ-ｚＡ-Ｚa-zA-Z0-9０-９\u4e00-\u9fa5]|[ａ-ｚＡ-Ｚa-zA-Z]+|[\u4e00-\u9fa5]+";

        #region Private fields

        static object _LockObj = new object();

        static Dict.WordDictionary _WordDictionary = null;
        static Match.ChsName _ChsName = null;

        private Match.MatchOptions _Options;
        private Match.MatchParameter _Parameters;
        #endregion


        #region Merge functions
        /// <summary>
        /// 合并浮点数
        /// </summary>
        /// <param name="words"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        //private String MergeFloat(ArrayList words, int start, ref int end)
        //{
        //    StringBuilder str = new StringBuilder();

        //    int dotCount = 0;
        //    end = start;
        //    int i ;

        //    for (i = start; i < words.Count; i++)
        //    {
        //        string word = (string)words[i];

        //        if (word == "")
        //        {
        //            break;
        //        }
                
        //        if ((word[0] >= '0' && word[0] <= '9')
        //            || (word[0] >= '０' && word[0] <= '９'))
        //        {
        //        }
        //        else if (word[0] == '.' && dotCount == 0)
        //        {
        //            dotCount++;
        //        }
        //        else
        //        {
        //            break;
        //        }

        //        str.Append(word);
        //    }

        //    end = i;

        //    return str.ToString();
        //}

        /// <summary>
        /// 合并Email
        /// </summary>
        /// <param name="words"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        //private String MergeEmail(ArrayList words, int start, ref int end)
        //{
        //    StringBuilder str = new StringBuilder();

        //    int dotCount = 0;
        //    int atCount = 0;
        //    end = start;
        //    int i;

        //    for (i = start; i < words.Count; i++)
        //    {
        //        string word = (string)words[i];

        //        if (word == "")
        //        {
        //            break;
        //        }

        //        if ((word[0] >= 'a' && word[0] <= 'z') ||
        //            (word[0] >= 'A' && word[0] <= 'Z') ||
        //            word[0] >= '0' && word[0] <= '9')
        //        {
        //            dotCount = 0;
        //        }
        //        else if (word[0] == '@' && atCount == 0)
        //        {
        //            atCount++;
        //        }
        //        else if (word[0] == '.' && dotCount == 0)
        //        {
        //            dotCount++;
        //        }
        //        else
        //        {
        //            break;
        //        }

        //        str.Append(word);

        //    }

        //    end = i;

        //    return str.ToString();
        //}

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
                if (Setting.PanGuSettings.Config.MatchOptions.IgnoreSpace)
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
                    case WordType.SimpleChinese:

                        PanGu.Framework.AppendList<Dict.PositionLength> pls = _WordDictionary.GetAllMatchs(cur.Value.Word, _Options.ChineseNameIdentify);
                        PanGu.Match.ChsFullTextMatch chsMatch = new PanGu.Match.ChsFullTextMatch();
                        chsMatch.Options = _Options;
                        chsMatch.Parameters = _Parameters;
                        SuperLinkedList<WordInfo> chsMatchWords = chsMatch.Match(pls.Items, cur.Value.Word, pls.Count);

                        foreach (WordInfo wi in chsMatchWords)
                        {
                            wi.Position += cur.Value.Position;
                        }

                        SuperLinkedListNode<WordInfo> lst = chsMatchWords.Last;

                        result.AddAfter(cur, chsMatchWords);
                        SuperLinkedListNode<WordInfo> removeItem = cur;
                        cur = lst.Next;
                        result.Remove(removeItem);
                        break;
                    case WordType.English:
                        cur.Value.Rank = Setting.PanGuSettings.Config.Parameters.EnglishRank;
                        cur = cur.Next;
                        break;
                    case WordType.Numeric:
                        cur.Value.Rank = Setting.PanGuSettings.Config.Parameters.NumericRank;
                        cur = cur.Next;
                        break;
                    case WordType.Symbol:
                        cur.Value.Rank = Setting.PanGuSettings.Config.Parameters.SymbolRank;
                        cur = cur.Next;
                        break;
                    default:
                        cur = cur.Next;
                        break;
                }

            }


            return result;

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

            lock (_LockObj)
            {
                if (Setting.PanGuSettings.Config == null)
                {
                    Init();
                }
            }

            return PreSegment(text);
        }

        #endregion

        #region Initialization

        public static void Init()
        {
            Init(null);
        }

        public static void Init(string fileName)
        {
            if (Setting.PanGuSettings.Config != null)
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

            _WordDictionary = new PanGu.Dict.WordDictionary();
            _WordDictionary.Load(Setting.PanGuSettings.Config.GetDictionaryPath() + "Dict.Dct");

            _ChsName = new PanGu.Match.ChsName();
            _ChsName.LoadChsName(Setting.PanGuSettings.Config.GetDictionaryPath());

            _WordDictionary.ChineseName = _ChsName;

        }



        #endregion
    }
}
