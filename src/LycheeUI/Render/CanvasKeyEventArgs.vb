Imports System.Windows.Forms

Namespace Render

    ''' <summary>
    ''' A key down event of the ui canvas.
    ''' </summary>
    ''' <remarks>
    ''' This type is not named as ``KeyEventArgs`` on purpose: a name like that
    ''' collides with the <c>System.Windows.Forms.KeyEventArgs</c> type of the
    ''' host application.
    ''' </remarks>
    Public Class CanvasKeyEventArgs : Inherits EventArgs

        ''' <summary>
        ''' The key code of the pressed key.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property KeyCode As Keys

        ''' <summary>
        ''' Is the alt key held down?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Alt As Boolean

        ''' <summary>
        ''' Is the control key held down?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Control As Boolean

        ''' <summary>
        ''' Is the shift key held down?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Shift As Boolean

        ''' <summary>
        ''' Set this flag to true when the key has been consumed by the ui, so
        ''' that the host control stops processing it.
        ''' </summary>
        ''' <returns></returns>
        Public Property Handled As Boolean

        Sub New(keyCode As Keys, Optional alt As Boolean = False,
                Optional control As Boolean = False,
                Optional shift As Boolean = False)

            Me.KeyCode = keyCode
            Me.Alt = alt
            Me.Control = control
            Me.Shift = shift
        End Sub
    End Class
End Namespace
