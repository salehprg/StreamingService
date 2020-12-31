using System.Diagnostics;

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

            proc.StartInfo.FileName = "/bin/bash";
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