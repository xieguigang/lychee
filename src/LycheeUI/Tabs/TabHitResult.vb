Namespace Tabs

    ''' <summary>
    ''' The kind of the element of a tab strip that has been hit by the mouse.
    ''' </summary>
    Public Enum TabHitKind
        ''' <summary>nothing of the tab strip has been hit</summary>
        None
        ''' <summary>the body of a tab, a click activates it</summary>
        Tab
        ''' <summary>the close button of a tab</summary>
        Close
        ''' <summary>the new tab button at the end of the strip</summary>
        NewTab
        ''' <summary>the empty part of the strip, it drags the window</summary>
        Caption
        ''' <summary>the icon of the window</summary>
        Icon
        ''' <summary>one of the window buttons: minimize, maximize or close</summary>
        WindowButton
    End Enum

    ''' <summary>
    ''' The result of a hit test on a <see cref="TabStrip"/>.
    ''' </summary>
    Public Class TabHitResult

        ''' <summary>
        ''' What has been hit.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Kind As TabHitKind

        ''' <summary>
        ''' The tab that has been hit, it is set for the <see cref="TabHitKind.Tab"/>
        ''' and the <see cref="TabHitKind.Close"/> kinds.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Tab As UiTab

        ''' <summary>
        ''' The index of the hit tab inside of the strip.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Index As Integer

        ''' <summary>
        ''' Which of the window buttons has been hit, it is only set for the
        ''' <see cref="TabHitKind.WindowButton"/> kind: 0 = minimize,
        ''' 1 = maximize, 2 = close.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property WindowButton As Integer

        Public Shared ReadOnly Empty As New TabHitResult(TabHitKind.None)

        Sub New(kind As TabHitKind, Optional tab As UiTab = Nothing,
                Optional index As Integer = -1, Optional windowButton As Integer = -1)

            Me.Kind = kind
            Me.Tab = tab
            Me.Index = index
            Me.WindowButton = windowButton
        End Sub

        Public Shared Widening Operator CType(kind As TabHitKind) As TabHitResult
            Return New TabHitResult(kind)
        End Operator
    End Class
End Namespace
