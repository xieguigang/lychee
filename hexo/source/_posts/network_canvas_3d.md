---
title: Network canvas in 3D
tags: [vb.net, SVG, gdi+, 3D]
date: 2016-8-7
---

![3D network in movie '007 Skyfall (2012)'](https://github.com/xieguigang/xieguigang.github.io-hexo/raw/master/images/3D_rotation/daa-skayfoll-1080p.mkv_005301660.jpg)

In this , there are two problem:

+ 3D rotation
+ 3D Projection

<!--more-->

### Background: 3D rotation

> [3D graphics tutorial - Rotating objects](https://petercollingridge.appspot.com/3D-tutorial/rotating-objects)

Rotating things in three dimensions sounds complicated and it can be, but there are some simple rotations. For example, if we imagine rotating our cube around the z-axis (which points out of the screen), we are actually just rotating a square in two dimensions.

![A cube rotating about its z-axis.](https://github.com/xieguigang/xieguigang.github.io-hexo/raw/master/images/3D_rotation/rotating_square.gif)

We can simplify things further, by just looking at a single node at position ``(x, 0)``. Using simple trigonometry we can find that the position of the point after rotating it by ``θ`` around the origin is ``(x', y')``, where,

```
x' = x × cos(θ)
y' = x × sin(θ)
```

![Rotating a point (x, 0) around the origin by θ.](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/3D_rotation/rotate_a_point.png)

##### Rotating a point about the origin
The example above allows us to rotate a point that starts on the x-axis about the origin, but what if it isn't on the x-axis? This requires some slightly more advanced trigonometry. If we call the distance between the point ``(x, y)`` and the origin ``r``, and the angle between the line to ``(x, y)`` and x-axis, ``α`` then,

```
x = r × cos(α)
y = r × sin(α)
```

If we rotate by β to point ``(x', y')``, then,

```
x' = r × cos(α + β)
y' = r × sin(α + β)
```

Using the trigonometric addition equations (derived here and here), we get,

```
x' = r × cos(α) cos(β) - r × sin(α) sin(β)
y' = r × sin(α) cos(β) + r × cos(α) sin(β)
```

Substituting in the values for x and y above, we get an equation for the new coordinates as a function of the old coordinates and the angle of rotation:

```
x' = x × cos(β) - y × sin(β)
y' = y × cos(β) + x × sin(β)
```

![Rotating a point (x, y) around the origin by β.](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/3D_rotation/rotate_a_point2.png)

Here is a list of articles that you can found more detail information about the 3D rotation mathematics:

+ https://en.wikipedia.org/wiki/Rotation_matrix
+ http://www.fastgraph.com/makegames/3drotation/
+ http://codentronix.com/2011/05/25/rotating-solid-cube-using-vb-net-and-gdi/
+ http://blog.xieguigang.me/2016/06/10/3d_rotation/

### The Point3D object
The theory is much more complicated than you think, but 

Assume that we have a 3D Point object like this:

```vbnet
''' <summary>
''' Defines the Point3D class that represents points in 3D space.
''' Developed by leonelmachava &lt;leonelmachava@gmail.com>
''' http://codentronix.com
'''
''' Copyright (c) 2011 Leonel Machava
''' </summary>
Public Structure Point3D
    Public Property X As Double
    Public Property Y As Double
    Public Property Z As Double
End Structure
```

then we can write to rotation function for the x,y,z axis separately based on the mathematics theory of the 3D rotation:

```vbnet
Public Function RotateX(angle As Double) As Point3D
    Dim rad As Double, cosa As Double, sina As Double, yn As Double, zn As Double

    rad = angle * Math.PI / 180
    cosa = Math.Cos(rad)
    sina = Math.Sin(rad)
    yn = Me.Y * cosa - Me.Z * sina
    zn = Me.Y * sina + Me.Z * cosa
    Return New Point3D(Me.X, yn, zn)
End Function

Public Function RotateY(angle As Double) As Point3D
    Dim rad As Double, cosa As Double, sina As Double, Xn As Double, Zn As Double

    rad = angle * Math.PI / 180
    cosa = Math.Cos(rad)
    sina = Math.Sin(rad)
    Zn = Me.Z * cosa - Me.X * sina
    Xn = Me.Z * sina + Me.X * cosa

    Return New Point3D(Xn, Me.Y, Zn)
End Function

Public Function RotateZ(angle As Double) As Point3D
    Dim rad As Double, cosa As Double, sina As Double, Xn As Double, Yn As Double

    rad = angle * Math.PI / 180
    cosa = Math.Cos(rad)
    sina = Math.Sin(rad)
    Xn = Me.X * cosa - Me.Y * sina
    Yn = Me.X * sina + Me.Y * cosa
    Return New Point3D(Xn, Yn, Me.Z)
End Function
```

Then we can creates a very basic cube model in 3D and makes rotation:

```vbnet

```

![A rotated 3D cube](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/3D_rotation/an_actual_cube.bmp)

### Background: 3D Projection

### Project Point3D to Drawing.Point

```vbnet
''' <summary>
''' Project the 3D point to the 2D screen. By using the projection result,
''' just read the property <see cref="PointXY"/>.
''' (将3D投影为2D，所以只需要取结果之中的<see cref="X"/>和<see cref="Y"/>就行了)
''' </summary>
''' <param name="viewWidth"></param>
''' <param name="viewHeight"></param>
''' <param name="fov">256默认值</param>
''' <param name="viewDistance"></param>
''' <returns></returns>
Public Function Project(viewWidth As Integer,
                        viewHeight As Integer,
                        fov As Integer,
                        viewDistance As Integer) As Point3D

    Dim factor As Double, Xn As Double, Yn As Double
    factor = fov / (viewDistance + Me.Z)
    Xn = Me.X * factor + viewWidth / 2
    Yn = Me.Y * factor + viewHeight / 2
    Return New Point3D(Xn, Yn, Me.Z)
End Function
```

### Network canvas in 3D

![Network canvas in 3D](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/3D_rotation/network_canvas_3D.bmp)

### Additional: Export SVG

