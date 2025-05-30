using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpSvn;
using ZLUtils;

namespace ResourceUpdatePack
{
    public class PackerConfig
	{
		// command line args
		public Boolean gui;
		public String uri;
		public AFileCfg afile_cfg;
		public String local_assets;
		public Boolean enable_ingameupdate;
		public String ingameupdate_dir;
		public Boolean force;
		public Boolean latest;
		public Boolean @base;
        public Boolean head;
		public String output;
        public Boolean originVersionFormat = false;
        public Boolean @delete;
        public Boolean debug_inc;
		public Boolean commit_history;
		public Boolean auto_tag_all_important = true;
		public Int32 auto_tag_important_size = -1;
		public Int32 auto_tag_important_filecount = -1;
		public Boolean auto_tag_normal = false;
        public String compareUri;
        public Boolean disable_compare = false;
        public Boolean compare_enable_ingameupdate;
        public String compare_ingameupdate_dir;
        public String comparePrefix = "switch";
		public String compare_local_assets;
        public String username;
        public String password;
        public String res_base_version_xml;
        public Int32 res_base_version_value = -1;

        public Boolean @normal_op;
        public Boolean @important_op;
        public Boolean @listV;
        public Int32 appendV;

		public Boolean earlydownload_op;
		public Boolean statusV;
		public Boolean first_revision;
		public Boolean startV;
		public Boolean start_bugfixV;
		public Boolean stopV;

        public Boolean difflist;
        public Int32 fromV;
        public Int32 toV;

        public Boolean @packlist;

        public String[] languages;
		public Boolean baseHasLanguage = false;	//	多语言时选项，表明基础资源目录是否包含默认语言资源，影响基础语言更新包内删除记录的生成，尤其影响小包机制下将多语言资源变为非多语言资源并移到小包的情形
		public static String baseLanguage = "base";

		//conf.txt
		public String project = "project";
		public String prefix = "prefix";
		public String extension = "extension";
		public Int32 step = 0;

		//tags.txt and history.txt
		/// <summary>
		/// 普通版本标识
		/// </summary>
		public Int32[] normalList;
		/// <summary>
		/// 关键版本标识
		/// </summary>
		public Int32[] importantList;
		/// <summary>
		/// 历史更新包版本记录
		/// </summary>
		public Int32[] historyList;
        /// <summary>
        /// 预下载包起始版本记录
        /// </summary>
        public Int32[] earlydownloadList;
        /// <summary>
        /// 预下载紧急更新包起始版本记录
        /// </summary>
        public Int32[] earlydownloadBugfixList;
        /// <summary>
        /// 预下载历史更新包版本记录(记录参与过打包的预下载区间的版本，用于固定各预下载包区间的版本号。预下载内容转换成正式版本后再打包能维持预下载相关的版本号，从而能用上提前下载的预下载包)
        /// </summary>
        public Int32[] earlydownloadHistoryList;

        /// <summary>
        /// 要忽略的文件版本
        /// </summary>
        public Dictionary<String, Int32[]> ignoredDict;

        /// <summary>
        /// assets文件夹名称
        /// </summary>
        public String assetsDirName = "assets";

		public const String configDirName = "config";
        public const String earlydownloadTagsFile = "earlydownload_tags.txt";
        public const String earlydownloadHistoryFile = "earlydownload_history.txt";

		private String tempDir;
		public String configSvnWorkDir { get { return tempDir; } }

		public String assetsUri { get { return uri + "/" + assetsDirName; } }
        public string compareAssetsUri
        {
            get { return disable_compare || compareUri == null ? null : compareUri + "/" + assetsDirName; }
        }
        public string compareConfigUri
        {
            get { return disable_compare || compareUri == null ? null : compareUri + "/" + configDirName; }
        }
        
