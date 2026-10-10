Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Xml.Linq
Imports std = System.Math

Namespace Tabs

    ''' <summary>
    ''' The state of a browser like tab strip: it owns the pages and the active
    ''' one of them, and it raises an event for every change, while the painting
    ''' of it is done by the <see cref="TabStripRenderer"/>.
    ''' </summary>
    Public Class TabStrip

        Private ReadOnly tabs As New List(Of UiTab)()
        Private activeIdValue As String = Nothing
        Private welcomeValue As XElement = Nothing

        ''' <summary>
        ''' Raised after a new tab has been appended and activated.
        ''' </summary>
        Public Event TabCreated(tab As UiTab)

        ''' <summary>
        ''' Raised after another tab has become the active one.
        ''' </summary>
        Public Event TabActivated(tab As UiTab)

        ''' <summary>
        ''' Raised after a tab has been removed from the strip.
        ''' </summary>
        Public Event TabClosed(tab As UiTab)

        ''' <summary>
        ''' The ui declaration that is shown when every tab has been closed.
        ''' </summary>
        ''' <returns></returns>
        Public Property WelcomeContent As XElement
            Get
                Return welcomeValue
            End Get
            Set
                welcomeValue = Value
            End Set
        End Property

        ''' <summary>
        ''' The minimum width of a tab: the tabs shrink when there are too many
        ''' of them but they never get narrower than this.
        ''' </summary>
        ''' <returns></returns>
        Public Property TabMinWidth As Single = 60.0F

        ''' <summary>
        ''' The preferred width of a tab.
        ''' </summary>
        ''' <returns></returns>
        Public Property TabMaxWidth As Single = 200.0F

        ''' <summary>
        ''' The height of the tab strip.
        ''' </summary>
        ''' <returns></returns>
        Public Property StripHeight As Single = 34.0F

        ''' <summary>
        ''' The background color of the content area, the active tab is painted
        ''' with the very same color so that both of them look like one piece.
        ''' </summary>
        ''' <returns></returns>
        Public Property ContentColor As System.Drawing.Color = System.Drawing.Color.FromArgb(32, 32, 32)

        ''' <summary>
        ''' The pages of this strip, in their current order.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TabList As IReadOnlyList(Of UiTab)
            Get
                Return tabs
            End Get
        End Property

        ''' <summary>
        ''' The number of the pages.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Count As Integer
            Get
                Return tabs.Count
            End Get
        End Property

        ''' <summary>
        ''' The id of the active page, nothing when every page has been closed.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ActiveId As String
            Get
                Return activeIdValue
            End Get
        End Property

        ''' <summary>
        ''' The active page, nothing when every page has been closed.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ActiveTab As UiTab
            Get
                Return FindById(activeIdValue)
            End Get
        End Property

        ''' <summary>
        ''' Is there no page left on this strip?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return tabs.Count = 0
            End Get
        End Property

        ''' <summary>
        ''' Appends a new page and makes it the active one.
        ''' </summary>
        ''' <param name="title"></param>
        ''' <param name="content"></param>
        ''' <param name="favicon"></param>
        ''' <param name="host"></param>
        ''' <param name="id"></param>
        ''' <returns></returns>
        Public Function NewTab(title As String, content As XElement,
                               Optional favicon As String = Nothing,
                               Optional host As Control = Nothing,
                               Optional id As String = Nothing) As UiTab

            Dim tab As New UiTab(id, title, content, favicon, host)

            tabs.Add(tab)
            Call Activate(tab.Id)
            RaiseEvent TabCreated(tab)

            Return tab
        End Function

        ''' <summary>
        ''' Removes the tab with the given id. when the closed tab was the active
        ''' one, the tab at its right becomes the active one, or the tab at its
        ''' left when there is no right neighbour.
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns>
        ''' true when a tab has been removed, false when no tab owns such an id.
        ''' </returns>
        Public Function CloseTab(id As String) As Boolean
            If String.IsNullOrEmpty(id) Then
                Return False
            End If

            Dim index As Integer = IndexOf(id)

            If index < 0 Then
                Return False
            End If

            Dim closed As UiTab = tabs(index)
            Dim wasActive As Boolean = (activeIdValue = id)

            tabs.RemoveAt(index)

            If wasActive Then
                If tabs.Count = 0 Then
                    activeIdValue = Nothing
                ElseIf index < tabs.Count Then
                    activeIdValue = tabs(index).Id
                Else
                    activeIdValue = tabs(tabs.Count - 1).Id
                End If
            End If

            RaiseEvent TabClosed(closed)

            If wasActive AndAlso activeIdValue IsNot Nothing Then
                RaiseEvent TabActivated(FindById(activeIdValue))
            End If

            Return True
        End Function

        ''' <summary>
        ''' Makes the tab with the given id the active one.
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns>true when such a tab exists.</returns>
        Public Function Activate(id As String) As Boolean
            If String.IsNullOrEmpty(id) OrElse FindById(id) Is Nothing Then
                Return False
            End If

            If activeIdValue = id Then
                Return True
            End If

            activeIdValue = id
            RaiseEvent TabActivated(FindById(id))

            Return True
        End Function

        ''' <summary>
        ''' Moves a tab to another position inside of the strip, it is used by
        ''' the drag and drop reordering of the tabs.
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="index">the target index, it is clamped.</param>
        ''' <returns>true when the tab has been moved.</returns>
        Public Function MoveTab(id As String, index As Integer) As Boolean
            Dim from As Integer = IndexOf(id)

            If from < 0 Then
                Return False
            End If

            Dim target As Integer = std.Max(0, std.Min(index, tabs.Count - 1))

            If target = from Then
                Return False
            End If

            Dim tab As UiTab = tabs(from)

            tabs.RemoveAt(from)
            tabs.Insert(target, tab)

            Return True
        End Function

        ''' <summary>
        ''' Activates the neighbour of the given tab, it is used when the active
        ''' tab is closed and the caller wants to keep a page on the screen.
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        Public Function ActivateNeighbourOf(id As String) As Boolean
            Dim index As Integer = IndexOf(id)

            If index < 0 Then
                Return False
            End If

            If index + 1 < tabs.Count Then
                Return Activate(tabs(index + 1).Id)
            ElseIf index > 0 Then
                Return Activate(tabs(index - 1).Id)
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Finds the tab with the given id.
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        Public Function FindById(id As String) As UiTab
            If String.IsNullOrEmpty(id) Then
                Return Nothing
            End If

            For Each tab As UiTab In tabs
                If tab.Id = id Then
                    Return tab
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' The index of the tab with the given id, -1 when there is no such tab.
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        Public Function IndexOf(id As String) As Integer
            If String.IsNullOrEmpty(id) Then
                Return -1
            End If

            For i As Integer = 0 To tabs.Count - 1
                If tabs(i).Id = id Then
                    Return i
                End If
            Next

            Return -1
        End Function
    End Class
End Namespace
