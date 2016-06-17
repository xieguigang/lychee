---
title: Circos plots
tags: [vb.net, CodeProject, circos, data visualization, GCModeller, perl]
date: 2016-06-15
---

![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/circos-cancer-cell.png)

Circos is a software package for visualizing data and information. It visualizes data in a circular layout — this makes Circos ideal for exploring relationships between objects or positions. There are other reasons why a circular layout is advantageous, not the least being the fact that it is attractive.

> HOME page: http://circos.ca/
> GCModeller API: https://github.com/SMRUCC/GCModeller.Circos

<!--more-->

The typically circos data directory consists of two parts of data file: \*.conf circos plots layout configuration file, and \*.txt plots data files in a directory which is named '**data**':

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

### **circos.conf**
#### a. Includes header

The files includes in the ``circos.conf`` configuration file using both relative path or absolute path. By default, almost all of the miscellaneous pre-defined config file is saved in the ``ect/`` directory, **likes color definitions, fonts, patterns, etc**. By using the relative path, circos program will search you included file in the directory of:

1. ./etc/
2. ../etc/
3. circos/bin/etc/
4. circos/etc/

As always, centralize all your inputs as much as possible.

All most all of the circos.conf layout config file required of these housekeeping includes. These should be present in every Circos configuration file and overridden as required. To see the content of these files, you can look in ``etc/`` in the Circos distribution.

It's best to include these files using relative paths. This way, the files if not found under your current directory will be drawn from the Circos distribution.

```xml
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

RGB/HSV color definitions, color lists, location of fonts, fill patterns. Included from Circos distribution.

In older versions of Circos, colors, fonts and patterns were included individually. Now, this is done from a central file.

```
<<include etc/colors_fonts_patterns.conf>> 
```

#### b. genome information &amp; plots configs
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
There are 5 track plot type that we usually used in the genome visualization: histogram, scatter, heatmap, links, and text.

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
