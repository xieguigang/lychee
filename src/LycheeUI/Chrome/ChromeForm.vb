Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports LycheeUI.Chrome

''' <summary>
''' A windows forms form whose frame is not painted any more while every
''' native behaviour of it is preserved.
''' </summary>
''' <remarks>
''' <para>
''' The visible frame is removed by handling <c>WM_NCCALCSIZE</c>: the client
''' rectangle is made as large as the window rectangle, so the ui engine paints
''' the whole window, while the non client area itself is kept alive and the
''' dwm therefore keeps drawing the real window shadow, the resize borders and
''' the aero snap.
''' </para>
''' <para>
''' This is why the <see cref="Form.FormBorderStyle"/> of such a form must stay
''' <see cref="FormBorderStyle.Sizable"/>: setting it to <c>None</c> would drop
''' the shadow, the resize borders and the snap at once.
''' </para>
''' </remarks>
Public Class ChromeForm : Inherits Form

    Private captionRegionValue As Rectangle = Rectangle.Empty
    Private chromeReady As Boolean = False

    ''' <summary>
    ''' The region of the client area that behaves like a title bar: dragging it
    ''' moves the window and a double click on it toggles the maximized state.
    ''' </summary>
    ''' <remarks>
    ''' The region is expressed in client coordinates and is filled in by the ui
    ''' engine after every layout, the empty rectangle disables the caption.
    ''' </remarks>
    ''' <returns></returns>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    <Browsable(False)>
    Public Property CaptionRegion As Rectangle
        Get
            Return captionRegionValue
        End Get
        Set
            captionRegionValue = Value
        End Set
    End Property

    ''' <summary>
    ''' The width of the border that reacts on a resize drag, zero uses the
    ''' system frame width.
    ''' </summary>
    ''' <returns></returns>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ResizeBorderWidth As Integer = 0

    Public Sub New()
        ' the frame has to stay resizable and captioned, otherwise the dwm
        ' drops the shadow and the snap, the frame is only hidden by the
        ' WM_NCCALCSIZE handler
        FormBorderStyle = FormBorderStyle.Sizable
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)

        If chromeReady Then
            Return
        End If

        chromeReady = True

        Try
            ' a thin sheet of glass makes the dwm paint the window shadow even
            ' though the visible frame has been removed
            Dim margins As New NativeMethods.MARGINS(0, 0, 1, 0)

            Call NativeMethods.DwmExtendFrameIntoClientArea(Handle, margins)
        Catch ex As Exception
            Call Console.WriteLine("[lychee] the window shadow can not be enabled: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' The window styles of a chrome form: the thick frame and the caption are
    ''' required by the dwm shadow and by the aero snap even though the frame is
    ''' not painted any more.
    ''' </summary>
    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim params As CreateParams = MyBase.CreateParams

            params.Style = params.Style Or
                NativeMethods.WS_THICKFRAME Or
                NativeMethods.WS_CAPTION Or
                NativeMethods.WS_MINIMIZEBOX Or
                NativeMethods.WS_MAXIMIZEBOX

            Return params
        End Get
    End Property

    ''' <summary>
    ''' Is the given client point inside of the border that resizes the window?
    ''' </summary>
    Private Function ResizeHit(p As Point) As Integer
        If WindowState <> FormWindowState.Normal Then
            Return NativeMethods.HTCLIENT
        End If

        Dim border As Integer = If(ResizeBorderWidth > 0, ResizeBorderWidth, NativeMethods.FrameWidth())
        Dim onLeft As Boolean = p.X <= border
        Dim onRight As Boolean = p.X >= ClientSize.Width - border
        Dim onTop As Boolean = p.Y <= border
        Dim onBottom As Boolean = p.Y >= ClientSize.Height - border

        If Not (onLeft OrElse onRight OrElse onTop OrElse onBottom) Then
            Return NativeMethods.HTCLIENT
        End If

        If onTop AndAlso onLeft Then
            Return NativeMethods.HTTOPLEFT
        ElseIf onTop AndAlso onRight Then
            Return NativeMethods.HTTOPRIGHT
        ElseIf onBottom AndAlso onLeft Then
            Return NativeMethods.HTBOTTOMLEFT
        ElseIf onBottom AndAlso onRight Then
            Return NativeMethods.HTBOTTOMRIGHT
        ElseIf onLeft Then
            Return NativeMethods.HTLEFT
        ElseIf onRight Then
            Return NativeMethods.HTRIGHT
        ElseIf onTop Then
            Return NativeMethods.HTTOP
        Else
            Return NativeMethods.HTBOTTOM
        End If
    End Function

    ''' <summary>
    ''' Does the given client point belong to the custom title bar?
    ''' </summary>
    Private Function IsCaption(p As Point) As Boolean
        Return Not captionRegionValue.IsEmpty AndAlso captionRegionValue.Contains(p)
    End Function

    Protected Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case NativeMethods.WM_NCCALCSIZE
                If m.WParam <> IntPtr.Zero Then
                    ' removing the visible frame is done by making the client
                    ' rectangle as large as the window rectangle, the non
                    ' client area itself is kept alive
                    If WindowState = FormWindowState.Maximized Then
                        ' a maximized window extends beyond the screen by the
                        ' size of the frame, the content would be cut off, so
                        ' the frame is added back at here
                        Dim frameX As Integer = NativeMethods.FrameWidth()
                        Dim frameY As Integer = NativeMethods.FrameHeight()
                        Dim rect As NativeMethods.RECT =
                            Marshal.PtrToStructure(m.LParam, GetType(NativeMethods.RECT))

                        rect.Left += frameX
                        rect.Top += frameY
                        rect.Right -= frameX
                        rect.Bottom -= frameY

                        Call Marshal.StructureToPtr(rect, m.LParam, False)
                    End If

                    m.Result = IntPtr.Zero
                    Return
                End If

            Case NativeMethods.WM_NCHITTEST
                Call MyBase.WndProc(m)

                If m.Result.ToInt32() = NativeMethods.HTCLIENT Then
                    Dim screen As New Point(m.LParam.ToInt32() And &HFFFF,
                                            (m.LParam.ToInt32() >> 16) And &HFFFF)
                    Dim p As Point = PointToClient(screen)
                    Dim hit As Integer = ResizeHit(p)

                    If hit = NativeMethods.HTCLIENT AndAlso IsCaption(p) Then
                        hit = NativeMethods.HTCAPTION
                    End If

                    If hit <> NativeMethods.HTCLIENT Then
                        m.Result = New IntPtr(hit)
                    End If
                End If

                Return
        End Select

        Call MyBase.WndProc(m)
    End Sub
End Class
