Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Drawing.DirectX
Imports Microsoft.VisualBasic.Imaging

Namespace Render

    ''' <summary>
    ''' The alternative rendering backend of the ui engine: the directx swap
    ''' chain is created on the window handle of the host control itself, so
    ''' the control collection of the host is not touched at all.
    ''' </summary>
    ''' <remarks>
    ''' A windows forms control paints its own background with gdi+ before it
    ''' raises the paint event, and that background would erase the directx
    ''' frame, so the window procedure of the host control is subclassed at
    ''' here: the background erasing is dropped and the whole frame is drawn
    ''' inside of the <c>WM_PAINT</c> message.
    ''' </remarks>
    Public NotInheritable Class DxWindowSurface
        Implements IRenderSurface

        Private Const WM_PAINT As Integer = &HF
        Private Const WM_ERASEBKGND As Integer = &H14
        Private Const WM_SIZE As Integer = &H5

        Private ReadOnly listener As WindowListener
        Private canvas As DxWindowCanvas
        Private host As Control
        Private disposedValue As Boolean

        Public Event Frame As EventHandler(Of RenderFrameEventArgs) Implements IRenderSurface.Frame
        Public Event PointerDown As EventHandler(Of PointerEventArgs) Implements IRenderSurface.PointerDown
        Public Event PointerMove As EventHandler(Of PointerEventArgs) Implements IRenderSurface.PointerMove
        Public Event PointerUp As EventHandler(Of PointerEventArgs) Implements IRenderSurface.PointerUp
        Public Event KeyDown As EventHandler(Of CanvasKeyEventArgs) Implements IRenderSurface.KeyDown
        Public Event TextInput As EventHandler(Of CanvasTextEventArgs) Implements IRenderSurface.TextInput

        Sub New(Optional vsync As Boolean = True)
            listener = New WindowListener(Me)
            Me.vsync = vsync
        End Sub

        Private ReadOnly vsync As Boolean

        Public ReadOnly Property Size As Size Implements IRenderSurface.Size
            Get
                If host Is Nothing Then
                    Return Size.Empty
                End If

                Return host.ClientSize
            End Get
        End Property

        Public Sub Attach(container As Control) Implements IRenderSurface.Attach
            If container Is Nothing Then
                Throw New ArgumentNullException(NameOf(container))
            End If

            host = container

            If Not container.IsHandleCreated Then
                ' the window handle is required by the swap chain, a handle is
                ' created here when the host control is not yet shown
                Dim force As IntPtr = container.Handle
            End If

            AddHandler container.MouseDown, AddressOf handleMouseDown
            AddHandler container.MouseMove, AddressOf handleMouseMove
            AddHandler container.MouseUp, AddressOf handleMouseUp
            AddHandler container.KeyDown, AddressOf handleKeyDown
            AddHandler container.KeyPress, AddressOf handleKeyPress

            Call listener.AssignHandle(container.Handle)
        End Sub

        Public Sub Invalidate() Implements IRenderSurface.Invalidate
            If host Is Nothing Then
                Return
            End If

            ' the gdi+ invalidation is enough at here: the paint message is
            ' then handled by the window listener of this surface
            host.Invalidate()
        End Sub

        ''' <summary>
        ''' Draws one frame of the user interface on the swap chain of the host
        ''' window handle.
        ''' </summary>
        Friend Sub DrawFrame()
            If host Is Nothing OrElse host.ClientSize.Width <= 0 OrElse host.ClientSize.Height <= 0 Then
                Return
            End If

            If canvas Is Nothing Then
                canvas = New DxWindowCanvas(host.Handle, host.ClientSize.Width, host.ClientSize.Height, 96.0F, vsync)
            End If

            Try
                Call canvas.BeginDraw()
                RaiseEvent Frame(Me, New RenderFrameEventArgs(canvas.Graphics, host.ClientSize))
                Call canvas.EndDraw()
            Catch ex As Exception
                Call Console.WriteLine("[lychee] dx window frame error: " & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' Rebuilds the swap chain after the host control has been resized.
        ''' </summary>
        Friend Sub ResizeCanvas()
            If canvas Is Nothing OrElse host Is Nothing Then
                Return
            End If

            If host.ClientSize.Width <= 0 OrElse host.ClientSize.Height <= 0 Then
                Return
            End If

            Try
                Call canvas.Resize(host.ClientSize.Width, host.ClientSize.Height)
            Catch ex As Exception
                Call Console.WriteLine("[lychee] dx window resize error: " & ex.Message)
            End Try
        End Sub

        Private Sub handleMouseDown(sender As Object, e As MouseEventArgs)
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

        ''' <summary>
        ''' The native window subclass of the host control.
        ''' </summary>
        Private NotInheritable Class WindowListener : Inherits NativeWindow

            Private ReadOnly surface As DxWindowSurface

            Sub New(surface As DxWindowSurface)
                Me.surface = surface
            End Sub

            Protected Overrides Sub WndProc(ByRef m As Message)
                Select Case m.Msg
                    Case WM_ERASEBKGND
                        ' never let gdi+ erase the directx frame
                        m.Result = New IntPtr(1)
                        Return

                    Case WM_PAINT
                        Call surface.DrawFrame()
                        m.Result = IntPtr.Zero
                        Return

                    Case WM_SIZE
                        Call surface.ResizeCanvas()
                End Select

                MyBase.WndProc(m)
            End Sub
        End Class

        Private Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    If host IsNot Nothing Then
                        RemoveHandler host.MouseDown, AddressOf handleMouseDown
                        RemoveHandler host.MouseMove, AddressOf handleMouseMove
                        RemoveHandler host.MouseUp, AddressOf handleMouseUp
                        RemoveHandler host.KeyDown, AddressOf handleKeyDown
                        RemoveHandler host.KeyPress, AddressOf handleKeyPress
                    End If

                    If listener.Handle <> IntPtr.Zero Then
                        Call listener.ReleaseHandle()
                    End If

                    If canvas IsNot Nothing Then
                        Call canvas.Dispose()
                    End If
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
