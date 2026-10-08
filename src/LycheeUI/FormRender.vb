Imports System.Drawing
Imports System.Windows.Forms
Imports System.Xml.Linq
Imports Microsoft.VisualBasic.Drawing.DirectX
Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Controls
Imports LycheeUI.Events
Imports LycheeUI.Layout
Imports LycheeUI.Render

''' <summary>
''' The ui engine: it renders a html + css user interface declaration on a
''' windows forms control with the directx api.
''' </summary>
''' <remarks>
''' The engine is created by the host control itself:
'''
''' ```vbnet
''' ReadOnly UI As XElement =
'''     &lt;form style="background-color: gray;">
'''         &lt;button style="left:50%;top:50%;width:200px;height:60px"
'''                 onclick="clickButton()">hello&lt;/button>
'''     &lt;/form>
'''
''' Dim ui As New FormRender(UI, Me)
''' ```
'''
''' The host may be any windows forms control: a <see cref="Form"/>, a
''' <see cref="Panel"/> or a <see cref="PictureBox"/>, the layout of the user
''' interface is recalculated whenever the size of the host has been changed.
''' </remarks>
Public Class FormRender
    Implements IDisposable

    ''' <summary>
    ''' Raised after the script expression of a clicked control has been
    ''' invoked successfully.
    ''' </summary>
    Public Event OnClick(sender As Object, e As EventArgs)

    Private ReadOnly host As Control
    Private ReadOnly layout As UiLayoutEngine
    Private ReadOnly binder As MethodBinder
    Private ReadOnly factory As New ControlRendererFactory()
    Private ReadOnly surface As IRenderSurface

    Private hovered As UiBox = Nothing
    Private pressed As UiBox = Nothing
    Private disposedValue As Boolean

    ''' <summary>
    ''' the directx graphics driver is registered only once per process
    ''' </summary>
    Private Shared dxRegistered As Boolean = False

    ''' <summary>
    ''' The layout engine of this user interface.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property UiLayout As UiLayoutEngine
        Get
            Return layout
        End Get
    End Property

    ''' <summary>
    ''' The drawing surface that hosts this user interface.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property RenderSurface As IRenderSurface
        Get
            Return surface
        End Get
    End Property

    ''' <summary>
    ''' The renderer of the html elements of this user interface.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Renderers As ControlRendererFactory
        Get
            Return factory
        End Get
    End Property

    ''' <summary>
    ''' The message of the last binding failure of a click event.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property LastError As String
        Get
            Return binder.LastError
        End Get
    End Property

    ''' <summary>
    ''' Creates the ui engine on the given host control.
    ''' </summary>
    ''' <param name="ui">the html + css declaration of the user interface.</param>
    ''' <param name="container">the host control of the user interface.</param>
    ''' <param name="backend">
    ''' the drawing surface implementation: the default one embeds a directx
    ''' canvas control into the host, while
    ''' <see cref="DxWindowSurface"/> paints on the window handle of the host
    ''' control itself.
    ''' </param>
    Sub New(ui As XElement, container As Control, Optional backend As IRenderSurface = Nothing)
        If ui Is Nothing Then
            Throw New ArgumentNullException(NameOf(ui))
        End If
        If container Is Nothing Then
            Throw New ArgumentNullException(NameOf(container))
        End If

        Call EnsureDirectX()

        host = container
        layout = New UiLayoutEngine(ui)
        binder = New MethodBinder(container)
        surface = If(backend, New DxCanvasSurface(layout.BackgroundColor))

        AddHandler surface.Frame, AddressOf handleFrame
        AddHandler surface.PointerMove, AddressOf handlePointerMove
        AddHandler surface.PointerDown, AddressOf handlePointerDown
        AddHandler surface.PointerUp, AddressOf handlePointerUp

        ' the title of the root element of the declaration is applied to the
        ' text of the host window
        If Not String.IsNullOrEmpty(layout.Title) Then
            container.Text = layout.Title
        End If

        Call surface.Attach(container)
    End Sub

    ''' <summary>
    ''' Registers the directx graphics device as the backend of the drawing
    ''' primitives and of the text metrics of the layout engine.
    ''' </summary>
    Private Shared Sub EnsureDirectX()
        If dxRegistered Then
            Return
        End If

        dxRegistered = True

        Try
            Call Dx2DDriver.RegisterDx2D()
        Catch ex As Exception
            Call Console.WriteLine("[lychee] the directx driver can not be registered: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Draws one frame of the user interface.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub handleFrame(sender As Object, e As RenderFrameEventArgs)
        Dim g As IGraphics = e.Graphics
        Dim viewport As Size = e.Size

        If viewport.Width <= 0 OrElse viewport.Height <= 0 Then
            Return
        End If

        Try
            Call g.Clear(layout.BackgroundColor)

            Dim boxes As List(Of UiBox) = layout.Relayout(viewport, g)

            For Each box As UiBox In boxes
                If box.Bounds.Width <= 0 OrElse box.Bounds.Height <= 0 Then
                    Continue For
                End If

                Try
                    Call factory.GetRenderer(box.Tag).Render(g, box)
                Catch ex As Exception
                    ' a broken control must not break the whole frame
                    Call Console.WriteLine($"[lychee] render <{box.Tag}> error: {ex.Message}")
                End Try
            Next
        Catch ex As Exception
            Call Console.WriteLine("[lychee] layout error: " & ex.Message)
        End Try
    End Sub

    Private Sub handlePointerMove(sender As Object, e As PointerEventArgs)
        Dim hit As UiBox = layout.HitTest(e.X, e.Y)

        If hit Is hovered Then
            Return
        End If

        If hovered IsNot Nothing Then
            hovered.Hover = False
        End If

        hovered = hit

        If hovered IsNot Nothing Then
            hovered.Hover = True
        End If

        Call surface.Invalidate()
    End Sub

    Private Sub handlePointerDown(sender As Object, e As PointerEventArgs)
        If e.Button <> MouseButtons.Left Then
            Return
        End If

        pressed = layout.HitTest(e.X, e.Y)

        If pressed IsNot Nothing Then
            pressed.Pressed = True
            Call surface.Invalidate()
        End If
    End Sub

    Private Sub handlePointerUp(sender As Object, e As PointerEventArgs)
        Dim release As UiBox = pressed

        pressed = Nothing

        If release Is Nothing Then
            Return
        End If

        release.Pressed = False

        ' a click is only raised when the mouse is released on the very same
        ' control that has been pressed down
        If layout.HitTest(e.X, e.Y) IsNot release Then
            Call surface.Invalidate()
            Return
        End If

        Call surface.Invalidate()
        Call RaiseClick(release)
    End Sub

    ''' <summary>
    ''' Runs the script expression of the ``onclick`` attribute of the given
    ''' control.
    ''' </summary>
    ''' <param name="box"></param>
    ''' <returns>
    ''' true when the host declares the method of the expression and the call
    ''' has been invoked.
    ''' </returns>
    Public Function RaiseClick(box As UiBox) As Boolean
        If box Is Nothing OrElse String.IsNullOrEmpty(box.OnClick) Then
            Return False
        End If

        If Not binder.Invoke(box.OnClick) Then
            Call Console.WriteLine($"[lychee] {binder.LastError}")
            Return False
        End If

        RaiseEvent OnClick(box, EventArgs.Empty)

        Return True
    End Function

    ''' <summary>
    ''' Finds the control that is located at the given point of the canvas.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns>
    ''' the topmost interactive control that covers the point, or nothing when
    ''' the point is not on a control.
    ''' </returns>
    Public Function HitTest(x As Integer, y As Integer) As UiBox
        Return layout.HitTest(x, y)
    End Function

    ''' <summary>
    ''' The controls of the user interface, sorted by their paint order.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Controls As IReadOnlyList(Of UiBox)
        Get
            Return layout.TopLevel
        End Get
    End Property

    ''' <summary>
    ''' Simulates a mouse click on the control that is located at the given
    ''' point of the canvas: it is used by the automated smoke test.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    Public Function SimulateClick(x As Integer, y As Integer) As Boolean
        Dim hit As UiBox = layout.HitTest(x, y)

        Return RaiseClick(hit)
    End Function

    ''' <summary>
    ''' Requests a new frame of the user interface.
    ''' </summary>
    Public Sub Invalidate()
        Call surface.Invalidate()
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                RemoveHandler surface.Frame, AddressOf handleFrame
                RemoveHandler surface.PointerMove, AddressOf handlePointerMove
                RemoveHandler surface.PointerDown, AddressOf handlePointerDown
                RemoveHandler surface.PointerUp, AddressOf handlePointerUp

                Call surface.Dispose()
            End If

            disposedValue = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Call Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class
