---
title: XAML
tags: [XAML, WPF, vb.net, 前端设计, UI]
date: 2016-06-01
---

## XAML定义

``XAML``是一种相对简单、通用的声明式编程语言，它适合于构建和初始化.NET对象。
``XAML``仅仅是一种使用.NET API的方式，把它与HTML、可伸缩向量图形（SVG）或其他特定领域的格式或语言作比较是完全错误的。XAML由一些规则（告诉解析器和编译器如何处理XML）和一些关键字组成，但它自己没有任何有意义的元素。因此，如果在没有WPF这样的框架的基础上讨论XAML，就如同在没有.NET Framework的基础上讨论VisualBasic一样。
``XAML``在WPF中扮演的角色通常是令人困惑的，因此第一件要搞清楚的事情是WPF和XAML可以独立使用，它们并不是互相依赖的。虽然XAML最初是为WPF而设计，但它也可以应用于其他技术（如WF）。由于XAML的通用性，实际上可以把它应用于任何.NET技术。然而，是否在使用WPF时使用XAML是可选的，每一件XAML能做的事情完全可以由任何一种你喜欢的.NET语言来实现（但反过来则不行）。但是，由于XAML的诸多好处，很少会看到现实世界中使用WPF却不使用XAML的情况。

<!--more-->

## 元素和特性

XAML规范定义了一些规则，用于把.NET命名空间、类型、属性和事件映射为XML命名空间、元素和特性。以下面为例，它定义了一个WPF按钮，跟另一段与之功能一致的VisualBasic代码比较一下：

```xml
<Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Content="OK" Click="button_Click"/>
```

```vbnet
Dim b As New System.Window.Controls.Button()
b.Content = "OK"
AddHandler b.Click, AddressOf button_Click
```

虽然这两段代码功能是相同的，但如果去除掉``XAML``中的``Click``特性，你可以很快地在IE浏览器中查看``XAML``，还会看到一个活生生的按钮放在浏览器窗口中。而VisualBasic代码则必须要额外的代码编译方可使用。
在XAML中定义一个XML元素（叫作对象元素）与在.NET中实例化一个对应的对象（总是使用默认的构造函数）是等价的。设置对象元素的一个特性（attribute），与设置一个同名属性（property attribute，称为属性特性）或者为一个同名事件设置一个事件处理程序（也称为事件特性），也是等价的。

## 生成和事件处理的顺序

在运行时（run-time）模式下，为任何一个``XAML``声明的对象设置属性之前，总要添加一些事件处理程序，这样就可以让某个事件在属性被设置时被触发，而不用担心XAML使用特性的顺序。
至于多个属性集或添加多个事件处理程序，它们总会遵照一定顺序，即属性特性和事件属性是在对象元素中指定的。这一排序方式不会在实际应用中产生影响，因为.NET设计指南指出：类应该允许以任何顺序设置属性，添加事件处理程序也是如此。

## 命名空间

