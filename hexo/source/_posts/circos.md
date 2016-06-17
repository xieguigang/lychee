---
title: Circos plots
tags: [vb.net, CodeProject, circos, data visualization, GCModeller]
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

Using circos program just very easy, using ``cd`` command enter the plots' config directory which the file **circos.conf** is located in the terminal, and then inputs the perl script location of circos, circos script will search for the **circos.conf** file automatically in your directory and then invoke drawing:

```bash
$ ~/circos/bin/circos
```

> ![](https://raw.githubusercontent.com/SMRUCC/GCModeller.Circos/master/manual/AQ%20Magazine%2C%20April%202011_Simon%20Fraser%20University.png)
> **AQ Magazine, April 2011 (Simon Fraser University)**

### Layout configs
The circos layout configs is consist with 3 config file in the majority:

+ **circos.conf**
	This is the main configuration of your circos plot, and the required graphics includes file, genome brief description, plots' details information was recorded at here.
+ **ideogram.conf**
	ideogram visualizes linear layout of CHROMOSOMES, plots of the chromosomes brief information, and layout overview controls of the circos plot.
+ **ticks.conf**
	The display style of the periodic loci labels for the genome chromosomes sequence was define at here.

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
##### c. plots definitions(Tracks)
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

### Track data types
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

And then we can define the data point of scatter, stack value, and text labels.

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

### Plot types
