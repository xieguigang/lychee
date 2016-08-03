---
title: Dependency Injection In Programming
tags: [vb.net, 架构, CodeProject, interface]
date: 2016-06-25
---

## What is Dependency Injection ?

Dependency injection mean **if your class depend on another class or another external login so you will need to create object of this dependent class to use its logic in your designed class.**

For example:

> If you want to design laptop class that use another class called electric power that supply your laptop class by power to run.

So you will use electric power object in laptop class (**Not Good Design**) Like this:

```vbnet
Public Class Laptop

    Dim _electricPowerObj As ElectricPower

    Public Sub New()
        _electricPowerObj = New ElectricPower()
    End Sub

    Public Sub Start()
        ' any logic
    End Sub
End Class
```

<!--more-->

The problems of this designed if you want to change and test and Power source Object you must change constructor method and other variables.
The solution of this problem is using ``Dependency Injection Design Pattern`` that allow you to inject your dependencies as parameters to method and if you want to change dependencies only simply change passed or injected parameter:

```vbnet
Public Interface IPowerSource
    Sub SupplyPower()
End Interface

Public Class ElectricPower : Implements IPowerSource

    Public Sub SupplyPower() Implements IPowerSource.SupplyPower
        ' Any login to supply power
    End Sub
End Class

Public Class Laptop

    Dim _PowerObj As IPowerSource

    Public Sub New(power As IPowerSource)
        _PowerObj = power
    End Sub

    Public Sub Start()
        ' any logic
    End Sub
End Class

Public Module Program

    Public Sub Main ()
        Dim powersourceObj As New ElectricPower()
        Dim laptopObj As New Laptop(powersourceObj)

        Call laptopObj.Start()
    End Sub
End Module
```

This model is useful if alter there are another power sources solutions like power source from gas engine. Simply you can change laptop power source like:

```vbnet
Public Class ElectricPowerFromEngine : Implements IPowerSource

    Public Sub SupplyPower() Implements IPowerSource.SupplyPower
        ' any login to supply power
    End Sub
End Class

Public Module Program

    Public Sub Main()
        Dim powersourceObj As New ElectricPowerFromEngine()
        Dim laptopObj As New Laptop(powersourceObj)

        Call laptopObj.Start()
    End Sub
End Module
```

Without any changes in laptop class, just inject another object.

## Why Dependency Injection?
Dependency Injection is useful because it make your program easy to main and testable

## Inversion of Control?
When you use dependency injection in your program, this mean you inverse control instead of class hard depend on its dependencies, no dependencies injected to class.

## What is automatic dependency injection?
There are many tools that make dependency injection automatic and enable to create IOC(Inversion of Control) Container to register your all dependencies objects and its resolve in one place. These tools like ``Unity``, ``AutoFac``.

> source [Ahmed Abd EL-Latif](http://www.codeproject.com/Tips/1108797/Dependency-Injection-In-Programming), 25 Jun 2016