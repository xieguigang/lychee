Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Namespace Render

    ''' <summary>
    ''' The data of a rendering frame of the ui canvas.
    ''' </summary>
    Public Class RenderFrameEventArgs : Inherits EventArgs

        ''' <summary>
        ''' The canvas of the frame that is being rendered: every drawing
        ''' command that is submitted in the handler becomes visible when the
        ''' frame is presented right after the handler returns.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Graphics As IGraphics

        ''' <summary>
        ''' The size of the canvas in pixels.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Size As Size

        ''' <summary>
        ''' The width of the canvas in pixels.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Width As Integer
            Get
                Return Size.Width
            End Get
        End Property

        ''' <summary>
        ''' The height of the canvas in pixels.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Height As Integer
            Get
                Return Size.Height
            End Get
        End Property

        Sub New(graphics As IGraphics, size As Size)
            Me.Graphics = graphics
            Me.Size = size
        End Sub
    End Class
End Namespace
