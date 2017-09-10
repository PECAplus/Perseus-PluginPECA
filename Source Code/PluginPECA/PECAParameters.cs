using BaseLibS.Num;
using BaseLibS.Param;
using PerseusApi.Matrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPECA
{
    public static class PECAParameters
    {
        public static string RNAInference = "mRNA Level Inference";
        public static string RNAexp = "mRNA Expression Series";
        public static string series1 = "Expression Series 1";
        public static string series2 = "Expression Series 2";

        public static string smoothingPar1 = "Gaussian Kernel Variance Parameter";
        public static string smoothingPar2 = "Gaussian Kernel Scale Parameter";
        public static string network = "Module";
        public static string edgeFile = "Edge File";
        public static string networkFile = "Biological Function Annotation File";

        //not needed anymore
        public static string minNetwork = "Minimum Size of Pathways";
        public static string maxNetwork = "Maximum Size of Pathways";
        public static string gsa = "Gene Set Analysis";

        public static string fdrCutoff = "Enrichment Analysis FDR Cutoff";
        public static string backgroundPar1 = "Minimum % of Genes to Consider a Pathway";
        public static string backgroundPar2 = "Minimum Number For Hypothesis Testing";

        public static string pecaRSeries1 = "mRNA Concentration Data";

        public static string pecaRSeries2 = "Protein Expression Data";

        public static string pecaPSSeries1 = "mRNA Concentration Data";

        public static string pecaPSSeries2 = "PRE/REF SILAC Data";
        public static string pecaPSSeries3 = "NEW/REF SILAC Data";

        public static string dataForm1 = "Data Input Form 1";
        public static string dataForm2 = "Data Input Form 2";
        public static string dataForm3 = "Data Input Form 3";
        public static string dataFormGeneric = "Data Input Form ";

        public static FolderParam GetWorkingDir()
        {
            return new FolderParam("Working Directory")
            {
                Help = "Specifies the directory where internal input files, output matrices and plots produced by PECA will be saved.",
                
            };
        }

        public static IntParam GetRepNum()
        {
            return new IntParam("Number of Replicates", 1)
            {
                Help = "Specification of the number of replicates in the datasets."
            };
        }

        //public static IntParam GetTimePointNum()
        //{
        //    return new IntParam("Number of Time Points", 8);
        //}

        public static SingleChoiceWithSubParams GetDataForm(string paramName)
        {
            int parNameWidth = 100;
            int totWidth = 600;

            Parameters emptySubParams = new Parameters();
            Parameters customSubParamsBase = new Parameters(new Parameter[] { new DoubleParam("Base", 2.0) });
            string[] baseList = new[] { "Raw", "ln", "log_2", "log_10", "log_custom" };
            Parameters[] subParamListBase = new[] { emptySubParams, emptySubParams, emptySubParams, emptySubParams, customSubParamsBase };

            return new SingleChoiceWithSubParams(paramName, 0)
            {
                Values = baseList,
                SubParams = subParamListBase,
                ParamNameWidth = parNameWidth,
                TotalWidth = totWidth,
                Help = "Specification for the data input form.\nRaw: unprocessed, untransformed data.\nln: log_e transformed data.\nlog_2: log_2 transformed data.\nlog_10:  log_10 transformed data.\nlog_custom: log_X transformed data, where X is a specified positive real value"
            };
        }

        public static SingleChoiceParam GetGeneNameColumn(IMatrixData mdata)
        {
            return new SingleChoiceParam("Gene Name Column", 0)
            {
                Values = mdata.StringColumnNames,
                Help = "The gene ID identifiers in PECA analysis."
            };
        }

        //remove in the future
        public static MultiChoiceParam GetSeries1(IMatrixData mdata)
        {
            string[] expSeriesValuesList = ArrayUtils.Concat(mdata.ColumnNames, mdata.NumericColumnNames);
            int[] def1 = expSeriesValuesList.Length > 0 ? Enumerable.Range(0, expSeriesValuesList.Length / 2).ToArray() : new int[0];
            return new MultiChoiceParam("Expression Series 1", def1)
            {
                Values = expSeriesValuesList,
                Repeats = false,
                Help = "Expression series 1 (typically mRNA concentration data) which comes before expression series 2 (typically protein concentration data) where expression series 1 represents degradation and 2 represents synthesis."
            };
        }

        public static MultiChoiceParam GetSeries2(IMatrixData mdata)
        {
            string[] expSeriesValuesList = ArrayUtils.Concat(mdata.ColumnNames, mdata.NumericColumnNames);
            int[] def2 = expSeriesValuesList.Length > 0 ? Enumerable.Range(expSeriesValuesList.Length / 2, expSeriesValuesList.Length / 2).ToArray() : new int[0];
            return new MultiChoiceParam("Expression Series 2", def2)
            {
                Values = expSeriesValuesList,
                Repeats = false,
                Help = "Expression series 2 (typically protein concentration data) which comes after expression series 1."
            };
        }

        public static MultiChoiceParam GetSeries(IMatrixData mdata, string name, int[] def, string help)
        {
            string[] expSeriesValuesList = ArrayUtils.Concat(mdata.ColumnNames, mdata.NumericColumnNames);
            return new MultiChoiceParam(name, def)
            {
                Values = expSeriesValuesList,
                Repeats = false,
                Help = help
            };
        }


        public static BoolWithSubParams GetSmoothing()
        {
            return new BoolWithSubParams("Smoothing")
            {
                Default = false,
                Help = "Gaussian Kernel Smoothing",
                SubParamsTrue = new Parameters(new Parameter[]{
                    new DoubleParam(smoothingPar1, 2.0){ Help = "Determines the variation of values from the mean. A small value will result in the function values changing quickly." },
                    new DoubleParam(smoothingPar2, 1.0) { Help = "Scaling factor that determines the smoothness of the curve. A small value will result in a function that stays close to the mean value."}
                })
            };
        }

        public static FileParam GetEdgeLoc()
        {
            return new FileParam(edgeFile)
            {
                Help = "Specifies the file path of the edge file as part of biological network data that should be used for the inference of rate parameters."
            };
        }

        public static FileParam GetModuleLoc()
        {
            return new FileParam(networkFile)
            {
                Help = "Specifies the file path of the function annotation file that should be used for the time-dependent functional enrichment analysis."
            };
        }

        public static IntParam GetModuleMin()
        {
            return new IntParam(minNetwork, 1)
            {
                Help = "Specifies the minimum number of genes needed in a pathway for it to be considered"
            };
        }

        public static IntParam GetModuleMax()
        {
            return new IntParam(maxNetwork, 20)
            {
                Help = "Specifies the maximum number of genes needed in a pathway for it to be considered."
            };
        }

        public static DoubleParam GetModuleFDR()
        {
            return new DoubleParam(fdrCutoff, 0.05)
            {
                Help = "Defines the FDR cutoff for which enrichment analysis should use when analyzing biological functions at specific time points"
            };
        }
        
        //need parameter checking at some point
        public static IntParam GetBackground1()
        {
            return new IntParam(backgroundPar1, 0)
            {
                Help = "Specifies the minimum percentage of genes needed in the experimental data for a pathway to be analyzed.\nFor instance, if 20% is specified then at least 20 genes need to be present in the experimental data for a pathway of 100 genes to be considered. The value of this parameter should lie between 0 and 100."
            };
        }
        public static IntParam GetBackground2()
        {
            return new IntParam(backgroundPar2, 1)
            {
                Help = "Specifies the minimum number of genes that must be selected from the experimental data for a particular pathway to be considered for hypothesis testing.\nAnything below this number will be assigned a p-value of 1."
            };
        }

        public static StringParam GetTimepoints()
        {
            return new StringParam("Time Points", "0 1 2 3 4 5")
            {
                Help = "Specification of the time points in the datasets.\nTime points can be in any units such as, minutes or hours, but units need to be the same across the entire list.\nThey should be all numeric values, each separated by a whitespace"
            };
        }



        //getModule but only appears when true
        public static Parameter[] GetConditionalModule()
        {
            return new Parameter[]
            {
                new BoolWithSubParams(gsa)
                {
                    Default = false,
                    Help = "Time-dependent functional enrichment analysis on the output matrix of PECA.\nThe result will be displayed as an additional output matrix",
                    SubParamsTrue = new Parameters(GetModuleCore())
                }
            };
        }

        //core version
        public static Parameter[] GetModuleCore()
        {
            return new Parameter[]
            {
                GetModuleLoc(),
                GetModuleFDR(),
                GetBackground1(),
                GetBackground2()
            };

        }

        //get all of module for PECA N
        public static Parameter[] GetModule()
        {
            return new Parameter[]
            {
                GetEdgeLoc(),
                GetModuleLoc(),
                //not needed anymore with the current version
                //GetModuleMin(),
                //GetModuleMax(),
                GetModuleFDR(),
                GetBackground1(),
                GetBackground2()
            };

        }


        public static Parameter[] GetFeatures()
        {
            return new Parameter[]
            {
                GetSmoothing()
            };
        }

        public static Parameter[] GetAboutData()
        {
            return new Parameter[]
            {
                GetRepNum(),
                //getTimePointNum(),
                //getDataForm()
            };
        }

        public static Parameter[] GetAboutDataWithTP()
        {
            return new Parameter[]
            {
                GetTimepoints(),
                GetRepNum(),
                //getTimePointNum(),
                //getDataForm()
            };
        }

        //for PECA CORE and N
        public static Parameter[] SelectData(IMatrixData mdata)
        {
            return new Parameter[]
            {
                GetGeneNameColumn(mdata),
                GetSeries1(mdata),
                GetDataForm(dataForm1),
                GetSeries2(mdata),
                GetDataForm(dataForm2)
            };

        }

        public static Parameter[] SelectExpData(IMatrixData mdata)
        {
            return new Parameter[]
            {
                GetSeries1(mdata),
                GetDataForm(dataForm1),
                GetSeries2(mdata),
                GetDataForm(dataForm2)
            };

        }

        public static Parameter[] SelectRNAData(IMatrixData mdata)
        {
            string[] expSeriesValuesList = ArrayUtils.Concat(mdata.ColumnNames, mdata.NumericColumnNames);
            int[] def1 = expSeriesValuesList.Length > 0 ? Enumerable.Range(0, expSeriesValuesList.Length / 2).ToArray() : new int[0];
            return new Parameter[]
            {
                GetSeries(mdata, RNAexp, def1, "If mRNA Level Inference is checked, this is the selected expression/numerical columns that should be used as mRNA expression data."),
                GetDataForm(dataFormGeneric)
            };

        }

        //for mRNA level inference
        public static Parameter[] SelectConditionalData(IMatrixData mdata)
        {
            return new Parameter[]
            {
                GetGeneNameColumn(mdata),
                new BoolWithSubParams(RNAInference)
                {
                    Default = false,
                    Help = "If checked, mRNA level inference will be performed, i.e. Expression Series 1 is assumed to be DNA concentrations with 1 as values and Expression Series 2 is set as the given mRNA Expression Series.\nIf not, Expression Series 1 and 2 are both set by the user.",
                    SubParamsTrue = new Parameters(SelectRNAData(mdata)),
                    SubParamsFalse = new Parameters(SelectExpData(mdata))
                }
            };
        }

        //n is the how many data
        //tot is the total column length
        public static int[][] GenDefaultDataList(int n, int tot)
        {
            int interval = tot / n;
            int[][] defList = new int[n][];

            //string[] expSeriesValuesList = ArrayUtils.Concat(mdata.ColumnNames, mdata.NumericColumnNames);
            //int[] def2 = expSeriesValuesList.Length > 0 ? Enumerable.Range(expSeriesValuesList.Length / 2, expSeriesValuesList.Length / 2).ToArray() : new int[0];
            for (int i = 0; i < n; i++)
            {
                defList[i] = tot > 0 ? Enumerable.Range(interval * i, interval).ToArray() : new int[0];
            }

            return defList;
        }

        //2 or more with own descriptions
        public static Parameter[] SelectMultipleData(IMatrixData mdata, string[] nameList, string[] help)
        {

            int[][] defList = GenDefaultDataList(nameList.Length, mdata.ColumnCount + mdata.NumericColumnCount);
            Parameter[] dataArr = new Parameter[2*nameList.Length + 1];
            dataArr[0] = GetGeneNameColumn(mdata);
            for (int i = 0; i < nameList.Length; i++)
            {
                dataArr[2*i + 1] = GetSeries(mdata, nameList[i], defList[i], help[i]);
                dataArr[2 * (i + 1)] = GetDataForm(dataFormGeneric + (i+1).ToString());
            }
            return dataArr;
        }


        public static Parameter[] GetMCMCParams()
        {
            return new Parameter[]
            {
                new IntParam("MCMC Burn-In",1000){
                    Help = "Defines the iterations to be thrown away at the beginning of MCMC run, i.e. burn-in period."
                },
                new IntParam("MCMC Thinning", 10){
                    Help = "Defines the interval in which iterations of MCMC are recorded."
                },
                new IntParam("MCMC Samples", 1000)
                {
                    Help = "Defines the total of number of post-burn-in samples to be recorded from MCMC"
                }
            };
            //Parameters emptySubParams = new Parameters();
            //Parameters customSubParamsBurn = new Parameters(new Parameter[] { new IntParam("#", 5000) });
            //Parameters customSubParamsThin = new Parameters(new Parameter[] { new IntParam("#", 10) });
            //Parameters customSubParamsSample = new Parameters(new Parameter[] { new IntParam("#", 1000) });
            //string[] MCMCValueList = new[] { "Low", "Medium", "High", "Custom", "Test" };
            //Parameters[] subParamListBurn = new[] { emptySubParams, emptySubParams, emptySubParams, customSubParamsBurn, emptySubParams };
            //Parameters[] subParamListThin = new[] { emptySubParams, emptySubParams, emptySubParams, customSubParamsThin, emptySubParams };
            //Parameters[] subParamListSample = new[] { emptySubParams, emptySubParams, emptySubParams, customSubParamsSample, emptySubParams };

            //int parNameWidth = 100;
            //int totWidth = 600;


            //return new Parameter[] {
            //    new SingleChoiceWithSubParams("MCMC Burn-In", 4)
            //        {
            //            Values = MCMCValueList,
            //            SubParams = subParamListBurn,
            //            ParamNameWidth = parNameWidth,
            //            TotalWidth = totWidth
            //        },
            //        new SingleChoiceWithSubParams("MCMC Thinning", 4)
            //        {
            //            Values = MCMCValueList,
            //            SubParams = subParamListThin,
            //            ParamNameWidth = parNameWidth,
            //            TotalWidth = totWidth
            //        },
            //        new SingleChoiceWithSubParams("MCMC Samples", 4)
            //        {
            //            Values = MCMCValueList,
            //            SubParams = subParamListSample,
            //            ParamNameWidth = parNameWidth,
            //            TotalWidth = totWidth
            //        }
            //};
        }

    }


}
