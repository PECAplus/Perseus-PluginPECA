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
using System.Text.RegularExpressions;
using BaseLibS.Graph;
using System.Reflection;
using BaseLibS.Graph.Image;

namespace PluginPECA
{
    public static class Utils
    {

        public static string CheckNonDataParameters(IMatrixData mdata, Parameters param)
        {
            string workingDirectory = param.GetParam<string>("Working Directory").Value;
            int[] nameInd = new[] { param.GetParam<int>("Gene Name Column").Value };
            int n_rep = param.GetParam<int>("Number of Replicates").Value;
            ParameterWithSubParams<bool> getSmoothing = param.GetParamWithSubParams<bool>("Smoothing");

            //working directory checking
            if (!Directory.Exists(workingDirectory))
            {
                return "Select a valid working directory";
            }

            //smoothing checking
            if (getSmoothing.Value)
            {
                if (getSmoothing.GetSubParameters().GetParam<double>(PECAParameters.smoothingPar1).Value <= 0 || getSmoothing.GetSubParameters().GetParam<double>(PECAParameters.smoothingPar2).Value <= 0)
                {
                    return "Smoothing parameters should be positive real numbers";
                }
            }

            //select data checking
            if (nameInd.Length == 0)
            {
                return "Select a gene name column";
            }

            //MCMC parameters checking

            if (param.GetParam<int>("MCMC Burn-In").Value <= 0 || param.GetParam<int>("MCMC Thinning").Value <= 0 || param.GetParam<int>("MCMC Samples").Value <= 0)
            {
                return "MCMC parameters should be positive integers";
            }

            return null;
        }

        //public static string CheckRNAParameters(IMatrixData mdata, Parameters param, Parameters dataParam, string RNAexp)
        //{
        //    int[] RNAexpInd = dataParam.GetParam<int[]>(RNAexp).Value;
        //    int n_rep = param.GetParam<int>("Number of Replicates").Value;
        //    if ((RNAexpInd.Length % n_rep) != 0)
        //    {
        //        return "Number of columns not multiple of Number of Replicates";
        //    }
        //    if (RNAexpInd.Length == 0)
        //    {
        //        return "Select some columns for RNA Expression Series";
        //    }

        //    return null;
        //}


        //return the error info if any error is found
        public static string CheckCommonParameters(IMatrixData mdata, Parameters param, Parameters dataParam, string expSeries1 = "Expression Series 1", string expSeries2 = "Expression Series 2", string dataForm1 = "Data Input Form 1", string dataForm2 = "Data Input Form 2")
        {
            int[] series1Ind = dataParam.GetParam<int[]>(expSeries1).Value;
            int[] series2Ind = expSeries2 != null ? dataParam.GetParam<int[]>(expSeries2).Value : new int[0];
            int n_rep = param.GetParam<int>("Number of Replicates").Value;

            //about data checking
            if ((series1Ind.Length % n_rep) != 0)
            {
                return "Number of columns not multiple of Number of Replicates";
            }
            if (series1Ind.Length == 0)
            {
                return "Select some columns for Expression Series";
            }
            if (expSeries2 != null && series1Ind.Length != series2Ind.Length)
            {
                return "Expression Series 1 and 2 should have matching number of columns";
            }

            double baseVal1 = GetBase(dataParam.GetParamWithSubParams<int>(dataForm1));
            double baseVal2 = dataForm2 != null ? GetBase(dataParam.GetParamWithSubParams<int>(dataForm2)) : int.MaxValue;
            //check data form
            if (baseVal1<0 && baseVal1 != -1)
            {
                return dataForm1 + " should have a positive base";
            }
            if (dataForm2 != null && baseVal2 < 0 && baseVal2 != -1)
            {
                return dataForm2 + " should have a positive base";
            }

            return CheckNonDataParameters(mdata,param);
        }

        //the common part to both Core and N in case enrichment is used
        public static string CheckEnrichmentCommon(Parameters param)
        {
            if (!File.Exists(param.GetParam<string>(PECAParameters.networkFile).Value))
            {
                return "Select a valid " + PECAParameters.networkFile;
            }

            if (param.GetParam<Double>(PECAParameters.fdrCutoff).Value < 0 || param.GetParam<Double>(PECAParameters.fdrCutoff).Value > 1)
            {
                return PECAParameters.fdrCutoff + " should be between 0.0 and 1.0";
            }
            if (param.GetParam<int>(PECAParameters.backgroundPar1).Value < 0 || param.GetParam<int>(PECAParameters.backgroundPar1).Value > 100)
            {
                return PECAParameters.backgroundPar1 + " should be between 0 and 100";
            }
            if (param.GetParam<int>(PECAParameters.backgroundPar2).Value <= 0 )
            {
                return PECAParameters.backgroundPar2 + " should be a positive integer";
            }
            return null;
        }


