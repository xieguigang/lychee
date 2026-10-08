Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Layout
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush

Namespace Controls

    ''' <summary>
    ''' Paints an image element: the raster image of the ``src`` attribute is
    ''' stretched into the bounds of the element, a gray placeholder is painted
    ''' when the image can not be loaded.
    ''' </summary>
    Public NotInheritable Class ImageRenderer
        Implements IControlRenderer

        ''' <summary>
        ''' The color of the placeholder box of a broken image.
        ''' </summary>
        ''' <returns></returns>
        Public Property PlaceholderColor As Color = Color.Gainsboro

        ''' <summary>
        ''' The color of the cross that marks a broken image.
        ''' </summary>
        ''' <returns></returns>
        Public Property BrokenColor As Color = Color.IndianRed

        Public Sub Render(g As IGraphics, box As UiBox) Implements IControlRenderer.Render
            Dim bounds As RectangleF = box.Bounds

            If bounds.Width <= 0 OrElse bounds.Height <= 0 Then
                Return
            End If

            Call BoxPainter.FillBox(g, box, bounds, box.Background)

            Dim image As Image = UiImages.GetOrLoad(box.Src)

            If image Is Nothing Then
                Call drawPlaceholder(g, box, bounds)
                Return
            End If

            Dim area As RectangleF = box.ContentBounds

            If area.Width <= 0 OrElse area.Height <= 0 Then
                area = bounds
            End If

            Try
                Call g.DrawImage(image, area)
            Catch ex As Exception
                Call Console.WriteLine($"[lychee] the image '{box.Src}' can not be drawn: {ex.Message}")
                Call drawPlaceholder(g, box, bounds)
            End Try

            Call BoxPainter.DrawBorder(g, box, bounds, box.BorderWidth, box.BorderColor)
        End Sub

        Private Sub drawPlaceholder(g As IGraphics, box As UiBox, bounds As RectangleF)
            Call g.FillRectangle(New SolidBrush(PlaceholderColor), bounds)
            Call g.DrawRectangle(New Pen(BrokenColor, 1.0F), bounds)

            Dim a As New PointF(bounds.Left + 2, bounds.Top + 2)
            Dim b As New PointF(bounds.Right - 2, bounds.Bottom - 2)
            Dim c As New PointF(bounds.Right - 2, bounds.Top + 2)
            Dim d As New PointF(bounds.Left + 2, bounds.Bottom - 2)

            Call g.DrawLine(New Pen(BrokenColor, 1.0F), a, b)
            Call g.DrawLine(New Pen(BrokenColor, 1.0F), c, d)

            Dim alt As String = box.Attribute("alt")

            If Not String.IsNullOrEmpty(alt) Then
                Dim size As SizeF = g.MeasureString(alt, box.Font)
                Dim x As Single = bounds.Left + (bounds.Width - size.Width) / 2.0F
                Dim y As Single = bounds.Bottom - size.Height - 2

                Call g.DrawString(alt, box.Font, New SolidBrush(BrokenColor), x, y)
            End If
        End Sub
    End Class
End Namespace
