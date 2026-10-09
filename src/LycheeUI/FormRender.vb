Imports System.Drawing
Imports System.Diagnostics
Imports System.Windows.Forms
Imports System.Xml.Linq
Imports Microsoft.VisualBasic.Drawing.DirectX
Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Controls
Imports LycheeUI.Events
Imports LycheeUI.Layout
Imports LycheeUI.Render
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports std = System.Math

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
Public Class FormRender : Implements IDisposable

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
    ''' the graphics drivers are registered only once per process
    ''' </summary>
    Private Shared driversReady As Boolean = False

    ''' <summary>
    ''' the control that holds the keyboard focus of the canvas
    ''' </summary>
    Private focused As UiBox = Nothing

    ''' <summary>
    ''' toggles the visibility of the caret of the focused text input control
    ''' </summary>
    Private caretTimer As Timer
    Private caretVisible As Boolean = True

    ''' <summary>
    ''' delays the tooltip until the mouse stops moving over a control
    ''' </summary>
    Private tooltipTimer As Timer
    Private tooltipShown As Boolean = False
    Private tooltipContent As String = Nothing
    Private tooltipPoint As Point
    Private pointer As Point

    ''' <summary>
    ''' Should a ``href`` that points to a web address be opened by the default
    ''' browser? a host application that does not want to leave the canvas can
    ''' turn this off, the click is then passed to the host method instead.
    ''' </summary>
    ''' <returns></returns>
    Public Property OpenLinks As Boolean = True

    ''' <summary>
    ''' The number of the milliseconds that the mouse has to rest on a control
    ''' before its tooltip is shown.
    ''' </summary>
    ''' <returns></returns>
    Public Property TooltipDelay As Integer
        Get
            Return tooltipTimer.Interval
        End Get
        Set
            tooltipTimer.Interval = std.Max(0, Value)
        End Set
    End Property

    ''' <summary>
    ''' Is the tooltip of a control visible right now?
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property TooltipVisible As Boolean
        Get
            Return tooltipShown
        End Get
    End Property

    ''' <summary>
    ''' The rich text of the tooltip that is currently visible.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property TooltipText As String
        Get
            Return tooltipContent
        End Get
    End Property

    ''' <summary>
    ''' The position where the tooltip panel is drawn.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property TooltipLocation As Point
        Get
            Return tooltipPoint
        End Get
    End Property

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
    ''' <param name="theme">
    ''' default nothing means use the <see cref="Theme.DefaultTheme()"/>.
    ''' </param>
    Sub New(ui As XElement, container As Control, Optional backend As IRenderSurface = Nothing, Optional theme As Theme = Nothing)
        If ui Is Nothing Then
            Throw New ArgumentNullException(NameOf(ui))
        ElseIf container Is Nothing Then
            Throw New ArgumentNullException(NameOf(container))
        Else
            theme = If(theme, Theme.DefaultTheme)
        End If

        Call EnsureDrivers()

        host = container
        layout = New UiLayoutEngine(ui)
        binder = New MethodBinder(container)
        surface = If(backend, New DxCanvasSurface(layout.BackgroundColor))

        caretTimer = New Timer With {.Interval = 500}
        AddHandler caretTimer.Tick, AddressOf handleCaretTick

        tooltipTimer = New Timer With {.Interval = 600}
        AddHandler tooltipTimer.Tick, AddressOf handleTooltipTick

        AddHandler surface.Frame, AddressOf handleFrame
        AddHandler surface.PointerMove, AddressOf handlePointerMove
        AddHandler surface.PointerDown, AddressOf handlePointerDown
        AddHandler surface.PointerUp, AddressOf handlePointerUp
        AddHandler surface.KeyDown, AddressOf handleKeyDown
        AddHandler surface.TextInput, AddressOf handleTextInput

        ' the title of the root element of the declaration is applied to the
        ' text of the host window
        If Not String.IsNullOrEmpty(layout.Title) Then
            container.Text = layout.Title
        End If

        Call surface.Attach(container)
    End Sub

    ''' <summary>
    ''' Registers the graphics device and the raster image decoder of the
    ''' drawing primitives.
    ''' </summary>
    ''' <remarks>
    ''' The order of the two registrations matters: both of them claim the
    ''' ``GDI`` slot of the driver loader, so the gdi+ raster image decoder has
    ''' to be registered first and the directx device driver overwrites it
    ''' afterwards, otherwise the gdi+ canvas would replace the directx one.
    ''' </remarks>
    Private Shared Sub EnsureDrivers()
        If driversReady Then
            Return
        End If

        driversReady = True

        Try
            Call Microsoft.VisualBasic.Imaging.Driver.ImageDriver.Register()
        Catch ex As Exception
            Call Console.WriteLine("[lychee] the raster image driver can not be registered: " & ex.Message)
        End Try

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
                    Call factory.GetRenderer(box).Render(g, box)
                Catch ex As Exception
                    ' a broken control must not break the whole frame
                    Call Console.WriteLine($"[lychee] render <{box.Tag}> error: {ex.ToString()}")
                End Try
            Next

            ' the tooltip is painted on the top of every control of the user
            ' interface, it is not a box of the layout and is never hit tested
            If tooltipShown AndAlso Not String.IsNullOrEmpty(tooltipContent) Then
                Try
                    Call TooltipRenderer.Render(g, tooltipContent,
                                                If(hovered?.Font, New Font(FontFace.SegoeUI, 12)),
                                                tooltipPoint, viewport)
                Catch ex As Exception
                    Call Console.WriteLine("[lychee] tooltip error: " & ex.Message)
                End Try
            End If
        Catch ex As Exception
            Call Console.WriteLine("[lychee] layout error: " & ex.Message)
        End Try
    End Sub

    Private Sub handlePointerMove(sender As Object, e As PointerEventArgs)
        Dim hit As UiBox = layout.HitTest(e.X, e.Y)

        ' the tooltip follows the mouse, so its position has to be tracked even
        ' when the hovered control does not change at all
        pointer = New Point(e.X, e.Y)

        If hit IsNot hovered Then
            If hovered IsNot Nothing Then
                hovered.Hover = False
            End If

            hovered = hit

            If hovered IsNot Nothing Then
                hovered.Hover = True
            End If

            Call ResetTooltip()
        End If

        ' the tooltip is only shown when the mouse stops moving over a control
        ' that declares a tooltip
        If hovered IsNot Nothing AndAlso Not String.IsNullOrEmpty(hovered.Tooltip) Then
            If Not tooltipTimer.Enabled Then
                Call tooltipTimer.Start()
            End If
        ElseIf tooltipTimer.Enabled Then
            Call tooltipTimer.Stop()
            Call HideTooltip()
        End If

        Call surface.Invalidate()
    End Sub

    Private Sub handlePointerDown(sender As Object, e As PointerEventArgs)
        If e.Button <> MouseButtons.Left Then
            Return
        End If

        pressed = layout.HitTest(e.X, e.Y)

        ' only one control can hold the keyboard focus, a click on the empty
        ' area of the canvas releases it
        Call SetFocus(If(pressed IsNot Nothing AndAlso pressed.IsTextInput, pressed, Nothing))

        If pressed IsNot Nothing Then
            pressed.Pressed = True

            If pressed.IsTextInput Then
                pressed.Caret = CaretFromPoint(pressed, e.X)
                pressed.SelectionLength = 0
            End If

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

        If release.IsCheckable Then
            Call ToggleChecked(release)
        End If

        Call surface.Invalidate()
        Call RaiseClick(release)
    End Sub

    ''' <summary>
    ''' Moves the keyboard focus to the given control.
    ''' </summary>
    ''' <param name="box">
    ''' nothing releases the focus of the current control.
    ''' </param>
    Public Sub SetFocus(box As UiBox)
        If focused Is box Then
            Return
        End If

        If focused IsNot Nothing Then
            focused.Focused = False
        End If

        focused = box

        If focused IsNot Nothing Then
            focused.Focused = True
            caretVisible = True

            If Not caretTimer.Enabled Then
                Call caretTimer.Start()
            End If
        ElseIf caretTimer.Enabled Then
            ' an idle canvas must not be repainted again and again just for a
            ' caret that is not visible at all
            Call caretTimer.Stop()
        End If
    End Sub

    Private Sub handleTooltipTick(sender As Object, e As EventArgs)
        Try
            Call tooltipTimer.Stop()

            If hovered Is Nothing OrElse String.IsNullOrEmpty(hovered.Tooltip) Then
                Return
            End If

            ' the tooltip is shown a little bit below of the mouse pointer
            tooltipPoint = New Point(pointer.X + 12, pointer.Y + 16)
            tooltipContent = hovered.Tooltip
            tooltipShown = True

            Call surface.Invalidate()
        Catch ex As Exception
            ' the host window may have been closed while this timer is still
            ' running, an idle timer must not crash the application
            Call tooltipTimer.Stop()
        End Try
    End Sub

    ''' <summary>
    ''' Hides the tooltip and stops its timer.
    ''' </summary>
    Public Sub HideTooltip()
        tooltipShown = False
        tooltipContent = Nothing

        If tooltipTimer.Enabled Then
            Call tooltipTimer.Stop()
        End If

        Call surface.Invalidate()
    End Sub

    Private Sub ResetTooltip()
        tooltipShown = False
        tooltipContent = Nothing

        If tooltipTimer.Enabled Then
            Call tooltipTimer.Stop()
        End If
    End Sub

    ''' <summary>
    ''' Simulates a mouse move over the canvas: it is used by the automated
    ''' smoke test to place the mouse pointer on a control.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    Public Sub SimulateHover(x As Integer, y As Integer)
        Call handlePointerMove(Me, New PointerEventArgs(x, y))
    End Sub

    ''' <summary>
    ''' Shows the tooltip of the control that is currently hovered, without
    ''' waiting for the delay: it is used by the automated smoke test.
    ''' </summary>
    Public Sub ShowTooltipNow()
        Call handleTooltipTick(Me, EventArgs.Empty)
    End Sub

    Private Sub handleCaretTick(sender As Object, e As EventArgs)
        Try
            caretVisible = Not caretVisible
            factory.TextInput.CaretVisible = caretVisible

            Call surface.Invalidate()
        Catch ex As Exception
            ' the host window may have been closed while this timer is still
            ' running, an idle timer must not crash the application
            Call caretTimer.Stop()
        End Try
    End Sub

    ''' <summary>
    ''' Flips the checked state of a checkbox or of a radio button.
    ''' </summary>
    ''' <param name="box"></param>
    ''' <remarks>
    ''' The radio buttons that share the same ``name`` attribute are mutually
    ''' exclusive, so every other radio button of the same group is unchecked
    ''' when one of them has been selected.
    ''' </remarks>
    Public Sub ToggleChecked(box As UiBox)
        If box Is Nothing OrElse Not box.IsCheckable Then
            Return
        End If

        If box.InputType = "radio" Then
            box.Checked = True

            Dim group As String = box.GroupName

            If String.IsNullOrEmpty(group) Then
                group = Nothing
            End If

            For Each other As UiBox In layout.Boxes
                If other Is box OrElse Not other.IsCheckable Then
                    Continue For
                End If
                If other.InputType <> "radio" Then
                    Continue For
                End If

                Dim sameGroup As Boolean

                If group Is Nothing Then
                    sameGroup = String.IsNullOrEmpty(other.GroupName)
                Else
                    sameGroup = (other.GroupName = group)
                End If

                If sameGroup Then
                    other.Checked = False
                End If
            Next
        Else
            box.Checked = Not box.Checked
        End If

        ' the change event is raised after the state has been settled
        If Not String.IsNullOrEmpty(box.ChangeScript) Then
            If Not binder.Invoke(box.ChangeScript) Then
                Call Console.WriteLine($"[lychee] {binder.LastError}")
            Else
                RaiseEvent OnClick(box, EventArgs.Empty)
            End If
        End If
    End Sub

    Private Sub handleKeyDown(sender As Object, e As CanvasKeyEventArgs)
        Dim target As UiBox = focused

        If target Is Nothing OrElse Not target.IsTextInput Then
            Return
        End If

        Dim text As String = If(target.Value, "")
        Dim caret As Integer = std.Min(std.Max(target.Caret, 0), text.Length)

        Select Case e.KeyCode
            Case Keys.Left
                target.Caret = std.Max(0, caret - 1)
                target.SelectionLength = 0
            Case Keys.Right
                target.Caret = std.Min(text.Length, caret + 1)
                target.SelectionLength = 0
            Case Keys.Home
                target.Caret = 0
                target.SelectionLength = 0
            Case Keys.End
                target.Caret = text.Length
                target.SelectionLength = 0
            Case Keys.Back
                If caret > 0 Then
                    target.Value = text.Remove(caret - 1, 1)
                    target.Caret = caret - 1
                End If

                target.SelectionLength = 0
            Case Keys.Delete
                If caret < text.Length Then
                    target.Value = text.Remove(caret, 1)
                    target.Caret = caret
                End If

                target.SelectionLength = 0
            Case Keys.V
                If e.Control Then
                    Call InsertText(target, Clipboard.GetText())
                    e.Handled = True
                    Return
                Else
                    Return
                End If
            Case Keys.A
                If e.Control Then
                    target.SelectionStart = 0
                    target.SelectionLength = If(target.Value, "").Length
                    e.Handled = True
                End If
            Case Keys.Tab, Keys.Enter, Keys.Escape
                ' the focus is released so that the host window may use these
                ' keys for its own navigation
                Call SetFocus(Nothing)
                Return
            Case Else
                Return
        End Select

        e.Handled = True
        caretVisible = True
        factory.TextInput.CaretVisible = True

        Call surface.Invalidate()
    End Sub

    Private Sub handleTextInput(sender As Object, e As CanvasTextEventArgs)
        Dim target As UiBox = focused

        If target Is Nothing OrElse Not target.IsTextInput Then
            Return
        End If

        ' the control characters are editing keys, they are handled by the key
        ' down event instead of being appended to the text
        If Char.IsControl(e.Character) Then
            Return
        End If

        Call InsertText(target, e.Character.ToString())

        e.Handled = True
    End Sub

    ''' <summary>
    ''' Inserts the given text at the caret of a text input control.
    ''' </summary>
    ''' <param name="box"></param>
    ''' <param name="text"></param>
    Public Sub InsertText(box As UiBox, text As String)
        If box Is Nothing OrElse Not box.IsTextInput OrElse String.IsNullOrEmpty(text) Then
            Return
        End If

        Dim source As String = If(box.Value, "")
        Dim caret As Integer = std.Min(std.Max(box.Caret, 0), source.Length)

        ' an existing selection is replaced by the inserted text
        If box.SelectionLength > 0 Then
            Dim start As Integer = std.Min(std.Max(box.SelectionStart, 0), source.Length)
            Dim length As Integer = std.Min(box.SelectionLength, source.Length - start)

            source = source.Remove(start, length)
            caret = start
            box.SelectionLength = 0
        End If

        text = text.Replace(vbCr, "").Replace(vbLf, "")

        box.Value = source.Insert(caret, text)
        box.Caret = caret + text.Length
        caretVisible = True
        factory.TextInput.CaretVisible = True

        Call surface.Invalidate()
    End Sub

    ''' <summary>
    ''' Finds the caret offset that is the nearest one to the given horizontal
    ''' position of a text input control.
    ''' </summary>
    ''' <param name="box"></param>
    ''' <param name="x">the horizontal position inside of the canvas.</param>
    ''' <returns></returns>
    Public Function CaretFromPoint(box As UiBox, x As Integer) As Integer
        If box Is Nothing Then
            Return 0
        End If

        Dim text As String = If(box.Value, "")

        If text.Length = 0 Then
            Return 0
        End If

        Dim g As IGraphics = Nothing

        Try
            If TypeOf surface Is DxCanvasSurface Then
                g = DirectCast(surface, DxCanvasSurface).CanvasControl.Graphics
            End If
        Catch ex As Exception
            g = Nothing
        End Try

        If g Is Nothing Then
            Return text.Length
        End If

        Dim font As Font = box.Font
        Dim start As Single = box.ContentBounds.Left - x
        Dim best As Integer = 0
        Dim bestDelta As Single = Single.MaxValue

        For i As Integer = 0 To text.Length
            Dim delta As Single = std.Abs(g.MeasureString(text.Substring(0, i), font).Width + start)

            If delta < bestDelta Then
                bestDelta = delta
                best = i
            End If
        Next

        Return best
    End Function

    ''' <summary>
    ''' Runs the action of the given control: the ``href`` of a hyperlink or
    ''' the script expression of its ``onclick`` attribute.
    ''' </summary>
    ''' <param name="box"></param>
    ''' <returns>
    ''' true when the host declares the method of the expression and the call
    ''' has been invoked.
    ''' </returns>
    Public Function RaiseClick(box As UiBox) As Boolean
        If box Is Nothing Then
            Return False
        End If

        Call HideTooltip()

        ' a hyperlink may point to a web address or to a host method
        If box.IsLink AndAlso Not String.IsNullOrEmpty(box.Href) Then
            Dim href As String = box.Href.Trim()

            If href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) Then
                href = href.Substring("javascript:".Length).Trim()
            End If

            If isOpenUrl(href) Then
                If Not OpenLinks Then
                    Call Console.WriteLine($"[lychee] the link '{href}' is not opened, OpenLinks is off.")
                    RaiseEvent OnClick(box, EventArgs.Empty)
                    Return False
                End If

                If OpenBrowser(href) Then
                    RaiseEvent OnClick(box, EventArgs.Empty)
                    Return True
                End If

                Return False
            End If

            If href.Length > 0 Then
                Return InvokeScript(box, href)
            End If
        End If

        If String.IsNullOrEmpty(box.OnClick) Then
            Return False
        End If

        Return InvokeScript(box, box.OnClick)
    End Function

    Private Shared Function isOpenUrl(href As String) As Boolean
        Return href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) OrElse
            href.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' Opens the given web address in the default browser of the system.
    ''' </summary>
    ''' <param name="url"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' ``Process.Start(url)`` fails on .net core because the shell execute is
    ''' turned off by default there, so the flag has to be set explicitly.
    ''' </remarks>
    Private Shared Function OpenBrowser(url As String) As Boolean
        Try
            Call Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})
            Return True
        Catch ex As Exception
            Call Console.WriteLine($"[lychee] the link '{url}' can not be opened: {ex.Message}")
            Return False
        End Try
    End Function

    Private Function InvokeScript(box As UiBox, expression As String) As Boolean
        If Not binder.Invoke(expression) Then
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

        If hit Is Nothing Then
            Return False
        End If

        If hit.IsCheckable Then
            Call ToggleChecked(hit)
            Call surface.Invalidate()
        End If

        Return RaiseClick(hit)
    End Function

    ''' <summary>
    ''' Finds the control with the given ``id`` attribute.
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Public Function FindById(id As String) As UiBox
        Return layout.FindById(id)
    End Function

    ''' <summary>
    ''' Returns the user-interface element whose ``id`` attribute equals the given
    ''' value, or nothing when no such element exists. Mirrors the DOM
    ''' ``document.getElementById`` lookup and exposes the box so callers can read
    ''' its <see cref="UiBox.Value"/>, attributes, etc.
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Public Function GetElementById(id As String) As UiBox
        Return layout.FindById(id)
    End Function

    ''' <summary>
    ''' Reads the value of the control with the given ``id``: the text of a
    ''' text input control, or the "True"/"False" literal of the checked state
    ''' of a checkbox and of a radio button.
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Public Function GetValue(id As String) As String
        Dim box As UiBox = layout.FindById(id)

        If box Is Nothing Then
            Return Nothing
        End If
        If box.IsCheckable Then
            Return If(box.Checked, "True", "False")
        End If

        Return If(box.Value, "")
    End Function

    ''' <summary>
    ''' Sets the value of the control with the given ``id``.
    ''' </summary>
    ''' <param name="id"></param>
    ''' <param name="value">
    ''' a "True"/"False" literal is understood as the checked state of a
    ''' checkbox and of a radio button.
    ''' </param>
    Public Sub SetValue(id As String, value As String)
        Dim box As UiBox = layout.FindById(id)

        If box Is Nothing Then
            Return
        End If

        If box.IsCheckable Then
            Call SetChecked(id, value = "True" OrElse value = "true" OrElse value = "1")
        ElseIf box.IsTextInput Then
            box.Value = If(value, "")
            box.Caret = box.Value.Length
            box.SelectionLength = 0

            Call surface.Invalidate()
        End If
    End Sub

    ''' <summary>
    ''' Reads the checked state of the checkbox or of the radio button with the
    ''' given ``id``.
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Public Function GetChecked(id As String) As Boolean
        Dim box As UiBox = layout.FindById(id)

        Return box IsNot Nothing AndAlso box.Checked
    End Function

    ''' <summary>
    ''' Sets the checked state of the checkbox or of the radio button with the
    ''' given ``id``.
    ''' </summary>
    ''' <param name="id"></param>
    ''' <param name="checked"></param>
    Public Sub SetChecked(id As String, checked As Boolean)
        Dim box As UiBox = layout.FindById(id)

        If box Is Nothing OrElse Not box.IsCheckable Then
            Return
        End If

        If box.Checked = checked Then
            Return
        End If

        If checked AndAlso box.InputType = "radio" Then
            Call ToggleChecked(box)
        Else
            box.Checked = checked
        End If

        Call surface.Invalidate()
    End Sub

    ''' <summary>
    ''' The ``id`` of the control that holds the keyboard focus, nothing when
    ''' no control is focused.
    ''' </summary>
    ''' <returns></returns>
    Public Function GetFocusedId() As String
        If focused Is Nothing Then
            Return Nothing
        End If

        Return focused.Attribute("id")
    End Function

    ''' <summary>
    ''' Types the given text into the control that holds the keyboard focus: it
    ''' is used by the automated smoke test.
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns>true when a text input control has been focused.</returns>
    Public Function SimulateType(text As String) As Boolean
        If focused Is Nothing OrElse Not focused.IsTextInput Then
            Return False
        End If

        For Each c As Char In If(text, "")
            If Char.IsControl(c) Then
                Continue For
            End If

            Call InsertText(focused, c.ToString())
        Next

        Return True
    End Function

    ''' <summary>
    ''' Moves the keyboard focus to the control with the given ``id``.
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns>true when such a control exists.</returns>
    Public Function FocusById(id As String) As Boolean
        Dim box As UiBox = layout.FindById(id)

        If box Is Nothing Then
            Return False
        End If

        Call SetFocus(box)
        Call surface.Invalidate()

        Return True
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
                RemoveHandler surface.KeyDown, AddressOf handleKeyDown
                RemoveHandler surface.TextInput, AddressOf handleTextInput

                If caretTimer IsNot Nothing Then
                    RemoveHandler caretTimer.Tick, AddressOf handleCaretTick
                    Call caretTimer.Stop()
                    Call caretTimer.Dispose()
                    caretTimer = Nothing
                End If

                If tooltipTimer IsNot Nothing Then
                    RemoveHandler tooltipTimer.Tick, AddressOf handleTooltipTick
                    Call tooltipTimer.Stop()
                    Call tooltipTimer.Dispose()
                    tooltipTimer = Nothing
                End If

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
