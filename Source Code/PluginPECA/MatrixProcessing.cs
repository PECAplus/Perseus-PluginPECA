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

        }
    }

}