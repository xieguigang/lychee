Imports System.Drawing
Imports System.Xml.Linq
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.MIME.Html.Render.CSS

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
                Dim box As CssBox = root

                ' the root element of the ui declaration is the first child box
                ' of the initial container
                If box.Boxes IsNot Nothing AndAlso box.Boxes.Count > 0 Then
                    box = box.Boxes(0)
                End If

                Return box.ActualBackgroundColor
            End Get
        End Property

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

        Sub New(ui As XElement)
            root = New InitialContainer(ui)
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
            If viewport.Width <= 0 OrElse viewport.Height <= 0 Then
                Return paintOrder
            End If

            Call ApplyViewport(viewport)
            Call root.SetBounds(New RectangleF(0, 0, viewport.Width, viewport.Height))
            Call root.MeasureBounds(g)

            roots.Clear()
            paintOrder.Clear()

            Call walk(root, depth:=0)

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
                End If

                paintOrder.Add(view)

                Call walk(child, depth + 1)
            Next
        End Sub

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

            views(box) = view

            Return view
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
