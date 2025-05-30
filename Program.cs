using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpSvn;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using SevenZip;
using System.Collections.ObjectModel;
using System.Xml;
using ZLUtils;
using System.Reflection;

namespace ResourceUpdatePack
{
	class Program
	{

        static FormLogin login;

        static void Usage()
        {
            ConsoleUtil.Log(@"
usage: {0} --<option>[=<value>] [--<option>[=<value>]] ...
options:
     --gui=[布尔值] 窗口模式
     --uri=[字符串] svn远程路径，必需字段
     --local-assets=[字符串] assets svn工作目录，生成更新包必需字段
     --afile-cfg=[字符串] afile-cfg 配置文件路径，生成更新包必需字段
     --ingameupdate-dir=[字符串] 小包信息目录，相对于 assets 目录，不传时不考虑小包
     --force=[布尔值] 强制生成所有更新包
     --latest=[布尔值] 输出最新版本号
     --base=[布尔值] 输出起始版本号
     --output=[字符串] 更新包输出目录，必需字段
     --delete=[布尔值] 删除无关文件
     --commit-history=[布尔值] 是否提交更新包历史版本信息
     --auto-tag-all-important=[bool] 将所有版本都标记为关键版本，以达到关闭关键版本、但保留此机制的目的，开启此选项，其它 --auto-tag 开头的参数将忽略
     --auto-tag-normal=[bool] 将最新版本自动标记为普通版本
     --auto-tag-important-size=[数字] 为正值时(单位为MB)，若到最新版本的更新包有大小超过此值，则将最新版本标记自动标记为关键版本并提交
     --auto-tag-important-filecount=[数字] 为正值时，若到最新版本的更新包内增加或修改的文件数超过此值，则将最新版本标记自动标记为关键版本并提交
     --username=[字符串] svn账号
     --password=[字符串] svn密码
     --res_base_version_xml=[字符串] res_base_version.xml文件路径
     --res_base_version_value=[数字] res_base_version.xml文件中version值，只有在设置时才需要指定
     --normal=[bool] normal版本操作
     --important=[bool] Important版本操作
     --earlydownload=[bool] 预下载版本操作，依赖子选项
     --status=[bool] 预下载操作子选项，以字符串形式返回当前预下载状态
     --first-revision=[bool] 预下载操作子选项，返回首个预下载状态，非预下载状态返回-1
     --start=[bool] 预下载操作子选项，进入预下载模式
     --start-bugfix=[bool] 预下载操作子选项，进入紧急更新模式
     --stop=[bool] 预下载操作子选项，退出预下载或紧急更新模式
     --list=[bool] 显示版本列表
     --append=[数字] 添加版本
     --diff=from:to 差异列表
     --packlist=[bool] 更新资源包
     --languages=[逗号分隔的多语言完整语言名称列表]:基础更新包按特殊情况名称约定为 base，生成在 output/ 目录；其它语言对应 L10N 下的子目录名称，更新包生成在 output/[language]/ 目录下; version 信息将合并写入 output/version.txt 中;
     --base-has-language=[布尔值] 多语言时选项，表明基础资源目录是否包含默认语言资源，影响基础语言更新包内删除记录的生成，尤其影响小包机制下将多语言资源变为非多语言资源并移到小包的情形

[布尔值]为true或者false，也可以省略值，此时值为true

example:
    {0} --gui --uri=http://example.com/svn/resource
", App.ModuleName);
        }

        [STAThread]
		static void Main(string[] args)
		{
            //增加一个登陆方式
            bool hasLoging = false;
            if (args.Length == 0)
            {
                if((hasLoging = Login()))
                {
                    List<string> newargs = new List<string>();
                    newargs.Add("--gui=true");
                    newargs.Add("--uri=" + login.SvnUri);
                    newargs.Add("--output=resource_update_cache");
                    newargs.Add("--delete=false");
                    if (!String.IsNullOrEmpty(login.Username))
                        newargs.Add("--username=" + login.Username);
                    if (!String.IsNullOrEmpty(login.Username))
                        newargs.Add("--password=" + login.Password);

                    args = newargs.ToArray();
                }
            }

			//Console.OutputEncoding = Encoding.UTF8;
            if (args.Length == 0)
			{
                Usage();
				return;
			}

			PackerConfig packerConfig = null;
            try
            {
				packerConfig = PackerConfig.ParseConfig(args);
				Run(packerConfig);
            }
            catch (Exception e)
            {
                ConsoleUtil.LogException(e);
                App.Exit(-1);
            }
            finally
            {
                if (packerConfig != null)
                {
                    packerConfig.DeleteTempDir();
                }
            }
		}


        static bool Login()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            login = new FormLogin();
            DialogResult dr = login.ShowDialog();
            if(dr == DialogResult.OK)
            {
                return true;
            }
            return false;
        }

