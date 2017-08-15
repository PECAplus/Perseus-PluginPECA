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
using BaseLibS.Graph.Image;

namespace PluginPECA.Core
{
    class MatrixProcessing : PluginPECA.MatrixProcessing, IMatrixProcessing
    {
        public override string Name => "PECA Core";

        public override string Description => "PECA Core implements the core functionality, i.e. for a two-level time series data set (e.g. paired protein and mRNA concentration data), it identifies significantly regulated genes and significant change points (via false discovery rate calculations). It identifies the direction of the change (up or down), but does not deconvolute the contributions of changes in synthesis or degradation.";

        public override float DisplayRank => 1;

        public override Bitmap2 DisplayImage => Utils.GetImage("logo.png");

        //public string HelpOutput { get; }

        //public string[] HelpSupplTables { get; }

        //public int NumSupplTables { get; }

        //public string[] HelpDocuments { get; }

        //public int NumDocuments { get; }

        public override Parameters GetParameters(IMatrixData mdata, ref string errString)
        {

            Parameters parameters = new Parameters
                (
                    PECAParameters.GetWorkingDir()
                );

            parameters.AddParameterGroup(PECAParameters.GetAboutData(), "About Data", false);

            //smoothing
            parameters.AddParameterGroup(PECAParameters.GetFeatures(), "Features", false);

            parameters.AddParameterGroup(PECAParameters.GetConditionalModule(), "Gene Set Enrichment Analysis", false);

            parameters.AddParameterGroup(PECAParameters.SelectData(mdata), "Select Data", true);

            parameters.AddParameterGroup(PECAParameters.GetMCMCParams(), "MCMC Parameters", false);


            return parameters;
        }

        public override void ProcessData(IMatrixData mdata, global::BaseLibS.Param.Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo)
        {
            

            string geneNameColumn = mdata.StringColumnNames[param.GetParam<int>("Gene Name Column").Value];

            string defaultPECAPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @".\bin\PECAInstallations\peca.exe");

            string workingDirectory = param.GetParam<string>("Working Directory").Value;

            string pythonFDR = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @".\bin\PECAInstallations\fdr.exe");

            string outputFile = System.IO.Path.Combine(workingDirectory, @".\data_R_CPS.txt");

            string errString;

            string paramInfo = Utils.CheckCoreParameters(mdata, param);

            if (paramInfo != null)
            {
                processInfo.ErrString = paramInfo;
                return;
            }


            if (Utils.WriteInputFiles(mdata, param, workingDirectory, out errString) != 0)
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
            //int totPECA = Utils.getMCMCParam(param.GetParamWithSubParams<int>("MCMC Burn-In"), 0) + Utils.getMCMCParam(param.GetParamWithSubParams<int>("MCMC Thinning"), 1) * Utils.getMCMCParam(param.GetParamWithSubParams<int>("MCMC Samples"), 2);
            int totPECA = param.GetParam<int>("MCMC Burn-In").Value + param.GetParam<int>("MCMC Thinning").Value * param.GetParam<int>("MCMC Samples").Value;
            if (ExternalProcess.RunExe(defaultPECAPath, inputLocation, workingDirectory, processInfo.Status, processInfo.Progress, 0, totPECA, out string processInfoErrString) != 0)
            {
                processInfo.ErrString = processInfoErrString;
                return;
            }

            int totFDR = mdata.RowCount;
            //breaks when user path contains space?
            if (ExternalProcess.RunExe(pythonFDR, null, workingDirectory, processInfo.Status, processInfo.Progress, 1, totFDR, out string processInfoErrString3) != 0)
            {
                processInfo.ErrString = processInfoErrString3;
                return;
            }

            Utils.GetOutput(mdata, param, outputFile, geneNameColumn);

            ParameterWithSubParams<bool> getAnalysis = param.GetParamWithSubParams<bool>(PECAParameters.gsea);

            //not sure if this will work
            //need to test
            if (getAnalysis.Value)
            {
                string modulePath = getAnalysis.GetSubParameters().GetParam<string>(PECAParameters.networkFile).Value;
                string moduleDestination = Path.Combine(@workingDirectory, @"networkFile.txt");
                File.Copy(modulePath, moduleDestination, true);

                if (Utils.WriteCPSInputParam(getAnalysis.GetSubParameters(), workingDirectory) != 0)
                {
                    processInfo.ErrString = "Unable To Process the Enrichment Analysis Parameters";
                    return;
                }
                string exeCPS = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @".\bin\PECAInstallations\cps.exe");
                string inputCPSLocation = "\"" + System.IO.Path.Combine(@workingDirectory, @".\cps_input") + "\"";
                if (ExternalProcess.RunExe(exeCPS, inputCPSLocation, workingDirectory, processInfo.Status, processInfo.Progress, -1, -1, out string processInfoErrString4) != 0)
                {
                    processInfo.ErrString = processInfoErrString4;
                    return;
                }
                //processInfo.ErrString = processInfoErrString5;
                supplTables = new IMatrixData[] { PluginPECA.Utils.GetGOEnr(mdata, workingDirectory) };

            }


            processInfo.Progress(0);

            

        }

    }

}