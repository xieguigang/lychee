Imports LycheeUI

Public Class Form1

    ReadOnly UI As XElement =
        <form style="background-color: gray;" title="test direct-x form">
            <button id="hello" style="text-align:center; left:50%;top: 50%; width: 200px;height: 60px; color: blue; background-color: red" onclick="clickButton()">hello</button>
            <button id="hello" style="text-align:center; right:0;button: 0; width: 200px;height: 60px; color: blue; background-color: yellow" onclick="click2('aa+bb+cc')">hello</button>
        </form>

    Dim WithEvents form As New FormRender(UI, Me)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub clickButton()
        MessageBox.Show("Hello world!")
    End Sub

    Private Sub click2(text As String)
        MessageBox.Show(text, "click 2")
    End Sub
End Class
