---
title: Simple SVG
tags: [vb.net, note, svg, document]
date: 2016-06-11
---

Recently I works on a [**network visualization solution**](http://www.codeproject.com/Articles/1104741/NET-Canvas-for-Network-visualization) in vb.net language, and want to export the image of my network in svg document format as the high quality publication material. And as the svg image is a kind of XML file, so that by using xml serialization technology, this makes the svg image generates easily.

![](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/SVG_TestOutput.png)
Example svg output on github: https://github.com/xieguigang/VisualBasic_AppFramework/blob/master/Datavisualization/Datavisualization.Network/SVG_TestOutput.svg

Wikipedia: https://en.wikipedia.org/wiki/Scalable_Vector_Graphics

> Scalable Vector Graphics (SVG) is an XML-based vector image format for two-dimensional graphics with support for interactivity and animation. The SVG specification is an open standard developed by the World Wide Web Consortium (W3C) since 1999.

> SVG images and their behaviors are defined in XML text files. This means that they can be searched, indexed, scripted, and compressed. As XML files, SVG images can be created and edited with any text editor, but are more often created with drawing software.

> All major modern web browsers—including Mozilla Firefox, Internet Explorer, Google Chrome, Opera, Safari, and Microsoft Edge—have at least some degree of SVG rendering support.

Here is a tips of how to generate the svg image in a very simple way.

<!--more-->

As the svg image is a XML document file, so that we can create a xml class object to modelling the SVG image, and then just using xml serialization to creates the SVG images as the high quantity published image for your document.

Majority, the svg just behavior likes the HTML, it can be consists of two major section:

1. Vector Diagram parts to defines the image
2. CSS styles to tweaks on the diagram styles

## Part1: Vector diagram object
As you can treating the svg document as html file, and we can tweaks on the diagram by using CSS, so that we can definite a basically SVG node element in the SVG Xml document:

```vbnet
    ''' <summary>
    ''' The basically SVG XML document node, it can be tweaks on the style by using CSS
    ''' </summary>
    Public MustInherit Class node

        ''' <summary>
        ''' CSS style definition
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property style As String
        ''' <summary>
        ''' node class id, just like the id in HTML, you can also using this attribute to tweaks on the style by CSS.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property [class] As String

        Public Overrides Function ToString() As String
            Return MyClass.GetJson
        End Function
    End Class
```

In the above code, the **style** property can overrides the global css style or complement the style definition for the specific node object. The **class** property defines the class id of the node element in the XML or HTML, which is can be applied the global css style through this class id property.
In my network canvas project, i used circle to visualize node and line object to visualize the edges, and here is the object definition:
```vbnet
Public Class circle : Inherits node
    <XmlAttribute> Public Property cy As Double
    <XmlAttribute> Public Property cx As Double
    <XmlAttribute> Public Property r As Double
    Public Property title As title
End Class

Public Class line : Inherits node
    <XmlAttribute> Public Property y2 As Double
    <XmlAttribute> Public Property x2 As Double
    <XmlAttribute> Public Property y1 As Double
    <XmlAttribute> Public Property x1 As Double
End Class
```

Almost all of the diagram object includes the circle and line object that used in the d3js svg is defined in the namespace: **Microsoft.VisualBasic.Imaging.SVG**, and you can found the entire source code at here: **[Microsoft.VisualBasic.Imaging/SVG/Xml.vb](https://github.com/xieguigang/VisualBasic_AppFramework/blob/master/Datavisualization/Microsoft.VisualBasic.Imaging/SVG/Xml.vb)**.

By define these graphics diagram object for the xml serialization, that this makes the program generates the svg image by simply using two Linq expression, and here is the code of generates the svg image for my network visualize canvas object:

```vbnet
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.Imaging.SVG.CSS
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MarkupLanguage.HTML
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts.Interfaces

''' <summary>
'''
''' </summary>
''' <param name="graph"></param>
''' <param name="style">Default value is <see cref="DefaultStyle"/></param>
''' <param name="size">The export canvas size</param>
''' <returns></returns>
<Extension>
Public Function ToSVG(graph As NetworkGraph, size As Size, Optional style As CSS.DirectedForceGraph = Nothing) As SVGXml
    Dim rect As New Rectangle(New Point, size)
    Dim nodes As SVG.circle() =
        LinqAPI.Exec(Of SVG.circle) <= From n As Graph.Node
                                       In graph.nodes
                                       Let pos As Point = Renderer.GraphToScreen(TryCast(n.Data.initialPostion, FDGVector2), rect)
                                       Let c As Color = If(
                                           TypeOf n.Data.Color Is SolidBrush,
                                           DirectCast(n.Data.Color, SolidBrush).Color,
                                           Color.Black)
                                       Let r As Single = n.__getRadius
                                       Let pt = New Point(CInt(pos.X - r / 2), CInt(pos.Y - r / 2))
                                       Select New circle With {
                                           .class = "node",
                                           .cx = pt.X,
                                           .cy = pt.Y,
                                           .r = r,
                                           .style = $"fill: rgb({c.R}, {c.G}, {c.B});"
                                       }
    Dim links As line() =
        LinqAPI.Exec(Of line) <= From edge As Edge
                                 In graph.edges
                                 Let source As Graph.Node = edge.Source
                                 Let target As Graph.Node = edge.Target
                                 Let pts As Point = Renderer.GraphToScreen(TryCast(source.Data.initialPostion, FDGVector2), rect)
                                 Let ptt As Point = Renderer.GraphToScreen(TryCast(target.Data.initialPostion, FDGVector2), rect)
                                 Let rs As Single = source.__getRadius / 2,
                                     rt As Single = target.__getRadius / 2
                                 Select New line With {
                                     .class = "link",
                                     .x1 = pts.X - rs,
                                     .x2 = ptt.X - rt,
                                     .y1 = pts.Y - rs,
                                     .y2 = ptt.Y - rt
                                 }
    Dim svg As New SVGXml With {
        .defs = New CSSStyles With {
            .styles = {
                New XmlMeta.CSS With {
                    .style = If(style Is Nothing, DefaultStyle(), style).ToString
                }
            }
        },
        .width = size.Width & "px",
        .height = size.Height & "px",
        .lines = links,
        .circles = nodes,
        .fill = "#dbf3ff"
    }

    Return svg
End Function
```

## Part2: CSS style
The svg CSS can be define using HTML css object in VisualBasic:

```vbnet
Public Class CSSStyles
    <XmlElement("style")> Public Property styles As XmlMeta.CSS()
End Class
```

And the **XmlMeta.CSS** object is a simple object define the css styles data in namespace []():
```vbnet
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization

Namespace HTML.XmlMeta

    Public Class CSS

        <XmlAttribute> Public Property type As String
            Get
                Return "text/css"
            End Get
            Set(value As String)
                ' ReadOnly, Do Nothing
            End Set
        End Property

        <XmlText> Public Property style As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
```
Define the CSS style is based on your specific application situation, and here is the CSS generated code for the network canvas objects in my work: nodes and edges:

```vbnet
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MarkupLanguage.HTML

Namespace SVG.CSS

    ''' <summary>
    ''' Style generator for the value of <see cref="XmlMeta.CSS.style"/>
    ''' </summary>
    Public Class DirectedForceGraph
        Public Property node As CssValue
        Public Property link As CssValue

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            Call sb.AppendLine(".node {")
            Call sb.AppendLine(node.ToString)
            Call sb.AppendLine("}")
            Call sb.AppendLine(".link {")
            Call sb.AppendLine(link.ToString)
            Call sb.AppendLine("}")

            Return sb.ToString
        End Function
    End Class

    Public Class CssValue : Inherits ClassObject
        <DataFrameColumn> Public Property stroke As String
        <DataFrameColumn("stroke-width")> Public Property strokeWidth As String
        <DataFrameColumn("stroke-opacity")> Public Property strokeOpacity As String
        <DataFrameColumn> Public Property opacity As String

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            For Each prop In DataFrameColumnAttribute.LoadMapping(Of CssValue).Values
                Dim value As Object = prop.GetValue(Me)
                If Not value Is Nothing Then
                    Call sb.AppendLine("    " & $"{prop.Identity}: {Scripting.ToString(value)};")
                End If
            Next

            Return sb.ToString
        End Function
    End Class
End Namespace
```

which it defines the HTML element styles, and this HTML CSS definition is also works in SVG. The source show above generates such CSS definition for rendering the elements which it has a class id named **"node" or "link"**:
```php
    Public Function DefaultStyle() As CSS.DirectedForceGraph
        Return New DirectedForceGraph With {
            .link = New CssValue With {
                .stroke = "#CCC",
                .strokeOpacity = "0.85",
                .strokeWidth = "6"
            },
            .node = New CssValue With {
                .strokeWidth = "0.5px",
                .strokeOpacity = "0.8",
                .stroke = "#FFF",
                .opacity = "0.85"
            }
        }
    End Function
```
```html
<defs>
    <style type="text/css">
.node {
    opacity: 0.85;
    stroke: #FFF;
    strokeOpacity: 0.8;
    strokeWidth: 0.5px;
}
.link {
    stroke: #CCC;
    strokeOpacity: 0.85;
    strokeWidth: 6;
}
</style>
</defs>

...
<circle style="fill: rgb(176, 196, 222);" class="node" cy="356" cx="1118" r="10" />
<line class="link" y2="967" x2="1014" y1="997" x1="1047" />
```


## Save the SVG document

So that we can define a basically svg document object consist with these two part of data: which it has a css style property named defs and inherits from graphics object, which its element name in svg is g and contains the property definition of graphics diagram objects, such as circles, lines, rectangle, texts, etc:
```vbnet
''' <summary>
''' The svg vector graphics in Xml document format.
''' </summary>
<XmlType("svg")> Public Class SVGXml : Inherits g
    Implements ISaveHandle

#Region "xml root property"
    <XmlAttribute> Public Property width As String
    <XmlAttribute> Public Property height As String
    <XmlAttribute> Public Property version As String
#End Region

    ''' <summary>
    ''' Style definition of the xml node in this svg document.
    ''' you can define the style by using css and set the class
    ''' attribute for the specific node to controls the
    ''' visualize style.
    ''' </summary>
    ''' <returns></returns>
    Public Property defs As CSSStyles
......
End Class

Public Class g : Inherits node
    .......
    <XmlElement("line")> Public Property lines As line()
    <XmlElement("circle")> Public Property circles As circle()
    <XmlAttribute> Public Property fill As String
End Class
```
Due to the reason of xml serialization in the vb.net is in the utf-16 encoding and using the default xml namespace, so that we should manually modify the xml encodings to utf-8, modified the xml namespace to svg namespace: http://www.w3.org/2000/svg, and add a xlink namespace from: http://www.w3.org/1999/xlink.

Here is the code example of save the SVG xml document:

```vbnet
''' <summary>
''' Save this svg document object into the file system.
''' </summary>
''' <param name="Path"></param>
''' <param name="encoding"></param>
''' <returns></returns>
Private Function SaveAsXml(Optional Path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
    Dim xml As New XmlDoc(Me.GetXml)
    xml.encoding = XmlEncodings.UTF8
    xml.standalone = False
    xml.xmlns.Set("xlink", "http://www.w3.org/1999/xlink")
    xml.xmlns.xmlns = "http://www.w3.org/2000/svg"

    Return xml.SaveTo(Path, encoding)
End Function

''' <summary>
''' Save this svg document object into the file system.
''' </summary>
''' <param name="Path"></param>
''' <param name="encoding"></param>
''' <returns></returns>
Public Function SaveAsXml(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
    Return SaveAsXml(Path, encoding.GetEncodings)
End Function
```

After the xml file save operation, you can click on your generated svg image and displayed in your browser:
![](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/2016-06-11.png)