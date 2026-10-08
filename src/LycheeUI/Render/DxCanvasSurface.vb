Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Drawing.DirectX
Imports Microsoft.VisualBasic.Imaging

Namespace Render

    ''' <summary>
    ''' The default rendering backend of the ui engine: a gpu accelerated
    ''' <c>DxCanvas</c> user control is docked into the host control, so the
    ''' device creation, the device lost recovery and the resize handling are
    ''' done by the canvas control itself.
    ''' </summary>
    ''' <remarks>
    ''' The canvas paints every pixel of its own client area, so the automatic
    ''' background clearing of the canvas is disabled here: the background of
    ''' the user interface is painted by the ui engine, which reads it from the
    ''' root element of the ui declaration.
    ''' </remarks>
    Public NotInheritable Class DxCanvasSurface
        Implements IRenderSurface

        Private ReadOnly canvas As DxCanvas
        Private disposedValue As Boolean

        ''' <summary>
        ''' The underlying directx canvas control of this surface.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CanvasControl As DxCanvas
            Get
                Return canvas
            End Get
        End Property

        Public Event Frame As EventHandler(Of RenderFrameEventArgs) Implements IRenderSurface.Frame
        Public Event PointerDown As EventHandler(Of PointerEventArgs) Implements IRenderSurface.PointerDown
        Public Event PointerMove As EventHandler(Of PointerEventArgs) Implements IRenderSurface.PointerMove
        Public Event PointerUp As EventHandler(Of PointerEventArgs) Implements IRenderSurface.PointerUp
        Public Event KeyDown As EventHandler(Of CanvasKeyEventArgs) Implements IRenderSurface.KeyDown
        Public Event TextInput As EventHandler(Of CanvasTextEventArgs) Implements IRenderSurface.TextInput

        Sub New(Optional backgroundColor As Color = Nothing, Optional vsync As Boolean = True)
            canvas = New DxCanvas With {
                .Dock = DockStyle.Fill,
                .AutoClear = False,
                .BackgroundColor = If(backgroundColor.IsEmpty, Color.White, backgroundColor),
                .VSync = vsync
            }

            AddHandler canvas.Render, AddressOf handleRender
            AddHandler canvas.MouseDown, AddressOf handleMouseDown
            AddHandler canvas.MouseMove, AddressOf handleMouseMove
            AddHandler canvas.MouseUp, AddressOf handleMouseUp
            AddHandler canvas.KeyDown, AddressOf handleKeyDown
            AddHandler canvas.KeyPress, AddressOf handleKeyPress
        End Sub

        Public ReadOnly Property Size As Size Implements IRenderSurface.Size
            Get
                Return canvas.ClientSize
            End Get
        End Property

        Public Sub Attach(container As Control) Implements IRenderSurface.Attach
            If container Is Nothing Then
                Throw New ArgumentNullException(NameOf(container))
            End If

            ' the canvas is added first so that it is docked behind the other
            ' child controls of the host control
            container.Controls.Add(canvas)
        End Sub

        Public Sub Invalidate() Implements IRenderSurface.Invalidate
            ' invalidating the host control would not repaint the canvas: the
            ' parent window clips the child window out of its own update region
            canvas.Invalidate()
        End Sub

        Private Sub handleRender(sender As Object, e As DxRenderEventArgs)
            RaiseEvent Frame(Me, New RenderFrameEventArgs(e.Graphics, e.Size))
        End Sub

        Private Sub handleMouseDown(sender As Object, e As MouseEventArgs)
            ' the canvas has to take the keyboard focus on a mouse click, so
            ' that a text input control of the user interface can be edited
            If Not canvas.Focused Then
                Call canvas.Focus()
            End If

            RaiseEvent PointerDown(Me, New PointerEventArgs(e.X, e.Y, e.Button))
        End Sub

        Private Sub handleMouseMove(sender As Object, e As MouseEventArgs)
            RaiseEvent PointerMove(Me, New PointerEventArgs(e.X, e.Y, e.Button))
        End Sub

        Private Sub handleMouseUp(sender As Object, e As MouseEventArgs)
            RaiseEvent PointerUp(Me, New PointerEventArgs(e.X, e.Y, e.Button))
        End Sub

        Private Sub handleKeyDown(sender As Object, e As KeyEventArgs)
            Dim args As New CanvasKeyEventArgs(e.KeyCode, e.Alt, e.Control, e.Shift)

            RaiseEvent KeyDown(Me, args)

            If args.Handled Then
                e.Handled = True
                e.SuppressKeyPress = True
            End If
        End Sub

        Private Sub handleKeyPress(sender As Object, e As KeyPressEventArgs)
            Dim args As New CanvasTextEventArgs(e.KeyChar)

            RaiseEvent TextInput(Me, args)

            If args.Handled Then
                e.Handled = True
            End If
        End Sub

        Private Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    RemoveHandler canvas.Render, AddressOf handleRender
                    RemoveHandler canvas.MouseDown, AddressOf handleMouseDown
                    RemoveHandler canvas.MouseMove, AddressOf handleMouseMove
                    RemoveHandler canvas.MouseUp, AddressOf handleMouseUp
                    RemoveHandler canvas.KeyDown, AddressOf handleKeyDown
                    RemoveHandler canvas.KeyPress, AddressOf handleKeyPress

                    Call canvas.Dispose()
                End If

                disposedValue = True
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Call Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
