Public Class Theme : Inherits ElementTheme

    ' property of the theme object from the elementtheme basetype
    ' used as the global theme default value if the corresponding
    ' property value of elementtheme is missing.

    Public Property button As ElementTheme
    Public Property anchor As ElementTheme
    Public Property textbox As ElementTheme
    Public Property label As ElementTheme
    Public Property radio As ElementTheme
    Public Property div As ElementTheme
    Public Property checkbox As ElementTheme
    Public Property image As ElementTheme

    Public Shared Function DefaultTheme() As Theme

    End Function

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
