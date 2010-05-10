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
using System.IO;
using System.Diagnostics;

namespace PanGu.Dict
{

    public class SearchWordResult : IComparable
    {
        /// <summary>
        /// 单词
        /// </summary>
        public WordAttribute Word;

        /// <summary>
        /// 相似度
        /// </summary>
        public float SimilarRatio;

        public override string ToString()
        {
            return Word.Word;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            SearchWordResult dest = (SearchWordResult)obj;

            if (this.SimilarRatio == dest.SimilarRatio)
            {
                return 0;
            }
            else if (this.SimilarRatio > dest.SimilarRatio)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        #endregion
    }

    [Serializable]
    public class WordDictionaryFile
    {
        public List<WordAttribute> Dicts = new List<WordAttribute>();
    }

    public struct PositionLength
    {
        public int Level ;
        public int Position;
        public int Length;
        public WordAttribute WordAttr;

        public PositionLength(int position, int length, WordAttribute wordAttr)
        {
            this.Position = position;
            this.Length = length;
            this.WordAttr = wordAttr;
            this.Level = 0;
        }
    }

    /// <summary>
    /// Dictionary for word
    /// </summary>
    public class WordDictionary
    {
        Dictionary<string, WordAttribute> _WordDict = new Dictionary<string, WordAttribute>();

        Dictionary<char, WordAttribute> _FirstCharDict = new Dictionary<char, WordAttribute>();
        Dictionary<uint, byte[]> _DoubleCharDict = new Dictionary<uint, byte[]>();

        internal Dict.ChsName ChineseName = null;

        public int Count
        {
            get
            {
                if (_WordDict == null)
                {
                    return 0;
                }
                else
                {
                    return _WordDict.Count;
                }
            }
        }

        #region Private Methods
        private WordDictionaryFile LoadFromBinFile(String fileName)
        {
            WordDictionaryFile dictFile = new WordDictionaryFile();
            dictFile.Dicts = new List<WordAttribute>();

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            byte[] version = new byte[32];
            fs.Read(version, 0, version.Length);
            String ver = Encoding.UTF8.GetString(version, 0, version.Length);

            String verNumStr = Framework.Regex.GetMatch(ver, "Pan Gu Segment V(.+)", true);

            while (fs.Position < fs.Length)
            {
                byte[] buf = new byte[sizeof(int)];
                fs.Read(buf, 0, buf.Length);
                int length = BitConverter.ToInt32(buf, 0);

                buf = new byte[length];

                fs.Read(buf, 0, buf.Length);

                string word = Encoding.UTF8.GetString(buf, 0, length - sizeof(int) - sizeof(double));
                POS pos = (POS)BitConverter.ToInt32(buf, length - sizeof(int) - sizeof(double));
                double frequency = BitConverter.ToDouble(buf, length - sizeof(double));

                WordAttribute dict = new WordAttribute(word, pos, frequency);
                string.Intern(dict.Word);

                dictFile.Dicts.Add(dict);
            }

            fs.Close();

            return dictFile;
        }

        private void SaveToBinFile(String fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                byte[] version = new byte[32];

                int i = 0;
                foreach (byte v in System.Text.Encoding.UTF8.GetBytes("Pan Gu Segment V1.0"))
                {
                    version[i] = v;
                    i++;
                }

                fs.Write(version, 0, version.Length);

                foreach (WordAttribute wa in _WordDict.Values)
                {
                    byte[] word = System.Text.Encoding.UTF8.GetBytes(wa.Word);
                    byte[] pos = System.BitConverter.GetBytes((int)wa.Pos);
                    byte[] frequency = System.BitConverter.GetBytes(wa.Frequency);
                    byte[] length = System.BitConverter.GetBytes(word.Length + frequency.Length + pos.Length);

                    fs.Write(length, 0, length.Length);
                    fs.Write(word, 0, word.Length);
                    fs.Write(pos, 0, pos.Length);
                    fs.Write(frequency, 0, frequency.Length);
                }
            }
        }


