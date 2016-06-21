---
title: Zplay Music
tags: [libzplay, vb.net, 前端设计, music, works]
date: 2016-06-19
---

Zplay-Music based on libzplay(http://libzplay.sourceforge.net/) kernel
![](https://raw.githubusercontent.com/xieguigang/Zplay-Music/master/manual/Zplay-Music.png)
https://github.com/xieguigang/Zplay-Music

<!--more-->

使用Windows 10的任务栏的高级特性只需要导入命名空间``Microsoft.Windows.Taskbar``就可以轻松使用：

```vbnet
Imports Microsoft.Windows.Taskbar

Public Class cloudMusic

    Dim WithEvents buttonPrevious As ThumbnailToolBarButton
    Dim WithEvents buttonNext As ThumbnailToolBarButton
    Dim WithEvents buttonPause As ThumbnailToolBarButton

    Private Sub cloudMusic_Load(sender As Object, e As EventArgs) Handles Me.Load
        Size = New Size(310, 431)
    End Sub

    Private Sub cloudMusic_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        buttonPrevious = New ThumbnailToolBarButton(My.Resources.start, "Previous")
        buttonNext = New ThumbnailToolBarButton(My.Resources._end, "Next")
        buttonPause = New ThumbnailToolBarButton(My.Resources.pause, "play/pause")

        TaskbarManager.Instance.ThumbnailToolBars.AddButtons(
            Handle,
            buttonPrevious,
            buttonPause,
            buttonNext)
        Call cloudMusic_SizeChanged(Nothing, Nothing)
    End Sub

    Private Sub cloudMusic_SizeChanged(sender As Object, e As EventArgs) Handles Me.SizeChanged
        Dim sz As New Size(90, 90)
        TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip(
            Handle,
            New Rectangle(New Point(), sz))
    End Sub
End Class
```

![](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/c/cloudMusic.png)