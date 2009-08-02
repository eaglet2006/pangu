using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PanGu.Dict
{

    [Serializable]
    public class WordDictionaryFile
    {
        public List<WordInfo> Dicts = new List<WordInfo>();
    }

    /// <summary>
    /// Dictionary for word
    /// </summary>
    public class WordDictionary
    {
        Dictionary<string, WordInfo> _WordDict = new Dictionary<string, WordInfo>();

        Dictionary<char, byte[]> _FirstCharDict = new Dictionary<char,byte[]>();

        public void Load(String fileName)
        {
            _WordDict = new Dictionary<string, WordInfo>();
            _FirstCharDict = new Dictionary<char, byte[]>();

            foreach (WordInfo wordInfo in LoadFromBinFile(fileName).Dicts)
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
                        }
                    }

                }
            }


        }

        static public WordDictionaryFile LoadFromBinFile(String fileName)
        {
            WordDictionaryFile dictFile = new WordDictionaryFile();
            dictFile.Dicts = new List<WordInfo>();

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

                WordInfo dict = new WordInfo();

                fs.Read(buf, 0, buf.Length);

                dict.Word = Encoding.UTF8.GetString(buf, 0, length - sizeof(int) - sizeof(double));
                string.Intern(dict.Word);

                dict.Pos = BitConverter.ToInt32(buf, length - sizeof(int) - sizeof(double));
                dict.Frequency = BitConverter.ToDouble(buf, length - sizeof(double));
                dictFile.Dicts.Add(dict);
            }

            fs.Close();

            return dictFile;
        }

    }
}
