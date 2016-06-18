---
title: Circos plots
tags: [vb.net, CodeProject, circos, data visualization, GCModeller, perl]
date: 2016-06-15
---

![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/circos-cancer-cell.png)

Circos is a software package for visualizing data and information which is written in **Perl** language. It visualizes data in a circular layout — this makes Circos ideal for exploring relationships between objects or positions. There are other reasons why a circular layout is advantageous, not the least being the fact that it is attractive. From the previous works on the bacterial genome annotation by using GCModeller, required of hybried programming with Perl language with vb.net for the annotation result visualization, by using this Circos software Perl script, I created this Circos API for .NET language, and introduce this .NET language API library.

> HOME page: http://circos.ca/
> GCModeller API: https://github.com/SMRUCC/GCModeller.Circos

<!--more-->

The typically circos data directory consists of two parts of data file: **\*.conf** circos plots layout configuration file, and **\*.txt** plots data files in a directory which is named '**data**':

>  + circos.conf
>  + ideogram.conf
>  + ticks.conf
>  + ./data/...

![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/manual/workflow.png)

Using circos program just very easy, using ``cd`` command enter the plots' config directory which the file **circos.conf** is located in the terminal, and then inputs the perl script location of circos, circos script will search for the **circos.conf** file automatically in your directory and then invoke drawing:

```bash
$ ~/circos/bin/circos
```

> ![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/manual/AQ%20Magazine%2C%20April%202011_Simon%20Fraser%20University.png)
> *AQ Magazine, April 2011 (Simon Fraser University)*

## Layout configs
The circos layout configs is consist with 3 config file in the majority:

+ **circos.conf**
	This is the main configuration of your circos plot, and the required graphics includes file, genome brief description, plots' details information was recorded at here.
+ **ideogram.conf**
	ideogram visualizes linear layout of CHROMOSOMES, plots of the chromosomes brief information, and layout overview controls of the circos plot.
+ **ticks.conf**
	The display style of the periodic loci labels for the genome chromosomes sequence was define at here.

### Basic Document Abstract

First of all, we define the very basically class object at here for all of the circos documents at here:

```vbnet
''' <summary>
''' This object can be convert to text document by using method <see cref="GenerateDocument"/>
''' </summary>
Public Interface ICircosDocNode
    Function GenerateDocument(IndentLevel As Integer) As String
End Interface

''' <summary>
''' This object can be save as a text doc for the circos plot
''' </summary>
Public Interface ICircosDocument : Inherits ICircosDocNode, ISaveHandle
End Interface
```

In this interface definition, we just created the basically text document model for the files in the circos program: ``ICircosDocNode`` have an abstract function to generates the document text and ``ICircosDocument`` object by multiple inheritance, it have both method of document generation inherits from ``ICircosDocNode`` and abstract save method that inherits from text file save model ``ISaveHandle``

So that based on the ``ICircosDocument`` abstract type, we can define the model of ``*.conf`` file at here:

