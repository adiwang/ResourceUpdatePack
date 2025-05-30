using SevenZip;
using SharpSvn;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ResourceUpdatePack
{

    public class DiffSummaryItem
    {
        public int Revision { get; set; }
        public string Path { get; set; }
        public SvnDiffKind DiffKind
        {
            get;
            set;
        }
    }

    public class ChangeItem
    {
        string path;
        int revision;
        SvnChangeItem svnChangeItem;

        public ChangeItem(int revision, string path, SvnChangeItem item)
        {
            this.path = path;
            this.revision = revision;
            this.svnChangeItem = item;
        }

        public int Revision
        {
            get { return revision; }
        }

        public string Path
        {
            get { return path; }
        }

        public SvnNodeKind NodeKind
        {
            get { return svnChangeItem.NodeKind; }
        }

        public SvnChangeAction Action
        {
            get { return svnChangeItem.Action; }
        }

        public string CopyFromPath
        {
            get { return svnChangeItem.CopyFromPath; }
        }

        public long CopyFromRevision
        {
            get { return svnChangeItem.CopyFromRevision; }
        }
    }

    public class LogInfo {
        string status = "";
        List<ChangeItem> changeList;
        Dictionary<string, ChangeItem> changeDictionary;

        public int Revision {
            get;
            set;
        }

        public string Author {
            get;
            set;
        }

        public string Message {
            get;
            set;
        }

        public DateTime Time {
            get;
            set;
        }

        public List<ChangeItem> ChangeList
        {
            get
            {
                return changeList;
            }
        }

        public Dictionary<string, ChangeItem> ChangeDictionary
        {
            get { return changeDictionary; }
        }

        public int Count {
            get {
                return changeList.Count;
            }
        }

        public string Status {
            get {
                if(status == "") {
                    int flags = 0;
                    char[] actions = new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
                    foreach(var entry in changeList)
                    {
                        switch(entry.Action)
                        {
                            case SvnChangeAction.Add: actions[0] = 'A'; flags |= 0x01; break;
                            case SvnChangeAction.Modify: actions[2] = 'M'; flags |= 0x02; break;
                            case SvnChangeAction.Delete: actions[4] = 'D'; flags |= 0x04; break;
                            case SvnChangeAction.Replace: actions[6] = 'R'; flags |= 0x08; break;
                        }

                        if (flags == 0x0F)
                            break;
                    }

                    status = new String(actions);
                }
                return status;
            }
        }

        public LogInfo(int capacity)
        {
            this.changeList = new List<ChangeItem>(capacity);
            this.changeDictionary = new Dictionary<string, ChangeItem>(capacity);
        }

        public override string ToString() {
            return string.Format("[{0,6}] {1,-15} {2}", Revision, Author, Message);
        }

        public void AddChangeItem(ChangeItem item)
        {
            this.changeList.Add(item);
            this.changeDictionary.Add(item.Path, item);
        }
    }

    public class PackInfo {
        public string Name {
            get;
            set;
        }

        public string FullName {
            get;
            set;
        }

        public int From {
            get;
            set;
        }

        public int To {
            get;
            set;
        }

        public long Size {
            get;
            set;
        }

		public int FileCount
		{
			get;
			set;
		}

		public bool IsEmpty
		{
			get;
			set;
		}

        public string Hash {
            get;
            set;
        }

        public bool New {
            get;
            set;
        }

        public bool IsBranchDiff {
            get;
            set;
        }

        public bool IsNotDiff { // 多语言包需要某个版本的完整资源，此时不是计算两个分支的 diff；这种情况以 From=0为特殊标记，0为游戏运行时此语言更新目录的初始版本
            get { return From == 0; }
            set { From = 0; }
        }

        public bool IsEarlydownload
        {
            get;
            set;
        }

		public bool IsBugfix
		{
			get;
			set;
		}
    }

    public enum LogMode {
        Gui         = 0x01, // 只在gui模式下输出
        Console     = 0x02, // 只在console模式下输出
        Both        = 0x03, // 两种模式下都输出
    }

    class Packer {

        string assetsUri { get { return config.assetsUri; } }

        string assetsWorkdir { get { return config.local_assets; } }

        bool useOriginVertionFormat { get { return config.originVersionFormat; } }

        string _compareAssetsUri;
        string compareAssetsUri {
            get {
                if (_compareAssetsUri == null && config.compareAssetsUri != null) {
                    _compareAssetsUri = client.GetRepositoryRoot(new Uri(assetsUri)).ToString() + config.compareAssetsUri.TrimStart('/');
                }
                return _compareAssetsUri;
            }
        }
        string compareConfigUri
        {
            get { return config.compareConfigUri == null ? null : client.GetRepositoryRoot(new Uri(assetsUri)).ToString() + config.compareConfigUri.TrimStart('/'); }
        }
		string compareAssetsWorkdir { get { return config.compare_local_assets; } }

		string outputSubPath { get { return (isCurrentLanguageOthers ? ("/res_lang/" + currentLanguage) : "/res_base") + "/patch"; } }
        string output { get { return config.output + outputSubPath; } }
		string outputLangRoot { get { return config.output + "/res_lang"; } }
        string versionPath {  get { return useOriginVertionFormat ? string.Format("{0}/version.txt", config.output + "/res_base/patch") : string.Format("{0}/version.txt", output); } }
        string diffPath {  get { return config.output + "_diff" + outputSubPath;  } }
        string packlistPath { get { return config.output + "_packlist" + outputSubPath + "/packlist.txt"; } }
        string prefix { get { return config.prefix; } }
        string project { get { return config.project; } }
        string extension { get { return config.extension; } }
		int auto_tag_important_size_in_bytes { get { return config.auto_tag_important_size * 1024 * 1024; } }

        bool force { get { return config.force; } }
        bool delete { get { return config.delete; } }
        bool debug_inc { get { return config.debug_inc; } }

        PackerConfig config;

        int _curLanguageIndex = -1;
        int languageCount { get { return config.languageCount; } }
        string LanguageAt(int index)
        {
            return config.LanguageAt(index);
        }
        static bool IsLanguageBase(string lang)
        {
            return PackerConfig.IsLanguageBase(lang);
        }
        static bool IsLanguageOthers(string lang)
        {
            return PackerConfig.IsLanguageOthers(lang);
        }
        string currentLanguage
        {
            get
            {
                return LanguageAt(_curLanguageIndex);
            }
        }
        bool isCurrentLanguageBase
        {
            get
            {
                return IsLanguageBase(currentLanguage);
            }
        }
        bool isCurrentLanguageOthers
        {
            get
            {
                return IsLanguageOthers(currentLanguage);
            }
        }

        bool isEarlydownloadMode
        {
            get
            {
                return config.IsEarlydownloadMode();
            }
        }
        bool isEarlydownloadBugfixMode
        {
            get
            {
                return config.IsBugfixMode();
            }
        }
        bool hasEarlydownload
        {
            get
            {
                return isEarlydownloadMode || isEarlydownloadBugfixMode;
            }
        }

        SvnClient client = new SvnClient();
        List<LogInfo> logList = new List<LogInfo>();
		int compareAssetsSwitchRevision = -1;
		Dictionary<int, bool>[] assetsLangRulesResult = { new Dictionary<int, bool>(), new Dictionary<int, bool>() };
		Dictionary<int, bool> GetLangRulesResult(bool skipLangCountRule)
		{
			return assetsLangRulesResult[skipLangCountRule ? 1 : 0];
		}

        public event Action<string, LogMode> OnOutputLog;

        public Packer(PackerConfig config) {
			this.config = config;
            this.client.Authentication.UserNamePasswordHandlers += (sender, e) =>
            {
                e.UserName = config.username;
                e.Password = config.password;
            };

			CheckAndApplyConfig();
        }

        void OutputLog(string message, LogMode mode = LogMode.Both) {
            if(OnOutputLog != null) {
                OnOutputLog(message, mode);
            }
        }

        public void CheckAndApplyConfig()
		{
			if (String.IsNullOrEmpty(config.output))
                throw new Exception("output参数未设置");
        }

        int GetLatestRevision(string uri)
        {
            SvnLogArgs logArgs = new SvnLogArgs
            {
                Start = SvnRevision.Head,
                End = SvnRevision.One,
                Limit = 1,
            };

            Collection<SvnLogEventArgs> logItems;
            if (!client.GetLog(new Uri(uri), logArgs, out logItems))
            {
                throw new Exception("日志获取错误");
            }

            return (int)logItems[0].Revision;
        }

        bool ContainsRevision(string uri, int revision)
        {
            SvnLogArgs logArgs = new SvnLogArgs
            {
                Start = new SvnRevision(revision),
                End = SvnRevision.One,
                Limit = 1,
                ThrowOnError = false,
            };

            Collection<SvnLogEventArgs> logItems;
            if (!client.GetLog(new Uri(uri), logArgs, out logItems))
            {
                return false;
            }

            return (int)logItems[0].Revision == revision;
        }

        string GetRepositoryRelativePath(string uri)
        {
            SvnTarget target = SvnTarget.FromString(uri);
            SvnInfoEventArgs infoArgs;
            if (!client.GetInfo(target, out infoArgs))
            {
                throw new Exception(string.Format("GetInfo Error: {0}", uri));
            }
            return uri.Substring(infoArgs.RepositoryRoot.ToString().Length);
        }

        void AddLatestRevision(List<int> normalList, List<int> importantList) {
            int startRevision = 0;
            int maxVersion = 0;
            if(normalList.Count > 0) {
                startRevision = normalList[0];
                maxVersion = normalList[normalList.Count - 1];
            }

            List<int> revisionList = new List<int>();
            foreach(LogInfo info in logList) {
                revisionList.Add(info.Revision);
            }

            int headRevision = revisionList[revisionList.Count - 1];
            Earlydownload_EnsureInRange(startRevision, headRevision);

			if (maxVersion < headRevision)
			{
				normalList.Add(headRevision);
			}

			Earlydownload_AddImportantRevision(normalList, startRevision, headRevision);
			Earlydownload_AddImportantRevision(importantList, startRevision, headRevision);

			normalList.RemoveAll((r) => r > headRevision);
            importantList.RemoveAll((r) => r > headRevision);
        }

        void Earlydownload_EnsureInRange(int startRevision, int headRevision)
        {
            if (headRevision <= 0 || startRevision > headRevision)
            {
                throw new Exception(string.Format("Earlydownload_EnsureInRange: invalid startRevision:{0} or headRevision:{1}", startRevision, headRevision));
            }
            if (config.earlydownloadList.Count() > 0)
            {
                if (config.earlydownloadList.First() < startRevision)
                {
                    throw new Exception(string.Format("Earlydownload_EnsureInRange: earlydownloadList.First():{0} < startRevision:{1}", config.earlydownloadList.First(), startRevision));
                }
                if (config.earlydownloadList.Last() > headRevision)
                {
                    throw new Exception(string.Format("Earlydownload_EnsureInRange: earlydownloadList.Last():{0} > headRevision:{1}", config.earlydownloadList.Last(), headRevision));
                }
            }
            if (config.earlydownloadBugfixList.Count() > 0)
            {
                if (config.earlydownloadBugfixList.First() < startRevision)
                {
                    throw new Exception(string.Format("Earlydownload_EnsureInRange: earlydownloadBugfixList.First():{0} < startRevision:{1}", config.earlydownloadBugfixList.First(), startRevision));
                }
                if (config.earlydownloadBugfixList.Last() > headRevision)
                {
                    throw new Exception(string.Format("Earlydownload_EnsureInRange: earlydownloadBugfixList.Last():{0} > headRevision:{1}", config.earlydownloadBugfixList.Last(), headRevision));
                }
            }
        }

        //  预下载打包涉及的历史版本、预下载非紧急更新状态的版本，添加到列表里参与生成更新包
        void Earlydownload_AddImportantRevision(List<int> toList, int startRevision, int headRevision)
        {
            if (startRevision <= 0)
            {
                return; //  没有设置起始版本时、 不会生成正常更新包，此处也忽略，避免产生未预料的结果
            }

            void AddToList(int revision)
            {
                if (!toList.Contains(revision))
                {
                    toList.Add(revision);
                }
            }

            foreach(int revision in config.earlydownloadHistoryList)
            {
                if (revision < startRevision || revision > headRevision)
                {
                    continue;
                }
                AddToList(revision);
            }

            if (isEarlydownloadMode)
            {
                for (int i = 0; i < config.earlydownloadList.Count(); ++i)
                {
                    if (i < config.earlydownloadList.Count() - 1)
                    {
                        AddToList(config.earlydownloadList[i]);
                        AddToList(config.earlydownloadBugfixList[i]);
                    }
                    else
                    {
                        if (config.earlydownloadList[i] < headRevision) //  与 headRevision 相同时，说明此预下载版本还未真正生效，假定此状态还可能会被取消，因此先不添加
                        {
                            AddToList(config.earlydownloadList[i]);
                            AddToList(headRevision);                    //  预下载包的每次版本，都应添加
                        }
                    }
                }
            }
            else if (isEarlydownloadBugfixMode)
            {
                for (int i = 0; i < config.earlydownloadList.Count(); ++i)
                {
                    AddToList(config.earlydownloadList[i]);
                    AddToList(config.earlydownloadBugfixList[i]);
                }
            }

            //  添加完成后必须排序
            toList.Sort();
        }

        //  预下载打包涉及的历史版本、预下载非紧急更新状态的版本，统计其有效的范围
        void Earlydownload_GetRevisionRange(int startRevision, int headRevision, out int from, out int to)
        {
            from = 0;
            to = 0;

            if (config.earlydownloadHistoryList.Count() > 0)
            {
                from = config.earlydownloadHistoryList.FirstOrDefault(revision => revision >= startRevision && revision <= headRevision);
                to = config.earlydownloadHistoryList.LastOrDefault(revision => revision >= startRevision && revision <= headRevision);
            }

            if (isEarlydownloadMode)
            {
                if (from == 0 || from > config.earlydownloadList.First())
                {
                    from = config.earlydownloadList.First();
                }
                if (to == 0 || to < headRevision)
                {
                    to = headRevision;								//	一直到最新版本，都是预下载包范围
                }
            }
            else if (isEarlydownloadBugfixMode)
            {
                if (from == 0 || from > config.earlydownloadList.First())
                {
                    from = config.earlydownloadList.First();
                }
                if (to == 0 || to < config.earlydownloadBugfixList.Last())
                {
                    to = config.earlydownloadBugfixList.Last();     //  预下载包范围只截止到当前的紧急更新版本
                }
            }
        }

        void ExportCompareUriEarlydownloadTags(out int[] outEarlydownloadList, out int[] outEarlydownloadBugfixList)
        {
            if (!ContainsFile(compareConfigUri, PackerConfig.earlydownloadTagsFile))
            {
                outEarlydownloadList = new List<int>().ToArray();
                outEarlydownloadBugfixList = new List<int>().ToArray();
                return;
            }
            String uri = String.Format("{0}/{1}", compareConfigUri, PackerConfig.earlydownloadTagsFile);
            SvnTarget svnTarget = new SvnUriTarget(uri, SvnRevision.Head);
            String tempFilePath = Path.GetTempFileName();
            try
            {
                SvnExportSingleFile(svnTarget, tempFilePath);
                PackerConfig.ParseEarlydownloadTags(tempFilePath, out outEarlydownloadList, out outEarlydownloadBugfixList);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        bool ContainsFile(string fileRootUri, string fileName)
        {
            bool result = false;
            SvnListArgs listArgs = new SvnListArgs
            {
                Depth = SvnDepth.Infinity,
                Revision = SvnRevision.Head,
            };
            SvnTarget listTarget = MakeSvnTrueTarget(client, fileRootUri, SvnRevision.Head);
            if (!client.List(listTarget, listArgs, (sender, eventArgs) =>
            {
                if (eventArgs.Entry.NodeKind != SvnNodeKind.File)
                {
                    return;
                }
                String path = eventArgs.Path;
                if (path == fileName || path.EndsWith("/" + fileName))
                {
                    result = true;
                }
            }))
            {
                throw new Exception(String.Format("failed to List {0} for {1}", fileRootUri, fileName));
            }
            return result;
        }        

		void ValidateTags()
		{
			int headRevision = logList.Last().Revision;
			List<int> normalList = config.normalList.ToList();
			normalList.RemoveAll((r) => r > headRevision);
			if (normalList.Count() == 0)
			{
				throw new Exception(string.Format("没有有效的版本节点，请检查tags.txt，当前最新版本为{0}", headRevision));
			}
		}

        void ValidateEarlydownload()
        {
            if (config.earlydownloadList.Count() > 0 || config.earlydownloadBugfixList.Count() > 0)
            {
                int startRevision = config.normalList.Count() > 0 ? config.normalList[0] : logList.First().Revision;
                int headRevision = logList.Last().Revision;
                PackerConfig.ValidateEarlydownloadTags(config.earlydownloadList, config.earlydownloadBugfixList, startRevision, headRevision);
            }
        }

		void ValidateLanguageRulesBeforePack()
		{
			bool throwException = false;

			int startRevision = 0;
			if (config.normalList.Count() > 0)
			{
				startRevision = config.normalList[0];
			}
			bool assetsValid = true;
			if (startRevision > 0 && !ValidateLanguageRules(assetsUri, startRevision, true, throwException))
			{
				assetsValid = false;
			}
			int headRevision = logList.Last().Revision;
			if (!ValidateLanguageRules(assetsUri, headRevision, false, throwException))
			{
				assetsValid = false;
			}

			bool compareAssetsValid = (compareAssetsUri == null) || ValidateLanguageRules(compareAssetsUri, compareAssetsSwitchRevision, true, throwException);
			if (!assetsValid || !compareAssetsValid)
			{
				throw new Exception("语言资源不符合规则，具体原因见上");
			}
		}

		bool ValidateLanguageRules(string uri, int revision, bool skipLangCountRule, bool throwException)
		{
			if (!config.hasLanguage || config.baseHasLanguage)
			{
				return true;
			}
			bool result = false;
			if (uri != assetsUri || !GetLangRulesResult(skipLangCountRule).TryGetValue(revision, out result))
			{
				OutputLog(string.Format("正在检查'{0}'的版本{1}是否符合语言资源规则", uri, revision));

				//	获取版本对应的资源列表，并划分成基础资源、L10N下的各语言资源，L10N下语言子目录资源去掉L10N及语言前缀便于后续比较
				Dictionary<string, int> fileList = GetFilesRevision(uri, revision, ((path) => PackageListContainsForPath(path)));
				var baseList = new Dictionary<string, bool>();
				var langLists = new Dictionary<string, Dictionary<string, bool>>();
				foreach (var pair in fileList)
				{
					if (!IsPathUnderDirectory(pair.Key, L10N))
					{
						var path = UnifyPath(pair.Key);
						if (!baseList.ContainsKey(path))
						{
							baseList.Add(path, true);
						}
					}
					else
					{
						string lang = RetrieveLanguageName(pair.Key);
						if (lang == null)
						{
							continue;
						}
						if (!config.IsInFullLanguageList(lang))
						{
							continue;
						}
						Dictionary<string, bool> langList = null;
						if (!langLists.TryGetValue(lang, out langList))
						{
							langList = new Dictionary<string, bool>();
							langLists.Add(lang, langList);
						}
						string pathWithoutL10N = UnifyPath(RemoveLanguagePrefixForPath(pair.Key));
						if (!langList.ContainsKey(pathWithoutL10N))
						{
							langList.Add(pathWithoutL10N, true);
						}
					}
				}
				if (langLists.Count() != config.languageCount - 1)  //	-1 用来排除特殊值 "base" (表示基础资源)
				{
					OutputLog(string.Format("已发现有效的语言子目录['{0}']、少于指定的语言子目录['{1}']",
						string.Join(",", langLists.Keys),
						string.Join(",", config.languages.Where(lang => !IsLanguageBase(lang)))
						));
				}

				//	验证关注的各语言子目录下资源文件个数和路径完全相同（不验证文件大小）
				var langDiffs = new Dictionary<string, Dictionary<string, List<string>>>(); //	lang->lang2, lang.Except(lang2)
				foreach (var pair in langLists)
				{
					var lang = pair.Key;
					var langList = pair.Value;
					foreach (var pair2 in langLists)
					{
						var lang2 = pair2.Key;
						if (lang2 == lang)
						{
							continue;
						}
						var lang2List = pair2.Value;
						var langOnlyFiles = langList.Except(lang2List);
						if (langOnlyFiles.Count() == 0)
						{
							continue;
						}
						Dictionary<string, List<string>> langDiff;
						if (!langDiffs.TryGetValue(lang, out langDiff))
						{
							langDiff = new Dictionary<string, List<string>>();
							langDiffs.Add(lang, langDiff);
						}
						langDiff.Add(lang2, langOnlyFiles.Select(pair3 => pair3.Key).ToList());
					}
				}
				if (langDiffs.Count() > 0)
				{
					foreach (var pair in langDiffs)
					{
						var lang = pair.Key;
						var langDiff = pair.Value;
						foreach (var pair2 in langDiff)
						{
							var lang2 = pair2.Key;
							var langOnlyFiles = pair2.Value;
							OutputLog(string.Format("语言{0}内有语言{1}没有的{2}个文件:", lang, lang2, langOnlyFiles.Count()));
							foreach (var path in langOnlyFiles)
							{
								OutputLog(string.Format("'{0}'", path));
							}
						}
					}
				}

				//	验证任一L10N下语言子目录内的资源，在基础资源内不存在
				var baseOverlaps = new Dictionary<string, List<string>>();
				foreach (var pair in langLists)
				{
					var lang = pair.Key;
					var langList = pair.Value;
					var baseOverlap = baseList.Intersect(langList);
					if (baseOverlap.Count() == 0)
					{
						continue;
					}
					baseOverlaps.Add(lang, baseOverlap.Select(pair2 => pair2.Key).ToList());
				}
				if (baseOverlaps.Count() > 0)
				{
					foreach (var pair in baseOverlaps)
					{
						var lang = pair.Key;
						var baseOverlap = pair.Value;
						OutputLog(string.Format("基础资源内有{0}个文件名与语言{1}重合:", baseOverlap.Count(), lang));
						foreach (var path in baseOverlap)
						{
							OutputLog(string.Format("'{0}'", path));
						}
					}
				}

				result = (skipLangCountRule || (langLists.Count() == config.languageCount - 1)) && (langDiffs.Count() == 0 && baseOverlaps.Count() == 0);
				if (uri == assetsUri)
				{
					GetLangRulesResult(skipLangCountRule).Add(revision, result);
				}
			}
			if (!result)
			{
				string message = string.Format("'{0}'在版本{1}的语言资源不符合规则", uri, revision);
				if (throwException)
				{
					throw new Exception(message);
				}
				else
				{
					OutputLog(message);
				}
			}
			return result;
		}

        string MakeFirstVersionLine(List<int> normalList, int[] earlydownloadList, int[] earlydownloadBugfixList)
        {
			List<int> firstLineVersions = new List<int>();
			if (hasEarlydownload)
			{
				firstLineVersions.Add(normalList[0]);
				firstLineVersions.Add(earlydownloadList[0]);

				//  earlydownloadList 与 earlydownloadBugfixList 交替出现，且 earlydownloadList 先出现
				for (int i = 0; i < earlydownloadBugfixList.Count(); ++i)
				{
					if (i < earlydownloadBugfixList.Count() - 1 || earlydownloadBugfixList[i] != normalList.Last())   //  若为最新版本，则不用添加，因为尚未真正生效
					{
						firstLineVersions.Add(earlydownloadBugfixList[i]);       //  第i次预下载版本范围:earlydownloadList[i] - earlydownloadBugfixList[i]
					}
					if (i + 1 < earlydownloadList.Count())
					{
						if (i + 1 < earlydownloadList.Count() - 1 || earlydownloadList[i + 1] != normalList.Last())
						{
							firstLineVersions.Add(earlydownloadList[i + 1]);     //  第i次紧急更新版本范围:earlydownloadBugfixList[i] - earlydownloadList[i+1]
						}
					}
				}
			}
			else
			{
				firstLineVersions.Add(normalList[0]);
				firstLineVersions.Add(normalList[normalList.Count - 1]);
			}
			return string.Format("{{ {0} }},\n", string.Join(", ", firstLineVersions));
        }

        string MakeEarlyownloadFirstVersionLine(List<int> normalList, int[] earlydownloadList, int[] earlydownloadBugfixList)
        {
            //  earlydownloadList 与 earlydownloadBugfixList 交替出现，且 earlydownloadList 先出现
            string firstLine = string.Format("Version:    {0}/{1}", earlydownloadList[0], normalList[0]);
            for (int i = 0; i < earlydownloadBugfixList.Count(); ++i)
            {
                if (i < earlydownloadBugfixList.Count() - 1 || earlydownloadBugfixList[i] != normalList.Last())   //  若为最新版本，则不用添加，因为尚未真正生效
                {
                    firstLine += string.Format("/{0}", earlydownloadBugfixList[i]);                //  第i次预下载版本范围:earlydownloadList[i] - earlydownloadBugfixList[i]
                }
                if (i + 1 < earlydownloadList.Count())
                {
                    if (i + 1 < earlydownloadList.Count() - 1 || earlydownloadList[i + 1] != normalList.Last())
                    {
                        firstLine += string.Format("/{0}", earlydownloadList[i + 1]);     //  第i次紧急更新版本范围:earlydownloadBugfixList[i] - earlydownloadList[i+1]
                    }
                }
            }
            return firstLine;
        }

        static String UnifyPath(String path)
		{
			return path.ToLowerInvariant().Replace('\\', '/');
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="revisionFrom">开始版本</param>
		/// <param name="revisionTo">开始版本</param>
		/// <param name="limit">限制取得结果的数量, 0 表示无限制</param>
		/// <returns></returns>
        public List<LogInfo> GetLog(SvnRevision revisionFrom, SvnRevision revisionTo, int limit)
        {
            List<LogInfo> logList = new List<LogInfo>();
            OutputLog("正在获取svn日志");

            SvnInfoEventArgs infoArgs;
            SvnTarget target = SvnTarget.FromString(assetsUri);
            if (!client.GetInfo(target, out infoArgs))
            {
                throw new Exception("信息获取错误");
            }

            string prefix = infoArgs.RepositoryRoot.AbsoluteUri;
            prefix = "/" + assetsUri.Substring(assetsUri.IndexOf(prefix) + prefix.Length) + "/";

            SvnLogArgs logArgs = new SvnLogArgs
            {
                Start = revisionFrom,
                End = revisionTo,
                Limit = limit,
            };

            Collection<SvnLogEventArgs> logItems;
            if (!client.GetLog(new Uri(assetsUri), logArgs, out logItems))
            {
                throw new Exception("日志获取错误");
            }

            int count = logItems.Count;
            foreach (SvnLogEventArgs args in logItems)
            {
                int revision = (int)args.Revision;
                var info = new LogInfo(args.ChangedPaths.Count)
                {
                    Revision = revision,
                    Author = args.Author,
                    Message = args.LogMessage,
                    Time = args.Time.ToLocalTime(),
                };

                foreach (SvnChangeItem item in args.ChangedPaths)
                {
                    if (item.Action == SvnChangeAction.None)
                    {
                        throw new Exception("不应该出现的异常");
                    }

                    if (!item.Path.StartsWith(prefix))
                    {
                        continue;
                    }

                    string path = item.Path.Substring(prefix.Length);
                    if (item.NodeKind == SvnNodeKind.Directory)
                    {
                        path = path + "/";
                    }

                    if (!PackageListContainsForPath(path))
                    {
                        continue;
                    }

                    info.AddChangeItem(new ChangeItem(revision, path, item));
                }

                logList.Add(info);
            }

            return logList;
        }

		/// <summary>
		/// 生成 svn target，可应对 copy/rename
		/// </summary>
		/// <param name="svnClient"></param>
		/// <param name="uri"></param>
		/// <param name="revision"></param>
		/// <returns></returns>
		static Uri GetSvnTrueUri(SvnClient svnClient, String uri, SvnRevision revision)
		{
			Collection<SvnInfoEventArgs> infoCollection;
			if (!(svnClient.GetInfo(SvnTarget.FromString(uri), new SvnInfoArgs{Revision=revision}, out infoCollection)))
				throw new Exception("failed to get diff from info");
			SvnInfoEventArgs info = infoCollection.First();
			return info.Uri;
		}

		/// <summary>
		/// 生成 svn target，可应对 copy/rename
		/// </summary>
		/// <param name="svnClient"></param>
		/// <param name="uri"></param>
		/// <param name="revision"></param>
		/// <returns></returns>
		static SvnUriTarget MakeSvnTrueTarget(SvnClient svnClient, String uri, SvnRevision revision)
		{
			return new SvnUriTarget(GetSvnTrueUri(svnClient, uri, revision), revision);
		}

        Dictionary<string, DiffSummaryItem> SvnDiff(string fromURI, int from, string toURI, int to)
        {
            Dictionary<string, DiffSummaryItem> changes = new Dictionary<string, DiffSummaryItem>();
            SvnTarget fromTarget = new SvnUriTarget(fromURI, new SvnRevision(from));
            SvnTarget toTarget = new SvnUriTarget(toURI, new SvnRevision(to));
            if (!client.DiffSummary(fromTarget, toTarget, (sender, eventArgs) =>
            {
                if (eventArgs.NodeKind != SvnNodeKind.File)
                    return;
                String path = eventArgs.Path;
                if (!PackageListContainsForPath(path))
                {
                    return;
                }

                DiffSummaryItem item;
                if (changes.TryGetValue(path, out item))
                {
                    item.Revision = to;
                    item.DiffKind = eventArgs.DiffKind;
                }
                else
                {
                    changes.Add(path, new DiffSummaryItem
                    {
                        Path = path,
                        Revision = to,
                        DiffKind = eventArgs.DiffKind
                    });
                }
            }))
            {
                throw new Exception(String.Format("failed to DiffSummary between {0}-{1}", from, to));
            }

			return changes;
        }

        Dictionary<string, DiffSummaryItem> SvnBranchDiff(string fromURI, string from_ingameupdate_dir, int from, string toURI, string to_ingameupdate_dir, int to) {
            Dictionary<string, DiffSummaryItem> changes;
			if (config.enable_ingameupdate != config.compare_enable_ingameupdate)
			{
				throw new Exception(string.Format("无法制作服务器切换包: 切换前{0}小包、而切换后{1}小包!", config.enable_ingameupdate ? "使用了" : "没使用", config.compare_enable_ingameupdate ? "使用了" : "没使用"));
			}
            else if (config.enable_ingameupdate)
            {
				//  给定 svn path 如何判断是否为B包文件: 在 b_file_list.txt 中出现即为B包文件
				//  给定 svn path 如何判断是否为A包文件: 由于 a_file_list.txt 中不包含 lua 等文件所以实际上并不完整，因此准确的判断方式为不在 b_file_list.txt 才是A包文件，或者收集完整列表并排除出现在 b_file_list.txt 中的文件
				//  无论是A包文件、还是B包文件，首先都需要被 PackageListContainsForPath 认可
				//  需要生成的是A包文件的变化更新包、也就是不考虑B包，B包文件在游戏内有单独的更新机制
				//
				//  有B包时从两个分支 svn diff 计算出从from A包到to A包更新包的考虑:
				//  1.先考虑增加、删除操作，资源不一定有变化(未变化资源在A、B包间切换也会产生)
				//      1.1 需要增加：不在 from A，但在 to A 的
				//      2.2 需要删除: 在 from A，但不在 to A 的
				//  2.修改操作的处理:
				//      2.1 如果在 from A包、也在 to A 包的

				List<string> from_alist, from_blist;
				LoadABFileList(fromURI, from_ingameupdate_dir, from, out from_alist, out from_blist);

				List<string> to_alist, to_blist;
				LoadABFileList(toURI, to_ingameupdate_dir, to, out to_alist, out to_blist);

                changes = new Dictionary<string, DiffSummaryItem>();
                Action<String, SvnDiffKind> AddToChanges = (path, diffKind) =>
                {
                    DiffSummaryItem item;
                    if (changes.TryGetValue(path, out item))
                    {
                        item.DiffKind = diffKind;
                    }
                    else
                    {
                        changes.Add(path, new DiffSummaryItem
                        {
                            Path = path,
                            Revision = to,
                            DiffKind = diffKind,
                        });
                    }
                };

                var addedFiles = to_alist.Except(from_alist);
                foreach (var path in addedFiles)
                {
                    AddToChanges(path, SvnDiffKind.Added);      //  1.1
                }

                var removedFiles = from_alist.Except(to_alist);
                foreach (var path in removedFiles)
                {
                    AddToChanges(path, SvnDiffKind.Deleted);    //  1.2
                }

                Dictionary<string, DiffSummaryItem> diffList = SvnDiff(fromURI, from, toURI, to);
                foreach (var diff in diffList)
                {
                    if (diff.Value.DiffKind == SvnDiffKind.Modified)
                    {
                        string path = diff.Value.Path;
                        if (from_alist.Contains(path) && to_alist.Contains(path))
                        {
                            AddToChanges(path, SvnDiffKind.Modified);   //  2.1
                        }
                    }
                }
            }
            else
            {
                changes = SvnDiff(fromURI, from, toURI, to);
			}
            return changes;
        }

        Int32[] GetIgnoredRevisionBetween(int from, int to, Dictionary<string, Int32[]> ignoredDict)
        {
            SortedSet<int> allIgnoredRevision = new SortedSet<int>();
            if (ignoredDict != null && ignoredDict.Count > 0)
            {

                foreach (var e in ignoredDict.Values)
                {
                    foreach (var r in e)
                    {
                        if (r >= from && r <= to)
                            allIgnoredRevision.Add(r);
                    }
                }
            }

            allIgnoredRevision.Add(from);
            allIgnoredRevision.Add(to);
            return allIgnoredRevision.ToArray();
        }

        bool IsIgnored(string path, int revision, Dictionary<string, Int32[]> ignoredDict)
        {
            if (ignoredDict != null && ignoredDict.Count > 0)
            {
                Int32[] ignoredRevision;
                if (ignoredDict.TryGetValue(path, out ignoredRevision))
                {
                    return Array.IndexOf(ignoredRevision, revision) >= 0;
                }
            }
            return false;
        }

		IEnumerable<String> LoadFileList(SvnTarget svnTarget)
		{
			HashSet<String> fileList = new HashSet<String>();
			String tempFilePath = Path.GetTempFileName();
			try
			{
				SvnExportSingleFile(svnTarget, tempFilePath);

				return File.ReadAllLines(tempFilePath).Where(path=>!String.IsNullOrWhiteSpace(path))
					.Select(path=>UnifyPath(path));
			}
			finally
			{
				File.Delete(tempFilePath);
			}
		}

		IEnumerable<String> LoadInGameUpdateFileList(string fileName, int revision)
		{
            return LoadInGameUpdateFileList(assetsUri, config.ingameupdate_dir, fileName, revision);
        }

        IEnumerable<String> LoadInGameUpdateFileList(string assetsUri, string ingameupdate_dir, string fileName, int revision)
        {
            String uri = String.Format("{0}/{1}/{2}", assetsUri, ingameupdate_dir, fileName);
            return LoadFileList(new SvnUriTarget(uri, new SvnRevision(revision)));
        }

		void LoadABFileList(string uri, string ingameupdate_dir, int revision, out List<string> out_alist, out List<string> out_blist)
		{
			List<string> blist = LoadInGameUpdateFileList(uri, ingameupdate_dir, "b_file_list.txt", revision).ToList();
			blist.RemoveAll((path) => !PackageListContainsForPath(path));
			List<string> alist = GetFilesRevision(uri, revision, ((path) => PackageListContainsForPath(path) && !blist.Contains(path))).Keys.ToList();

			out_alist = alist;
			out_blist = blist;
		}

        Int32[] ExportCompareUriLatestHistoryList()
        {
            String uri = String.Format("{0}/history.txt", compareConfigUri);
            SvnTarget svnTarget = new SvnUriTarget(uri, SvnRevision.Head);
            String tempFilePath = Path.GetTempFileName();
            try
            {
                SvnExportSingleFile(svnTarget, tempFilePath);
                Int32[] compareHistoryList;
                PackerConfig.ParseHistory(tempFilePath, out compareHistoryList);
                return compareHistoryList;
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        void GetInGameUpdateExchangeDiffList(int from, int to, out IEnumerable<String> addedList, out IEnumerable<String> removedList)
		{
			IEnumerable<String> from_alist = LoadInGameUpdateFileList("a_file_list.txt", from);
			IEnumerable<String> from_blist = LoadInGameUpdateFileList("b_file_list.txt", from);
			IEnumerable<String> to_alist = LoadInGameUpdateFileList("a_file_list.txt", to);
			IEnumerable<String> to_blist = LoadInGameUpdateFileList("b_file_list.txt", to);

			addedList = to_alist.Except(from_alist).Intersect(from_blist);
			removedList = from_alist.Except(to_alist).Intersect(to_blist);
		}

        Dictionary<string, DiffSummaryItem> SvnDiff(int from, int to, List<int> historyAndTagsList, Dictionary<string, Int32[]> ignoredDict)
        {
            if (from >= to)
                throw new Exception(string.Format("invalid revision"));
            int fromLogIndex = logList.FindIndex((i) => i.Revision == from);
            int toLogIndex = logList.FindIndex((i) => i.Revision == to);
            if (fromLogIndex == -1 || toLogIndex == -1)
                throw new Exception(string.Format("Revision not found"));

            //考虑到客户端可以处于中间版本，不能只计算 from 与 to 的差异，需计算相邻(历史版本+所有有标记版本)之间的差异
            //某个文件，只要在任意一个差异中出现，就应将其最终状态 (增加/修改或删除) 计入更新包

            int fromIndex = historyAndTagsList.IndexOf(from);
            int toIndex = historyAndTagsList.IndexOf(to);
            if (fromIndex == -1 || toIndex == -1 || toIndex <= fromIndex)
                throw new Exception(string.Format("internal error"));

            Dictionary<string, DiffSummaryItem> changes = new Dictionary<string, DiffSummaryItem>();
            for (int iDiff = 0; iDiff < toIndex - fromIndex; ++iDiff)
            {
                int diffFrom = historyAndTagsList[fromIndex + iDiff];
                int diffTo = historyAndTagsList[fromIndex + iDiff + 1];
                int[] allRevision = GetIgnoredRevisionBetween(diffFrom, diffTo, ignoredDict);
                for (int i = 0; i < allRevision.Length - 1; ++i)
                {
                    diffFrom = allRevision[i];
                    diffTo = allRevision[i + 1];

                    SvnTarget diffFromTarget = MakeSvnTrueTarget(client, assetsUri, new SvnRevision(diffFrom));
                    SvnTarget diffToTarget = MakeSvnTrueTarget(client, assetsUri, new SvnRevision(diffTo));
					List<DiffSummaryItem> diffList = new List<DiffSummaryItem>();
                    if (!client.DiffSummary(diffFromTarget, diffToTarget, (sender, eventArgs) =>
					{
                        if (eventArgs.NodeKind != SvnNodeKind.File)
                            return;

						diffList.Add(new DiffSummaryItem
                        {
                            Path = eventArgs.Path,
                            Revision = diffTo,
                            DiffKind = eventArgs.DiffKind,
                        });
					}))
                    {
                        throw new Exception(String.Format("failed to DiffSummary between {0}-{1}", diffFrom, diffTo));
                    }

					Action<String, SvnDiffKind, bool> ProcessOneDiffFile = (path, diffKind, checkIgnore) =>
					{
						if (!PackageListContainsForPath(path))
						{
							return;
						}

                        int lastChangedRevision = diffTo;
                        SvnDiffKind lastSvnDiffKind = diffKind;
                        if (checkIgnore && IsIgnored(path, diffTo, ignoredDict))	//A、B间移动不能忽略
                        {
                            return;
                        }

                        DiffSummaryItem item;
                        if (changes.TryGetValue(path, out item))
                        {
                            item.Revision = lastChangedRevision;
                            item.DiffKind = lastSvnDiffKind;
                        }
                        else
                        {
                            changes.Add(path, new DiffSummaryItem
                            {
                                Path = path,
                                Revision = lastChangedRevision,
                                DiffKind = lastSvnDiffKind,
                            });
                        }
					};

					IEnumerable<String> ingameupdate_addedFiles;
					IEnumerable<String> ingameupdate_removedFiles;
					foreach (DiffSummaryItem diff in diffList)
                    {
						ProcessOneDiffFile(diff.Path, diff.DiffKind, true);
                    }
					//如果 list_exchange_flag.txt 变化了，表明 A、B 包有文件交换，需要用文件列表计算交换产生的更新
					if (config.enable_ingameupdate && (!config.hasLanguage || isCurrentLanguageBase) && diffList.Where(diff=>diff.Path == config.ingameupdate_dir + "/list_exchange_flag.txt").Any())
					{
						GetInGameUpdateExchangeDiffList(diffFrom, diffTo, out ingameupdate_addedFiles, out ingameupdate_removedFiles);
						foreach (String path in ingameupdate_addedFiles)
						{
							ProcessOneDiffFile(path, SvnDiffKind.Added, false);
						}
						foreach (String path in ingameupdate_removedFiles)
						{
							ProcessOneDiffFile(path, SvnDiffKind.Deleted, false);
						}
					}
				}
            }
			//排除 B 包文件
			if (config.enable_ingameupdate && (!config.hasLanguage || isCurrentLanguageBase))
			{
				Dictionary<String, String> bFileSet = LoadInGameUpdateFileList("b_file_list.txt", to).ToDictionary(path=>path);
				IEnumerable<String> ignoredPaths = changes
					.Where(change=>bFileSet.ContainsKey(change.Key.ToLower()))
					.Where(change=>change.Value.DiffKind != SvnDiffKind.Deleted)
					.Select(change=>change.Key);
				foreach (String ignoredPath in ignoredPaths.ToArray())	//需要强制 ignoredPaths 完成计算，然后再开始删除
				{
					changes.Remove(ignoredPath);
				}
			}
            return changes;
        }

        Dictionary<string, DiffSummaryItem> SvnListRevisionAsDiff(int to)
        {
            Dictionary<string, DiffSummaryItem> changes = new Dictionary<string, DiffSummaryItem>();
            SvnListArgs listArgs = new SvnListArgs
            {
                Depth = SvnDepth.Infinity,
                Revision = new SvnRevision(to),
                RetrieveEntries = SvnDirEntryItems.Revision,
            };
            SvnTarget listTarget = MakeSvnTrueTarget(client, assetsUri, new SvnRevision(to));
            if (!client.List(listTarget, listArgs, (sender, eventArgs) =>
            {
                if (eventArgs.Entry.NodeKind != SvnNodeKind.File)
                    return;

                String path = eventArgs.Path;
                if (!PackageListContainsForPath(path))
                    return;

                changes.Add(path, new DiffSummaryItem
                {
                    Path = path,
                    Revision = (int)eventArgs.Entry.Revision,
                    DiffKind = SvnDiffKind.Added,
                });
            }))
            {
                throw new Exception(String.Format("failed to List @ {0}", to));
            }
            return changes;
        }

        /// <summary>
        /// 导出指定 svn uri 的部分文件，每个文件可指定导出版本
        /// </summary>
        void SvnExport(string uri, string output, Dictionary<string, DiffSummaryItem> changes)
        {
            if (Directory.Exists(output))
                Directory.Delete(output, true);
            Directory.CreateDirectory(output);

            SvnExportArgs exportArg = new SvnExportArgs();
            exportArg.Overwrite = true;
            exportArg.ThrowOnError = false;
            foreach (var item in changes.Values) {
                if (item.DiffKind == SvnDiffKind.Deleted)
                    continue;

                string val = item.Path;
                string path = string.Format("{0}/element/{1}", output, RemoveLanguagePrefixForPath(val.ToLower()));
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                int tryTimes = 0;
                exportArg.Revision = new SvnRevision(item.Revision);
                SvnTarget exportTarget = new SvnUriTarget(string.Format("{0}/{1}", uri, val.Replace("#", "%23")), exportArg.Revision);
                while (!client.Export(exportTarget, path, exportArg)) {
                    if (++tryTimes > 100) {
                        throw exportArg.LastException;
                    }

                    OutputLog(string.Format("第{0}次导出失败，正在重试：'{1}'", tryTimes, val), LogMode.Console);
                    OutputLog(exportArg.LastException.ToString(), LogMode.Console);
                    Thread.Sleep(3000);
                }
            }
        }

		/// <summary>
		/// 导出指定 svn uri 的一个文件
		/// </summary>
        void SvnExportSingleFile(SvnTarget uriTarget, string outputPath)
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            SvnExportArgs exportArg = new SvnExportArgs();
            exportArg.Overwrite = true;
            exportArg.ThrowOnError = false;

            int tryTimes = 0;
            while (!client.Export(uriTarget, outputPath, exportArg)) {
                if (++tryTimes > 100) {
                    throw exportArg.LastException;
                }

                OutputLog(string.Format("第{0}次导出失败，正在重试：'{1}'", tryTimes, uriTarget.TargetName), LogMode.Console);
                OutputLog(exportArg.LastException.ToString(), LogMode.Console);
                Thread.Sleep(3000);
            }
		}

		/// <summary>
		/// 利用工作目录导出 svn 特定版本的部分文件
		/// 与 SvnExport 相比，此方法性能较好 (文件数量很多时，性能远好)，但有如下限制：
		///	  只能指定一个整体版本号
		///	  需要提供本地工作目录
		/// </summary>
        void SvnExportFromWorkDir(string uri, int revision, string workDir, string output, Dictionary<string, DiffSummaryItem> changes)
		{
            if (Directory.Exists(output))
                Directory.Delete(output, true);
            Directory.CreateDirectory(output);

			SvnUriTarget trueUriTarget = MakeSvnTrueTarget(client, uri, revision);
			OutputLog(string.Format("检出本地目录 {0} 为 {1}", workDir, trueUriTarget));
			SvnPureCheckOut(trueUriTarget, workDir);

            foreach (var item in changes.Values) {
                if (item.DiffKind == SvnDiffKind.Deleted)
                    continue;

                string sourcePath = string.Format("{0}/{1}", workDir, item.Path);
                string destPath = string.Format("{0}/element/{1}", output, RemoveLanguagePrefixForPath(item.Path.ToLower()));
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
				File.Copy(sourcePath, destPath, true);
				new FileInfo(destPath).IsReadOnly = false;
            }
		}

		void SvnPureCheckOut(SvnUriTarget uriTarget, string workDir)
		{
			if (Directory.Exists(workDir))	//兼容 workDir 还未检出的情况
			{
				SvnPureRevert(uriTarget, workDir, true);
			}

			Directory.CreateDirectory(Path.GetDirectoryName(workDir));

            SvnCheckOutArgs checkOutArg = new SvnCheckOutArgs();
            checkOutArg.ThrowOnError = false;

            int tryTimes = 0;
            while (!client.CheckOut(uriTarget, workDir, checkOutArg)) {
				if (++tryTimes > 20) {
					throw checkOutArg.LastException;
				}
                OutputLog(string.Format("第{0}次检出失败，正在重试", tryTimes), LogMode.Console);
                OutputLog(checkOutArg.LastException.ToString(), LogMode.Console);
                Thread.Sleep(3000);
			}

			//检查是否有文件修改
			SvnPureRevert(uriTarget, workDir, false);
		}

		void SvnPureRevert(SvnUriTarget uriTarget, string workDir, bool ignoreNonWorkingCopy)
		{
			//检查是否有文件修改
            SvnStatusArgs statusArg = new SvnStatusArgs();
			Collection<SvnStatusEventArgs> statuses;
			try
			{
				client.GetStatus(workDir, statusArg, out statuses);
			}
			catch (SvnWorkingCopyException)
			{
				if (ignoreNonWorkingCopy)
					return;
				else
					throw;
			}
			if (statuses.Count > 0) {
				//有修改时再 revert，可以快一点
				SvnRevertArgs revertArg = new SvnRevertArgs();
				revertArg.Depth = SvnDepth.Infinity;
				client.Revert(workDir, revertArg);

				//revert 后再检查下
				Collection<SvnStatusEventArgs> statusesAfterRevert;
				client.GetStatus(workDir, statusArg, out statusesAfterRevert);

				if (statusesAfterRevert.Count > 0)
				{
					StringBuilder msgBuilder = new StringBuilder();
					foreach (SvnStatusEventArgs status in statusesAfterRevert) {
						msgBuilder.AppendFormat("{0}, ", status.Path);
					}
					throw new Exception(string.Format("assets 工作目录有文件处于修改状态：\n{0}", msgBuilder));
				}
			}
		}

        Dictionary<string, int> GetFilesRevision(string uri, int revision, Predicate<string> filter = null) {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            SvnListArgs args = new SvnListArgs();
            args.Depth = SvnDepth.Infinity;
            args.Revision = new SvnRevision(revision);
            args.RetrieveEntries = SvnDirEntryItems.Revision;

            if (!client.List(uri, args, (sender, e) => {
                if (e.Entry.NodeKind != SvnNodeKind.File)
                    return;

                string path = e.Path;
                if (filter == null || filter(path)) {
                    dict.Add(path, (int)e.Entry.Revision);
                }
            })) {
                throw new Exception(string.Format("获取文件版本号列表出错误: '{0}', r'{1}'", uri, revision));
            }

            return dict;
        }

        public void CompressBigFile(string file)
        {
            string dstTmpFile = file + ".tmp";
            if (File.Exists(dstTmpFile))
                File.Delete(dstTmpFile);
            File.Copy(file, dstTmpFile, true);
            try
            {
                FileStream fsRead = new FileStream(dstTmpFile, FileMode.Open);
                FileStream stream = new FileStream(file, FileMode.Create);
                //这个其实就是AFilePackage_Compress_LZ4
                stream.WriteByte(0x58);
                stream.WriteByte(0xAF);
                stream.WriteByte(0x5A);
                stream.WriteByte(0x00);
                byte[] bytes = BitConverter.GetBytes(Convert.ToUInt32(fsRead.Length));
                stream.Write(bytes, 0, bytes.Length);

                byte[] buffer = new byte[1024 * 8];
                int size = 0;
                do
                {
                    size = fsRead.Read(buffer, 0, buffer.Length);
                    if(size >0) stream.Write(buffer, 0, size);
                } while (size > 0);
                fsRead.Close();
                stream.Close();
            }
            finally
            {
                if (File.Exists(dstTmpFile))
                    File.Delete(dstTmpFile);
            }
        }

        //单个文件超过系统允许最大内存后，将会异常
        public void CompressFile(string path) {
            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
			DateTime startTime = System.DateTime.Now;
			ParallelLoopResult parallelLoopResult = Parallel.ForEach(files, (file) =>
			{
				String extension = Path.GetExtension(file).ToLower();
				AFileCompressionMethod compressionMethod = config.afile_cfg.GetCompressionMethod(extension);
				OutputLog(string.Format("CompressFile with method '{0}': {1}", compressionMethod, file.Substring(path.Length)));
				byte[] data = File.ReadAllBytes(file);
				AFilePackageCompress.CompressToFile(compressionMethod, file, data);
			});
			if (!parallelLoopResult.IsCompleted)
			{
				throw new Exception("----Parallel CompressFile Failed!----");
			}
			DateTime finishTime = System.DateTime.Now;
			TimeSpan timeSpan = finishTime.Subtract(startTime);
			OutputLog(string.Format("Compressing {0} files within '{1}' costs {2}", files.Count(), path, timeSpan.ToString()));
		}

        void Compress7z(string input, string output, bool deleteInput) {
            if(File.Exists(output)) {
                File.Delete(output);
            }

            SevenZipCompressor szc = new SevenZipCompressor();
            szc.PreserveDirectoryRoot = true;
            szc.ArchiveFormat = OutArchiveFormat.SevenZip;
            szc.CompressionLevel = CompressionLevel.Fast;
            szc.CustomParameters.Add("mt", "on");   //turn on multi-threaded compression
            szc.CustomParameters.Add("s", "off");   //-ms = off  一定要用非固实压缩，否则解包会很费！！
			szc.CompressionMethod = CompressionMethod.Lzma;
            szc.CompressDirectory(Path.GetFullPath(input), output);	//源路径用绝对路径，否则会生成奇怪的上层目录

            if(deleteInput) {
                Directory.Delete(input, true);
            }
        }

		bool IsPackEmptyByUncompress7z(string input)
		{
			try
			{
				using (SevenZipExtractor zip = new SevenZipExtractor(Path.GetFullPath(input), InArchiveFormat.SevenZip))
				{
					if (zip.FilesCount > 1)
					{
						return false;
					}
					string NameInc = "inc";
					bool IsRealNameIncIn7zPack(string FileName)
					{
						int seperatorIndex = FileName.IndexOfAny(PathSeperators);
						if (seperatorIndex < 0)
						{
							return false;
						}
						string realName = FileName.Substring(seperatorIndex+1);
						return realName == NameInc;
					}
					var IncInfo = zip.ArchiveFileData.FirstOrDefault(archiveFileInfo => !archiveFileInfo.IsDirectory && IsRealNameIncIn7zPack(archiveFileInfo.FileName));
					if (!IsRealNameIncIn7zPack(IncInfo.FileName))
					{
						return false;
					}
					using (MemoryStream ms = new MemoryStream())
					{
						zip.ExtractFile(IncInfo.Index, ms);
						using (var reader = new StreamReader(ms, Encoding.ASCII))
						{
							ms.Position = 0;

							string line;
							while ((line = reader.ReadLine()) != null)
							{
								line = line.Trim();
								if (line.Length == 0)
								{
									continue;
								}
								if (line.StartsWith("#"))
								{
									continue;
								}
								return false;	//	不是注释行、也不是空行，不管内容是啥都认为非空
							}
							return true;
						}
					}
				}
			}
			catch (Exception e)
			{
				OutputLog(string.Format("解压检查 Inc 遇到异常：'{0}', Message:'{1}'", input, e.Message));
			}
			return false;
		}

        void WriteIncFile(string path, PackInfo info, Dictionary<string, DiffSummaryItem> changes, bool debug)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("# {0} {1} {2} {3}", info.From, info.To, project, changes.Count));

            List<string> changePaths = changes.Keys.ToList();
            changePaths.Sort();
            foreach (var changePath in changePaths)
            {
                var item = changes[changePath];
                if (item.DiffKind == SvnDiffKind.Deleted)
                {
                    if (debug)
                        builder.AppendLine(string.Format("- @{0, -6} {1}", item.Revision, RemoveLanguagePrefixForPath(item.Path.ToLower())));
                    else
                        builder.AppendLine(string.Format("- {0}", RemoveLanguagePrefixForPath(item.Path.ToLower())));
                }
                else
                {
                    if (debug)
                        builder.AppendLine(string.Format("! @{0, -6} {1}", item.Revision, RemoveLanguagePrefixForPath(item.Path.ToLower())));
                    else
                        builder.AppendLine(string.Format("! {0}", RemoveLanguagePrefixForPath(item.Path.ToLower())));
                }
            }

            File.WriteAllText(path, builder.ToString());
        }

        void SetTimestamp(string path, DateTime timestamp) {
            if(File.Exists(path)) {
                File.SetCreationTimeUtc(path, timestamp);
                File.SetLastAccessTimeUtc(path, timestamp);
                File.SetLastWriteTimeUtc(path, timestamp);
            } else {
                Directory.SetCreationTimeUtc(path, timestamp);
                Directory.SetLastAccessTimeUtc(path, timestamp);
                Directory.SetLastWriteTimeUtc(path, timestamp);
                foreach(String entry in Directory.GetFileSystemEntries(path)) {
                    SetTimestamp(entry, timestamp);
                }
            }
        }

		string ComputeFileHash(string path)
		{
			using (Stream fileStream = File.OpenRead(path))
			{
				MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
				byte[] hashBytes = md5.ComputeHash(fileStream);
				return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
			}
		}

        void HashFileName(string path, PackInfo info) {
			FileInfo fileInfo = new FileInfo(path);
			info.Size = fileInfo.Length;
			if ((int)info.Size < 0)
			{
				OutputLog(string.Format("文件 {0} 已超过2GB，当前大小为 {1} 字节，{2:N2} GB", path, info.Size, ((double)info.Size) / 1024 / 1024 / 1024));
			}
			info.Hash = ComputeFileHash(path);
            info.FullName = string.Format("{0}.{1}.{2}", info.Name, info.Hash.Substring(0, 6), extension);

            string newZipFile = string.Format("{0}/{1}", output, info.FullName);
            if(File.Exists(newZipFile)) {
                File.Delete(newZipFile);
            }
            File.Move(path, newZipFile);
        }

        void Pack(PackInfo info, List<int> historyAndTagsList, Dictionary<string, Int32[]> ignoredDict)
        {
			string packTag = GetPackTag(info);

            string exportDir = string.Format("{0}/{1}", output, info.Name);
            if(Directory.Exists(exportDir)) {
                OutputLog(string.Format("[0]{0} 正在删除文件夹:{1}", packTag, info.Name));
                Directory.Delete(exportDir, true);
            }

            Directory.CreateDirectory(exportDir);

			void ConditionalValidateLanguageRules(string uri, int revision)
			{
				if (config.hasLanguage && isCurrentLanguageBase)
				{
					bool skipLangCountRule = true;	//	考虑到可能中途加入语言，打包过程中语言个数不同时不抛异常
					bool throwException = true;     //	需要抛异常，确保 ProcessChangesByLanguage 内"把语言下的删除操作带进基础资源更新包"逻辑可靠
					ValidateLanguageRules(uri, revision, skipLangCountRule, throwException);
				}
			}

            string log = string.Format("{0}-{1}", info.From, info.To);
            OutputLog(string.Format("[1]{0} 正在获取版本差异文件列表: {1}", packTag, log));
            Dictionary<string, DiffSummaryItem> changes;
            if (info.IsBranchDiff) {
				ConditionalValidateLanguageRules(assetsUri, info.From);
				ConditionalValidateLanguageRules(compareAssetsUri, info.To);
                changes = SvnBranchDiff(assetsUri, config.ingameupdate_dir, info.From, compareAssetsUri, config.compare_ingameupdate_dir, info.To);
            } else if (info.IsNotDiff) {
				ConditionalValidateLanguageRules(assetsUri, info.To);
                changes = SvnListRevisionAsDiff(info.To);
			} else {
				ConditionalValidateLanguageRules(assetsUri, info.From);
				ConditionalValidateLanguageRules(assetsUri, info.To);
                changes = SvnDiff(info.From, info.To, historyAndTagsList, ignoredDict);
            }
			ProcessChangesByLanguage(info, changes);

            Directory.CreateDirectory(diffPath);
            string diff_filename = string.Format("{0}/{1}{2}-{3}.diff.txt", diffPath, info.IsBranchDiff ? "switch" : "", info.From, info.To);
            CalcDiffList(changes, diff_filename);
            OutputLog(string.Format("[2]{0} 版本{1}到版本{2}差异文件共{3}个。", packTag, info.From, info.To, changes.Count));
			CountChanges(changes, info);

			if (isCurrentLanguageOthers && info.IsNotDiff && info.FileCount <= 0)
			{
				throw new Exception(string.Format("切往语言[{0}]的更新包到版本{1}为止没有增加或修改的内容，这将导致切换后没有变化!", currentLanguage, info.To));
			}

            OutputLog(string.Format("[3]{0} 正在导出版本差异文件: {1}", packTag, log));
			if (info.IsBranchDiff)
			{
				if (compareAssetsWorkdir != null && compareAssetsWorkdir.Length > 0)
				{
					SvnExportFromWorkDir(compareAssetsUri, info.To, compareAssetsWorkdir, exportDir, changes);
				}
				else
				{
					SvnExport(compareAssetsUri, exportDir, changes);
				}
			}
			else
			{
				SvnExportFromWorkDir(assetsUri, info.To, assetsWorkdir, exportDir, changes);
			}

            OutputLog(string.Format("[4]{0} 正在压缩版本差异文件: {1}", packTag, log));
            CompressFile(exportDir);
            WriteIncFile(string.Format("{0}/inc", exportDir), info, changes, false);
            if(debug_inc)
                WriteIncFile(string.Format("{0}/inc.debug", exportDir), info, changes, true);
            SetTimestamp(exportDir, new DateTime(2000, 1, 1, 0, 0, 0));

            OutputLog(string.Format("[5]{0} 正在生成更新包: {1}", packTag, log));
            string zipFile = string.Format("{0}.{1}", exportDir, extension);
            Compress7z(exportDir, zipFile, true);
            HashFileName(zipFile, info);
        }

        public List<PackInfo> Pack()
        {
            if (string.IsNullOrEmpty(assetsWorkdir))
            {
                throw new Exception("local-assets 参数未设置，生成更新包时此参数为必需参数");
            }
            if (config.afile_cfg == null)
            {
                throw new Exception("afile-cfg 参数未设置，生成更新包时此参数为必需参数");
            }

            void MergeHistoryList(ref int[] historyList, List<PackInfo> packList)
            {
                HashSet<int> packVersionList = new HashSet<int>();
                foreach (PackInfo info in packList)
                {
                    if (info.IsBranchDiff)
                        continue;
                    if (info.IsNotDiff)
                        continue;
                    if (!packVersionList.Contains(info.From))
                        packVersionList.Add(info.From);
                    if (!packVersionList.Contains(info.To))
                        packVersionList.Add(info.To);
                }
                historyList = historyList.Union(packVersionList).Distinct().OrderBy(i => i).ToArray();
            }

            void MergeEarlydownloadHistoryList(ref int[] earlydownloadHistoryList, List<PackInfo> packList)
            {
                HashSet<int> packVersionList = new HashSet<int>();
                foreach (PackInfo info in packList)
                {
                    if (!info.IsEarlydownload)
                        continue;
                    if (!packVersionList.Contains(info.From))
                        packVersionList.Add(info.From);
                    if (!packVersionList.Contains(info.To))
                        packVersionList.Add(info.To);
                }
                earlydownloadHistoryList = earlydownloadHistoryList.Union(packVersionList).Distinct().OrderBy(i => i).ToArray();
            }

            {
                logList = GetLog(config.normalList.Count() > 0 ? new SvnRevision(config.normalList[0]) : SvnRevision.One, SvnRevision.Head, 0);
				compareAssetsSwitchRevision = (compareAssetsUri != null) ? RetrieveCompareAssetsSwitchRevision() : -1;
				GetLangRulesResult(false).Clear();
				GetLangRulesResult(true).Clear();
				ValidateTags();
                ValidateEarlydownload();
				ValidateLanguageRulesBeforePack();
            }

			int GetAutoTagLatestVersion()
			{
				int maxVersion = 0;
				if (config.normalList.Count() > 0)
				{
					maxVersion = config.normalList.Last();
				}
				int headRevision = logList.Last().Revision;
				if (maxVersion < headRevision)
				{
					maxVersion = headRevision;
				}
				return maxVersion;
			}
			int autoTagLatestVersion = GetAutoTagLatestVersion();

			bool ShouldAutoTagAllImportant()
			{
				if (!config.auto_tag_all_important)
				{
					return false;
				}
				if (isEarlydownloadMode)
				{
					return false;   //	预下载非紧急更新模式有单独记录，此处忽略
				}
				if (isEarlydownloadBugfixMode)
				{
					if (config.earlydownloadBugfixList.Last() == autoTagLatestVersion)
					{
						return false;   //	最新版本是刚标记的紧急更新版本，还有机会取消继续紧急更新状态，暂时不用处理
					}
				}
				if (config.normalList.Count() == 0)
				{
					return false;   //	没有起始版本是异常情况，强行添加则相当于设置了起始版本，因此需要忽略
				}
				if (config.importantList.Contains(autoTagLatestVersion))
				{
					return false;   //	已标记，不用重复处理
				}
				return true;
			}
			bool ShouldCheckPackSizeAndAutoTagImportant()
			{
				if (config.auto_tag_all_important)
				{
					return false;
				}
				if (config.auto_tag_important_size <= 0)
				{
					return false;   //	未配置正确的大小，则不启用此功能
				}
				if (isEarlydownloadMode)
				{
					return false;   //	预下载非紧急更新模式有单独记录，此处忽略
				}
				if (isEarlydownloadBugfixMode)
				{
					if (config.earlydownloadBugfixList.Last() == autoTagLatestVersion)
					{
						return false;	//	最新版本是刚标记的紧急更新版本，还有机会取消继续紧急更新状态，暂时不用处理
					}
				}
				if (config.normalList.Count() == 0)
				{
					return false;   //	没有起始版本是异常情况，忽略此处理
				}
				if (config.importantList.Contains(autoTagLatestVersion))
				{
					return false;   //	已是关键版本，不用再处理
				}
				return true;
			}
			bool bCheckPackSizeAndAutoTagImportant = ShouldCheckPackSizeAndAutoTagImportant();

			void CheckPackSizeForAutoTagImportant(string packOutputDir, List<PackInfo> packList, Dictionary<string, List<PackInfo>> outputDirAndLargePacks)
			{
				if (!bCheckPackSizeAndAutoTagImportant)
				{
					return;
				}
				foreach (PackInfo info in packList)
				{
					if (info.IsBranchDiff || info.IsNotDiff || info.IsEarlydownload)
					{
						continue;
					}
					if (info.To != autoTagLatestVersion)
					{
						continue;
					}
					if (info.Size < auto_tag_important_size_in_bytes)
					{
						continue;
					}

					List<PackInfo> largePacks;
					if (outputDirAndLargePacks.TryGetValue(packOutputDir, out largePacks))
					{
						largePacks.Add(info);
					}
					else
					{
						largePacks = new List<PackInfo>();
						largePacks.Add(info);
						outputDirAndLargePacks.Add(packOutputDir, largePacks);
					}
				}
			}

			bool ShouldCheckPackFileCountAndAutoTagImportant()
			{
				if (config.auto_tag_all_important)
				{
					return false;
				}
				if (config.auto_tag_important_filecount <= 0)
				{
					return false;   //	未配置正确的大小，则不启用此功能
				}
				if (isEarlydownloadMode)
				{
					return false;   //	预下载非紧急更新模式有单独记录，此处忽略
				}
				if (isEarlydownloadBugfixMode)
				{
					if (config.earlydownloadBugfixList.Last() == autoTagLatestVersion)
					{
						return false;   //	最新版本是刚标记的紧急更新版本，还有机会取消继续紧急更新状态，暂时不用处理
					}
				}
				if (config.normalList.Count() == 0)
				{
					return false;   //	没有起始版本是异常情况，忽略此处理
				}
				if (config.importantList.Contains(autoTagLatestVersion))
				{
					return false;   //	已是关键版本，不用再处理
				}
				return true;
			}
			bool bCheckPackFileCountAndAutoTagImportant = ShouldCheckPackFileCountAndAutoTagImportant();

			void CheckPackFileCountForAutoTagImportant(string packOutputDir, List<PackInfo> packList, Dictionary<string, List<PackInfo>> outputDirAndLargeNumberPacks)
			{
				if (!bCheckPackFileCountAndAutoTagImportant)
				{
					return;
				}
				foreach (PackInfo info in packList)
				{
					if (info.IsBranchDiff || info.IsNotDiff || info.IsEarlydownload)
					{
						continue;
					}
					if (info.To != autoTagLatestVersion)
					{
						continue;
					}
					if (!info.New || info.FileCount < config.auto_tag_important_filecount)	//	不是 New 的 pack，当前没有计算 FileCount
					{
						continue;
					}

					List<PackInfo> largeNumberPacks;
					if (outputDirAndLargeNumberPacks.TryGetValue(packOutputDir, out largeNumberPacks))
					{
						largeNumberPacks.Add(info);
					}
					else
					{
						largeNumberPacks = new List<PackInfo>();
						largeNumberPacks.Add(info);
						outputDirAndLargeNumberPacks.Add(packOutputDir, largeNumberPacks);
					}
				}
			}

			bool ShouldAutoTagNormal()
			{
				if (config.auto_tag_all_important)
				{
					return false;
				}
				if (!config.auto_tag_normal)
				{
					return false;	//	未启用此功能，忽略
				}
				if (isEarlydownloadMode)
				{
					return false;   //	预下载非紧急更新模式有单独记录，此处忽略
				}
				if (isEarlydownloadBugfixMode)
				{
					if (config.earlydownloadBugfixList.Last() == autoTagLatestVersion)
					{
						return false;   //	最新版本是刚标记的紧急更新版本，还有机会取消继续紧急更新状态，暂时不用处理
					}
				}
				if (config.normalList.Count() == 0)
				{
					return false;   //	没有起始版本是异常情况，强行添加则相当于设置了起始版本，因此需要忽略
				}
				if (config.normalList.Contains(autoTagLatestVersion))
				{
					return false;   //	已标记，不用重复处理
				}
				return true;
			}

            List<PackInfo> result = new List<PackInfo>();

            int[] newHistoryList = config.historyList.Distinct().OrderBy(i => i).ToArray();
            int[] newEarlydownloadHistoryList = config.earlydownloadHistoryList;
			Dictionary<string, List<PackInfo>> newOutputDirAndLargePacks = new Dictionary<string, List<PackInfo>>();
			Dictionary<string, List<PackInfo>> newOutputDirAndLargeNumberPacks = new Dictionary<string, List<PackInfo>>();
            if (config.hasLanguage)
            {
                _curLanguageIndex = -1;

                //  验证语言参数在 svn 上是存在的
                HashSet<string> svnFullLanguageList = RetrieveSvnFullLanguageAndValidate(assetsUri, SvnRevision.Head);
				if (compareAssetsUri != null)
				{
					//	语言参数在灰度服svn上也应存在，否则会在切换包中生成删除所有语言资源的操作
					RetrieveSvnFullLanguageAndValidate(compareAssetsUri, new SvnRevision(compareAssetsSwitchRevision));
				}

                //  依次对所有指定的语言打包、收集版本列表结束时提交
                for (int index = 0; index < languageCount; ++index)
                {
                    _curLanguageIndex = index;
                    List<PackInfo> packList = PackCurrent(svnFullLanguageList);
                    if (config.commit_history)
                    {
                        MergeHistoryList(ref newHistoryList, packList);
                        if (hasEarlydownload)
                        {
                            MergeEarlydownloadHistoryList(ref newEarlydownloadHistoryList, packList);
                        }
                    }
					CheckPackSizeForAutoTagImportant(output, packList, newOutputDirAndLargePacks);
					CheckPackFileCountForAutoTagImportant(output, packList, newOutputDirAndLargeNumberPacks);
                    result.AddRange(packList);
                }
                _curLanguageIndex = -1;
            }
            else
            {
                _curLanguageIndex = -1;
                List<PackInfo> packList = PackCurrent(null);
                if (config.commit_history)
                {
                    MergeHistoryList(ref newHistoryList, packList);
                    if (hasEarlydownload)
                    {
                        MergeEarlydownloadHistoryList(ref newEarlydownloadHistoryList, packList);
                    }
                }
				CheckPackSizeForAutoTagImportant(output, packList, newOutputDirAndLargePacks);
				CheckPackFileCountForAutoTagImportant(output, packList, newOutputDirAndLargeNumberPacks);
                result = packList;
            }

			if (File.Exists(versionPath))
			{
				string versionMd5 = ComputeFileHash(versionPath);
				if (versionMd5 != null)
				{
					OutputLog(string.Format("version.txt当前状态: MD5[{0}], LastWriteTime[{1}], FullPath[{2}]", versionMd5, File.GetLastWriteTime(versionPath).ToString(), Path.GetFullPath(versionPath)));
				}
			}

            if (config.commit_history)
            {
                config.historyList = newHistoryList;
                if (hasEarlydownload)
                {
                    config.earlydownloadHistoryList = newEarlydownloadHistoryList;
                }

                config.SaveAndCommitHistory("ResourceUpdatePack auto add and commit history");
            }

			if (ShouldAutoTagAllImportant())
			{
				//	再次检查所有 packList ，确认此版本是有效版本
				bool bLatestVersionUsed = false;
				foreach (PackInfo info in result)
				{
					if (info.IsBranchDiff || info.IsNotDiff || info.IsEarlydownload)
					{
						continue;
					}
					if (info.To != autoTagLatestVersion)
					{
						continue;
					}
					bLatestVersionUsed = true;
					break;
				}
				if (bLatestVersionUsed)
				{
					if (config.AddImportant(autoTagLatestVersion))
					{
						config.SaveAndCommitTags("ResourceUpdatePack auto add and commit important");
						OutputLog(string.Format("版本号:{0} 已标记为关键版本!", autoTagLatestVersion));
					}
				}
			}
			if (bCheckPackSizeAndAutoTagImportant && newOutputDirAndLargePacks != null && newOutputDirAndLargePacks.Count > 0 && !config.importantList.Contains(autoTagLatestVersion))
			{
				foreach (var item in newOutputDirAndLargePacks)
				{
					foreach (var largePack in item.Value)
					{
						OutputLog(string.Format("发现较大更新包: {0}/{1}, 大小为 {2} >= {3}({4}MB)!", item.Key, largePack.FullName, largePack.Size, auto_tag_important_size_in_bytes, config.auto_tag_important_size));
					}
				}

				config.AddImportant(autoTagLatestVersion);
				config.SaveAndCommitTags("ResourceUpdatePack auto add and commit important by pack size");
				OutputLog(string.Format("版本号:{0} 已标记为关键版本!", autoTagLatestVersion));
			}
			if (bCheckPackFileCountAndAutoTagImportant && newOutputDirAndLargeNumberPacks != null && newOutputDirAndLargeNumberPacks.Count > 0 && !config.importantList.Contains(autoTagLatestVersion))
			{
				foreach (var item in newOutputDirAndLargeNumberPacks)
				{
					foreach (var pack in item.Value)
					{
						OutputLog(string.Format("发现文件较多更新包: {0}/{1}, 添加修改的文件总数为 {2} >= {3}!", item.Key, pack.FullName, pack.FileCount, config.auto_tag_important_filecount));
					}
				}

				config.AddImportant(autoTagLatestVersion);
				config.SaveAndCommitTags("ResourceUpdatePack auto add and commit important by filecount");
				OutputLog(string.Format("版本号:{0} 已标记为关键版本!", autoTagLatestVersion));
			}
			if (ShouldAutoTagNormal())
			{
				//	再次检查所有 packList ，确认此版本是有效版本
				bool bLatestVersionUsed = false;
				foreach (PackInfo info in result)
				{
					if (info.IsBranchDiff || info.IsNotDiff || info.IsEarlydownload)
					{
						continue;
					}
					if (info.To != autoTagLatestVersion)
					{
						continue;
					}
					bLatestVersionUsed = true;
					break;
				}
				if (bLatestVersionUsed)
				{
					if (config.AddNormal(autoTagLatestVersion))
					{
						config.SaveAndCommitTags("ResourceUpdatePack auto add and commit normal");
						OutputLog(string.Format("版本号:{0} 已标记为普通版本!", autoTagLatestVersion));
					}
				}
			}

            return result;
        }
        private List<PackInfo> PackCurrent(HashSet<string> svnFullLanguageList)
        {
			string packTag = GetPackTag(null);

            List<int> normalList = config.normalList.ToList();
            List<int> importantList = config.importantList.ToList();
            List<int> historyList = config.historyList.ToList();

            AddLatestRevision(normalList, importantList);

            List<int> historyAndTagsList = normalList.Union(historyList).Distinct().OrderBy(i => i).ToList();
            List<PackInfo> packList = new List<PackInfo>();
            if (normalList.Count == 0)
            {
                normalList.Add(logList.First().Revision);
            }

            for (int i = 1; i < normalList.Count; ++i)
            {
                int from = normalList[i - 1];
                int to = normalList[i];

                //现在 from-to 为相邻版本，把 to 修正到下一个关键版本(或最新版本)
				to = normalList[normalList.Count - 1];
				foreach (int iv in importantList)
				{
					if (iv > from)
					{
						to = iv;
						break;
					}
				}

                PackInfo info = new PackInfo
                {
                    Name = string.Format("{0}{1}-{2}", prefix, from, to),
                    From = from,
                    To = to,
                    New = true,
                };
                if (hasEarlydownload)
                {
                    if (config.IsEarlydownloadPack(from, to))
                    {
                        info.IsEarlydownload = true;
                    }
					else if (config.IsBugfixPack(from, to))
					{
						info.IsBugfix = true;
					}
                }
                packList.Add(info);
            }

            //  生成此语言的完整版本包，玩家首次下载此语言包时选择此包
            if (isCurrentLanguageOthers)
            {
                int to = normalList.Last();
                if (hasEarlydownload)
                {
                    to = config.earlydownloadList[0];   //  预下载模式特殊，只能到预下载起作用前的版本
                }
                PackInfo info = new PackInfo
                {
                    To = to,
                    New = true,
                };
                info.IsNotDiff = true;
                info.Name = string.Format("{0}{1}-{2}", prefix, info.From, info.To);
                packList.Add(info);
            }

            Directory.CreateDirectory(output);
            string[] zipFiles = Directory.GetFiles(output, string.Format("*.{0}", extension), SearchOption.TopDirectoryOnly);

			if (compareAssetsUri != null && (!config.hasLanguage || isCurrentLanguageBase)) //  制作基础资源在两个服务器间的切换包
            {
                int to = compareAssetsSwitchRevision;
                int from = normalList.Last();
                if (hasEarlydownload)
                {
                    from = config.earlydownloadList[0];     //  预下载模式特殊，只能到预下载起作用前的版本
                }
                PackInfo info = new PackInfo {
                    From = from,
                    To = to,
                    New = true,
                    IsBranchDiff = true,
                };

                info.Name = string.Format("{0}-{1}{2}-{3}", prefix, config.comparePrefix, from, info.To);
                packList.Add(info);
            }

			StringBuilder versionTxtBuilder = new StringBuilder();
            List<string> OldVersionLines = new List<string>();

            versionTxtBuilder.AppendLine("{");
			versionTxtBuilder.Append(MakeFirstVersionLine(normalList, config.earlydownloadList, config.earlydownloadBugfixList));

            if (hasEarlydownload)
            {
                OldVersionLines.Add(MakeEarlyownloadFirstVersionLine(normalList, config.earlydownloadList, config.earlydownloadBugfixList));
            }
            else
            {
                OldVersionLines.Add(string.Format("Version:    {0}/{1}", normalList[normalList.Count - 1], normalList[0]));
            }
            OldVersionLines.Add(string.Format("Project:    {0}", project));
            int OldVersionLinesIndex = OldVersionLines.Count;

            foreach (PackInfo info in packList)
            {
                if (force)
                {
                    Pack(info, historyAndTagsList, config.ignoredDict);
                }
                else
                {
                    bool find = false;
                    foreach (string file in zipFiles)
                    {
                        string name = Path.GetFileName(file);
                        if (name.StartsWith(info.Name + "."))
                        {
                            OutputLog(string.Format("正在校验已存在的更新包: {0}", name), LogMode.Gui);
                            FileInfo fileInfo = new FileInfo(file);
							info.Size = fileInfo.Length;
							if ((int)info.Size < 0)
							{
								OutputLog(string.Format("文件 {0} 已超过2GB，当前大小为 {1} 字节，{2:N2} GB", file, info.Size, ((double)info.Size) / 1024 / 1024 / 1024));
							}
							info.Hash = ComputeFileHash(file);
                            info.FullName = string.Format("{0}.{1}.{2}", info.Name, info.Hash.Substring(0, 6), extension);
                            if (name != info.FullName)
                            {
                                OutputLog(string.Format("更新包校验失败: {0}", name));
                                File.Delete(file);
                                continue;
                            }

                            find = true;
                            info.New = false;
							info.IsEmpty = IsPackEmptyByUncompress7z(file);
                            break;
                        }
                    }

                    if (!find)
                    {
                        Pack(info, historyAndTagsList, config.ignoredDict);
                    }
                }

				string Masks = "";
				void AddMask(string mask)
				{
					if (Masks.Length > 0)
					{
						Masks += ";";
					}
					Masks += mask;
				}
				if (info.IsEmpty)
				{
					OutputLog(string.Format("更新包被检测为空包: {0}，当前大小为 {1} 字节", info.FullName, info.Size));
					AddMask("empty");	//	标记为空包，可以跳过下载直接修改版本号
				}
				if (info.IsBranchDiff)
				{
					AddMask("switch");	//	标记为切换包
				}
                if (Masks.Length > 0) {
					versionTxtBuilder.AppendFormat("{{ '{0}', {1}, {2}, {3}, '{4}', '{5}' }},\n", info.FullName, info.Size, info.From, info.To, info.Hash, Masks);
                } else {
					versionTxtBuilder.AppendFormat("{{ '{0}', {1}, {2}, {3}, '{4}' }},\n", info.FullName, info.Size, info.From, info.To, info.Hash);
                }

                if (info.IsBranchDiff)
                {
                    OldVersionLines.Insert(OldVersionLinesIndex, string.Format("{0}-{1}-{2}    {3}    {4}", config.comparePrefix, info.From, info.To, info.Hash, info.Size));
                }
                else
                {
                    OldVersionLines.Add(string.Format("{0}-{1}    {2}    {3}", info.From, info.To, info.Hash, info.Size));
                }
                
            }
			versionTxtBuilder.AppendLine("}");

			if (delete)
			{
				OutputLog(string.Format("[6]{0} 正在删除多余文件", packTag));
				string[] entries = Directory.GetFileSystemEntries(output);
				foreach (string entry in entries)
				{
					if (Directory.Exists(entry))
					{
						Directory.Delete(entry, true);
					}
					else
					{
						string name = Path.GetFileName(entry);
						if (packList.Find((e) => e.FullName == name) == null)
						{
							File.Delete(entry);
						}
					}
				}

				if (!config.hasLanguage)
				{
					if (Directory.Exists(outputLangRoot))
					{
						Directory.Delete(outputLangRoot, true);
					}
				}
				else if (isCurrentLanguageBase)
				{
					if (Directory.Exists(outputLangRoot))
					{
						foreach (string entry in Directory.GetFileSystemEntries(outputLangRoot))
						{
							if (!IsOutputEntryInAllowedLanguageList(entry, outputLangRoot, svnFullLanguageList))
							{
								if (Directory.Exists(entry))
								{
									Directory.Delete(entry, true);
								}
							}
						}
					}
				}
			}

            OutputLog(string.Format("[7]{0} 正在更新 version.txt", packTag));
            if (useOriginVertionFormat)
            {
                SaveVersion(OldVersionLines, svnFullLanguageList);
            }
            else
            {
                File.WriteAllText(versionPath, versionTxtBuilder.ToString());
            }
			    

            if (!config.gui)
            {
                int nameLength = 5, sizeLength = 5;
                foreach (PackInfo info in packList)
                {
                    if (info.FullName.Length > nameLength)
                    {
                        nameLength = info.FullName.Length;
                    }

                    string sizeStr = info.Size.ToString();
                    if (sizeStr.Length > sizeLength)
                    {
                        sizeLength = sizeStr.Length;
                    }
                }

                StringBuilder sb = new StringBuilder();
                string format = "{0,-" + nameLength + "} {1," + sizeLength + "}  {2}";
                foreach (PackInfo info in packList)
                {
                    if (info.New)
                    {
                        Console.WriteLine(format, info.FullName, info.Size, info.Hash);
                        sb.AppendLine(info.FullName);
                    }
                }
                if (config.@packlist)
                {
                    string filename = packlistPath;
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                    using (FileStream fs = File.OpenWrite(filename))
                    {
                        byte[] buff = Encoding.UTF8.GetBytes(sb.ToString());
                        fs.Write(buff, 0, buff.Length);
                    }
                }
            }

            return packList;
        }

		string GetPackTag(PackInfo info)
		{
			string packTag = "";
			if (config.hasLanguage)
			{
				packTag = string.Format("[{0}]", currentLanguage);
			}
			if (info != null)
			{
				if (info.IsBranchDiff)
				{
					packTag += "[preview-switch]";
				}
				else
				{
					if (info.IsEarlydownload)
					{
						packTag += "[earlydownload]";
					}
					else if (info.IsBugfix)
					{
						packTag += "[bugfix]";
					}
				}
			}
			return packTag;
		}

		int RetrieveCompareAssetsSwitchRevision()
		{
			if (compareAssetsUri == null)
			{
				throw new Exception("错误的调用:当前未配置切换服务器资源n地址");
			}
			if ((config.enable_ingameupdate == true) != (config.compare_enable_ingameupdate == true))
			{
				throw new Exception(string.Format("无法制作服务器切换包: 切换前{0}小包、而切换后{1}小包!", config.enable_ingameupdate ? "使用了" : "没使用", config.compare_enable_ingameupdate ? "使用了" : "没使用"));
			}

			int to = 0;
			{
				int[] compareUriEarlydownloadList;
				int[] compareUriEarlydownloadBugfixList;
				ExportCompareUriEarlydownloadTags(out compareUriEarlydownloadList, out compareUriEarlydownloadBugfixList);
				if (compareUriEarlydownloadList.Count() > 0)
				{
					to = compareUriEarlydownloadList[0];   //  预下载模式特殊，只能到预下载起作用前的版本
				}
			}
			if (to >= 0)
			{
				Int32[] compareHistoryList = ExportCompareUriLatestHistoryList();
				if (compareHistoryList.Length > 0)
				{
					if (to == 0 || to > compareHistoryList.Last())  //	预下载模式没打过包时忽略
					{
						to = compareHistoryList.Last();
					}
				}
			}
			if (to == 0)
			{
				to = GetLatestRevision(compareAssetsUri);
			}
			return to;
		}

		private HashSet<string> RetrieveSvnFullLanguageList(string uri, SvnRevision revision)
        {
            HashSet<string> LanguageList = new HashSet<string>();
            SvnListArgs listArgs = new SvnListArgs
            {
                Depth = SvnDepth.Infinity,
                Revision = revision,
            };
            SvnTarget listTarget = MakeSvnTrueTarget(client, uri, revision);
            if (!client.List(listTarget, listArgs, (sender, eventArgs) =>
            {
                if (eventArgs.Entry.NodeKind != SvnNodeKind.Directory)
                {
                    return;
                }

                String path = eventArgs.Path;
                string languageName = RetrieveLanguageName(path);
                if (languageName != null)
                {
                    LanguageList.Add(languageName);
                }
            }))
            {
                throw new Exception(String.Format("failed to List"));
            }
            return (LanguageList.Count > 0) ? LanguageList : null;
        }

        private bool IsOutputEntryInAllowedLanguageList(string outputEntry, string entryRoot, HashSet<string> svnFullLanguageList)
        {
            if (outputEntry != null)
            {
                string subEntry = outputEntry.Substring(entryRoot.Length);
                subEntry = subEntry.TrimStart(PathSeperators);
                int seperatorIndex = subEntry.IndexOfAny(PathSeperators);
                string directSubDirName = (seperatorIndex >= 0) ? subEntry.Substring(0, seperatorIndex) : (Directory.Exists(outputEntry) ? subEntry : null);
                return IsInAllowedLanguageList(directSubDirName, svnFullLanguageList);
            }
            return false;
        }

        private bool IsOutputEntryVersionPath(string outputEntry)
        {
            string outputEntryUnified = UnifyPath(outputEntry);
            string versionPathUnified = UnifyPath(versionPath);
            return outputEntryUnified.Equals(versionPathUnified);
        }

        private bool IsInAllowedLanguageList(string candidate, HashSet<string> svnFullLanguageList)
        {
            if (candidate != null)
            {
                if (!IsInSvnLanguageList(candidate, svnFullLanguageList))
                {
                    return false;
                }
                if (config.hasFullLanguageList)
                {
                    return config.IsInFullLanguageList(candidate);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

		private HashSet<string> RetrieveSvnFullLanguageAndValidate(string uri, SvnRevision revision)
		{
			HashSet<string> svnFullLanguageList = RetrieveSvnFullLanguageList(uri, revision);

			for (int i = 0; i < languageCount; ++i)
			{
				string lang = LanguageAt(i);
				if (IsLanguageOthers(lang))
				{
					if (!IsInSvnLanguageList(lang, svnFullLanguageList))
					{
						throw new Exception(string.Format("语言参数:{0} 在 svn {1} 版本 {2} 上不存在，存在的语言列表为:{3}", lang, uri, revision.Revision, string.Join(",", svnFullLanguageList.ToArray())));
					}
				}
			}

			return svnFullLanguageList;
		}

        private bool IsInSvnLanguageList(string candidate, HashSet<string> svnFullLanguageList)
        {
            if (candidate != null && svnFullLanguageList != null)
            {
                return svnFullLanguageList.Contains(candidate);
            }
            return false;
        }

        private void SaveVersion(List<string> versionLines, HashSet<string> svnFullLanguageList)
        {
            if (!config.hasLanguage)
            {
                StringBuilder builder = new StringBuilder();
                foreach (string line in versionLines)
                {
                    builder.AppendLine(line);
                }
                File.WriteAllText(versionPath, builder.ToString());
            }
            else
            {
                string rootName = "root";
                if (IsVersionListInvalid())
                {
                    XDocument xdoc = new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement(rootName, CreateVersionElement(versionLines)));
                    xdoc.Save(versionPath);
                }
                else
                {
                    XDocument xdoc = XDocument.Load(versionPath);
                    XElement root = xdoc.Element(rootName);

                    XElement newRoot = new XElement(rootName);
                    XDocument newDoc = new XDocument(new XDeclaration("1.0", "utf-8", null), newRoot);

                    //  添加 base 标签
                    if (isCurrentLanguageBase)
                    {
                        //  当前是 base，直接创建新的
                        newRoot.Add(CreateVersionElement(versionLines));
                    }
                    else
                    {
                        //  当前是其它语言，从原xml中拷贝
                        XElement oldBaseVersionNode = root.Element(GetBaseVersionTag());
                        if (oldBaseVersionNode != null)
                        {
                            newRoot.Add(oldBaseVersionNode);
                        }
                    }

                    //  收集其它语言标签
                    Dictionary<string, XElement> otherLanguages = new Dictionary<string, XElement>(StringComparer.Ordinal);
                    if (isCurrentLanguageBase)
                    {
                        //  覆盖 base 分支的版本列表时、只保留有效的多语言版本列表
                        foreach (XElement element in root.Elements())
                        {
                            string name = element.Name.ToString();
                            if (name.Equals(GetBaseVersionTag()))
                            {
                                continue;
                            }
                            if (!IsInAllowedLanguageList(name, svnFullLanguageList))
                            {
                                continue;
                            }
                            if (otherLanguages.ContainsKey(name))
                            {
                                throw new Exception(string.Format("version.txt 中包含重复的语言名称: {0}", name));
                            }
                            otherLanguages.Add(name, element);
                        }
                    }
                    else
                    {
                        //  添加当前语言
                        otherLanguages.Add(GetCurrentVersionTag(), CreateVersionElement(versionLines));

                        //  拷贝其它语言
                        foreach (XElement element in root.Elements())
                        {
                            string name = element.Name.ToString();
                            if (name.Equals(GetBaseVersionTag()))
                            {
                                continue;
                            }
                            if (otherLanguages.ContainsKey(name))
                            {
                                continue;
                            }
                            otherLanguages.Add(name, element);
                        }
                    }

                    //  按顺序添加其它语言标签
                    string[] otherLanguageNames = otherLanguages.Keys.ToArray();
                    Array.Sort(otherLanguageNames, string.CompareOrdinal);
                    foreach (string name in otherLanguageNames)
                    {
                        XElement element = otherLanguages[name];
                        newRoot.Add(element);
                    }

                    //  存储到文件(去掉BOM)
                    using (var writer = new System.Xml.XmlTextWriter(versionPath, new UTF8Encoding(false)))
                    {
                        writer.Formatting = System.Xml.Formatting.Indented;
                        newDoc.Save(writer);
                    }
                }
            }
        }

        private bool IsVersionListInvalid()
        {
            bool isInvalid = false;
            if (!File.Exists(versionPath))
            {
                isInvalid = true;
            }
            else
            {
                try
                {
                    XDocument xdoc = XDocument.Load(versionPath);
                }
                catch (Exception e)
                {
                    OutputLog(string.Format("非法的 xml 文件：'{0}', Message:'{1}'", versionPath, e.Message));
                    isInvalid = true;
                }
            }
            return isInvalid;
        }

        private XElement CreateVersionElement(List<string> versionLines)
        {
            XElement currentVersionRoot = new XElement(GetCurrentVersionTag());
            foreach(string line in versionLines)
            {
                currentVersionRoot.Add(new XElement("line", line));
            }
            return currentVersionRoot;
        }

        private string GetCurrentVersionTag()
        {
            if (!config.hasLanguage)
                throw new Exception("GetCurrentVersionTag, invalid call");
            return isCurrentLanguageBase ? GetBaseVersionTag() : currentLanguage;
        }

        private string GetBaseVersionTag()
        {
            return "base";
        }

        public void CalcDiffList(Int32 from, Int32 to)
        {
            if(to == -1)
            {
                to = GetLatestRevision(config.uri);
            }

            Dictionary<string, DiffSummaryItem> diffs = SvnDiff(assetsUri, from, assetsUri, to);
            
            CalcDiffList(diffs, string.Format("{0}-{1}.diff.txt", from, to));
            OutputLog(string.Format("版本{0}到版本{1}差异文件共{2}个。", from, to, diffs.Count));
        }

        private void CalcDiffList(Dictionary<string, DiffSummaryItem> diffs, string filename)
        {
            using (FileStream fs = File.OpenWrite(filename))
            {
                fs.SetLength(0);
                foreach (var item in diffs)
                {
                    String line = item.Key + "," + item.Value.DiffKind.ToString() + "\r\n";
                    byte[] buff = Encoding.UTF8.GetBytes(line);
                    fs.Write(buff, 0, buff.Length);
                }
            }
        }

        private void ProcessChangesByLanguage(PackInfo info, Dictionary<string, DiffSummaryItem> changes)
        {
			if (!config.hasLanguage || isCurrentLanguageOthers || config.baseHasLanguage)
			{
				foreach (var item in changes.Where(pair => !CanExportPathByLanguage(pair.Value.Path)).ToList())
				{
					changes.Remove(item.Key);
				}
			}
			else
			{
				//	多语言基础资源目录内不含任何多语言内容时（故意被完整剔除，目的是可以做成不同内置语言的安装包、同时共享同一个更新服务器）
				//	此时，安装包一般是不含语言的基础资源+额外强行拷贝进去的某语言资源做成；
				//	这种情况下，如果不特殊处理，则无法通过文件系统的删除标记功能从安装包内伪删除某资源（这个功能对小包尤其重要），因为标记删除只能在基础资源的更新时使用（语言资源更新时若使用标记删除则将导致语言资源变成非语言资源时不可访问）
				//	因此，最终此时需要做特殊处理，目的是将多语言下的所有'删除'记录'转换'并保留到基础资源的更新包内

				var langDeletes = new Dictionary<string, DiffSummaryItem>();
				foreach (var item in changes.Where(pair => !CanExportPathByLanguage(pair.Value.Path)).ToList())
				{
					changes.Remove(item.Key);

					if (item.Value.DiffKind == SvnDiffKind.Deleted)
					{
						string lang = RetrieveLanguageName(item.Key);
						if (lang != null && config.IsInFullLanguageList(lang))
						{
							langDeletes.Add(item.Key, item.Value);
						}
					}
				}
				if (langDeletes.Count() > 0)
				{
					//	计算目标版本基础资源文件列表、有小包时排除小包文件
					List<string> baseFiles = new List<string>();
					if (info.IsBranchDiff)
					{
						if (config.enable_ingameupdate)
						{
							List<string> alist, blist;
							LoadABFileList(compareAssetsUri, config.compare_ingameupdate_dir, info.To, out alist, out blist);
							baseFiles = alist;
						}
						else
						{
							baseFiles = GetFilesRevision(compareAssetsUri, info.To, ((path) => PackageListContainsForPath(path))).Keys.ToList();
						}
					}
					else
					{
						if (config.enable_ingameupdate)
						{
							List<string> alist, blist;
							LoadABFileList(assetsUri, config.ingameupdate_dir, info.To, out alist, out blist);
							baseFiles = alist;
						}
						else
						{
							baseFiles = GetFilesRevision(assetsUri, info.To, ((path) => PackageListContainsForPath(path))).Keys.ToList();
						}
					}
					baseFiles = baseFiles.Where(path => !IsPathUnderDirectory(path, L10N)).ToList();

					var langDeletesToAddToChanges = new Dictionary<string, DiffSummaryItem>();
					foreach (var item in langDeletes.ToList())
					{
						string basePath = RemoveLanguagePrefixForPath(item.Key);
						if (!baseFiles.Contains(basePath, StringComparer.OrdinalIgnoreCase))
						{
							string tempKey = UnifyPath(basePath);
							DiffSummaryItem existedItem;
							if (langDeletesToAddToChanges.TryGetValue(tempKey, out existedItem))
							{
								if (existedItem.Revision < item.Value.Revision)
								{
									existedItem.Path = item.Key;
									existedItem.Revision = item.Value.Revision;
								}
							}
							else
							{
								langDeletesToAddToChanges.Add(tempKey, new DiffSummaryItem
								{
									Path = item.Key,
									Revision = item.Value.Revision,
									DiffKind = SvnDiffKind.Deleted
								});
							}
							OutputLog(string.Format("版本{0}对资源'{1}'的删除记录已收集", item.Value.Revision, item.Key));
						}
						else
						{
							OutputLog(string.Format("版本{0}对资源'{1}'的删除记录已忽略(因基础资源中仍存在)", item.Value.Revision, item.Key));
						}
					}

					//	清除其它同路径记录，以确保后续添加的删除记录生效
					var ignoredPaths = changes.Where(change => langDeletesToAddToChanges.ContainsKey(UnifyPath(change.Key))).Select(change => change.Key).ToArray();
					foreach(string ignoredPath in ignoredPaths.ToArray())
					{
						changes.Remove(ignoredPath);
					}

					//	将语言目录下的删除记录、同步到基础资源的更新包上，确保能删除安装包内可能存在的语言资源
					foreach (var item in langDeletesToAddToChanges.ToList())
					{
						string basePath = RemoveLanguagePrefixForPath(item.Value.Path);
						changes.Add(basePath, new DiffSummaryItem
						{
							Path = basePath,
							Revision = item.Value.Revision,
							DiffKind = SvnDiffKind.Deleted
						});
						OutputLog(string.Format("版本{0}对资源'{1}'的删除记录已转换成对'{2}'的删除操作并作用至当前更新包", item.Value.Revision, item.Value.Path, basePath));
					}
				}
			}
		}

		private void CountChanges(Dictionary<string, DiffSummaryItem> changes, PackInfo info)
		{
			info.FileCount = changes.Where(pair => (pair.Value.DiffKind == SvnDiffKind.Added || pair.Value.DiffKind == SvnDiffKind.Modified)).ToArray().Count();
			info.IsEmpty = changes.Count() == 0;
		}

        private bool PackageListContainsForPath(string path)
        {
            if (config.afile_cfg == null)
            {
                return true;
            }

            if (config.afile_cfg.PackageListContainsForPath(path))
            {
                return true;
            }
            else
            {
                string unifiedPath = UnifyPath(path);
                if (!unifiedPath.StartsWith(L10N + '/', StringComparison.OrdinalIgnoreCase))
                {
                    return false;   //  无论目录或文件，均应只允许以 L10N/ 开头
                }
                int iFirstSep = L10N.Length;
                if (unifiedPath.EndsWith("/"))
                {
                    //  目录
                    if (unifiedPath.Length == L10N.Length+1)
                    {
                        return true;    //  目录名是 L10N/
                    }
                    int iSecondSep = unifiedPath.IndexOf('/', iFirstSep + 1);
                    if (unifiedPath.Length == iSecondSep+1)
                    {
                        return true;    //  目录名是 L10N/{language}/ ，假定 {language} 都是合法的语言名称
                    }
                    string subPathUnderLanguage = unifiedPath.Substring(iSecondSep + 1);
                    return config.afile_cfg.PackageListContainsForPath(subPathUnderLanguage);
                }
                else
                {
                    //  文件
                    int iSecondSep = unifiedPath.IndexOf('/', iFirstSep + 1);
                    if (iSecondSep < 0)
                    {
                        return false;
                    }
                    string subPathUnderLanguage = unifiedPath.Substring(iSecondSep + 1);
                    return config.afile_cfg.PackageListContainsForPath(subPathUnderLanguage);
                }
            }
        }


        private Boolean CanExportPathByLanguage(string path)
        {
            if (isCurrentLanguageOthers)
            {
                return IsPathUnderDirectory(path, L10N, currentLanguage);
            }
            else
            {
                return !IsPathUnderDirectory(path, L10N);
            }
        }

        private string RemoveLanguagePrefixForPath(string path)
        {
            if (IsPathUnderDirectory(path, L10N))
            {
                if (path.Length > L10N.Length)
                {
                    string subPath = path.Substring(L10N.Length);
                    subPath = subPath.TrimStart(PathSeperators);
                    if (subPath.Length > 0)
                    {
                        int seperatorIndex = subPath.IndexOfAny(PathSeperators);
                        if (seperatorIndex >= 0)
                        {
                            return subPath.Substring(seperatorIndex + 1);
                        }
                    }
                }
                throw new Exception(string.Format("Cann't remove language prefix for unexpected path '{0}'", path));
            }
            else
            {
                return path;
            }
        }

        private static string RetrieveLanguageName(string path)
        {
            if (IsPathUnderDirectory(path, L10N))
            {
                string subPath = path.Substring(L10N.Length);
                subPath = subPath.TrimStart(PathSeperators);
                if (subPath.Length > 0)
                {
                    int seperatorIndex = subPath.IndexOfAny(PathSeperators);
                    string languageName = (seperatorIndex >= 0) ? subPath.Substring(0, seperatorIndex) : subPath;
                    return (languageName.Length > 0) ? languageName : null;
                }
            }
            return null;
        }

        private static Boolean IsPathUnderDirectory(string path, string testDirName)
        {
            //  If starts with testDirName
            if (!path.StartsWith(testDirName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            //  If path seperator follows
            if (path.Length == testDirName.Length)
            {
                return true;
            }
            else
            {
                char c = path[testDirName.Length];
                return IsPathSeperator(c);
            }
        }

        private static Boolean IsPathUnderDirectory(string path, string testDirName, string testSubDirName)
        {
            if (!IsPathUnderDirectory(path, testDirName))
            {
                return false;
            }

            //  Without path seperator after testDirName, so impossible to be under testSubDirName
            if (path.Length <= testDirName.Length)
            {
                return false;
            }

            //  If follows with testSubDirName
            if (0 != String.Compare(path, testDirName.Length + 1, testSubDirName, 0, testSubDirName.Length, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            //  If path seperator follows
            if (path.Length == testDirName.Length + 1 + testSubDirName.Length)
            {
                return true;
            }
            else
            {
                char c2 = path[testDirName.Length + 1 + testSubDirName.Length];
                return IsPathSeperator(c2);
            }
        }

        private static bool IsPathSeperator(char c)
        {
            foreach (char sep in pathSeperators)
            {
                if (c == sep)
                {
                    return true;
                }
            }
            return false;
        }

        private static char[] PathSeperators
        {
            get { return pathSeperators; }
        }

        private static string L10N = "L10N";
        private static char[] pathSeperators = { '/', '\\' };

    }
}