        #endregion

        #region Public Methods

        public WordAttribute GetWordAttr(string word)
        {
            WordAttribute wa;

            if (_WordDict.TryGetValue(word.ToLower(), out wa))
            {
                return wa;
            }
            else
            {
                return null;
            }
        }

        public Framework.AppendList<PositionLength> GetAllMatchs(string text, bool chineseNameIdentify)
        {
            Framework.AppendList<PositionLength> result = new PanGu.Framework.AppendList<PositionLength>();

            if (text == null && text == "")
            {
                return result;
            }

            string keyText = text;

            if (text[0] < 128)
            {
                keyText = keyText.ToLower();
            }

            for (int i = 0; i < text.Length; i++)
            {

                byte[] lenList;
                char fst = keyText[i];

                List<string> chsNames = null;

                if (chineseNameIdentify)
                {
                    chsNames = ChineseName.Match(text, i);

                    if (chsNames != null)
                    {
                        foreach (string name in chsNames)
                        {
                            WordAttribute wa = new WordAttribute(name, POS.POS_A_NR, 0);

                            result.Add(new PositionLength(i, name.Length, wa));
                        }
                    }
                }


                WordAttribute fwa;
                if (_FirstCharDict.TryGetValue(fst, out fwa))
                {
                    result.Add(new PositionLength(i, 1, fwa));
                }

                if (i >= keyText.Length - 1)
                {
                    continue;
                }

                uint doubleChar = (uint)(keyText[i] * 65536) + keyText[i+1];

                if (_DoubleCharDict.TryGetValue(doubleChar, out lenList))
                {
                    foreach (byte len in lenList)
                    {
                        if (len == 0)
                        {
                            break;
                        }

                        if (i + len > keyText.Length)
                        {
                            continue;
                        }

                        string key = keyText.Substring(i, len);

                        WordAttribute wa;

                        if (_WordDict.TryGetValue(key, out wa))
                        {
                            if (chsNames != null)
                            {
                                bool find = false;

                                foreach (string name in chsNames)
                                {
                                    if (wa.Word == name)
                                    {
                                        find = true;
                                        break;
                                    }
                                }

                                if (find)
                                {
                                    continue;
                                }
                            }

                            result.Add(new PositionLength(i, len, wa));
                        }
                    }
                }
            }

            return result;
        }


        public void Load(String fileName)
        {
            _WordDict = new Dictionary<string, WordAttribute>();
            _FirstCharDict = new Dictionary<char, WordAttribute>();
            _DoubleCharDict = new Dictionary<uint, byte[]>();

            foreach (WordAttribute wa in LoadFromBinFile(fileName).Dicts)
            {
                string key = wa.Word.ToLower();

                if (!_WordDict.ContainsKey(key))
                {
                    _WordDict.Add(key, wa);

                    if (key.Length == 1)
                    {
                        _FirstCharDict.Add(key[0], wa);
                        continue;
                    }

                    uint doubleChar = ((uint)key[0] * 65536) + key[1];

                    byte[] wordLenArray;
                    if (!_DoubleCharDict.TryGetValue(doubleChar, out wordLenArray))
                    {
                        wordLenArray = new byte[4];
                        wordLenArray[0] = (byte)key.Length;

                        _DoubleCharDict.Add(doubleChar, wordLenArray);
                    }
                    else
                    {
                        bool find = false;
                        int i;
                        for(i = 0 ; i < wordLenArray.Length; i++)
                        {
                            byte len = wordLenArray[i];
                            if (len == key.Length)
                            {
                                find = true;
                                break;
                            }

                            if (len == 0)
                            {
                                wordLenArray[i] = (byte)key.Length;
                                find = true;
                                break;
                            }
                        }

                        if (!find)
                        {
                            byte[] temp = new byte[wordLenArray.Length * 2];

                            wordLenArray.CopyTo(temp, 0);
                            wordLenArray = temp;
                            wordLenArray[i] = (byte)key.Length;

                            _DoubleCharDict[doubleChar] = wordLenArray;
                        }
                    }

                }
            }


        }