```vbnet
    ''' <summary>
    ''' Abstract of the circos config files.
    ''' </summary>
    Public MustInherit Class CircosConfig : Inherits ITextFile
        Implements ICircosDocument

        ''' <summary>
        ''' 文档的对其他的配置文件的引用列表
        ''' </summary>
        ''' <returns></returns>
        Public Property Includes As List(Of CircosConfig)

        ''' <summary>
        ''' This config file was included in ``circos.conf``.(主配置文件Circos.conf)
        ''' </summary>
        ''' <returns></returns>
        Public Property main As Circos

        Sub New(FileName As String, Circos As Circos)
            MyBase.FilePath = FileName
            Me.main = Circos
        End Sub

        Protected Function GenerateIncludes() As String
            If Includes.IsNullOrEmpty Then
                Return ""
            End If

            Dim sb As New StringBuilder(1024)

            For Each includeFile As CircosConfig In Includes
                Call __appendLine(sb, includeFile)
                Call includeFile.Save(Encoding:=Encoding.ASCII)
            Next

            Return sb.ToString
        End Function

        Private Shared Sub __appendLine(ByRef sb As StringBuilder, include As CircosConfig)
            Dim refPath As String = Tools.TrimPath(include)

            If TypeOf include Is CircosDistributed Then
                Dim name As String = DirectCast(include, CircosDistributed).Section
                If Not String.IsNullOrEmpty(name) Then
                    Call sb.AppendLine($"<{name}>")
                    Call sb.AppendLine($"   <<include {refPath}>>")
                    Call sb.AppendLine($"</{name}>")
                Else
                    Call sb.AppendLine($"<<include {refPath}>>")
                End If
            Else
                Call sb.AppendLine($"<<include {refPath}>>")
            End If
        End Sub

        Protected MustOverride Function GenerateDocument(IndentLevel As Integer) As String Implements ICircosDocument.GenerateDocument

        ''' <summary>
        ''' Auto detected that current is circos distribution or not, if true, then this file will not be saved.
        ''' </summary>
        ''' <param name="FilePath"></param>
        ''' <param name="Encoding"></param>
        ''' <returns></returns>
        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean Implements ICircosDocument.Save
            If TypeOf Me Is CircosDistributed Then
                Return True ' 系统自带的不需要进行保存了
            End If

            Dim doc As String = GenerateDocument(IndentLevel:=Scan0)
            Return doc.SaveTo(getPath(FilePath), If(Encoding Is Nothing, Encoding.ASCII, Encoding))
        End Function
    End Class
```

### **circos.conf**
#### a. Includes header

The files includes in the ``circos.conf`` configuration file using both relative path or absolute path. By default, almost all of the circos distribution miscellaneous pre-defined config file is saved in the ``ect/`` directory, **likes color definitions, fonts, patterns, etc**. By using the relative path, circos program will search you included file in the directory of:

1. ./etc/
2. ../etc/
3. circos/bin/etc/
4. circos/etc/

As always, centralize all your inputs as much as possible.

All most all of the ``circos.conf`` layout config file required of these housekeeping includes. These should be present in every Circos configuration file and overridden as required. To see the content of these files, you can look in ``etc/`` in the Circos distribution.

It's best to include these files using relative paths. This way, the files if not found under your current directory will be drawn from the Circos distribution.

```html
<<include etc/colors_fonts_patterns.conf>>
<<include etc/housekeeping.conf>>
<<include ticks.conf>>
<<include ideogram.conf>>

<image>
  <<include etc/image.conf>>
</image>
```

Debugging, I/O an dother system parameters Included from Circos distribution.

```
<<include etc/housekeeping.conf>> 
```

RGB/HSV color definitions, color lists, location of fonts, fill patterns. Included from Circos distribution. In older versions of Circos, colors, fonts and patterns were included individually. Now, this is done from a central file.

```
<<include etc/colors_fonts_patterns.conf>> 
```

And here is the definition of the reference to these circos distribution includes:

