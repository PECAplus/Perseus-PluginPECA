# PECA Plugin

This page is the PECA Plugin manual.

Instructions and links to the examples can be found in this page.

# Table of Contents

- [PECA Plugin](#peca-plugin)
- [Table of Contents](#table-of-contents)
- [PECAplus Overview](#pecaplus-overview)
- [Requirements](#requirements)
- [PECA Core](#peca-core)
  - [Description](#description)
  - [Parameters](#parameters)
    - [Working Directory](#working-directory)
    - [About the Data](#about-the-data)
      - [Number of Replicates](#number-of-replicates)
    - [Smoothing](#smoothing)
      - [Variance parameter of the Gaussian Process](#variance-parameter-of-the-gaussian-process)
      - [Gaussian Process scale parameter](#gaussian-process-scale-parameter)
    - [Function Enrichment Analysis](#function-enrichment-analysis)
      - [Biological Function Annotation File](#biological-function-annotation-file)
      - [Enrichment Analysis FDR Cutoff](#enrichment-analysis-fdr-cutoff)
      - [Minimum % of Genes to Consider a Pathway to Be Tested](#minimum-%25-of-genes-to-consider-a-pathway-to-be-tested)
      - [Minimum Number of Genes For Hypothesis Testing](#minimum-number-of-genes-for-hypothesis-testing)
    - [Select Data](#select-data)
      - [Gene Name Column](#gene-name-column)
      - [Expression Series 1](#expression-series-1)
      - [Data Input Form 1](#data-input-form-1)
      - [Expression Series 2](#expression-series-2)
      - [Data Input Form 2](#data-input-form-2)
    - [MCMC Parameters](#mcmc-parameters)
      - [MCMC Burn-In](#mcmc-burn-in)
      - [MCMC Thinning](#mcmc-thinning)
      - [MCMC Samples](#mcmc-samples)
  - [Output](#output)
    - [General Output](#general-output)
    - [FEA Output (if FEA had been checked)](#fea-output-if-fea-had-been-checked)
- [PECA-N](#peca-n)
  - [Description](#description-1)
  - [Parameters](#parameters-1)
    - [Working Directory](#working-directory-1)
    - [About Data](#about-data)
    - [Smoothing](#smoothing-1)
    - [Module Info](#module-info)
      - [Function Annotation File](#function-annotation-file)
      - [Minimum Size of Pathways](#minimum-size-of-pathways)
      - [Maximum Size of Pathways](#maximum-size-of-pathways)
      - [Enrichment Analysis FDR Cutoff](#enrichment-analysis-fdr-cutoff-1)
      - [Minimum % of Genes to Consider a Pathway](#minimum-%25-of-genes-to-consider-a-pathway)
      - [Minimum Number For Hypothesis Testing](#minimum-number-for-hypothesis-testing)
    - [Select Data](#select-data-1)
    - [MCMC Parameters](#mcmc-parameters-1)
  - [Output](#output-1)
- [PECA-pS](#peca-ps)
  - [Description](#description-2)
  - [Parameters](#parameters-2)
    - [Working Directory](#working-directory-2)
    - [About Data](#about-data-1)
      - [Time Points](#time-points)
      - [Number of Replicates](#number-of-replicates-1)
    - [Smoothing](#smoothing-2)
    - [Select Data](#select-data-2)
      - [Gene Name Column](#gene-name-column-1)
      - [Absolute mRNA Concentration Data](#absolute-mrna-concentration-data)
      - [Data Input Form 1](#data-input-form-1-1)
      - [PRE/REF SILAC Data](#preref-silac-data)
      - [Data Input Form 2](#data-input-form-2-1)
      - [NEW/REF SILAC Data](#newref-silac-data)
      - [Data Input Form 3](#data-input-form-3)
    - [MCMC Parameters](#mcmc-parameters-2)
  - [Output](#output-2)
- [PECA-R](#peca-r)
  - [Description](#description-3)
  - [Parameters](#parameters-3)
    - [Working Directory](#working-directory-3)
    - [About the Data](#about-the-data-1)
    - [Smoothing](#smoothing-3)
    - [Select Data](#select-data-3)
      - [Gene Name Column](#gene-name-column-2)
      - [mRNA Concentration Data](#mrna-concentration-data)
      - [Data Input Form 1](#data-input-form-1-2)
      - [Protein Concentration Data](#protein-concentration-data)
      - [Data Input Form 2](#data-input-form-2-2)
    - [MCMC Parameters](#mcmc-parameters-3)
  - [Output](#output-3)


# PECAplus Overview

Protein Expression Control Analysis (PECA) is a statistical toolbox to analyze time-series multi-omics dataset where molecules in one -omics platform serve as template for synthesis of the other.  For example, PECA can analyze paired RNA and protein time series data in order to identify change points of the impact translation and protein degradation on changes in protein concentration *given* changing RNA concentrations. 

The modules are presented mostly for the analysis of paired protein-RNA data. However, we note that PECA can be applied to any analogous dataset. For example, one can use PECA can analyze paired DNA and RNA concentration data (where the DNA concentration is typically set to 1) and provide a change point analysis for the impact of transcription and RNA degradation on changes in RNA concentration, given DNA. 

The principal method was published [here](http://pubs.acs.org/doi/abs/10.1021/pr400855q) (Teo et al, *J. Prot. Res.* 2014). Additional information on the methods and the modules contained in PECAplus are described in a forthcoming manuscript. 

# Requirements

64 bit Windows PC

PERSEUS version 1.6.0.2 or above

Installing the plugin: Place 'pluginPECA.dll' file and 'PECAInstallations' folder inside the bin folder where Perseus.exe is located.

# PECA Core

## Description

PECA Core implements the core functionality for analyzing a two-level time series data set (e.g. paired protein and mRNA concentration data). It identifies significantly regulated genes at each time point with probability scores of significant change points. Note that PECA Core does not deconvolute the contributions of changes in synthesis or degradation. 

## Parameters

### Working Directory

Specifies the directory where input files, output matrices and plots produced by PECA will be saved. It can be specified manually by typing in the path or the folder can be provided by using the "Select" button. 

*Friendly Reminder*: DO NOT SELECT DESKTOP because the tool produces MANY files

### About the Data

#### Number of Replicates

Specification of the number of replicate experiments in the datasets for expression series 1 and 2 (default: 1)

### Smoothing

If checked, Gaussian Process (GP) smoothing will be applied to the datasets (default: unchecked).

#### Variance parameter of the Gaussian Process 

Determines the variation of values from the mean (default 2.0). A small value will result in the function values changing quickly.

#### Gaussian Process scale parameter

Scaling factor that determines the smoothness of the curve (default 1.0). A small value will result in a function that stays close to the mean value.

### Function Enrichment Analysis

If checked, a time-dependent functional enrichment analysis (FEA) will be performed on the output matrix of PECA and the result will be displayed as an additional output matrix (default unchecked). The resulting matrix reports the biological functions whose members are up or down-regulated at specific time points.

#### Biological Function Annotation File

Specifies the file path of the function annotation file that should be used for the time-dependent functional enrichment analysis. 

*Note*: make sure this file is in Windows text file format. Windows text file uses '\r\n' as the end of line character, whereas Unix uses just '\n'. One can produce such a file by saving an Excel files(on Windows) as '.txt'. 

*File Format:*

	First column named as 'Pathwayid', specifying the pathway IDs, e.g. GO terms

	Second column named as the same name as gene name columns provided in the parameters, specifying the genes involved

	Third column named as 'pathway', specifying the pathways involved

[Sample file with annotations for human genes](Samples/module.txt)

#### Enrichment Analysis FDR Cutoff

Defines the FDR cutoff for which enrichment analysis should use when analyzing biological functions at specific time points (default 0.05, i.e. 5%). The value of this parameter should lie between 0 and 1 (e.g., 0.05, 0.1, 0.2).

#### Minimum % of Genes to Consider a Pathway to Be Tested

Specifies the minimum percentage of genes needed in the experimental data for a pathway to be analyzed (default 0). For instance, if 20% is specified, then at least 20 genes need to be present in the experimental data for a pathway of 100 genes. The value of this parameter should lie between 0 and 100.  

#### Minimum Number of Genes For Hypothesis Testing

Specifies the minimum number of significant genes (within the FDR cutoff) from the experimental data for a particular pathway to be reported (default 1). Anything below this number will be assigned a p-value of 1. The value of this parameter should be a positive integer. 

### Select Data

#### Gene Name Column

The selected text column will be used as the gene ID identifiers in PECA analysis (default: first text column).

#### Expression Series 1

The selected expression/numerical columns that should be used as expression series 1 (typically mRNA concentration data), which comes before expression series 2 (typically protein concentration data) where expression series 1 represents degradation and 2 represents synthesis.

	The columns should be ordered by timepoints and then by replicates.

	Order: time point 1 replicate 1, … , time point N replicate 1 , time point 1 replicate 2 , … , time point N replicate 2 

#### Data Input Form 1 

Specification for the data input form of Expression Series 1, i.e. what data transformation has been applied already (default: Raw). 

	Raw: unprocessed, untransformed data.

	ln: log_e transformed data.

	log_2: log_2transformed data.

	log_10:  log(10) transformed data.

	log_custom: log_X transformed data, where X is a specified positive real value

#### Expression Series 2

The selected expression/numerical columns that should be used as expression series 2 (typically protein concentration data) which comes after expression series 1 (default: second half of expression/numerical columns).

Same order as [Expression Series 1](#expression-series-1). The number of columns should also match expression series 1. 
#### Data Input Form 2

Specification for the data input form of Expression Series 2 (default: Raw).

[Same as Data Input Form 1](#data-input-form-1)

### MCMC Parameters

PECA model parameters are estimated using a sampling-based algorithm called MCMC (Markov chain Monte Carlo), which requires the parameters below. All values should be positive integers.

#### MCMC Burn-In

Defines the iterations to be thrown away at the beginning of MCMC run, i.e. the burn-in period (default: 1000).

#### MCMC Thinning

Defines the interval in which iterations of MCMC are recorded (default: 10).

#### MCMC Samples

Defines the total of number of post-burn-in samples to be recorded from MCMC (default: 1000).

## Output

### General Output

The text column is the gene name column provided by [Gene Name Column](#gene-name-column).

The main/expression columns are the log_e transformed Expression Series 1 and Expression Series 2 data sets. 

The numeric columns contain : RX, CPSX, signedCPSX, FDRX, where X is the dummy time point ranging from 0 to N-1, where N is # of time points, representing the index of the actual time points

	RX is the rate ratio for the time interval preceding the specified time point (e.g. if X = 1, then the interval is between time points 1 and 2)

	CPSX is the change point score in absolute value

	signedCPSX is the change point score with signs indicating up/down regulation. Positive sign describes upregulation; negative sign down regulation

	FDRX is the False Discovery Rate

NOTE: the rate ratio itself does NOT inform on the significance or direction of the change. The DIFFERENCE between consecutive rate ratios (adjacent time intervals) describes the direction of change, and the FDR the significance. 

### FEA Output (if FEA had been checked)

The text column contains: the gene name column provided from [Gene Name Column](#gene-name-column), GO_name, GO_id

	GO_name is the name of the Gene Ontology

	GO_id is the ID of the Gene Ontology

The numeric columns contain: MaxSig(Up), MaxSig(Down), Max(Both), GO_size, GO_size_background, GO_EdgeCount, Up(X), Down(X), Sig(X), where X is the time point

	MaxSig(Up) is the maximum value of -log10(Up(X)) for all X

	MaxSig(Down) is the maximum value of -log10(Down(X)) for all X

	Max(Both) is the maximum value of -log10(Sig(X)) for all X

	GO_size is the number of genes in the pathway

	GO_size_background is the number of genes in the pathway that appears in the experimental data

	Up(X) is the p-value calculated from the number of up-regulated genes

	Down(X) is the p-value calculated from the number of down-regulated genes

	Sig(X) is the p-value calculated from the number of up and down-regulated genes

# PECA-N

## Description

PECA-N (Network) module is modified from PECA to incorporate user-provided biological network data into the inference of rate parameter changes in co-regulated genes, where it is assumed that functionally related genes tend to be co-regulated along the time course. 

## Parameters

### Working Directory

See [PECA Core Working Directory](#working-directory)

### About Data

See [PECA Core About Data](#about-data)

### Smoothing

See [PECA Core Smoothing](#smoothing)

### Module Info

Specifies info about the user-provided biological network data. Similar to PECA with GSEA option checked, time-dependent functional enrichment analysis will be performed on the output matrix and a similar additional resulting matrix will be produced.

#### Function Annotation File

See [PECA Core Function Annotation File](#))

#### Minimum Size of Pathways

Specifies the minimum number of significant genes needed in a pathway for it to be considered (default 1). The value should be a non-negative integer.

#### Maximum Size of Pathways

Specifies the maximum number of genes needed in a pathway for it to be considered (default 20). The value should be a non-negative integer.

#### Enrichment Analysis FDR Cutoff

See [PECA Core Enrichment Analysis FDR Cutoff](#heading=h.jzksf2s7d0z8)

#### Minimum % of Genes to Consider a Pathway

See [PECA Core Minimum % of Genes to Consider a Pathway](#heading=h.40in8e6uzeqm)

#### Minimum Number For Hypothesis Testing

See [PECA Core Minimum Number For Hypothesis Testing](#heading=h.ol9sgf47cfoj)

### Select Data

See [PECA Core Select Data](#heading=h.c2xc5jtnr1va)

### MCMC Parameters

See [PECA Core MCMC Parameters](#heading=h.re4q74rpxfs4)

## Output

Produces two matrices as output:

1. See [PECA Core General Output](#heading=h.t8d5f1ehsoyd)

2. Enrichment Analysis Matrix (contains an extra GO_EdgeCount column compared to PECA Core Enrichment Analysis Matrix):

For matrix 2:

The text column contains: the gene name column provided from [Gene Name Column](https://docs.google.com/document/d/1v46yjeTMdryhBfDTc_f8ixbqk1OojArlNT7zquf8AUk/edit#heading=h.ftah2ydkxo03), GO_name, GO_id

	GO_name is the name of the Gene Ontology

	GO_id is the ID of the Gene Ontology

The numeric columns contain: MaxSig(Up), MaxSig(Down), Max(Both), GO_size, GO_size_background, GO_EdgeCount, Up(X), Down(X), Sig(X), where X is the time point

	MaxSig(Up) is the maximum value of -log10(Up(X)) for all X

	MaxSig(Down) is the maximum value of -log10(Down(X)) for all X

	Max(Both) is the maximum value of -log10(Sig(X))for all X

	GO_size is the number of genes in the pathway

	GO_size_background is the number of genes in the pathway that appears in the experimental data

	GO_EdgeCount is the sum of the number of outward edges from all the pathways considered for the given gene 

	Up(X) is the p-value calculated from the number of up-regulated genes

	Down(X) is the p-value calculated from the number of down-regulated genes

	Sig(X) is the p-value calculated from the number of up and down-regulated genes

The output format is almost the same as PECA Core, but the values are different since PECA-N incorporates biological network data when inferring rate parameter changes.

# PECA-pS

## Description

PECA-pS (pulsed SILAC) is a model that separates estimation of rate parameters from pulsed proteomics data, estimating synthesis and degradation rates per gene per measurement interval. 

NOTE: For correct estimation of rates, the model requires that the time course pattern must be monotone decreasing in the channel representing degradation of pre-existing proteins, and monotone increasing in the channel representing synthesis of new proteins. The model does not know if this assumption holds true; the quality of the data needs to be checked by the user. 

NOTE: We explain the tool at an example which used three labels: H for the PRE-existing proteins, M for the NEWly synthesized proteins and L as REFerence label. PECA-pS can also be applied to a two-label SILAC experiment. In that case, the REF label should be the sum of PRE and NEW. 

NOTE: the degradation rates for each interval can be easily average over the entire time course and converted to half-lives using the following formula:

## Parameters

### Working Directory

See [PECA Core Working Directory](#heading=h.w6ot40ipphv0)

### About Data

#### Time Points

Specification of the time points in the datasets(default: 0 1 2 4 6 9 12). Time points can be in any units such as, minutes or hours, but units need to be the same across the entire list. They should be all numeric values, each separated by a whitespace.

#### Number of Replicates

Specification of the number of replicates in the datasets for mRNA data and label-free proteomics data (default: 1)

### Smoothing

See [PECA Core Smoothing](#heading=h.cr5s5co06zir)

### Select Data

#### Gene Name Column

The selected text column will be used as the gene ID identifiers in PECA analysis (default: first text column).

#### Absolute mRNA Concentration Data

The selected expression/numerical columns that should be used as mRNA data.

	The columns should be ordered by timepoints and then by replicates.

	Order:  time point 1 replicate 1, … , time point N replicate 1 , time point 1 replicate 2 , … , time point N replicate 2 

#### Data Input Form 1 

Specification for the data input form of mRNA data, i.e. what data transformation has been applied already (default: Raw). 

	Raw: unprocessed, untransformed data.

	ln: log_e transformed data.

	log_2: log_2 transformed data.

	log_10:  log_10 transformed data.

	log_custom:  log_X transformed data, where X is a specified positive real value

#### PRE/REF SILAC Data 

The selected expression/numerical columns that should be used as the channel representing degradation of pre-existing proteins (PRE), e.g. SILAC ratios of PRE/REF.

Same order as [mRNA Data](#heading=h.woqv3b2k6vnk). The number of columns should also match mRNA Data.

#### Data Input Form 2

Specification for the data input form of PRE/REF SILAC Data (default: Raw).

[Same as Data Input Form 1](#heading=h.bwcih7ex36y3)

#### NEW/REF SILAC Data

The selected expression/numerical columns that should be used as the channel representing synthesis of new proteins, e.g. SILAC ratios of NEW/REF.

Same order as [mRNA Data](#heading=h.woqv3b2k6vnk). The number of columns should also match mRNA Data.

#### Data Input Form 3

Specification for the data input form of PRE/REF SILAC Data (default: Raw).

[Same as Data Input Form 1](#heading=h.bwcih7ex36y3)

### MCMC Parameters

See [PECA Core MCMC Parameters](#heading=h.re4q74rpxfs4)

## Output

The text column is the gene name column provided from [Gene Name Column](#heading=h.uvh8x68czlq9)

The main/expression columns are the log_e transformed mRNA and label-free proteomics data sets. 

The numeric columns contain: RX, DX, signedCPRX, , where X is the dummy time point ranging from 0 to N-1, where N is # of time points, representing the index of the actual time points

	RX is the synthesis rate 

	DX is the degradation rate

	signedCPRX is the change point score for synthesis rates

	signedCPDX is the change point score for degradation rates //edit here

# PECA-R

## Description

PECA-R (Rate) is a model that separates estimation of rate parameters from absolute concentration data, approximating synthesis and degradation rates from via two assumptions. See manuscript for PECAplus for detailed descriptions. 

NOTE: if the data was derived from SILAC experiments, use PECA-pS instead. If one has SILAC data but wishes to use PECA-R for analysis, one has to derive 'artificial' absolute concentrations by summing over the two channels (e.g. heavy and light). 

NOTE: the degradation rates for each interval can be easily average over the entire time course and converted to half-lives using the following formula:

	

## Parameters

### Working Directory

See [PECA Core Working Directory](#heading=h.w6ot40ipphv0)

### About the Data

See [PECA-pS About Data](#heading=h.kl8syrft66fp)

### Smoothing

See [PECA Core Smoothing](#heading=h.cr5s5co06zir)

### Select Data

#### Gene Name Column

The selected text column will be used as the gene ID identifiers in PECA analysis (default: first text column).

#### mRNA Concentration Data

The selected expression/numerical columns that should be used as mRNA data.

	The columns should be ordered by timepoints and then by replicates.

	Order:  time point 1 replicate 1, … , time point N replicate 1 , time point 1 replicate 2 , … , time point N replicate 2

#### Data Input Form 1 

Specification for the data input form of Absolute mRNA Concentration Data, i.e. what data transformation has been applied already (default: Raw). 

	Raw: unprocessed, untransformed data.

	ln: log_e transformed data.

	log_2: log_2 transformed data.

	log_10:  log_10 transformed data.

	log_custom:  log_X transformed data, where X is a specified positive real value

#### Protein Concentration Data

The selected expression/numerical columns that should be used as label-free proteomics data, e.g. M/L SILAC data.//need to change this in the future //see above

Same order as [mRNA Data](#heading=h.bie29c1jlo9q). The number of columns should also match mRNA Data.

#### Data Input Form 2

Specification for the data input form of Data 1 (default: Raw).

[Same as Data Input Form 1](#heading=h.oibjx0e2q5o6)

### MCMC Parameters

See [PECA Core MCMC Parameters](#heading=h.re4q74rpxfs4)

## Output

See [PECA-pS Output](#heading=h.ipg2h2o8u820)
