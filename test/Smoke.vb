Imports System.Drawing
Imports System.Windows.Forms
Imports System.Xml.Linq
Imports LycheeUI
Imports LycheeUI.Layout
Imports LycheeUI.Render
Imports Microsoft.VisualBasic.Drawing.DirectX
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

''' <summary>
''' The unattended smoke test of the ui engine: it builds a host window that
''' declares its user interface with the xml literal syntax, waits for the very
''' first directx frame, asserts the layout of the declared controls and then
''' simulates a mouse click on each of them.
''' </summary>
''' <remarks>
''' The host window of this test writes the click results into a list instead of
''' showing a message box, so the whole test can be run without any user
''' interaction:
'''
''' ```
''' test.exe --smoke
''' ```
'''
''' the exit code of the process is 0 when every assertion has passed.
''' </remarks>
Module Smoke

    ''' <summary>
    ''' The host window of the smoke test.
    ''' </summary>
    Public Class SmokeHost : Inherits Form

        ''' <summary>
        ''' the results of the click events that have been raised
        ''' </summary>
        Public ReadOnly Clicks As New List(Of String)

        ReadOnly UI As XElement =
            <form style="background-color: gray;" title="lychee smoke">
                <button id="hello" style="text-align:center; left:50%;top: 50%; width: 200px;height: 60px; color: blue; background-color: red" onclick="clickButton()">hello</button>
                <button id="hello2" style="text-align:center; right:0;bottom: 0; width: 200px;height: 60px; color: blue; background-color: yellow" onclick="click2('aa+bb+cc')">hello</button>
            </form>

        Dim WithEvents renderer As FormRender

        Sub New()
            Me.Text = "lychee smoke"
            Me.ClientSize = New Size(800, 450)

            renderer = New FormRender(UI, Me)
        End Sub

        ''' <summary>
        ''' The ui engine of this host window.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Engine As FormRender
            Get
                Return renderer
            End Get
        End Property

        ' both of the click handlers are private on purpose: the engine must
        ' find them with a non public reflection lookup
        Private Sub clickButton()
            Clicks.Add("clickButton")
        End Sub

        Private Sub click2(text As String)
            Clicks.Add("click2:" & text)
        End Sub
    End Class

    ''' <summary>
    ''' Runs the smoke test.
    ''' </summary>
    ''' <returns>0 when every assertion has passed, 1 otherwise.</returns>
    Public Function Run() As Integer
        Dim host As New SmokeHost()
        Dim errors As New List(Of String)

        Call host.Show()

        ' the layout of the user interface is calculated inside of the very
        ' first rendered frame, so the message loop has to be pumped before any
        ' assertion can be made
        For i As Integer = 0 To 20
            Call Application.DoEvents()
            Call Threading.Thread.Sleep(20)
        Next

        Dim boxes As IReadOnlyList(Of UiBox) = host.Engine.UiLayout.Boxes
        Dim buttons As New List(Of UiBox)

        For Each box As UiBox In boxes
            If box.Tag = "button" Then
                buttons.Add(box)
            End If
        Next

        If buttons.Count <> 2 Then
            errors.Add($"expected 2 buttons on the top level, but {buttons.Count} was found.")
        End If

        If buttons.Count >= 1 Then
            Call expectBox(errors, "button[0]", buttons(0), 400, 225, 200, 60)
        End If

        If buttons.Count >= 2 Then
            Call expectBox(errors, "button[1]", buttons(1), 600, 390, 200, 60)
        End If

        ' the click events are raised through the onclick expression of the
        ' element, which is resolved with a reflection lookup on the host window
        If buttons.Count >= 1 Then
            Dim b As RectangleF = buttons(0).Bounds

            If Not host.Engine.SimulateClick(CInt(b.Left + b.Width / 2), CInt(b.Top + b.Height / 2)) Then
                errors.Add("the click of button[0] has not been resolved: " & host.Engine.LastError)
            End If
        End If

        If buttons.Count >= 2 Then
            Dim b As RectangleF = buttons(1).Bounds

            If Not host.Engine.SimulateClick(CInt(b.Left + b.Width / 2), CInt(b.Top + b.Height / 2)) Then
                errors.Add("the click of button[1] has not been resolved: " & host.Engine.LastError)
            End If
        End If

        If Not host.Clicks.Contains("clickButton") Then
            errors.Add("the host method 'clickButton' has not been invoked.")
        End If

        If Not host.Clicks.Contains("click2:aa+bb+cc") Then
            errors.Add("the host method 'click2' has not been invoked with the argument 'aa+bb+cc'.")
        End If

        Try
            Dim surface = TryCast(host.Engine.RenderSurface, DxCanvasSurface)

            If surface IsNot Nothing Then
                Call surface.CanvasControl.Invalidate()

                For i As Integer = 0 To 10
                    Call Application.DoEvents()
                    Call Threading.Thread.Sleep(20)
                Next

                Call surface.CanvasControl.SaveImage("./lychee-smoke.png", ImageFormats.Png)
                Console.WriteLine("[smoke] the frame has been saved to ./lychee-smoke.png")
            End If
        Catch ex As Exception
            errors.Add("the frame can not be saved: " & ex.Message)
        End Try

        For Each box As UiBox In boxes
            Console.WriteLine($"[smoke] {box}")
        Next

        For Each line As String In host.Clicks
            Console.WriteLine($"[smoke] click -> {line}")
        Next

        For Each err As String In errors
            Console.WriteLine("[smoke] FAILED: " & err)
        Next

        Call host.Close()

        If errors.Count > 0 Then
            Console.WriteLine($"[smoke] {errors.Count} assertion(s) failed.")
            Return 1
        End If

        Console.WriteLine("[smoke] every assertion has passed.")
        Return 0
    End Function

    Private Sub expectBox(errors As List(Of String), name As String, box As UiBox, x As Single, y As Single, w As Single, h As Single)
        Dim b As RectangleF = box.Bounds

        If std.Abs(b.Left - x) > 1 OrElse std.Abs(b.Top - y) > 1 Then
            errors.Add($"{name} is located at [{b.Left},{b.Top}], but [{x},{y}] was expected.")
        End If

        If std.Abs(b.Width - w) > 1 OrElse std.Abs(b.Height - h) > 1 Then
            errors.Add($"{name} is sized {b.Width}x{b.Height}, but {w}x{h} was expected.")
        End If
    End Sub
End Module