```vbnet
''' <summary>
''' The circos distributed includes files.
''' (这个对象仅仅是为了引用Cricos系统内的预置的配置文件的设立的，故而<see cref="CircosDistributed.GenerateDocument">
''' </see>方法和<see cref="CircosDistributed.Save"></see>方法可以不会被实现)
''' </summary>
''' <remarks></remarks>
Public Class CircosDistributed : Inherits CircosConfig

    Public ReadOnly Property Section As String

    ''' <summary>
    ''' 由于这些是系统的预置的数据，是不能够再修改了的，所以这里由于没有数据配置项，直接忽略掉了Circos配置数据
    ''' </summary>
    ''' <param name="path"></param>
    ''' <remarks></remarks>
    Protected Sub New(path As String)
        Call MyBase.New(path, Circos:=Nothing)
    End Sub

    Protected Sub New(path As String, name As String)
        Me.New(path)
        Me.Section = name
    End Sub

    Protected Overrides Function GenerateDocument(IndentLevel As Integer) As String
        Return ""
    End Function

#Region "The Circos Distribution includes"
    ''' <summary>
    ''' Debugging, I/O an dother system parameters included from Circos distribution.
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property HouseKeeping As CircosDistributed =
        New CircosDistributed("etc/housekeeping.conf")

    Public Shared ReadOnly Property ColorBrain As CircosDistributed =
        New CircosDistributed("color.brain.conf", "colors")
    Public Shared ReadOnly Property Image As CircosDistributed =
            New CircosDistributed("etc/image.conf", "image")

    ''' <summary>
    ''' RGB/HSV color definitions, color lists, location of fonts, fill
    ''' patterns. Included from Circos distribution.
    '''
    ''' In older versions Of Circos, colors, fonts And patterns were
    ''' included individually. Now, this Is done from a central file. Make
    ''' sure that you're not importing these values twice by having
    '''
    ''' *** Do Not Do THIS ***
    ''' &lt;colors>
    '''     &lt;&lt;include etc/colors.conf>>
    ''' &lt;colors>
    ''' **********************
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property ColorFontsPatterns As CircosDistributed =
        New CircosDistributed("etc/colors_fonts_patterns.conf")
#End Region
End Class
```

#### b. genome information &amp; plots configs

Here is some property that used on the genome layout controls in the ``circos.conf`` file:

```vbnet
''' <summary>
''' circos.conf
'''                                     ____ _
'''                                    / ___(_)_ __ ___ ___  ___
'''                                   | |   | | '__/ __/ _ \/ __|
'''                                   | |___| | | | (_| (_) \__ \
'''                                    \____|_|_|  \___\___/|___/
'''
'''                                                round Is good
'''
''' circos - generate circularly composited information graphics
''' 
''' (Circo基因组绘图程序的主配置文件)
''' </summary>
''' <remarks>
''' ![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/manual/workflow.png)
''' 
''' Typically a central configuration file which defines data track information (circos.conf) imports other 
''' configuration files that store parameters that change less frequently 
''' (tick marks, ideogram size, grid, etc). 
''' 
''' Data for each data track Is stored in a file And the same file can be used for multiple tracks.
''' 
''' + PNG image output Is ideal For immediate viewing, web-based reporting Or presentation. 
''' + SVG output Is most suitable For generating very high resolution line art For publication And For customizing aspects Of the figure.
''' </remarks>
Public Class Circos : Inherits CircosConfig
    Implements ICircosDocument

    ''' <summary>
    ''' The basically genome structure plots: Chromosome name, size and color definition.(基本的数据文件)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Circos> Public Property karyotype As String = "data/genes.txt"
    <Circos> Public Property genome As String = null
    <Circos> Public Property use_rules As String = yes

    ''' <summary>
    ''' The chromosomes_unit value is used as a unit (suffix "u") to shorten
    ''' values In other parts Of the configuration file. Some parameters,
    ''' such As ideogram And tick spacing, accept "u" suffixes, so instead Of
    ''' </summary>
    ''' <returns></returns>
    <Circos> Public Property chromosomes_units As String = "5000"
    ''' <summary>
    ''' The default behaviour is to display all chromosomes defined in the
    ''' karyotype file. In this example, I Select only a subset.
    '''
    ''' The 'chromosomes' parameter has several uses, and selecting which
    ''' chromosomes To show Is one Of them. You can list them
    ''' Or provide a regular expression that selects them based On a successful match
    ''' The ``$`` anchor Is necessary, otherwise chromosomes Like *hs10, hs11 And
    ''' hs20* are also matched.
    ''' </summary>
    ''' <returns></returns>
    <Circos> Public Property chromosomes_display_default As String = yes
    <Circos> Public Property chromosomes As String = null
    ''' <summary>
    ''' By default, the scale progression is clockwise. You can set the
    ''' Global angle progression Using 'angle_orientation' in the ``&lt;image>``
    ''' block (clockwise Or counterclockwise). To reverse it For one Or
    ''' several ideograms, use 'chromosomes-reverse'
    ''' </summary>
    ''' <returns></returns>
    <Circos> Public Property chromosomes_reverse As String = null
    ''' <summary>
    ''' The default radial position for all ideograms is set by 'radius' in
    ''' the ``&lt;ideogram>`` block (see ideogram.conf). To change the value For
    ''' specific ideograms, use chromosomes_radius.
    ''' </summary>
    ''' <returns></returns>
    <Circos> Public Property chromosomes_radius As String = null
    ''' <summary>
    ''' The size of the ideogram on the figure can be adjusted using an
    ''' absolute Or relative magnification. Absolute scaling,
    ''' shrinks Or expands the ideogram by a fixed factor. When the "r"
    ''' suffix Is used, the magnification becomes relative To the
    ''' circumference Of the figure. Thus, 
    ''' makes ``hs1`` To occupy 50% Of the figure. To uniformly distribute
    ''' several ideogram within a fraction Of the figure, use a regular
    ''' expression that selects the ideograms And the "rn" suffix (relative
    ''' normalized).
    ''' </summary>
    ''' <returns></returns>
    <Circos> Public Property chromosomes_scale As String = null
    ''' <summary>
    ''' The color of each ideogram is taken from the karyotype file. To
    ''' change it, use 'chromosomes_color'.
    ''' </summary>
    ''' <returns></returns>
    <Circos> Public Property chromosomes_color As String = null
    <Circos> Public Property chromosomes_order As String = null
    <Circos> Public Property chromosomes_breaks As String = null
		
    ''' <summary>
    ''' 基因组的骨架信息
    ''' </summary>
    ''' <returns></returns>
    Public Property SkeletonKaryotype As SkeletonInfo
```

