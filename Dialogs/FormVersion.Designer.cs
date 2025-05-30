namespace ResourceUpdatePack {
    partial class FormVersion {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.ckShowTops = new System.Windows.Forms.CheckBox();
			this.ckShowVersion = new System.Windows.Forms.CheckBox();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.btnCommit = new System.Windows.Forms.Button();
			this.ctxMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuRemove = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuNormal = new System.Windows.Forms.ToolStripMenuItem();
			this.menuImportant = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStart = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuHistory = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.menuAddAll = new System.Windows.Forms.ToolStripMenuItem();
			this.menuRemoveAll = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.menuEarlydownload = new System.Windows.Forms.ToolStripMenuItem();
			this.menuEarlydownloadBugfix = new System.Windows.Forms.ToolStripMenuItem();
			this.menuEarlydownloadHistory = new System.Windows.Forms.ToolStripMenuItem();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.logListView = new ResourceUpdatePack.CustomListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.splitContainer3 = new System.Windows.Forms.SplitContainer();
			this.fileListView = new ResourceUpdatePack.CustomListView();
			this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ctxFile = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuAddFile = new System.Windows.Forms.ToolStripMenuItem();
			this.menuRemoveFile = new System.Windows.Forms.ToolStripMenuItem();
			this.ignoreTreeView = new System.Windows.Forms.TreeView();
			this.ctxIgnore = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuExpand = new System.Windows.Forms.ToolStripMenuItem();
			this.menuCollapse = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.menuRemoveIgnore = new System.Windows.Forms.ToolStripMenuItem();
			this.menuClearIgnore = new System.Windows.Forms.ToolStripMenuItem();
			this.menuReloadIgnore = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.menuFileView = new System.Windows.Forms.ToolStripMenuItem();
			this.menuRevisionView = new System.Windows.Forms.ToolStripMenuItem();
			this.ckCommitHistory = new System.Windows.Forms.CheckBox();
			this.ckShowOnlyAfterInit = new System.Windows.Forms.CheckBox();
			this.ckDelete = new System.Windows.Forms.CheckBox();
			this.btnPublish = new System.Windows.Forms.Button();
			this.ckForce = new System.Windows.Forms.CheckBox();
			this.saveVersionDlg = new System.Windows.Forms.SaveFileDialog();
			this.outputDlg = new System.Windows.Forms.FolderBrowserDialog();
			this.ctxMenuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
			this.splitContainer3.Panel1.SuspendLayout();
			this.splitContainer3.Panel2.SuspendLayout();
			this.splitContainer3.SuspendLayout();
			this.ctxFile.SuspendLayout();
			this.ctxIgnore.SuspendLayout();
			this.SuspendLayout();
			// 
			// ckShowTops
			// 
			this.ckShowTops.AutoSize = true;
			this.ckShowTops.Checked = true;
			this.ckShowTops.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ckShowTops.Location = new System.Drawing.Point(126, 14);
			this.ckShowTops.Name = "ckShowTops";
			this.ckShowTops.Size = new System.Drawing.Size(108, 16);
			this.ckShowTops.TabIndex = 1;
			this.ckShowTops.Text = "只显示近期节点";
			this.ckShowTops.UseVisualStyleBackColor = true;
			this.ckShowTops.CheckedChanged += new System.EventHandler(this.ckShowTops_CheckedChanged);
			// 
			// ckShowVersion
			// 
			this.ckShowVersion.AutoSize = true;
			this.ckShowVersion.Location = new System.Drawing.Point(12, 14);
			this.ckShowVersion.Name = "ckShowVersion";
			this.ckShowVersion.Size = new System.Drawing.Size(108, 16);
			this.ckShowVersion.TabIndex = 2;
			this.ckShowVersion.Text = "只显示版本节点";
			this.ckShowVersion.UseVisualStyleBackColor = true;
			this.ckShowVersion.CheckedChanged += new System.EventHandler(this.ckShowVersion_CheckedChanged);
			// 
			// btnRefresh
			// 
			this.btnRefresh.Location = new System.Drawing.Point(390, 6);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(80, 30);
			this.btnRefresh.TabIndex = 3;
			this.btnRefresh.Text = "刷新";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// btnCommit
			// 
			this.btnCommit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCommit.Location = new System.Drawing.Point(913, 6);
			this.btnCommit.Name = "btnCommit";
			this.btnCommit.Size = new System.Drawing.Size(80, 30);
			this.btnCommit.TabIndex = 3;
			this.btnCommit.Text = "保存并提交";
			this.btnCommit.UseVisualStyleBackColor = true;
			this.btnCommit.Click += new System.EventHandler(this.btnCommit_Click);
			// 
			// ctxMenuStrip
			// 
			this.ctxMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuRemove,
            this.toolStripSeparator1,
            this.menuNormal,
            this.menuImportant,
            this.menuStart,
            this.toolStripMenuItem1,
            this.menuHistory,
            this.toolStripSeparator4,
            this.menuAddAll,
            this.menuRemoveAll,
            this.toolStripSeparator5,
            this.menuEarlydownload,
            this.menuEarlydownloadBugfix,
            this.menuEarlydownloadHistory});
			this.ctxMenuStrip.Name = "contextMenuStrip1";
			this.ctxMenuStrip.Size = new System.Drawing.Size(231, 248);
			this.ctxMenuStrip.Opened += new System.EventHandler(this.ctxMenuStrip_Opened);
			// 
			// menuRemove
			// 
			this.menuRemove.ForeColor = System.Drawing.Color.Gray;
			this.menuRemove.Name = "menuRemove";
			this.menuRemove.Size = new System.Drawing.Size(230, 22);
			this.menuRemove.Text = "取消版本";
			this.menuRemove.Click += new System.EventHandler(this.menuRemove_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(227, 6);
			// 
			// menuNormal
			// 
			this.menuNormal.ForeColor = System.Drawing.Color.Blue;
			this.menuNormal.Name = "menuNormal";
			this.menuNormal.Size = new System.Drawing.Size(230, 22);
			this.menuNormal.Text = "设为普通版本";
			this.menuNormal.Click += new System.EventHandler(this.menuNormal_Click);
			// 
			// menuImportant
			// 
			this.menuImportant.ForeColor = System.Drawing.Color.Purple;
			this.menuImportant.Name = "menuImportant";
			this.menuImportant.Size = new System.Drawing.Size(230, 22);
			this.menuImportant.Text = "设为关键版本";
			this.menuImportant.Click += new System.EventHandler(this.menuImportant_Click);
			// 
			// menuStart
			// 
			this.menuStart.ForeColor = System.Drawing.Color.Green;
			this.menuStart.Name = "menuStart";
			this.menuStart.Size = new System.Drawing.Size(230, 22);
			this.menuStart.Text = "设为起始版本";
			this.menuStart.Click += new System.EventHandler(this.menuStart_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(227, 6);
			// 
			// menuHistory
			// 
			this.menuHistory.Name = "menuHistory";
			this.menuHistory.Size = new System.Drawing.Size(230, 22);
			this.menuHistory.Text = "历史版本";
			this.menuHistory.Click += new System.EventHandler(this.menuHistory_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(227, 6);
			// 
			// menuAddAll
			// 
			this.menuAddAll.Name = "menuAddAll";
			this.menuAddAll.Size = new System.Drawing.Size(230, 22);
			this.menuAddAll.Text = "加入忽略列表";
			this.menuAddAll.Click += new System.EventHandler(this.menuAddAll_Click);
			// 
			// menuRemoveAll
			// 
			this.menuRemoveAll.Name = "menuRemoveAll";
			this.menuRemoveAll.Size = new System.Drawing.Size(230, 22);
			this.menuRemoveAll.Text = "移出忽略列表";
			this.menuRemoveAll.Click += new System.EventHandler(this.menuRemoveAll_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(227, 6);
			// 
			// menuEarlydownload
			// 
			this.menuEarlydownload.ForeColor = System.Drawing.Color.DeepSkyBlue;
			this.menuEarlydownload.Name = "menuEarlydownload";
			this.menuEarlydownload.Size = new System.Drawing.Size(230, 22);
			this.menuEarlydownload.Text = "预下载起始版本";
			this.menuEarlydownload.Click += new System.EventHandler(this.menuEarlydownload_Click);
			// 
			// menuEarlydownloadBugfix
			// 
			this.menuEarlydownloadBugfix.ForeColor = System.Drawing.Color.OrangeRed;
			this.menuEarlydownloadBugfix.Name = "menuEarlydownloadBugfix";
			this.menuEarlydownloadBugfix.Size = new System.Drawing.Size(230, 22);
			this.menuEarlydownloadBugfix.Text = "紧急更新起始版本";
			this.menuEarlydownloadBugfix.Click += new System.EventHandler(this.menuEarlydownloadBugfix_Click);
			// 
			// menuEarlydownloadHistory
			// 
			this.menuEarlydownloadHistory.ForeColor = System.Drawing.Color.DarkOrange;
			this.menuEarlydownloadHistory.Name = "menuEarlydownloadHistory";
			this.menuEarlydownloadHistory.Size = new System.Drawing.Size(230, 22);
			this.menuEarlydownloadHistory.Text = "历史版本(预下载/紧急更新)";
			this.menuEarlydownloadHistory.Click += new System.EventHandler(this.menuEarlydownloadHistory_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.IsSplitterFixed = true;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ckCommitHistory);
			this.splitContainer1.Panel2.Controls.Add(this.ckShowOnlyAfterInit);
			this.splitContainer1.Panel2.Controls.Add(this.ckDelete);
			this.splitContainer1.Panel2.Controls.Add(this.btnCommit);
			this.splitContainer1.Panel2.Controls.Add(this.btnPublish);
			this.splitContainer1.Panel2.Controls.Add(this.btnRefresh);
			this.splitContainer1.Panel2.Controls.Add(this.ckForce);
			this.splitContainer1.Panel2.Controls.Add(this.ckShowVersion);
			this.splitContainer1.Panel2.Controls.Add(this.ckShowTops);
			this.splitContainer1.Size = new System.Drawing.Size(1008, 792);
			this.splitContainer1.SplitterDistance = 742;
			this.splitContainer1.TabIndex = 5;
			// 
			// splitContainer2
			// 
			this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.logListView);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
			this.splitContainer2.Size = new System.Drawing.Size(1008, 742);
			this.splitContainer2.SplitterDistance = 507;
			this.splitContainer2.TabIndex = 1;
			// 
			// logListView
			// 
			this.logListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.logListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
			this.logListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logListView.FullRowSelect = true;
			this.logListView.GridLines = true;
			this.logListView.HideSelection = false;
			this.logListView.Location = new System.Drawing.Point(0, 0);
			this.logListView.MultiSelect = false;
			this.logListView.Name = "logListView";
			this.logListView.Size = new System.Drawing.Size(1006, 505);
			this.logListView.TabIndex = 1;
			this.logListView.UseCompatibleStateImageBehavior = false;
			this.logListView.View = System.Windows.Forms.View.Details;
			this.logListView.SelectedIndexChanged += new System.EventHandler(this.logListView_SelectedIndexChanged);
			this.logListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.logListView_MouseDown);
			this.logListView.MouseLeave += new System.EventHandler(this.logListView_MouseLeave);
			this.logListView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.logListView_MouseMove);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Revision";
			this.columnHeader1.Width = 80;
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "Actions";
			this.columnHeader7.Width = 80;
			// 
			// columnHeader8
			// 
			this.columnHeader8.Text = "Count";
			this.columnHeader8.Width = 80;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Author";
			this.columnHeader2.Width = 100;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Date";
			this.columnHeader3.Width = 200;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Message";
			this.columnHeader4.Width = 500;
			// 
			// splitContainer3
			// 
			this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer3.Location = new System.Drawing.Point(0, 0);
			this.splitContainer3.Name = "splitContainer3";
			// 
			// splitContainer3.Panel1
			// 
			this.splitContainer3.Panel1.Controls.Add(this.fileListView);
			// 
			// splitContainer3.Panel2
			// 
			this.splitContainer3.Panel2.Controls.Add(this.ignoreTreeView);
			this.splitContainer3.Size = new System.Drawing.Size(1006, 229);
			this.splitContainer3.SplitterDistance = 614;
			this.splitContainer3.TabIndex = 0;
			// 
			// fileListView
			// 
			this.fileListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.fileListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader9,
            this.columnHeader10});
			this.fileListView.ContextMenuStrip = this.ctxFile;
			this.fileListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fileListView.FullRowSelect = true;
			this.fileListView.HideSelection = false;
			this.fileListView.Location = new System.Drawing.Point(0, 0);
			this.fileListView.Name = "fileListView";
			this.fileListView.Size = new System.Drawing.Size(614, 229);
			this.fileListView.TabIndex = 3;
			this.fileListView.UseCompatibleStateImageBehavior = false;
			this.fileListView.View = System.Windows.Forms.View.Details;
			this.fileListView.VirtualMode = true;
			this.fileListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.fileListView_MouseClick);
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Path";
			this.columnHeader5.Width = 400;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Action";
			this.columnHeader6.Width = 100;
			// 
			// columnHeader9
			// 
			this.columnHeader9.Text = "Copy from path";
			this.columnHeader9.Width = 400;
			// 
			// columnHeader10
			// 
			this.columnHeader10.Text = "Revision";
			this.columnHeader10.Width = 100;
			// 
			// ctxFile
			// 
			this.ctxFile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuAddFile,
            this.menuRemoveFile});
			this.ctxFile.Name = "ctxPath";
			this.ctxFile.Size = new System.Drawing.Size(153, 48);
			this.ctxFile.Opening += new System.ComponentModel.CancelEventHandler(this.ctxFile_Opening);
			// 
			// menuAddFile
			// 
			this.menuAddFile.Name = "menuAddFile";
			this.menuAddFile.Size = new System.Drawing.Size(152, 22);
			this.menuAddFile.Text = "加入忽略列表";
			this.menuAddFile.Click += new System.EventHandler(this.menuAddFile_Click);
			// 
			// menuRemoveFile
			// 
			this.menuRemoveFile.Name = "menuRemoveFile";
			this.menuRemoveFile.Size = new System.Drawing.Size(152, 22);
			this.menuRemoveFile.Text = "移出忽略列表";
			this.menuRemoveFile.Click += new System.EventHandler(this.menuRemoveFile_Click);
			// 
			// ignoreTreeView
			// 
			this.ignoreTreeView.ContextMenuStrip = this.ctxIgnore;
			this.ignoreTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ignoreTreeView.FullRowSelect = true;
			this.ignoreTreeView.HideSelection = false;
			this.ignoreTreeView.ItemHeight = 16;
			this.ignoreTreeView.Location = new System.Drawing.Point(0, 0);
			this.ignoreTreeView.Name = "ignoreTreeView";
			this.ignoreTreeView.ShowLines = false;
			this.ignoreTreeView.Size = new System.Drawing.Size(388, 229);
			this.ignoreTreeView.TabIndex = 1;
			// 
			// ctxIgnore
			// 
			this.ctxIgnore.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuExpand,
            this.menuCollapse,
            this.toolStripSeparator2,
            this.menuRemoveIgnore,
            this.menuClearIgnore,
            this.menuReloadIgnore,
            this.toolStripSeparator3,
            this.menuFileView,
            this.menuRevisionView});
			this.ctxIgnore.Name = "ctxRevision";
			this.ctxIgnore.Size = new System.Drawing.Size(153, 170);
			this.ctxIgnore.Opening += new System.ComponentModel.CancelEventHandler(this.ctxIgnore_Opening);
			// 
			// menuExpand
			// 
			this.menuExpand.Name = "menuExpand";
			this.menuExpand.Size = new System.Drawing.Size(152, 22);
			this.menuExpand.Text = "展开";
			this.menuExpand.Click += new System.EventHandler(this.tsExpandAll_Click);
			// 
			// menuCollapse
			// 
			this.menuCollapse.Name = "menuCollapse";
			this.menuCollapse.Size = new System.Drawing.Size(152, 22);
			this.menuCollapse.Text = "折叠";
			this.menuCollapse.Click += new System.EventHandler(this.tsCollapseAll_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
			// 
			// menuRemoveIgnore
			// 
			this.menuRemoveIgnore.Name = "menuRemoveIgnore";
			this.menuRemoveIgnore.Size = new System.Drawing.Size(152, 22);
			this.menuRemoveIgnore.Text = "移出忽略列表";
			this.menuRemoveIgnore.Click += new System.EventHandler(this.menuRemoveIgnore_Click);
			// 
			// menuClearIgnore
			// 
			this.menuClearIgnore.Name = "menuClearIgnore";
			this.menuClearIgnore.Size = new System.Drawing.Size(152, 22);
			this.menuClearIgnore.Text = "清空忽略列表";
			this.menuClearIgnore.Click += new System.EventHandler(this.menuClearIgnore_Click);
			// 
			// menuReloadIgnore
			// 
			this.menuReloadIgnore.Name = "menuReloadIgnore";
			this.menuReloadIgnore.Size = new System.Drawing.Size(152, 22);
			this.menuReloadIgnore.Text = "重载忽略列表";
			this.menuReloadIgnore.Click += new System.EventHandler(this.menuReloadIgnore_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(149, 6);
			// 
			// menuFileView
			// 
			this.menuFileView.Name = "menuFileView";
			this.menuFileView.Size = new System.Drawing.Size(152, 22);
			this.menuFileView.Text = "按文件分组";
			this.menuFileView.Click += new System.EventHandler(this.menuFileView_Click);
			// 
			// menuRevisionView
			// 
			this.menuRevisionView.Name = "menuRevisionView";
			this.menuRevisionView.Size = new System.Drawing.Size(152, 22);
			this.menuRevisionView.Text = "按版本分组";
			this.menuRevisionView.Click += new System.EventHandler(this.menuRevisionView_Click);
			// 
			// ckCommitHistory
			// 
			this.ckCommitHistory.AutoSize = true;
			this.ckCommitHistory.Location = new System.Drawing.Point(716, 14);
			this.ckCommitHistory.Name = "ckCommitHistory";
			this.ckCommitHistory.Size = new System.Drawing.Size(96, 16);
			this.ckCommitHistory.TabIndex = 6;
			this.ckCommitHistory.Text = "提交历史版本";
			this.ckCommitHistory.UseVisualStyleBackColor = true;
			// 
			// ckShowOnlyAfterInit
			// 
			this.ckShowOnlyAfterInit.AutoSize = true;
			this.ckShowOnlyAfterInit.Checked = true;
			this.ckShowOnlyAfterInit.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ckShowOnlyAfterInit.Location = new System.Drawing.Point(242, 14);
			this.ckShowOnlyAfterInit.Name = "ckShowOnlyAfterInit";
			this.ckShowOnlyAfterInit.Size = new System.Drawing.Size(144, 16);
			this.ckShowOnlyAfterInit.TabIndex = 5;
			this.ckShowOnlyAfterInit.Text = "只显示初始版本后节点";
			this.ckShowOnlyAfterInit.UseVisualStyleBackColor = true;
			this.ckShowOnlyAfterInit.CheckedChanged += new System.EventHandler(this.ckShowOnlyAfterInit_CheckedChanged);
			// 
			// ckDelete
			// 
			this.ckDelete.AutoSize = true;
			this.ckDelete.Checked = true;
			this.ckDelete.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ckDelete.Location = new System.Drawing.Point(614, 14);
			this.ckDelete.Name = "ckDelete";
			this.ckDelete.Size = new System.Drawing.Size(96, 16);
			this.ckDelete.TabIndex = 4;
			this.ckDelete.Text = "删除无关文件";
			this.ckDelete.UseVisualStyleBackColor = true;
			// 
			// btnPublish
			// 
			this.btnPublish.Location = new System.Drawing.Point(818, 6);
			this.btnPublish.Name = "btnPublish";
			this.btnPublish.Size = new System.Drawing.Size(80, 30);
			this.btnPublish.TabIndex = 3;
			this.btnPublish.Text = "对外正式包";
			this.btnPublish.UseVisualStyleBackColor = true;
			this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
			// 
			// ckForce
			// 
			this.ckForce.AutoSize = true;
			this.ckForce.Location = new System.Drawing.Point(500, 14);
			this.ckForce.Name = "ckForce";
			this.ckForce.Size = new System.Drawing.Size(108, 16);
			this.ckForce.TabIndex = 2;
			this.ckForce.Text = "强制生成更新包";
			this.ckForce.UseVisualStyleBackColor = true;
			this.ckForce.CheckedChanged += new System.EventHandler(this.ckShowVersion_CheckedChanged);
			// 
			// FormVersion
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1008, 792);
			this.Controls.Add(this.splitContainer1);
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(880, 320);
			this.Name = "FormVersion";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Load += new System.EventHandler(this.FormVersion_Load);
			this.SizeChanged += new System.EventHandler(this.FormVersion_SizeChanged);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormVersion_KeyDown);
			this.ctxMenuStrip.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer3.Panel1.ResumeLayout(false);
			this.splitContainer3.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
			this.splitContainer3.ResumeLayout(false);
			this.ctxFile.ResumeLayout(false);
			this.ctxIgnore.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.CheckBox ckShowTops;
        private System.Windows.Forms.CheckBox ckShowVersion;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnCommit;
        private System.Windows.Forms.ContextMenuStrip ctxMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuStart;
        private System.Windows.Forms.ToolStripMenuItem menuImportant;
        private System.Windows.Forms.ToolStripMenuItem menuNormal;
        private System.Windows.Forms.ToolStripMenuItem menuRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SaveFileDialog saveVersionDlg;
        private System.Windows.Forms.Button btnPublish;
        private System.Windows.Forms.FolderBrowserDialog outputDlg;
        private System.Windows.Forms.CheckBox ckForce;
        private System.Windows.Forms.CheckBox ckDelete;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private CustomListView logListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.CheckBox ckShowOnlyAfterInit;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem menuHistory;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private CustomListView fileListView;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ContextMenuStrip ctxFile;
        private System.Windows.Forms.ToolStripMenuItem menuAddFile;
        private System.Windows.Forms.ToolStripMenuItem menuRemoveFile;
        private System.Windows.Forms.ContextMenuStrip ctxIgnore;
        private System.Windows.Forms.ToolStripMenuItem menuExpand;
        private System.Windows.Forms.ToolStripMenuItem menuCollapse;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuRemoveIgnore;
        private System.Windows.Forms.ToolStripMenuItem menuClearIgnore;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem menuFileView;
        private System.Windows.Forms.ToolStripMenuItem menuRevisionView;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem menuAddAll;
        private System.Windows.Forms.ToolStripMenuItem menuRemoveAll;
        private System.Windows.Forms.TreeView ignoreTreeView;
        private System.Windows.Forms.ToolStripMenuItem menuReloadIgnore;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem menuEarlydownload;
        private System.Windows.Forms.ToolStripMenuItem menuEarlydownloadBugfix;
        private System.Windows.Forms.ToolStripMenuItem menuEarlydownloadHistory;
		private System.Windows.Forms.CheckBox ckCommitHistory;
	}
}