        public bool hasLanguage
        {
            get
            {
                return languages != null;
            }
        }
        public int languageCount
        {
            get
            {
                if (languages != null)
                {
                    return languages.Length;
                }
                return 0;
            }
        }
        public bool hasFullLanguageList
        {
            get
            {
                return languageCount > 1;
            }
        }
        public bool IsInFullLanguageList(string lang)
        {
            return hasFullLanguageList && Array.IndexOf(languages, lang) >= 0;
        }
        public string LanguageAt(int index)
        {
            if (index >= 0 && index < languageCount)
            {
                return languages[index];
            }
            else
            {
                return "";
            }
        }
        public static bool IsLanguageBase(string lang)
        {
            return (lang != null) && lang.Equals(baseLanguage);
        }
        public static bool IsLanguageOthers(string lang)
        {
            return (lang != null) && (lang.Length > 0) && !lang.Equals(baseLanguage);
        }

		public static PackerConfig ParseConfig(string[] args)
		{
			PackerConfig config = new PackerConfig();

			Option option = new Option(args);
			{
				Boolean gui;
				if (option.TryGetValue("gui", out gui))
					config.gui = gui;
			}
			{
				String uri;
				if (option.TryGetValue("uri", out uri))
				{
					config.uri = uri;
				}
				else
				{
					throw new Exception("uri参数未设置");
				}
			}
			{
				String afile_cfg;
				if (option.TryGetValue("afile-cfg", out afile_cfg))
				{
					config.afile_cfg = AFileCfg.LoadFromXmlFile(afile_cfg);
				}
			}
			{
				String local_assets;
				if (option.TryGetValue("local-assets", out local_assets))
				{
					config.local_assets = local_assets;
				}
			}
			{
				String ingameupdate_dir;
				if (option.TryGetValue("ingameupdate-dir", out ingameupdate_dir))
				{
					config.ingameupdate_dir = ingameupdate_dir;
					if (!String.IsNullOrEmpty(ingameupdate_dir))
						config.enable_ingameupdate = true;
				}
			}
			{
				Boolean force;
				if (option.TryGetValue("force", out force))
					config.force = force;
			}
            {
				Boolean disable_compare;
				if (option.TryGetValue("disable_compare", out disable_compare))
					config.disable_compare = disable_compare;
			}
			{
				Boolean latest;
				if (option.TryGetValue("latest", out latest))
					config.latest = latest;
			}
			{
				Boolean @base;
				if (option.TryGetValue("base", out @base))
					config.@base = @base;
			}
            {
                Boolean head;
                if (option.TryGetValue("head", out head))
                    config.head = head;
            }
			{
				String output;
				if (option.TryGetValue("output", out output))
					config.output = output;
			}
            {
                Boolean originVersionFormat;
                if (option.TryGetValue("origin-version-format", out originVersionFormat))
                    config.originVersionFormat = originVersionFormat;
            }
            {
				Boolean @delete;
				if (option.TryGetValue("delete", out @delete))
					config.@delete = @delete;
			}
            {
                Boolean debug_inc;
                if (option.TryGetValue("debug-inc", out debug_inc))
                    config.debug_inc = debug_inc;
            }
			{
				Boolean commit_history;
				if (option.TryGetValue("commit-history", out commit_history))
					config.commit_history = commit_history;
			}
			{
				Boolean auto_tag_all_important;
				if (option.TryGetValue("auto-tag-all-important", out auto_tag_all_important))
					config.auto_tag_all_important = auto_tag_all_important;
			}
			{
				Int32 auto_tag_important_size;
				if (option.TryGetValue("auto-tag-important-size", out auto_tag_important_size))
					config.auto_tag_important_size = auto_tag_important_size;
			}
			{
				Int32 auto_tag_important_filecount;
				if (option.TryGetValue("auto-tag-important-filecount", out auto_tag_important_filecount))
					config.auto_tag_important_filecount = auto_tag_important_filecount;
			}
			{
				Boolean auto_tag_normal;
				if (option.TryGetValue("auto-tag-normal", out auto_tag_normal))
					config.auto_tag_normal = auto_tag_normal;
			}
            {
                String username;
                if (option.TryGetValue("username", out username))
                    config.username = username;
            }
            {
                String password;
                if (option.TryGetValue("password", out password))
                    config.password = password;
            }
            {
                String res_base_version_xml;
                if (option.TryGetValue("res_base_version_xml", out res_base_version_xml))
                    config.res_base_version_xml = res_base_version_xml;
            }
            {
                Int32 res_base_version_value;
                if (option.TryGetValue("res_base_version_value", out res_base_version_value))
                    config.res_base_version_value = res_base_version_value;
            }

            {
                Boolean @normal_op;
                if (option.TryGetValue("normal", out @normal_op))
                    config.@normal_op = @normal_op;

                Boolean @important_op;
                if (option.TryGetValue("important", out @important_op))
                    config.@important_op = @important_op;

                Boolean @listV;
                if (option.TryGetValue("list", out @listV))
                    config.@listV = @listV;

                Int32 appendV;
                if (option.TryGetValue("append", out appendV))
                    config.appendV = appendV;
            }
			{
                Boolean earlydownload_op;
                if (option.TryGetValue("earlydownload", out earlydownload_op))
                    config.earlydownload_op = earlydownload_op;

                Boolean statusV;
                if (option.TryGetValue("status", out statusV))
                    config.statusV = statusV;

                Boolean first_revision;
                if (option.TryGetValue("first-revision", out first_revision))
                    config.first_revision = first_revision;

				Boolean startV;
				if (option.TryGetValue("start", out startV))
					config.startV = startV;

				Boolean start_bugfixV;
				if (option.TryGetValue("start-bugfix", out start_bugfixV))
					config.start_bugfixV = start_bugfixV;

				Boolean stopV;
				if (option.TryGetValue("stop", out stopV))
					config.stopV = stopV;
			}
            {
                String strList;
                if(option.TryGetValue("diff", out strList))
                {
                    if(!String.IsNullOrEmpty(strList))
                    {
                        String[] diffList = strList.Split(new char[] { ':' });
                        if(diffList.Length >0)
                        {
                            String strFrom = diffList[0];
                            String strTo = null;
                            if (diffList.Length > 1)
                            {
                                strTo = diffList[1];
                            }

                            Int32 vFrom = -1;
                            Int32 vTo = -1;
                            int.TryParse(strFrom, out vFrom);
                            if (!string.IsNullOrEmpty(strTo))
                                int.TryParse(strTo, out vTo);
                            if(vFrom != -1)
                            {
                                config.difflist = true;
                                config.fromV = vFrom;
                                config.toV = vTo;
                            }
                        }
                    }
                }
            }

            Boolean @packlist;
            if (option.TryGetValue("packlist", out @packlist))
                config.@packlist = @packlist;

            {
                String languages;
                if (option.TryGetValue("languages", out languages))
                    config.languages = languages.Split(',').ToArray();
                if (config.languages != null)
                {
                    if (config.languages.Length == 0 || config.languages.Length == 1 && config.languages[0].Length == 0)
                        throw new Exception("languages 参数为空");
                    foreach (string lang in config.languages)
                    {
                        if (lang != null && lang.Length == 0)
                            throw new Exception("languages 参数内有空项");
                    }
                    for (int i = 0; i < config.languages.Length; ++ i)
                    {
                        string lang = config.languages[i];
                        for (int j = i+1; j < config.languages.Length; ++ j)
                        {
                            string otherLang = config.languages[j];
                            if (otherLang.Equals(lang, StringComparison.OrdinalIgnoreCase))
                                throw new Exception(string.Format("languages 参数有重复项 {0}", lang));
                        }
                    }
                    if (config.languages.Length == 1 && config.languages[0].Equals(baseLanguage))
                        throw new Exception("languages 参数没有指定有效的其它语言");

                    //  补充 base 作为多语言情况下的基础包、并且排序放到第一位
                    List<string> languagesInOrder = new List<string>();
                    bool withBaseLanguage = false;
                    foreach(string lang in config.languages)
                    {
                        if (lang.Equals(baseLanguage))
                        {
                            languagesInOrder.Insert(0, lang);
                            withBaseLanguage = true;
                        }
                        else
                        {
                            languagesInOrder.Add(lang);
                        }
                    }
                    if (!withBaseLanguage)
                    {
                        languagesInOrder.Insert(0, string.Copy(baseLanguage));
                    }
                    config.languages = languagesInOrder.ToArray();

					Boolean baseHasLanguage;
					if (option.TryGetValue("base-has-language", out baseHasLanguage))
						config.baseHasLanguage = baseHasLanguage;
				}
            }

            config.CheckoutConfig();
            config.LoadIgnore();
			config.LoadTagsAndHistory();
            config.LoadEarlydownloadTags();
            config.LoadEarlydownloadHistory();

			string confFilePath = string.Format("{0}/conf.txt", config.configSvnWorkDir);
			if (File.Exists(confFilePath))
			{
				String confContent = Encoding.UTF8.GetString(File.ReadAllBytes(confFilePath));
				Option confOption = Option.FromConfig(confContent);

				{
					String project;
					if (confOption.TryGetValue("project", out project))
						config.project = project;
				}

				{
					String prefix;
					if (confOption.TryGetValue("prefix", out prefix))
						config.prefix = prefix;
				}

				{
					String extension;
					if (confOption.TryGetValue("extension", out extension))
						config.extension = extension;
				}

				{
					Int32 step;
					if (confOption.TryGetValue("step", out step))
						config.step = step;
				}

                {
                    String assets;
                    if (confOption.TryGetValue("assets", out assets))
                        config.assetsDirName = assets;
                }
                {
                    String compareUri;
                    if (confOption.TryGetValue("compare-uri", out compareUri))
                    {
                        if (!String.IsNullOrEmpty(compareUri))
                            config.compareUri = compareUri;
                    }
                }
                if (config.compareUri != null)
                {
                    String compare_ingameupdate_dir;
                    if (confOption.TryGetValue("compare-ingameupdate-dir", out compare_ingameupdate_dir))
                    {
                        config.compare_ingameupdate_dir = compare_ingameupdate_dir;
                        if (!String.IsNullOrEmpty(compare_ingameupdate_dir))
                            config.compare_enable_ingameupdate = true;
                    }
                }
                {
                    String comparePrefix;
                    if (confOption.TryGetValue("compare-prefix", out comparePrefix))
                        config.comparePrefix = comparePrefix;
                }
				{
					String compare_local_assets;
					if (option.TryGetValue("compare-local-assets", out compare_local_assets))
					{
						config.compare_local_assets = compare_local_assets;
					}
				}
			}

            return config;
		}