#### c. plots definitions(Tracks)
The plots definitions in the circos is in the section of ``<plots>...</plots>``, and each circle plot is define in the sub-section of ``<plot>...</plot>``, we call this circle plot as tracks. **Tracks are confined to a radial range and may overlap.**

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
|r1, r0|The outer and inner radius of the current circle, and this radius value is relative to the define of **ideogram** its radius value. <br />In the most of situation, radius value is recommended end with a lower case letter ``r`` as suffix.|``r1 = 3.95r``|

![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/manual/track_radius.png)

The track is confined within ``r0``-``r1`` radius limits. When using the relative ``r`` suffix, the values are relative To the position Of the ``ideogram``.

So that we can define a basically abstract class for the circos plot in VisualBasic:

```vbnet
Public MustInherit Class Plot

   Public MustOverride ReadOnly Property type As String
   Public Property file As String

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

### **ideogram.conf**
In Circos images, ideograms refer to graphical representations of chromosomes (or regions of chromosomes). By default ideograms are arranged in a circle, but radial positions of individual ideograms can be changed. The legibility of the figure depends on an ideogram organization that suits the data type and density.

#### karyotype &amp; band
The ideogram using karyotype file to define the genome skeleton information, which defines the name, size and color of chromosomes. 

A simple karyotype with 5 chromosomes:

```
# chr1 5Mb
# chr2 10Mb
# chr3 20Mb
# chr4 50Mb
# chr5 100Mb

chr - chr1 1 0 5000000 spectral-5-div-1
chr - chr2 2 0 10000000 spectral-5-div-2
chr - chr3 3 0 20000000 spectral-5-div-3
chr - chr4 4 0 50000000 spectral-5-div-4
chr - chr5 5 0 100000000 spectral-5-div-5
```

The format Of this file Is

```
chr - CHRNAME CHRLABEL START End COLOR
```
	
In data files, chromosomes are referred To by CHRNAME. On the image, they are labeled by CHRLABEL. Colors are taken from the spectral Brewer palette. To learn about Brewer palettes, see: [www.colorbrewer.org](http://www.colorbrewer.org)

```vbnet
    ''' <summary>
    ''' The ideogram using karyotype file to define the genome skeleton information, which defines the name, size and color of chromosomes. 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    Public Class Karyotype

        Public Property chrName As String
        Public Property chrLabel As String
        Public Property start As Integer
        Public Property [end] As Integer
        Public Property color As String

        Public Overrides Function ToString() As String
            Return $"chr - {chrName} {chrLabel} {start} {[end]} {color}"
        End Function
    End Class
