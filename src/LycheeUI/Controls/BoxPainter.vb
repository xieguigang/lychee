Imports System.Drawing
Imports Microsoft.VisualBasic.Drawing.DirectX
Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Layout
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush

Namespace Controls

    ''' <summary>
    ''' The primitive drawing helpers that are shared by the control renderers:
    ''' the rounded corners of a control are painted with the native direct2d
    ''' rounded rectangle when the canvas is a directx canvas, and they fall
    ''' back to a plain rectangle on any other graphics driver.
    ''' </summary>
    Public Module BoxPainter

        ''' <summary>
        ''' Paints the interior of the box.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="box"></param>
        ''' <param name="bounds">the outer rectangle of the box.</param>
        ''' <param name="color">the fill color, a transparent color is skipped.</param>
        Public Sub FillBox(g As IGraphics, box As UiBox, bounds As RectangleF, color As Color)
            If color.A = 0 Then
                Return
            End If

            Dim radius As Single = box.Radius
            Dim dx As DxGraphics = TryCast(g, DxGraphics)

            If radius > 0 AndAlso dx IsNot Nothing Then
                Call dx.FillRoundedRectangle(New SolidBrush(color), bounds, radius)
            Else
                Call g.FillRectangle(New SolidBrush(color), bounds)
            End If
        End Sub

        ''' <summary>
        ''' Paints the outline of the box.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="box"></param>
        ''' <param name="bounds">the outer rectangle of the box.</param>
        ''' <param name="width">the width of the outline, zero means no border.</param>
        ''' <param name="color">the color of the outline.</param>
        Public Sub DrawBorder(g As IGraphics, box As UiBox, bounds As RectangleF, width As Single, color As Color)
            If width <= 0 OrElse color.A = 0 Then
                Return
            End If

            Dim radius As Single = box.Radius
            Dim dx As DxGraphics = TryCast(g, DxGraphics)
            Dim pen As New Pen(color, width)

            If radius > 0 AndAlso dx IsNot Nothing Then
                Call dx.DrawRoundedRectangle(pen, bounds, radius)
            Else
                Call g.DrawRectangle(pen, bounds)
            End If
        End Sub

        ''' <summary>
        ''' Draws the text of a control inside of its content box.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="box"></param>
        ''' <param name="text"></param>
        ''' <param name="color">the text color, a transparent color is skipped.</param>
        Public Sub DrawText(g As IGraphics, box As UiBox, text As String, color As Color)
            If String.IsNullOrEmpty(text) OrElse color.A = 0 Then
                Return
            End If

            Dim area As RectangleF = box.ContentBounds
            Dim font As Font = box.Font
            Dim size As SizeF = g.MeasureString(text, font)
            Dim x As Single = area.Left
            Dim y As Single = area.Top + (area.Height - size.Height) / 2.0F

            Select Case box.TextAlign
                Case "center"
                    x = area.Left + (area.Width - size.Width) / 2.0F
                Case "right"
                    x = area.Right - size.Width
                Case Else
                    x = area.Left
            End Select

            If area.Width > 0 AndAlso size.Width > area.Width Then
                ' the text is wider than the control, keep it inside of the box
                x = area.Left
            End If

            Call g.DrawString(text, font, New SolidBrush(color), x, y)
        End Sub

        ''' <summary>
        ''' Brightens or darkens a color: it is used to paint the hover and the
        ''' pressed state of a control.
        ''' </summary>
        ''' <param name="color"></param>
        ''' <param name="factor">
        ''' a value that is greater than one brightens the color and a value
        ''' that is less than one darkens it.
        ''' </param>
        ''' <returns></returns>
        Public Function Shade(color As Color, factor As Single) As Color
            If color.A = 0 Then
                Return color
            End If

            Dim r As Integer = clamp(CInt(color.R * factor))
            Dim g As Integer = clamp(CInt(color.G * factor))
            Dim b As Integer = clamp(CInt(color.B * factor))

            Return Color.FromArgb(color.A, r, g, b)
        End Function

        Private Function clamp(v As Integer) As Integer
            If v < 0 Then
                Return 0
            ElseIf v > 255 Then
                Return 255
            Else
                Return v
            End If
        End Function
    End Module
End Namespace
