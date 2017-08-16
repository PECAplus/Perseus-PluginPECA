using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibS.Param;
using PerseusApi.Matrix;
using PerseusApi.Utils;
using BaseLibS.Parse;
using BaseLibS.Num;
using PerseusApi.Generic;
using System.IO;
using PluginPECA;

namespace PluginPECA.ModuleN
{
    public class Utils
    {

        public static string CheckEnrichmentModule(Parameters param)
        {
            if (param.GetParam<int>(PECAParameters.minNetwork).Value <= 0)
            {
                return PECAParameters.minNetwork + " should be a positive integer";
            }
            if (param.GetParam<int>(PECAParameters.maxNetwork).Value <= 0)
            {
                return PECAParameters.maxNetwork + " should be a positive integer";
            }
            return PluginPECA.Utils.CheckEnrichmentCommon(param);
        }

        public static string CheckParameters(IMatrixData mdata, Parameters param, string expSeries1 = "Expression Series 1", string expSeries2 = "Expression Series 2")
        {
            string commonErr = PluginPECA.Utils.CheckCommonParameters(mdata, param, expSeries1, expSeries2);
            if (commonErr != null) return commonErr;
            return CheckEnrichmentModule(param);

        }

        public static int WriteInputFiles(IMatrixData mdata, Parameters param, string workingDirectory, ProcessInfo processInfo, out string errString)
        {
            //moving module
            string modulePath = param.GetParam<string>(PECAParameters.networkFile).Value;
            string moduleDestination = Path.Combine(@workingDirectory, @"networkFile.txt");
            File.Copy(modulePath, moduleDestination, true);
            errString = PluginPECA.Utils.ConvertUnix2Dos(moduleDestination, processInfo);
            if (errString != null) return -1;
            return PluginPECA.Utils.WriteInputFiles(mdata, param, workingDirectory, out errString);
       
        }

        public static string[] GetLinesWithNetwork(Parameters param)
        {
            string[] lines =  PluginPECA.Utils.GetInputParamLines(param);

            string[] newLines = new string[lines.Length + 2];

            for (int i = 0; i < newLines.Length-2; i++)
            {
                newLines[i] = lines[i];
            }
            newLines[newLines.Length - 2] = "MODULE=networkFile.txt";
            newLines[newLines.Length - 1] = string.Format("MODULE_SIZE={0} {1}", param.GetParam<int>(PECAParameters.minNetwork).Value, param.GetParam<int>(PECAParameters.maxNetwork).Value);
            return newLines;
        }

        public static int WriteInputParam(Parameters param, string workingDir)
        {

            string[] lines = GetLinesWithNetwork(param);

            string inputLocation = System.IO.Path.Combine(workingDir, "input");
            System.IO.File.WriteAllLines(inputLocation, lines);
            return 0;
        }

        public static string[] GetCPSInputParamLinesWithNetwork(Parameters param)
        {
            string[] lines = PluginPECA.Utils.GetCPSInputParamLines(param);
            string[] newLines = new string[lines.Length + 1];

            for (int i = 0; i < newLines.Length - 1; i++)
            {
                newLines[i] = lines[i];
            }
            newLines[newLines.Length - 1] = "MODULE=Adjacency_list.txt";
            return newLines;
        }

        public static int WriteCPSInputParam(Parameters param, string workingDir)
        {
            string[] lines = GetCPSInputParamLinesWithNetwork(param);

            string inputLocation = System.IO.Path.Combine(workingDir, "cps_input");
            System.IO.File.WriteAllLines(inputLocation, lines);
            return 0;
        }


    }
}
