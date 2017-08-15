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

namespace PluginPECA
{
    public abstract class MatrixProcessing : IMatrixProcessing
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public virtual float DisplayRank => 1;

        public virtual bool IsActive => true;

        //might need to change this in the future
        public virtual int GetMaxThreads(global::BaseLibS.Param.Parameters parameters) => 1;

        public virtual bool HasButton => false;

        public virtual Bitmap2 DisplayImage => null;

        //insert github plugin in the future
        public virtual string Url => "";

        public virtual string Heading => "PECA";

        public virtual string HelpOutput { get; }

        public virtual string[] HelpSupplTables { get; }

        public virtual int NumSupplTables { get; }

        public virtual string[] HelpDocuments { get; }

        public virtual int NumDocuments { get; }

        public virtual Parameters GetParameters(IMatrixData mdata, ref string errString)
        {



            Parameters parameters = new Parameters
                (
                    PECAParameters.GetWorkingDir()
                );

            parameters.AddParameterGroup(PECAParameters.GetAboutData(), "About Data", false);

            parameters.AddParameterGroup(PECAParameters.SelectData(mdata), "Select Data", true);

            parameters.AddParameterGroup(PECAParameters.GetMCMCParams(), "MCMC Parameters", false);


            return parameters;
        }

        public virtual void ProcessData(IMatrixData mdata, global::BaseLibS.Param.Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo)
        {

            // test
            //processInfo.ErrString = string.Format("stringcolumn count is: {0}, numerical column count is: {1}, general count is {2}", mdata.StringColumnCount, mdata.NumericColumnCount, mdata.Column);

            string geneNameColumn = mdata.StringColumnNames[param.GetParam<int>("Gene Name Column").Value];

            string defaultPECAPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @".\bin\PECAInstallations\peca.exe");

            string workingDirectory = param.GetParam<string>("Working Directory").Value;

            string pythonFDR = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @".\bin\PECAInstallations\fdr.exe");

            string outputFile = System.IO.Path.Combine(workingDirectory, @".\data_R_CPS.txt");

            string errString;



            //processInfo.ErrString = workingDirectory;
            //return;

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
            int totPECA = Utils.GetMCMCParam(param.GetParamWithSubParams<int>("MCMC Burn-In"), 0) + Utils.GetMCMCParam(param.GetParamWithSubParams<int>("MCMC Thinning"), 1) * Utils.GetMCMCParam(param.GetParamWithSubParams<int>("MCMC Samples"), 2);
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


            processInfo.Progress(0);

        }
    }

}