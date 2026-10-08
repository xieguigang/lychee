Imports System.Drawing
Imports System.Windows.Forms

Namespace Render

    ''' <summary>
    ''' The abstraction of the drawing surface that hosts the user interface.
    ''' </summary>
    ''' <remarks>
    ''' The ui engine does not talk to a concrete canvas type directly: the
    ''' <see cref="DxCanvasSurface"/> implementation embeds a
    ''' <c>DxCanvas</c> user control into the host control, while the
    ''' <see cref="DxWindowSurface"/> implementation paints on the window
    ''' handle of the host control itself.
    ''' </remarks>
    Public Interface IRenderSurface
        Inherits IDisposable

        ''' <summary>
        ''' The current size of the canvas in pixels.
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Size As Size

        ''' <summary>
        ''' Raised when the canvas needs to redraw its content.
        ''' </summary>
        Event Frame As EventHandler(Of RenderFrameEventArgs)

        ''' <summary>
        ''' Raised when a mouse button is pressed down on the canvas.
        ''' </summary>
        Event PointerDown As EventHandler(Of PointerEventArgs)

        ''' <summary>
        ''' Raised when the mouse is moved over the canvas.
        ''' </summary>
        Event PointerMove As EventHandler(Of PointerEventArgs)

        ''' <summary>
        ''' Raised when a mouse button is released on the canvas.
        ''' </summary>
        Event PointerUp As EventHandler(Of PointerEventArgs)

        ''' <summary>
        ''' Raised when a key is pressed down while the canvas holds the keyboard
        ''' focus: the navigation keys and the editing keys are delivered here.
        ''' </summary>
        Event KeyDown As EventHandler(Of CanvasKeyEventArgs)

        ''' <summary>
        ''' Raised for every printable character that has been typed into the
        ''' canvas while it holds the keyboard focus.
        ''' </summary>
        Event TextInput As EventHandler(Of CanvasTextEventArgs)

        ''' <summary>
        ''' Mounts this drawing surface on the given host control.
        ''' </summary>
        ''' <param name="container">
        ''' any windows forms control: a <see cref="Form"/>, a
        ''' <see cref="Panel"/> or a <see cref="PictureBox"/> for example.
        ''' </param>
        Sub Attach(container As Control)

        ''' <summary>
        ''' Requests a new frame of the canvas.
        ''' </summary>
        Sub Invalidate()
    End Interface
End Namespace
