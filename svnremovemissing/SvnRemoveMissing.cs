using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharpSvn;
using System.Collections.ObjectModel;

namespace svnremovemissing
{
	class SvnRemoveMissing
	{
		public static void DoRemove(String path, bool verbose = false)
		{
			SvnClient svnClient = new SvnClient();
			if (!IsWorkingCopy(svnClient, path))
				throw new Exception("path is not svn working folder: " + path);

            if (verbose) {
                Console.Out.WriteLine("remove unversioned files and directorys at {0}", path);
            }

            Dictionary<String, bool> deleteFlags = new Dictionary<string, bool>();
            Collection<SvnStatusEventArgs> statuses;
            if(svnClient.GetStatus(path, out statuses)) {
                foreach(SvnStatusEventArgs args in statuses) {
                    //加GetPathMode排除windows大小写导致错误而无法提交svn
                    if (args.LocalNodeStatus == SvnStatus.Missing ){//&& 0 == GetPathMode(args.Path)) {
                        String ParentPath = Path.GetDirectoryName(args.Path);
                        bool flagParent;
                        deleteFlags.TryGetValue(ParentPath, out flagParent);

                        bool flag;
                        deleteFlags.TryGetValue(args.Path, out flag);
                        if (!flagParent && !flag) {
                            svnClient.Delete(args.Path);
                            deleteFlags[args.Path] = true;
                            if (verbose) {
                                Console.Out.WriteLine("remove unversioned files or directorys {0}", args.Path);
                            }
                        }
                    }
                }
            }
		}

		private static Boolean IsWorkingCopy(SvnClient svnClient, String path)
		{
			return svnClient.GetUriFromWorkingCopy(path) != null;
		}

        private static int GetPathMode(String path)
        {
            if (File.Exists(path))
                return 1;
            else if (Directory.Exists(path))
                return 2;
            else
                return 0;
        }
	}
}
