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

namespace PluginPECA.ModulePS
{
    public static class Utils
    {

        public static string CheckParameters(IMatrixData mdata, Parameters param)
        {
            string expSeries1 = PECAParameters.pecaPSSeries1;
            string expSeries2 = PECAParameters.pecaPSSeries2;
            string expSeries3 = PECAParameters.pecaPSSeries3;
            string commonErr = PluginPECA.Utils.CheckCommonParameters(mdata, param, expSeries1, expSeries2);
            if (commonErr != null) return commonErr;

            string thirdSeriesErr = checkThirdSeries(param, expSeries1, expSeries3);
            if (thirdSeriesErr != null) return thirdSeriesErr;

            return PluginPECA.Utils.CheckTimePoints(mdata, param, expSeries1);
        }

        public static string checkThirdSeries(Parameters param, string expSeries1, string expSeries3)
        {
            int[] series1Ind = param.GetParam<int[]>(expSeries1).Value;
            int[] series3Ind = param.GetParam<int[]>(expSeries3).Value;
            double baseVal3 = PluginPECA.Utils.GetBase(param.GetParamWithSubParams<int>(PECAParameters.dataForm3));

            if (series1Ind.Length != series3Ind.Length)
            {
                return "Expression Series 1 and 3 should have matching number of columns";
            }
            if (baseVal3 < 0 && baseVal3 != -1)
            {
                return PECAParameters.dataForm3 + " should have a positive base";
            }
            return null;
        }

        public static int WriteInputFiles(IMatrixData mdata, Parameters param, string workingDirectory, out string errString, string expSeries1 = "Expression Series 1", string expSeries2 = "Expression Series 2", string expSeries3 = "Expression Series 3")
        {
            //need to write a general version
            //accepts text only data for now



            //set as global
            string fileTempX = System.IO.Path.Combine(workingDirectory, @".\tempX.txt");
            string fileX = System.IO.Path.Combine(workingDirectory, @".\fileX.txt");
            string fileTempM = System.IO.Path.Combine(workingDirectory, @".\tempM.txt");
            string fileM = System.IO.Path.Combine(workingDirectory, @".\fileM.txt");
            string fileTempY = System.IO.Path.Combine(workingDirectory, @".\tempY.txt");
            string fileY = System.IO.Path.Combine(workingDirectory, @".\fileY.txt");


            IMatrixData mCopyX = (IMatrixData)mdata.Clone();
            IMatrixData mCopyM = (IMatrixData)mdata.Clone();
            IMatrixData mCopyY = (IMatrixData)mdata.Clone();


            int[] nameInd = new[] { param.GetParam<int>("Gene Name Column").Value };

            int[] xInd = param.GetParam<int[]>(expSeries1).Value;
            int[] mInd = param.GetParam<int[]>(expSeries2).Value;
            int[] yInd = param.GetParam<int[]>(expSeries3).Value;

            //int n_tp = param.GetParam<int>("Number of Time Points").Value;
            int n_rep = param.GetParam<int>("Number of Replicates").Value;

            if (nameInd.Length == 0)
            {
                errString = "Please select a gene name column";
                return -1;
            }
            if (xInd.Length == 0)
            {
                errString = "Please select some columns for Expression Series";
                return -1;
            }

            if (xInd.Length != yInd.Length || xInd.Length != mInd.Length)
            {
                errString = "Expression Series need to have matching number of columns";
                return -1;
            }

            if ((xInd.Length % n_rep) != 0)
            {
                errString = "number of columns not multiple of Number of Replicates";
                return -1;
            }
            //check also timepoints here

            //if ((xInd.Length % n_tp)!=0)
            //{
            //    errString = "number of columns not multiple of Number of Time Points";
            //    return -1;
            //}

            double baseVal1 = PluginPECA.Utils.GetBase(param.GetParamWithSubParams<int>(PECAParameters.dataForm1));
            double baseVal2 = PluginPECA.Utils.GetBase(param.GetParamWithSubParams<int>(PECAParameters.dataForm2));
            double baseVal3 = PluginPECA.Utils.GetBase(param.GetParamWithSubParams<int>(PECAParameters.dataForm3));

            PluginPECA.Utils.SetupMDataForInput(mCopyX, xInd, nameInd, baseVal1);
            PluginPECA.Utils.SetupMDataForInput(mCopyM, mInd, nameInd, baseVal2);
            PluginPECA.Utils.SetupMDataForInput(mCopyY, yInd, nameInd, baseVal3);

            try
            {
                //hacky way of removing comments
                //better to just ask GS to ignore comments from his code
                PerseusUtils.WriteMatrixToFile(mCopyX, fileTempX, false);
                PluginPECA.Utils.RemoveCommentsFromFile(fileTempX, fileX);
                PerseusUtils.WriteMatrixToFile(mCopyM, fileTempM, false);
                PluginPECA.Utils.RemoveCommentsFromFile(fileTempM, fileM);
                PerseusUtils.WriteMatrixToFile(mCopyY, fileTempY, false);
                PluginPECA.Utils.RemoveCommentsFromFile(fileTempY, fileY);

            }
            catch (Exception e)
            {
                //need to reorganize this
                errString = e.ToString();
                return -1;
            }

            errString = null;
            return 0;
        }


        public static string[] GetInputParamLines(Parameters param)
        {
            //actual time points in this case
            string tp = param.GetParam<string>("Time Points").Value;
            //gpTime is # of time points
            string gpTime = string.Join(" ", Enumerable.Range(0, param.GetParam<int[]>(PECAParameters.pecaRSeries1).Value.Length / param.GetParam<int>("Number of Replicates").Value));


            //isLN check
            //if yes then false - does not need to be logged
            //if not true - needs to be logged
            string needLoggedString1 = PluginPECA.Utils.GetBase(param.GetParamWithSubParams<int>(PECAParameters.dataForm1)) == 0 ? "0" : "1";
            string needLoggedString2 = PluginPECA.Utils.GetBase(param.GetParamWithSubParams<int>(PECAParameters.dataForm2)) == 0 ? "0" : "1";
            string needLoggedString3 = PluginPECA.Utils.GetBase(param.GetParamWithSubParams<int>(PECAParameters.dataForm3)) == 0 ? "0" : "1";
            //string isLoggedString = param.GetParam<bool>("Is log2(x)?").Value ? "false" : "true";


            string[] lines =
            {
                "FILE_X = fileX.txt "+needLoggedString1,
                //need to add this back
                "FILE_M = fileM.txt " +needLoggedString2,
                "FILE_Y = fileY.txt " +needLoggedString3,
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
