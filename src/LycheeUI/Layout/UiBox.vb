Imports System.Drawing
Imports System.Text
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Html.Render.CSS
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports std = System.Math

Namespace Layout

    ''' <summary>
    ''' A lightweight view of a <see cref="CssBox"/> of the html layout engine:
    ''' it exposes the computed style values that are required by the directx
    ''' control renderers, and keeps the interactive state (hover and pressed)
    ''' that is not a part of the css box model.
    ''' </summary>
    ''' <remarks>
    ''' The layout of the box is recalculated on every frame by the html layout
    ''' engine, so every geometry property of this view is delegated to the
    ''' underlying <see cref="CssBox"/> instead of being cached.
    ''' </remarks>
    Public Class UiBox

        ''' <summary>
        ''' The underlying css box of the html layout engine.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Source As CssBox

        ''' <summary>
        ''' The tag name of the html element, e.g. ``button`` or ``div``.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Tag As String

        ''' <summary>
        ''' The document order of this box, it is used as the secondary sort key
        ''' of the paint order.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Order As Integer

        ''' <summary>
        ''' Is the mouse currently located inside of this box?
        ''' </summary>
        ''' <returns></returns>
        Public Property Hover As Boolean

        ''' <summary>
        ''' Is a mouse button currently pressed down on this box?
        ''' </summary>
        ''' <returns></returns>
        Public Property Pressed As Boolean

        ''' <summary>
        ''' The text of a text input control; it is the "True"/"False" literal of
        ''' the checked state for a checkbox and a radio button.
        ''' </summary>
        ''' <returns></returns>
        Public Property Value As String

        ''' <summary>
        ''' Is this checkbox or radio button selected?
        ''' </summary>
        ''' <returns></returns>
        Public Property Checked As Boolean

        ''' <summary>
        ''' Does this control hold the keyboard focus of the canvas?
        ''' </summary>
        ''' <returns></returns>
        Public Property Focused As Boolean

        ''' <summary>
        ''' The offset of the caret inside of <see cref="Value"/>.
        ''' </summary>
        ''' <returns></returns>
        Public Property Caret As Integer

        ''' <summary>
        ''' The offset where the selected range of the text starts.
        ''' </summary>
        ''' <returns></returns>
        Public Property SelectionStart As Integer

        ''' <summary>
        ''' The number of the characters that are selected, zero means that there
        ''' is no selection at all.
        ''' </summary>
        ''' <returns></returns>
        Public Property SelectionLength As Integer

        Sub New(source As CssBox, tag As String, order As Integer)
            Me.Source = source
            Me.Tag = tag
            Me.Order = order
        End Sub

        ''' <summary>
        ''' The ``type`` attribute of an input element: ``text``, ``password``,
        ''' ``radio`` or ``checkbox``.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property InputType As String
            Get
                Dim type As String = Source.GetAttribute("type")

                Return If(String.IsNullOrEmpty(type), "text", type.ToLower().Trim())
            End Get
        End Property

        ''' <summary>
        ''' The ``name`` attribute of an input element: the radio buttons that
        ''' share the same group name are mutually exclusive.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property GroupName As String
            Get
                Return Source.GetAttribute("name")
            End Get
        End Property

        ''' <summary>
        ''' The ``src`` attribute of an image element.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Src As String
            Get
                Return Source.GetAttribute("src")
            End Get
        End Property

        ''' <summary>
        ''' The hint text that is drawn inside of an empty text input control.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Placeholder As String
            Get
                Return Source.GetAttribute("placeholder")
            End Get
        End Property

        ''' <summary>
        ''' The label text of a checkbox or a radio button: it is taken from the
        ''' ``label`` attribute, or from the text of the element itself.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LabelText As String
            Get
                Dim text As String = Source.GetAttribute("label")

                Return If(String.IsNullOrEmpty(text), Text, text)
            End Get
        End Property

        ''' <summary>
        ''' Is this element a text input control?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsTextInput As Boolean
            Get
                If Tag <> "input" Then
                    Return False
                End If

                Return InputType = "text" OrElse InputType = "password"
            End Get
        End Property

        ''' <summary>
        ''' Is this element a checkbox or a radio button?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsCheckable As Boolean
            Get
                If Tag <> "input" Then
                    Return False
                End If

                Return InputType = "checkbox" OrElse InputType = "radio"
            End Get
        End Property

        ''' <summary>
        ''' Is this element an image?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsImage As Boolean
            Get
                Return Tag = "img"
            End Get
        End Property

        ''' <summary>
        ''' Is this element disabled? a disabled control is painted in a gray
        ''' color and it does not react on the mouse at all.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsDisabled As Boolean
            Get
                Dim flag As String = Source.GetAttribute("disabled")

                If String.IsNullOrEmpty(flag) Then
                    Return False
                End If

                Return Not (flag = "false" OrElse flag = "0")
            End Get
        End Property

        ''' <summary>
        ''' The outer rectangle of the box, in the pixel coordinate space of the
        ''' canvas: it covers the border of the box.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Bounds As RectangleF
            Get
                Return Source.PaintBounds
            End Get
        End Property

        ''' <summary>
        ''' The inner rectangle of the box: the area that is left by the border
        ''' and the padding of the box.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ContentBounds As RectangleF
            Get
                Return New RectangleF(
                    Bounds.Left + Source.ActualBorderLeftWidth + Source.ActualPaddingLeft,
                    Bounds.Top + Source.ActualBorderTopWidth + Source.ActualPaddingTop,
                    Bounds.Width - Source.ActualBorderLeftWidth - Source.ActualBorderRightWidth - Source.ActualPaddingLeft - Source.ActualPaddingRight,
                    Bounds.Height - Source.ActualBorderTopWidth - Source.ActualBorderBottomWidth - Source.ActualPaddingTop - Source.ActualPaddingBottom)
            End Get
        End Property

        ''' <summary>
        ''' The background color of the box.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Background As Color
            Get
                Return Source.ActualBackgroundColor
            End Get
        End Property

        ''' <summary>
        ''' The text color of the box.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ForeColor As Color
            Get
                Return Source.ActualColor
            End Get
        End Property

        ''' <summary>
        ''' The font that should be used to draw the text of the box.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Font As Font
            Get
                Return Source.ActualFont
            End Get
        End Property

        ''' <summary>
        ''' The width of the border of the box, the thickest edge wins.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BorderWidth As Single
            Get
                Return std.Max(
                    std.Max(Source.ActualBorderTopWidth, Source.ActualBorderRightWidth),
                    std.Max(Source.ActualBorderBottomWidth, Source.ActualBorderLeftWidth))
            End Get
        End Property

        ''' <summary>
        ''' The color of the border of the box.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BorderColor As Color
            Get
                If Source.ActualBorderTopWidth > 0 Then
                    Return Source.ActualBorderTopColor
                ElseIf Source.ActualBorderRightWidth > 0 Then
                    Return Source.ActualBorderRightColor
                ElseIf Source.ActualBorderBottomWidth > 0 Then
                    Return Source.ActualBorderBottomColor
                Else
                    Return Source.ActualBorderLeftColor
                End If
            End Get
        End Property

        ''' <summary>
        ''' Should the box be painted with rounded corners?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsRounded As Boolean
            Get
                Return Source.IsRounded
            End Get
        End Property

        ''' <summary>
        ''' The largest corner radius of the box.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Radius As Single
            Get
                Return std.Max(
                    std.Max(Source.ActualCornerNW, Source.ActualCornerNE),
                    std.Max(Source.ActualCornerSE, Source.ActualCornerSW))
            End Get
        End Property

        ''' <summary>
        ''' The horizontal alignment of the text of the box: ``left``,
        ''' ``center``, ``right`` or ``justify``.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TextAlign As String
            Get
                Return If(Source.TextAlign, CssConstants.Left).ToLower()
            End Get
        End Property

        ''' <summary>
        ''' The paint order of the box, a box with a larger value is painted on
        ''' top of a box with a smaller one.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ZIndex As Integer
            Get
                Dim z As Integer = 0

                If Integer.TryParse(Source.ZIndex, z) Then
                    Return z
                Else
                    Return 0
                End If
            End Get
        End Property

        ''' <summary>
        ''' The plain text content of the box.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Text As String
            Get
                Return CollectText(Source)
            End Get
        End Property

        ''' <summary>
        ''' The script expression of the ``onclick`` attribute of the element,
        ''' e.g. ``click2('aa+bb+cc')``.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property OnClick As String
            Get
                Return Source.GetAttribute("onclick")
            End Get
        End Property

        ''' <summary>
        ''' The script expression of the ``onchange`` attribute of the element:
        ''' it is raised by a checkbox and a radio button after its checked
        ''' state has been changed.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property OnChange As String
            Get
                Return Source.GetAttribute("onchange")
            End Get
        End Property

        ''' <summary>
        ''' The script expression of the ``onchange`` attribute, the
        ''' ``onclick`` attribute is used as the fallback of it so that a
        ''' checkbox may be declared with the very same attribute as a button.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ChangeScript As String
            Get
                If Not String.IsNullOrEmpty(OnChange) Then
                    Return OnChange
                End If

                Return OnClick
            End Get
        End Property

        ''' <summary>
        ''' The value of a custom attribute of the html element.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public ReadOnly Property Attribute(name As String) As String
            Get
                Return Source.GetAttribute(name)
            End Get
        End Property

        ''' <summary>
        ''' Can the mouse interact with this box?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsInteractive As Boolean
            Get
                If IsDisabled Then
                    Return False
                End If

                Return Tag = "button" OrElse Tag = "input" OrElse
                    Not String.IsNullOrEmpty(OnClick) OrElse
                    Not String.IsNullOrEmpty(OnChange)
            End Get
        End Property

        ''' <summary>
        ''' Collects the plain text of a css box: the text of a html element is
        ''' stored in the anonymous inline boxes of the element, so the text is
        ''' gathered from the whole sub tree of the box.
        ''' </summary>
        ''' <param name="box"></param>
        ''' <returns></returns>
        Public Shared Function CollectText(box As CssBox) As String
            Dim text As New StringBuilder()

            Call walkText(box, text)

            Return text.ToString().Trim()
        End Function

        Private Shared Sub walkText(box As CssBox, text As StringBuilder)
            If box Is Nothing Then
                Return
            End If

            If Not String.IsNullOrEmpty(box.Text) Then
                Call text.Append(box.Text)
            End If

            If box.Boxes Is Nothing Then
                Return
            End If

            For Each child As CssBox In box.Boxes
                ' a box that owns an html tag is a real element of the ui
                ' declaration: the text of it belongs to the element itself and
                ' must not be painted a second time by its container. only the
                ' anonymous boxes (the ones without an html tag) carry the text
                ' of their parent element.
                If child.HtmlTag IsNot Nothing Then
                    Continue For
                End If

                Call walkText(child, text)
            Next
        End Sub

        Public Overrides Function ToString() As String
            Dim b As RectangleF = Bounds

            Return $"<{Tag}> [{CInt(b.Left)},{CInt(b.Top)} {CInt(b.Width)}x{CInt(b.Height)}]"
        End Function
    End Class
End Namespace
