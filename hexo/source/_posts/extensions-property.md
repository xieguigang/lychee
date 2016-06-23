---
title: Extension Property in VisualBasic
tags: [vb.net, architecture, language, circos, 架构设计]
date: 2016.06.23
---

``Extension`` methods (``Function`` or ``Sub``) are a powerful tool to extend an otherwise sealed object. It's easier than creating a ``Class`` extension because you do not need to update your (maybe already deployed) code, e.g. change ``Dim nice As SomeClass`` to ``Dim nice As SomeClassExtendedAndNicer``. Obviously, different situations call for different solutions and you need an extended Class, but I prefer extension methods if I can solve the situation with those. One big huge drawback with extension methods is that you cannot write extension properties! See on [MSDN here](https://msdn.microsoft.com/en-us/library/bb384936.aspx).

In this tip, I show how to use the ``Extension`` Property in vb.net Class Object for a proxy of an extension property or AlmostExtensionProperty. This tip was inspired by Mr. veen_rp's article: [An Almost Extension Property](http://www.codeproject.com/Articles/1087921/An-Almost-Extension-Property). Instead of by using the ``tag`` property on the Control, and this trick is works on the ``Extension`` Property in the VisualBasic class, so that this trick is not only works on the Controls.

<!--more-->

## VisualBasic Class

First of all, let's see how does the basically class object was defined in the VisualBasic language:

```vbnet
Namespace Microsoft.VisualBasic.Language

    ''' <summary>
    ''' The base class object in VisualBasic language
    ''' </summary>
    Public Class ClassObject

        ''' <summary>
        ''' The extension property.(为了节省内存的需要，这个附加属性尽量不要被自动初始化)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' Dummy field for solve the problem of xml serialization >>>simpleContent&lt;&lt;&lt;
        '''
        ''' http://stackoverflow.com/questions/2501466/xmltext-attribute-in-base-class-breakes-serialization
        '''
        ''' So I think you could make it work by adding a dummy property or field that you never use in the LookupItem class.
        ''' If you're never assign a value to it, it will remain null and will not be serialized, but it will prevent your
        ''' class from being treated as simpleContent. I know it's a dirty workaround, but I see no other easy way...
        ''' </remarks>
        <XmlIgnore> <ScriptIgnore> Public Overridable Property Extension As ExtendedProps

        ''' <summary>
        ''' Get dynamics property value.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function ReadProperty(Of T)(name As String) As PropertyValue(Of T)
            Return PropertyValue(Of T).Read(Me, name)
        End Function

        ' ......
    End Class
```

For this part of the class object definition that we can see that, every classobject in the VisualBasic may have a property named ``Extension``, and this property is implemented by using a hash table ``Dictionary(Of String, Object)`` to stores the additional tag data. So that the dynamics extension property value is going to stored at here.

## Property Value

By knowing how this extension property it works, first we look into how to implements a property. From the language of VB6, **the class property is defined by union a get function and a set method**, The property definition in vb.net goes simple, just using the ``Property`` keyword then we can simply define a property in a line of code. But by expands the vb.net class property or viewing the il code or the reflection result, that we can known that the property it is works as the way of VB6: but the difference is that the property get and set using two inline function in vb.net. And the java language is also does in this way, using a ``get`` word prefix named function as the property read method and using a ``set`` word prefix named method as the property write.

So that the extension property in VisualBasic is working as the Class instance Property it does: **using a get function lambda for get custom value, and using a set method lambda for set the custom value.**

```vbnet
Public Class PropertyValue(Of T) : Inherits Value(Of T)

    ReadOnly __get As Func(Of T)
    ReadOnly __set As Action(Of T)

    Public Overrides Property Value As T
        Get
            Return __get()
        End Get
        Set(value As T)
            MyBase.Value = value
            If Not __set Is Nothing Then
                Call __set(value)  ' 因为在初始化的时候会对这个属性赋值，但是set没有被初始化，所以会出错，在这里加了一个if判断来避免空引用的错误
            End If
        End Set
    End Property

    ''' <summary>
    ''' The instance object for this extension property
    ''' </summary>
    ''' <returns></returns>
    Public Property obj As ClassObject

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="[get]">请勿使用<see cref="GetValue"/></param>函数，否则会出现栈空间溢出
    ''' <param name="[set]">请勿使用<see cref="SetValue"/></param>方法，否则会出现栈空间溢出
    Sub New([get] As Func(Of T), [set] As Action(Of T))
        __get = [get]
        __set = [set]
    End Sub

    ''' <summary>
    ''' 默认是将数据写入到基本类型的值之中
    ''' </summary>
    Sub New()
        __get = Function() MyBase.Value
		__set = Sub(v) MyBase.Value = v
	End Sub

    ''' <summary>
    ''' 这个主要是应用于Linq表达式之中，将属性值设置之后返回宿主对象实例
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    Public Function SetValue(value As T) As ClassObject
        Call __set(value)
        Return obj
    End Function

    Public Overloads Shared Narrowing Operator CType(x As PropertyValue(Of T)) As T
        Return x.Value
    End Operator

    Public Overrides Function ToString() As String
        Return Value.GetJson
    End Function

    ' ......
End Class
```

