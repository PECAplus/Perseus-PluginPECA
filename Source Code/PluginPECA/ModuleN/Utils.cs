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
            if (!File.Exists(param.GetParam<string>(PECAParameters.edgeFile).Value))
            {
                return "Select a valid " + PECAParameters.edgeFile;
            }
            return PluginPECA.Utils.CheckEnrichmentCommon(param);
        }

        public static string CheckParameters(IMatrixData mdata, Parameters param, Parameters dataParam, string expSeries1 = "Expression Series 1", string expSeries2 = "Expression Series 2", string dataForm1 = "Data Input Form 1", string dataForm2 = "Data Input Form 2")
        {
            string commonErr = PluginPECA.Utils.CheckCommonParameters(mdata, param, dataParam, expSeries1, expSeries2, dataForm1, dataForm2);
            if (commonErr != null) return commonErr;
            return CheckEnrichmentModule(param);

        }

        public static int WriteInputFiles(IMatrixData mdata, Parameters param, Parameters dataParam, bool isRNAOnly, string workingDirectory, ProcessInfo processInfo, out string errString)
        {
            //moving module
            string edgePath = param.GetParam<string>(PECAParameters.edgeFile).Value;
            string edgeDestination = Path.Combine(@workingDirectory, "edgeFile.txt");
            File.Copy(edgePath, edgeDestination, true);
            errString = PluginPECA.Utils.ConvertUnix2Dos(edgeDestination, processInfo);
            if (errString != null) return -1;
            return PluginPECA.Utils.WriteInputFilesCond(mdata, param, dataParam, isRNAOnly, workingDirectory, out errString);
       
        }

        public static string[] GetLinesWithNetwork(Parameters param, Parameters dataParam, bool isRNAOnly)
        {
            string[] lines =  PluginPECA.Utils.GetInputParamLines(param, dataParam, isRNAOnly);
            //change from 2 to 1 because module size removed
            string[] newLines = new string[lines.Length + 1];

            for (int i = 0; i < newLines.Length-1; i++)
            {
                newLines[i] = lines[i];
            }
            newLines[newLines.Length - 1] = "MODULE=edgeFile.txt";
            //removed from the new version
            //newLines[newLines.Length - 1] = string.Format("MODULE_SIZE={0} {1}", param.GetParam<int>(PECAParameters.minNetwork).Value, param.GetParam<int>(PECAParameters.maxNetwork).Value);
            return newLines;
        }

        public static int WriteInputParam(Parameters param, Parameters dataParam, bool isRNAOnly, string workingDir)
        {

            string[] lines = GetLinesWithNetwork(param, dataParam, isRNAOnly);

            string inputLocation = System.IO.Path.Combine(workingDir, "input");
            System.IO.File.WriteAllLines(inputLocation, lines);
            return 0;
        }

        //not needed with new ver
        public static string[] GetCPSInputParamLinesWithNetwork(Parameters param)
        {
            //option = -2 for N
            string[] lines = PluginPECA.Utils.GetGSAInputParamLines(param, -2);
            string[] newLines = new string[lines.Length + 1];

            for (int i = 0; i < newLines.Length - 1; i++)
            {
                newLines[i] = lines[i];
            }
            newLines[newLines.Length - 1] = "MODULE=Adjacency_list.txt";
            return newLines;
        }

        //not needed with new ver
        public static int WriteCPSInputParam(Parameters param, string workingDir)
        {
            string[] lines = GetCPSInputParamLinesWithNetwork(param);
            //string[] lines = PluginPECA.Utils.GetGSAInputParamLines(param, -2);
            string inputLocation = System.IO.Path.Combine(workingDir, "cps_input");
            System.IO.File.WriteAllLines(inputLocation, lines);
            return 0;
        }


    }
}
