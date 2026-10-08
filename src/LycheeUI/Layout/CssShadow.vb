Imports System.Collections.Generic
Imports System.Drawing
Imports System.Text
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

Namespace Layout

    ''' <summary>
    ''' The parsed value of the css ``box-shadow`` property of a control.
    ''' </summary>
    ''' <remarks>
    ''' Only the outer shadow is supported at here: the declaration has the
    ''' shape of ``offset-x offset-y blur color``, the ``inset`` keyword and the
    ''' optional spread length are accepted but ignored.
    ''' </remarks>
    Public Structure CssShadow

        ''' <summary>
        ''' The horizontal offset of the shadow.
        ''' </summary>
        ''' <returns></returns>
        Public OffsetX As Single

        ''' <summary>
        ''' The vertical offset of the shadow.
        ''' </summary>
        ''' <returns></returns>
        Public OffsetY As Single

        ''' <summary>
        ''' The blur radius of the shadow, zero means a hard shadow.
        ''' </summary>
        ''' <returns></returns>
        Public Blur As Single

        ''' <summary>
        ''' The color of the shadow.
        ''' </summary>
        ''' <returns></returns>
        Public Color As Color

        ''' <summary>
        ''' Has a shadow been declared on the control?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Color.A = 0
            End Get
        End Property

        ''' <summary>
        ''' Parses the raw css value of a ``box-shadow`` declaration.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns>
        ''' an empty shadow is returned when the value is empty or when it can
        ''' not be understood, this method never throws.
        ''' </returns>
        Public Shared Function Parse(text As String) As CssShadow
            Dim empty As New CssShadow With {.Color = Color.Empty}

            If String.IsNullOrEmpty(text) Then
                Return empty
            End If

            Dim tokens As String() = Tokenize(text)
            Dim lengths As New List(Of Single)
            Dim colorText As String = Nothing

            For Each token As String In tokens
                Dim lower As String = token.ToLower()

                ' the inset keyword and the spread length are not supported
                If lower = "inset" OrElse lower = "none" Then
                    Continue For
                End If

                If isLength(lower) AndAlso lengths.Count < 3 Then
                    Call lengths.Add(ParseLength(lower))
                ElseIf isColor(lower) OrElse HasColorName(lower) Then
                    colorText = token
                ElseIf lengths.Count >= 2 Then
                    ' an unknown trailing token is most likely the color
                    colorText = token
                End If
            Next

            If lengths.Count < 2 OrElse colorText Is Nothing Then
                Return empty
            End If

            Dim resolved As Boolean = False
            Dim resolvedColor As Color = colorText.TranslateColor(throwEx:=False, success:=resolved)

            If Not resolved OrElse resolvedColor.A = 0 Then
                Return empty
            End If

            Return New CssShadow With {
                .OffsetX = lengths(0),
                .OffsetY = lengths(1),
                .Blur = If(lengths.Count > 2, lengths(2), 0),
                .Color = resolvedColor
            }
        End Function

        ''' <summary>
        ''' Splits the declaration into tokens: a parenthesized group like
        ''' ``rgba(0, 0, 0, 0.5)`` is kept as a single token, so that a color
        ''' value that contains blanks is not broken into pieces.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Friend Shared Function Tokenize(text As String) As String()
            Dim tokens As New List(Of String)()
            Dim buffer As New StringBuilder()
            Dim depth As Integer = 0

            For Each c As Char In text
                If c = "("c Then
                    depth += 1
                    buffer.Append(c)
                ElseIf c = ")"c Then
                    depth = std.Max(0, depth - 1)
                    buffer.Append(c)
                ElseIf depth = 0 AndAlso (c = " "c OrElse c = ControlChars.Tab) Then
                    If buffer.Length > 0 Then
                        tokens.Add(buffer.ToString())
                        buffer.Clear()
                    End If
                Else
                    buffer.Append(c)
                End If
            Next

            If buffer.Length > 0 Then
                tokens.Add(buffer.ToString())
            End If

            Return tokens.ToArray()
        End Function

        Private Shared Function isLength(text As String) As Boolean
            If text.Length = 0 Then
                Return False
            End If

            If Not (Char.IsDigit(text(0)) OrElse (text(0) = "-"c AndAlso text.Length > 1) OrElse
                (text(0) = "+"c AndAlso text.Length > 1)) Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' Converts a css length into pixels, a percentage value is treated as
        ''' a pixel value because a shadow has no meaningful base length.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Public Shared Function ParseLength(text As String) As Single
            Dim buffer As New StringBuilder()
            Dim value As Single = 0

            For Each c As Char In text
                If Char.IsDigit(c) OrElse c = "."c OrElse c = "-"c OrElse c = "+"c Then
                    buffer.Append(c)
                Else
                    Exit For
                End If
            Next

            If buffer.Length > 0 Then
                Single.TryParse(buffer.ToString(), Globalization.NumberStyles.Float,
                                Globalization.CultureInfo.InvariantCulture, value)
            End If

            Return value
        End Function

        Private Shared Function isColor(text As String) As Boolean
            Return text.StartsWith("#"c) OrElse
                text.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) OrElse
                text.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase) OrElse
                text.StartsWith("hsl(", StringComparison.OrdinalIgnoreCase)
        End Function

        ''' <summary>
        ''' Is the token likely to be a named color? any word that is not a
        ''' length or a keyword is treated as a named color at here.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Private Shared Function HasColorName(text As String) As Boolean
            If text.Length = 0 Then
                Return False
            End If

            If isLength(text) OrElse isColor(text) Then
                Return False
            End If

            Return text.All(AddressOf Char.IsLetter)
        End Function

        Public Overrides Function ToString() As String
            Return $"shadow({OffsetX}, {OffsetY}, {Blur}, {Color})"
        End Function
    End Structure
End Namespace
