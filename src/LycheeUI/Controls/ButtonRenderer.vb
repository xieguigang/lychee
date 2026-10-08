Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Layout

Namespace Controls

    ''' <summary>
    ''' Paints a push button: the background of the button is shaded when the
    ''' mouse is over it and it is shaded a little bit more while a mouse button
    ''' is pressed down on it.
    ''' </summary>
    Public NotInheritable Class ButtonRenderer
        Implements IControlRenderer

        ''' <summary>
        ''' The brightness of the background of a hovered button.
        ''' </summary>
        ''' <returns></returns>
        Public Property HoverFactor As Single = 1.25F

        ''' <summary>
        ''' The brightness of the background of a pressed button.
        ''' </summary>
        ''' <returns></returns>
        Public Property PressedFactor As Single = 0.85F

        Public Sub Render(g As IGraphics, box As UiBox) Implements IControlRenderer.Render
            Dim bounds As RectangleF = box.Bounds

            If bounds.Width <= 0 OrElse bounds.Height <= 0 Then
                Return
            End If

            Dim background As Color = box.Background

            If box.Pressed Then
                background = BoxPainter.Shade(background, PressedFactor)
            ElseIf box.Hover Then
                background = BoxPainter.Shade(background, HoverFactor)
            End If

            Call BoxPainter.DrawShadow(g, box, bounds)
            Call BoxPainter.FillBox(g, box, bounds, background)
            Call BoxPainter.DrawBorder(g, box, bounds, box.BorderWidth, box.BorderColor)
            Call BoxPainter.DrawText(g, box, box.Text, box.ForeColor)
        End Sub
    End Class
End Namespace
