Imports LycheeUI

Public Class Form1

    ''' <summary>
    ''' 主界面的 html + css 申明
    ''' </summary>
    ReadOnly UI As XElement =
        <form style="background-color: gray;" title="test direct-x form">
            <!-- a container element: background, border shorthand, rounded corners and padding -->
            <div id="box" style="left:16px;top:16px;width:320px;height:180px;
                                background-color:lightblue;
                                border:2px solid navy;
                                border-radius:14px;
                                padding:10px">
                <label style="display:block;left:10px;top:12px;width:280px;height:24px;
                              color:darkblue;font-size:15px;text-align:center">nested label</label>
                <button style="left:10px;top:80px;width:160px;height:38px;
                               background-color:seagreen;color:white;
                               border-radius:6px;text-align:center"
                    onclick="nested('from div')">in div</button>
            </div>

            <!-- z-index: the second box overlaps the first one and is painted on top of it -->
            <div style="left:370px;top:16px;width:120px;height:110px;
                        background-color:forestgreen;color:white;
                        text-align:center;z-index:1">z=1</div>
            <div style="left:430px;top:56px;width:120px;height:110px;
                        background-color:darkorange;color:white;
                        text-align:center;z-index:2">z=2</div>

            <!-- the three horizontal alignments and the font styling -->
            <label style="display:block;left:16px;top:210px;width:320px;height:26px;
                          color:navy;font-size:15px;font-weight:bold;text-align:left">left aligned</label>
            <label style="display:block;left:16px;top:240px;width:320px;height:26px;
                          color:purple;font-size:15px;font-style:italic;text-align:center">center aligned</label>
            <label style="display:block;left:16px;top:270px;width:320px;height:26px;
                          color:teal;font-size:15px;font-family:Consolas;text-align:right">right aligned</label>

            <!-- a long paragraph verifies the word wrapping, the span inside of it is an inline element -->
            <p style="left:16px;top:306px;width:320px;height:60px;
                      font-size:12px;color:#333333">
                lychee renders a html and css user interface declaration with
                the <span style="font-weight:bold;color:crimson">directx</span>
                api onto any windows forms control, this paragraph is long
                enough to verify the automatic word wrapping of the layout engine.
            </p>

            <!-- the box model: margin, padding, border and the rounded corners -->
            <button style="left:16px;top:378px;width:150px;height:30px;
                           margin:4px;padding:8px;border:3px solid darkred;
                           border-radius:10px;
                           background-color:gold;color:darkred;text-align:center"
                onclick="clickButton()">box model</button>

            <!-- the original two buttons of this test case -->
            <button id="hello" style="text-align:center; left:50%;top: 50%; width: 200px;height: 60px; color: blue; background-color: red" onclick="clickButton()">hello</button>
            <button id="hello2" style="text-align:center; right:0;bottom: 0; width: 200px;height: 60px; color: blue; background-color: yellow" onclick="click2('aa+bb+cc')">hello</button>

            <!-- the event binding: no argument, a number, two arguments and a literal that contains a comma -->
            <button style="left:210px;top:378px;width:110px;height:30px;
                           background-color:steelblue;color:white;text-align:center"
                onclick="onCount(42)">number</button>
            <button style="left:330px;top:378px;width:110px;height:30px;
                           background-color:mediumpurple;color:white;text-align:center"
                onclick="onMulti('items', 7)">two args</button>
            <button style="left:450px;top:378px;width:130px;height:30px;
                           background-color:dimgray;color:white;text-align:center"
                onclick="onSpecial('a, b (c)')">literal</button>

            <!-- the input controls: a text box, a password box, two radio buttons
                 that share the same group name, and two check boxes -->
            <label style="display:block;left:610px;top:146px;width:180px;height:20px;
                          color:darkslategray;font-size:13px">input controls</label>
            <input type="text" id="name" style="display:block;left:610px;top:170px;width:180px;height:26px;
                               background-color:white;border:1px solid gray;color:black"
                value="lychee" placeholder="user name"/>
            <input type="password" id="pwd" style="display:block;left:610px;top:204px;width:180px;height:26px;
                                   background-color:white;border:1px solid gray;color:black"
                value="1234"/>

            <button style="left:0px, button:0px; width:130px;height:30px;background-color:dimgray;color:white;text-align:center;" onclick="login()">Login</button>

            <input type="radio" id="optA" name="choice" style="display:block;left:610px;top:238px;width:180px;height:24px;
                                  color:black" checked="checked" onchange="onCheck('A', true)" label="option A"/>
            <input type="radio" id="optB" name="choice" style="display:block;left:610px;top:266px;width:180px;height:24px;
                                  color:black" onchange="onCheck('B', true)" label="option B"/>
            <input type="checkbox" id="cb1" style="display:block;left:610px;top:294px;width:180px;height:24px;
                                   color:black" checked="checked" onchange="onToggle('cb1', true)" label="remember me"/>
            <input type="checkbox" id="cb2" style="display:block;left:610px;top:322px;width:180px;height:24px;
                                   color:black" onchange="onToggle('cb2', true)" label="auto start"/>

            <!-- an image element: the natural size is used when no width or height is declared -->
            <img id="logo" src="./lychee-form1-img.png" alt="lychee"
                style="display:block;left:360px;top:340px;width:110px;height:100px;
                        background-color:#e0e0e0;border:1px solid silver"/>

            <!-- a hyperlink: a web address is opened by the browser, a script
                 expression is resolved against the host object -->
            <a id="help" href="openHelp('docs')"
                style="display:block;left:610px;top:352px;width:180px;height:22px"
                tooltip="&lt;b&gt;Documentation&lt;/b&gt;&lt;br/&gt;&lt;font color='gray'&gt;opens the user guide&lt;/font&gt;">documentation</a>
            <a id="site" href="https://github.com/" style="display:block;left:610px;top:378px;width:180px;height:22px"
                tooltip="external &lt;i&gt;web site&lt;/i&gt;">lychee on the web</a>

            <!-- the css drop shadow -->
            <div id="card" style="left:360px;top:150px;width:200px;height:110px;
                                 background-color:white;border:1px solid silver;
                                 border-radius:8px;padding:8px;
                                 box-shadow: 4px 4px 8px rgba(0, 0, 0, 0.45)"
                tooltip="&lt;b&gt;shadow&lt;/b&gt;&lt;br/&gt;css box-shadow demo">
                <label style="display:block;color:dimgray;font-size:13px">shadow demo</label>
            </div>

            <button id="no-css">apply default theme</button>
        </form>

    ''' <summary>
    ''' 渲染在 panel 控件容器中的第二份界面申明，用于验证宿主容器不一定是窗体
    ''' </summary>
    ReadOnly PANEL_UI As XElement =
        <form style="background-color:#202020;" title="panel host">
            <label style="display:block;left:10px;top:10px;width:180px;height:22px;
                          color:white;font-size:13px">rendered on a panel</label>
            <button style="left:10px;top:44px;width:150px;height:30px;
                           background-color:steelblue;color:white;
                           border-radius:4px;text-align:center"
                    onclick="panelClick('panel host')">panel button</button>
        </form>

    Dim WithEvents form As New FormRender(UI, Me)

    ''' <summary>
    ''' the engine is not restricted to a form: any windows forms control can
    ''' host a user interface, a panel is used at here
    ''' </summary>
    Dim panel As New Panel With {
        .Location = New Point(580, 20),
        .Size = New Size(200, 100),
        .BorderStyle = BorderStyle.FixedSingle
    }

    Dim panelUi As FormRender

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' the canvas of the root user interface is docked into the whole client
        ' area of this form and it is the front most control of it, so the panel
        ' must be brought to the front after it has been added
        Call Me.Controls.Add(panel)
        Call panel.BringToFront()

        panelUi = New FormRender(PANEL_UI, panel)
    End Sub

    Private Sub login()
        Dim name = form.GetElementById("name").Value
        Dim pwd = form.GetElementById("pwd").Value

        MessageBox.Show($"name={name}, password={pwd}", "login()")
    End Sub

    Private Sub clickButton()
        MessageBox.Show("Hello world!")
    End Sub

    Private Sub click2(text As String)
        MessageBox.Show(text, "click 2")
    End Sub

    Private Sub nested(text As String)
        MessageBox.Show(text, "nested in div")
    End Sub

    Private Sub onCount(n As Integer)
        MessageBox.Show($"number = {n}", "onCount")
    End Sub

    Private Sub onMulti(name As String, count As Integer)
        MessageBox.Show($"{name} x {count}", "onMulti")
    End Sub

    Private Sub onSpecial(text As String)
        MessageBox.Show(text, "onSpecial")
    End Sub

    Private Sub panelClick(text As String)
        MessageBox.Show(text, "panelClick")
    End Sub

    ''' <summary>
    ''' 由引擎在单选按钮被选中之后回调
    ''' </summary>
    Private Sub onCheck(letter As String, state As Boolean)
        MessageBox.Show($"option {letter} = {state}", "onCheck")
    End Sub

    ''' <summary>
    ''' 由引擎在复选框状态翻转之后回调
    ''' </summary>
    Private Sub onToggle(id As String, state As Boolean)
        MessageBox.Show($"{id} = {state}", "onToggle")
    End Sub

    Private Sub openHelp(topic As String)
        MessageBox.Show(topic, "openHelp")
    End Sub
End Class
