---
title: Circos plots
tags: [vb.net, CodeProject, circos, data visualization]
date: 2016-06-15
---

![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/circos-cancer-cell.png)

Circos is a software package for visualizing data and information. It visualizes data in a circular layout — this makes Circos ideal for exploring relationships between objects or positions. There are other reasons why a circular layout is advantageous, not the least being the fact that it is attractive.

> HOME page: http://circos.ca/

<!--more-->

The typically circos data directory consists of two parts of data file: \*.conf circos plots layout configuration file, and \*.txt plots data files in a directory which is named '**data**':

>  + circos.conf
>  + ideogram.conf
>  + ticks.conf
>  + ./data/...

Using circos program just very easy, using cd command enter the plots' config directory which the file circos.conf is located in the terminal, and then inputs the perl script location of circos, circos script will search for the circos.conf file automatically in your directory and then invoke drawing:

```bash
$ ~/circos/bin/circos
```

### Layout configs
The circos layout configs is consist with 3 config file in the majority:

#### circos.conf
##### a. Includes header

The files includes in the ``circos.conf`` configuration file using both relative path or absolute path. By default, almost all of the miscellaneous pre-defined config file is saved in the ``ect/`` directory, **likes color definitions, fonts, patterns, etc**. By using the relative path, circos program will search you included file in the directory of:

1. ./etc/
2. ../etc/
3. circos/bin/etc/
4. circos/etc/

All most all of the circos.conf layout config file required of these housekeeping includes:

```xml
<<include etc/colors_fonts_patterns.conf>>
<<include etc/housekeeping.conf>>
<<include ticks.conf>>
<<include ideogram.conf>>

<image>
  <<include etc/image.conf>>
</image>
```

##### b. genome information &amp; plots configs
##### c. plots definitions
The plots definitions in the circos is in the section of ``<plots>...</plots>``, and each circle plot is define in the sub-section of ``<plot>...</plot>``

```xml
  <plot>

  #   --> "LANS.SystemsBiology.AnalysisTools.DataVisualization.Interaction.Circos.Documents.Karyotype.Highlights.GradientMappings"

    type              = highlight
    file              = data/highlight_data_0.txt
    r1                = 3.95r
    r0                = 3.8r
    max               = 0.233698381696429
    min               = 0
    fill_color        = orange
    orientation       = in
    thickness         = 2
    stroke_thickness  = 0
    stroke_color      = grey
  </plot>
```

Plot elements have 4 required property:

|Property|Description|Example|
|--------|-----------|-------|
|type|The circle plot type|``type = highlight``|
|file|The data file for this circle plot|``file = data/highlight_data_0.txt``|
|r1, r0|The outer and inner radius of the current circle, and this radius value is relative to the define of **ideogram** its radius value. <br />In the most of situation, radius value must end with a lower case letter ``r``.|``r1 = 3.95r``|

So that we can define a basically abstract class for the circos plot in VisualBasic:

```vbnet
Public MustInherit Class Plot

   Public MustOverride ReadOnly Property Type As String
   Public Property File As String

   ''' <summary>
   ''' 圈外径(单位 r，请使用格式"<double>r")
   ''' </summary>
   ''' <value></value>
   ''' <returns></returns>
   ''' <remarks></remarks>
   Public Property r1 As String = "0.75r"

   ''' <summary>
   ''' 圈内径(单位 r，请使用格式"<double>r")
   ''' </summary>
   ''' <value></value>
   ''' <returns></returns>
   ''' <remarks></remarks>
   Public Property r0 As String = "0.6r"
   ...
End Class
```

#### ideogram.conf
#### ticks.conf

![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/manual/image-02.png)

### Plot types
### Data types