比较上述XAML代码示例和相应的VisualBasic代码示例，最神秘的地方在于XAML命名空间（[http://schemas.microsoft.com/winfx/2006/xaml/presentation](http://schemas.microsoft.com/winfx/2006/xaml/presentation)）是如何被映射到.NET命名空间（``System.Windows.Controls``）上的。该映射及其他WPF命名空间的映射是在WPF程序集中硬编码完成的，里面有好几个``Xmlns-DefinitionAttribute``自定义特性的实例。（在``schemas.microsoft.com``这个URL中不存在网页，这仅仅是一个人为设定的字符串，就像其他命名空间一样。）
``XAML``文件的根对象元素属性指定至少一个XML命名空间，用于验证自己和子元素。你可以（在根元素或子元素上）声明额外的XML命名空间，但每一个命名空间下的标识符都必须有一个唯一的前缀。例如，WPF的XAML文件都会使用第二个命名空间加上前缀x（记作``xmlns:x``而不仅仅是``xmlns``）：

```xml
xmlns:x=http://schemas.microsoft.com/winfx/2006/xaml
```

这是XAML语言命名空间，用于映射``System.Windows.Markup``命名空间中的类型，而且它也定义了XAML编译器或解析器中的一些特殊的指令。这些指令通常是作为XAML元素的特性出现的，因此，它们看上去像宿主元素的属性，但实际上并不是如此。
我们把[http://schemas.microfost.com/winfx/2006/xaml/presentation](http://schemas.microfost.com/winfx/2006/xaml/presentation)作为默认（主要）命名空间，把[http://schemas.microsoft.com/winfx/2006-/xaml](http://schemas.microsoft.com/winfx/2006-/xaml)作为次要命名空间。次要命名空间的前缀是``x``，这仅仅是一个规则，就像VisualBasic文件要以``Imports System``指令开始一样。你可以改写原来那个``XAML``文件，含义是相同的：

```xml
<WpfNamespace:Button xmlns:WpfNamespace="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Content="OK"/>
```

当然，从可读性来讲，在使用这些常见的命名空间的时候不需要前缀（即原始的XML命名空间），其他一些命名空间则使用一个短前缀。

## 属性元素

富创建是WPF的亮点之一，我们可以用``Button``来演示。你可以把任意内容放在``Button``里面，不仅限于文本，如下所示（在``Button``中嵌入了一个简单的方形来做一个VCR的停止按钮）：

```vbnet
Dim b As new System.Windows.Controls.Button()
Dim r As new System.Windows.Shapes.Rectangle()

r.Width = 40
r.Height = 40
r.Fill = System.Windows.Media.Brushes.Black
b.Content = r  ' 将按钮中的内容设置为方格
```

``Button``的``Content``属性是``System.Object``类型的，因此它很容易被设置到40*40的``Rectangle``对象。但如何才能在XAML中用属性特性语法做相同的事呢？你该为``Content``属性设置哪种字串才能完成VisualBasic中声明的``Rectangle``功能呢？没有这样的字串，但XAML提供了一种替代的语法来设置复杂的属性值，即属性元素。如下所示：

```xml
<Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
<Button.Content>
    <Rectangle Height="40" Width="40" Fill="Black"/>
</Button.Content>
</Button>
```

``Content``属性被设置为一个XML元素而不是XML特性，``Button.Content``中的句点用于区分对象元素（object element）与属性元素（property element）。它们总会以“**类型名，属性名TypeName.PropertyName**”的形式出现，总会包含在“类型名”对象元素中，但它们没有属于自己的特性。
属性元素语法也可以用于简单的属性值。下面的Button使用特性设置了两个属性，它们是``Content``和``Background``：

```xml
<Button xmlns="http://schema.microsoft.com/winfx/2006/xaml/presentation" Content="OK" Background="White"/>
```

这等同于使用元素设置该Button的两个相同的属性：

```xml
<Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
<Button.Content>
     OK
</Button.Content>
<Button.Background>
     White
</Button.Background>
</Button>
```

当然，在任何可以使用特性的地方使用特性更为便捷。

## 类型转换器

上例XAML文件中的``white``是如何与VisualBasic中的``System.Windows.Media.Brushes.White``等价的呢？这个示例提供了一些如何使用字符串设置XAML属性的细节，这些属性的类型即不是``System.String``，也不是``System.Object``。在这种情况下，XAML解析器或编译器必须寻找一个类型转换器，该转换器知道如何将一个字符串表达式转换为一种想要的数据类型。WPF提供了许多常用数据类型的类型转换器，如``Brush``、``Color``、``FontWeight``、``Point``等，它们都派生自Type-Converter的类（如``BrushConverter``、``ColorConverter``等），你也可以为自定义的数据类型写类型转换器。与XAML语言不同，类型转换器通常支持不区分大小写的字符串。
如果没有``Brush``类型转换器，你就必须使用属性元素语法来设置XAML中的``Background``属性，如下所示：

```xml
<Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Content="OK">
<Button.Background>
    <SolidColorBrush>
    <SolidColorBrush.Color>
        <Color A="255" R="255" G="255" B="255"/>
    </SolidColorBrush.Color>
    </SolidColorBrush>
</Button.Background>
</Button>
```

由上可见，类型转换器不仅增强了XAML的可读性，也使一些本来不能被表达的概念得以表达。
下面的代码更精确地表达了运行时获取和执行适合Brush的类型转换器的过程：

```vbnet
Dim b As New System.Windows.Controls.Button()
Dim brType As Type = System.ComponentModel.TypeDescriptor.GetConverter(GetType(Brush))

b.Content = "OK"
b.Background = brType.ConvertFromInvariantString("White").As(Of Brush)
```

## 标记扩展

标记扩展就像类型转换器一样，可用于扩展``XAML``的表达能力。它们都可以在运行时计算字符串特性的值，并生成一个合适的基于字符串的对象。WPF有好几个内建的标记扩展，你会发现它们都派生自本书最前面的内封中的``MarkupExtension``。
与类型转换器不同的是，标记扩展是通过XAML的显式的、一致的语法调用的，因此，标记扩展是最好的扩展XAML的方法。
只要特性值由花括号括起来，XAML编译器或解析器就会把它认作一个标记扩展值而不是一个普通的字符串。下面的按钮使用了3个不同的标记扩展类型，其中分别用到了3个不同的特性：

```xml
<Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
Background="{x:Null}"
Height="{x:Static SystemParameters.IconHeight}"
Content="{Binding Path=Height, RelativeSource={RelativeSource Self}}"/>
```

每个花括号中的第一个识别符是标记扩展类的名称。按照惯例，这样的类都以``Extension``后缀结尾，但在XAML中使用它时，可以不用该后缀。在上面例子中，``NullExtension``（即``x:Null``）和``StaticExtension``（即``x:Static``）是``System.Windows.Markup``命名空间的类，因此必须使用前缀x来定位它们。``Binding``（没有``Extension``后缀）是在``System.Windows.Data``命名空间下的，因此在默认的XML命名空间下就可以找到它。
如果标记扩展支持，可使用逗号分隔的参数来指定它的值。
定位参数（如本例中的``SystemParameters.IconHeight``）被作为字符串参数传入扩展类的相应构造函数中。命名参数（如本例中的``Path``和``RelativeSource``）可用来在已构造好的扩展对象上设置相应名字的属性。这些属性的值可以是标记扩展值自己，也可以是文本值，它们可通过普通的类型转换过程。你可能注意到设计和使用标记扩展与设计和使用自定义特性很相似，这是被有意设计的。
在本例中，``NullExtension``允许设置``Background``笔刷为``null``。``StaticExtension``允许使用静态属性、字段、常量和枚举值，而不使用XAML写的硬编码字面值。在这个例子中，``Button``的高度是遵循操作系统当前的图标高度设置的，这一设置可通过``System.Windows.SystemParameters``类的``IconHeight``静态字段获得。``Binding``可以把``Content``设置为与它的``Height``属性相同的值。
如果你需要设置一个属性特性值为字面字符串（以左花括号开始），就必须将其转义，我们可通过在其前面增加一对空花括号来实现，如：

```xml
<Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
Content="{}{This is not a markup extension!}"/>
```

此外，也可以使用属性元素语法，因为花括号在上下文中不会有特殊的意义：

```xml
<Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    {This is not a markup extension}
</Button>
```

因为标记扩展是有默认构造函数的类，它们可以与属性元素语法一起使用，如下例：

```vbnet
<Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
<Button.Background>
    <x:Null/>
</Button.Background>
<Button.Height>
    <x:Static Member="SystemParameters.IconHeight"/>
</Button.Height>
<Button.Content>
    <Binding Path="Height">
    <Binding.RelativeSource>
        <RelativeSource Mode="Self"/>
    </Binding.RelativeSource>
    </Binding>
</Button.Content>
</Button>
```

该转换之所以可以执行是因为这些标记扩展都有与形参化的构造函数的实参（使用属性特性语法的定位实参）对应的属性。例如，``StaticExtension``有一个``Member``属性与之前传入到开参化构造函数中的实参意思是一样的，``RelativeSource``有一个对应于构造函数实参的``Mode``属性。
由于标记扩展完成的实际工作对于每个扩展都是不同的。如下面的VisualBasic代码与使用``NullExtension``、``StaticExtension``和``Binding``的基于XAML表示的按钮是一个意思：

```vbnet
Dim b As New System.Windows.Controls.Button()

' 设置Background
b.Background = null
' 设置Height
b.Height = System.Windows.SystemParameters.IconHeight
' 设置Content
Dim binding As New System.Windows.Data.Binding()
binding.Path = new System.Windows.PropertyPath("Height")
binding.RelativeSource = System.Windows.Data.RelativeSource.Self
b.SetBinding(System.Windows.Controls.Button.ContentProperty, binding)
```

尽管如此，这里的代码与``XAML``解析器或编译器使用的机制是不同的，解析器和编译器是依靠每个标记扩展在运行时设置合适的值（本质上是通过调用每个类的``ProvideValue``方法来实现的）。与这一机制完全对应的过程式代码通常很复杂。

## 对象元素的子元素

XAML文件就像所有的XML文件一样，必须有一个单独的根对象元素。对象元素是可以支持子对象元素的。一个对象元素可以有3种类型的子元素：一个内容属性值，集合项，或一个能够通过类型转换到它的父元素的值。

## 内容属性

大多数WPF类（通过定制特性）指定了一个属性，该属性可以被设置为XML元素中的任何内容。这个属性叫作内容属性，它确实是一个让XAML呈现变得更轻便简单的捷径。从某种意义上讲，这些内容属性有点像VB中的默认属性。``Button``中的``Content``属性就是这样指定的，如：

```xml
<Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Content="OK"/>
```

可以被重写为：

```xml
<Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
OK
</Button>
```

还有更复杂的方式：

```xml
<Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
<Button.Content>
    <Rectangle Height="40" Width="40" Fill="Black"/>
</Button.Content>
</Button>
```

可以被重写为：

```xml
<Button xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <Rectangle Height="40" Width="40" Fill="Black"/>
</Button>
```

## 集合项

XAML允许将项添加到支持索引的两种类型的集合中：``List``和``Dictionary``。

### List

``List``是实现了``System.Collection.IList``接口的集合，如``System.Collections.ArrayList``和许多WPF定义的集合类都是``List``。如，下面的XAML向``ListBox``添加了两个项，它的``Items``属性是实现了``IList``的``ItemCollection``类型：

```xml
<ListBox xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
<ListBox.Items>
     <ListBoxItem Content="Item 1"/>
     <ListBoxItem Content="Item 2"/>
</ListBox.Items>
</ListBox>
```

因为``Items``是``ListBox``的内容属性，可以进一步简化：

```xml
<ListBox xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <ListBoxItem Content="Item 1"/>
    <ListBoxItem Content="Item 2"/>
</ListBox>
```

### Dictionary


``System.Windows.ResourceDictionary``是WPF中的一个常用的集合类型，它实现了``System.Collections.IDictionary``接口，能够支持在过程式代码中添加、移除枚举键/值对。下面的XAML添加了两个``Color``对象到一个``ResourceDictionary``中。

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Color x:Key="1" A="255" R="255" G="255" B="255"/>
    <Color x:Key="2" A="0" R="0" G="0" B="0"/>
</ResourceDictionary>
```

**注意，在带有x:Key的XAML中指定的值总是被作为字符串处理的，除非使用标记扩展，但不会尝试使用类型转换。**

XAML是设计用来与.NET类型系统一起工作的，你可以在其中使用任何类型的.NET对象，也可以使用自己定义的对象。但对象必须以“友好声明”的方式进行设计。如果一个类没有默认构造函数，也没有提供有用的实例属性，那么它在XAML中是无法直接使用的。
WPF程序集都被加上了``XmlnsDefinitionAttribute``属性，这样可以将.NET命名空间映射为XAML文件中的XML命名空间，但对于那个不是专门为XAML设计的程序集又该如何处理呢？它们的类型仍然可以使用，只需要一个特殊的指令作为XML命名空间就可以了。如下例：

```vbnet
Dim h As New System.Collections.Hashtable()
h.Add("key1", 1)
h.Add("key2", 2)
```

以上代码在XAML中表示为：

```xml
<collections:Hashtable xmlns:collections="clr-namespace:System.Collections;assembly=mscorlib"
xmlns:sys="clr-namespace:System;assembly=mscorlib"
xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <sys:Int32 x:Key="key1">1</sys:Int32>
    <sys:Int32 x:Key="key2">2</sys:Int32>
</collections:Hashtable>
```

``clr-namespace``标记允许直接在XAML中放入一个.NET命名空间。

为了避免混淆，在转换子元素时，任何一个有效的XAML解析器或编译器必须遵循下面的规则：

+ 如果该类型实现了IList接口，就为每个子元素调用IList.Add。
+ 否则，如果该类型实现了IDictionary，就为每个子元素调用IDictionary.Add，在该值的键和元素中使用x:Key特性值。
+ 否则，如果父元素支持内容属性（由System.Windows.Markup.ContentPropertyAttribute表示），而且子元素的类型与该内容属性是兼容的，就把子元素作为它的值。
+ 否则，如果子对象是普通文本，且有类型转换器将子对象转换为父类型（没有在父元素上设置属性），则把子元素作为类型转换器的输入，将输出作为父对象的实例。
+ 其他情况下则抛出错误。

## 编译：将XAML与过程式代码混合使用

大多数WPF应用程序是XAML与过程式代码的混合体。

## 在运行时加载和解析XAML

WPF的运行时XAML解析器公开为两个类，它们都位于``System.Windows.Markup``命名空间中：``XamlReader``和``XamlWriter``。``XamlReader``包含了一些对静态``Load``方法的重载，而``XamlWriter``包含了一些对静态``Save``方法的重载。因此，用任何一种.NET语言写的程序都可以在运行时依赖XAML，而不用程序员付出太多努力。

## XamlReader

``XamlReader.Load``方法的设置将解析XAML，创建合适的.NET对象，然后返回一个根元素的实例。因此，如果在当前目录下有一个XAML文件叫作MyWindow-.xaml，它包含了一个``Window``对象作为根结点，那么可以使用下面的代码来加载和获得``Window``对象。

```vbnet
Dim window As Window = null

Using fs As FileStream = New FileStream("MyWindow.xaml", FileMode.Open, FileAccess.Read)
    ' 获得根元素，该元素是一个Window对象
    Window = (Window)XamlReader.Load(fs)
End Using
```

在``Load``返回之后，整个XAML文件的对象层级将在内存中被实例化，因此就不再需要XAML文件了。退出``Using``代码块后，``FileSteam``将被立即关闭。由于可向``XamlReader``传入一个任意的``Stream``（或使用另一个重载来传入``System.Xml.XmlReader``对象），所以有许多可选择的方式来获得XAML的内容。
``XamlReader``也定义了``LoadAsync``实例方法用于异步加载和解析XAML内容。在加载大文件或网络文件时，可使用``LoadAsync``保持用户界面处于响应状态。``CancelAsync``方法用于停止处理，``LoadCompleted``事件可让我们知道处理何时完成。
既然已有一个根元素的实例存在，就可利用适当的内容属性或集合属性来获得子元素。下面的代码假设``Window``有一个类行为``StackPanel``的子元素，Stack-Panel的第5个子对象是一个OK Button。

```vbnet
Dim window As Window = Nothing

Using fs As New FileStream("MyWindow.xaml", FileMode.Open, FileAccess.Read)
    ' 获得根元素，该元素是一个Window对象
    window = DirectCast(XamlReader.Load(fs), Window)
End Using

' 通过（硬编码知识）遍历子元素获取OK按钮
Dim panel As StackPanel = DirectCast(window.Content, StackPanel)
Dim okButton As Button = DirectCast(panel.Children(4), Button)
```

有了这个``Button``的引用，就可以做任何想做的事：设置额外的属性，添加事件处理程序，或执行一些无法用XAML完成的动作。
但使用硬编码索引和其他关于用户界面结构假设的代码并不能让人满意。XAML支持元素命名，这样就可以从过程式代码中找到这些元素并放心地使用它们。

## 命名XAML元素

XAML语言命名空间有一个Name关键字，它是用来给元素命名的。示例如下：

```xml
<Button x:Name="okButton">OK</Button>
```

上例代码可改如下：

```vbnet
Dim window As Window = Nothing

Using fs As New FileStream("MyWindow.xaml", FileMode.Open, FileAccess.Read)
    ' 获得根元素，该元素是一个Window对象
    window = DirectCast(XamlReader.Load(fs), Window)
End Using

' 通过按钮的名称获得OK按钮Button
okButton = DirectCast(window.FindName("okButton"), Button)
```

``FindName``并不仅仅在``Window``类中存在，在``FrameworkElement``、``FrameworkContentElement``及许多重要的WPF类的基类中也有``FindName``的定义。

## 编译XAML

XAML编译包括三项事件：

+ 将一个XAML文件转换为一种特殊的二进制格式。
+ 将转换好的内容作为二进制资源嵌入到正在被创建的程序集中。
+ 执行链接操作，将XAML和过程式代码自动连接起来。

如果你不在乎将XAML文件和过程式代码融合，那么只需把它添加到VS的WPF项目中来，并用界面中的Build动作来完成编译即可。但如果要编译一个XAML文件并将它与过程式代码混合，第一步要做的就是为XAML文件的根元素指定一个子类，可以用XAML语言命名空间中的Class关键字来完成。例如：

```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
xmlns="http://schemas.microsoft.com/winfx/2006/xaml"
x:Class="MyNamespace.MyWindow">
...
</Window>
```

在一个独立的源文件中（但是在同一个项目中），可以一个子类，并添加任何想添加的成员：

```vbnet
Namespace MyNamespace
	Partial Class MyWindow
		Inherits Window
		Public Sub New()
			'一定要调用，这样才能加载XAML定义的内容
				' ...
			InitializeComponent()
		End Sub
		' Any other members can go here...
	End Class
End Namespace

```

通常我们把这样的文件叫作代码隐藏文件。如果你引用XAML中的任何一个事件处理程序（通过事件特性，如Button的Click特性），这里就是我们定义这些事件处理程序的地方。
当在VS中创建一个基于WPF的VisualBasic或VB项目，或当使用``Add New Item...``来添加某WPF项目时，VS会自动创建一个XAML文件，并把``x:Class``作为根元素，同时创建一个具有部分类定义的代码隐藏源文件，最后把两者连接起来，这样代码构建（build）才能顺利进行。

## BAML

BAML是``Binary Application Markup Language``的缩写，意思是二进制应用程序标记语言，它其实是被解析、标记化，最后转换为二进制形式的XAML。虽然大块的XAML代码可以被表示为过程式代码，但XAML到BAML的编译过程不会生成过程源代码。因此，BAML不像MSIL，它是一个压缩的声明格式，比加载和解析普通的XAML文件快，且比普通XAML文件要小。BAML仅仅是XAML编译过程的详细实现，没有任何直接公开的方法，在未来可能会被一些其他东西所取代。

## 生成的源代码

``x:Class``只能在要编译的XAML文件中使用。但在没有``x:Class``的情况下，编译XAML文件也是没有问题的。这意味着没有对应的代码隐藏文件，因此不能使用任何需要过程式代码才能实现的特性。
每个生成的源文件中包含了一个由根对象元素中的``x:Class``指定的类的部分类定义。XAML文件中的每个已命名的元素在该部分类中都有一个成员（默认是私有的），这些成员的名称就是元素名称。其中还有一个``InitializeComponent``方法用于完成一大堆烦人的工作。

## BAML可以反编译为XAML吗？

可以，无论怎么声明，任何一个公司的.NET类实例都可以被序列化为XAML。第一个步骤是获取一个实例，这个实例是用来作为根对象的。如果你还没有这个对象，可以调用静态的``System.Windows.Application.LoadComponent``方法，如下所示：

```vbnet
Dim uri As New System.Uri("MyWindow.xaml", System.UriKind.Relative)
Dim window As Window = Application.LoadComponent(uri).As(Of Window)
```

由URI指定的名称并不要求物理上存在一个独立的xaml文件。当指定了一个合适的URI后，``LoadComponent``可以自动获得作为资源嵌入的BAML。实际上，VS自动生成的``InitializeComponent``方法就是调用``Application.LoadComponent``来加载嵌入的BAML。
获得根元素的实例后，可以使用``System.Windows.Markup.XamlWrite``类来获得根元素的XAML表示。``XamlWriter``包含了5个静态``Save``方法的重载，是最简单的返回一个适当XAML字符串的方法，通过传入一个对象实例来实现。如：

```vbnet
Dim xaml As String = XamlWriter.Save(window)
```

## XAML关键字

XAML语言的命名空间（[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)）定义了一批XAML编译器或解析器必须特殊处理的关键字。它们主要控制元素如何被提供给过程式代码，但即使没有过程式代码，有一些关键字还是有用的。如``Key``、``Name``、``Class``、``Subclass``和``Code``。

## WPF的重要新概念

### 逻辑树与可视树

XAML天生就是用来呈现用户界面的，这是由于它具有层次化的特性。在WPF中，用户界面由一个对象树构建而成，这棵树叫作逻辑树。
WPF用户界面的逻辑树也并不一定用XAML创建，它完全可能用过程式代码来实现。
逻辑树的概念很直观，但为什么要关注它呢？因为几乎WPF的每一方面（属性、资源、事件等）都有与逻辑树相关联的行为。如，属性值有时会沿着树自动传递给子元素，而触发的事件可以自底向上或自顶向下遍历树。
与逻辑树类似的一个概念是可视树。可视树基本上是逻辑树的扩展，在可视树中，节点都被打散，分放到核心可视组件中。可视树提供了一些详细的可视化实现，而不是把每个元素当作一个“黑盒”。如，虽然``ListBox``从逻辑上讲是一个单独的控件，但它的默认可视呈现是由更多的原始WPF元素组成的：一个``Border``对象、两个``ScrollBar``及其他一些元素。
并非所有的逻辑树节点都会出现在可视树中，只有从``System.Windows.Media.Visual``或``System.Windows.Media.Visual3D``派生的元素才会被包含进去。其他元素不会包含在内，因为它们自己并没有与生俱来的呈现行为。
使用``System.Windows.LogicTreeHelper``和``System.Windows.Media.VisualTreeHelper``这两个有些对象的类可以方便地遍历逻辑树和可视树。

注意：**不要根据具体的可视树写代码。逻辑树是静态的，不会受到程序员的干扰（例如动态添加/删除）元素，但只要用户切换不同的Windows主题，可视树就会改变。**

遍历和打印逻辑树和可视树的示例代码：

```vbnet
Imports System.Diagnostics
Imports System.Windows
Imports System.Windows.Media

Public Partial Class AboutDialog
	Inherits Window
	Public Sub New()
		IntializeComponent()
		PrintLogicalTree(0, Me)
	End Sub
	Protected Overrides Sub OnContentRendered(e As EventArgs)
		MyBase.OnContentRendered(e)
		PrintVisualTree(0, Me)
	End Sub

	Private Sub PrintLogicalTree(depth As Integer, obj As Object)
		'打印对象，使用前置空格表示深度
		Debug.WriteLine(New String(" ", depth) & Convert.ToString(obj))

		'有时，叶子节点不是DependencyObject，如string
		If Not (TypeOf obj Is DependencyObject) Then
			Return
		End If

		'递归调用每个逻辑子节点
		For Each child As Object In LogicalTreeHelper.GetChildren(TryCast(obj, DependencyObject))
			PrintLogicalTree(depth + 1, child)
		Next
	End Sub

	Private Sub PrintVisualTree(depth As Integer, obj As DependencyObject)
		'打印对象，使用前置空格表示深度
		Debug.WriteLine(New String(" ", depth) & Convert.ToString(obj))
		'递归调用每个可视子节点
		For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(obj) - 1
			PrintVisualTree(depth + 1, VisualTreeHelper.GetChild(obj, i))
		Next
	End Sub
End Class
```

虽然在Window的构造函数中就可以遍历逻辑树，但可视树真到Window完成至少一次布局后才会有节点，否则是空的。这也是为什么``PrintVisualTree``是在``On-ContentRendered``中调用的，因为``OnContentRendered``是在布局完成后才被调用的。

### 依赖属性

WPF引入了一个新的属性类型叫作依赖属性，整个WPF平台中都会使用到它，用来实现样式化、自动数据绑定、动画等。
依赖属性在任何时刻都是依靠多个提供程序来判断它的值的。这些提供程序可以是一段一直在改变值的动画，或者一个父元素的属性值从上慢慢传递给子元素等。依赖属性的最大特征是其内建的传递变更通知的能力。
添加这样的智能给属性，其动力在于能够声明标记中直接启用富功能。WPF友好声明设计的关键在于它使用了很多属性。例如，``Button``控件有96个公共属性。属性可以方便地在XAML中设置而不用程序代码。但如果依赖属性没有额外的垂直传递，在不写额外代码的情况下，很难在设置属性这样简单的动作中获得想要的结果。

### 依赖属性的实现

实际上，依赖属性仅仅是普通的.NET属性，只不过它已融入到了WPF架构中。它完全是由WPF API实现的，没有一种.NET语言天生就能理解依赖属性。
下例展示了一个``Button``如何有效地实现一个叫``IsDefault``的依赖属性：

```vbnet
Public Class Button : Inherits ButtonBase
	' 依赖属性
	Public Shared ReadOnly IsDefaultProperty As DependencyProperty

	Shared Sub New()
		' 注册属性
		' ...
		Button.IsDefaultProperty = DependencyProperty.Register(
		    "IsDefault",
			GetType(Boolean),
			GetType(Button),
			New FrameworkPropertyMetadata(False, New PropertyChangedCallback(AddressOf OnIsDefaultChanged)))
	End Sub

	' .NET属性包装器（可选）
	Public Property IsDefault() As Boolean
		Get
			Return CBool(GetValue(Button.IsDefaultProperty))
		End Get
		Set
			SetValue(Button.IsDefaultProperty, value)
		End Set
	End Property

	' 属性改变的回调（可选）
	Private Shared Sub OnIsDefaultChanged(o As DependencyObject, e As DependencyPropertyChangedEventArgs)
		' ...
	End Sub
End Class
```

``IsDefaultProperty``静态成员是真正的依赖属性，类型为System.Windows.DependencyProperty。按规则，所有的DependencyProperty成员都必须是public、static，并且有一个Property作为后缀。依赖属性通常调用DependencyProperty.Register静态方法创建，这样的方法需要一个名称（IsDefault）、一个属性类型（bool）及拥有这个属性的类（Button类）。通过不同的Register方法重载，你可以传入metadata（元数据）来告诉WPF如何处理该属性、如何处理属性值改变的回调、如何处理强制值转换、及如何验证值。Button会在它的静态构造函数中调用Register的重载，给依赖属性一个默认值false，并为变更通知添加一个委托。
最后，那个叫作IsDefault的传统.NET属性会调用继承自System.Windows.DependencyObject的GetValue和SetValue方法来实现自己的访问器，System.-Windows.DependencyObject是底层基类，这是拥有依赖属性的类必须继承的。GetValue返回最后一次由SetValue设置的值，如果SetValue从未被调用过，那么就是该属性注册时的默认值。IsDefault .NET属性并不是必需的，Button的使用者可能会直接调用GetValue/SetValue方法，因为它们是公开的。

注意：**在运行时，绕过了.NET属性包装器在XAML中设置依赖属性。**

虽然XAML编译器在编译时是依靠该属性包装器的，但在运行时WPF是直接调用GetValue和SetValue的。因此，为让使用XAML设置属性与使用过程式代码设置属性保持一致，在属性包装器中除了GetValue/SetValue调用外，不应该包含任何其他逻辑，这是至关重要的。如果要添加自定义逻辑，应该在注册的回调函数中添加。
表面上看，上例代码像是一种冗长的呈现简单布尔属性的方式。然而，因为GetValue和SetValue内部使用了高效的稀疏存储系统，而IsDefaultProperty是一个静态成员（而不是一个实例成员），与典型的.NET属性相比，依赖属性的实现节省了保存每个实现所需要的内存。
依赖属性的好处远不止节约内存而已。它把相当一部分代码集中起来，并做标准化处理。

### 变更通知

无论何时，只要依赖属性的值变了，WPF就会自动根据属性的元数据（metadata）触发一系列动作。内建的变更通知最有趣的特性之一是属性触发器，它可以在属性值改变时执行自定义动作，而不用更改任何过程式代码。
例如，你想让Button在鼠标移上去时变为蓝色。如果没有属性触发器的话，你要为每个Button添加两个事件处理程序，一个为MouseEvent事件准备，一个为MouseLeave事件准备。

```xml
<Button MouseEnter="Button_MouseEnter" MouseLeave="Button_MouseLeave" 
MinWidth="75" Margin="10">
    Help</Button>
<Button MouseEnter="Button_MouseEnter" MouseLeave="Button_MouseLeave" 
MinWidth="75" Margin="10">
    OK</Button>
```

下面的代码实现了这两个事件处理程序：

```vbnet

	Private Sub Button_MouseEnter(sender As Object, e As MouseEventArgs)
		Dim b As Button = TryCast(sender, Button)
		If b IsNot Nothing Then
			b.Foreground = Brushed.Blue
		End If
	End Sub

	Private Sub Mouse_MouseLeave(sender As Object, e As MouseEventArgs)
		Dim b As Button = TryCast(sender, Button)
		If b IsNot Nothing Then
			b.Foreground = Brushed.Black
		End If
	End Sub
```

然而有了属性触发器，完全可以在XAML中完成相同的行为：

```xml
<Button MinWidth="75" Margin="10">
<Button.Style>
    <Style TargetType="{x:Type Button}">
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Foreground" Value="Blue" />
        </Trigger>
    </Style.Triggers>
    </Style>
</Button.Style>
    OK
</Button>
```

属性触发器仅仅是WPF支持的3种触发器之一。数据触发器是属性触发器的另一种形式，它可以在任何.NET属性中工作（而不仅仅是依赖属性）。事件触发器会通过声明方式指定动作，该动作在路由事件触发时生效。

### 属性值继承

术语“**属性值继承**”并不是指传统的面向对象的类继承，而是指属性值自顶向下沿着元素树传递。
属性值的继承行为由以下两个因素决定：

+ 并不是每个依赖属性都参与属性值继承。（从内部来讲，依赖属性会通过传递FrameworkPropertyMetadataOptions.Inherits给DependencyProperty-.Register方法来完成继承。
+ 有其他一些优先级更高的源来设置这些属性值。

### 对多个提供程序的支持

WPF有许多强大的机制可以独立地去尝试设置依赖属性的值。如果没有设计良好的机制来处理这些完全不同的属性值提供程序，这个系统会变得混乱，属性值会变得不稳定。当然，正如它们的名字所表达的，依赖属性就是设计为以一致的、有序的方式依靠这些提供程序。
下图展示了这5步流程，通过该流程，WPF运行每个依赖属性并最终计算出它的值。依靠依赖属性中内嵌的变更通知，这个流程才可自动发生。

![]()

### 判断基础值

大多数属性值提供程序会把基础值的计算纳入考虑范畴。下面的清单显示了8个提供程序，它们可以设置大多数依赖属性的值，优先级顺序从高到低为：

> 本地值->样式触发器->模板触发器->样式设置程序->主题样式触发器->主题样式设置程序->属性值继承->默认值

本地值技术上的含义是任何对DependencyObject.SetValue的调用，但它通常会有一个简单的属性赋值，这是用XAML或过程式代码完成的。
默认值指的是依赖属性注册时使用的初始值。

### 计算

如果第一步中的值是表达式（派生自System.Windows.Expression的一个对象），那么WPF会执行一种特殊的演算步骤--把表达式转换为具体的结果。在WPF 3.0中，表达式仅在使用动态资源或数据绑定时起作用。

### 应用动画

如果一个或多个动画在运行，它们有能力改变当前的属性值或完全替代当前的属性值。

### 限制

在所有属性值提供程序处理过后，WPF将拿到一个几乎是终值的属性值，如果依赖属性已经注册了CoerceValueCallback，还会把这个属性值传递给Coerce-ValueCallback委托。该回调函数负责返回一个新的值，它是基于自定义逻辑实现的。

### 验证

最后，如果依赖属性已经注册了ValidateValueCallback，之前的限制中的值将被传入ValidateValueCallback委托。如果输入值有效，该回调函数返回true，否则返回false。返回false将会导致抛出一个异常，并使整个流程被取消。
如果没办法判断依赖属性从哪里获得当前值，那么可以得到静态方法DependencyPropertyHelper.GetValueSource作为调试助手。该方法将返回一个ValueSource结构，其中包含以下一些数据：一个BaseValueSource枚举值，它反映的是基础值从哪里来的（流程中的第一步）；IsExpression、IsAnimated和IsCoerced几个布尔类型属性，它反映了第二步到第四步的信息。
请不要在程序代码中使用这个方法，WPF以后的版本中将打破值计算的假设，会根据它的源类型采用不同的方式处理属性值，而不是根据假设WPF应用程序中的方式来处理。
你很可能需要清除本地值，并让WPF从下一个最高优先级的提供程序中获得值，然后使用这个值来设置最终的属性值。DependencyObject提供了这样的机制，可通过调用ClearValue方法来实现。

```vbnet
' b为Button实例
b.ClearValue(Button.ForegroundProperty)
```

``Button.ForegroundProperty``是一个``DependencyProperty``静态成员，在调用``ClearValue``后，会重新计算基础值，并把本地值从方程式中删除。

### 附加属性

附加属性是依赖属性的一种特殊形式，可以被有效地添加到任何对象中。这可能听上去很奇怪，但这个机制在WPF中有多种应用。

类似于WinForm那样的技术，许多WPF类定义了一个Tag属性（类型是System.Object），目的是为了存储每一个实例的自定义数据。但要添加自定义数据给任何一个派生自DependencyObject的对象，附加属性是一种更加强大、更加灵活的机制。通常我们会忽略一点，即可以用附加属性高效的向密封类（sealed class）的实例添加自定义数据。

另外，大家对附加属性有一个曲解，虽然在XAML中设置它们依赖于SetXXX静态方法，但可在过程式代码中绕过这个方法，直接去调用DependencyObject-.SetValue方法。这意味着在过程式代码中，可以把任何一个依赖属性作为一个附加属性。如，下面的代码把ListBox的IsTextSearchEnabled属性添加到了Button控件上，并赋予该属性一个值：

```vbnet
' 向Button添加一个不相关的属性，并把它的值设置为true
okButton.SetValue(ListBox.IsTextSearchEnabledProperty, True)
```

虽然这似乎没有任何意义，但你可以用一种对应用程序或组件有意义的方式来随意使用这个属性值。

### 路由事件

正如WPF在简单的.NET属性概念上添加了许多基础的东西一样，它也为.NET事件添加了许多基础的东西。路由事件是专门设计用于在元素树中使用的事件。当路由事件触发后，它可以向上或向下遍历可视树和逻辑树，用一种简单而且持久的方式在每个元素上触发，而不需要使用任何定制代码。
事件路由让许多程序不去留意可视树的细节（对于样式重置来说这是很不错的），并且对于成功的WPF元素创作至关重要。
在前面的文章部分之中，对于VCR样式的Stop按钮来说，一个用户可能在Rectangle逻辑子元素上直接按下鼠标左键。由于事件遍历了逻辑树，Button元素还是会发现这个事件，并处理该事件。因此，你可以在一个元素（如Button）中嵌入任何复杂内容或设置一棵复杂的可视树，鼠标左键单击其中任何一个内部元素，仍然会触发父元素Button的Click事件。如果没有路由事件，内部内容的创造者或按钮的使用者不得不编写代码来把事件串起来。
路由事件的实现和行为与依赖属性有许多相同的地方。

### 路由事件的实现

与依赖属性一样，没有一种.NET语言（除XAML外）天生具有理解路由指派的能力。
就像依赖属性是由公共的静态DependencyProperty成员加上一个约定的Property后缀名构成的一样，路由事件也是由公共的静态RoutedEvent成员加上一个约定的Event后缀名构成的。路由事件的注册很像静态构建器中注册依赖属性，它会定义一个普通的.NET事件或一个事件包装器，这样可以保证在过程式代码中使用起来更加熟悉，并且可以在XAML中用事件特性语法添加一个事件处理程序。与属性包装器一样，事件包装器在访问器中只能调用AddHandler和RemoveHandler，而不应该做其他事件。

```vbnet
Public Class Button
	Inherits ButtonBase
	'路由事件
	Public Shared ReadOnly ClickEvent As RoutedEvent

	Shared Sub New()
		'注册事件
			' ...
		Button.ClickEvent = EventManager.RegisterRoutedEvent(
        "Click", 
        RoutingStrategy.Bubble, 
        GetType(RoutedEventHandler), 
        GetType(Button))
	End Sub

	'.NET事件包装器（可选）
	Public Custom Event Click As RoutedEventHandler
		AddHandler(ByVal value As RoutedEventHandler)
			Call [AddHandler](Button.ClickEvent, value)
		End AddHandler
		RemoveHandler(ByVal value As RoutedEventHandler)
			Call [RemoveHandler](Button.ClickEvent, value)
		End RemoveHandler
	End Event

	Protected Overrides Sub OnMouseLeftButtonDown(e As MouseButtonEventArgs)
		' ...
		'触发事件
		Call [RaiseEvent](New RoutedEventArgs(Button.ClickEvent, Me))
		' ...
	End Sub
	' ...
End Class
```

这些``AddHandler``和``RemoveHandler``方法没有从DependencyObject继承，而是从System.Windows.UIElement继承的，UIElement是一个更高层的供元素（如Button元素）继承的基类。这些方法可以向一个适当的路由事件添加一个委托或从路由事件移除一个委托。在``OnMouseLeftButtonDown``中，它使用适当的RoutedEvent成员调用RaiseEvent来触发Click事件。当前的Button实例（this）被传递给事件的源元素（source element）。在代码清单中没有列出，但是作为对KeyDown事件的响应，Button的Click事件将被触发，这样就可以处理由空格健或回车键完成点击动作的情况。

### 路由策略和事件处理程序

当注册完成后，每个路由事件将选择3个路由策略中的一个。所谓路由策略就是事件触发遍历整棵元素树的方式，这些策略由``RoutingStategy``枚举值提供。

+ Tunneling（管道传递）----事件首先在根元素上被触发，然后从每一个元素向下沿着树传递，直至到达源元素为止（或者直至处理程序把事件标记为已处理为止）。
+ Bubbling（冒泡）----事件首先在源元素上被触发，然后从每一个元素向上沿着树传递，直至到达根元素为止（或者直至处理程序把事件标记为已处理为止）。
+ Direct（直接）----事件仅在源元素上触发。这与普通.NET事件的行为相同，不同的是这样的事件仍然会参与一些路由事件的特定机制，如事件触发器。

路由事件的事件处理程序有一个签名，它与通用.NET事件处理程序的模式匹配：第一个参数是一个System.Object对象，名为sender，第二个参数（一般命名为e）是一个派生自``System.EventArgs``的类。传递给事件处理程序的sender参数就是该处理程序被添加到的元素。参数e是RoutedEventArgs的一个实例（或者派生自RoutedEventArgs），RoutedEventArgs是EventArgs的一个子类，它提供了4个有用的属性：

|属性名|描述|
|-----|----|
|Source|逻辑树中一开始触发该事件的元素。|
|OriginalSource|可视树中一开始触发该事件的元素（例如，TextBlock或标准的Button元素的ButtonChrome子元素）。|
|Handled|布尔值，设置为true表示标记事件为已处理，这就是用于停止Tunneling或Bubbling的标记。|
|RoutedEvent|真正的路由事件对象（如Button.ClickEvent），当一个事件处理程序同时被用于多个路由事件时，它可以有效的识别被触发的事件。|

``Source``和``OriginalSource``的存在允许使用更高级别的逻辑树或更低级别的可视树。然而，这种区别仅对于像鼠标事件这样的物理事件有效。对于更抽象的事件来说，不需要与可视树中的某个元素建立直接关系（就像由于键盘支持的``Click``），WPF会传递相同的对象给``Source``和``OriginalSource``。

### 路由事件的实现

UIElement类为键盘、鼠标、指示笔输入定义了许多路由事件。大多数路由事件是冒泡事件，但许多事件与管道事件是配对的。管道事件很容易被识别，因为按照惯例，它们的名字中都有一个Preview前缀，在它们的配对冒泡事件发生前，这些事件会立即被触发。例如，``PreviewMouseMove``就是一个管道事件，在MouseMove冒泡事件前被触发。
为许多不同的行为提供一对事件是为了给元素一个有效地取消事件或在事件即将发生前修改事件的机会。根据惯例，（当定义了冒泡和管道的事件对后）WPF的内嵌元素只会在响应一个冒泡事件时采取行动，这样可以保证管道事件能够名副其实的做到“预览”。例如，在TextBox控件的Preview事件中对录入的文本进行校验，过滤不符合规范的文本。

### 处理单击鼠标中键的事件在哪里？

如果浏览一遍UIElement或ConentElement提供的所有鼠标事件，可以找到``MouseLeftButtonDown``、``MouseLeftButtonUp``、``MouseRightButtonDown``、``MouseRightButtonUp``事件，但有些鼠标上出现的附加按键该怎么办呢？
这一信息可以通过更加通用的MouseDown和MouseUp事件获得。传入这样的事件处理程序的参数包括一个MouseButton枚举值，它表示鼠标状态Left、Right、Midle、XButton1、XButton2，还有一个MouseButtonState枚举值，表示这个按钮是Pressed还是Released。

### 中止路由事件是一种假象

虽然在事件处理程序中设置RoutedEventArgs参数的Handled属性为true，可以终止管道传递或冒泡，但是进一步沿着树向上或向下的每个处理程序还是可以收到这些事件。这只能在代码中完成。在任何时候，都应该尽可能地避免处理已处理过的事件，因为事件应该是在第一时间被处理的。
总之，终止管理传递或冒泡仅仅是一种假像而已。更加准确的说法应该是，当一个路由事件标记为已处理时，管道传递和冒泡仍然会继续，但默认情况下，事件处理程序只会处理没有处理过的事件。

### 附加事件

通过附加事件，WPF可以通过一个没有定义过该事件的元素来完成路由事件的管道传递和冒泡。
附加事件与附加属性操作起来很像。每个路由事件都可以被当作附加事件使用。
由于需要传递许多信息给路由事件，可以用上层的“megahandler”来处理每一个管道或冒泡事件。这个处理程序通过分析RoutedEvent对象判断哪个事件被触发了，并把RoutedEventArgs参数转换为一个合适的子类，然后继续。

### 命令

WPF提供了内建的命令支持，这是一个更为抽象且松耦合的事件版本。尽管事件是与某个用户动作相关联的，但命令表示的是那些与用户界面分离的动作，最标准的命令示例是剪切（Cut）、复制（Copy）、粘贴（Paste）。应用程序总能通过许多同步的机制提供这些动作：Menu控件中的MenuItem、ContextMenu控件中的MenuItem、ToolBar控件中的Button、键盘快捷方式等。

### 内建命令

命令是任何一个实现了``ICommand``接口（位于``System.Windows.Input``命名空间）的对象，每个对象定义了3个简单的成员：

|成员|介绍|
|----|---|
|Execute|执行特定命令的逻辑的方法。|
|CanExecute|如果命令允许被执行，则返回true，否则返回false。|
|CanExecuteChanged|无论何时，只要CanExecute的值改变，该事件就会触发。|

如果需要创建剪切、复制和粘贴命令，可以定义3个实现ICommand接口的类，找一个地方存储这3个类（如放在主窗口的静态成员中），从相关的事件处理程序中调用Execute（当CanExecute返回true时），处理CanExecuteChanged事件，改变相关用户界面中的IsEnabled属性。

像Button、CheckBox、MenuItem这样的控件有相关的逻辑会与任何命令做交互。它们会有一个简单的Command属性（类型为ICommand），当设置了Command属性后，无论何时Click事件触发，这些控件会自动调用命令的Execute方法（只要CanExecute返回true时）。另外，它们会自动保持IsEnabled的值与CanExecute的值同步，这是通过CanExecuteChanged事件实现的。通过这种给属性赋值的方式，任何逻辑在XAML下都是可以实现的。

同时，WPF已经定义了一系列命令，因此不需要为Cut、Copy和Paste命令实现ICommand对象，也不用担心在哪里保存这些命令。WPF有5个类的静态属性实现了WPF的内建命令：

> ApplicationCommands:
> Close、Copy、Cut、Delete、Find、Help、New、Open、Paste、Print、PrintPreview、Properties、Redo、Replace、Save、SaveAs、SelectAll、Stop、Undo等。

其他4个类为``ComponentCommands``、``MediaCommands``、``NavigationCommands``、``EditCommands``。
每个属性返回``RoutedUICommand``的实例，``RoutedUIElement``类不仅实现了``ICommand``接口，还可以像路由事件一样支持冒泡。