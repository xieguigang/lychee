---
title: 梯形法、龙格库塔法求解微分方程 数值解
tags: [ODE, tags, eluer, RK2, RK4, vb.net]
date: 2016-9-10
---

根据积分表达式,微分方程的数值解关键在于微分方程的初值及计算微分方程式在``tm``(上一时刻)与``tm+d``（下一时刻）与坐标轴围成面积，若这个面积计算得越准确则得到的数值解也就越精确。微分表达式中与坐标轴围成的面积可表示如下，再实施算法的时候可以结合这个图更加直观点：

![图1 微分方程中微分表达式中tm与tm+1与坐标中组成阴影面积图](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/ODE_Trapezoidal.png)
<!--more-->
## 1.梯形法求数值解

显然阴影部分的面积的近似计算中，用梯形法近似比用矩形近似更为准确，梯形的斜边用``tm``时刻的``f``的值代替，即原函数在``tm``时刻的导数。即用梯形法的数值解的表达式变为``y(tm+1) = y(tm) + ( f（ y(tm), tm) + f( y1(tm+1), tm+1) ) / 2 * (tm+1 - tm)``

从这个表达式来看，如果要用梯形法求得``y(tm+1)``的值则需要先知道``y1(tm+1)``的值，这个值可以用欧拉法求得，再把用欧拉法求得的下一时刻的数值解带入上式校正一个更为精确的值。如此，梯形法的求解表达式就变为：

```
y1(tm+1) = y(tm) + f((ym, tm)) * (tm+1 - tm)                                  (1)   [欧拉法求解数值解]
y(tm+1)  = y(tm) + ( f(y(tm), tm) + f(y1(tm+1), tm+1) ) / 2 * (tm+1 - tm)     (2)   [梯形法校正数值解]
```

采用程序求解数值解时初值和步长依然是必不可少的。除此之外用另外一个一个变量来存下一刻值也是必须的。用下面的代码表示：

```vbnet
''' <summary>
''' 欧拉法解微分方程，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0,y0)，ODE的结果会从x和y这两个数组指针返回
''' </summary>
''' <param name="n"></param>
''' <param name="a"></param>
''' <param name="b"></param>
<Extension>Public Sub Eluer(ByRef df As ODE, n As Integer, a As Double, b As Double)
    Dim h As Double = (b - a) / n

    df.x = New Double(n - 1) {}
    df.y = New Double(n - 1) {}
    df.x(Scan0) = a
    df.y(Scan0) = df.y0

    Dim x = df.x, y = df.y

    For i As Integer = 1 To n - 1
        x(i) = a + h * i
        y(i) = x(i - 1) + h * df(x(i - 1), y(i - 1))
    Next
End Sub
```

其中``y_tm``表示上一时刻的数值解，``a，b``表示上下限，``h``表示步长值为``(tm+1 - tm),y1_tm1``为用欧拉法求解的下一刻的数值解，``function``表示微分方程表达式，用来求``tm``和``tm+1``时刻的值作为梯形上底和下底值，``y_tm1``为用梯形法获得的数值解，一次计算完毕后，``ytm1``变为上一时刻的数值解，一次求解就能将``[a, b]``之间的数值解求完毕。

## 2.龙格库塔法求数值解
### 2.1 二阶龙格库塔法

二阶龙格库塔法就是梯形法。它求解的表达式为：
```
y(tm+1) = y(tm)  + (k1 + k2) / 2 * h;
k1 = f(y(tm), tm)
k2 = f( y(tm+1), tm+1 )
```

所以求解二阶段的程序段也是一样的。

### 2.2四阶龙格库塔法

二阶龙格库塔法的截断误差正比于步长h的三次方，在计算精度要求比较高的情形下多使用四阶龙格库塔法。其计算公式为

```
y(tm+1) =y(tm)  + (k1 + 2k2 +2k3 + k4) / 6 *h      (1)
k1 =f( y(tm), tm)                                  (2)
k2 =f( y(tm)  + k1 / 2 * h, tm + h / 2)            (3)
k3 =f( y(tm)  + k2 / 2 * h, tm + h / 2)            (4)
k4 =f( y(tm) + h * k3, tm + h)                     (5)
```

