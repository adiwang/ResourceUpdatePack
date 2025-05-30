using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ZLUtils
{
    public sealed class ConsoleUtil
    {
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(int proID);

        public static void SetLogToFile(bool b)
        {
            logFile = b;
        }
        public static void SetPrintPrefix(bool b)
        {
            printPrefix = b;
        }
        public static void SetLogFileInit(bool b)
        {
            LogFileInited = b;
        }

        static bool logFile = true;
        static bool printPrefix = true;
        const string warning_prefix = "警告：";
        const string error_prefix = "错误：";
        const string stacktrace_title = "StackTrace:";

        static object lockobj = new object();
        static bool LogFileInited = false;
        static bool allocConsole = false;
        static bool allocConsoled = false;

        static string _moduleName;
        static string moduleName
        {
            get
            {
                if (_moduleName != null)
                    return _moduleName;
                _moduleName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
                return _moduleName;
            }
        }
        static string logFileName
        {
            get { return moduleName + ".log"; }
        }

        private static void _Begin()
        {
            lock (lockobj)
            {
                if (allocConsoled) return;
                if (allocConsole) AllocConsole();
                allocConsoled = true;
                if (LogFileInited)
                    return;
                string logContent = string.Format("==================={0:G}: LogBegin ===================\n", System.DateTime.Now);
                if (logFile)
                {
                    if (File.Exists(logFileName))
                        File.Delete(logFileName);

                    using (System.IO.StreamWriter sw = System.IO.File.AppendText(logFileName))
                    {
                        sw.WriteLine(logContent);
                    }
                }
                else
                {
                }
                LogFileInited = true;
            }
        }
        public static void BeginConsole(bool alloc = true)
        {
            allocConsole = alloc;
            _Begin();
        }
        private static void _Finish()
        {
            lock (lockobj)
            {
                if (!allocConsoled) return;
                string logContent = string.Format("\n==================={0:G}: LogEnd ===================", System.DateTime.Now);
                if (logFile)
                {
                    using (System.IO.StreamWriter sw = System.IO.File.AppendText(logFileName))
                    {
                        sw.WriteLine(logContent);
                    }
                }
                else
                {

                }
                FreeConsole();
                allocConsole = false;
                LogFileInited = false;
                allocConsoled = false;
            }
        }
        public static void FinishConsole()
        {
            _Finish();
        }

        private static void WriteLogFile(string filename, string str)
        {
            lock (lockobj)
            {
                if (!logFile) return;
                using (System.IO.StreamWriter sw = System.IO.File.AppendText(filename))
                {
                    string logContent = string.Format("{0:G}: {1}", System.DateTime.Now, str);
                    sw.WriteLine(logContent);
                }
            }
        }

        public static void Log(string str, params object[] args)
        {
            _Begin();
            if(args.Length >0) str = string.Format(str, args);
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(str);
            Console.ForegroundColor = oldColor;
            WriteLogFile(logFileName, str);
        }

        public static void LogWarning(string str, params object[] args)
        {
            _Begin();
            if (args.Length > 0) str = string.Format(str, args);
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(printPrefix ? warning_prefix + str : str);
            Console.ForegroundColor = oldColor;

            WriteLogFile(logFileName, printPrefix ? warning_prefix + str : str);
        }

        public static void LogError(string str, params object[] args)
        {
            _Begin();
            if (args.Length > 0) str = string.Format(str, args);
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(printPrefix ? error_prefix + str : str);
            Console.ForegroundColor = oldColor;
            WriteLogFile(logFileName, printPrefix ? error_prefix + str : str);
        }

        public static void LogException(System.Exception ex)
        {
            _Begin();
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(printPrefix ? error_prefix + ex.Message : ex.Message);
            WriteLogFile(logFileName, printPrefix ? error_prefix + ex.Message : ex.Message);
            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                Console.Error.WriteLine(printPrefix ? error_prefix + stacktrace_title + ex.StackTrace : stacktrace_title + ex.StackTrace);
                WriteLogFile(logFileName, printPrefix ? error_prefix + stacktrace_title + ex.StackTrace : stacktrace_title + ex.StackTrace);

                //_PrintStackTrace(ex.StackTrace.Trim());
            }
            Console.ForegroundColor = oldColor;
        }

        public static void Exit(int code = 0)
        {
            _Finish();
            Environment.Exit(code);
        }

        static void _PrintStackTrace(string stackTrace)
        {
            string[] s = stackTrace.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            for(int i=0; i < s.Length; ++i)
            {
                string info = s[i].Trim().TrimEnd('\n');
                Console.Error.WriteLine(StrRep(' ', 2) + info);
                WriteLogFile(logFileName, StrRep(' ', 2) + info);
            }
        }
        static string StrRep(string c, int indent)
        {
            string s = "";
            for (int n = 0; n < indent; ++n)
            {
                s += c;
            }
            return s;
        }
        static string StrRep(char c, int indent)
        {
            string s = "";
            for (int n = 0; n < indent; ++n)
            {
                s += c;
            }
            return s;
        }
    }
}
