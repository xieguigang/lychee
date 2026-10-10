Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports System.Xml.Linq
Imports LycheeUI
Imports LycheeUI.Layout
Imports LycheeUI.Render
Imports LycheeUI.Tabs
Imports Microsoft.VisualBasic.Drawing.DirectX
Imports Microsoft.VisualBasic.Imaging
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports std = System.Math

''' <summary>
''' The unattended smoke test of the ui engine: it builds a host window that
''' declares its user interface with the xml literal syntax, waits for the very
''' first directx frame, asserts the layout of the declared controls and then
''' simulates a mouse click on each of them. the main window of this
''' application (<see cref="Form1"/>) is rendered as well, so that a
''' regression of the real test case is caught too.
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
    ''' the rich text of the tooltip that is declared on the hyperlink
    ''' </summary>
    Public Const TOOLTIP_TEXT As String = "<b>docs</b><br/><font color='gray'>user guide</font>"


    ''' <summary>
    ''' The host window of the smoke test: its user interface declaration
    ''' covers the whole feature set of the engine.
    ''' </summary>
    Public Class SmokeHost : Inherits Form

        ''' <summary>
        ''' the results of the click events that have been raised
        ''' </summary>
        Public ReadOnly Clicks As New List(Of String)

        ReadOnly UI As XElement =
            <form style="background-color: gray;" title="lychee smoke">
                <!-- the container element: background, border shorthand, rounded corners and padding -->
                <div id="box" style="left:16px;top:16px;width:320px;height:180px;
                                     background-color:lightblue;
                                     border:2px solid navy;
                                     border-radius:14px;
                                     padding:10px;
                                     box-shadow: 4px 4px 8px rgba(0, 0, 0, 0.45)">
                    <label id="lbl" style="display:block;left:10px;top:12px;width:280px;height:24px;
                                          color:darkblue;font-size:15px;text-align:center">nested label</label>
                    <button id="inbox" style="left:10px;top:80px;width:160px;height:38px;
                                              background-color:seagreen;color:white;
                                              border-radius:6px;text-align:center"
                            onclick="nested('from div')">in div</button>
                </div>
                <!-- z-index: the second box overlaps the first one and is painted on top of it -->
                <div id="z1" style="left:370px;top:16px;width:120px;height:110px;
                                    background-color:forestgreen;color:white;
                                    text-align:center;z-index:1">z=1</div>
                <div id="z2" style="left:430px;top:56px;width:120px;height:110px;
                                    background-color:darkorange;color:white;
                                    text-align:center;z-index:2">z=2</div>
                <!-- a long paragraph verifies the word wrapping, the span inside of it is an inline element -->
                <p id="para" style="left:16px;top:306px;width:320px;height:60px;
                                    font-size:12px;color:#333333">
                    lychee renders a html and css user interface declaration with
                    the <span id="word" style="font-weight:bold;color:crimson">directx</span>
                    api onto any windows forms control, this paragraph is long
                    enough to verify the automatic word wrapping of the layout engine.
                </p>
                <!-- the original two buttons of the test case -->
                <button id="hello" style="text-align:center; left:50%;top: 50%; width: 200px;height: 60px; color: blue; background-color: red" onclick="clickButton()">hello</button>
                <button id="hello2" style="text-align:center; right:0;bottom: 0; width: 200px;height: 60px; color: blue; background-color: yellow" onclick="click2('aa+bb+cc')">hello</button>
                <!-- the event binding: a number, two arguments and a literal that contains a comma -->
                <button id="bnum" style="left:210px;top:378px;width:110px;height:30px;
                                         background-color:steelblue;color:white;text-align:center"
                        onclick="onCount(42)">number</button>
                <button id="bargs" style="left:330px;top:378px;width:110px;height:30px;
                                          background-color:mediumpurple;color:white;text-align:center"
                        onclick="onMulti('items', 7)">two args</button>
                <button id="blit" style="left:450px;top:378px;width:130px;height:30px;
                                         background-color:dimgray;color:white;text-align:center"
                        onclick="onSpecial('a, b (c)')">literal</button>
                <!-- the input controls -->
                <input type="text" id="name" style="display:block;left:610px;top:170px;width:180px;height:26px;
                                     background-color:white;border:1px solid gray;color:black"
                       value="lychee" placeholder="user name"/>
                <input type="password" id="pwd" style="display:block;left:610px;top:204px;width:180px;height:26px;
                                       background-color:white;border:1px solid gray;color:black"
                       value="1234"/>
                <input type="radio" id="optA" name="choice" style="display:block;left:610px;top:238px;width:180px;height:24px;
                                      color:black" checked="checked" onchange="onCheck('A', true)" label="option A"/>
                <input type="radio" id="optB" name="choice" style="display:block;left:610px;top:266px;width:180px;height:24px;
                                      color:black" onchange="onCheck('B', true)" label="option B"/>
                <input type="checkbox" id="cb1" style="display:block;left:610px;top:294px;width:180px;height:24px;
                                       color:black" checked="checked" onchange="onToggle('cb1', true)" label="remember me"/>
                <input type="checkbox" id="cb2" style="display:block;left:610px;top:322px;width:180px;height:24px;
                                       color:black" onchange="onToggle('cb2', true)" label="auto start"/>
                <!-- an image with a declared size and an image that uses its natural size -->
                <img id="logo" src="./lychee-smoke-img.png" alt="lychee"
                     style="display:block;left:360px;top:340px;width:80px;height:100px;
                            background-color:#e0e0e0;border:1px solid silver"/>
                <img id="logo2" src="./lychee-smoke-img.png" alt="lychee"
                     style="display:block;left:220px;top:60px"/>
                <!-- a hyperlink with a rich text tooltip -->
                <a id="link" href="openHelp('docs')"
                   style="display:block;left:500px;top:150px;width:120px;height:22px"
                   tooltip="&lt;b&gt;docs&lt;/b&gt;&lt;br/&gt;&lt;font color='gray'&gt;user guide&lt;/font&gt;">documentation</a>
                <!-- a browser like tab control -->
                <tabcontrol id="tabs" style="left:360px;top:295px;width:230px;height:78px">
                    <page id="t1" title="first" favicon="./lychee-smoke-img.png">
                        <label style="display:block;left:10px;top:6px;color:white">page one</label>
                    </page>
                    <page id="t2" title="second">
                        <label style="display:block;left:10px;top:6px;color:white">page two</label>
                    </page>
                </tabcontrol>
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

        ' every click handler is private on purpose: the engine must find them
        ' with a non public reflection lookup
        Private Sub clickButton()
            Clicks.Add("clickButton")
        End Sub

        Private Sub click2(text As String)
            Clicks.Add("click2:" & text)
        End Sub

        Private Sub nested(text As String)
            Clicks.Add("nested:" & text)
        End Sub

        Private Sub onCount(n As Integer)
            Clicks.Add("onCount:" & n)
        End Sub

        Private Sub onMulti(name As String, count As Integer)
            Clicks.Add($"onMulti:{name}/{count}")
        End Sub

        Private Sub onSpecial(text As String)
            Clicks.Add("onSpecial:" & text)
        End Sub

        Private Sub onCheck(letter As String, state As Boolean)
            Clicks.Add($"onCheck:{letter}/{state}")
        End Sub

        Private Sub onToggle(id As String, state As Boolean)
            Clicks.Add($"onToggle:{id}/{state}")
        End Sub

        Private Sub openHelp(topic As String)
            Clicks.Add("openHelp:" & topic)
        End Sub
    End Class

    ''' <summary>
    ''' Runs the smoke test.
    ''' </summary>
    ''' <returns>0 when every assertion has passed, 1 otherwise.</returns>
    ''' <summary>
    ''' Writes the sample image that is referenced by the ``img`` elements of
    ''' the test declaration, so that the repository does not have to carry a
    ''' binary asset.
    ''' </summary>
    ''' <returns>the path of the generated png file.</returns>
    Private Function MakeSampleImage() As String
        Dim path As String = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lychee-smoke-img.png")

        ' a leftover image file of a previous run may still be locked by a
        ' process that has not released it yet, the already existing file is
        ' good enough at that case
        If File.Exists(path) Then
            Try
                Call File.Delete(path)
            Catch ex As IOException
                Return path
            Catch ex As UnauthorizedAccessException
                Return path
            End Try
        End If

        Dim bmp As New Bitmap(64, 64)

        For y As Integer = 0 To 63
            For x As Integer = 0 To 63
                Call bmp.SetPixel(x, y, Color.DarkOrange)
            Next
        Next

        Call bmp.Save(path, ImageFormats.Png)

        ' the main window of this application references its own copy of the
        ' sample image, so both of them can be rendered without any repository
        ' asset
        Call File.Copy(path, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lychee-form1-img.png"), True)

        Return path
    End Function

    Public Function Run() As Integer
        Call MakeSampleImage()

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

        For Each box As UiBox In boxes
            Console.WriteLine($"[smoke] {box}")
        Next

        ' the geometry of the declared controls
        Call expectBox(errors, "hello", findById(boxes, "hello"), 400, 225, 200, 60)
        Call expectBox(errors, "hello2", findById(boxes, "hello2"), 600, 390, 200, 60)
        Call expectBox(errors, "div#box", findById(boxes, "box"), 16, 16, 320, 180)
        ' the nested button is placed against the padding box of its container:
        ' 16 + border 2 + padding 10 = 28, plus the declared offset of the button
        Call expectBox(errors, "button#inbox", findById(boxes, "inbox"), 38, 108, 160, 38)
        Call expectBox(errors, "div#z1", findById(boxes, "z1"), 370, 16, 120, 110)
        Call expectBox(errors, "div#z2", findById(boxes, "z2"), 430, 56, 120, 110)
        Call expectBox(errors, "p#para", findById(boxes, "para"), 16, 306, 320, 60)

        ' the rounded corners of the container element
        Dim box1 As UiBox = findById(boxes, "box")

        If box1 IsNot Nothing Then
            If Not box1.IsRounded Then
                errors.Add("div#box should be painted with rounded corners.")
            End If
            If std.Abs(box1.Radius - 14) > 0.5 Then
                errors.Add($"div#box has a corner radius of {box1.Radius}, but 14 was expected.")
            End If
            ' a container element must not paint the text of its child elements
            If box1.Text.Length > 0 Then
                errors.Add($"div#box should not aggregate the text of its child elements, but '{box1.Text}' was found.")
            End If
        End If

        ' the paint order of the two overlapping boxes
        Dim z1 As Integer = indexOf(boxes, "z1")
        Dim z2 As Integer = indexOf(boxes, "z2")

        If z1 < 0 OrElse z2 < 0 Then
            errors.Add("the two overlapping boxes are missing from the paint order.")
        ElseIf z2 <= z1 Then
            errors.Add("the box with z-index 2 must be painted after the box with z-index 1.")
        End If

        ' an inline element gets its geometry from the line box that hosts it
        Dim span As UiBox = findById(boxes, "word")

        If span Is Nothing Then
            errors.Add("the inline span element is missing.")
        ElseIf span.Bounds.Width <= 0 OrElse span.Bounds.Height <= 0 Then
            errors.Add("the inline span element has an empty paint rectangle.")
        End If

        ' every form of the click expression
        Call expectClick(host, errors, "hello", "clickButton")
        Call expectClick(host, errors, "hello2", "click2:aa+bb+cc")
        Call expectClick(host, errors, "inbox", "nested:from div")
        Call expectClick(host, errors, "bnum", "onCount:42")
        Call expectClick(host, errors, "bargs", "onMulti:items/7")
        Call expectClick(host, errors, "blit", "onSpecial:a, b (c)")

        ' the initial state of the input controls is read out of the declaration
        If host.Engine.GetValue("name") <> "lychee" Then
            errors.Add($"the text box #name should be 'lychee', but '{host.Engine.GetValue("name")}' was found.")
        End If
        If host.Engine.GetValue("pwd") <> "1234" Then
            errors.Add("the password box #pwd should be '1234'.")
        End If
        If Not host.Engine.GetChecked("optA") OrElse host.Engine.GetChecked("optB") Then
            errors.Add("the initial radio state is wrong: optA should be selected and optB should not.")
        End If
        If Not host.Engine.GetChecked("cb1") OrElse host.Engine.GetChecked("cb2") Then
            errors.Add("the initial checkbox state is wrong: #cb1 should be checked and #cb2 should not.")
        End If

        ' the image elements
        Call expectBox(errors, "img#logo", findById(boxes, "logo"), 360, 340, 80, 100)
        ' the natural size of the png file is written back to the undeclared
        ' box, and since it is a top level element its offsets are relative to
        ' the page box
        Call expectBox(errors, "img#logo2", findById(boxes, "logo2"), 220, 60, 64, 64)

        ' the keyboard: the focus is moved to the text box and a text is typed into it
        If Not host.Engine.FocusById("name") Then
            errors.Add("the text box #name can not be focused.")
        ElseIf host.Engine.GetFocusedId() <> "name" Then
            errors.Add($"the focused control should be #name, but #{host.Engine.GetFocusedId()} was focused.")
        ElseIf Not host.Engine.SimulateType("abc") Then
            errors.Add("the text can not be typed into the focused control.")
        ElseIf host.Engine.GetValue("name") <> "lycheeabc" Then
            errors.Add($"the text box should contain 'lycheeabc', but '{host.Engine.GetValue("name")}' was found.")
        End If

        ' the radio buttons of the same group are mutually exclusive
        Call clickCenter(host, "optB")

        If Not host.Engine.GetChecked("optB") Then
            errors.Add("the radio button #optB should be selected after a click on it.")
        End If
        If host.Engine.GetChecked("optA") Then
            errors.Add("the radio button #optA should have been unselected by the click on #optB.")
        End If
        If Not host.Clicks.Contains("onCheck:B/True") Then
            errors.Add("the change of #optB should have raised 'onCheck:B/True'.")
        End If

        ' the check box is toggled by every click
        Call clickCenter(host, "cb2")

        If Not host.Engine.GetChecked("cb2") Then
            errors.Add("the check box #cb2 should be checked after the first click on it.")
        End If

        Call clickCenter(host, "cb2")

        If host.Engine.GetChecked("cb2") Then
            errors.Add("the check box #cb2 should be unchecked after the second click on it.")
        End If

        ' the css box shadow is parsed into a shadow structure
        Dim shadow As CssShadow = findById(boxes, "box").Shadow

        If shadow.IsEmpty Then
            errors.Add("the box shadow of div#box has not been parsed.")
        Else
            If std.Abs(shadow.OffsetX - 4) > 0.01 OrElse std.Abs(shadow.OffsetY - 4) > 0.01 Then
                errors.Add($"the shadow offset should be 4x4, but {shadow.OffsetX}x{shadow.OffsetY} was parsed.")
            End If
            If std.Abs(shadow.Blur - 8) > 0.01 Then
                errors.Add($"the shadow blur should be 8, but {shadow.Blur} was parsed.")
            End If
            If std.Abs(shadow.Color.A - 114) > 1 Then
                errors.Add($"the shadow alpha should be 114, but {shadow.Color.A} was parsed.")
            End If
        End If

        ' the hyperlink resolves its script expression against the host object
        Call expectClick(host, errors, "link", "openHelp:docs")

        ' the rich text tooltip: the delay is shortened so that the unattended
        ' test does not have to wait for it
        host.Engine.TooltipDelay = 1

        Dim link As UiBox = findById(boxes, "link")
        Dim linkRect As RectangleF = link.Bounds

        Call host.Engine.SimulateHover(CInt(linkRect.Left + linkRect.Width / 2), CInt(linkRect.Top + linkRect.Height / 2))
        Call host.Engine.ShowTooltipNow()

        If Not host.Engine.TooltipVisible Then
            errors.Add("the tooltip of the link is not visible.")
        ElseIf host.Engine.TooltipText <> TOOLTIP_TEXT Then
            errors.Add($"the tooltip should be '{TOOLTIP_TEXT}', but '{host.Engine.TooltipText}' was found.")
        End If

        ' the tooltip is painted on the top of everything, so the saved frame
        ' shows it and its colors can be verified with a pixel sampling
        Call saveFrame(host.Engine, "./lychee-smoke.png", errors)

        ' the tooltip is hidden as soon as the mouse leaves the control
        Call host.Engine.SimulateHover(5, 440)

        If host.Engine.TooltipVisible Then
            errors.Add("the tooltip should be hidden after the mouse has left the control.")
        End If

        ' the browser like tab strip
        Call expectTabStrip(host, errors)

        Call renderMainWindow(errors)

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

    ''' <summary>
    ''' Renders the real main window of this application and saves its frame, so
    ''' that a regression of the test case itself is caught as well.
    ''' </summary>
    ''' <param name="errors"></param>
    Private Sub renderMainWindow(errors As List(Of String))
        Dim main As Form1 = Nothing

        Try
            main = New Form1()

            Call main.Show()

            For i As Integer = 0 To 20
                Call Application.DoEvents()
                Call Threading.Thread.Sleep(20)
            Next

            Dim canvases As New List(Of DxCanvas)()

            Call collectCanvas(main, canvases)

            If canvases.Count < 2 Then
                errors.Add($"the main window should host two directx canvases (the form and the panel), but {canvases.Count} was found.")
            End If

            For Each canvas As DxCanvas In canvases
                Call canvas.Invalidate()
            Next

            For i As Integer = 0 To 10
                Call Application.DoEvents()
                Call Threading.Thread.Sleep(20)
            Next

            For Each canvas As DxCanvas In canvases
                ' the canvas of the form itself and the canvas of the panel host
                ' are saved as two separate frames
                Dim file As String = If(TypeOf canvas.Parent Is Form, "./lychee-form1.png", "./lychee-form1-panel.png")

                Call canvas.SaveImage(file, ImageFormats.Png)
                Console.WriteLine($"[smoke] the frame of {canvas.Parent.GetType().Name} has been saved to {file}")
            Next
        Catch ex As Exception
            errors.Add("the main window can not be rendered: " & ex.Message)
        Finally
            If main IsNot Nothing Then
                Call main.Dispose()
            End If
        End Try
    End Sub

    Private Sub collectCanvas(host As Control, result As List(Of DxCanvas))
        For Each child As Control In host.Controls
            If TypeOf child Is DxCanvas Then
                result.Add(DirectCast(child, DxCanvas))
            End If

            Call collectCanvas(child, result)
        Next
    End Sub

    Private Sub saveFrame(engine As FormRender, file As String, errors As List(Of String))
        Try
            Dim surface = TryCast(engine.RenderSurface, DxCanvasSurface)

            If surface IsNot Nothing Then
                Call surface.CanvasControl.Invalidate()

                For i As Integer = 0 To 10
                    Call Application.DoEvents()
                    Call Threading.Thread.Sleep(20)
                Next

                Call surface.CanvasControl.SaveImage(file, ImageFormats.Png)
                Console.WriteLine($"[smoke] the frame has been saved to {file}")
            End If
        Catch ex As Exception
            errors.Add("the frame can not be saved: " & ex.Message)
        End Try
    End Sub

    Private Function findById(boxes As IReadOnlyList(Of UiBox), id As String) As UiBox
        For Each box As UiBox In boxes
            If box.Attribute("id") = id Then
                Return box
            End If
        Next

        Return Nothing
    End Function

    Private Function indexOf(boxes As IReadOnlyList(Of UiBox), id As String) As Integer
        For i As Integer = 0 To boxes.Count - 1
            If boxes(i).Attribute("id") = id Then
                Return i
            End If
        Next

        Return -1
    End Function

    ''' <summary>
    ''' Simulates a mouse click in the middle of the given control and verifies
    ''' that the host method of its onclick expression has been invoked.
    ''' </summary>
    Private Sub expectClick(host As SmokeHost, errors As List(Of String), id As String, result As String)
        Dim box As UiBox = findById(host.Engine.UiLayout.Boxes, id)

        If box Is Nothing Then
            errors.Add($"the control #{id} is missing.")
            Return
        End If

        Dim b As RectangleF = box.Bounds
        Dim x As Integer = CInt(b.Left + b.Width / 2)
        Dim y As Integer = CInt(b.Top + b.Height / 2)

        If Not host.Engine.SimulateClick(x, y) Then
            errors.Add($"the click of #{id} has not been resolved: {host.Engine.LastError}")
            Return
        End If

        If Not host.Clicks.Contains(result) Then
            errors.Add($"the click of #{id} should have raised '{result}'.")
        End If
    End Sub

    ''' <summary>
    ''' Asserts the state machine and the events of the tab strip that the host
    ''' has registered through the programmatic api.
    ''' </summary>
    Private Sub expectTabStrip(host As SmokeHost, errors As List(Of String))
        Dim strip As New Tabs.TabStrip With {
            .WelcomeContent =
                <page>
                    <label style="display:block;left:16px;top:16px;color:silver">no page is open</label>
                </page>
        }

        Call host.Engine.AddTabStrip(strip)

        If host.Engine.TabStrips.Count = 0 Then
            errors.Add("the registered tab strip is not part of the ui engine.")
            Return
        End If

        Dim activated As New List(Of String)

        AddHandler strip.TabActivated, Sub(tab As Tabs.UiTab) activated.Add(tab.Id)

        Call strip.NewTab("first", Nothing, id:="t1")
        Call strip.NewTab("second", Nothing, id:="t2")

        If strip.Count <> 2 Then
            errors.Add($"the tab strip should own 2 pages, but {strip.Count} was found.")
            Return
        End If

        ' switching to another page
        Call strip.Activate("t2")

        If strip.ActiveId <> "t2" Then
            errors.Add("the page #t2 should be active after Activate.")
        End If
        If Not activated.Contains("t2") Then
            errors.Add("the activation of #t2 should have raised TabActivated.")
        End If

        ' the pages are kept in order until they are moved
        Call strip.MoveTab("t1", 1)

        If strip.TabList(0).Id <> "t2" OrElse strip.TabList(1).Id <> "t1" Then
            errors.Add("the pages have not been reordered by MoveTab.")
        End If

        ' closing a page that is not the active one keeps the active page
        Call strip.CloseTab("t1")

        If strip.Count <> 1 OrElse strip.ActiveId <> "t2" Then
            errors.Add("the page #t2 should survive the close of #t1.")
        End If

        ' closing the last page shows the welcome page
        Call strip.CloseTab("t2")

        If Not strip.IsEmpty OrElse strip.ActiveId IsNot Nothing Then
            errors.Add("the tab strip should be empty after its last page has been closed.")
        End If

        ' a new page is created and activated in one step
        Dim created As Tabs.UiTab = strip.NewTab("created", Nothing)

        If strip.ActiveId <> created.Id Then
            errors.Add("the new page should have been activated.")
        End If
        If Not activated.Contains(created.Id) Then
            errors.Add("the creation of a page should have raised TabActivated.")
        End If
    End Sub

    ''' <summary>
    ''' Simulates a mouse click in the middle of the given control.
    ''' </summary>
    Private Sub clickCenter(host As SmokeHost, id As String)
        Dim box As UiBox = findById(host.Engine.UiLayout.Boxes, id)

        If box Is Nothing Then
            Return
        End If

        Dim b As RectangleF = box.Bounds

        Call host.Engine.SimulateClick(CInt(b.Left + b.Width / 2), CInt(b.Top + b.Height / 2))
    End Sub

    Private Sub expectBox(errors As List(Of String), name As String, box As UiBox, x As Single, y As Single, w As Single, h As Single)
        If box Is Nothing Then
            errors.Add($"the control {name} is missing.")
            Return
        End If

        Dim b As RectangleF = box.Bounds

        ' the paint rectangle of a box that hosts a raster image is the union
        ' of its line boxes, and that rectangle includes the border of the box
        Const tolerance As Single = 3.0F

        If std.Abs(b.Left - x) > tolerance OrElse std.Abs(b.Top - y) > tolerance Then
            errors.Add($"{name} is located at [{b.Left},{b.Top}], but [{x},{y}] was expected.")
        End If

        If std.Abs(b.Width - w) > tolerance OrElse std.Abs(b.Height - h) > tolerance Then
            errors.Add($"{name} is sized {b.Width}x{b.Height}, but {w}x{h} was expected.")
        End If
    End Sub
End Module
