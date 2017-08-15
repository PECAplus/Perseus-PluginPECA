using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PluginPECA
{
    public static class ExternalProcess
    {
        //total is the total number of iterations or genes to run depending on the given task
        
        public static int RunExe(string exe, string arg, string workingDir, Action<string> status, Action<int> progress, int task, int total, out string errorString)
        {
            int PECATASK = 0;
            int FDRTASK = 1;
            string pattern = @"(?<![A-Za-z0-9.])[0-9]+";
            Regex reg = new Regex(pattern);
            Regex notWanted = new Regex("(nprot)|(data)");


            errorString = null;
            var externalProcessInfo = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = arg,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = @workingDir
                //"\"" + workingDir + "\""
            };
            var process = new Process { StartInfo = externalProcessInfo };
            var outputData = new List<string>();
            process.OutputDataReceived += (sender, output) =>
            {
                //might want to reorganize this part in the future
                //Debug.WriteLine(output.Data);
                if  (output.Data!= null && (task == PECATASK || task == FDRTASK))
                {
                    Match m = reg.Match(output.Data);
                    Match notW = notWanted.Match(output.Data);
                    if (!notW.Success && m.Success)
                    {
                        status(m.Value + "/" + total);
                        if (task == PECATASK)
                        {
                            progress((int)(50 * (int.Parse(m.Value) / Convert.ToSingle(total))));
                        }
                        else
                        {
                            progress((int)(50 + 50 * (int.Parse(m.Value) / Convert.ToSingle(total))));
                        }
                    }
                    else
                    {
                        status(output.Data);
                    }
                }
                else
                {
                    status(output.Data);
                }

                outputData.Add(output.Data);
            };
            var errorData = new List<string>();
            process.ErrorDataReceived += (sender, error) =>
            {
                //Debug.WriteLine(error.Data);
                errorData.Add(error.Data);
            };
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            int exitCode = process.ExitCode;
            //Debug.WriteLine($"Process exited with exit code {exitCode}");
            if (exitCode != 0)
            {
                var statusString = String.Join("\n", outputData);
                var errString = String.Join("\n", errorData);
                errorString = String.Concat("Output\n", statusString, "\n", "Error\n", errString);
            }
            process.Dispose();
            return exitCode;
        }
    }
}