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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using PanGu.Dict;

namespace Demo
{
    public partial class FormDemo : Form
    {
        String _InitSource = "盘古 简介: 盘古 是由eaglet 开发的一款基于字典的中英文分词组件\r\n" +
            "主要功能: 中英文分词，未登录词识别,多元歧义自动识别,全角字符识别能力\r\n" +
            "主要性能指标:\r\n" +
            "分词准确度:90%以上(有待专家的权威评测)\r\n" +
            "处理速度: 600KBytes/s\r\n" +
            "用于测试的句子:\r\n" +
            "长春市长春节致词\r\n" +
            "长春市长春药店\r\n" +
            "IＢM的技术和服务都不错\r\n" +
            "张三在一月份工作会议上说的确实在理\r\n" +
            "于北京时间5月10日举行运动会\r\n" +
            "我的和服务必在明天做好";

        WordDictionary _WordDict = new WordDictionary();

        public FormDemo()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string dir = textBox1.Text;
            string currentDir = System.IO.Directory.GetCurrentDirectory();
            System.IO.Directory.SetCurrentDirectory(dir);
            dir = System.IO.Directory.GetCurrentDirectory();
            System.IO.Directory.SetCurrentDirectory(currentDir);

            string dictFile = PanGu.Framework.Path.AppendDivision(dir, '\\') + "Dict.Dct";
            _WordDict.Load(dictFile);
            button1.Enabled = false;
        }

        private void FormDemo_Load(object sender, EventArgs e)
        {
            textBox2.Text = _InitSource;
            textBox2.Text = "长春市长春节致辞";

            string str = "中文化軟體聯盟－ 軟體分類－ 分類瀏覽";

            //str = Microsoft.VisualBasic.Strings.StrConv(str, Microsoft.VisualBasic.VbStrConv.SimplifiedChinese, 0);


            foreach (char c in str)
            {
                int i = (int)c;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PanGu.Framework.AppendList<PositionLength> pls = _WordDict.GetAllMatchs(textBox2.Text);
            PanGu.Match.ChsFullTextMatch chsMatch = new PanGu.Match.ChsFullTextMatch();
            chsMatch.Match(pls.Items, textBox2.Text.Length, pls.Count);
        }
    }
}
