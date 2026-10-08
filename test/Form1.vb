Imports LycheeUI

Public Class Form1

    ReadOnly UI As XElement =
        <form style="background-color: gray;">
            <button id="hello" style="text-align:center; left:50%;top: 50%; width: 200px;height: 60px; color: blue; background-color: red" onclick="clickButton()">hello</button>
        </form>

    Dim WithEvents form As New FormRender(UI, Me)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub clickButton()
        MessageBox.Show("Hello world!")
    End Sub
End Class
