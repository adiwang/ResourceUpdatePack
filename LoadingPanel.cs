using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResourceUpdatePack {
    public partial class LoadingPanel : UserControl {

        public string Message {
            get {
                return label.Text;
            } set {
                label.Text = value;
            }
        }

        public LoadingPanel() {
            InitializeComponent();
        }
    }
}
