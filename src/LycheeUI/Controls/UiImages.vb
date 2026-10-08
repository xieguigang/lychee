Imports System.IO
Imports Microsoft.VisualBasic.Imaging
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap

Namespace Controls

    ''' <summary>
    ''' Loads and caches the images of the ``img`` elements of a user
    ''' interface declaration.
    ''' </summary>
    ''' <remarks>
    ''' The raster image of the imaging namespace decodes a png or a jpeg file
    ''' only after a raster image loader has been registered, the ui engine
    ''' registers the gdi+ one at its startup (see
    ''' <see cref="FormRender"/>), a bmp file is decoded by the built in reader
    ''' of the core library in any case.
    ''' </remarks>
    Public Module UiImages

        Private ReadOnly cache As New Dictionary(Of String, Image)()
        Private ReadOnly failures As New HashSet(Of String)()

        ''' <summary>
        ''' The number of the images that are cached at here.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Count As Integer
            Get
                Return cache.Count
            End Get
        End Property

        ''' <summary>
        ''' Gets the image of the given source, the image is decoded only once
        ''' and is then kept in a process wide cache.
        ''' </summary>
        ''' <param name="src">
        ''' a file path, it may be an absolute one or a path that is relative to
        ''' the base directory of the application.
        ''' </param>
        ''' <returns>
        ''' nothing is returned when the source is empty or when the file can
        ''' not be decoded.
        ''' </returns>
        Public Function GetOrLoad(src As String) As Image
            If String.IsNullOrEmpty(src) Then
                Return Nothing
            End If

            src = src.Trim()

            If cache.ContainsKey(src) Then
                Return cache(src)
            End If
            If failures.Contains(src) Then
                Return Nothing
            End If

            Dim path As String = Resolve(src)
            Dim image As Image = Nothing

            If path IsNot Nothing Then
                Try
                    image = Image.FromFile(path)
                Catch ex As Exception
                    image = Nothing
                End Try
            End If

            If image Is Nothing Then
                failures.Add(src)
                Call Console.WriteLine($"[lychee] the image '{src}' can not be loaded.")
                Return Nothing
            End If

            ' the decoded image may be a wrapper of an external raster engine,
            ' it is copied into a plain bitmap buffer at here so that the
            ' directx canvas can read the pixels of it on every frame without
            ' locking an external bitmap again and again
            Try
                image = New Bitmap(image)
            Catch ex As Exception
                ' the original image is kept when the copy fails
            End Try

            cache(src) = image

            Return image
        End Function

        ''' <summary>
        ''' Resolves the given source into an existing file path.
        ''' </summary>
        ''' <param name="src"></param>
        ''' <returns>nothing when no such file exists.</returns>
        Public Function Resolve(src As String) As String
            If String.IsNullOrEmpty(src) Then
                Return Nothing
            End If

            If File.Exists(src) Then
                Return src
            End If

            ' a relative path is resolved against the directory of the
            ' executable instead of the current working directory, which may
            ' have been changed by the host application
            Dim base As String = AppDomain.CurrentDomain.BaseDirectory
            Dim relative As String = Path.Combine(base, src.TrimStart("."c, "/"c, "\"c))

            If File.Exists(relative) Then
                Return relative
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' Drops every cached image, so that the next frame decodes the image
        ''' files again.
        ''' </summary>
        Public Sub Clear()
            cache.Clear()
            failures.Clear()
        End Sub
    End Module
End Namespace