其中y(tm+1)表示下一时刻的数值解，y(tm)表示上一时刻数值解，f()表示微分方程表达式。

仔细分析一下这几个式子，看看是怎么来逼近 微分表达式积分段与坐标抽围成的面积的：

``k1 * h``是对阴影面积的一个逼近，欧拉似逼近。
``k2 * h``亦是对阴影面积的一个逼近，``k1 / 2 * h``表示用欧拉法求得的是从当前时刻算起跨步长为``h / 2``的面积，则``y(tm)  + k1 / 2 * h``表示在``tm + h/2``处的数值近似解，则k2代表微分表达式``tm + h / 2``处的值，则``2k2 * h``表示计算了阴影面积的两倍。
``k3 * h`` 更是对阴影面积的逼近，而且比``k2 * h``更为准确，因为``k2 * h``基于欧拉法求得原函数在``tm + h / 2``处的值从而得到微分表达式在``tm + h / 2``处的值，``k3``也是微分表达式在``tm + h / 2``处的值，只是基于k2，更加精确。``2k3 * h``表示2倍的阴影面积。``
k4 * h``是对阴影面积的一个逼近，采用``y(tm) + h * k3``得到原函数``tm+1``时刻的数值解从而求得``k4 = f( y(tm) + h * k3, tm + h)``为微分表达式在``tm + h``时刻的解。则``k4 * h``是表示用``tm+1``时刻的微分表达式值求得的阴影部分面积，也是用矩阵法求得的，属欧拉法。
那么（``k1 + 2k2 +2k3 + k4``）就表示6倍与阴影面积，所以``(k1 + 2k2+2k3 + k4) / 6 * h``就表示求得平均值，数学直觉上讲，这样子的到的平均值往往更趋近于真实值。

若要用程序来表示龙格库塔法，首先初值和步长依然是导火线。然后就根据(1) ~ (5 )式子列就行了：

```vbnet
''' <summary>
''' 四阶龙格库塔法解解微分方程，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0, y0)
''' 参考http://blog.sina.com.cn/s/blog_698c6a6f0100lp4x.html 和维基百科
''' </summary>
''' <param name="df"></param>
''' <param name="n"></param>
''' <param name="a"></param>
''' <param name="b"></param>
<Extension>Public Sub RK4(ByRef df As ODE, n As Integer, a As Double, b As Double)
    Dim h As Double = (b - a) / n

    df.x = New Double(n - 1) {}
    df.y = New Double(n - 1) {}
    df.x(Scan0) = a
    df.y(Scan0) = df.y0

    Dim x = df.x, y = df.y
    Dim k1, k2, k3, k4 As Double

    For i As Integer = 1 To n - 1
        x(i) = a + h * i
        k1 = df(x(i - 1), y(i - 1))
        k2 = df(x(i - 1) + 0.5 * h, y(i - 1) + 0.5 * h * k1)
        k3 = df(x(i - 1) + 0.5 * h, y(i - 1) + 0.5 * h * k2)
        k4 = df(x(i - 1) + h, y(i - 1) + h * k3)
        y(i) = y(i - 1) + h / 6 * (k1 + 2 * k2 + 2 * k3 + k4)
    Next
End Sub
```

``y+t0``为初值常值，``function``为分为表达式，``t - h``表示用欧拉法计算矩形面积（前一时刻的值为矩形高度）。然后求解的过程跟分析的过程相符合。

## 总结

龙格库塔法是求解已知初值微分方程的主要数值算法。它还试用于非线性的微分方程的数值解。如果需要描述求解值的曲线，因为这些值都是很准确的，所有采用曲线拟合或图形界面就能将微分方程解的曲线绘制出来。

> ![梯形法、龙格库塔法求解微分方程 数值解](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/qrcode/ODE_TrapezoidalMethod.png)