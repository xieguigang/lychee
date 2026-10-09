Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Reflection
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime

Namespace Events

    ''' <summary>
    ''' Binds the script expression of an event attribute to the methods of the
    ''' host control: the methods are looked up on the host control itself and
    ''' then on every parent of it, so the click handler of a button may live on
    ''' the form even when the ui is rendered inside of a panel.
    '''
    ''' The javascript code of the event attribute is parsed and executed by the
    ''' LiteJs interpreter; the host methods are exposed to that interpreter as
    ''' global functions (one per method name, bound through reflection), so the
    ''' script may call them like any other javascript function and pass arbitrary
    ''' javascript values as arguments.
    ''' </summary>
    Public NotInheritable Class MethodBinder

        Private Const flags As BindingFlags = BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic

        Private ReadOnly _targets As New List(Of Object)()
        Private ReadOnly _registered As New HashSet(Of String)()
        Private ReadOnly _engine As Interpreter

        ''' <summary>
        ''' The message of the last binding failure, nothing means that every
        ''' call has been resolved and executed successfully.
        ''' </summary>
        ''' <returns></returns>
        Public Property LastError As String

        Sub New(container As Control)
            If container Is Nothing Then
                Throw New ArgumentNullException(NameOf(container))
            End If

            Dim host As Control = container

            While host IsNot Nothing
                _targets.Add(host)
                host = host.Parent
            End While

            _engine = New Interpreter()

            For Each target As Object In _targets
                Call RegisterHost(target)
            Next
        End Sub

        ''' <summary>
        ''' Adds an additional object to the lookup list of this binder and
        ''' exposes its instance methods to the interpreter as well.
        ''' </summary>
        ''' <param name="target"></param>
        Public Sub AddTarget(target As Object)
            If target IsNot Nothing AndAlso Not _targets.Contains(target) Then
                _targets.Add(target)
                Call RegisterHost(target)
            End If
        End Sub

        ''' <summary>
        ''' Invokes the javascript expression of the given event attribute through
        ''' the LiteJs interpreter. Host methods are reachable from the script as
        ''' global functions.
        ''' </summary>
        ''' <param name="expression">the value of the event attribute.</param>
        ''' <returns>
        ''' true when the script has been parsed and executed without error,
        ''' false when the expression is empty or when the script failed.
        ''' </returns>
        Public Function Invoke(expression As String) As Boolean
            LastError = Nothing

            If String.IsNullOrEmpty(expression) Then
                Return False
            End If

            Dim expr As String = expression.Trim()

            If expr.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) Then
                expr = expr.Substring("javascript:".Length).Trim()
            End If

            If expr.Length = 0 Then
                Return False
            End If

            Try
                Dim program As Program = Parser.Parse(expr)
                Call _engine.Run(program)
                Return True
            Catch ex As Exception
                If ex.InnerException IsNot Nothing Then
                    LastError = ex.InnerException.Message
                Else
                    LastError = ex.Message
                End If

                Return False
            End Try
        End Function

        ''' <summary>
        ''' Exposes every instance method of the given host object to the
        ''' interpreter as a global function. The first (closest) host that
        ''' declares a given name wins, mirroring the old resolver behaviour.
        ''' </summary>
        ''' <param name="target"></param>
        Private Sub RegisterHost(target As Object)
            If target Is Nothing Then
                Return
            End If

            Dim methods As New List(Of MethodInfo)(target.GetType().GetMethods(flags))

            For Each method As MethodInfo In methods
                ' skip Object base methods and compiler/property/event accessors
                If method.DeclaringType Is GetType(Object) Then
                    Continue For
                End If
                If method.IsSpecialName Then
                    Continue For
                End If
                If method.IsGenericMethodDefinition Then
                    Continue For
                End If
                If HasByRefParameter(method) Then
                    Continue For
                End If

                Dim name As String = method.Name

                If _registered.Contains(name) Then
                    Continue For
                End If

                _registered.Add(name)

                Dim [overloads] As New List(Of MethodInfo)()

                For Each m As MethodInfo In methods
                    If m.Name = name AndAlso Not m.IsSpecialName Then
                        [overloads].Add(m)
                    End If
                Next

                Dim hostFn As New HostFunction(target, [overloads])

                Call _engine.DefineGlobal(name, hostFn.InvokeDelegate)
            Next
        End Sub

        ''' <summary>
        ''' True when any parameter of the method is passed by reference.
        ''' </summary>
        Private Shared Function HasByRefParameter(method As MethodInfo) As Boolean
            For Each p As ParameterInfo In method.GetParameters()
                If p.ParameterType.IsByRef Then
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' Adapter that turns a set of host method overloads into a
        ''' <c>Func(Of Object(), Object)</c> callable from the interpreter.
        ''' </summary>
        Private NotInheritable Class HostFunction

            Private ReadOnly _target As Object
            Private ReadOnly _methods As List(Of MethodInfo)
            Public ReadOnly InvokeDelegate As Func(Of Object(), Object)

            Public Sub New(target As Object, methods As List(Of MethodInfo))
                _target = target
                _methods = methods
                InvokeDelegate = AddressOf Invoke
            End Sub

            Public Function Invoke(args As Object()) As Object
                Return MethodBinder.InvokeHost(_target, _methods, args)
            End Function
        End Class

        ''' <summary>
        ''' Dispatches a script call to the best matching overload of a host
        ''' method, converting the javascript arguments to the .NET signature and
        ''' the return value back to a javascript value.
        ''' </summary>
        Private Shared Function InvokeHost(target As Object, [overloads] As List(Of MethodInfo), args As Object()) As Object
            Dim method As MethodInfo = PickOverload([overloads], args)

            If method Is Nothing Then
                Throw New InvalidOperationException(
                    "no overload of '" & [overloads](0).Name & "' accepts " & args.Length & " argument(s).")
            End If

            Dim parameters As ParameterInfo() = method.GetParameters()
            Dim clrArgs(parameters.Length - 1) As Object

            For i As Integer = 0 To parameters.Length - 1
                Dim jsValue As Object = If(i < args.Length, args(i), JsRuntime.Undef)
                clrArgs(i) = ConvertJsToClr(jsValue, parameters(i).ParameterType)
            Next

            Dim result As Object = method.Invoke(target, clrArgs)

            Return ConvertClrToJs(result)
        End Function

        ''' <summary>
        ''' Selects the overload whose parameter count best matches the script
        ''' argument count, preferring an exact match.
        ''' </summary>
        Private Shared Function PickOverload([overloads] As List(Of MethodInfo), args As Object()) As MethodInfo
            Dim exact As MethodInfo = Nothing

            For Each m As MethodInfo In [overloads]
                If m.GetParameters().Length = args.Length Then
                    exact = m
                    Exit For
                End If
            Next

            If exact IsNot Nothing Then
                Return exact
            End If

            Dim best As MethodInfo = Nothing
            Dim bestDiff As Integer = Integer.MaxValue

            For Each m As MethodInfo In [overloads]
                Dim n As Integer = m.GetParameters().Length

                If n <= args.Length Then
                    Dim diff As Integer = System.Math.Abs(n - args.Length)

                    If diff < bestDiff Then
                        bestDiff = diff
                        best = m
                    End If
                End If
            Next

            If best IsNot Nothing Then
                Return best
            End If

            Return [overloads](0)
        End Function

        ''' <summary>
        ''' Converts a javascript value (number/string/boolean/array/object/undefined)
        ''' to the expected .NET parameter type.
        ''' </summary>
        Private Shared Function ConvertJsToClr(value As Object, targetType As Type) As Object
            If value Is Nothing OrElse value Is JsRuntime.Undef Then
                If targetType.IsValueType Then
                    Return Activator.CreateInstance(targetType)
                End If
                Return Nothing
            End If

            If targetType Is GetType(Object) Then
                Return value
            End If

            Dim valueType As Type = value.GetType()

            If targetType.IsAssignableFrom(valueType) Then
                Return value
            End If

            Try
                If targetType Is GetType(String) Then
                    Return JsRuntime.JsStr(value)
                ElseIf targetType Is GetType(Boolean) Then
                    Return JsRuntime.JsTruthy(value)
                ElseIf targetType Is GetType(Double) Then
                    Return JsRuntime.JsNum(value)
                ElseIf targetType Is GetType(Single) Then
                    Return CSng(JsRuntime.JsNum(value))
                ElseIf targetType Is GetType(Decimal) Then
                    Return CDec(JsRuntime.JsNum(value))
                ElseIf targetType.IsPrimitive Then
                    Dim d As Double = JsRuntime.JsNum(value)
                    If targetType Is GetType(Integer) Then Return CInt(System.Math.Truncate(d))
                    If targetType Is GetType(Long) Then Return CLng(System.Math.Truncate(d))
                    If targetType Is GetType(Short) Then Return CShort(System.Math.Truncate(d))
                    If targetType Is GetType(Byte) Then Return CByte(System.Math.Truncate(d))
                    If targetType Is GetType(SByte) Then Return CSByte(System.Math.Truncate(d))
                    If targetType Is GetType(UInteger) Then Return CUInt(System.Math.Truncate(d))
                    If targetType Is GetType(ULong) Then Return CULng(System.Math.Truncate(d))
                    If targetType Is GetType(UShort) Then Return CUShort(System.Math.Truncate(d))
                    If targetType Is GetType(Char) Then Return CChar(ChrW(CInt(System.Math.Truncate(d))))
                    Return Convert.ChangeType(d, targetType, CultureInfo.InvariantCulture)
                End If
            Catch
            End Try

            Try
                Return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture)
            Catch
                Return value
            End Try
        End Function

        ''' <summary>
        ''' Converts a .NET return value into a javascript value understood by the
        ''' interpreter. A null / void result becomes <see cref="JsRuntime.Undef"/>.
        ''' </summary>
        Private Shared Function ConvertClrToJs(value As Object) As Object
            If value Is Nothing Then
                Return JsRuntime.Undef
            End If
            If value Is JsRuntime.Undef Then
                Return value
            End If

            Dim t As Type = value.GetType()

            If t Is GetType(Double) OrElse t Is GetType(String) OrElse t Is GetType(Boolean) Then
                Return value
            End If
            If t Is GetType(Single) OrElse t Is GetType(Decimal) OrElse t.IsPrimitive Then
                Return Convert.ToDouble(value, CultureInfo.InvariantCulture)
            End If

            Return value
        End Function
    End Class
End Namespace
