# PluginPECA

Perseus Plugin for PECA

<img src="https://github.com/PECAplus/Perseus-PluginPECA/wiki/images/PECALogo.png" align="left" width="200" height="150">



**Protein Expression Control Analysis** (**PECA**) is a statistical toolbox to analyze time-series multi-omics dataset where molecules in one -omics platform serve as template for synthesis of the other.  For example, PECA can analyze paired RNA and protein time series data in order to identify change points of the impact translation and protein degradation on changes in protein concentration *given* changing RNA concentrations. 



The modules are presented mostly for the analysis of paired protein-RNA data. However, we note that PECA can be applied to any analogous dataset. For example, one can use PECA can analyze paired DNA and RNA concentration data (where the DNA concentration is typically set to 1) and provide a change point analysis for the impact of transcription and RNA degradation on changes in RNA concentration, given DNA.

The principal method was published [here](http://pubs.acs.org/doi/abs/10.1021/pr400855q) (Teo et al, *J. Prot. Res.* 2014). Additional information on the methods and the modules contained in PECAplus are described in a forthcoming manuscript. 


## Full Documentation

See the [Wiki](https://github.com/PECAplus/Perseus-PluginPECA/wiki) for full documentation and examples.

## Requirements

64 bit Windows with .NET Framework 4.5 or higher (See [Perseus Requirements](http://www.coxdocs.org/doku.php?id=perseus:common:download_and_installation))

PERSEUS version 1.6.0.2 (See [Perseus Download and Installation Guide](http://www.coxdocs.org/doku.php?id=perseus:common:download_and_installation#download))

## Installing the plugin

* [Download Perseus-PluginPECA ZIP](https://github.com/PECAplus/Perseus-PluginPECA/archive/master.zip)
* Unzip `Perseus-PluginPECA-master.zip`
* Locate the directory of `Perseus.exe`, which contains `bin` folder
* Copy/Cut `pluginPECA.dll` file and `PECAInstallations` folder from `Perseus-PluginPECA-master\Plugin`
* Paste into the `bin` folder

## Bugs and Feedback

For bugs, questions and discussions please use the [GitHub Issues](https://github.com/PECAplus/Perseus-PluginPECA/issues).

## License

Still needs to be updated ...


