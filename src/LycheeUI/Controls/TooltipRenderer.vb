Imports System.Drawing
Imports Microsoft.VisualBasic.Drawing.DirectX
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports std = System.Math

Namespace Controls

    ''' <summary>
    ''' Paints the rich text tooltip of a control.
    ''' </summary>
    ''' <remarks>
    ''' The content of the tooltip is a small html fragment like
    ''' ``&lt;b&gt;title&lt;/b&gt;&lt;br/&gt;&lt;font color='gray'&gt;text&lt;/font&gt;``,
    ''' it is parsed by the <see cref="TextAPI"/> of the html library and is
    ''' painted by the <see cref="HTMLRender.RenderHTML"/> extension of the
    ''' imaging library.
    ''' </remarks>
    Public Module TooltipRenderer

        ''' <summary>
        ''' The horizontal and the vertical padding of the tooltip panel.
        ''' </summary>
        ''' <returns></returns>
        Public Property Padding As Single = 6.0F

        ''' <summary>
        ''' The background color of the tooltip panel.
        ''' </summary>
        ''' <returns></returns>
        Public Property BackColor As Color = Color.FromArgb(255, 255, 225)

        ''' <summary>
        ''' The color of the frame of the tooltip panel.
        ''' </summary>
        ''' <returns></returns>
        Public Property FrameColor As Color = Color.FromArgb(120, 120, 120)

        ''' <summary>
        ''' The color of the drop shadow of the tooltip panel.
        ''' </summary>
        ''' <returns></returns>
        Public Property ShadowColor As Color = Color.FromArgb(70, 0, 0, 0)

        ''' <summary>
        ''' The default text color of the tooltip.
        ''' </summary>
        ''' <returns></returns>
        Public Property ForeColor As Color = Color.Black

        ''' <summary>
        ''' Measures the size of the tooltip panel for the given rich text.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="text"></param>
        ''' <param name="font"></param>
        ''' <returns></returns>
        Public Function Measure(g As IGraphics, text As String, font As Font) As SizeF
            If String.IsNullOrEmpty(text) Then
                Return SizeF.Empty
            End If

            Dim tokens As TextString() = TextAPI.TryParse(text, font, ForeColor).ToArray()
            Dim content As SizeF = g.MeasureSize(tokens)
            Dim pad As Single = Padding * 2

            Return New SizeF(content.Width + pad, content.Height + pad)
        End Function

        ''' <summary>
        ''' Paints the tooltip panel at the given location.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="text"></param>
        ''' <param name="font"></param>
        ''' <param name="location">the preferred top left corner of the panel.</param>
        ''' <param name="viewport">
        ''' the size of the canvas, the panel is clamped into it because a swap
        ''' chain can not paint outside of its own window.
        ''' </param>
        Public Sub Render(g As IGraphics, text As String, font As Font,
                          location As PointF, viewport As Size)

            If String.IsNullOrEmpty(text) OrElse viewport.Width <= 0 OrElse viewport.Height <= 0 Then
                Return
            End If

            Dim tokens As TextString() = TextAPI.TryParse(text, font, ForeColor).ToArray()
            Dim content As SizeF = g.MeasureSize(tokens)
            Dim size As New SizeF(content.Width + Padding * 2, content.Height + Padding * 2)
            Dim x As Single = location.X
            Dim y As Single = location.Y

            ' the panel must stay inside of the canvas, so it is moved to the
            ' left and upwards when it would fall out of the viewport
            If x + size.Width > viewport.Width Then
                x = std.Max(0, viewport.Width - size.Width)
            End If
            If y + size.Height > viewport.Height Then
                y = std.Max(0, viewport.Height - size.Height)
            End If

            Dim bounds As New RectangleF(x, y, size.Width, size.Height)
            Dim dx As DxGraphics = TryCast(g, DxGraphics)
            Dim radius As Single = 3.0F

            ' the shadow of the panel
            If dx IsNot Nothing Then
                Call dx.FillRoundedRectangle(New SolidBrush(ShadowColor), bounds, radius)
            End If

            Call g.FillRectangle(New SolidBrush(BackColor), bounds)
            Call g.DrawRectangle(New Pen(FrameColor, 1.0F), bounds)
            Call g.RenderHTML(tokens, New PointF(x + Padding, y + Padding))
        End Sub
    End Module
End Namespace