        public static string CheckCoreParameters(IMatrixData mdata, Parameters param, Parameters dataParam, string expSeries1 = "Expression Series 1", string expSeries2 = "Expression Series 2", string dataForm1 = "Data Input Form 1", string dataForm2 = "Data Input Form 2")
        {
            string commonErr = CheckCommonParameters(mdata, param, dataParam, expSeries1, expSeries2, dataForm1, dataForm2);
            if (commonErr != null) return commonErr;
            ParameterWithSubParams<bool> getAnalysis = param.GetParamWithSubParams<bool>(PECAParameters.gsa);
            if (getAnalysis.Value)
            {
                return CheckEnrichmentCommon(getAnalysis.GetSubParameters());
            }
            return null;
        }

        public static string CheckTimePoints(IMatrixData mdata, Parameters param, string expSeries1)
        {
            int[] series1Ind = param.GetParam<int[]>(expSeries1).Value;
            string tp = param.GetParam<String>("Time Points").Value;
            string[] splitTP = tp.Split(' ');

            if (splitTP.Length == 0)
            {
                return "Select some time points";
            }

            if (series1Ind.Length % splitTP.Length != 0)
            {
                return expSeries1 + " should be multiple of number of time points";
            }
            double numVal;
            foreach (string time in splitTP)
            {
                //#comeBack
                bool parsed = Double.TryParse(time, out numVal);
                if (!parsed)
                {
                    return "Invalid character in Time Points. Should be all numeric values separated by whitespace";
                }
            }

            return null;

        }

        //this function is modified from PerseusPluginLib/Load/UnstructuredTxtUpload.cs LoadSplit function
        //obtains the output from fdr.exe (so only applicable to PECA CORE and N)
        public static void GetOutput(IMatrixData mdata, Parameters param, Parameters dataParam, string filename, string geneName, string expSeries1 = "Expression Series 1", int numOfSeries = 2)
        {
            char separator = '\t';

            //gene name column name is not included in the file so need to replace it

            //gene name
            ReplaceFirstLine(filename, geneName);

            
            string[] colNames = TabSep.GetColumnNames(filename, 0, PerseusUtils.commentPrefix,
                PerseusUtils.commentPrefixExceptions, null, separator);

            string[][] cols = TabSep.GetColumns(colNames, filename, 0, PerseusUtils.commentPrefix,
                PerseusUtils.commentPrefixExceptions, separator);
            int nrows = TabSep.GetRowCount(filename);



            string[] expressionColumnsNames = ArrayUtils.Concat(mdata.ColumnNames, mdata.NumericColumnNames);


            mdata.Clear();
            mdata.Name = "PECA Analysis";
            mdata.Values.Init(nrows, 0);
            mdata.SetAnnotationColumns(new List<string>(colNames), new List<string>(colNames), new List<string[]>(cols), new List<string>(),
                new List<string>(), new List<string[][]>(), new List<string>(), new List<string>(), new List<double[]>(),
                new List<string>(), new List<string>(), new List<double[][]>());

            //be careful with changes of Number of time points in the future
            int numOfExpCols = numOfSeries * dataParam.GetParam<int[]>(expSeries1).Value.Length;

            //file format is structured so that expressions columns are before numeric ones
            //so convert the numeric ones before expression columns

            //first column guaranteed to be the name column
            int[] expList = Enumerable.Range(1, numOfExpCols).ToArray();
            int[] numericList = Enumerable.Range(numOfExpCols+1, colNames.Count() - numOfExpCols - 1).ToArray();
            
            StringToNumerical(numericList, mdata);
            StringToExpression(expList, mdata);


        }

