# Parameters For Sample Files

## Description
	PECA Core uses N_Sample.txt as dataset and module.txt as Biological Function Annotation File
	PECA-N uses N_Sample.txt as dataset and module.txt as Biological Function Annotation File
	PECA-pS uses pS_Sample.txt as dataset
	PECA-R uses R_Sample.txt as dataset

## PECA Core Parameters
	Working Directory: //select a preferred directory where results should be saved
	Number of Replicates: 2
	[x] Feature Smoothing:
		Gaussian Kernel Signal Variance: 2.0
		Gaussian Kernel Lengthscale: 1.0
	[x] Function Enrichment Analysis:
		Biological Function Annotation File: //select module.txt
		Enrichment Analysis FDR Cutoff: 0.05
		Minimum % of Genes to Consider a Pathway: 0
		Minimum Number for Hypothesis Testing: 1
	Gene Name Column: ENSG
	Expression Series 1:
		R1t0
		R1t0.5
		... (ordered by time then by replicate)
		R3t30
	Data Input Form 1: ln
	Expression Series 2:
		LFQ.intensity.1_0h_RS1
		LFQ.intensity.2_05h_RS1
		... (ordered by time then by replicate)
		LFQ.intensity.8_30h_RS3
	Data Input Form 2: ln
	MCMC Burn-In: 1000
	MCMC Thinning: 10
	MCMC Samples: 1000

## PECA-N Parameters
	Working Directory: //select a preferred directory where results should be saved
	Number of Replicates: 2
	[x] Feature Smoothing:
		Gaussian Kernel Signal Variance: 2.0
		Gaussian Kernel Lengthscale: 1.0
	Biological Function Annotation File: //select module.txt
	Minimum Size of Pathways: 0
	Maximum Size of Pathways: 20
	Enrichment Analysis FDR Cutoff: 0.05
	Minimum % of Genes to Consider a Pathway: 20
	Minimum Number for Hypothesis Testing: 5
	Gene Name Column: ENSG
	Expression Series 1:
		R1t0
		R1t0.5
		... (ordered by time then by replicate)
		R3t30
	Data Input Form 1: ln
	Expression Series 2:
		LFQ.intensity.1_0h_RS1
		LFQ.intensity.2_05h_RS1
		... (ordered by time then by replicate)
		LFQ.intensity.8_30h_RS3
	Data Input Form 2: ln
	MCMC Burn-In: 1000
	MCMC Thinning: 10
	MCMC Samples: 1000

## PECA-pS Parameters
	Working Directory: //select a preferred directory where results should be saved
	Time Points: 0 1 2 4 6 9 12
	Number of Replicates: 2
	[x] Feature Smoothing:
		Gaussian Kernel Signal Variance: 2.0
		Gaussian Kernel Lengthscale: 1.0
	Gene Name Column: Gene_Group
	mRNA Concentration Data:
		R1_LPS_0h
		R1_LPS_01h
		... (ordered by time then by replicate)
		R2_LPS_12h
	Data Input Form 1: Raw
	PRE/REF SILAC Data:
		ML_LPS_R1_0h
		ML_LPS_R1_01h
		... (ordered by time then by replicate)
		ML_LPS_R2_12h
	Data Input Form 2: ln
	NEW/REF SILAC Data:
		ML_LPS_R1_0h
		ML_LPS_R1_01h
		... (ordered by time then by replicate)
		ML_LPS_R2_12h
	Data Input Form 3: ln
	MCMC Burn-In: 1000
	MCMC Thinning: 10
	MCMC Samples: 1000

## PECA-R Parameters
	Working Directory: //select a preferred directory where results should be saved
	Time Points: 0 1 2 4 6 9 12
	Number of Replicates: 2
	[x] Feature Smoothing:
		Gaussian Kernel Signal Variance: 2.0
		Gaussian Kernel Lengthscale: 1.0
	Gene Name Column: Gene_Group
	mRNA Concentration Data:
		R1_LPS_0h
		R1_LPS_01h
		... (ordered by time then by replicate)
		R2_LPS_12h
	Data Input Form 1: ln
	Protein Concentration Data:
		M/L_LPS_R1_0h
		M/L_LPS_R1_01h
		... (ordered by time then by replicate)
		M/L_LPS_R2_12h
	Data Input Form 2: raw
	MCMC Burn-In: 1000
	MCMC Thinning: 10
	MCMC Samples: 1000


