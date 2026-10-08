Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Layout

Namespace Controls

    ''' <summary>
    ''' Paints a generic container element: only the background and the border
    ''' of the box are drawn, the child boxes are painted by their own renderer.
    ''' </summary>
    Public NotInheritable Class DivRenderer
        Implements IControlRenderer

        Public Sub Render(g As IGraphics, box As UiBox) Implements IControlRenderer.Render
            Dim bounds As RectangleF = box.Bounds

            If bounds.Width <= 0 OrElse bounds.Height <= 0 Then
                Return
            End If

            Call BoxPainter.FillBox(g, box, bounds, box.Background)
            Call BoxPainter.DrawBorder(g, box, bounds, box.BorderWidth, box.BorderColor)
            Call BoxPainter.DrawText(g, box, box.Text, box.ForeColor)
        End Sub
    End Class
End Namespace
