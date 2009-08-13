using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using System.Threading;

namespace PanGu.Lucene.ImportTool
{
    public partial class FormImport : Form
    {
        public FormImport()
        {
            InitializeComponent();
        }

        private void buttonCreateIndex_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(openFileDialog.FileName);
                    XmlNodeList nodes = xmlDoc.SelectNodes(@"News/Item");

                    Stopwatch watch = new Stopwatch();

                    DateTime old = DateTime.Now;
                    int count = 0;
                    int MaxCount = Math.Min(40000, nodes.Count);

                    long totalChars = 0;
                    Index.CreateIndex(Index.INDEX_DIR);
                    Index.MaxMergeFactor = 301;
                    Index.MinMergeDocs = 301;

                    progressBar.Value = 0;
                    Application.DoEvents();

                    foreach (XmlNode node in nodes)
                    {
                        String title = node.Attributes["Title"].Value;
                        DateTime time = DateTime.Parse(node.Attributes["Time"].Value);
                        String Url = node.Attributes["Url"].Value;
                        String content = node.Attributes["Content"].Value;

                        totalChars += title.Length + 8 + Url.Length + content.Length;


                        watch.Start();

                        Index.IndexString(Index.INDEX_DIR, Url, title,
                            time, content);

                        watch.Stop();

                        count++;
                        progressBar.Value = count * 100 / MaxCount;
                        labelProgress.Text = progressBar.Value + "%";
                        Application.DoEvents();

                        if (count >= MaxCount)
                        {
                            break;
                        }

                        if (count % 300 == 0)
                        {
                            Index.CloseWithoutOptimize();
                            Index.CreateIndex(Index.INDEX_DIR);
                            Index.MaxMergeFactor = 301;
                            Index.MinMergeDocs = 301;
                        }
                    }

                    watch.Start();

                    Index.Close();

                    watch.Stop();

                    TimeSpan s = DateTime.Now - old;
                    MessageBox.Show(String.Format("插入{0}行数据,共{1}字符,用时{2}秒",
                        MaxCount, totalChars, watch.ElapsedMilliseconds / 1000 + "." + watch.ElapsedMilliseconds % 1000),
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        List<string> _ContentList;

        long _TotalChars = 0;
        int _SegmentCount = 0;
        int _FinishCount = 0;
        void TestSpeed(object firstIndex)
        {
            for (int i = (int)firstIndex; i < _ContentList.Count; i += 2)
            {
                Segment segment = new Segment();
                segment.DoSegment(_ContentList[i]);

                lock (this)
                {
                    _TotalChars += _ContentList[i].Length;
                    _SegmentCount++;
                }
            }

            _FinishCount++;
        }

        private void buttonTestSpeed_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(openFileDialog.FileName);
                    XmlNodeList nodes = xmlDoc.SelectNodes(@"News/Item");

                    Stopwatch watch = new Stopwatch();

                    int count = 0;

                    _TotalChars = 0;
                    progressBar.Value = 0;
                    Application.DoEvents();
                    Segment.Init();

                     _ContentList = new List<string>();

                    foreach (XmlNode node in nodes)
                    {
                        string content = node.Attributes["Content"].Value;
                        _ContentList.Add(content);
                    }

                    int threadNum = checkBoxDoubleThread.Checked ? 2 : 1;

                    for (int i = 0; i < threadNum; i++ )
                    {
                        ThreadPool.QueueUserWorkItem(TestSpeed, i);
                    }

                    watch.Reset();
                    watch.Start();
                    while (_FinishCount < threadNum)
                    {
                        lock (this)
                        {
                            count = _SegmentCount;
                        }

                        int progress = count * 2 * 100 / (threadNum * nodes.Count);
                        if (progress > 100)
                        {
                            progress = 100;
                        }

                        progressBar.Value = progress;
                        labelProgress.Text = progressBar.Value + "%";
                        Application.DoEvents();
                        Thread.Sleep(10);
                    }
                    watch.Stop();

                    MessageBox.Show(String.Format("插入{0}行数据,共{1}字符,用时{2}秒 {3}字符每秒",
                        count, _TotalChars, 
                        watch.ElapsedMilliseconds / 1000 + "." + watch.ElapsedMilliseconds % 1000,
                        _TotalChars / watch.Elapsed.TotalSeconds),
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
