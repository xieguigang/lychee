Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Layout
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush

Namespace Controls

    ''' <summary>
    ''' Paints a check box: a small square at the left side of the control that
    ''' holds a tick mark while the control is selected, and the label text at
    ''' the right side of it.
    ''' </summary>
    Public NotInheritable Class CheckboxRenderer
        Implements IControlRenderer

        ''' <summary>
        ''' The size of the square indicator in pixels.
        ''' </summary>
        ''' <returns></returns>
        Public Property BoxSize As Single = 14.0F

        ''' <summary>
        ''' The background color of the square indicator.
        ''' </summary>
        ''' <returns></returns>
        Public Property BoxColor As Color = Color.White

        ''' <summary>
        ''' The color of the tick mark and of the frame of a selected control.
        ''' </summary>
        ''' <returns></returns>
        Public Property AccentColor As Color = Color.DodgerBlue

        ''' <summary>
        ''' The color of the frame of an unselected control.
        ''' </summary>
        ''' <returns></returns>
        Public Property FrameColor As Color = Color.DimGray

        ''' <summary>
        ''' The gap between the square indicator and the label text.
        ''' </summary>
        ''' <returns></returns>
        Public Property Gap As Single = 6.0F

        Public Sub Render(g As IGraphics, box As UiBox) Implements IControlRenderer.Render
            Dim bounds As RectangleF = box.Bounds

            If bounds.Width <= 0 OrElse bounds.Height <= 0 Then
                Return
            End If

            Call BoxPainter.FillBox(g, box, bounds, box.Background)
            Call BoxPainter.DrawBorder(g, box, bounds, box.BorderWidth, box.BorderColor)

            Dim size As Single = BoxSize
            Dim area As RectangleF = box.ContentBounds
            Dim indicator As New RectangleF(
                area.Left,
                area.Top + (area.Height - size) / 2.0F,
                size, size)

            Dim accent As Color = If(box.IsDisabled, Color.Gray, AccentColor)

            Call g.FillRectangle(New SolidBrush(BoxColor), indicator)
            Call g.DrawRectangle(New Pen(If(box.Checked, accent, FrameColor), 1.0F), indicator)

            If box.Checked Then
                ' the tick mark: a short stroke down to the right followed by a
                ' longer stroke up to the right
                Dim x As Single = indicator.Left
                Dim y As Single = indicator.Top
                Dim tick As PointF() = {
                    New PointF(x + size * 0.22F, y + size * 0.52F),
                    New PointF(x + size * 0.43F, y + size * 0.72F),
                    New PointF(x + size * 0.78F, y + size * 0.28F)
                }

                Call g.DrawLines(New Pen(accent, 2.0F), tick)
            End If

            Dim text As String = box.LabelText

            If Not String.IsNullOrEmpty(text) Then
                Dim font As Font = box.Font
                Dim labelSize As SizeF = g.MeasureString(text, font)
                Dim color As Color = If(box.ForeColor.A = 0, Color.Black, box.ForeColor)
                Dim x As Single = indicator.Right + Gap
                Dim y As Single = area.Top + (area.Height - labelSize.Height) / 2.0F

                Call g.DrawString(text, font, New SolidBrush(If(box.IsDisabled, Color.Gray, color)), x, y)
            End If
        End Sub
    End Class
End Namespace
