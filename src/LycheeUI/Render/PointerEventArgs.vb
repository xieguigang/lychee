Imports System.Drawing
Imports System.Windows.Forms

Namespace Render

    ''' <summary>
    ''' A mouse event that is expressed in the relative coordinate space of
    ''' the ui canvas: <see cref="X"/> and <see cref="Y"/> are measured from
    ''' the top left corner of the host control client area, so that they can
    ''' be compared with the layout bounds of the controls directly.
    ''' </summary>
    Public Class PointerEventArgs : Inherits EventArgs

        ''' <summary>
        ''' The horizontal offset of the mouse from the left edge of the canvas.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property X As Integer

        ''' <summary>
        ''' The vertical offset of the mouse from the top edge of the canvas.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Y As Integer

        ''' <summary>
        ''' The mouse button that raises this event.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Button As MouseButtons

        ''' <summary>
        ''' The number of the notches that the wheel has been turned, it is only
        ''' set by the wheel events: a positive value means that the wheel has
        ''' been turned away from the user.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Delta As Integer

        ''' <summary>
        ''' The location of the mouse inside the canvas.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Location As Point
            Get
                Return New Point(X, Y)
            End Get
        End Property

        Sub New(x As Integer, y As Integer,
                Optional button As MouseButtons = MouseButtons.None,
                Optional delta As Integer = 0)

            Me.X = x
            Me.Y = y
            Me.Button = button
            Me.Delta = delta
        End Sub
    End Class
End Namespace
