Imports System.Runtime.InteropServices

Namespace Chrome

    ''' <summary>
    ''' The win32 declarations that are required by the custom window chrome of
    ''' the ui engine.
    ''' </summary>
    Friend Module NativeMethods

        ' window messages
        Friend Const WM_NCCALCSIZE As Integer = &H83
        Friend Const WM_NCHITTEST As Integer = &H84
        Friend Const WM_NCLBUTTONDOWN As Integer = &HA1
        Friend Const WM_NCLBUTTONDBLCLK As Integer = &HA3

        ' the return values of WM_NCHITTEST
        Friend Const HTCLIENT As Integer = 1
        Friend Const HTCAPTION As Integer = 2
        Friend Const HTLEFT As Integer = 10
        Friend Const HTRIGHT As Integer = 11
        Friend Const HTTOP As Integer = 12
        Friend Const HTTOPLEFT As Integer = 13
        Friend Const HTTOPRIGHT As Integer = 14
        Friend Const HTBOTTOM As Integer = 15
        Friend Const HTBOTTOMLEFT As Integer = 16
        Friend Const HTBOTTOMRIGHT As Integer = 17

        ' the system metrics of the resizable frame
        Friend Const SM_CXFRAME As Integer = 32
        Friend Const SM_CYFRAME As Integer = 33
        Friend Const SM_CXPADDEDBORDER As Integer = 92

        ' the window styles that keep the dwm shadow and the aero snap alive
        Friend Const WS_CAPTION As Integer = &HC00000
        Friend Const WS_THICKFRAME As Integer = &H40000
        Friend Const WS_MINIMIZEBOX As Integer = &H20000
        Friend Const WS_MAXIMIZEBOX As Integer = &H10000

        <StructLayout(LayoutKind.Sequential)>
        Friend Structure RECT
            Public Left As Integer
            Public Top As Integer
            Public Right As Integer
            Public Bottom As Integer
        End Structure

        <DllImport("user32.dll")>
        Friend Function ReleaseCapture() As Boolean
        End Function

        <DllImport("user32.dll")>
        Friend Function SendMessage(hWnd As IntPtr, msg As Integer,
                                    wParam As IntPtr, lParam As IntPtr) As IntPtr
        End Function

        <DllImport("user32.dll")>
        Friend Function GetSystemMetrics(nIndex As Integer) As Integer
        End Function

        <DllImport("dwmapi.dll")>
        Friend Function DwmExtendFrameIntoClientArea(hWnd As IntPtr, ByRef margins As MARGINS) As Integer
        End Function

        ''' <summary>
        ''' The frame margins that are passed to
        ''' <see cref="DwmExtendFrameIntoClientArea"/>: a one pixel sheet of glass
        ''' is enough to make the dwm paint the window shadow.
        ''' </summary>
        <StructLayout(LayoutKind.Sequential)>
        Friend Structure MARGINS
            Public Left As Integer
            Public Right As Integer
            Public Top As Integer
            Public Bottom As Integer

            Sub New(l As Integer, r As Integer, t As Integer, b As Integer)
                Left = l : Right = r : Top = t : Bottom = b
            End Sub
        End Structure

        ''' <summary>
        ''' The width of the resizable frame of a window in pixels, it includes
        ''' the padding that windows adds around it.
        ''' </summary>
        ''' <returns></returns>
        Friend Function FrameWidth() As Integer
            Return GetSystemMetrics(SM_CXFRAME) + GetSystemMetrics(SM_CXPADDEDBORDER)
        End Function

        ''' <summary>
        ''' The height of the resizable frame of a window in pixels.
        ''' </summary>
        ''' <returns></returns>
        Friend Function FrameHeight() As Integer
            Return GetSystemMetrics(SM_CYFRAME) + GetSystemMetrics(SM_CXPADDEDBORDER)
        End Function

        ''' <summary>
        ''' Starts the system move loop of the given window: the window follows
        ''' the mouse until the mouse button is released, exactly like a native
        ''' title bar drag would do, and the aero snap still works.
        ''' </summary>
        ''' <param name="handle">the handle of the top level window.</param>
        Friend Sub DragWindow(handle As IntPtr)
            Try
                Call ReleaseCapture()
                Call SendMessage(handle, WM_NCLBUTTONDOWN, New IntPtr(HTCAPTION), IntPtr.Zero)
            Catch ex As Exception
                Call Console.WriteLine("[lychee] the window can not be dragged: " & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' Starts the system size loop of the given window from the edge that is
        ''' described by the given hit test code.
        ''' </summary>
        ''' <param name="handle"></param>
        ''' <param name="hit">one of the HTLEFT/HTRIGHT/HTTOP/... codes.</param>
        Friend Sub ResizeWindow(handle As IntPtr, hit As Integer)
            Try
                Call ReleaseCapture()
                Call SendMessage(handle, WM_NCLBUTTONDOWN, New IntPtr(hit), IntPtr.Zero)
            Catch ex As Exception
                Call Console.WriteLine("[lychee] the window can not be resized: " & ex.Message)
            End Try
        End Sub
    End Module
End Namespace
