Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Layout
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports std = System.Math

Namespace Controls

    ''' <summary>
    ''' Paints a text input control: the appearance of a windows forms textbox
    ''' is imitated by a bordered rectangle, the text itself and the caret are
    ''' painted with the directx canvas as well.
    ''' </summary>
    Public NotInheritable Class TextInputRenderer
        Implements IControlRenderer

        ''' <summary>
        ''' The color of the border of a control that holds the keyboard focus.
        ''' </summary>
        ''' <returns></returns>
        Public Property FocusColor As Color = Color.DodgerBlue

        ''' <summary>
        ''' The color of the hint text of an empty control.
        ''' </summary>
        ''' <returns></returns>
        Public Property PlaceholderColor As Color = Color.DimGray

        ''' <summary>
        ''' The mask character of a password input.
        ''' </summary>
        ''' <returns></returns>
        Public Property PasswordChar As Char = "•"c

        ''' <summary>
        ''' The background color of a disabled control.
        ''' </summary>
        ''' <returns></returns>
        Public Property DisabledColor As Color = Color.LightGray

        ''' <summary>
        ''' Is the caret of the focused control visible right now? it is
        ''' toggled by the blink timer of the ui engine.
        ''' </summary>
        ''' <returns></returns>
        Public Property CaretVisible As Boolean = True

        Public Sub Render(g As IGraphics, box As UiBox) Implements IControlRenderer.Render
            Dim bounds As RectangleF = box.Bounds

            If bounds.Width <= 0 OrElse bounds.Height <= 0 Then
                Return
            End If

            Dim background As Color = If(box.IsDisabled, DisabledColor, box.Background)
            Dim border As Color = box.BorderColor
            Dim color As Color = box.ForeColor

            If box.Focused Then
                border = FocusColor
            End If

            Call BoxPainter.FillBox(g, box, bounds, background)

            ' the default border color of the css box model is transparent, an
            ' input control without a declared border still needs its frame
            If border.A = 0 Then
                border = Color.Gray
            End If

            Call BoxPainter.DrawBorder(g, box, bounds, If(box.BorderWidth > 0, box.BorderWidth, 1.0F), border)

            Dim text As String = If(box.Value, "")
            Dim masked As Boolean = box.InputType = "password"

            If masked Then
                text = New String(PasswordChar, text.Length)
            End If

            If color.A = 0 Then
                color = Color.Black
            End If

            If text.Length = 0 AndAlso Not String.IsNullOrEmpty(box.Placeholder) Then
                Call drawText(g, box, box.Placeholder, PlaceholderColor)
            Else
                Call drawText(g, box, text, color)
            End If

            If box.Focused AndAlso CaretVisible Then
                Dim caretText As String = If(masked,
                    New String(PasswordChar, std.Max(0, box.Caret)),
                    If(box.Value, ""))

                Call drawCaret(g, box, caretText)
            End If
        End Sub

        ''' <summary>
        ''' Draws the text of the control: it is left aligned and vertically
        ''' centered inside of the content box of the control, and it is
        ''' clipped against that box.
        ''' </summary>
        Private Shared Sub drawText(g As IGraphics, box As UiBox, text As String, color As Color)
            Dim area As RectangleF = box.ContentBounds
            Dim font As Font = box.Font
            Dim size As SizeF = g.MeasureString(text, font)
            Dim x As Single = area.Left
            Dim y As Single = area.Top + (area.Height - size.Height) / 2.0F

            Select Case box.TextAlign
                Case "center"
                    x = area.Left + (area.Width - size.Width) / 2.0F
                Case "right"
                    x = area.Right - size.Width
            End Select

            If size.Width > area.Width Then
                ' the text is wider than the control: shift it so that the end
                ' of the text stays visible, just like a real textbox does
                x = area.Right - size.Width
            End If

            Dim clip As RectangleF = area

            g.SetClip(clip)

            Try
                Call g.DrawString(text, font, New SolidBrush(color), x, y)
            Finally
                Call g.ResetClip()
            End Try
        End Sub

        ''' <summary>
        ''' Draws the caret of the focused control at the caret offset of its
        ''' text.
        ''' </summary>
        Private Shared Sub drawCaret(g As IGraphics, box As UiBox, text As String)
            If String.IsNullOrEmpty(text) Then
                text = ""
            End If

            Dim area As RectangleF = box.ContentBounds
            Dim font As Font = box.Font
            Dim offset As Integer = std.Min(std.Max(box.Caret, 0), text.Length)
            Dim prefix As String = text.Substring(0, offset)
            Dim x As Single = area.Left + g.MeasureString(prefix, font).Width
            Dim height As Single = g.MeasureString("|", font).Height
            Dim top As Single = area.Top + (area.Height - height) / 2.0F

            If x > area.Right Then
                x = area.Right
            End If

            Call g.FillRectangle(New SolidBrush(Color.Black), x, top, 1.0F, height)
        End Sub
    End Class
End Namespace
