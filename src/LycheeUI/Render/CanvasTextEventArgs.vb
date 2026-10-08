Namespace Render

    ''' <summary>
    ''' A printable character that has been typed into the ui canvas.
    ''' </summary>
    ''' <remarks>
    ''' Only a printable character reaches this event: the control keys like the
    ''' backspace or the arrow keys are delivered through the
    ''' <see cref="IRenderSurface.KeyDown"/> event instead.
    ''' </remarks>
    Public Class CanvasTextEventArgs : Inherits EventArgs

        ''' <summary>
        ''' The character that has been typed.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Character As Char

        ''' <summary>
        ''' Set this flag to true when the character has been consumed by the ui.
        ''' </summary>
        ''' <returns></returns>
        Public Property Handled As Boolean

        Sub New(character As Char)
            Me.Character = character
        End Sub

        Public Overrides Function ToString() As String
            Return $"'{Character}'"
        End Function
    End Class
End Namespace
