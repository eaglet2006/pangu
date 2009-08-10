using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PanGu.Dict
{

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

        Dictionary<char, byte[]> _FirstCharDict = new Dictionary<char,byte[]>();

        internal Match.ChsName ChineseName = null;

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

                if (_FirstCharDict.TryGetValue(fst, out lenList))
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
            _FirstCharDict = new Dictionary<char, byte[]>();

            foreach (WordAttribute wordInfo in LoadFromBinFile(fileName).Dicts)
            {
                string key = wordInfo.Word.ToLower();
                if (!_WordDict.ContainsKey(key))
                {
                    _WordDict.Add(key, wordInfo);

                    byte[] wordLenArray;
                    if (!_FirstCharDict.TryGetValue(key[0], out wordLenArray))
                    {
                        wordLenArray = new byte[4];
                        wordLenArray[0] = (byte)key.Length;

                        _FirstCharDict.Add(key[0], wordLenArray);
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

                            _FirstCharDict[key[0]] = wordLenArray;
                        }
                    }

                }
            }


        }

        static public WordDictionaryFile LoadFromBinFile(String fileName)
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

    }
}
