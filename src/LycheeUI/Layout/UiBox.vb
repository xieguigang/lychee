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

        Sub New(source As CssBox, tag As String, order As Integer)
            Me.Source = source
            Me.Tag = tag
            Me.Order = order
        End Sub

        ''' <summary>
        ''' The outer rectangle of the box, in the pixel coordinate space of the
        ''' canvas: it covers the border of the box.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Bounds As RectangleF
            Get
                Return Source.Bounds
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
                Return Not String.IsNullOrEmpty(OnClick) OrElse Tag = "button"
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
                Call walkText(child, text)
            Next
        End Sub

        Public Overrides Function ToString() As String
            Dim b As RectangleF = Bounds

            Return $"<{Tag}> [{CInt(b.Left)},{CInt(b.Top)} {CInt(b.Width)}x{CInt(b.Height)}]"
        End Function
    End Class
End Namespace
