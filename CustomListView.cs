using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResourceUpdatePack {

    class ListViewColumnSorter : IComparer {

        int columnToSort;
        SortOrder orderOfSort;
        CaseInsensitiveComparer objectCompare;

        public ListViewColumnSorter() {
            columnToSort = 0;
            orderOfSort = SortOrder.None;
            objectCompare = new CaseInsensitiveComparer();
        }

        public int Compare(object x, object y) {
            ListViewItem itemX = (ListViewItem)x;
            ListViewItem itemY = (ListViewItem)y;
            int result = objectCompare.Compare(itemX.SubItems[columnToSort].Text, itemY.SubItems[columnToSort].Text);
            if(orderOfSort == SortOrder.Ascending) {
                return result;
            }

            if(orderOfSort == SortOrder.Descending) {
                return -result;
            }

            return 0;
        }

        public int SortColumn {
            get {
                return columnToSort;
            } set {
                columnToSort = value;
            }
        }

        public SortOrder Order {
            get {
                return orderOfSort;
            } set {
                orderOfSort = value;
            }
        }
    }

    class CustomListView : ListView {

        public CustomListView() {
            //开启双缓存
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
        }

        protected override void OnNotifyMessage(Message m) {
            if(m.Msg != 0x14) {
                base.OnNotifyMessage(m);
            }
        }

        private void InitializeComponent() {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }

}