		private void CheckoutConfig()
		{
			int pid = Process.GetCurrentProcess().Id;
			string localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string exeName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
			tempDir = string.Format("{0}/{1}/{2}", localPath, exeName, pid);

			if (Directory.Exists(tempDir))
			{
				DeleteDiretory(new DirectoryInfo(tempDir));
			}

			SvnClient client = new SvnClient();
            client.Authentication.UserNamePasswordHandlers += (sender, e) =>
			{
				e.UserName = this.username;
				e.Password = this.password;
			};

			SvnUriTarget configUriTarget = new SvnUriTarget(string.Format("{0}/{1}", uri, PackerConfig.configDirName));
			client.CheckOut(configUriTarget, configSvnWorkDir);
			client.Dispose();
		}

		private static void DeleteDiretory(FileSystemInfo fileSystemInfo)
		{
			var directoryInfo = fileSystemInfo as DirectoryInfo;
			if (directoryInfo != null)
			{
				foreach (FileSystemInfo childInfo in directoryInfo.GetFileSystemInfos())
				{
					DeleteDiretory(childInfo);
				}
			}
			fileSystemInfo.Attributes = FileAttributes.Normal;
			fileSystemInfo.Delete();
		}

		public void DeleteTempDir()
		{
			try
			{
				if (!String.IsNullOrEmpty(tempDir))
				{
					if (Directory.Exists(tempDir))
						DeleteDiretory(new DirectoryInfo(tempDir));
				}
			}
			catch (IOException e)
			{
				ConsoleUtil.LogException(e);
			}
		}