		static void Run(PackerConfig packerConfig)
		{
			if (packerConfig.latest)
			{
				int headRevision = -1;
				if (GetAssetHeadRevision(packerConfig, out headRevision))
                    Console.WriteLine(headRevision);
				return;
			}

			if (packerConfig.@base)
			{
				if (packerConfig.normalList.Length == 0)	//未设置任何版本标记时，从版本 1 开始制作更新包
				{
					Console.WriteLine("1");
					return;
				}
				int baseVersion = packerConfig.normalList[0];
				Console.WriteLine(baseVersion);
				return;
			}

            if (packerConfig.head)
            {
				if (packerConfig.normalList.Length == 0)	//未设置任何版本标记时，从版本 1 开始制作更新包
				{
					Console.WriteLine("1");
					return;
				}
                int headVersion = packerConfig.normalList[packerConfig.normalList.Length - 1];
                Console.WriteLine(headVersion);
                return;
            }

            if (packerConfig.res_base_version_xml != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(packerConfig.res_base_version_xml);
                if (packerConfig.res_base_version_value > 0)
                {
                    doc.DocumentElement.SetAttribute("value", packerConfig.res_base_version_value.ToString());
                    doc.Save(packerConfig.res_base_version_xml);
                }
                else
                {
                    Console.WriteLine(doc.DocumentElement.GetAttribute("value"));
                }
                return;
            }

            if(packerConfig.@normal_op)
            {
                var normalList = packerConfig.normalList;
                if(packerConfig.@listV)
                {
                    if(normalList == null || normalList.Length == 0)
                    {
                        ConsoleUtil.LogWarning(string.Format("normalList is empty"));
                        return;
                    }
                    StringBuilder sb = new StringBuilder();
                    for(int i=0;i<normalList.Length; ++i)
                    {
                        sb.Append(Convert.ToString(normalList[i]));
                        if (i < normalList.Length-1) sb.Append(',');
                    }
                    Console.WriteLine(sb.ToString());
                    return;
                }
                else if(packerConfig.appendV >0)
                {
                    List<int> vList = new List<int>();
                    if (normalList != null)
                    {
                        vList.AddRange(normalList);
                    }
                    if(vList.Contains(packerConfig.appendV))
                    {
                        ConsoleUtil.LogWarning(string.Format("normal:{0} is already exists.", packerConfig.appendV));
                        return;
                    }
                    vList.Add(packerConfig.appendV);
                    vList.Sort();
                    //
                    packerConfig.normalList = vList.ToArray();
                    packerConfig.SaveAndCommitAll(string.Format("append normal version {0}", packerConfig.appendV));
                    return;
                }
                else
                {
                    Usage();
                    return;
                }
            }

            if (packerConfig.@important_op)
            {
                var importantList = packerConfig.importantList;
                var normalList = packerConfig.normalList;
                if (packerConfig.@listV)
                {
                    if (importantList == null || importantList.Length == 0)
                    {
                        ConsoleUtil.LogWarning(string.Format("importantList is empty"));
                        return;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < importantList.Length; ++i)
                    {
                        sb.Append(Convert.ToString(importantList[i]));
                        if (i < importantList.Length - 1) sb.Append(',');
                    }
                    Console.WriteLine(sb.ToString());
                    return;
                }
                else if (packerConfig.appendV > 0)
                {
                    List<int> vList = new List<int>();
                    if (importantList != null)
                    {
                        vList.AddRange(importantList);
                    }
                    if (vList.Contains(packerConfig.appendV))
                    {
                        ConsoleUtil.LogWarning(string.Format("important:{0} is already exists.", packerConfig.appendV));
                        return;
                    }
                    vList.Add(packerConfig.appendV);
                    vList.Sort();
                    //
                    packerConfig.importantList = vList.ToArray();

                    List<int> vNormalList = new List<int>();
                    if (normalList != null)
                    {
                        vNormalList.AddRange(normalList);
                    }
                    if (vNormalList.Contains(packerConfig.appendV))
                    {
                        ConsoleUtil.LogWarning(string.Format("normal:{0} is already exists.", packerConfig.appendV));
                        return;
                    }
                    vNormalList.Add(packerConfig.appendV);
                    vNormalList.Sort();

                    packerConfig.normalList = vNormalList.ToArray();

                    packerConfig.SaveAndCommitAll(string.Format("append important version {0}", packerConfig.appendV));
                    return;
                }
                else
                {
                    Usage();
                    return;
                }
            }

            if (packerConfig.earlydownload_op)
			{
				int headRevision = -1;
				if (!GetAssetHeadRevision(packerConfig, out headRevision))
				{
					throw new Exception(string.Format("无法获取资源最新版本"));
				}
				int startRevision = 0;
				if (packerConfig.normalList.Count() > 0)
				{
					startRevision = packerConfig.normalList[0];
				}
				// 预防配置错误(比如从其它分支直接拷贝过来)，配置正确的情况下，其它操作才有意义
				PackerConfig.ValidateEarlydownloadTags(packerConfig.earlydownloadList, packerConfig.earlydownloadBugfixList, startRevision, headRevision);

				//	输出当前的预下载状态
				if (packerConfig.statusV)
				{
					string mode;
					if (packerConfig.IsEarlydownloadMode())
					{
						mode = "earlydownload";
					}
					else if (packerConfig.IsBugfixMode())
					{
						mode = "bugfix";
					}
					else
					{
						mode = "none";
					}
					Console.WriteLine(mode);
					return;
				}

				//	输出指定版本所处的预下载状态
				if (packerConfig.first_revision)
				{
					int firstRevision = packerConfig.earlydownloadList.Count() > 0 ? packerConfig.earlydownloadList.First() : -1;
					Console.WriteLine(string.Format("first-revision:[{0}]", firstRevision));
					return;
				}

				//	输出所有预下载、紧急更新版本范围
				if (packerConfig.listV)
				{
					ListEarlydownload(packerConfig);
					return;
				}

				//	尝试进入预下载状态
				if (packerConfig.startV)
				{
					if (packerConfig.IsEarlydownloadMode())
					{
						if (packerConfig.earlydownloadList.Last() == headRevision)
						{
							Console.WriteLine(string.Format("当前已经是预下载状态，起始版本号为{0}，最新版本即起始版本", packerConfig.earlydownloadList.Last()));
						}
						else
						{
							throw new Exception(string.Format("当前已经是预下载状态，起始版本号为{0}，最新版本为{1}", packerConfig.earlydownloadList.Last(), headRevision));
						}
					}
					else if (packerConfig.IsBugfixMode())
					{
						if (packerConfig.earlydownloadBugfixList.Last() == headRevision)
						{
							ConsoleUtil.Log(string.Format("bugfix->earlydownload: 当前还是紧急更新'准备'状态，可直接撤销，撤销后最新的预下载起始版本为{0}，最新版本为{1}", packerConfig.earlydownloadList.Last(), headRevision));
							List<int> earlydownloadBugfixList = packerConfig.earlydownloadBugfixList.ToList();
							earlydownloadBugfixList.RemoveAt(earlydownloadBugfixList.Count() - 1);
							packerConfig.earlydownloadBugfixList = earlydownloadBugfixList.ToArray();
							packerConfig.SaveAndCommitEarlydownloadTags(string.Format("bugfix->earlydownload at {0}", headRevision));
						}
						else
						{
							ConsoleUtil.Log(string.Format("bugfix->earlydownload: 进入预下载'准备'状态，起始版本为{0}", headRevision));
							List<int> earlydownloadList = packerConfig.earlydownloadList.ToList();
							earlydownloadList.Add(headRevision);
							packerConfig.earlydownloadList = earlydownloadList.ToArray();
							packerConfig.SaveAndCommitEarlydownloadTags(string.Format("bugfix->earlydownload at {0}", headRevision));
						}
					}
					else
					{
						ConsoleUtil.Log(string.Format("none->earlydownload: 进入预下载'准备'状态，起始版本为{0}", headRevision));
						List<int> earlydownloadList = new List<int>();
						earlydownloadList.Add(headRevision);
						packerConfig.earlydownloadList = earlydownloadList.ToArray();
						packerConfig.SaveAndCommitEarlydownloadTags(string.Format("none->earlydownload at {0}", headRevision));
					}
					ListEarlydownload(packerConfig);
					return;
				}

				//	尝试进入紧急更新状态
				if (packerConfig.start_bugfixV)
				{
					if (packerConfig.IsBugfixMode())
					{
						if (packerConfig.earlydownloadBugfixList.Last() == headRevision)
						{
							Console.WriteLine(string.Format("当前已经是紧急更新状态，起始版本为{0}，最新版本即起始版本", packerConfig.earlydownloadBugfixList.Last()));
						}
						else
						{
							throw new Exception(string.Format("当前已经是紧急更新状态，起始版本为{0}，最新版本为{1}", packerConfig.earlydownloadBugfixList.Last(), headRevision));
						}
					}
					else if (!packerConfig.IsEarlydownloadMode())
					{
						throw new Exception(string.Format("当前不是预下载状态，只能从预下载状态进入紧急更新状态。最新版本为{0}", headRevision));
					}
					else
					{
						if (packerConfig.earlydownloadList.Last() == headRevision)
						{
							if (packerConfig.earlydownloadList.Count() == 1)
							{
								throw new Exception(string.Format("当前还是'首次'预下载'准备'状态，直接撤销预下载状态恢复到正常更新即可。最新版本为{0}", headRevision));
							}
							else
							{
								ConsoleUtil.Log(string.Format("earlydownload->bugfix: 当前还是预下载'准备'状态，可直接撤销，撤销后最新的紧急更新起始版本为{0}，最新版本为{1}", packerConfig.earlydownloadBugfixList.Last(), headRevision));
								List<int> earlydownloadList = packerConfig.earlydownloadList.ToList();
								earlydownloadList.RemoveAt(earlydownloadList.Count() - 1);
								packerConfig.earlydownloadList = earlydownloadList.ToArray();
								packerConfig.SaveAndCommitEarlydownloadTags(string.Format("earlydownload->bugfix at {0}", headRevision));
							}
						}
						else
						{
							ConsoleUtil.Log(string.Format("earlydownload->bugfix: 进入紧急更新'准备'状态，起始版本为{0}", headRevision));
							List<int> earlydownloadBugfixList = packerConfig.earlydownloadBugfixList.ToList();
							earlydownloadBugfixList.Add(headRevision);
							packerConfig.earlydownloadBugfixList = earlydownloadBugfixList.ToArray();
							packerConfig.SaveAndCommitEarlydownloadTags(string.Format("earlydownload->bugfix at {0}", headRevision));
						}
					}
					ListEarlydownload(packerConfig);
					return;
				}

				//	尝试将所有预下载、紧急更新变成正常更新
				if (packerConfig.stopV)
				{
					if (packerConfig.IsEarlydownloadMode())
					{
						ConsoleUtil.Log(string.Format("earlydownload->none: 从预下载状态转为正常更新，预下载首个起始版本为{0}，预下载最新起始版本为{1}。最新版本为{2}", packerConfig.earlydownloadList.First(), packerConfig.earlydownloadList.Last(), headRevision));
						Int32[] emptyList = new List<int>().ToArray();
						packerConfig.earlydownloadList = emptyList;
						packerConfig.earlydownloadBugfixList = emptyList;
						packerConfig.SaveAndCommitEarlydownloadTags(string.Format("earlydownload->none at {0}", headRevision));
					}
					else if (packerConfig.IsBugfixMode())
					{
						ConsoleUtil.Log(string.Format("bugfix->none: 从紧急更新状态转为正常更新，预下载首个起始版本为{0},紧急更新最新起始版本为{1}。最新版本为{2}", packerConfig.earlydownloadList.First(), packerConfig.earlydownloadBugfixList.Last(), headRevision));
						Int32[] emptyList = new List<int>().ToArray();
						packerConfig.earlydownloadList = emptyList;
						packerConfig.earlydownloadBugfixList = emptyList;
						packerConfig.SaveAndCommitEarlydownloadTags(string.Format("bugfix->none at {0}", headRevision));
					}
					else
					{
						ConsoleUtil.Log(string.Format("当前已经是普通更新状态，最新版本为{0}", headRevision));
					}
					ListEarlydownload(packerConfig);
					return;
				}

				//	参数有误，打印当前的用法
				Usage();
				return;
			}

            if (packerConfig.difflist)
            {
                Packer packer = new Packer(packerConfig);
                packer.OnOutputLog += (message, mode) =>
                {
					if ((mode & LogMode.Console) == LogMode.Console) {
						ConsoleUtil.Log(message);
					}
                };
                packer.CalcDiffList(packerConfig.fromV, packerConfig.toV);
                return;
            }

			SevenZipCompressor.SetLibraryPath(string.Format("{0}/7z64.dll", Application.StartupPath));

			ConsoleUtil.Log(string.Format(@"Version:{0}", GetAppVersion()));

            if (packerConfig.gui)
			{
				Application.Run(new FormVersion(packerConfig));
			}
			else
			{
				Packer packer = new Packer(packerConfig);
                packer.OnOutputLog += (message, mode) =>
                {
					if ((mode & LogMode.Console) == LogMode.Console) {
	                    ConsoleUtil.Log(message);
					}
                };
				List<PackInfo> packList = packer.Pack();
            }
		}

