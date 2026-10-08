Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Layout
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush

Namespace Controls

    ''' <summary>
    ''' Paints a radio button: a small circle at the left side of the control
    ''' that holds a filled dot while the control is selected, and the label
    ''' text at the right side of it.
    ''' </summary>
    ''' <remarks>
    ''' The radio buttons that share the same ``name`` attribute are mutually
    ''' exclusive: that rule is applied by the ui engine itself when a radio
    ''' button has been clicked, this renderer only paints the state of it.
    ''' </remarks>
    Public NotInheritable Class RadioRenderer
        Implements IControlRenderer

        ''' <summary>
        ''' The diameter of the circle indicator in pixels.
        ''' </summary>
        ''' <returns></returns>
        Public Property BoxSize As Single = 14.0F

        ''' <summary>
        ''' The background color of the circle indicator.
        ''' </summary>
        ''' <returns></returns>
        Public Property BoxColor As Color = Color.White

        ''' <summary>
        ''' The color of the filled dot and of the frame of a selected control.
        ''' </summary>
        ''' <returns></returns>
        Public Property AccentColor As Color = Color.DodgerBlue

        ''' <summary>
        ''' The color of the frame of an unselected control.
        ''' </summary>
        ''' <returns></returns>
        Public Property FrameColor As Color = Color.DimGray

        ''' <summary>
        ''' The gap between the circle indicator and the label text.
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

            Call g.FillEllipse(New SolidBrush(BoxColor), indicator)
            Call g.DrawEllipse(New Pen(If(box.Checked, accent, FrameColor), 1.0F), indicator)

            If box.Checked Then
                Dim inset As Single = size * 0.28F
                Dim dot As New RectangleF(
                    indicator.Left + inset,
                    indicator.Top + inset,
                    size - inset * 2,
                    size - inset * 2)

                Call g.FillEllipse(New SolidBrush(accent), dot)
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
