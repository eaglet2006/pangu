using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PanGu.Dict
{
    class StopWord
    {
        Dictionary<string, string> _StopwordTbl = new Dictionary<string, string>();

        public bool IsStopWord(string word)
        {
            if (word == null || word == "")
            {
                return false;
            }

            string key;

            if (word[0] < 128)
            {
                key = word.ToLower();
            }
            else
            {
                key = word;
            }

            return _StopwordTbl.ContainsKey(key);
        }

        public void LoadStopwordsDict(String chsFileName)
        {
            using (StreamReader sw = new StreamReader(chsFileName, Encoding.GetEncoding("UTF-8")))
            {
                //加载中文停用词
                while (!sw.EndOfStream)
                {
                    //按行读取中文停用词
                    string stopWord = sw.ReadLine();

                    if (string.IsNullOrEmpty(stopWord))
                    {
                        continue;
                    }

                    string key;

                    if (stopWord[0] < 128)
                    {
                        key = stopWord.ToLower();
                    }
                    else
                    {
                        key = stopWord;
                    }

                    //如果哈希表中不包括该停用词则添加到哈希表中
                    if (!_StopwordTbl.ContainsKey(key))
                    {
                        _StopwordTbl.Add(key, stopWord);
                    }
                }

            }
        }
    }
}
