Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Layout

Namespace Controls

    ''' <summary>
    ''' Paints a plain text element.
    ''' </summary>
    Public NotInheritable Class LabelRenderer
        Implements IControlRenderer

        Public Sub Render(g As IGraphics, box As UiBox) Implements IControlRenderer.Render
            Dim bounds As RectangleF = box.Bounds

            If bounds.Width <= 0 OrElse bounds.Height <= 0 Then
                Return
            End If

            Call BoxPainter.DrawShadow(g, box, bounds)
            Call BoxPainter.FillBox(g, box, bounds, box.Background)
            Call BoxPainter.DrawBorder(g, box, bounds, box.BorderWidth, box.BorderColor)
            Call BoxPainter.DrawText(g, box, box.Text, box.ForeColor)
        End Sub
    End Class
End Namespace