        public static void ParseTags(string tagsFilePath, out Int32[] outNormalList, out Int32[] outImportantList)
        {
            List<Int32> normalList = new List<int>();
            List<Int32> importantList = new List<int>();

            String tagsContent = String.Empty;
            if (File.Exists(tagsFilePath))
            {
                tagsContent = Encoding.UTF8.GetString(File.ReadAllBytes(tagsFilePath));
            }
            if (!String.IsNullOrEmpty(tagsContent))
            {
                StringReader reader = new StringReader(tagsContent);
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;

                    line = line.Trim();
                    if (line.Length == 0 || line[0] == '#')
                        continue;

                    string[] elements = line.Split(' ');
                    if (elements.Length < 2)
                        throw new Exception("bad tag line: " + line);

                    int version = int.Parse(elements[0].Trim());
                    normalList.Add(version);

                    if (elements[1].Trim() != "normal")
                    {
                        importantList.Add(version);
                    }
                }
            }

            outNormalList = normalList.Distinct().OrderBy(i => i).ToArray();
            outImportantList = importantList.Distinct().OrderBy(i => i).ToArray();
        }

        public static void ParseHistory(String historyFilePath, out Int32[] outHistoryList)
        {
            List<Int32> historyList = new List<int>();

            String historyContent = String.Empty;
            if (File.Exists(historyFilePath))
            {
                historyContent = Encoding.UTF8.GetString(File.ReadAllBytes(historyFilePath));
            }
            if (!String.IsNullOrEmpty(historyContent))
            {
                StringReader reader = new StringReader(historyContent);
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;

                    line = line.Trim();
                    if (line.Length == 0 || line[0] == '#')
                        continue;

                    int version = int.Parse(line);
                    historyList.Add(version);
                }
            }

