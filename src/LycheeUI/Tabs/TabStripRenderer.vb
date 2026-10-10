Imports System.Drawing
Imports System.Xml.Linq
Imports Microsoft.VisualBasic.Drawing.DirectX
Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Controls
Imports LycheeUI.Layout
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports std = System.Math

Namespace Tabs

    ''' <summary>
    ''' The geometry of a laid out tab strip: it is calculated once per frame and
    ''' is then used by both of the painting and of the hit testing.
    ''' </summary>
    Public Class TabStripLayout

        Public Property StripRect As RectangleF
        Public Property ContentRect As RectangleF
        Public Property IconRect As RectangleF
        Public Property Buttons As New List(Of RectangleF)
        Public Property NewTabRect As RectangleF
        Public Property TabRects As New List(Of RectangleF)
        Public Property CloseRects As New List(Of RectangleF)
        Public Property ScrollMax As Single

        ''' <summary>
        ''' The tab that the mouse is currently over, nothing when the pointer is
        ''' not over the strip.
        ''' </summary>
        ''' <returns></returns>
        Public Property HoverIndex As Integer = -1
    End Class

    ''' <summary>
    ''' Paints a browser like tab strip and its content area.
    ''' </summary>
    Public Module TabStripRenderer

        ''' <summary>
        ''' the renderer factory of the page contents: a page is drawn with the
        ''' very same control renderers as a top level user interface
        ''' </summary>
        Friend ReadOnly PageRenderers As New ControlRendererFactory()

        Public Property StripBackColor As Color = Color.FromArgb(32, 32, 32)
        Public Property StripHoverColor As Color = Color.FromArgb(48, 48, 48)
        Public Property InactiveTabColor As Color = Color.FromArgb(45, 45, 45)
        Public Property InactiveTabHoverColor As Color = Color.FromArgb(62, 62, 62)
        Public Property ActiveTabColor As Color = Color.FromArgb(32, 32, 32)
        Public Property StripTextColor As Color = Color.FromArgb(200, 200, 200)
        Public Property ActiveTextColor As Color = Color.White
        Public Property CloseColor As Color = Color.FromArgb(180, 180, 180)
        Public Property NewTabColor As Color = Color.FromArgb(200, 200, 200)
        Public Property WindowButtonColor As Color = Color.FromArgb(200, 200, 200)
        Public Property WindowCloseColor As Color = Color.FromArgb(220, 220, 220)
        Public Property PlaceholderColor As Color = Color.FromArgb(70, 130, 180)

        Private ReadOnly tabFont As New Font(FontFace.SegoeUI, 9)
        Private ReadOnly iconFont As New Font(FontFace.SegoeUI, 10, FontStyle.Bold)

        ''' <summary>
        ''' The width of a favicon on a tab.
        ''' </summary>
        Public Property FaviconSize As Single = 16.0F

        ''' <summary>
        ''' The width of the window icon and of the window buttons.
        ''' </summary>
        Public Property CaptionButtonWidth As Single = 46.0F

        ''' <summary>
        ''' Lays out the tab strip and its content area.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="strip"></param>
        ''' <param name="bounds">the whole area of the component.</param>
        ''' <param name="hoverPoint">
        ''' the mouse position in canvas coordinates, or an empty point when the
        ''' mouse is outside of the component.
        ''' </param>
        ''' <param name="scrollOffset"></param>
        ''' <returns></returns>
        Public Function Layout(g As IGraphics, strip As TabStrip, bounds As RectangleF,
                               hoverPoint As Point, scrollOffset As Single) As TabStripLayout
            Dim layout As New TabStripLayout With {
                .StripRect = New RectangleF(bounds.Left, bounds.Top, bounds.Width, strip.StripHeight)
            }

            Dim x As Single = bounds.Left
            Dim iconSize As Single = strip.StripHeight - 12.0F

            ' the icon of the window at the left side of the strip
            layout.IconRect = New RectangleF(x + 8, bounds.Top + (strip.StripHeight - iconSize) / 2, iconSize, iconSize)
            x = layout.IconRect.Right + 8

            Dim tabsRight As Single = bounds.Right - CaptionButtonWidth * 3 - 8
            Dim available As Single = std.Max(60.0F, tabsRight - x - NewTabWidth())
            Dim count As Integer = strip.Count
            Dim tabWidth As Single = If(count > 0, std.Min(strip.TabMaxWidth, std.Max(strip.TabMinWidth, available / count)), 0)

            Dim offset As Single = std.Max(0, scrollOffset)
            Dim cursor As Single = x - offset

            For i As Integer = 0 To count - 1
                Dim rect As New RectangleF(cursor, bounds.Top + 3, tabWidth, strip.StripHeight - 3)
                Dim closeRect As RectangleF = CloseRectOf(rect)

                layout.TabRects.Add(rect)
                layout.CloseRects.Add(closeRect)

                cursor = rect.Right

                If i < count - 1 Then
                    cursor += 2
                End If
            Next

            ' the tabs that are scrolled out of the visible area are not hit
            ' testable and their rectangles are collapsed
            For i As Integer = 0 To count - 1
                If layout.TabRects(i).Right < x OrElse layout.TabRects(i).Left > tabsRight Then
                    layout.TabRects(i) = New RectangleF(-1000, -1000, 0, 0)
                    layout.CloseRects(i) = RectangleF.Empty
                End If
            Next

            ' the scroll range: the total width of the tabs minus the visible width
            Dim total As Single = If(count > 0, layout.TabRects(count - 1).Right - x + NewTabWidth(), 0)
            layout.ScrollMax = std.Max(0, total - available)

            layout.NewTabRect = New RectangleF(tabsRight, bounds.Top + 4, NewTabWidth(), strip.StripHeight - 8)

            For i As Integer = 0 To 2
                layout.Buttons.Add(New RectangleF(
                    bounds.Right - CaptionButtonWidth * (3 - i),
                    bounds.Top,
                    CaptionButtonWidth,
                    strip.StripHeight))
            Next

            layout.ContentRect = New RectangleF(
                bounds.Left, bounds.Top + strip.StripHeight,
                bounds.Width, std.Max(0, bounds.Height - strip.StripHeight))

            If hoverPoint <> Point.Empty Then
                Dim hit As TabHitResult = HitTest(layout, strip, hoverPoint)

                If hit.Kind = TabHitKind.Tab Then
                    layout.HoverIndex = hit.Index
                End If
            End If

            Return layout
        End Function

        Private Function CloseRectOf(tabRect As RectangleF) As RectangleF
            Dim size As Single = 14.0F

            Return New RectangleF(tabRect.Right - size - 6, tabRect.Top + (tabRect.Height - size) / 2, size, size)
        End Function

        ''' <summary>
        ''' The width of the new tab button.
        ''' </summary>
        ''' <returns></returns>
        Public Function NewTabWidth() As Single
            Return 28.0F
        End Function

        ''' <summary>
        ''' Finds the element of the tab strip that is located at the given point.
        ''' </summary>
        ''' <param name="layout"></param>
        ''' <param name="strip"></param>
        ''' <param name="p">the point in canvas coordinates.</param>
        ''' <returns></returns>
        Public Function HitTest(layout As TabStripLayout, strip As TabStrip, p As Point) As TabHitResult
            If layout.StripRect.Contains(p.X, p.Y) Then
                For i As Integer = 0 To layout.Buttons.Count - 1
                    If layout.Buttons(i).Contains(p.X, p.Y) Then
                        Return New TabHitResult(TabHitKind.WindowButton, windowButton:=i)
                    End If
                Next

                If layout.NewTabRect.Contains(p.X, p.Y) Then
                    Return New TabHitResult(TabHitKind.NewTab)
                End If

                For i As Integer = 0 To layout.TabRects.Count - 1
                    Dim tab As UiTab = strip.TabList(i)

                    If layout.CloseRects(i).Contains(p.X, p.Y) AndAlso strip.ActiveId = tab.Id Then
                        Return New TabHitResult(TabHitKind.Close, tab, i)
                    End If

                    If layout.TabRects(i).Contains(p.X, p.Y) Then
                        Return New TabHitResult(TabHitKind.Tab, tab, i)
                    End If
                Next

                If layout.IconRect.Contains(p.X, p.Y) Then
                    Return New TabHitResult(TabHitKind.Icon)
                End If

                Return New TabHitResult(TabHitKind.Caption)
            End If

            Return New TabHitResult(TabHitKind.None)
        End Function

        ''' <summary>
        ''' Paints the whole tab strip component: the strip itself, the content
        ''' area and the content of the active page.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="strip"></param>
        ''' <param name="bounds"></param>
        ''' <param name="layout"></param>
        ''' <param name="hoverPoint"></param>
        ''' <param name="scrollOffset"></param>
        Public Sub Render(g As IGraphics, strip As TabStrip, bounds As RectangleF,
                          layout As TabStripLayout, hoverPoint As Point, scrollOffset As Single)

            Call g.FillRectangle(New SolidBrush(StripBackColor), layout.StripRect)

            Dim icon As Image = Nothing
            Dim iconText As String = "L"

            If strip.Count > 0 OrElse True Then
                Call g.FillEllipse(New SolidBrush(PlaceholderColor), layout.IconRect)
                Call g.DrawString(iconText, iconFont, New SolidBrush(Color.White),
                                  layout.IconRect.Left + 1, layout.IconRect.Top - 1)
            End If

            For i As Integer = 0 To strip.Count - 1
                Dim tab As UiTab = strip.TabList(i)
                Dim rect As RectangleF = layout.TabRects(i)
                Dim isActive As Boolean = (strip.ActiveId = tab.Id)
                Dim isHover As Boolean = (layout.HoverIndex = i)
                Dim back As Color

                If isActive Then
                    back = ActiveTabColor
                ElseIf isHover Then
                    back = InactiveTabHoverColor
                Else
                    back = InactiveTabColor
                End If

                Dim radius As Single = If(isActive, 0, 4.0F)
                Dim dx As DxGraphics = TryCast(g, DxGraphics)

                If dx IsNot Nothing Then
                    Call dx.FillRoundedRectangle(New SolidBrush(back), rect, radius)
                Else
                    Call g.FillRectangle(New SolidBrush(back), rect)
                End If

                ' the active tab looks like a part of the content area
                If isActive Then
                    Dim foot As New RectangleF(rect.Left, rect.Bottom - 2, rect.Width, 2)

                    Call g.FillRectangle(New SolidBrush(back), foot)
                End If

                Call drawFavicon(g, tab, New RectangleF(rect.Left + 8, rect.Top + (rect.Height - FaviconSize) / 2, FaviconSize, FaviconSize), isActive)

                Dim textColor As Color = If(isActive, ActiveTextColor, StripTextColor)
                Dim text As String = tab.Title
                Dim textSize As SizeF = g.MeasureString(text, tabFont)
                Dim textLeft As Single = rect.Left + 8 + FaviconSize + 4
                Dim textWidth As Single = rect.Width - (textLeft - rect.Left) - 4

                If layout.CloseRects(i).Width > 0 AndAlso (isActive OrElse isHover) Then
                    textWidth -= 14
                End If

                If textWidth > 0 Then
                    If textSize.Width > textWidth Then
                        While text.Length > 1 AndAlso g.MeasureString(text & "...", tabFont).Width > textWidth
                            text = text.Substring(0, text.Length - 1)
                        End While

                        If text.Length > 0 Then
                            text = text.TrimEnd() & "..."
                        End If
                    End If

                    Dim ty As Single = rect.Top + (rect.Height - textSize.Height) / 2

                    Call g.DrawString(text, tabFont, New SolidBrush(textColor), textLeft, ty)
                End If

                ' the close button is only visible on the active tab and on the
                ' tab that the mouse is currently over
                If layout.CloseRects(i).Width > 0 AndAlso (isActive OrElse isHover) Then
                    Call drawClose(g, layout.CloseRects(i),
                                   If(isActive, ActiveTextColor, CloseColor))
                End If
            Next

            Call drawNewTab(g, layout.NewTabRect)

            For i As Integer = 0 To layout.Buttons.Count - 1
                Call drawWindowButton(g, layout.Buttons(i), i)
            Next

            Call renderContent(g, strip, layout, hoverPoint)
        End Sub

        Private Sub drawFavicon(g As IGraphics, tab As UiTab, rect As RectangleF, active As Boolean)
            Dim image As Image = UiImages.GetOrLoad(tab.Favicon)

            If image IsNot Nothing Then
                Call g.DrawImage(image, rect)
                Return
            End If

            ' a favicon that can not be loaded is replaced by the first letter
            ' of the title on a colored disc
            Call g.FillEllipse(New SolidBrush(PlaceholderColor), rect)

            Dim letter As String = If(String.IsNullOrEmpty(tab.Title), "?", tab.Title.Substring(0, 1).ToUpper())
            Dim size As SizeF = g.MeasureString(letter, iconFont)

            Call g.DrawString(letter, iconFont, New SolidBrush(Color.White),
                              rect.Left + (rect.Width - size.Width) / 2,
                              rect.Top + (rect.Height - size.Height) / 2)
        End Sub

        Private Sub drawClose(g As IGraphics, rect As RectangleF, color As Color)
            Dim inset As Single = rect.Width * 0.28F
            Dim a As New PointF(rect.Left + inset, rect.Top + inset)
            Dim b As New PointF(rect.Right - inset, rect.Bottom - inset)
            Dim c As New PointF(rect.Right - inset, rect.Top + inset)
            Dim d As New PointF(rect.Left + inset, rect.Bottom - inset)

            Call g.DrawLine(New Pen(color, 1.6F), a, b)
            Call g.DrawLine(New Pen(color, 1.6F), c, d)
        End Sub

        Private Sub drawNewTab(g As IGraphics, rect As RectangleF)
            Dim midY As Single = rect.Top + rect.Height / 2
            Dim midX As Single = rect.Left + rect.Width / 2
            Dim arm As Single = 5.0F

            Call g.DrawLine(New Pen(NewTabColor, 1.6F), midX - arm, midY, midX + arm, midY)
            Call g.DrawLine(New Pen(NewTabColor, 1.6F), midX, midY - arm, midX, midY + arm)
        End Sub

        Private Sub drawWindowButton(g As IGraphics, rect As RectangleF, index As Integer)
            Dim midY As Single = rect.Top + rect.Height / 2
            Dim cx As Single = rect.Left + rect.Width / 2
            Dim color As Color = If(index = 2, WindowCloseColor, WindowButtonColor)

            Select Case index
                Case 0
                    ' minimize
                    Call g.DrawLine(New Pen(color, 1.4F), cx - 5, midY, cx + 5, midY)
                Case 1
                    ' maximize: a small hollow square
                    Call g.DrawRectangle(New Pen(color, 1.2F), New RectangleF(cx - 5, midY - 5, 10, 10))
                Case Else
                    ' close
                    Call drawClose(g, New RectangleF(cx - 6, midY - 6, 12, 12), color)
            End Select
        End Sub

        ''' <summary>
        ''' Paints the content area and the content of the active page: the
        ''' background is painted with the color of the active tab so that both
        ''' of them look like one piece, and the page itself is laid out by its
        ''' own layout engine.
        ''' </summary>
        Private Sub renderContent(g As IGraphics, strip As TabStrip, layout As TabStripLayout, hoverPoint As Point)
            Dim area As RectangleF = layout.ContentRect

            If area.Width <= 0 OrElse area.Height <= 0 Then
                Return
            End If

            Call g.FillRectangle(New SolidBrush(strip.ContentColor), area)

            Dim active As UiTab = strip.ActiveTab

            If active Is Nothing Then
                Call renderWelcome(g, strip, area)
                Return
            End If

            Dim engine As UiLayoutEngine = active.Layout

            If engine Is Nothing Then
                Return
            End If

            Dim viewport As New Rectangle(
                CInt(area.Left), CInt(area.Top),
                CInt(std.Max(0, area.Width)), CInt(std.Max(0, area.Height)))

            Call engine.SetPageBackground(strip.ContentColor)

            Dim boxes As List(Of UiBox) = engine.Relayout(viewport, g)

            For Each box As UiBox In boxes
                If box.Bounds.Width <= 0 OrElse box.Bounds.Height <= 0 Then
                    Continue For
                End If

                Try
                    Call PageRenderers.GetRenderer(box).Render(g, box)
                Catch ex As Exception
                    Call Console.WriteLine($"[lychee] render <{box.Tag}> error: {ex.Message}")
                End Try
            Next
        End Sub

        Private Sub renderWelcome(g As IGraphics, strip As TabStrip, area As RectangleF)
            If strip.WelcomeContent Is Nothing Then
                Return
            End If

            If welcomeEngine Is Nothing OrElse welcomeSource IsNot strip.WelcomeContent Then
                welcomeSource = strip.WelcomeContent
                welcomeEngine = New UiLayoutEngine(strip.WelcomeContent)
            End If

            Dim viewport As New Rectangle(
                CInt(area.Left), CInt(area.Top),
                CInt(std.Max(0, area.Width)), CInt(std.Max(0, area.Height)))

            Call welcomeEngine.SetPageBackground(strip.ContentColor)

            For Each box As UiBox In welcomeEngine.Relayout(viewport, g)
                If box.Bounds.Width <= 0 OrElse box.Bounds.Height <= 0 Then
                    Continue For
                End If

                Try
                    Call PageRenderers.GetRenderer(box).Render(g, box)
                Catch ex As Exception
                End Try
            Next
        End Sub

        Private welcomeEngine As UiLayoutEngine
        Private welcomeSource As XElement
    End Module
End Namespace