        //returns the CORE input parameter lines
        public static string[] GetInputParamLines(Parameters param, Parameters dataParam, bool isRNAOnly = false)
        {
            int seriesLen = isRNAOnly ? dataParam.GetParam<int[]>(PECAParameters.RNAexp).Value.Length : dataParam.GetParam<int[]>(PECAParameters.series1).Value.Length;

            string tp = string.Join(" ", Enumerable.Range(0, seriesLen / param.GetParam<int>("Number of Replicates").Value));

            string lineX, lineY;

            if (isRNAOnly)
            {
                string needLoggedString = PluginPECA.Utils.GetBase(dataParam.GetParamWithSubParams<int>(PECAParameters.dataFormGeneric)) == 0 ? "0" : "1";
                lineX = "FILE_X = ";
                lineY = "FILE_Y = fileY.txt " + needLoggedString;
            } else {
                //isLN check
                //if yes then false - does not need to be logged
                //if not true - needs to be logged
                string needLoggedString1 = PluginPECA.Utils.GetBase(dataParam.GetParamWithSubParams<int>(PECAParameters.dataForm1)) == 0 ? "0" : "1";
                string needLoggedString2 = PluginPECA.Utils.GetBase(dataParam.GetParamWithSubParams<int>(PECAParameters.dataForm2)) == 0 ? "0" : "1";
                //string isLoggedString = param.GetParam<bool>("Is log2(x)?").Value ? "false" : "true";
                lineX = "FILE_X = fileX.txt " + needLoggedString1;
                lineY = "FILE_Y = fileY.txt " + needLoggedString2;
            }

            string[] lines =
            {
                lineX,
                lineY,
                string.Format("N_REP = {0}", param.GetParam<int>("Number of Replicates").Value),
                "TIME = " + tp,
                string.Format("N_BURN = {0}", param.GetParam<int>("MCMC Burn-In").Value),
                string.Format("N_THIN = {0}", param.GetParam<int>("MCMC Thinning").Value),
                string.Format("N_SAMPLE = {0}", param.GetParam<int>("MCMC Samples").Value),
                //string.Format("N_BURN = {0}", getMCMCParam(param.GetParamWithSubParams<int>("MCMC Burn-In"), 0)),
                //string.Format("N_THIN = {0}", getMCMCParam(param.GetParamWithSubParams<int>("MCMC Thinning"), 1)),
                //string.Format("N_SAMPLE = {0}", getMCMCParam(param.GetParamWithSubParams<int>("MCMC Samples"), 2)),
                ""
            };
            //success if 0

            //replace with smoothing

            ParameterWithSubParams<bool> getSmoothing = param.GetParamWithSubParams<bool>("Smoothing");

            lines[lines.Length - 1] =  getSmoothing.Value ? string.Format("SMOOTHING= {0} {1}", getSmoothing.GetSubParameters().GetParam<double>(PECAParameters.smoothingPar1).Value, getSmoothing.GetSubParameters().GetParam<double>(PECAParameters.smoothingPar2).Value) : "";

            return lines;


        }

        //input is the paratemer file
        public static int WriteInputParam(Parameters param, Parameters dataParam, string workingDir, bool isRNAOnly = false)
        {

            string[] lines = GetInputParamLines(param, dataParam, isRNAOnly);

            string inputLocation = System.IO.Path.Combine(workingDir, "input");
            System.IO.File.WriteAllLines(inputLocation, lines);
            return 0;
        }

        //CPS, i.e. gene enrichment analysis, parameters
        public static string[] GetGSAInputParamLines(Parameters param, int option)
        {

            //empty last line for PS,R or N
            string lastLine = "";

            switch(option)
            {
                //PECA_CORE
                case -1:
                    break;
                //PECA_N
                case -2:
                    //don't need this part since the program already checks
                    //lastLine = "MODULE=Adjacency_list.txt";
                    break;
                //SYNTHESIS = 1, DEGRADATION = 0
                case 0:
                    //degradation
                    lastLine = "SYNTHESIS=0";
                    break;
                case 1:
                    //synthesis
                    lastLine = "SYNTHESIS=1";
                    break;
                default:
                    return null;
            }

            string[] lines =
            {
                //get FDR here
                string.Format("FDR_cutoff={0}", param.GetParam<Double>(PECAParameters.fdrCutoff).Value),
                "GO_term_table=networkFile.txt",
                //minimum percentage 
                string.Format("BACKGROUND = {0} {1}", param.GetParam<int>(PECAParameters.backgroundPar1).Value, param.GetParam<int>(PECAParameters.backgroundPar2).Value),
                lastLine
            };
            
            return lines;


        }

        public static int WriteGSAInputParam(Parameters param, int option, string workingDir)
        {
            string[] lines = GetGSAInputParamLines(param, option);
            if (lines == null) return -1;
            string inputLocation = System.IO.Path.Combine(workingDir, "gsa_input");
            System.IO.File.WriteAllLines(inputLocation, lines);
            return 0;
        }

