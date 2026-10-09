Imports System.Text
Imports System.Xml.Linq

''' <summary>
''' The typed css value set of a default element style: every property holds
''' the value of one css property, a nothing value means that the property is
''' not defined by the theme.
''' </summary>
''' <remarks>
''' all property value is a css value string, example as ``13px`` or
''' ``1px solid gray``.
''' </remarks>
Public Class ElementTheme

    Public Property background As String
    Public Property color As String
    Public Property font As String
    Public Property fontSize As String
    Public Property border As String
    Public Property shadow As String
    Public Property width As String
    Public Property height As String
    Public Property padding As String
    Public Property textAlign As String
    Public Property display As String

    ''' <summary>
    ''' Builds the css style text of this theme: only the properties that have
    ''' a value are emitted, each of them as a ``name:value;`` declaration.
    ''' </summary>
    Public Function ToCss() As String
        Dim css As New StringBuilder()

        Call appendValue(css, "background-color", background)
        Call appendValue(css, "color", color)
        Call appendValue(css, "font", font)
        Call appendValue(css, "font-size", fontSize)
        Call appendValue(css, "border", border)
        Call appendValue(css, "box-shadow", shadow)
        Call appendValue(css, "width", width)
        Call appendValue(css, "height", height)
        Call appendValue(css, "padding", padding)
        Call appendValue(css, "text-align", textAlign)
        Call appendValue(css, "display", display)

        Return css.ToString()
    End Function

    Private Shared Sub appendValue(css As StringBuilder, name As String, value As String)
        If String.IsNullOrEmpty(value) Then
            Return
        End If

        Call css.Append(name).Append(":").Append(value.Trim()).Append(";")
    End Sub
End Class

''' <summary>
''' The theme of the ui engine: it provides a default css style for each kind
''' of the user interface controls, so that a control which does not declare
''' any style in the ui document still gets a usable size, font and color
''' instead of being rendered as an empty 0x0 rectangle.
''' </summary>
''' <remarks>
''' The properties that are inherited from <see cref="ElementTheme"/> provide
''' the default style of the root ``form`` element, the other properties
''' provide the default style of a control by its tag name (the ``input``
''' elements are resolved by their ``type`` attribute). A theme style only
''' fills the style properties that the ui document does not declare, an
''' explicitly declared style of the user always wins.
''' </remarks>
Public Class Theme : Inherits ElementTheme

    Public Property button As ElementTheme
    Public Property anchor As ElementTheme
    Public Property textbox As ElementTheme
    Public Property checkbox As ElementTheme
    Public Property radio As ElementTheme
    Public Property label As ElementTheme
    Public Property image As ElementTheme

    ''' <summary>
    ''' Gets the default css style text of the given element tag.
    ''' </summary>
    ''' <param name="tag">the tag name of the element, example as ``button``.</param>
    ''' <param name="inputType">
    ''' the ``type`` attribute value of an ``input`` element.
    ''' </param>
    Public Function GetCss(tag As String, Optional inputType As String = Nothing) As String
        If String.IsNullOrEmpty(tag) Then
            Return Nothing
        End If

        Select Case tag.ToLower().Trim()
            Case "form", "body"
                Return ToCss()
            Case "button"
                Return cssOf(button)
            Case "a"
                Return cssOf(anchor)
            Case "label"
                Return cssOf(label)
            Case "img"
                Return cssOf(image)
            Case "input"
                Select Case If(inputType, "text").ToLower().Trim()
                    Case "checkbox"
                        Return cssOf(checkbox)
                    Case "radio"
                        Return cssOf(radio)
                    Case "button", "submit", "reset"
                        Return cssOf(button)
                    Case Else
                        Return cssOf(textbox)
                End Select
            Case Else
                Return Nothing
        End Select
    End Function

    Private Shared Function cssOf(style As ElementTheme) As String
        If style Is Nothing Then
            Return Nothing
        Else
            Return style.ToCss()
        End If
    End Function

    ''' <summary>
    ''' Applies the default styles of this theme to the ui document: every
    ''' element gets the theme style of its tag merged into its ``style``
    ''' attribute, but only the style properties that the element does not
    ''' declare on its own, so the style of the user always wins and no css
    ''' property name can be duplicated in the merged style text.
    ''' </summary>
    ''' <param name="ui">the root element of the ui document.</param>
    Public Sub Apply(ui As XElement)
        If ui Is Nothing Then
            Return
        End If

        Call applyElement(ui)
        Call applyChildren(ui)
    End Sub

    Private Sub applyChildren(node As XElement)
        For Each child As XElement In node.Elements()
            Call applyElement(child)
            Call applyChildren(child)
        Next
    End Sub

    Private Sub applyElement(e As XElement)
        Dim themeCss As String = GetCss(e.Name.LocalName, e.@type)

        If String.IsNullOrEmpty(themeCss) Then
            Return
        End If

        Dim userStyle As String = e.@style
        Dim filled As String = FillMissing(themeCss, userStyle)

        If Not String.IsNullOrEmpty(filled) Then
            e.@style = If(String.IsNullOrEmpty(userStyle), filled, filled & " " & userStyle)
        End If
    End Sub

    ''' <summary>
    ''' Builds the style text of the theme declarations that the user style
    ''' does not declare: nothing is returned when every theme property is
    ''' already covered by the user style.
    ''' </summary>
    ''' <param name="themeCss">the css style text of the theme.</param>
    ''' <param name="userStyle">
    ''' the css style text that is declared on the element by the user.
    ''' </param>
    Public Shared Function FillMissing(themeCss As String, userStyle As String) As String
        If String.IsNullOrEmpty(themeCss) Then
            Return Nothing
        ElseIf String.IsNullOrEmpty(userStyle) Then
            Return themeCss
        End If

        Dim used As New HashSet(Of String)(ParseKeys(userStyle))
        Dim css As New StringBuilder()

        For Each p As (key As String, value As String) In ParsePairs(themeCss)
            If String.IsNullOrEmpty(p.key) OrElse String.IsNullOrEmpty(p.value) Then
                Continue For
            ElseIf used.Contains(p.key) Then
                Continue For
            Else
                Call css.Append(p.key).Append(":").Append(p.value).Append(";")
            End If
        Next

        Return css.ToString()
    End Function

    Private Shared Function ParseKeys(css As String) As IEnumerable(Of String)
        Dim keys As New List(Of String)()

        For Each p As (key As String, value As String) In ParsePairs(css)
            If Not String.IsNullOrEmpty(p.key) Then
                Call keys.Add(p.key)
            End If
        Next

        Return keys
    End Function

    Private Shared Iterator Function ParsePairs(css As String) As IEnumerable(Of (key As String, value As String))
        For Each decl As String In css.Split(";"c)
            Dim i As Integer = decl.IndexOf(":"c)

            If i <= 0 Then
                Continue For
            End If

            Dim key As String = decl.Substring(0, i).Trim().ToLower()
            Dim value As String = decl.Substring(i + 1).Trim()

            Yield (key, value)
        Next
    End Function

    ''' <summary>
    ''' The built-in default theme of the ui engine.
    ''' </summary>
    Public Shared Function DefaultTheme() As Theme
        Return New Theme With {
            .background = "#FFFFFF",
            .color = "#202020",
            .fontSize = "13px",
            .button = New ElementTheme With {
                .width = "110px",
                .height = "30px",
                .background = "#3B6EA5",
                .color = "#FFFFFF",
                .border = "1px solid #808080",
                .textAlign = "center",
                .display = "block",
                .fontSize = "13px"
            },
            .textbox = New ElementTheme With {
                .width = "180px",
                .height = "26px",
                .background = "#FFFFFF",
                .color = "#000000",
                .border = "1px solid #808080",
                .display = "block",
                .fontSize = "13px"
            },
            .checkbox = New ElementTheme With {
                .width = "16px",
                .height = "16px",
                .display = "block"
            },
            .radio = New ElementTheme With {
                .width = "16px",
                .height = "16px",
                .display = "block"
            },
            .anchor = New ElementTheme With {
                .color = "#1E90FF",
                .fontSize = "13px"
            },
            .label = New ElementTheme With {
                .color = "#202020",
                .fontSize = "13px"
            },
            .image = New ElementTheme With {
                .display = "block"
            }
        }
    End Function

End Class
