Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Layout

Namespace Controls

    ''' <summary>
    ''' Maps the tag name of a html element to the renderer that paints it.
    ''' </summary>
    Public NotInheritable Class ControlRendererFactory

        Private ReadOnly renderers As New Dictionary(Of String, IControlRenderer)() From {
            {"a", link}
        }
        Private ReadOfallback As IControlRenderer = New DivRenderer()

        ' the input elements: a single tag name covers a text box, a password
        ' box, a radio button and a check box, so they are dispatched by the
        ' type attribute of the element instead of by its tag name
        Friend ReadOnly textInput As New TextInputRenderer()
        Private ReadOnly radio As New RadioRenderer()
        Private ReadOnly checkbox As New CheckboxRenderer()
        Private ReadOnly image As New ImageRenderer()
        Private ReadOnly link As New LinkRenderer()

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
        ''' The renderer that paints the text input controls of the ui.
        ''' </summary>
        ''' <returns></returns>
        ''' <summary>
        ''' Gets the renderer of the given control.
        ''' </summary>
        ''' <param name="box"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' The state of the control wins over its tag name here: the
        ''' ``input`` tag maps to four different renderers, and they are told
        ''' apart by the ``type`` attribute of the element.
        ''' </remarks>
        Public Function GetRenderer(box As UiBox) As IControlRenderer
            If box Is Nothing Then
                Return Fallback
            End If

            If box.IsImage Then
                Return image
            End If
            If box.IsCheckable Then
                Return If(box.InputType = "radio", DirectCast(radio, IControlRenderer), checkbox)
            End If
            If box.IsTextInput Then
                Return textInput
            End If

            Return GetRenderer(box.Tag)
        End Function

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