```

### **ticks.conf**

The tick label is derived by multiplying the tick position by ``multiplier`` and casting it in ``format``:

```c
sprintf(format,position*multiplier)
```

|Format Controls|Data types|
|---------------|----------|
|%d  |integer|
|%f  |float|
|%.1f|float with one decimal|
|%.2f|float with two decimals|

For other formats, you can see the document of perl: **http://perldoc.perl.org/functions/sprintf.html**

![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/manual/image-02.png)

## Track data types
**Data for tracks is loaded from a plain-text file. Each data point is stored on a separate line, except for links which use two lines per link. The definition of a data point within a track is based on the genomic range, which is a combination of chromosome and start/end position.** For example, this is a very basically tracks data in the circos:

```
# the basis for a data point is a range
chr12 1000 5000
```

In this tracks data example, there are two elements: First token ``chr12`` is the name of the chromosome which its identifer in your research is chr12, and then the followed two number value is that this track data on the position of this chromosome: from ranges of 1000bp to 5000bp with default forwards strand. 
So that we can define a data model for this track data in .NET:

```vbnet
''' <summary>
''' Data for tracks is loaded from a plain-text file. Each data point is stored on a 
''' separate line, except for links which use two lines per link.
''' 
''' The definition Of a data point within a track Is based On the genomic range, 
''' which Is a combination Of chromosome And start/End position.
''' </summary>
Public MustInherit Class TrackData

    ''' <summary>
    ''' Chromosomes name
    ''' </summary>
    Public Property chr As String
    Public Property start As Integer
    Public Property [end] As Integer
    Public Property formatting As Formatting

    ''' <summary>
    ''' Using <see cref="ToString()"/> method for creates tracks data document.
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function ToString() As String
        Dim s As String = __trackData()
        Dim format As String = formatting.ToString

        If Not String.IsNullOrEmpty(format) Then
            s &= " " & format
        End If

        Return s
    End Function

    Protected MustOverride Function __trackData() As String

End Class
```

![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/manual/track_example.png)

And at here we can using ``formatting`` property to provides some additional data track annotation:

```vbnet
''' <summary>
''' Annotated with formatting parameters that control how the point Is drawn. 
''' </summary>
Public Structure Formatting

    ''' <summary>
    ''' Only works in scatter, example is ``10p``
    ''' </summary>
    Dim glyph_size As String
    ''' <summary>
    ''' Only works in scatter, example is ``circle``
    ''' </summary>
    Dim glyph As String
    ''' <summary>
    ''' Works on histogram
    ''' </summary>
    Dim fill_color As String
    ''' <summary>
    ''' Works on any <see cref="Trackdata"/> data type.
    ''' </summary>
    Dim URL As String

    Public Overrides Function ToString() As String
        Dim s As New StringBuilder

        Call __attach(s, NameOf(glyph), glyph)
        Call __attach(s, NameOf(glyph_size), glyph_size)
        Call __attach(s, NameOf(fill_color), fill_color)
        Call __attach(s, "url", URL)

        Return s.ToString
    End Function

    Private Shared Sub __attach(ByRef s As StringBuilder, name As String, value As String)
        If s.Length = 0 Then
            Call s.Append($"{name}={value}")
        Else
            Call s.Append($",{name}={value}")
        End If
    End Sub
End Structure
```

And then we can define the data point of scatter, stack value, and text labels, separatly, from inherits of the abstract base type ``TrackData`` that we define above:

```vbnet
''' <summary>
''' Tracks such As scatter plot, line plot, histogram Or heat map, associate a value With Each range. The input To this kind Of track would be
''' </summary>
Public Class ValueTrackData : Inherits TrackData

    Public Property value As Double

    Protected Overrides Function __trackData() As String
        Return $"{chr} {start} {[end]} {value}"
    End Function
