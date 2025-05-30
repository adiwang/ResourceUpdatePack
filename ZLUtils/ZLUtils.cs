

using System;
using System.IO;
namespace ZLUtils
{
    public sealed class OS
    {
        public static System.Diagnostics.Process Exec(string bin, string arguments = "", bool window = false, bool async = false, EventHandler onExit = null, Action<bool, System.Diagnostics.DataReceivedEventArgs> onData = null)
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(bin);
            startInfo.Arguments = arguments;
            startInfo.CreateNoWindow = !window;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true; //重定向输出，
            startInfo.RedirectStandardError = true;

            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo = startInfo;
                if (!window)
                {
                    process.OutputDataReceived += (s, e) =>
                    {
                        if(e.Data != null) ConsoleUtil.Log(e.Data);
                        if (onData != null) onData(true, e);
                    };
                    process.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data != null) ConsoleUtil.LogError(e.Data);
                        if (onData != null) onData(false, e);
                    };
                }
                process.EnableRaisingEvents = true;
                process.Exited += (s, e) =>
                {
                    if (onExit != null) onExit(s, e);
                };
                process.Start();
                if (!window)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }
                if (!async)
                    process.WaitForExit();
                return process;
            }
            catch (Exception e)
            {
                ConsoleUtil.LogException(e);
                return null;
            }
        }
    }

    public sealed class App
    {
        public static void Exit(int code)
        {
            ConsoleUtil.Exit(code);
        }
        public static string ModuleName
        {
            get { return Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName); }
        }
        public static string ModulePath
        {
            get { return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName); }
        }
    }
}