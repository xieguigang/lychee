Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Layout

Namespace Controls

    ''' <summary>
    ''' Paints one kind of the html elements of the ui declaration.
    ''' </summary>
    Public Interface IControlRenderer

        ''' <summary>
        ''' Draws the given control on the canvas.
        ''' </summary>
        ''' <param name="g">the gpu accelerated canvas of the current frame.</param>
        ''' <param name="box">the control that should be drawn.</param>
        Sub Render(g As IGraphics, box As UiBox)
    End Interface
End Namespace
