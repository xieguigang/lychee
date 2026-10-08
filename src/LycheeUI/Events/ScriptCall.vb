Imports System.Collections.Generic
Imports System.Text

Namespace Events

    ''' <summary>
    ''' A script expression of an event attribute of a html element, e.g.
    ''' <c>onclick="clickButton()"</c> or <c>onclick="click2('aa+bb+cc')"</c>.
    ''' </summary>
    Public Class ScriptCall

        ''' <summary>
        ''' The name of the host method that should be called.
        ''' </summary>
        ''' <returns></returns>
        Public Property MethodName As String

        ''' <summary>
        ''' The literal arguments of the call.
        ''' </summary>
        ''' <returns></returns>
        Public Property Arguments As String()

        ''' <summary>
        ''' The raw expression text.
        ''' </summary>
        ''' <returns></returns>
        Public Property Expression As String

        Sub New(methodName As String, arguments As String(), Optional expression As String = Nothing)
            Me.MethodName = methodName
            Me.Arguments = arguments
            Me.Expression = expression
        End Sub

        ''' <summary>
        ''' Parses a script expression of an event attribute.
        ''' </summary>
        ''' <param name="expression">
        ''' the value of the event attribute, a leading javascript prefix like
        ''' ``javascript:`` is accepted and removed.
        ''' </param>
        ''' <returns>
        ''' nothing is returned when the given expression is empty or when it is
        ''' not a method call expression.
        ''' </returns>
        Public Shared Function Parse(expression As String) As ScriptCall
            If String.IsNullOrEmpty(expression) Then
                Return Nothing
            End If

            Dim expr As String = expression.Trim()

            If expr.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) Then
                expr = expr.Substring("javascript:".Length).Trim()
            End If

            Dim open As Integer = expr.IndexOf("("c)

            If open <= 0 Then
                ' a bare method name without any argument
                Return New ScriptCall(expr, New String() {}, expression)
            End If

            Dim close As Integer = expr.LastIndexOf(")"c)

            If close < open Then
                close = expr.Length
            End If

            Dim name As String = expr.Substring(0, open).Trim()
            Dim inner As String = expr.Substring(open + 1, close - open - 1)

            Return New ScriptCall(name, SplitArguments(inner), expression)
        End Function

        ''' <summary>
        ''' Splits the argument list of a call expression: the commas that are
        ''' located inside of a string literal are not treated as a separator.
        ''' </summary>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        Public Shared Function SplitArguments(arguments As String) As String()
            Dim result As New List(Of String)()

            If String.IsNullOrEmpty(arguments) Then
                Return result.ToArray()
            End If

            Dim buffer As New StringBuilder()
            Dim quote As Char = ChrW(0)

            For Each c As Char In arguments
                If quote <> ChrW(0) Then
                    If c = quote Then
                        quote = ChrW(0)
                    Else
                        buffer.Append(c)
                    End If
                ElseIf c = "'"c OrElse c = """"c Then
                    quote = c
                ElseIf c = ","c Then
                    result.Add(buffer.ToString().Trim())
                    buffer.Clear()
                Else
                    buffer.Append(c)
                End If
            Next

            result.Add(buffer.ToString().Trim())

            ' the trailing empty arguments of a call like ``f(1,)`` are dropped
            For i As Integer = result.Count - 1 To 0 Step -1
                If result(i).Length = 0 Then
                    result.RemoveAt(i)
                Else
                    Exit For
                End If
            Next

            Return result.ToArray()
        End Function

        Public Overrides Function ToString() As String
            Return $"{MethodName}({String.Join(", ", Arguments)})"
        End Function
    End Class
End Namespace