            outHistoryList = historyList.Distinct().OrderBy(i => i).ToArray();
        }

        public void LoadTagsAndHistory()
		{
			{
				string tagsFilePath = string.Format("{0}/tags.txt", configSvnWorkDir);
                ParseTags(tagsFilePath, out this.normalList, out this.importantList);
			}
			{
				string historyFilePath = string.Format("{0}/history.txt", configSvnWorkDir);
                ParseHistory(historyFilePath, out this.historyList);
			}
		}

        public void LoadEarlydownloadTags()
        {
            string filePath = string.Format("{0}/{1}", configSvnWorkDir, earlydownloadTagsFile);
            ParseEarlydownloadTags(filePath, out earlydownloadList, out earlydownloadBugfixList);
        }

        public void LoadEarlydownloadHistory()
        {
            string historyFilePath = string.Format("{0}/{1}", configSvnWorkDir, earlydownloadHistoryFile);
            ParseHistory(historyFilePath, out this.earlydownloadHistoryList);
        }

        public static void ParseEarlydownloadTags(string earlydownloadFilePath, out int[] outEarlydownloadList, out int[] outEarlydownloadBugfixList)
        {
            List<int> earlydownload = new List<int>();
            List<int> earlydownloadBugfix = new List<int>();

            String fileContent = String.Empty;
            if (File.Exists(earlydownloadFilePath))
            {
                fileContent = Encoding.UTF8.GetString(File.ReadAllBytes(earlydownloadFilePath));
            }
            if (!String.IsNullOrEmpty(fileContent))
            {
                StringReader reader = new StringReader(fileContent);
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;

                    line = line.Trim();
                    if (line.Length == 0 || line[0] == '#')
                        continue;

                    string[] elements = line.Split(' ');
                    if (elements.Length < 2)
                        throw new Exception("bad earlydownload line: " + line);

                    int version = int.Parse(elements[0].Trim());
                    string tag = elements[1].Trim();
                    if (tag == "earlydownload")
                    {
                        earlydownload.Add(version);
                    }
                    else if (tag == "bugfix")
                    {
                        earlydownloadBugfix.Add(version);
                    }
                    else
                    {
                        throw new Exception("bad earlydownload line: " + line);
                    }
                }
            }

            //  初步验证版本合法性，避免后续未预料的错误
            outEarlydownloadList = earlydownload.Distinct().OrderBy(i => i).ToArray();
            outEarlydownloadBugfixList = earlydownloadBugfix.Distinct().OrderBy(i => i).ToArray();
            ValidateEarlydownloadTags(outEarlydownloadList, outEarlydownloadBugfixList, 0, 0);
        }

        public static void ValidateEarlydownloadTags(int[] earlydownloadList, int[] earlydownloadBugfixList, int startRevision, int headRevision)
        {
            //  验证传入此处的 earlydownload 和 earlydownloadBugfixList 是正数、去重、且有序的
            for (int i = 0; i < earlydownloadList.Count(); ++ i)
            {
                if (earlydownloadList[i] <= 0)
                {
                    throw new Exception(string.Format("预下载包起始版本出现无效值{0}", earlydownloadList[i]));
                }
                if (i > 0)
                {
                    if (earlydownloadList[i] == earlydownloadList[i-1])
                    {
                        throw new Exception(string.Format("预下载包起始版本出现重复值{0}", earlydownloadList[i]));
                    }
                    if (earlydownloadList[i] < earlydownloadList[i-1])
                    {
                        throw new Exception(string.Format("预下载包起始版本不是有序的：第{0}个版本{1}, 第{2}个版本{3}", i-1, earlydownloadList[i-1], i, earlydownloadList[i]));
                    }
                }
            }
            for (int i = 0; i < earlydownloadBugfixList.Count(); ++i)
            {
                if (earlydownloadBugfixList[i] <= 0)
                {
                    throw new Exception(string.Format("紧急更新起始版本出现无效值{0}", earlydownloadBugfixList[i]));
                }
                if (i > 0)
                {
                    if (earlydownloadBugfixList[i] == earlydownloadBugfixList[i - 1])
                    {
                        throw new Exception(string.Format("紧急更新起始版本出现重复值{0}", earlydownloadBugfixList[i]));
                    }
                    if (earlydownloadBugfixList[i] < earlydownloadBugfixList[i - 1])
                    {
                        throw new Exception(string.Format("紧急更新起始版本不是有序的：第{0}个版本{1}, 第{2}个版本{3}", i - 1, earlydownloadBugfixList[i - 1], i, earlydownloadBugfixList[i]));
                    }
                }
            }

            //  验证 earlydownload 和 earlydownloadBugfixList 版本是交替出现的
            {

                if (earlydownloadList.Count() > 0 && earlydownloadBugfixList.Count() == 0)
                {
                    if (earlydownloadList.Count() > 1)
                    {
                        throw new Exception(string.Format("没有紧急更新时、预下载包起始版本只应有一个，当前有{0}个!", earlydownloadList.Count()));
                    }
                }
                else if (earlydownloadList.Count() == 0 && earlydownloadBugfixList.Count() > 0)
                {
                    throw new Exception(string.Format("没有预下载包起始版本、却有{0}个紧急更新起始版本!", earlydownloadBugfixList.Count()));
                }
                else if (earlydownloadList.Count() > 0 && earlydownloadBugfixList.Count() > 0)
                {
                    if (earlydownloadBugfixList.Count() != earlydownloadList.Count() &&
                        earlydownloadBugfixList.Count() != earlydownloadList.Count() - 1)
                    {
                        throw new Exception(string.Format("紧急更新总是跟在预下载包后，因此个数{0}应等于或只比后者个数{1}少1!", earlydownloadBugfixList.Count(), earlydownloadList.Count()));
                    }
                    for (int i = 0; i < earlydownloadBugfixList.Count(); ++i)
                    {
                        if (earlydownloadBugfixList[i] <= earlydownloadList[i])
                        {
                            throw new Exception(string.Format("第{0}个紧急更新起始版本{1}应大于相应的预下载起始版本{2}!", i, earlydownloadBugfixList[i], earlydownloadList[i]));
                        }
                        if (i + 1 < earlydownloadList.Count() && earlydownloadBugfixList[i] >= earlydownloadList[i + 1])
                        {
                            throw new Exception(string.Format("第{0}个紧急更新起始版本{1}应小于后一个预下载起始版本{2}!", i, earlydownloadBugfixList[i], earlydownloadList[i + 1]));
                        }
                    }
                }
            }

            //  检查是否低于起始版本
            if (startRevision > 0)
            {
                if (earlydownloadList.Count() > 0)
                {
                    if (startRevision > earlydownloadList[0])
                    {
                        throw new Exception(string.Format("更新起始版本{0}之前、还有预下载起始版本{1}", startRevision, earlydownloadList[0]));
                    }
                }
                if (earlydownloadBugfixList.Count() > 0)
                {
                    if (startRevision >= earlydownloadBugfixList[0])
                    {
                        throw new Exception(string.Format("更新起始版本{0}及之前，还有紧急更新起始版本{1}", startRevision, earlydownloadBugfixList[0]));
                    }
                }
            }

            //  检查是否超过了最新版本
            if (headRevision > 0)
            {
                for (int i = 0; i < earlydownloadList.Count(); ++ i)
                {
                    if (earlydownloadList[i] > headRevision)
                    {
                        throw new Exception(string.Format("第{0}个预下载起始版本{1}超过了svn最新版本{2}", i, earlydownloadList[i], headRevision));
                    }
                }
                for (int i = 0; i < earlydownloadBugfixList.Count(); ++ i)
                {
                    if (earlydownloadBugfixList[i] > headRevision)
                    {
                        throw new Exception(string.Format("第{0}个紧急更新起始版本{1}超过了svn最新版本{2}", i, earlydownloadBugfixList[i], headRevision));
                    }
                }
            }
        }

        public bool IsEarlydownloadMode()
        {
            return earlydownloadList.Count() == (earlydownloadBugfixList.Count() + 1);
        }

        public bool IsBugfixMode()
        {
            return earlydownloadBugfixList.Count() > 0 && earlydownloadBugfixList.Count() == earlydownloadList.Count();
        }

        public bool IsEarlydownloadPack(int from, int to)
        {
			if (from >= to)
			{
				return false;
			}
            if (IsEarlydownloadMode())
            {
                for (int i = 0; i < earlydownloadList.Count() - 1; ++i)
                {
                    if (from >= earlydownloadList[i] && to <= earlydownloadBugfixList[i])
                    {
                        return true;
                    }
                }
                if (from >= earlydownloadList.Last())
                {
                    return true;
                }
            }
            else if (IsBugfixMode())
            {
                for (int i = 0; i < earlydownloadList.Count(); ++i)
                {
                    if (from >= earlydownloadList[i] && to <= earlydownloadBugfixList[i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

		public bool IsBugfixPack(int from, int to)
		{
			if (from >= to)
			{
				return false;
			}
			if (IsEarlydownloadMode())
			{
				for (int i = 0; i < earlydownloadBugfixList.Count(); ++i)
				{
					if (from >= earlydownloadBugfixList[i] && to <= earlydownloadList[i + 1])
					{
						return true;
					}
				}
			}
			else if (IsBugfixMode())
			{
				for (int i = 0; i < earlydownloadBugfixList.Count() - 1; ++i)
				{
					if (from >= earlydownloadBugfixList[i] && to <= earlydownloadList[i + 1])
					{
						return true;
					}
				}
				if (from >= earlydownloadBugfixList.Last())
				{
					return true;
				}
			}
			return false;
		}

		public bool AddImportant(int version)
		{
			bool Result = false;

			Result = AddNormal(version);

			if (!this.importantList.Contains(version))
			{
				List<int> importantList = this.importantList.ToList();
				importantList.Add(version);
				this.importantList = importantList.ToArray();
				Result = true;
			}

			return Result;
		}

		public bool AddNormal(int version)
		{
			bool Result = false;
			if (!this.normalList.Contains(version))
			{
				List<int> normalList = this.normalList.ToList();
				normalList.Add(version);
				this.normalList = normalList.ToArray();
				Result = true;
			}
			return Result;
		}

		private void SaveTags()
		{
			StringBuilder builder = new StringBuilder();
			foreach (int ver in normalList)
			{
				if (importantList.Contains(ver))
				{
					builder.AppendLine(string.Format("{0} important", ver));
				}
				else
				{
					builder.AppendLine(string.Format("{0} normal", ver));
				}
			}

			String tagsFilePath = Path.Combine(configSvnWorkDir, "tags.txt");
			SaveFile(tagsFilePath, builder.ToString());
		}

		private void SaveHistory()
		{
			{
				StringBuilder builder = new StringBuilder();
				foreach(int ver in historyList) {
					builder.AppendLine(string.Format("{0}", ver));
				}

				String historyFilePath = Path.Combine(configSvnWorkDir, "history.txt");
                SaveFile(historyFilePath, builder.ToString());
			}
		}

        private void SaveEarlydownloadTags()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < earlydownloadList.Length; ++i)
            {
                builder.AppendLine(string.Format("{0} earlydownload", earlydownloadList[i]));
                if (i < earlydownloadBugfixList.Length)
                {
                    builder.AppendLine(string.Format("{0} bugfix", earlydownloadBugfixList[i]));
                }
            }

            String filePath = Path.Combine(configSvnWorkDir, earlydownloadTagsFile);
            SaveFile(filePath, builder.ToString());
        }

        private void SaveEarlydownloadHistory()
        {
            StringBuilder builder = new StringBuilder();
            foreach (int ver in earlydownloadHistoryList)
            {
                builder.AppendLine(string.Format("{0}", ver));
            }

            String filePath = Path.Combine(configSvnWorkDir, earlydownloadHistoryFile);
            SaveFile(filePath, builder.ToString());
        }

        public void SaveAndCommitAll(String comment)
		{
            SaveIgnore();
			SaveTags();
			SaveHistory();
            SaveEarlydownloadTags();
            SaveEarlydownloadHistory();
            Commit(comment, "ignore.txt", "tags.txt", "history.txt", earlydownloadTagsFile, earlydownloadHistoryFile);
		}

		public void SaveAndCommitHistory(String comment)
		{
			SaveHistory();
            SaveEarlydownloadHistory();
            Commit(comment, "history.txt", earlydownloadHistoryFile);
		}

		public void SaveAndCommitTags(String comment)
		{
			SaveTags();
			Commit(comment, "tags.txt");
		}

		public void SaveAndCommitEarlydownloadTags(String comment)
		{
			SaveEarlydownloadTags();
			Commit(comment, earlydownloadTagsFile);
		}

        public void LoadIgnore()
        {
            ignoredDict = new Dictionary<string, int[]>();

            string ignoreFilePath = string.Format("{0}/ignore.txt", configSvnWorkDir);
            if (File.Exists(ignoreFilePath))
            {
                char[] sep = {','};
                string[] lines = File.ReadAllLines(ignoreFilePath);
                foreach(string line in lines)
                {
                    string[] entries = line.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                    if (entries.Length > 1)
                    {
                        string path = entries[0].Trim();
                        List<Int32> revisionList = new List<int>();
                        for (int i = 1; i < entries.Length; ++i)
                        {
                            revisionList.Add(Int32.Parse(entries[i].Trim()));
                        }
                        ignoredDict.Add(path, revisionList.Distinct().ToArray());
                    }
                }
            }
        }

        void SaveFile(string path, string content)
        {
            if(File.Exists(path))
            {
                var attr = File.GetAttributes(path);
                if(attr.HasFlag(FileAttributes.ReadOnly))
                {
                    File.SetAttributes(path, attr & ~FileAttributes.ReadOnly);
                }
            }

            File.WriteAllText(path, content);
        }

        public void SaveIgnore()
        {
            string ignoreFilePath = string.Format("{0}/ignore.txt", configSvnWorkDir);

            StringBuilder builder = new StringBuilder();
            foreach(var ignoredItem in ignoredDict)
            {
                builder.Append(ignoredItem.Key);
                foreach(var revision in ignoredItem.Value)
                    builder.AppendFormat(",{0}", revision);
                builder.AppendLine();
            }

            SaveFile(ignoreFilePath, builder.ToString());
        }

        public void SaveAndCommitIgnore(string comment)
        {
            SaveIgnore();
            Commit(comment, "ignore.txt");
        }

        void Commit(string comment, params string[] files)
        {
            SvnCommitArgs commitArgs = new SvnCommitArgs
            {
                LogMessage = comment,
            };

            SvnClient client = new SvnClient();
			client.Authentication.UserNamePasswordHandlers += (sender, e) =>
			{
				e.UserName = this.username;
				e.Password = this.password;
			};

            SvnAddArgs svnAddArgs = new SvnAddArgs();
            svnAddArgs.Force = true;
            svnAddArgs.NoAutoProps = true;
            if (!client.Add(configSvnWorkDir, svnAddArgs))
                throw new Exception("提交失败");

            List<string> fileList = new List<string>();
            foreach (string file in files)
                fileList.Add(Path.Combine(configSvnWorkDir, file));

            if (!client.Commit(fileList, commitArgs))
                throw new Exception("提交失败");

			client.Dispose();
        }
    }
}