        public void Save(string fileName)
        {
            SaveToBinFile(fileName);
        }

        public void InsertWord(String word, double frequency, POS pos)
        {
            if (_WordDict == null)
            {
                return;
            }

            string key = word.ToLower();
            if (_WordDict.ContainsKey(key))
            {
                return;
            }

            WordAttribute wa = new WordAttribute(word, pos, frequency);

            _WordDict.Add(key, wa);

            if (key.Length == 1)
            {
                _FirstCharDict.Add(key[0], wa);
                return;
            }

            uint doubleChar = (uint)(key[0] * 65536) + key[1];

            byte[] wordLenArray;
            if (!_DoubleCharDict.TryGetValue(doubleChar, out wordLenArray))
            {
                wordLenArray = new byte[4];
                wordLenArray[0] = (byte)key.Length;

                _DoubleCharDict.Add(doubleChar, wordLenArray);
            }
            else
            {
                bool find = false;
                int i;
                for (i = 0; i < wordLenArray.Length; i++)
                {
                    byte len = wordLenArray[i];
                    if (len == key.Length)
                    {
                        find = true;
                        break;
                    }

                    if (len == 0)
                    {
                        wordLenArray[i] = (byte)key.Length;
                        find = true;
                        break;
                    }
                }

                if (!find)
                {
                    byte[] temp = new byte[wordLenArray.Length * 2];

                    wordLenArray.CopyTo(temp, 0);
                    wordLenArray = temp;
                    wordLenArray[i] = (byte)key.Length;

                    _DoubleCharDict[doubleChar] = wordLenArray;
                }
            }


        }

        public void UpdateWord(String word, double frequency, POS pos)
        {
            if (_WordDict == null)
            {
                return;
            }

            string key = word.ToLower();
            if (!_WordDict.ContainsKey(key))
            {
                return;
            }

            _WordDict[key].Word = word;
            _WordDict[key].Frequency = frequency;
            _WordDict[key].Pos = pos;
        }

        public void DeleteWord(String word)
        {
            if (_WordDict == null)
            {
                return;
            }

            string key = word.ToLower();
            if (_WordDict.ContainsKey(key))
            {
                _WordDict.Remove(key);
            }
        }

        /// <summary>
        /// 通过遍历方式搜索
        /// </summary>
        /// <returns></returns>
        public List<SearchWordResult> Search(String key)
        {
            Debug.Assert(_WordDict != null);

            List<SearchWordResult> result = new List<SearchWordResult>();

            foreach (WordAttribute wa in _WordDict.Values)
            {
                if (wa.Word.Contains(key))
                {
                    SearchWordResult wordResult = new SearchWordResult();
                    wordResult.Word = wa;
                    wordResult.SimilarRatio = (float)key.Length / (float)wa.Word.Length;
                    result.Add(wordResult);
                }
            }

            return result;
        }

        public List<SearchWordResult> SearchByLength(int len)
        {
            Debug.Assert(_WordDict != null);

            List<SearchWordResult> result = new List<SearchWordResult>();

            foreach (WordAttribute wa in _WordDict.Values)
            {
                if (wa.Word.Length == len)
                {
                    SearchWordResult wordResult = new SearchWordResult();
                    wordResult.Word = wa;
                    wordResult.SimilarRatio = 0;
                    result.Add(wordResult);
                }
            }

            return result;
        }

        public List<SearchWordResult> SearchByPos(POS Pos)
        {
            Debug.Assert(_WordDict != null);

            List<SearchWordResult> result = new List<SearchWordResult>();

            foreach (WordAttribute wa in _WordDict.Values)
            {
                if ((wa.Pos & Pos) != 0)
                {
                    SearchWordResult wordResult = new SearchWordResult();
                    wordResult.Word = wa;
                    wordResult.SimilarRatio = 0;
                    result.Add(wordResult);
                }
            }

            return result;
        }
        #endregion
    }
}
