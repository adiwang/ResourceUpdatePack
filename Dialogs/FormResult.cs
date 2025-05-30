using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResourceUpdatePack {
    public partial class FormResult : Form {

        public List<PackInfo> Result {
            get;
            set;
        }

        public FormResult() {
            InitializeComponent();
        }

        private void FormResult_Load(object sender, EventArgs e) {
            foreach(PackInfo info in Result) {
                ListViewItem item = new ListViewItem();
                item.Text = info.Name;
                item.SubItems.Add(info.Hash);
                item.SubItems.Add(info.Size.ToString());
                if(info.New) {
                    item.ForeColor = Color.Purple;
                }
                pakListView.Items.Add(item);
            }
        }
    }
}
