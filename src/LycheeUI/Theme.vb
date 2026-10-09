Public Class Theme : Inherits ElementTheme

    Public Property button As Theme
    Public Property anchor As Theme
    Public Property textbox As Theme

End Class

''' <summary>
''' 
''' </summary>
''' <remarks>
''' all property element is the css style string
''' </remarks>
Public Class ElementTheme

    Public Property background As String
    Public Property color As String
    Public Property font As String
    Public Property border As String
    Public Property shadow As String

End Class
