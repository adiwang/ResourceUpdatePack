using System;
using System.Collections.Generic;
using System.Text;

namespace svnremovemissing
{
	class Program
	{
		static Int32 Main(string[] args)
		{
			String arg;
			if (args.Length > 0)
				arg = args[0];
			else
				arg = ".";

            bool verbose = false;
            if (args.Length > 1)
                verbose = args[1] == "-verbose";
            //arg = @"F:\Workspace\trunk\client\Tools\ContentSVN";
            //verbose = true;

            try
			{
				SvnRemoveMissing.DoRemove(arg, verbose);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
				return -1;
			}

			return 0;
		}
	}
}