		static bool GetAssetHeadRevision(PackerConfig packerConfig, out int headRevision)
		{
			Collection<SvnLogEventArgs> logList;
			SvnLogArgs logArgs = new SvnLogArgs
			{
				Start = SvnRevision.Head,
				Limit = 1,
			};

			Uri targetUri = new Uri(string.Format("{0}/{1}", packerConfig.uri, packerConfig.assetsDirName));
			SvnClient client = new SvnClient();
			client.Authentication.UserNamePasswordHandlers += (sender, e) =>
			{
				e.UserName = packerConfig.username;
				e.Password = packerConfig.password;
			};

			if (client.GetLog(targetUri, logArgs, out logList))
			{
				headRevision = (int)logList[0].Revision;
				client.Dispose();
				return true;
			}
			else
			{
				headRevision = -1;
				client.Dispose();
				return false;
			}
		}

		static void ListEarlydownload(PackerConfig packerConfig)
		{
			if (packerConfig.IsEarlydownloadMode() || packerConfig.IsBugfixMode())
			{
				Console.WriteLine("预下载当前版本情况:");
				List<int> TempList = new List<int>();
				TempList.AddRange(packerConfig.earlydownloadList);
				TempList.AddRange(packerConfig.earlydownloadBugfixList);
				TempList.Sort();
				for (int i = 0; i < TempList.Count(); ++i)
				{
					if ((i % 2) == 0)
					{
						if (i + 1 >= TempList.Count())
						{
							Console.WriteLine("第{0}个'预下载包'范围:[{1}-]", i / 2 + 1, TempList[i]);
						}
						else
						{
							Console.WriteLine("第{0}个'预下载包'范围:[{1}-{2}]", i / 2 + 1, TempList[i], TempList[i + 1]);
						}
					}
					else
					{
						if (i + 1 >= TempList.Count())
						{
							Console.WriteLine("第{0}个'紧急更新'范围:[{1}-]", i / 2 + 1, TempList[i]);
						}
						else
						{
							Console.WriteLine("第{0}个'紧急更新'范围:[{1}-{2}]", i / 2 + 1, TempList[i], TempList[i + 1]);
						}
					}
				}
			}
			else
			{
				Console.WriteLine("预下载当前版本情况:当前不是预下载或紧急更新状态");
			}
		}

		static string GetAppVersion()
		{
			return Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}
	}
}
