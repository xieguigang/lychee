Imports System.Globalization
Imports System.Reflection
Imports System.Windows.Forms

Namespace Events

    ''' <summary>
    ''' Binds the script expression of an event attribute to a method of the
    ''' host control: the method is looked up on the host control itself and
    ''' then on every parent of it, so the click handler of a button may live on
    ''' the form even when the ui is rendered inside of a panel.
    ''' </summary>
    Public NotInheritable Class MethodBinder

        Private Const flags As BindingFlags = BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic

        Private ReadOnly targets As New List(Of Object)()
        Private ReadOnly cache As New Dictionary(Of String, MethodInfo)()

        ''' <summary>
        ''' The message of the last binding failure, nothing means that every
        ''' call has been resolved successfully.
        ''' </summary>
        ''' <returns></returns>
        Public Property LastError As String

        Sub New(container As Control)
            If container Is Nothing Then
                Throw New ArgumentNullException(NameOf(container))
            End If

            Dim host As Control = container

            While host IsNot Nothing
                targets.Add(host)
                host = host.Parent
            End While
        End Sub

        ''' <summary>
        ''' Adds an additional object to the lookup list of this binder.
        ''' </summary>
        ''' <param name="target"></param>
        Public Sub AddTarget(target As Object)
            If target IsNot Nothing AndAlso Not targets.Contains(target) Then
                targets.Add(target)
            End If
        End Sub

        ''' <summary>
        ''' Invokes the host method of the given script expression.
        ''' </summary>
        ''' <param name="expression">the value of the event attribute.</param>
        ''' <returns>
        ''' true when the method has been found and invoked, false when the
        ''' expression is empty or when the host does not declare such a method.
        ''' </returns>
        Public Function Invoke(expression As String) As Boolean
            Dim script As ScriptCall = ScriptCall.Parse(expression)

            If script Is Nothing Then
                Return False
            End If

            Return Invoke(script)
        End Function

        ''' <summary>
        ''' Invokes the host method of the given script call.
        ''' </summary>
        ''' <param name="script"></param>
        ''' <returns></returns>
        Public Function Invoke(script As ScriptCall) As Boolean
            LastError = Nothing

            If script Is Nothing OrElse String.IsNullOrEmpty(script.MethodName) Then
                Return False
            End If

            Dim method As MethodInfo = Resolve(script)

            If method Is Nothing Then
                LastError = $"the host object does not declare a method that is named as '{script.MethodName}' with {script.Arguments.Length} argument(s)."
                Return False
            End If

            Dim args As Object() = Nothing

            If Not TryConvertArguments(method, script.Arguments, args) Then
                LastError = $"the arguments of the call '{script}' can not be converted to the signature of the method '{script.MethodName}'."
                Return False
            End If

            Dim target As Object = method.DeclaringType

            For Each host As Object In targets
                If method.DeclaringType.IsInstanceOfType(host) Then
                    target = host
                    Exit For
                End If
            Next

            Try
                Call method.Invoke(target, args)
                Return True
            Catch ex As Exception
                LastError = ex.InnerException?.Message
                If LastError Is Nothing Then
                    LastError = ex.Message
                End If

                Return False
            End Try
        End Function

        ''' <summary>
        ''' Finds the method of the host objects that matches the given script
        ''' call, the result is cached because a control is clicked very often.
        ''' </summary>
        ''' <param name="script"></param>
        ''' <returns></returns>
        Private Function Resolve(script As ScriptCall) As MethodInfo
            Dim key As String = $"{script.MethodName.ToLower()}/{script.Arguments.Length}"

            If cache.ContainsKey(key) Then
                Return cache(key)
            End If

            For Each host As Object In targets
                Dim type As Type = host.GetType()
                Dim methods As MethodInfo() = type.GetMethods(flags)

                For Each method As MethodInfo In methods
                    If Not method.Name.Equals(script.MethodName, StringComparison.OrdinalIgnoreCase) Then
                        Continue For
                    End If
                    If method.GetParameters().Length <> script.Arguments.Length Then
                        Continue For
                    End If

                    cache(key) = method

                    Return method
                Next
            Next

            cache(key) = Nothing

            Return Nothing
        End Function

        ''' <summary>
        ''' Converts the literal arguments of a script call to the parameter
        ''' types of the resolved method.
        ''' </summary>
        ''' <param name="method"></param>
        ''' <param name="literals"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Private Shared Function TryConvertArguments(method As MethodInfo, literals As String(), ByRef args As Object()) As Boolean
            Dim parameters As ParameterInfo() = method.GetParameters()

            args = New Object(parameters.Length - 1) {}

            For i As Integer = 0 To parameters.Length - 1
                Dim parameterType As Type = parameters(i).ParameterType
                Dim literal As String = literals(i)

                Try
                    If parameterType Is GetType(String) Then
                        args(i) = literal
                    Else
                        args(i) = Convert.ChangeType(literal, parameterType, CultureInfo.InvariantCulture)
                    End If
                Catch ex As Exception
                    args(i) = literal

                    If parameterType.IsValueType Then
                        Return False
                    End If
                End Try
            Next

            Return True
        End Function
    End Class
End Namespace
