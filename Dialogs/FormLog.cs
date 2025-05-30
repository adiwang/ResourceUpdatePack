using SharpSvn;
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
    public partial class FormLog : Form {

        public string LogMessage {
            get {
                return textLog.Text;
            }
        }

        public FormLog() {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e) {
            string url = LogMessage;
            if(url == string.Empty) {
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
