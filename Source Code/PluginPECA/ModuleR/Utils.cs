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

namespace PluginPECA.ModuleR
{
    public static class Utils
    {

        public static string CheckParameters(IMatrixData mdata, Parameters param)
        {
            string expSeries1 = PECAParameters.pecaRSeries1;
            string expSeries2 = PECAParameters.pecaRSeries2;
            string commonErr = PluginPECA.Utils.CheckCommonParameters(mdata, param, expSeries1, expSeries2);
            if (commonErr != null) return commonErr;
            return PluginPECA.Utils.CheckTimePoints(mdata, param, expSeries1);
        }

        public static string[] GetInputParamLines(Parameters param)
        {
            //actual time points in this case
            string tp = param.GetParam<string>("Time Points").Value;
            //gpTime is # of time points
            string gpTime =  string.Join(" ", Enumerable.Range(0, param.GetParam<int[]>(PECAParameters.pecaRSeries1).Value.Length / param.GetParam<int>("Number of Replicates").Value));


            //isLN check
            //if yes then false - does not need to be logged
            //if not true - needs to be logged
            string needLoggedString1 = PluginPECA.Utils.GetBase(param.GetParamWithSubParams<int>(PECAParameters.dataForm1)) == 0 ? "0" : "1";
            string needLoggedString2 = PluginPECA.Utils.GetBase(param.GetParamWithSubParams<int>(PECAParameters.dataForm2)) == 0 ? "0" : "1";
            //string isLoggedString = param.GetParam<bool>("Is log2(x)?").Value ? "false" : "true";


            string[] lines =
            {
                "FILE_X = fileX.txt " +needLoggedString1,
                //need to add this back
                //has to be 1 in the test example
                "FILE_Y = fileY.txt " +needLoggedString2,
                string.Format("N_REP = {0}", param.GetParam<int>("Number of Replicates").Value),
                "TIME = " + tp,
                "GP_TIME = " + gpTime,
                string.Format("N_BURN = {0}", param.GetParam<int>("MCMC Burn-In").Value),
                string.Format("N_THIN = {0}", param.GetParam<int>("MCMC Thinning").Value),
                string.Format("N_SAMPLE = {0}", param.GetParam<int>("MCMC Samples").Value),
                ""
            };
            //success if 0

            //replace with smoothing

            ParameterWithSubParams<bool> getSmoothing = param.GetParamWithSubParams<bool>("Smoothing");

            lines[lines.Length - 1] = getSmoothing.Value ? string.Format("SMOOTHING= {0} {1}", getSmoothing.GetSubParameters().GetParam<double>(PECAParameters.smoothingPar1).Value, getSmoothing.GetSubParameters().GetParam<double>(PECAParameters.smoothingPar2).Value) : "";

            return lines;


        }

        //input is the paratemer file
        public static int WriteInputParam(Parameters param, string workingDir)
        {

            string[] lines = GetInputParamLines(param);

            string inputLocation = System.IO.Path.Combine(workingDir, "input");
            System.IO.File.WriteAllLines(inputLocation, lines);
            return 0;
        }
    }
}
