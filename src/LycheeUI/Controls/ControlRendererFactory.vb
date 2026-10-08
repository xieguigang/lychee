Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Layout

Namespace Controls

    ''' <summary>
    ''' Maps the tag name of a html element to the renderer that paints it.
    ''' </summary>
    Public NotInheritable Class ControlRendererFactory

        Private ReadOnly renderers As New Dictionary(Of String, IControlRenderer)()
        Private ReadOfallback As IControlRenderer = New DivRenderer()

        ''' <summary>
        ''' The renderer that paints the elements without a registered renderer.
        ''' </summary>
        ''' <returns></returns>
        Public Property Fallback As IControlRenderer
            Get
                Return ReadOfallback
            End Get
            Set
                ReadOfallback = Value
            End Set
        End Property

        Sub New()
            ' the anonymous text boxes of the html layout engine are painted by
            ' the label renderer: they only draw their own text
            renderers("text") = New LabelRenderer()
            renderers("label") = New LabelRenderer()
            renderers("span") = New LabelRenderer()
            renderers("p") = New LabelRenderer()
            renderers("div") = New DivRenderer()
            renderers("button") = New ButtonRenderer()
            renderers("input") = New ButtonRenderer()
        End Sub

        ''' <summary>
        ''' Gets the renderer of the given tag name.
        ''' </summary>
        ''' <param name="tag"></param>
        ''' <returns></returns>
        Public Function GetRenderer(tag As String) As IControlRenderer
            If String.IsNullOrEmpty(tag) Then
                Return Fallback
            End If

            tag = tag.ToLower()

            If renderers.ContainsKey(tag) Then
                Return renderers(tag)
            End If

            Return Fallback
        End Function

        ''' <summary>
        ''' Registers (or replaces) the renderer of the given tag name.
        ''' </summary>
        ''' <param name="tag"></param>
        ''' <param name="renderer"></param>
        Public Sub Register(tag As String, renderer As IControlRenderer)
            renderers(tag.ToLower()) = renderer
        End Sub
    End Class
End Namespace
