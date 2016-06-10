---
title: Simple SVG
tags: [vb.net, note, svg, document]
date: 2016-06-10
---

![](https://raw.githubusercontent.com/xieguigang/VisualBasic_AppFramework/master/Datavisualization/Datavisualization.Network/SVG_TestOutput.svg)

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

## Part2: CSS style

```vbnet
    Public Class CSSStyles
        <XmlElement("style")> Public Property styles As XmlMeta.CSS()
    End Class
```

## Save the SVG document

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