        public static IMatrixData getGSA(Parameters param, IMatrixData mdata, string workingDirectory, int option, ProcessInfo processInfo, out string errorString)
        {
            string folderDir = null;
            if (option == 0)
            {
                folderDir = "degradationGSA";
            } else if (option == 1)
            {
                folderDir = "synthesisGSA";
            }
            errorString = null;
            string modulePath = param.GetParam<string>(PECAParameters.networkFile).Value;
            string newWorkingDir = folderDir == null ? workingDirectory : Path.Combine(workingDirectory, @folderDir);
            string moduleDestination = Path.Combine(newWorkingDir, "networkFile.txt");
            //string moduleDestination = Path.Combine(@workingDirectory, "networkFile.txt");

            Directory.CreateDirectory(newWorkingDir);
            File.Copy(modulePath, moduleDestination, true);

            //copy output
            if (option == 0 || option == 1)
            {
                string oldCPSoutputLoc = Path.Combine(workingDirectory, @"data_R_CPS.txt");
                string newCPSOutputLoc = Path.Combine(newWorkingDir, @"data_R_CPS.txt");
                File.Copy(oldCPSoutputLoc, newCPSOutputLoc, true);
            }


            string convertErr = ConvertUnix2Dos(moduleDestination, processInfo);
            if (convertErr != null)
            {
                errorString = convertErr;
                return mdata;
            }
            //insert option here
            if (WriteGSAInputParam(param, option, newWorkingDir) != 0)
            {
                errorString = "Unable To Process GSA Parameters";
                return mdata;
            }
            string exeCPS = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @".\bin\PECAInstallations\gsa.exe");
            string inputCPSLocation = "\"" + System.IO.Path.Combine(@newWorkingDir, @".\gsa_input") + "\"";
            if (ExternalProcess.RunExe(exeCPS, inputCPSLocation, newWorkingDir, processInfo.Status, processInfo.Progress, -1, -1, out string processInfoErrString4) != 0)
            {
                errorString = processInfoErrString4;
                return mdata;
            }
            //processInfo.ErrString = processInfoErrString5;
            //supplTables = new IMatrixData[] { PluginPECA.Utils.GetGOEnr(mdata, workingDirectory) };
            return GetGOEnr(mdata, @newWorkingDir, option);
        }
        
        public static int WriteInputFilesCond(IMatrixData mdata, Parameters param, Parameters dataParam, bool isRNAOnly, string workingDirectory, out string errString, string expSeries1 = "Expression Series 1", string expSeries2 = "Expression Series 2")
        {
            if (isRNAOnly)
            {
                //new writeinput here
                return WriteRNAFiles(mdata, param, dataParam, workingDirectory, out errString, PluginPECA.PECAParameters.RNAexp);
            }
            else
            {
                return WriteInputFiles(mdata, param, dataParam, workingDirectory, out errString);
            }
        }

