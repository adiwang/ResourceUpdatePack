using SharpSvn;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZLUtils;

namespace ResourceUpdatePack {
    public partial class FormVersion : Form {

        string assetsUri { get { return config.assetsUri; } }

        List<int> normalList = new List<int>();
        List<int> importantList = new List<int>();
        List<int> historyList = new List<int>();
        List<int> earlydownloadList = new List<int>();
        List<int> earlydownloadBugfixList = new List<int>();
        List<int> earlydownloadHistoryList = new List<int>();

        LoadingPanel loadingPanel;
        ListViewItem selectedItem;
        ListViewColumnSorter sorter = new ListViewColumnSorter();
        Font boldFont = new Font(Control.DefaultFont, FontStyle.Bold);

		List<LogInfo> logList;
		Boolean isLogListLimited;
		const Int32 logListLimitCount = 100;
		Boolean isLogListAfterInit;
		private PackerConfig config;

        class DescendComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return y.CompareTo(x);
            }
        }

        bool _viewByFile;
        bool viewByFile
        {
            get { return _viewByFile; }
            set
            {
                _viewByFile = value;
                menuFileView.Checked = _viewByFile;
                menuRevisionView.Checked = !_viewByFile;
            }
        }

        SortedDictionary<string, SortedSet<int>> ignoredDictByFile = new SortedDictionary<string, SortedSet<int>>();
        SortedDictionary<int, SortedSet<string>> ignoredDictByRevision = new SortedDictionary<int, SortedSet<string>>(new DescendComparer());

        public FormVersion(PackerConfig config)
        {
            InitializeComponent();

            loadingPanel = new LoadingPanel();
            Controls.Add(loadingPanel);
            loadingPanel.BringToFront();

            fileListView.ListViewItemSorter = sorter;
            fileListView.RetrieveVirtualItem += fileListView_RetrieveVirtualItem;
            //fileListView.CacheVirtualItems += fileListView_CacheVirtualItems;

            this.config = config;
            this.viewByFile = true;

            ReloadIgnore();
        }

        private void Packer_OnOutputLog(string message, LogMode mode) {
            if ((mode & LogMode.Gui) == LogMode.Gui) {
                this.Invoke(new Action(() => {
                    loadingPanel.Message = message;
                }));
            }
			if ((mode & LogMode.Console) == LogMode.Console){
				ConsoleUtil.Log(message);
			}
		}

        private void FormVersion_Load(object sender, EventArgs e) {
            if (String.IsNullOrEmpty(config.compareUri)) {
                Text = config.uri;
            } else {
                Text = string.Format("{0} -> {1}", config.uri, config.compareUri);
            }

			normalList = config.normalList.ToList();
			importantList = config.importantList.ToList();
			historyList = config.historyList.ToList();
            earlydownloadList = config.earlydownloadList.ToList();
            earlydownloadBugfixList = config.earlydownloadBugfixList.ToList();
            earlydownloadHistoryList = config.earlydownloadHistoryList.ToList();

			if (config.delete)
				ckDelete.Checked = true;
			if (config.force)
				ckForce.Checked = true;
			if (config.commit_history)
				ckCommitHistory.Checked = true;

            btnRefresh_Click(sender, e);
            FillIgnoreTreeView();
        }

        private void FormVersion_SizeChanged(object sender, EventArgs e) {
            Size size = Size - loadingPanel.Size;
            loadingPanel.Location = new Point(size.Width / 2, size.Height / 3);
        }

        private void FormVersion_KeyDown(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.F5) {
                btnRefresh_Click(sender, e);
            } else if(e.KeyCode == Keys.F6) {
                if(splitContainer2.Orientation == Orientation.Vertical) {
                    splitContainer2.Orientation = Orientation.Horizontal;
                } else {
                    splitContainer2.Orientation = Orientation.Vertical;
                }
            }
        }

        void ApplyChangeToConfig()
		{
			config.normalList = normalList.ToArray();
			config.importantList = importantList.ToArray();
			config.historyList = historyList.ToArray();
            config.earlydownloadList = earlydownloadList.ToArray();
            config.earlydownloadBugfixList = earlydownloadBugfixList.ToArray();
            config.earlydownloadHistoryList = earlydownloadHistoryList.ToArray();

			config.delete = ckDelete.Checked;
			config.force = ckForce.Checked;
			config.commit_history = ckCommitHistory.Checked;

            var dict = config.ignoredDict;
            dict.Clear();
            foreach (var e in ignoredDictByFile)
                dict.Add(e.Key, e.Value.ToArray());
        }

		Packer createPacker()
		{
			Packer packer = new Packer(config);
			packer.OnOutputLog += Packer_OnOutputLog;
			return packer;
		}

        void enablePanel(bool enable) {
            splitContainer1.Enabled = enable;
            loadingPanel.Visible = !enable;
        }

        void updateFileListView(LogInfo info) {
            int newCount = 0;
            int oldCount = fileListView.Items.Count;

            fileListView.BeginUpdate();

            ListViewItem item;
            foreach(var entry in info.ChangeList) {
                if(newCount < oldCount) {
                    item = fileListView.Items[newCount];
                } else {
                    item = new ListViewItem();
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    fileListView.Items.Add(item);
                }

                item.Text = entry.Path;
                item.SubItems[1].Text = entry.Action.ToString();
                if(entry.CopyFromPath != null)
                {
                    item.SubItems[2].Text = entry.CopyFromPath;
                    item.SubItems[3].Text = entry.CopyFromRevision.ToString();
                }
                else
                {
                    item.SubItems[2].Text = "";
                    item.SubItems[3].Text = "";
                }

                if(entry.Action == SvnChangeAction.Add || entry.Action == SvnChangeAction.Replace) {
                    item.ForeColor = Color.Purple;
                } else if(entry.Action == SvnChangeAction.Delete) {
                    item.ForeColor = Color.Firebrick;
                } else if(entry.Action == SvnChangeAction.Modify) {
                    item.ForeColor = Color.Blue;
                }
                ++newCount;
            }

            for(int i = newCount; i < oldCount; ++i) {
                item = fileListView.Items[newCount];
                item.Remove();
            }
            fileListView.EndUpdate();
        }

        void ValidateChangeBeforeApply()
        {
            int startRevision = normalList.Count > 0 ? normalList[0] : 0;
            int headRevision = (logList != null && logList.Count > 0) ? logList[0].Revision : 0;
            PackerConfig.ValidateEarlydownloadTags(earlydownloadList.ToArray(), earlydownloadBugfixList.ToArray(), startRevision, headRevision);
        }

        void updateLogListView() {
			if (this.logList == null)
			{
				return;
			}
            int newCount = 0;
            int oldCount = logListView.Items.Count;
			int newSelectedIndex = -1;
			int oldSelectedRevision = -1;
			{
				var sels = logListView.SelectedItems;
				if (sels.Count > 0)
					oldSelectedRevision = ((LogInfo)sels[0].Tag).Revision;
			}

            logListView.BeginUpdate();
			logListView.SelectedIndices.Clear();

            List<LogInfo> logList = this.logList;
            for (Int32 i=0; i<logList.Count; ++i)
			{
				LogInfo info = logList[i];
                if(ckShowTops.Checked && i > logListLimitCount) {
                    break;
                }
				if(ckShowOnlyAfterInit.Checked && normalList.Count > 0 && info.Revision < normalList[0])
					break;

                bool isEarlydownload = earlydownloadList.Contains(info.Revision);
                bool isEarlydownloadBugfix = earlydownloadBugfixList.Contains(info.Revision);
                bool isEarlydownloadHistory = earlydownloadHistoryList.Contains(info.Revision);
                bool isStart = (normalList.Count > 0 && info.Revision == normalList[0]);
                bool isNormal = normalList.Contains(info.Revision);
                bool isImportant = importantList.Contains(info.Revision);
                bool isHistory = historyList.Contains(info.Revision);
                if(ckShowVersion.Checked) {
                    if(!isEarlydownload && !isEarlydownloadBugfix && !isEarlydownloadHistory && !isNormal && !isImportant && !isHistory) {
                        continue;
                    }
                }

				if (oldSelectedRevision >= 0 && info.Revision == oldSelectedRevision)
					newSelectedIndex = newCount;

                ListViewItem item;
                if(newCount >= oldCount) {
                    item = new ListViewItem();
                    logListView.Items.Add(item);
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                } else {
                    item = logListView.Items[newCount];
                }

                item.Tag = info;
                item.Text = info.Revision.ToString();
                item.SubItems[1].Text = (info.Status);
                item.SubItems[2].Text = (info.Count > 0 ? info.Count.ToString() : "");
                item.SubItems[3].Text = (info.Author);
                item.SubItems[4].Text = (info.Time.ToString("yyyy年MM月dd日 HH:mm:ss"));
                item.SubItems[5].Text = (info.Message);

                if (isEarlydownload) {
                    item.Font = boldFont;
                    item.ForeColor = Color.DeepSkyBlue;
                } else if(isEarlydownloadBugfix) {
                    item.Font = boldFont;
                    item.ForeColor = Color.OrangeRed;
                } else if(isEarlydownloadHistory) {
                    item.Font = boldFont;
                    item.ForeColor = Color.DarkOrange;
                } else if(isStart) {
                    item.Font = boldFont;
                    item.ForeColor = Color.Green;
                } else if(isImportant) {
                    item.Font = boldFont;
                    item.ForeColor = Color.Purple;
                } else if(isNormal) {
                    item.Font = boldFont;
                    item.ForeColor = Color.Blue;
                } else if(isHistory) {
                    item.Font = boldFont;
                    item.ForeColor = Color.Black;
				} else {
                    item.Font = DefaultFont;
                    item.ForeColor = Color.Gray;
                }

                ++newCount;
            }

            for(int i = newCount; i < oldCount; ++i) {
                logListView.Items.RemoveAt(newCount);
            }

			//恢复 sel
			if (newSelectedIndex >= 0)
			{
				logListView.SelectedIndices.Add(newSelectedIndex);
				logListView.SelectedItems[0].EnsureVisible();
			}

            logListView.EndUpdate();
        }

        void fetchLog(Action callback) {
            ThreadPool.QueueUserWorkItem((obj) => {
                try {
					Packer packer = createPacker();

					isLogListLimited = this.ckShowTops.Checked;
					isLogListAfterInit = this.ckShowOnlyAfterInit.Checked;
					int limit = isLogListLimited ? logListLimitCount : 0;
					SvnRevision revisionTo = isLogListAfterInit && normalList.Count > 0 ? new SvnRevision(normalList[0]) : SvnRevision.One;
                    logList = packer.GetLog(SvnRevision.Head, revisionTo, limit);
                    Invoke(callback);
                } catch(Exception e) {
                    Invoke(new Action(() => {
                        MessageBox.Show(e.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        callback();
                    }));
                }
            });
        }

        void buildPack(Action<bool> callback) {
            if(normalList.Count == 0) {
                MessageBox.Show("请设置版本节点", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                callback(false);
                return;
            }
            try
            {
                ValidateChangeBeforeApply();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                callback(false);
                return;
            }

			ApplyChangeToConfig();

            ThreadPool.QueueUserWorkItem((obj) =>
			{

                try {
					Packer packer = createPacker();
                    List<PackInfo> result = packer.Pack();
                    Invoke(new Action(() => {
                        if(result.Count > 0) {
                            new FormResult { Result = result }.ShowDialog(this);
                        }
						callback(true);
                    }));
                } catch(Exception e) {
                    Invoke(new Action(() => {
                        MessageBox.Show(e.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
						callback(false);
                    }));
                }
            });
        }

        private void ckShowVersion_CheckedChanged(object sender, EventArgs e) {
            updateLogListView();
        }

        private void ckShowTops_CheckedChanged(object sender, EventArgs e) {
			onLogListFilterChanged();
        }

		private void ckShowOnlyAfterInit_CheckedChanged(object sender, EventArgs e)
		{
			onLogListFilterChanged();
		}

		private void onLogListFilterChanged()
		{
			Boolean isNeedFetchLog = !ckShowTops.Checked && isLogListLimited || !ckShowOnlyAfterInit.Checked && isLogListAfterInit;
			if (isNeedFetchLog)
			{
				enablePanel(false);
				fetchLog(() => {
					updateLogListView();
					enablePanel(true);
				});
			}
			else
			{
				updateLogListView();
			}

		}

        private void ctxMenuStrip_Opened(object sender, EventArgs e) {
            menuStart.Checked = false;
            menuNormal.Checked = false;
            menuImportant.Checked = false;
            menuHistory.Checked = false;
            menuEarlydownload.Checked = false;
            menuEarlydownloadBugfix.Checked = false;
            menuEarlydownloadHistory.Checked = false;
            if(selectedItem != null) {
                LogInfo info = selectedItem.Tag as LogInfo;
                if(normalList.Count > 0 && info.Revision == normalList[0]) {
                    menuStart.Checked = true;
                } else {
                    if(importantList.Contains(info.Revision)) {
                        menuImportant.Checked = true;
                    } else if(normalList.Contains(info.Revision)) {
                        menuNormal.Checked = true;
                    }
                }

				menuHistory.Checked = historyList.Contains(info.Revision);
                menuEarlydownload.Checked = earlydownloadList.Contains(info.Revision);
                menuEarlydownloadBugfix.Checked = earlydownloadBugfixList.Contains(info.Revision);
                menuEarlydownloadHistory.Checked = earlydownloadHistoryList.Contains(info.Revision);
            }
        }

        private void menuImportant_Click(object sender, EventArgs e) {
            if(selectedItem != null) {
                LogInfo info = selectedItem.Tag as LogInfo;
                if(!normalList.Contains(info.Revision)) {
                    normalList.Add(info.Revision);
                    normalList.Sort();
                }

                if(!importantList.Contains(info.Revision)) {
                    importantList.Add(info.Revision);
                    importantList.Sort();
                }

                updateLogListView();
            }
        }

        private void menuNormal_Click(object sender, EventArgs e) {
            if(selectedItem != null) {
                LogInfo info = selectedItem.Tag as LogInfo;
                if(!normalList.Contains(info.Revision)) {
                    normalList.Add(info.Revision);
                    normalList.Sort();
                }

                if(importantList.Contains(info.Revision)) {
                    importantList.Remove(info.Revision);
                }

                updateLogListView();
            }
        }

        private void menuStart_Click(object sender, EventArgs e) {
            if(selectedItem != null) {
                LogInfo info = selectedItem.Tag as LogInfo;
                DialogResult ret = MessageBox.Show("将本版本设置为更新起始版本后，版本号低于本版本的客户端将不能自动更新，请谨慎操作", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if(ret == DialogResult.OK) {
                    normalList.RemoveAll((i) => i <= info.Revision);
                    importantList.RemoveAll((i) => i <= info.Revision);
                    earlydownloadList.RemoveAll((i) => i < info.Revision);
                    earlydownloadBugfixList.RemoveAll((i) => i <= info.Revision);
                    normalList.Insert(0, info.Revision);
                    updateLogListView();
                }
            }
        }

        private void menuRemove_Click(object sender, EventArgs e) {
            if(selectedItem != null) {
                LogInfo info = selectedItem.Tag as LogInfo;
                normalList.Remove(info.Revision);
                importantList.Remove(info.Revision);
                earlydownloadList.Remove(info.Revision);
                earlydownloadBugfixList.Remove(info.Revision);
                updateLogListView();
            }
        }

		private Boolean hasPromptAddHistoryWarning = false;
		private Boolean hasPromptRemoveHistoryWarning = false;

		private void menuHistory_Click(object sender, EventArgs e)
		{
            if(selectedItem != null) {
                LogInfo info = selectedItem.Tag as LogInfo;
                if (!menuHistory.Checked)	//转为 checked
				{
					if (!hasPromptAddHistoryWarning)
					{
						var result = MessageBox.Show(string.Format("手动添加历史版本记录可能造成更新包冗余内容，是否确实要添加？"), "注意", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
						if (result == DialogResult.Cancel)
							return;
						hasPromptAddHistoryWarning = true;
					}

					historyList.Add(info.Revision);
					historyList = historyList.Distinct().OrderBy(i=>i).ToList();
				}
				else	//转为 unchecked
				{
					if (!hasPromptRemoveHistoryWarning)
					{
						var result = MessageBox.Show(string.Format("手动删除历史版本记录可能造成更新包内容缺失，是否确实要删除？"), "注意", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
						if (result == DialogResult.Cancel)
							return;
						hasPromptRemoveHistoryWarning = true;
					}

					historyList.RemoveAll(revision=>revision==info.Revision);
				}
                updateLogListView();
			}
		}

        private void menuEarlydownload_Click(object sender, EventArgs e)
        {
            if (selectedItem == null)
            {
                return;
            }
            LogInfo info = selectedItem.Tag as LogInfo;
            if (!earlydownloadList.Contains(info.Revision))
            {
                earlydownloadBugfixList.RemoveAll((revision) => revision == info.Revision);
                earlydownloadList.Add(info.Revision);
                earlydownloadList.Sort();

                updateLogListView();
            }
            else
            {
                earlydownloadList.RemoveAll(revision => revision == info.Revision);

                updateLogListView();
            }
        }

        private void menuEarlydownloadBugfix_Click(object sender, EventArgs e)
        {
            if (selectedItem == null)
            {
                return;
            }
            LogInfo info = selectedItem.Tag as LogInfo;
            if (!earlydownloadBugfixList.Contains(info.Revision))
            {
                earlydownloadList.RemoveAll((revision) => revision == info.Revision);
                earlydownloadBugfixList.Add(info.Revision);
                earlydownloadBugfixList.Sort();

                updateLogListView();
            }
            else
            {
                earlydownloadBugfixList.RemoveAll(revision => revision == info.Revision);

                updateLogListView();
            }
        }

        private Boolean hasPromptAddEarlydownloadHistoryWarning = false;
        private Boolean hasPromptRemoveEarlydownloadHistoryWarning = false;
        private void menuEarlydownloadHistory_Click(object sender, EventArgs e)
        {
            if (selectedItem != null)
            {
                LogInfo info = selectedItem.Tag as LogInfo;
                if (!earlydownloadHistoryList.Contains(info.Revision))
                {
                    if (!hasPromptAddEarlydownloadHistoryWarning)
                    {
                        var result = MessageBox.Show(string.Format("手动添加可能导致更新包版本变化，是否添加？"), "注意", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                        if (result == DialogResult.Cancel)
                            return;
                        hasPromptAddEarlydownloadHistoryWarning = true;
                    }

                    earlydownloadHistoryList.Add(info.Revision);
                    earlydownloadHistoryList.Sort();
                }
                else    //转为 unchecked
                {
                    if (!hasPromptRemoveEarlydownloadHistoryWarning)
                    {
                        var result = MessageBox.Show(string.Format("手动删除可能导致更新包版本变化，是否删除？"), "注意", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                        if (result == DialogResult.Cancel)
                            return;
                        hasPromptRemoveEarlydownloadHistoryWarning = true;
                    }

                    earlydownloadHistoryList.RemoveAll(revision => revision == info.Revision);
                }
                updateLogListView();
            }
        }

        private void logListView_MouseLeave(object sender, EventArgs e) {
            ListView _listView = (ListView)sender;
            ListViewItem _oldItem = null;
            _oldItem = (ListViewItem)_listView.Tag;
            if(_oldItem != null) {
                _oldItem.BackColor = _listView.BackColor;
                _listView.Tag = null;
            }
        }

        private void logListView_MouseMove(object sender, MouseEventArgs e) {
            ListView _listView = (ListView)sender;
            ListViewItem _oldItem = null;
            if(_listView.Tag != null)
                _oldItem = (ListViewItem)_listView.Tag;
            ListViewItem _item = _listView.GetItemAt(e.X, e.Y);
            if(_item != null) {
                if(_oldItem != null && !_oldItem.Equals(_item)) {
                    _oldItem.BackColor = _listView.BackColor;
                    _item.BackColor = Color.LightSkyBlue;
                    _listView.Tag = _item;
                } else if(_oldItem == null) {
                    _item.BackColor = Color.LightSkyBlue;
                    _listView.Tag = _item;
                }
            } else {
                if(_oldItem != null && !_oldItem.BackColor.Equals(_listView.BackColor)) {
                    _oldItem.BackColor = _listView.BackColor;
                    _listView.Tag = null;
                }
            }
        }

        private void logListView_MouseDown(object sender, MouseEventArgs e) {
            selectedItem = logListView.HitTest(e.X, e.Y).Item;
            if(e.Button == MouseButtons.Right && selectedItem != null) {
                ctxMenuStrip.Show(logListView, e.Location);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e) {
            enablePanel(false);
            fetchLog(() => {
                updateLogListView();
                enablePanel(true);
            });
        }

        private void btnPublish_Click(object sender, EventArgs e) {
            enablePanel(false);
            buildPack((success) => {
                enablePanel(true);
				if (success)
				{
					FormVersion_Load(null, null);   //	历史记录等可能发生变化，需重新加载，避免"保存和提交"时覆盖
				}
			});
        }

        private void btnCommit_Click(object sender, EventArgs e) {
            try {
                ValidateChangeBeforeApply();
            } catch(Exception ex) {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try {
                FormLog form = new FormLog();
                if(form.ShowDialog(this) == DialogResult.OK) {
					ApplyChangeToConfig();
					config.SaveAndCommitAll(form.LogMessage);
                }
            } catch(Exception ex) {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void logListView_SelectedIndexChanged(object sender, EventArgs e) {
            if(logListView.SelectedItems.Count == 0) {
                fileListView.Tag = null;
                fileListView.Items.Clear();
            } else {
                selectedItem = logListView.SelectedItems[0];
                fileListView.Tag = selectedItem.Tag;
                fileListView.VirtualListSize = (selectedItem.Tag as LogInfo).Count;
                fileListView.Invalidate();
            }
        }

        private void fileListView_ColumnClick(object sender, ColumnClickEventArgs e) {
            //if(e.Column == sorter.SortColumn) {
            //    if(sorter.Order == SortOrder.Ascending) {
            //        sorter.Order = SortOrder.Descending;
            //    } else {
            //        sorter.Order = SortOrder.Ascending;
            //    }
            //} else {
            //    sorter.SortColumn = e.Column;
            //    sorter.Order = SortOrder.Ascending;
            //}

            //fileListView.Sort();
        }

        private void fileListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            ListViewItem item = new ListViewItem();
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");

            if (selectedItem != null)
            {
                LogInfo info = selectedItem.Tag as LogInfo;
                var entry = info.ChangeList[e.ItemIndex];
                item.Text = entry.Path;
                item.SubItems[1].Text = entry.Action.ToString();
                if (entry.CopyFromPath != null)
                {
                    item.SubItems[2].Text = entry.CopyFromPath;
                    item.SubItems[3].Text = entry.CopyFromRevision.ToString();
                }
                else
                {
                    item.SubItems[2].Text = "";
                    item.SubItems[3].Text = "";
                }

                if (entry.Action == SvnChangeAction.Add || entry.Action == SvnChangeAction.Replace)
                {
                    item.ForeColor = Color.Purple;
                }
                else if (entry.Action == SvnChangeAction.Delete)
                {
                    item.ForeColor = Color.Firebrick;
                }
                else if (entry.Action == SvnChangeAction.Modify)
                {
                    item.ForeColor = Color.Blue;
                }
            }

            e.Item = item;
        }

        private void fileListView_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e) {
        }

        void FillIgnoreTreeViewByFile()
        {
            int newCount = ignoredDictByFile.Count;
            int oldCount = ignoreTreeView.Nodes.Count;

            int index = 0;

            ignoreTreeView.BeginUpdate();
            foreach (var e in ignoredDictByFile)
            {
                TreeNode node;
                if (index < oldCount)
                {
                    node = ignoreTreeView.Nodes[index];
                }
                else
                {
                    node = new TreeNode();
                    ignoreTreeView.Nodes.Add(node);
                }

                node.Text = e.Key;
                ++index;

                int newCount2 = e.Value.Count;
                int oldCount2 = node.Nodes.Count;

                int index2 = 0;
                foreach (var r in e.Value)
                {
                    TreeNode node2;
                    if (index2 < oldCount2)
                    {
                        node2 = node.Nodes[index2];
                    }
                    else
                    {
                        node2 = new TreeNode();
                        node.Nodes.Add(node2);
                    }

                    node2.Tag = r;
                    node2.Text = r.ToString();
                    ++index2;
                }

                for (index2 = newCount2; index2 < oldCount2; ++index2)
                    node.Nodes.RemoveAt(newCount2);
            }

            for (index = newCount; index < oldCount; ++index)
                ignoreTreeView.Nodes.RemoveAt(newCount);

            ignoreTreeView.EndUpdate();
        }

        void FillIgnoreTreeViewByRevision()
        {
            int newCount = ignoredDictByRevision.Count;
            int oldCount = ignoreTreeView.Nodes.Count;

            int index = 0;

            ignoreTreeView.BeginUpdate();
            foreach (var e in ignoredDictByRevision)
            {
                TreeNode node;
                if (index < oldCount)
                {
                    node = ignoreTreeView.Nodes[index];
                }
                else
                {
                    node = new TreeNode();
                    ignoreTreeView.Nodes.Add(node);
                }

                node.Tag = e.Key;
                node.Text = e.Key.ToString();
                ++index;

                int newCount2 = e.Value.Count;
                int oldCount2 = node.Nodes.Count;

                int index2 = 0;
                foreach (var s in e.Value)
                {
                    TreeNode node2;
                    if (index2 < oldCount2)
                    {
                        node2 = node.Nodes[index2];
                    }
                    else
                    {
                        node2 = new TreeNode();
                        node.Nodes.Add(node2);
                    }

                    node2.Text = s;
                    ++index2;
                }

                for (index2 = newCount2; index2 < oldCount2; ++index2)
                    node.Nodes.RemoveAt(newCount2);
            }

            for (index = newCount; index < oldCount; ++index)
                ignoreTreeView.Nodes.RemoveAt(newCount);

            ignoreTreeView.EndUpdate();
        }

        void FillIgnoreTreeView()
        {
            if (viewByFile)
                FillIgnoreTreeViewByFile();
            else
                FillIgnoreTreeViewByRevision();
        }

        TreeNode findTreeNode(TreeNodeCollection nodes, string text)
        {
            foreach(TreeNode node in nodes)
            {
                if (node.Text == text)
                    return node;
            }

            return null;
        }

        private void fileListView_MouseClick(object sender, MouseEventArgs e)
        {
            LogInfo info = fileListView.Tag as LogInfo;
            if (info == null)
                return;

            var item = fileListView.GetItemAt(e.X, e.Y);
            if (item == null)
                return;

            if (viewByFile)
            {
                if (ignoredDictByFile.ContainsKey(item.Text))
                {
                    TreeNode node = findTreeNode(ignoreTreeView.Nodes, item.Text);
                    if (node != null)
                    {
                        node.Expand();
                        TreeNode child = findTreeNode(node.Nodes, info.Revision.ToString());
                        if (child != null)
                            ignoreTreeView.SelectedNode = child;
                        else
                            ignoreTreeView.SelectedNode = node;
                    }
                }
            }
            else
            {
                if (ignoredDictByRevision.ContainsKey(info.Revision))
                {
                    TreeNode node = findTreeNode(ignoreTreeView.Nodes, info.Revision.ToString());
                    if (node != null)
                    {
                        node.Expand();
                        TreeNode child = findTreeNode(node.Nodes, item.Text);
                        if (child != null)
                            ignoreTreeView.SelectedNode = child;
                        else
                            ignoreTreeView.SelectedNode = node;
                    }
                }
            }
        }

        bool AddToIgnoreDict(string path, int revision)
        {
            bool changed = false;
            SortedSet<int> ignoreList;
            if (ignoredDictByFile.TryGetValue(path, out ignoreList))
            {
                changed = ignoreList.Add(revision);
            }
            else
            {
                ignoreList = new SortedSet<int>(new DescendComparer());
                ignoreList.Add(revision);
                ignoredDictByFile.Add(path, ignoreList);
                changed = true;
            }

            SortedSet<string> ignoreFiles;
            if (ignoredDictByRevision.TryGetValue(revision, out ignoreFiles))
            {
                changed = ignoreFiles.Add(path) || changed;
            }
            else
            {
                ignoreFiles = new SortedSet<string>();
                ignoreFiles.Add(path);
                ignoredDictByRevision.Add(revision, ignoreFiles);
                changed = true;
            }
            return changed;
        }

        bool RemoveFromIgnore(string path)
        {
            SortedSet<int> ignoreList;
            if (ignoredDictByFile.TryGetValue(path, out ignoreList))
            {
                foreach (var r in ignoreList)
                {
                    SortedSet<string> ignoreFiles;
                    if (ignoredDictByRevision.TryGetValue(r, out ignoreFiles))
                    {
                        ignoreFiles.Remove(path);
                        if (ignoreFiles.Count == 0)
                        {
                            ignoredDictByRevision.Remove(r);
                        }
                    }
                }

                ignoredDictByFile.Remove(path);
                return true;
            }
            return false;
        }

        bool RemoveFromIgnore(int revision)
        {
            SortedSet<string> ignoreFiles;
            if (ignoredDictByRevision.TryGetValue(revision, out ignoreFiles))
            {
                foreach (var s in ignoreFiles)
                {
                    SortedSet<int> ignoreList;
                    if (ignoredDictByFile.TryGetValue(s, out ignoreList))
                    {
                        ignoreList.Remove(revision);
                        if (ignoreList.Count == 0)
                        {
                            ignoredDictByFile.Remove(s);
                        }
                    }
                }

                ignoredDictByRevision.Remove(revision);
                return true;
            }
            return false;
        }

        bool RemoveFromIgnore(string path, int revision)
        {
            bool changed = false;

            SortedSet<int> ignoreList;
            if (ignoredDictByFile.TryGetValue(path, out ignoreList))
            {
                changed = ignoreList.Remove(revision);
                if (ignoreList.Count == 0)
                    changed = ignoredDictByFile.Remove(path) || changed;
            }

            SortedSet<string> ignoreFiles;
            if (ignoredDictByRevision.TryGetValue(revision, out ignoreFiles))
            {
                changed = ignoreFiles.Remove(path) || changed;
                if (ignoreFiles.Count == 0)
                    changed = ignoredDictByRevision.Remove(revision) || changed;
            }

            return changed;
        }

        private void menuAddAll_Click(object sender, EventArgs e)
        {
            if (logListView.SelectedItems.Count == 0)
                return;

            var selectedItem = logListView.SelectedItems[0];
            var info = selectedItem.Tag as LogInfo;
            if (info == null)
                return;

            bool changed = false;
            foreach (var item in info.ChangeList)
            {
                if (item.NodeKind == SvnNodeKind.File)
                {
                    changed = AddToIgnoreDict(item.Path, item.Revision) || changed;
                }
            }

            if (changed)
                FillIgnoreTreeView();
        }

        private void menuRemoveAll_Click(object sender, EventArgs e)
        {
            if (logListView.SelectedItems.Count == 0)
                return;

            var selectedItem = logListView.SelectedItems[0];
            var info = selectedItem.Tag as LogInfo;
            if (info == null)
                return;

            bool changed = false;
            foreach (var item in info.ChangeList)
            {
                if (item.NodeKind == SvnNodeKind.File)
                {
                    changed = RemoveFromIgnore(item.Path, item.Revision) || changed;
                }
            }

            if (changed)
                FillIgnoreTreeView();
        }

        private void menuAddFile_Click(object sender, EventArgs e)
        {
            var info = fileListView.Tag as LogInfo;
            if (info == null)
                return;

            bool changed = false;
            var changes = info.ChangeList;
            var indices = fileListView.SelectedIndices;
            for (int i = 0; i < indices.Count; ++i)
            {
                var item = changes[indices[i]];
                if (item.NodeKind == SvnNodeKind.File)
                    changed = AddToIgnoreDict(item.Path, item.Revision) || changed;
            }

            if (changed)
                FillIgnoreTreeView();
        }

        private void menuRemoveFile_Click(object sender, EventArgs e)
        {
            var info = fileListView.Tag as LogInfo;
            if (info == null)
                return;

            bool changed = false;
            var changes = info.ChangeList;
            var indices = fileListView.SelectedIndices;
            for (int i = 0; i < indices.Count; ++i)
            {
                var item = changes[indices[i]];
                if (item.NodeKind == SvnNodeKind.File)
                    changed = RemoveFromIgnore(item.Path, item.Revision) || changed;
            }

            if (changed)
                FillIgnoreTreeView();
        }

        private void ctxFile_Opening(object sender, CancelEventArgs e)
        {
            var p = fileListView.PointToClient(MousePosition);
            ctxFile.Tag = fileListView.GetItemAt(p.X, p.Y);
        }

        private void menuRemoveIgnore_Click(object sender, EventArgs e)
        {
            var node = ctxIgnore.Tag as TreeNode;
            if (node == null)
                return;

            if (node.Parent == null)
            {
                if (viewByFile)
                    RemoveFromIgnore(node.Text);
                else
                    RemoveFromIgnore((int)node.Tag);
                node.Remove();
            }
            else
            {
                if (viewByFile)
                    RemoveFromIgnore(node.Parent.Text, (int)node.Tag);
                else
                    RemoveFromIgnore(node.Text, (int)node.Parent.Tag);
                if (node.Parent.Nodes.Count == 1)
                    node.Parent.Remove();
                else
                    node.Remove();
            }
        }

        private void menuClearIgnore_Click(object sender, EventArgs e)
        {
            ignoredDictByFile.Clear();
            ignoredDictByRevision.Clear();
            ignoreTreeView.Nodes.Clear();
        }

        private void menuReloadIgnore_Click(object sender, EventArgs e)
        {
            ReloadIgnore();
            FillIgnoreTreeView();
        }

        private void ctxIgnore_Opening(object sender, CancelEventArgs e)
        {
            var p = ignoreTreeView.PointToClient(MousePosition);
            var node = ignoreTreeView.GetNodeAt(p.X, p.Y);
            if (node != null)
                ignoreTreeView.SelectedNode = node;

            ctxIgnore.Tag = node;
            menuRemoveIgnore.Enabled = node != null;
        }

        private void menuFileView_Click(object sender, EventArgs e)
        {
            if (!viewByFile)
            {
                viewByFile = true;
                FillIgnoreTreeViewByFile();
            }
        }

        private void menuRevisionView_Click(object sender, EventArgs e)
        {
            if (viewByFile)
            {
                viewByFile = false;
                FillIgnoreTreeViewByRevision();
            }
        }

        private void tsExpandAll_Click(object sender, EventArgs e)
        {
            ignoreTreeView.ExpandAll();
        }

        private void tsCollapseAll_Click(object sender, EventArgs e)
        {
            ignoreTreeView.CollapseAll();
        }

        void SaveIgnore()
        {
            var dict = config.ignoredDict;
            dict.Clear();
            foreach (var e in ignoredDictByFile)
                dict.Add(e.Key, e.Value.ToArray());
        }

        void ReloadIgnore()
        {
            ignoredDictByFile.Clear();
            ignoredDictByRevision.Clear();

            foreach (var e in config.ignoredDict)
            {
                ignoredDictByFile.Add(e.Key, new SortedSet<int>(e.Value, new DescendComparer()));
                foreach (var r in e.Value)
                {
                    SortedSet<string> files;
                    if (ignoredDictByRevision.TryGetValue(r, out files))
                    {
                        files.Add(e.Key);
                    }
                    else
                    {
                        files = new SortedSet<string>();
                        files.Add(e.Key);
                        ignoredDictByRevision.Add(r, files);
                    }
                }
            }
        }
    }
}
