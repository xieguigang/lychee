Imports System.Drawing
Imports System.Xml.Linq
Imports Microsoft.VisualBasic.Imaging
Imports LycheeUI.Controls
Imports LycheeUI.Tabs
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.MIME.Html.Render.CSS
Imports Image = Microsoft.VisualBasic.Imaging.Image

Namespace Layout

    ''' <summary>
    ''' The layout engine of the ui: it reuses the css box model and the css
    ''' layout engine of the html code library, so the ui declaration is laid
    ''' out with exactly the same algorithm that renders a html document.
    ''' </summary>
    ''' <remarks>
    ''' The layout is recalculated whenever the size of the canvas or the
    ''' interactive state of a control has been changed, the
    ''' <see cref="UiBox"/> view objects are cached by their
    ''' <see cref="CssBox"/> source so that the hover and the pressed state of a
    ''' control survives a relayout.
    ''' </remarks>
    Public NotInheritable Class UiLayoutEngine

        Private ReadOnly root As InitialContainer
        Private ReadOnly source As XElement
        Private ReadOnly views As New Dictionary(Of CssBox, UiBox)()
        Private ReadOnly roots As New List(Of UiBox)()
        Private ReadOnly paintOrder As New List(Of UiBox)()

        ''' <summary>
        ''' The root css box of the ui declaration.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Container As InitialContainer
            Get
                Return root
            End Get
        End Property

        ''' <summary>
        ''' The background color that is declared on the root element of the ui.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BackgroundColor As Color
            Get
                Dim box As CssBox = PageBox

                Return box.ActualBackgroundColor
            End Get
        End Property

        ''' <summary>
        ''' The root element of the ui declaration, it fills the whole canvas.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PageBox As CssBox
            Get
                If root.Boxes IsNot Nothing AndAlso root.Boxes.Count > 0 Then
                    Return root.Boxes(0)
                End If

                Return root
            End Get
        End Property

        ''' <summary>
        ''' Overrides the background color of the root element: it is used by the
        ''' tab strip renderer to paint the content of a page with the very same
        ''' color as the active tab.
        ''' </summary>
        ''' <param name="color"></param>
        Public Sub SetPageBackground(color As Color)
            If color.IsEmpty Then
                Return
            End If

            Call PageBox.SetBackgroundColor(color)
        End Sub

        ''' <summary>
        ''' The title that is declared on the root element of the ui.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Title As String
            Get
                If root.Boxes IsNot Nothing AndAlso root.Boxes.Count > 0 Then
                    Return root.Boxes(0).GetAttribute("title")
                End If

                Return Nothing
            End Get
        End Property

        ''' <param name="theme">
        ''' the default styles of the controls: nothing means that the ui
        ''' document is parsed as-is, otherwise the theme styles are merged
        ''' into the ``style`` attributes of the document before the layout.
        ''' </param>
        Sub New(ui As XElement, Optional theme As Theme = Nothing)
            source = ui

            If theme IsNot Nothing Then
                ' a deep copy keeps the ui document of the caller untouched
                Dim doc As New XElement(ui)

                Call theme.Apply(doc)
                root = New InitialContainer(doc)
            Else
                root = New InitialContainer(ui)
            End If
        End Sub

        ''' <summary>
        ''' Recalculates the layout of the ui for the given canvas size.
        ''' </summary>
        ''' <param name="viewport">the client size of the canvas in pixels.</param>
        ''' <param name="g">
        ''' the canvas of the frame: the text metrics of the layout engine are
        ''' measured on it.
        ''' </param>
        ''' <returns>
        ''' the controls of the ui, sorted by their paint order (the z-index
        ''' first and the document order second).
        ''' </returns>
        Public Function Relayout(viewport As Size, g As IGraphics) As List(Of UiBox)
            Return Relayout(New Rectangle(Point.Empty, viewport), g)
        End Function

        ''' <summary>
        ''' Recalculates the layout of the ui inside of the given rectangle: it
        ''' is used by the tab strip to lay a page out inside of the content area
        ''' instead of the whole canvas.
        ''' </summary>
        ''' <param name="area"></param>
        ''' <param name="g"></param>
        ''' <returns></returns>
        Public Function Relayout(area As Rectangle, g As IGraphics) As List(Of UiBox)
            If area.Width <= 0 OrElse area.Height <= 0 Then
                Return paintOrder
            End If

            Call ApplyViewport(area.Size)
            Call ApplyImageSizes()
            Call root.SetBounds(New RectangleF(area.Left, area.Top, area.Width, area.Height))
            Call root.MeasureBounds(g)

            roots.Clear()
            paintOrder.Clear()

            Call walk(root, depth:=0)
            Call BuildTabStrips()

            ' the paint order is the z-index first and the document order
            ' second, a stable sort is required to keep the document order of
            ' the boxes that share the same z-index value
            paintOrder.Sort(Function(a, b)
                                Dim z As Integer = a.ZIndex.CompareTo(b.ZIndex)

                                Return If(z <> 0, z, a.Order.CompareTo(b.Order))
                            End Function)

            Return paintOrder
        End Function

        ''' <summary>
        ''' The root element of a user interface declaration always fills the
        ''' whole canvas: the default stylesheet of the html renderer gives a
        ''' document margin to the root element and it grows the element by its
        ''' own content only, while a user interface needs a full size page box
        ''' so that the percentage offsets of its controls can be resolved.
        ''' </summary>
        ''' <param name="viewport"></param>
        Private Sub ApplyViewport(viewport As Size)
            If root.Boxes Is Nothing OrElse root.Boxes.Count = 0 Then
                Return
            End If

            Dim page As CssBox = root.Boxes(0)

            page.MarginTop = "0"
            page.MarginBottom = "0"
            page.MarginLeft = "0"
            page.MarginRight = "0"
            page.Width = viewport.Width & "px"
            page.Height = viewport.Height & "px"

            ' the default stylesheet of the html renderer only gives the block
            ' display mode to the known html tags, so the display mode is
            ' forced at here to make a custom root tag of a ui declaration work
            page.Display = CssConstants.Block
        End Sub

        ''' <summary>
        ''' An image element does not carry any text, so a box that does not
        ''' declare a width or a height collapses into an empty rectangle. the
        ''' natural size of the image is written back to such a box at here,
        ''' before the layout of the whole document is measured.
        ''' </summary>
        Private Sub ApplyImageSizes()
            If root.Boxes Is Nothing Then
                Return
            End If

            Call applyImageSizes(root)
        End Sub

        Private Sub applyImageSizes(box As CssBox)
            If box.Boxes Is Nothing Then
                Return
            End If

            For Each child As CssBox In box.Boxes
                If child Is Nothing Then
                    Continue For
                End If

                applyImageSizes(child)

                If child.HtmlTag Is Nothing Then
                    Continue For
                End If
                If Not child.HtmlTag.TagName.Equals("img", StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If

                Dim src As String = child.GetAttribute("src")
                Dim image As Image = UiImages.GetOrLoad(src)

                If image Is Nothing Then
                    Continue For
                End If

                If isAutoSize(child.Width) Then
                    child.Width = image.Width & "px"
                End If
                If isAutoSize(child.Height) Then
                    child.Height = image.Height & "px"
                End If
            Next
        End Sub

        Private Shared Function isAutoSize(css As String) As Boolean
            Return String.IsNullOrEmpty(css) OrElse
                css.Trim().Equals(CssConstants.Auto, StringComparison.OrdinalIgnoreCase)
        End Function

        Private Sub walk(box As CssBox, depth As Integer)
            If box Is Nothing OrElse box.Display = CssConstants.None Then
                Return
            End If

            If box.Boxes Is Nothing Then
                Return
            End If

            For Each child As CssBox In box.Boxes
                If child Is Nothing Then
                    Continue For
                End If

                ' an anonymous box carries nothing but the text of its parent
                ' element, that text is drawn by the renderer of the parent
                ' element itself with the text alignment of it, so the
                ' anonymous box is not painted on its own
                If child.HtmlTag Is Nothing Then
                    Continue For
                End If

                Dim view As UiBox = GetView(child)

                If depth = 0 Then
                    roots.Add(view)
                    Call Console.WriteLine($"[lychee] top-level tag: '{view.Tag}' id='{view.Attribute("id")}'")
                End If

                ' a tab control is a reusable component: it owns its own state
                ' and it is painted by the tab strip renderer instead of the
                ' generic control renderers
                If view.IsTabControl Then
                    Call RegisterTabStrip(view)
                    Continue For
                End If

                paintOrder.Add(view)

                Call walk(child, depth + 1)
            Next
        End Sub

        ''' <summary>
        ''' Registers the box of a ``&lt;tabcontrol&gt;`` element, the tab strip
        ''' itself is built from the declaration by <see cref="BuildTabStrips"/>.
        ''' </summary>
        ''' <param name="view"></param>
        Private Sub RegisterTabStrip(view As UiBox)
            Dim id As String = If(view.Attribute("id"), "tabs")

            Call Console.WriteLine($"[lychee] tabcontrol registered: id='{id}'")

            tabBoxes(id) = view

            If Not strips.ContainsKey(id) Then
                strips(id) = New TabStrip()
            End If
        End Sub

        ''' <summary>
        ''' Builds the tab strips out of the ``&lt;tabcontrol&gt;`` elements of
        ''' the declaration: every ``&lt;page&gt;`` child becomes a tab whose
        ''' content is laid out by its own layout engine, so the state of the
        ''' input controls of a page survives a switch to another tab.
        ''' </summary>
        ''' <remarks>
        ''' The strips are built only once, the pages that are added at runtime
        ''' through the <see cref="TabStrip.NewTab"/> method are not touched.
        ''' </remarks>
        Private Sub BuildTabStrips()
            If source Is Nothing OrElse stripsBuilt Then
                Return
            End If

            stripsBuilt = True

            For Each control As XElement In source.Descendants("tabcontrol")
                Dim id As String = If(CStr(control.Attribute("id")), "tabs")

                If Not strips.ContainsKey(id) Then
                    strips(id) = New TabStrip()
                End If

                Dim strip As TabStrip = strips(id)

                If strip.Count > 0 Then
                    Continue For
                End If

                For Each page As XElement In control.Elements("page")
                    Call strip.NewTab(
                        If(CStr(page.Attribute("title")), "page"),
                        page,
                        CStr(page.Attribute("favicon")),
                        id:=CStr(page.Attribute("id")))
                Next

                If strip.Count = 0 Then
                    Call strip.NewTab("welcome", Nothing)
                End If
            Next
        End Sub

        ''' <summary>
        ''' The tab strips of this ui, keyed by the id of their
        ''' ``&lt;tabcontrol&gt;`` element.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TabStrips As IReadOnlyDictionary(Of String, TabStrip)
            Get
                Return strips
            End Get
        End Property

        ''' <summary>
        ''' The boxes of the ``&lt;tabcontrol&gt;`` elements, keyed by their id:
        ''' they provide the rectangle of the component on the canvas.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TabControlBoxes As IReadOnlyDictionary(Of String, UiBox)
            Get
                Return tabBoxes
            End Get
        End Property

        Private ReadOnly strips As New Dictionary(Of String, TabStrip)()
        Private ReadOnly tabBoxes As New Dictionary(Of String, UiBox)()
        Private stripsBuilt As Boolean = False

        ''' <summary>
        ''' Gets (or creates) the cached view object of the given css box.
        ''' </summary>
        ''' <param name="box"></param>
        ''' <returns></returns>
        Public Function GetView(box As CssBox) As UiBox
            If views.ContainsKey(box) Then
                Return views(box)
            End If

            Dim tag As String = If(box.HtmlTag Is Nothing, "", box.HtmlTag.TagName)

            If String.IsNullOrEmpty(tag) Then
                tag = "text"
            End If

            Dim view As New UiBox(box, tag.ToLower(), views.Count)

            Call initState(view)

            views(box) = view

            Return view
        End Function

        ''' <summary>
        ''' Reads the initial state of an input control out of its declaration.
        ''' </summary>
        ''' <param name="view"></param>
        ''' <remarks>
        ''' The state is only initialized once, so a value that has been typed
        ''' into a text input control by the user is not overwritten by a
        ''' relayout of the user interface.
        ''' </remarks>
        Private Shared Sub initState(view As UiBox)
            If view.IsTextInput Then
                view.Value = If(view.Source.GetAttribute("value"), "")
                view.Caret = view.Value.Length
            ElseIf view.IsCheckable Then
                Dim flag As String = view.Source.GetAttribute("checked")

                view.Checked = Not String.IsNullOrEmpty(flag) AndAlso
                    Not (flag = "false" OrElse flag = "0")
            End If
        End Sub

        ''' <summary>
        ''' Finds the control with the given ``id`` attribute.
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns>
        ''' nothing is returned when the declaration does not contain an element
        ''' with such an id, or when the layout has not been calculated yet.
        ''' </returns>
        Public Function FindById(id As String) As UiBox
            If String.IsNullOrEmpty(id) Then
                Return Nothing
            End If

            For Each box As UiBox In paintOrder
                If box.Attribute("id") = id Then
                    Return box
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' The view objects of the top level elements of the ui declaration.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TopLevel As IReadOnlyList(Of UiBox)
            Get
                Return roots
            End Get
        End Property

        ''' <summary>
        ''' Every control of the ui, sorted by their paint order (the z-index
        ''' first and the document order second).
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Boxes As IReadOnlyList(Of UiBox)
            Get
                Return paintOrder
            End Get
        End Property

        ''' <summary>
        ''' Finds the view object of the css box that contains the given point.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns>
        ''' the topmost interactive box that covers the point, or nothing when
        ''' the point is not on a control.
        ''' </returns>
        Public Function HitTest(x As Integer, y As Integer) As UiBox
            ' the paint order is sorted from the bottom to the top, so the
            ' topmost box is found by walking the list backwards
            For i As Integer = paintOrder.Count - 1 To 0 Step -1
                Dim box As UiBox = paintOrder(i)

                If Not box.IsInteractive Then
                    Continue For
                End If

                If box.Bounds.Contains(x, y) Then
                    Return box
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' Marks every control as not hovered and not pressed.
        ''' </summary>
        Public Sub ClearState()
            For Each box As UiBox In views.Values
                box.Hover = False
                box.Pressed = False
            Next
        End Sub
    End Class
End Namespace