        public static int WriteRNAFiles(IMatrixData mdata, Parameters param, Parameters dataParam, string workingDirectory, out string errString, string RNAexp)
        {
            //need to write a general version
            //accepts text only data for now

            string fileTempY = System.IO.Path.Combine(workingDirectory, @".\tempY.txt");
            string fileY = System.IO.Path.Combine(workingDirectory, @".\fileY.txt");


            IMatrixData mCopyY = (IMatrixData)mdata.Clone();

            int[] nameInd = new[] { param.GetParam<int>("Gene Name Column").Value };

            int[] yInd = dataParam.GetParam<int[]>(RNAexp).Value;

            double baseVal = GetBase(dataParam.GetParamWithSubParams<int>(PECAParameters.dataFormGeneric));

            SetupMDataForInput(mCopyY, yInd, nameInd, baseVal);

            try
            {
                //hacky way of removing comments
                PerseusUtils.WriteMatrixToFile(mCopyY, fileTempY, false);
                RemoveCommentsFromFile(fileTempY, fileY);

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


        public static int WriteInputFiles(IMatrixData mdata, Parameters param, Parameters dataParam, string workingDirectory, out string errString, string expSeries1 = "Expression Series 1", string expSeries2 = "Expression Series 2")
        {
            //need to write a general version
            //accepts text only data for now

            

            //set as global
            string fileTempX = System.IO.Path.Combine(workingDirectory, @".\tempX.txt");
            string fileX = System.IO.Path.Combine(workingDirectory, @".\fileX.txt");
            string fileTempY = System.IO.Path.Combine(workingDirectory, @".\tempY.txt");
            string fileY = System.IO.Path.Combine(workingDirectory, @".\fileY.txt");


            IMatrixData mCopyY = (IMatrixData)mdata.Clone();
            IMatrixData mCopyX = (IMatrixData)mdata.Clone();


            int[] nameInd = new[] { param.GetParam<int>("Gene Name Column").Value } ;

            int[] xInd = dataParam.GetParam<int[]>(expSeries1).Value;
            int[] yInd = dataParam.GetParam<int[]>(expSeries2).Value;

            double baseVal1 = GetBase(dataParam.GetParamWithSubParams<int>(PECAParameters.dataForm1));
            double baseVal2 = GetBase(dataParam.GetParamWithSubParams<int>(PECAParameters.dataForm2));

            SetupMDataForInput(mCopyX, xInd, nameInd, baseVal1);
            SetupMDataForInput(mCopyY, yInd, nameInd, baseVal2);

            try
            {
                //hacky way of removing comments
                PerseusUtils.WriteMatrixToFile(mCopyX, fileTempX, false);
                RemoveCommentsFromFile(fileTempX, fileX);
                PerseusUtils.WriteMatrixToFile(mCopyY, fileTempY, false);
                RemoveCommentsFromFile(fileTempY, fileY);

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

        //gene enrichment analysis for both Basic PECA and PECA-N
        public static IMatrixData GetGOEnr(IMatrixData mdata, string workingDir, int option)//, out string errString)
        {
            char separator = '\t';

            string filename = Path.Combine(workingDir, @".\Goterms.txt");

            IMatrixData mNew = (IMatrixData)mdata.CreateNewInstance();

            string name = "GSA";

            if (option == 0)
            {
                name = name + "_Degradation";
            } else if (option == 1)
            {
                name = name + "_Synthesis";
            }

            mNew.Clear();
            mNew.Name = name;
            mNew.AltName = name;

            //update
            //mNew.AltName = "Gene Set Enrichment Analysis";
            //mNew.Description = "Gene Set Enrichment Analysis";


            string[] colNames = TabSep.GetColumnNames(filename, 0, PerseusUtils.commentPrefix,
                PerseusUtils.commentPrefixExceptions, null, separator);

            string[][] cols = TabSep.GetColumns(colNames, filename, 0, PerseusUtils.commentPrefix,
                PerseusUtils.commentPrefixExceptions, separator);

            int nrows = TabSep.GetRowCount(filename);

            mNew.Values.Init(nrows, 0);

            mNew.SetAnnotationColumns(new List<string>(colNames), new List<string>(colNames), new List<string[]>(cols), new List<string>(),
                new List<string>(), new List<string[][]>(), new List<string>(), new List<string>(), new List<double[]>(),
                new List<string>(), new List<string>(), new List<double[][]>());


            //convert the ones not matching regex to numeric
            string pattern = @"^((?!id|name|members).)*$";
            Regex numericReg = new Regex(pattern);
            List<int> numericList = new List<int>();

            for (int i = 0; i < colNames.Length; i++)
            {
                if (numericReg.Match(colNames[i]).Success)
                {
                    numericList.Add(i);
                }
            }
            StringToNumerical(numericList, mNew);
            return mNew;
        }




        public static int GetMCMCParam(ParameterWithSubParams<int> option, int MCMCParam)
        {
            //for readability
            int BURN_IN = 0;
            int THIN = 1;
            int N_SAMPLE = 2;

            Parameters subParams = option.GetSubParameters();
            if (MCMCParam == BURN_IN)
            {
                switch (option.Value)
                {
                    case 0:
                        return 500;
                    case 1:
                        return 5000;
                    case 2:
                        return 10000;
                    case 3:
                        return subParams.GetParam<int>("#").Value;
                    case 4:
                        return 10;
                }
            }
            else if (MCMCParam == THIN)
            {
                switch (option.Value)
                {
                    case 0:
                        return 1;
                    case 1:
                        return 10;
                    case 2:
                        return 50;
                    case 3:
                        return subParams.GetParam<int>("#").Value;
                    case 4:
                        return 1;
                }
            }

            else if (MCMCParam == N_SAMPLE)
            {
                switch (option.Value)
                {
                    case 0:
                        return 500;
                    case 1:
                        return 1000;
                    case 2:
                        return 2000;
                    case 3:
                        return subParams.GetParam<int>("#").Value;
                    case 4:
                        return 10;
                }
            }

            else
            {
                //this will break the code
                return -1;
            }

            //to prevent Visual Studio from complaining that not all return a value
            return -1;
        }

        //unix2dos is part of dos2unix which is released under FreeDSB
        //go https://waterlan.home.xs4all.nl/dos2unix/COPYING.txt for full license details
        public static string ConvertUnix2Dos(string filename, ProcessInfo processInfo)
        {
            string unix2dos = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @".\bin\PECAInstallations\unix2dos.exe");

            string filepath = "\"" + filename + "\"";

            if (ExternalProcess.RunExe(unix2dos, filepath, null, processInfo.Status, processInfo.Progress, -1, -1, out string processInfoErrString) != 0)
            {
                return processInfoErrString;
            }

            return null;
        }
       

        //function obtained from PerseusPluginLib/Rearrange/ChangeColumnType.cs
        private static void StringToNumerical(IList<int> colInds, IMatrixData mdata)
        {
            int[] inds = ArrayUtils.Complement(colInds, mdata.StringColumnCount);
            string[] name = ArrayUtils.SubArray(mdata.StringColumnNames, colInds);
            string[] description = ArrayUtils.SubArray(mdata.StringColumnDescriptions, colInds);
            string[][] str = ArrayUtils.SubArray(mdata.StringColumns, colInds);
            var newNum = new double[str.Length][];
            for (int j = 0; j < str.Length; j++)
            {
                newNum[j] = new double[str[j].Length];
                for (int i = 0; i < newNum[j].Length; i++)
                {
                    if (str[j][i] == null || str[j][i].Length == 0)
                    {
                        newNum[j][i] = double.NaN;
                    }
                    else
                    {
                        string x = str[j][i];
                        double d;
                        bool success = double.TryParse(x, out d);
                        newNum[j][i] = success ? d : double.NaN;
                    }
                }
            }
            mdata.NumericColumnNames.AddRange(name);
            mdata.NumericColumnDescriptions.AddRange(description);
            mdata.NumericColumns.AddRange(newNum);
            mdata.StringColumns = ArrayUtils.SubList(mdata.StringColumns, inds);
            mdata.StringColumnNames = ArrayUtils.SubList(mdata.StringColumnNames, inds);
            mdata.StringColumnDescriptions = ArrayUtils.SubList(mdata.StringColumnDescriptions, inds);
        }

        //function obtained from PerseusPluginLib/Rearrange/ChangeColumnType.cs
        private static void NumericToString(IList<int> colInds, IMatrixData mdata)
        {
            int[] inds = ArrayUtils.Complement(colInds, mdata.NumericColumnCount);
            string[] name = ArrayUtils.SubArray(mdata.NumericColumnNames, colInds);
            string[] description = ArrayUtils.SubArray(mdata.NumericColumnDescriptions, colInds);
            double[][] num = ArrayUtils.SubArray(mdata.NumericColumns, colInds);
            var newString = new string[num.Length][];
            for (int j = 0; j < num.Length; j++)
            {
                newString[j] = new string[num[j].Length];
                for (int i = 0; i < newString[j].Length; i++)
                {
                    double x = num[j][i];
                    newString[j][i] = "" + x;
                }
            }
            mdata.StringColumnNames.AddRange(name);
            mdata.StringColumnDescriptions.AddRange(description);
            mdata.StringColumns.AddRange(newString);
            mdata.NumericColumns = ArrayUtils.SubList(mdata.NumericColumns, inds);
            mdata.NumericColumnNames = ArrayUtils.SubList(mdata.NumericColumnNames, inds);
            mdata.NumericColumnDescriptions = ArrayUtils.SubList(mdata.NumericColumnDescriptions, inds);
        }
        //function obtained from PerseusPluginLib/Rearrange/ChangeColumnType.cs
        private static void ExpressionToNumeric(IList<int> colInds, IMatrixData mdata)
        {
            int[] remainingInds = ArrayUtils.Complement(colInds, mdata.ColumnCount);
            foreach (int colInd in colInds)
            {
                double[] d = ArrayUtils.ToDoubles(mdata.Values.GetColumn(colInd));
                mdata.AddNumericColumn(mdata.ColumnNames[colInd], mdata.ColumnDescriptions[colInd], d);
            }
            mdata.ExtractColumns(remainingInds);
        }
        //function obtained from PerseusPluginLib/Rearrange/ChangeColumnType.cs
        private static void StringToExpression(IList<int> colInds, IMatrixData mdata)
        {
            int[] inds = ArrayUtils.Complement(colInds, mdata.StringColumnCount);
            string[] names = ArrayUtils.SubArray(mdata.StringColumnNames, colInds);
            string[] descriptions = ArrayUtils.SubArray(mdata.StringColumnDescriptions, colInds);
            string[][] str = ArrayUtils.SubArray(mdata.StringColumns, colInds);
            var newEx = new float[str.Length][];
            for (int j = 0; j < str.Length; j++)
            {
                newEx[j] = new float[str[j].Length];
                for (int i = 0; i < newEx[j].Length; i++)
                {
                    float f;
                    bool success = float.TryParse(str[j][i], out f);
                    newEx[j][i] = success ? f : float.NaN;
                }
            }
            float[,] newExp = new float[mdata.RowCount, mdata.ColumnCount + str.Length];
            float[,] newQual = new float[mdata.RowCount, mdata.ColumnCount + str.Length];
            bool[,] newIsImputed = new bool[mdata.RowCount, mdata.ColumnCount + str.Length];
            for (int i = 0; i < mdata.RowCount; i++)
            {
                for (int j = 0; j < mdata.ColumnCount; j++)
                {
                    newExp[i, j] = mdata.Values.Get(i, j);
                    newQual[i, j] = mdata.Quality.Get(i, j);
                    newIsImputed[i, j] = mdata.IsImputed[i, j];
                }
                for (int j = 0; j < newEx.Length; j++)
                {
                    newExp[i, j + mdata.ColumnCount] = newEx[j][i];
                    newQual[i, j + mdata.ColumnCount] = float.NaN;
                    newIsImputed[i, j + mdata.ColumnCount] = false;
                }
            }
            mdata.Values.Set(newExp);
            mdata.Quality.Set(newQual);
            mdata.IsImputed.Set(newIsImputed);
            mdata.ColumnNames.AddRange(names);
            mdata.ColumnDescriptions.AddRange(descriptions);
            mdata.StringColumns = ArrayUtils.SubList(mdata.StringColumns, inds);
            mdata.StringColumnNames = ArrayUtils.SubList(mdata.StringColumnNames, inds);
            mdata.StringColumnDescriptions = ArrayUtils.SubList(mdata.StringColumnDescriptions, inds);
            for (int i = 0; i < mdata.CategoryRowCount; i++)
            {
                mdata.SetCategoryRowAt(ExtendCategoryRow(mdata.GetCategoryRowAt(i), str.Length), i);
            }
            for (int i = 0; i < mdata.NumericRows.Count; i++)
            {
                mdata.NumericRows[i] = ExtendNumericRow(mdata.NumericRows[i], str.Length);
            }
        }
        //function obtained from PerseusPluginLib/Rearrange/ChangeColumnType.cs
        private static double[] ExtendNumericRow(IList<double> numericRow, int add)
        {
            var result = new double[numericRow.Count + add];
            for (int i = 0; i < numericRow.Count; i++)
            {
                result[i] = numericRow[i];
            }
            for (int i = numericRow.Count; i < numericRow.Count + add; i++)
            {
                result[i] = double.NaN;
            }
            return result;
        }
        //function obtained from PerseusPluginLib/Rearrange/ChangeColumnType.cs
        private static string[][] ExtendCategoryRow(IList<string[]> categoryRow, int add)
        {
            var result = new string[categoryRow.Count + add][];
            for (int i = 0; i < categoryRow.Count; i++)
            {
                result[i] = categoryRow[i];
            }
            for (int i = categoryRow.Count; i < categoryRow.Count + add; i++)
            {
                result[i] = new string[0];
            }
            return result;
        }

        public static void RemoveCommentsFromFile(string filename, string destFile)//, int lines_to_delete)
        {
            using (StreamReader reader = new StreamReader(filename))
            using (StreamWriter writer = new StreamWriter(destFile))
            {
                //writer.WriteLine(reader.ReadLine());
                //while (lines_to_delete-- > 0)
                //    reader.ReadLine();

                //string line;
                //while ((line = reader.ReadLine()) != null)
                //    writer.WriteLine(line);
                writer.WriteLine(reader.ReadLine());
                string line = reader.ReadLine();
                while (line.Contains("#!"))
                {
                    line = reader.ReadLine();
                }
                writer.WriteLine(line);
                while ((line = reader.ReadLine()) != null)
                    writer.WriteLine(line);
                //while (lines_to_delete-- > 0)
                //    reader.ReadLine();

                //string line;
                //while ((line = reader.ReadLine()) != null)
                //    writer.WriteLine(line);

            }
            File.Delete(filename);
        }


        //using this because the column names are not properly written with the name of the genes from
        //the executable produced
        private static void ReplaceFirstLine(string fileName, string geneName)
        {
            string[] fileLines = File.ReadAllLines(fileName);
            if (!fileLines[0].Contains(geneName))
            {
                fileLines[0] = geneName + "\t" + fileLines[0];
                File.WriteAllLines(fileName, fileLines);
            }
        }

        public static double GetBase(ParameterWithSubParams<int> form)
        {
            int RAW = 0;
            int LN = 1;
            int LOG10 = 2;
            int LOG2 = 3;
            int CUSTOM = 4;


            int formType = form.Value;

            if (formType == RAW) return -1;

            if (formType == LN) return 0;

            if (formType == LOG10)
            {
                return 10;
            }
            else if (formType == LOG2)
            {
                return 2;
            }
            else if (formType == CUSTOM)
            {
                Parameters subParams = form.GetSubParameters();
                return subParams.GetParam<double>("Base").Value;
            }
            else
            {
                return -2;
            }

        }

        public static void SetupMDataForInput(IMatrixData data, int[] columnIndx, int[] nameInd, double baseVal)
        {

            data.StringColumns = ArrayUtils.SubList(data.StringColumns, nameInd);
            data.StringColumnNames = ArrayUtils.SubList(data.StringColumnNames, nameInd);
            data.StringColumnDescriptions = ArrayUtils.SubList(data.StringColumnDescriptions, nameInd);

            //hacky way of forcing the order of columns but also possibly the only way for Perseus
            List<int> toConvert = new List<int>();
            List<int> numList = new List<int>();
            int expressInd = 0;
            foreach (int i in columnIndx)
            {
                if (i < data.ColumnCount)
                {
                    toConvert.Add(i);
                    numList.Add(data.NumericColumnCount + expressInd);
                    expressInd += 1;
                }
                else
                {
                    numList.Add(i - data.ColumnCount);
                }
            }

            int[] numArr = numList.ToArray();
            //convert expression to numeric
            data.ExtractColumns(toConvert.ToArray());
            ExpressionToNumeric(Enumerable.Range(0, data.ColumnCount).ToArray(), data);

            data.NumericColumns = ArrayUtils.SubList(data.NumericColumns, numArr);
            
            //change data form depending whether needed
            if (baseVal > 0) {
                foreach (int col in numArr)
                {
                    for (int i=0; i<data.RowCount; i++)
                    {
                        data.NumericColumns[col][i] = Math.Pow(baseVal, data.NumericColumns[col][i]); 
                    }
                }
            }

            data.NumericColumnNames = ArrayUtils.SubList(data.NumericColumnNames, numArr);
            data.NumericColumnDescriptions = ArrayUtils.SubList(data.NumericColumnDescriptions, numArr);

            NumericToString(Enumerable.Range(0, numArr.Length).ToArray(), data);


            for (int j=0; j<data.StringColumnCount; j++)
            {
                for (int i=0; i<data.RowCount; i++)
                {
                    data.StringColumns[j][i] = string.Equals(data.StringColumns[j][i], "NaN") ? "NA" : data.StringColumns[j][i];
                }
            }

            //clearing irrelevant info
            data.ClearMultiNumericColumns();
            data.ClearMultiNumericRows();
            data.ClearCategoryColumns();
            data.ClearCategoryRows();

        }

        //from perseus-plugins/PerseusPluginLib/Utils/PerseusPluginUtils.cs
        public static Bitmap2 GetImage(string file)
        {
            //remember to change the image files Build Action to Embedded Resource
            Assembly thisExe = Assembly.GetExecutingAssembly();
            //path is default namespace + folder, where each is separated by '.'
            Stream file1 = thisExe.GetManifestResourceStream( "PluginPECA.Resources." + file);
            if (file1 == null)
            {
                return null;
            }
            Bitmap2 bm = Image2.ReadImage(file1);
            file1.Close();
            return bm;
        }




    }
}
