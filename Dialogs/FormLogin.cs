using SharpSvn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZLUtils;

namespace ResourceUpdatePack
{
    public partial class FormLogin : Form
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string appName, string keyName, string sString, string fileName);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string appName, string keyName, string sDefault, StringBuilder returnedString, int nSize, string fileName);

        const String inifile = "userinfo.ini";
        SvnInfoEventArgs _args = null;

        String _svn_addr;
        String _username;
        String _password;

        public FormLogin()
        {
            InitializeComponent();

            this.Focus();
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            this.tb_tips.Text = "";
            String file = Path.Combine(App.ModulePath, inifile);
            if(File.Exists(file))
            {
                StringBuilder tmp = new StringBuilder();

                tmp.Clear();
                GetPrivateProfileString("info", "svn", "", tmp, 500, file);
                this.tb_svnaddr.Text = tmp.ToString();

                tmp.Clear();
                GetPrivateProfileString("info", "username", "", tmp, 500, file);
                this.tb_username.Text = tmp.ToString();

                tmp.Clear();
                GetPrivateProfileString("info", "password", "", tmp, 500, file);
                this.tb_username.Text = tmp.ToString();
            }
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            this.tb_tips.Text = "";
            String url = this.tb_svnaddr.Text;
            String username = this.tb_username.Text;
            String password = this.tb_password.Text;

            _svn_addr = url;
            _username = username;
            _password = password;

            if(String.IsNullOrEmpty(url))
            {
                MessageBox.Show("请输入远程svn地址", "[Login]");
                return;
            }
            
            this.tb_tips.Text = "登陆中......";

            try
            {
                Uri uri = new Uri(url);
                SvnTarget target = SvnTarget.FromUri(uri);
                SvnClient client = new SvnClient();
                if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
                {
                    client.Authentication.UserNamePasswordHandlers += (sd, ee) =>
                    {
                        ee.UserName = username;
                        ee.Password = password;
                    };
                }

                if (!client.GetInfo(target, out _args))
                {
                    this.tb_tips.Text = "";
                    MessageBox.Show("无法连接远程svn地址", "[Login]");
					client.Dispose();
                    return;
                }
				if (_args.NodeKind != SvnNodeKind.Directory)
				{
					throw new Exception(url + " 不是有效的资源根路径");
				}
				{
					SvnInfoEventArgs infoArgs;
					SvnTarget configTarget = SvnTarget.FromString(url + "/" + PackerConfig.configDirName);	// 资源目录下需有 config 目录，内中有版本配置等
					if (!client.GetInfo(configTarget, out infoArgs))
					{
						throw new Exception(uri + " 不是有效的资源根路径");
					}
					SvnTarget assetsTarget = SvnTarget.FromString(url + "/" + "assets");                    // 资源目录下时应有 assets 目录，如果目录名称不同，请直接使用 --uri=*** --assets=*** 启动
					if (!client.GetInfo(assetsTarget, out infoArgs))
					{
						throw new Exception(uri + " 不是有效的资源根路径");
					}
				}

				client.Dispose();
                SaveLoginInfo();
                this.DialogResult = DialogResult.OK;
            }
            catch(Exception ex)
            {
                this.tb_tips.Text = "";
                MessageBox.Show("无法连接远程svn地址，错误原因：" + ex.Message, "[Login]");
                return;
            }
        }

        void SaveLoginInfo()
        {
            String url = this.tb_svnaddr.Text;
            String username = this.tb_username.Text;
            String password = this.tb_password.Text;

            //密码暂时不加密存储了
            String file = Path.Combine(App.ModulePath, inifile);
            WritePrivateProfileString("info", "svn", url, file);
            WritePrivateProfileString("info", "username", username, file);
            WritePrivateProfileString("info", "password", password, file);
        }


        public String SvnUri
        {
            get
            {
                return _svn_addr;
            }
        }

        public String Username
        {
            get
            {
                return _username;
            }
        }

        public String Password
        {
            get
            {
                return _password;
            }
        }

        private void btn_ok_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                btn_ok_Click(sender, e);
            }
        }

        private void btn_cancle_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void FormLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                btn_ok_Click(sender, e);
            }
        }

    }
}
