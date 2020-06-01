namespace UXM
{
    partial class FormMain
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label lblBreak;
            System.Windows.Forms.Label lblExePath;
            System.Windows.Forms.Label lblStatus;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.btnPatch = new System.Windows.Forms.Button();
            this.btnUnpack = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnExplore = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtExePath = new System.Windows.Forms.TextBox();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.pbrProgress = new System.Windows.Forms.ProgressBar();
            this.ofdExe = new System.Windows.Forms.OpenFileDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            lblBreak = new System.Windows.Forms.Label();
            lblExePath = new System.Windows.Forms.Label();
            lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblBreak
            // 
            lblBreak.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            lblBreak.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            lblBreak.Location = new System.Drawing.Point(-26, 156);
            lblBreak.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            lblBreak.Name = "lblBreak";
            lblBreak.Size = new System.Drawing.Size(1330, 4);
            lblBreak.TabIndex = 31;
            // 
            // lblExePath
            // 
            lblExePath.AutoSize = true;
            lblExePath.Location = new System.Drawing.Point(24, 16);
            lblExePath.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            lblExePath.Name = "lblExePath";
            lblExePath.Size = new System.Drawing.Size(250, 24);
            lblExePath.TabIndex = 30;
            lblExePath.Text = "游戏可执行文件路径名";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new System.Drawing.Point(24, 172);
            lblStatus.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(58, 24);
            lblStatus.TabIndex = 32;
            lblStatus.Text = "状态";
            // 
            // btnPatch
            // 
            this.btnPatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPatch.Location = new System.Drawing.Point(784, 96);
            this.btnPatch.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnPatch.Name = "btnPatch";
            this.btnPatch.Size = new System.Drawing.Size(150, 42);
            this.btnPatch.TabIndex = 27;
            this.btnPatch.Text = "修补";
            this.btnPatch.UseVisualStyleBackColor = true;
            this.btnPatch.Click += new System.EventHandler(this.btnPatch_Click);
            // 
            // btnUnpack
            // 
            this.btnUnpack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUnpack.Location = new System.Drawing.Point(622, 96);
            this.btnUnpack.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnUnpack.Name = "btnUnpack";
            this.btnUnpack.Size = new System.Drawing.Size(150, 42);
            this.btnUnpack.TabIndex = 26;
            this.btnUnpack.Text = "解包";
            this.btnUnpack.UseVisualStyleBackColor = true;
            this.btnUnpack.Click += new System.EventHandler(this.btnUnpack_Click);
            // 
            // btnRestore
            // 
            this.btnRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRestore.Location = new System.Drawing.Point(946, 96);
            this.btnRestore.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(150, 42);
            this.btnRestore.TabIndex = 28;
            this.btnRestore.Text = "还原";
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbort.Enabled = false;
            this.btnAbort.Location = new System.Drawing.Point(1108, 96);
            this.btnAbort.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(150, 42);
            this.btnAbort.TabIndex = 29;
            this.btnAbort.Text = "终止";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // btnExplore
            // 
            this.btnExplore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExplore.Location = new System.Drawing.Point(1108, 42);
            this.btnExplore.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnExplore.Name = "btnExplore";
            this.btnExplore.Size = new System.Drawing.Size(150, 42);
            this.btnExplore.TabIndex = 25;
            this.btnExplore.Text = "打开目录";
            this.btnExplore.UseVisualStyleBackColor = true;
            this.btnExplore.Click += new System.EventHandler(this.btnExplore_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(946, 42);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(150, 42);
            this.btnBrowse.TabIndex = 24;
            this.btnBrowse.Text = "浏览";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtExePath
            // 
            this.txtExePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExePath.Location = new System.Drawing.Point(24, 46);
            this.txtExePath.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.txtExePath.Name = "txtExePath";
            this.txtExePath.Size = new System.Drawing.Size(906, 35);
            this.txtExePath.TabIndex = 23;
            this.txtExePath.Text = "G:\\games\\Dark Souls III\\Game\\DarkSoulsIII.exe";
            // 
            // txtStatus
            // 
            this.txtStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStatus.Location = new System.Drawing.Point(24, 202);
            this.txtStatus.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.Size = new System.Drawing.Size(1230, 35);
            this.txtStatus.TabIndex = 33;
            // 
            // pbrProgress
            // 
            this.pbrProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbrProgress.Location = new System.Drawing.Point(24, 250);
            this.pbrProgress.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.pbrProgress.Maximum = 1000;
            this.pbrProgress.Name = "pbrProgress";
            this.pbrProgress.Size = new System.Drawing.Size(1234, 42);
            this.pbrProgress.TabIndex = 34;
            // 
            // ofdExe
            // 
            this.ofdExe.FileName = "DarkSoulsIII.exe";
            this.ofdExe.Filter = "Dark Souls Executable|*.exe";
            this.ofdExe.Title = "Select Dark Souls executable...";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1282, 296);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(lblStatus);
            this.Controls.Add(this.pbrProgress);
            this.Controls.Add(lblBreak);
            this.Controls.Add(this.btnPatch);
            this.Controls.Add(this.btnUnpack);
            this.Controls.Add(this.btnRestore);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.btnExplore);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtExePath);
            this.Controls.Add(lblExePath);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximumSize = new System.Drawing.Size(3974, 367);
            this.MinimumSize = new System.Drawing.Size(688, 367);
            this.Name = "FormMain";
            this.Text = "DSUnpacker <version>";
            this.Activated += new System.EventHandler(this.FormMain_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPatch;
        private System.Windows.Forms.Button btnUnpack;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button btnExplore;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtExePath;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.ProgressBar pbrProgress;
        private System.Windows.Forms.OpenFileDialog ofdExe;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

