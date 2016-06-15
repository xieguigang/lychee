---
title: Circos plots
tags: [vb.net, CodeProject, circos, data visualization]
---

![](http://circos.ca/img/news/circos-cancer-cell.png)

Circos is a software package for visualizing data and information. It visualizes data in a circular layout — this makes Circos ideal for exploring relationships between objects or positions. There are other reasons why a circular layout is advantageous, not the least being the fact that it is attractive.

> HOME page: http://circos.ca/

The typically circos data directory consists of two parts of data file: \*.conf circos plots layout configuration file, and \*.txt plots data files in a directory which is named '**data**':

>  + circos.conf
>  + ideogram.conf
>  + ticks.conf
>  + ./data/...

Using circos program just very easy, using cd command enter the plots' config directory which the file circos.conf is located in the terminal, and then inputs the perl script location of circos, circos script will search for the circos.conf file automatically in your directory and then invoke drawing:

```bash
$ ~/circos/bin/circos
```

### Plot types
### Data types