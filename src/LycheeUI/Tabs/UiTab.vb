Imports System.Xml.Linq
Imports LycheeUI.Layout

Namespace Tabs

    ''' <summary>
    ''' One page of a <see cref="TabStrip"/>.
    ''' </summary>
    Public Class UiTab

        ''' <summary>
        ''' The unique identifier of the tab, it never changes.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Id As String

        ''' <summary>
        ''' The title that is drawn on the tab.
        ''' </summary>
        ''' <returns></returns>
        Public Property Title As String

        ''' <summary>
        ''' The image source of the favicon, it is resolved by the
        ''' <see cref="UiImages"/> loader; a favicon that can not be loaded is
        ''' replaced by the first letter of the title.
        ''' </summary>
        ''' <returns></returns>
        Public Property Favicon As String

        ''' <summary>
        ''' The ui declaration of the page content.
        ''' </summary>
        ''' <returns></returns>
        Public Property Content As XElement

        ''' <summary>
        ''' An existing windows forms control that should be displayed inside of
        ''' the content area while this tab is active.
        ''' </summary>
        ''' <returns></returns>
        Public Property Host As Control

        ''' <summary>
        ''' The layout engine of this page: every tab owns one, so the state of
        ''' the input controls of a page survives a switch to another tab.
        ''' </summary>
        ''' <returns></returns>
        Private engine As UiLayoutEngine
        Private engineBuilt As Boolean = False

        Sub New(id As String, title As String, content As XElement,
                Optional favicon As String = Nothing, Optional host As Control = Nothing)

            Me.Id = If(String.IsNullOrEmpty(id), Guid.NewGuid().ToString("N"), id)
            Me.Title = If(title, id)
            Me.Content = content
            Me.Favicon = favicon
            Me.Host = host
        End Sub

        ''' <summary>
        ''' The layout engine of this page.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Layout As UiLayoutEngine
            Get
                If Not engineBuilt Then
                    engineBuilt = True

                    If Content IsNot Nothing Then
                        Try
                            engine = New UiLayoutEngine(Content)
                        Catch ex As Exception
                            Call Console.WriteLine($"[lychee] the page '{Title}' can not be parsed: {ex.Message}")
                            engine = Nothing
                        End Try
                    End If
                End If

                Return engine
            End Get
        End Property

        ''' <summary>
        ''' Does this page own a windows forms control that has to be displayed?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HasHost As Boolean
            Get
                Return Host IsNot Nothing
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{Id}] {Title}"
        End Function
    End Class
End Namespace
