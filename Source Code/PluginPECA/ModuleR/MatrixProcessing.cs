using BaseLibS.Graph;
using BaseLibS.Num;
using BaseLibS.Param;
using BaseLibS.Parse;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PluginPECA;

namespace PluginPECA.ModuleR
{
    class MatrixProcessing : PluginPECA.MatrixProcessing, IMatrixProcessing
    {
        public override string Name => "PECA-R";

        public override string Description => "PECA-R (Rate) is a model that separates estimation of rate parameters from absolute concentration data, approximating synthesis and degradation rates from via two assumptions.";

        public override float DisplayRank => 4;

        //public string HelpOutput { get; }

        //public string[] HelpSupplTables { get; }

        //public int NumSupplTables { get; }

        //public string[] HelpDocuments { get; }

        //public int NumDocuments { get; }

        public override Parameters GetParameters(IMatrixData mdata, ref string errString)
        {
            string[] expSeriesNames = new string[] { PECAParameters.pecaRSeries1, PECAParameters.pecaRSeries2 };
            string[] expSeriesHelp = new string[] { "mRNA data", "M/L data from SILAC", "H/L data from SILAC" };

            Parameters parameters = new Parameters
                (
                    PECAParameters.GetWorkingDir()
                );

            parameters.AddParameterGroup(PECAParameters.GetAboutDataWithTP(), "About Data", false);

            parameters.AddParameterGroup(PECAParameters.GetFeatures(), "Features", false);

            parameters.AddParameterGroup(PECAParameters.SelectMultipleData(mdata, expSeriesNames, expSeriesHelp), "Select Data", true);

            parameters.AddParameterGroup(PECAParameters.GetMCMCParams(), "MCMC Parameters", false);

            return parameters;
        }

        public override void ProcessData(IMatrixData mdata, global::BaseLibS.Param.Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo)
        {

            // test
            //processInfo.ErrString = string.Format("stringcolumn count is: {0}, numerical column count is: {1}, general count is {2}", mdata.StringColumnCount, mdata.NumericColumnCount, mdata.Column);

            string geneNameColumn = mdata.StringColumnNames[param.GetParam<int>("Gene Name Column").Value];

            string defaultPECAPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @".\bin\PECAInstallations\peca2.exe");

            string workingDirectory = param.GetParam<string>("Working Directory").Value;

            string pythonFDR = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @".\bin\PECAInstallations\fdr2.exe");

            string outputFile = System.IO.Path.Combine(workingDirectory, @".\data_R_CPS.txt");

            string errString;

            string paramInfo = Utils.CheckParameters(mdata, param);

            if (paramInfo != null)
            {
                processInfo.ErrString = paramInfo;
                return;
            }

            //need extra checking here like timepoints
            //maybe remove checking from writeInputFiles in the future
            //create an independent one to check
            if (PluginPECA.Utils.WriteInputFiles(mdata, param, workingDirectory, out errString, PECAParameters.pecaRSeries1, PECAParameters.pecaRSeries2) != 0)
            {
                processInfo.ErrString = errString;
                return;
            }


            if (Utils.WriteInputParam(param, workingDirectory) != 0)
            {
                processInfo.ErrString = "Unable To Process the Given Parameters";
                return;
            }

            string inputLocation = "\"" + System.IO.Path.Combine(@workingDirectory, @".\input") + "\"";


            //0 is PECA task
            int totPECA = param.GetParam<int>("MCMC Burn-In").Value + param.GetParam<int>("MCMC Thinning").Value * param.GetParam<int>("MCMC Samples").Value;
            if (ExternalProcess.RunExe(defaultPECAPath, inputLocation, workingDirectory, processInfo.Status, processInfo.Progress, 0, totPECA, out string processInfoErrString) != 0)
            {
                processInfo.ErrString = processInfoErrString;
                return;
            }

            int totFDR = mdata.RowCount;
            //error with NA?

            if (ExternalProcess.RunExe(pythonFDR, null, workingDirectory, processInfo.Status, processInfo.Progress, 1, totFDR, out string processInfoErrString3) != 0)
            {
                processInfo.ErrString = processInfoErrString3;
                return;
            }

            PluginPECA.Utils.GetOutput(mdata, param, outputFile, geneNameColumn, PECAParameters.pecaRSeries1);


            processInfo.Progress(0);

        }
    }

}