End Class

''' <summary>
''' The exception Is a stacked histogram, which associates a list Of values With a range.
''' </summary>
Public Class StackedTrackData : Inherits TrackData

    Public Property values As Double()

    Protected Overrides Function __trackData() As String
        Dim values As String = Me.values.Select(Function(d) d.ToString).JoinBy(",")
        Return $"{chr} {start} {[end]} {values}"
    End Function
End Class

''' <summary>
''' The value For a text track Is interpreted As a text label (other tracks require that this field be a floating point number).
''' </summary>
Public Class TextTrackData : Inherits TrackData

    Public Property text As String

    Protected Overrides Function __trackData() As String
        Return $"{chr} {start} {[end]} {text}"
    End Function
End Class
```

And then we can creates a object for generates the tracks data plant-text document:

```vbnet
''' <summary>
''' Tracks data document generator
''' </summary>
''' <typeparam name="T"></typeparam>
Public Class data(Of T As TrackData) : Inherits List(Of T)

    Sub New()
        Call MyBase.New(0)
    End Sub

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Function GetDocumentText() As String
        Dim sb As New StringBuilder

        For Each x As T In Me
            If Not String.IsNullOrEmpty(x.comment) Then
                Call sb.AppendLine("# " & x.comment)
            End If
            Call sb.AppendLine(x.ToString)
        Next

        Return sb.ToString
    End Function
End Class
```

## Track plot types
There are 5 track plot type that we usually used in the genome visualization: **histogram, scatter, heatmap, links, and text.**

### histogram

Histograms are a type of track that displays 2D data, which associates a value with a genomic position. Line plots, scatter plots and heat maps are examples of other 2D tracks.

The data format for 2D data is:

```
 #chr start end value [options]
 ...
 hs3 196000000 197999999 71.0000
 hs3 198000000 199999999 57.0000
 hs4 0 1999999 28.0000
 hs4 2000000 3999999 40.0000
 hs4 4000000 5999999 59.0000
 ...
```

Each histogram is defined in a ``<plot>`` block within an enclosing ``<plots>`` block.

![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/manual/tracks_histogram.png)
### scatter



![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/manual/tracks_scatter.png)
### heatmap

Heat maps are used for data types which associate a value with a genomic position, or region. As such, this track uses the same data format as histograms.

The track linearly maps a range of values ``[min,max]`` onto a list of colors ``c[n], i=0..N.``

```
 f = (value - min) / ( max - min )
 n = N * f
```

Colors are defined by a combination of lists or CSV. Color lists exist for all Brewer palletes (see etc/colors.brewer.lists.conf) as well as for N-step hue (hue-sN, e.g. hue-s5 = hue000,hue005,hue010,...) and N-color hue (hue-sN, e.g. hue-3 = hue000,hue120,hue140). Here is a color example:

```
color = hs1_a5,hs1_a4,hs1_a3,hs1_a2,hs1_a1,hs1
```

If scale_log_base is used, the mapping is not linear, but a power law:

```
 n = N * f**(1/scale_log_base)
```

+ When ``scale_log_base`` > 1 the dynamic range for values close to ``min`` is expanded.
+ When ``scale_log_base`` < 1 the dynamic range for values close to ``max`` is expanded.

```
scale_log_base = 5
```

![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/manual/tracks_heatmap.png)
### links
![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/manual/tracks_links.png)
### text

Like with other tracks, text is limited to a radial range by setting ``r0`` and ``r1``.

Individual labels can be repositioned automatically with in a position window to fit more labels, without overlap. This is an advanced feature.

![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/manual/tracks_text.png)

### Extension track types
