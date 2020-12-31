using System.Diagnostics;
using System.Runtime.InteropServices;

namespace streamingservice.Helper
{
    public class ShellRunner
    {
        public static string Execute(string command)
        {
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            string fileName = "";

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName = "powershell.exe";
            }

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                fileName = "/bin/bash";
            }
            
            proc.StartInfo.FileName = fileName;
            proc.Start();

            proc.StandardInput.WriteLine(command);
            proc.StandardInput.Flush();
            proc.StandardInput.Close();
            proc.WaitForExit();

            string output = proc.StandardOutput.ReadToEnd();

            if(!string.IsNullOrEmpty(output))
                return output;

            return proc.StandardError.ReadToEnd();
        }
    }
}