## Get Extension Property

By invoke get or set the value of the extension property, first we should get the extension property definition, as we should gets the ``PropertyInfo`` at first to using the property in the Reflection operation, and here is how to gets the property definition from the ``Extension`` property from the VisualBasic class object.

```vbnet
Public Shared Function [New](Of Cls As ClassObject)(x As Cls, name As String) As PropertyValue(Of T)
    Dim value As New PropertyValue(Of T)()
    x.Extension.DynamicHash.Value(name) = value
    value.obj = x
    Return value
End Function

''' <summary>
''' 读取<see cref="ClassObject"/>对象之中的一个拓展属性
''' </summary>
''' <typeparam name="Cls"></typeparam>
''' <param name="x"></param>
''' <param name="name"></param>
''' <returns></returns>
Public Shared Function Read(Of Cls As ClassObject)(x As Cls, name As String) As PropertyValue(Of T)
    If x.Extension Is Nothing Then
        x.Extension = New ExtendedProps
    End If
    Dim prop As Object = x.Extension.DynamicHash(name)
    If prop Is Nothing Then
        prop = PropertyValue(Of T).[New](Of Cls)(x, name)
    End If
    Return DirectCast(prop, PropertyValue(Of T))
End Function
```

Gets the property value definition just very easy, right? we just gets the value from the dictionary by using the property name as the key.

## Extension Property Example

```vbnet
Imports Microsoft.VisualBasic.Language

Public Class Karyotype : Inherits ClassObject
    Implements IKaryotype

    Public Property chrName As String Implements IKaryotype.chrName
    Public Property chrLabel As String
    Public Property start As Integer Implements IKaryotype.start
    Public Property [end] As Integer Implements IKaryotype.end
    Public Property color As String Implements IKaryotype.color

    Public Overrides Function ToString() As String Implements IKaryotype.GetData
        Return $"chr - {chrName} {chrLabel} {start} {[end]} {color}"
    End Function
End Class
```

```vbnet
Public Module KaryotypeExtensions

    ''' <summary>
    ''' nt核苷酸基因组序列拓展属性
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <Extension>
    Public Function nt(x As Karyotype) As PropertyValue(Of FastaToken)
        Return PropertyValue(Of FastaToken).Read(Of Karyotype)(x, NameOf(nt))
    End Function
End Module
```

```vbnet
''' <summary>
''' 使用这个函数进行创建多条染色体的
''' </summary>
''' <param name="source">Band数据</param>
''' <param name="chrs">karyotype数据</param>
''' <returns></returns>
Public Shared Function FromBlastnMappings(source As IEnumerable(Of BlastnMapping), chrs As IEnumerable(Of FastaToken)) As BasicGenomeSkeleton
    Dim ks As Karyotype() =
        LinqAPI.Exec(Of Karyotype) <= From nt As SeqValue(Of FastaToken)
                                      In chrs.SeqIterator
                                      Let name As String = nt.obj.Title.NormalizePathString(True).Replace(" ", "_")
                                      Select New Karyotype With {
                                          .chrName = "chr" & nt.i,
                                          .chrLabel = name,
                                          .color = "",
                                          .start = 0,
                                          .end = nt.obj.Length
                                      }.nt.SetValue(nt.obj).As(Of Karyotype)
     ' ......
End Function
```

And here is another simple example by using this extension property:

Here is the extension property definition:

```vbnet
Public Module ClassAPI

    ''' <summary>
    ''' Example of the extension property in VisualBasic
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Uid(Of T As ClassObject)(x As T) As PropertyValue(Of Long)
        Return PropertyValue(Of Long).Read(Of T)(x, NameOf(Uid))
    End Function
End Module
```

So that by using this extension property, that for get value, we can do as this:

```vbnet
Dim n As Long = x.Uid
```

For set property value, that we can write the code:

```vbnet
Dim n As Long = VBMath.Rnd() * 100000000000L
x.Uid.value = n
```