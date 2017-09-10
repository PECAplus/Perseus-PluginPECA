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
using PluginPECA.Core;

namespace PluginPECA.ModuleN
{
    class MatrixProcessing : PluginPECA.MatrixProcessing, IMatrixProcessing
    {
        public override string Name => "PECA-N";

        public override string Description => "PECA-N (Network) module is modified from PECA to incorporate user-provided biological network data into the inference of rate parameter changes in co-regulated genes, where it is assumed that functionally related genes tend to be co-regulated along the time course.";

        public override float DisplayRank => 2;

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

            parameters.AddParameterGroup(PECAParameters.GetFeatures(), "Features", false);

            parameters.AddParameterGroup(PECAParameters.GetModule(), PECAParameters.network + " Info", false);

            parameters.AddParameterGroup(PECAParameters.SelectConditionalData(mdata), "Select Data", true);

            parameters.AddParameterGroup(PECAParameters.GetMCMCParams(), "MCMC Parameters", false);


            return parameters;
        }

        public override void ProcessData(IMatrixData mdata, global::BaseLibS.Param.Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo)
        {
            // test
            //processInfo.ErrString = string.Format("stringcolumn count is: {0}, numerical column count is: {1}, general count is {2}", mdata.StringColumnCount, mdata.NumericColumnCount, mdata.Column);

            string geneNameColumn = mdata.StringColumnNames[param.GetParam<int>("Gene Name Column").Value];

            string defaultPECAPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @".\bin\PECAInstallations\peca_core_N.exe");

            string workingDirectory = param.GetParam<string>("Working Directory").Value;

            string pythonFDR = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @".\bin\PECAInstallations\fdr.exe");

            string outputFile = System.IO.Path.Combine(workingDirectory, @".\data_R_CPS.txt");

            string errString;

            ParameterWithSubParams<bool> getRNAInf = param.GetParamWithSubParams<bool>(PECAParameters.RNAInference);

            string paramInfo = null;

            if (getRNAInf.Value)
            {
                //RNAinf checking
                paramInfo = Utils.CheckParameters(mdata, param, getRNAInf.GetSubParameters(), PECAParameters.RNAexp, null, PECAParameters.dataFormGeneric, null);
            }
            else
            {
                paramInfo = Utils.CheckParameters(mdata, param, getRNAInf.GetSubParameters());
            }
            if (paramInfo != null)
            {
                processInfo.ErrString = paramInfo;
                return;
            }

            if (Utils.WriteInputFiles(mdata, param, getRNAInf.GetSubParameters(), getRNAInf.Value, workingDirectory, processInfo, out errString) != 0)
            {
                processInfo.ErrString = errString;
                return;
            }


            if (Utils.WriteInputParam(param, getRNAInf.GetSubParameters(), getRNAInf.Value, workingDirectory) != 0)
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

            if (getRNAInf.Value)
            {
                PluginPECA.Utils.GetOutput(mdata, param, getRNAInf.GetSubParameters(), outputFile, geneNameColumn, PECAParameters.RNAexp, 2);
            }
            else
            {
                PluginPECA.Utils.GetOutput(mdata, param, getRNAInf.GetSubParameters(), outputFile, geneNameColumn);
            }

            ////need to include the GSA part

            //new version no longer need this
            //if (Utils.WriteCPSInputParam(param, workingDirectory) != 0)
            //{
            //    processInfo.ErrString = "Unable To Process the Enrichment Analysis Parameters";
            //    return;
            //}
            //string exeCPS = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @".\bin\PECAInstallations\cps.exe");
            //string inputCPSLocation = "\"" + System.IO.Path.Combine(@workingDirectory, @".\cps_input") + "\"";
            //if (ExternalProcess.RunExe(exeCPS, inputCPSLocation, workingDirectory, processInfo.Status, processInfo.Progress, -1, -1, out string processInfoErrString4) != 0)
            //{
            //    processInfo.ErrString = processInfoErrString4;
            //    return;
            //}

            IMatrixData supplData = PluginPECA.Utils.getGSA(param, mdata, workingDirectory, -2, processInfo, out string gsaErrString);
            if (gsaErrString != null)
            {
                processInfo.ErrString = gsaErrString;
                return;
            }

            //processInfo.ErrString = processInfoErrString5;
            supplTables = new IMatrixData[] { supplData };
            
            processInfo.Progress(0);



        }

    }

}