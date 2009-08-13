namespace PanGu.Lucene.ImportTool
{
    partial class FormImport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImport));
            this.labelProgress = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.buttonCreateIndex = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonTestSpeed = new System.Windows.Forms.Button();
            this.checkBoxDoubleThread = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(421, 27);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(21, 13);
            this.labelProgress.TabIndex = 3;
            this.labelProgress.Text = "0%";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(24, 21);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(391, 26);
            this.progressBar.TabIndex = 2;
            // 
            // buttonCreateIndex
            // 
            this.buttonCreateIndex.Location = new System.Drawing.Point(24, 95);
            this.buttonCreateIndex.Name = "buttonCreateIndex";
            this.buttonCreateIndex.Size = new System.Drawing.Size(75, 23);
            this.buttonCreateIndex.TabIndex = 4;
            this.buttonCreateIndex.Text = "创建索引";
            this.buttonCreateIndex.UseVisualStyleBackColor = true;
            this.buttonCreateIndex.Click += new System.EventHandler(this.buttonCreateIndex_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "xml";
            this.openFileDialog.FileName = "news.xml";
            this.openFileDialog.Filter = "xml|*.xml";
            this.openFileDialog.RestoreDirectory = true;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(24, 69);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(418, 13);
            this.linkLabel1.TabIndex = 5;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://pangusegment.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=31482";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "News.xml 下载地址：";
            // 
            // buttonTestSpeed
            // 
            this.buttonTestSpeed.Location = new System.Drawing.Point(119, 95);
            this.buttonTestSpeed.Name = "buttonTestSpeed";
            this.buttonTestSpeed.Size = new System.Drawing.Size(93, 23);
            this.buttonTestSpeed.TabIndex = 7;
            this.buttonTestSpeed.Text = "测试分词速度";
            this.buttonTestSpeed.UseVisualStyleBackColor = true;
            this.buttonTestSpeed.Click += new System.EventHandler(this.buttonTestSpeed_Click);
            // 
            // checkBoxDoubleThread
            // 
            this.checkBoxDoubleThread.AutoSize = true;
            this.checkBoxDoubleThread.Location = new System.Drawing.Point(229, 99);
            this.checkBoxDoubleThread.Name = "checkBoxDoubleThread";
            this.checkBoxDoubleThread.Size = new System.Drawing.Size(134, 17);
            this.checkBoxDoubleThread.TabIndex = 8;
            this.checkBoxDoubleThread.Text = "双线程同时分词测试";
            this.checkBoxDoubleThread.UseVisualStyleBackColor = true;
            // 
            // FormImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 130);
            this.Controls.Add(this.checkBoxDoubleThread);
            this.Controls.Add(this.buttonTestSpeed);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.buttonCreateIndex);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.progressBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormImport";
            this.Text = "Import tool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button buttonCreateIndex;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonTestSpeed;
        private System.Windows.Forms.CheckBox checkBoxDoubleThread;
    }
}

