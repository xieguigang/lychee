Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Layout
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush

Namespace Controls

    ''' <summary>
    ''' Paints a hyperlink element: the appearance of the windows forms
    ''' <c>LinkLabel</c> control is imitated by a colored and underlined text
    ''' that changes its color while the mouse is over it.
    ''' </summary>
    Public NotInheritable Class LinkRenderer
        Implements IControlRenderer

        ''' <summary>
        ''' The color of the text of a link.
        ''' </summary>
        ''' <returns></returns>
        Public Property LinkColor As Color = Color.FromArgb(0, 102, 204)

        ''' <summary>
        ''' The color of the text of a link while the mouse is over it.
        ''' </summary>
        ''' <returns></returns>
        Public Property ActiveLinkColor As Color = Color.FromArgb(204, 68, 0)

        ''' <summary>
        ''' The color of the text of a disabled link.
        ''' </summary>
        ''' <returns></returns>
        Public Property DisabledLinkColor As Color = Color.Gray

        ''' <summary>
        ''' The width of the underline.
        ''' </summary>
        ''' <returns></returns>
        Public Property UnderlineWidth As Single = 1.0F

        Public Sub Render(g As IGraphics, box As UiBox) Implements IControlRenderer.Render
            Dim bounds As RectangleF = box.Bounds

            If bounds.Width <= 0 OrElse bounds.Height <= 0 Then
                Return
            End If

            Call BoxPainter.DrawShadow(g, box, bounds)
            Call BoxPainter.FillBox(g, box, bounds, box.Background)
            Call BoxPainter.DrawBorder(g, box, bounds, box.BorderWidth, box.BorderColor)

            Dim text As String = box.Text

            If String.IsNullOrEmpty(text) Then
                Return
            End If

            Dim color As Color = If(box.IsDisabled, DisabledLinkColor,
                If(box.Hover, ActiveLinkColor, LinkColor))
            Dim font As Font = box.Font
            Dim size As SizeF = g.MeasureString(text, font)
            Dim area As RectangleF = box.ContentBounds
            Dim x As Single = area.Left
            Dim y As Single = area.Top + (area.Height - size.Height) / 2.0F

            Select Case box.TextAlign
                Case "center"
                    x = area.Left + (area.Width - size.Width) / 2.0F
                Case "right"
                    x = area.Right - size.Width
            End Select

            Call g.DrawString(text, font, New SolidBrush(color), x, y)

            ' the underline of the link, it is not drawn for a disabled link
            If Not box.IsDisabled Then
                Dim underline As Single = y + size.Height + 1.0F

                Call g.DrawLine(New Pen(color, UnderlineWidth), x, underline, x + size.Width, underline)
            End If
        End Sub
    End Class
End Namespace
