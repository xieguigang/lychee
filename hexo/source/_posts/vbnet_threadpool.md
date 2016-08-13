---
title: VB.NET 线程池
tags: [vb.net, threadpool, threading, web server]
date: 2016-8-10
---

> 水是生命之源，计算机资源也一样。

每一线程尤如一滴水，你花一滴，我花一滴，你还一滴，我还一滴，就象游兵散将一样，线程越多，越复杂混乱。**而每一个线程创建需要开销，活动的线程也需要开销。过多的线程导致系统内存占用过度或系统资源不足。**为了解决线程生命周期开销问题和资源不足问题，创建线程池，让每滴水（线程）纳入统一管理。特别是那些生存期比较短暂的线程。使用线程池执行任务比每次完成一个任务时都创建一个全新的线程，随后又删除掉的做法更有效率。

## 一、线程池管理

线程池管埋是指在多线程应用程序的初始化过程屮创建线程的一个集合，当需要线程时，为新任务重用这些线程，而不是创建新的线程的过程。这个过程中创建的线程数量通常部是固定的。然而，增加可用的线程数量也是可以的。池中的每个线程都被分派一个任务，当任务完成时，线程就返回线程池中等待下一次分派。

### 1. 什么时候需要用线程池

线程池在多线程应用程序中是必需的，原因如下：

1. 线程池有效改善了应用程序的响应时间，因为线程池中的线程是现成的，就处于等待任务分派的状态中，系统无需再从头创建线程。
2. 在线程池方式下，CLR节省了为每个生存期短暂的任务创建一个全新线程，并在其结束时回收其资源的开销。
3. 线程池根据系统当前正在运行的进程情况优化线程时间片。
4. 线程池允许我们在不逐一设置线程的属性的情况下，启动多个线程。
5. 线程池允许我们将状态信息作为一个对象传递给当前正在执行任务的过程参数。
6. 线程池可以将处理客户请求的线程数量固定为某一个最大值。

### 2. 线程池的概念

影响多线程应用程序的响应性的一个主要原因就是为每个任务创建线程的时间开销。
例如，Web服务器是响应10个客户的访问，以前的做法是：若服务器采用为每个客户创建一个线程的策略，则需创建10个新线程。服务器将承担线程的创建、在整个生存期内管理这些线程的开销。在某个时刻，线程可能会耗尽整个系统的资源。替代的方法：若服务器使用线程池来响应客户请求，每当客户发出请求时，系统就无需再为创建线程耗费时间。这就是线程池管理方式的重要概念。
Windows操作系统为响应客户请求维护一个线程池。当我们的应用程序请求一个新的线程时，Windows就试图从池中取出一个，如果线程池是空的，那就创建一个新的供我们使用。Windows将动态处理线程池的大小以加快应用程序的响应时间。

影响多线程应用程序线程设计的因素有:**应用程序的响应性、线程管理资源的分配、资源共享、线程同步**。

## 二、CLR与线程池

CLR是专门用于创建托管代码环境，为在.NET平台上运行的应用程序提供各种服务的，例如编译、垃圾收集、内存管理，还有线程池。

确实，在定义宿主应用程序使用的线程的进程方面，Win32和.NET Framework有着显著的差别。
在传统的多线程Win32应用程序中，每个进程都是由线程集合组成的。每个线程又由线程本地存储(Thread Local Storage，TLS)、调用堆栈组成，用于在单处理器系统中提供时间片。单处理器系统根据线程的优先级为每个线程分配时间片。当某个特定线程的时间片消耗完时，它就会处于挂起状态，其他线程就将开始执行其任务。
在.NET Framework中，每个进程都可分成多个应用程序域，它是用于宿主线程以及TLS和调用堆栈的。值得关注的是，进程间的通信是通过.NETFramework中的一个称为远程处理的技术来进行处理的。

### 1．CLR管理线程池

CLR构成了.NET Framework的灵魂和核心，为托管应用程序提供多个服务(线程池管理就是其中之一)。对于线程池中排在队列中的每个任务(任务项)，CLR从线程池中指派一个线程(工作者线程)，然后在任务结束时将线程释放回池中。

线程池总是通过CLR使用多线程单元模式，借助抢先式多任务管理使用高性能的队列和调度程序来实现的。它是CPU时间被分成多个时间片的一个过程。在每个时间片中，都有个特定的线程在执行，而其他线程则处于等待状态。一旦这个时间片用完之后，系统就根据剩余线程的最高优先级决定由哪个线程使用CPU。客户请求排在任务队列中，队列中的毎个任务都将被分配给线程池中第一个可用的线程。
一旦线程完成了分配给它的任务，它就返回到线程池中等待CLR的下一次分配。
线程池的大小可以是固定不变的，也可以是动态变化的。在前面的示例中，线程的数量在线程池的生存期间不发生变化。通常情况下，这种类型的线程池用于我们确切知道应用程序可用资源的数量的情况，这样固定数目的线程就可以在线程池初始化过程中创建完成。
面这种情况正好适用于这种类型：我们为企业内部网开发解决方案或者在可以严格定义目标平台的系统需求的应用程序中，大小动态可变的线程池适用于不知道可用资源数量的情况，因为在Web服务器的情况下，我们不知道将要同时处理多少客户请求。

### 2. 避免使用线程池的情况

尽管线程池在我们构建多线程应用程序时给我们带來了大量的好处,但是，下面情况应避免使用线程池：

1. CLR将线程池中的线程分配给任务，并在任务完成时将线程释放间池中。如果任务已经被添加到队列中，此时就没有直接的方法可以终止任务。
2. 线程池管理对于任务生存期较短的情况非常有效，例如Web服务器响应客户对某个特定文件的请求。线程池不适用又大又长的任务。
3. 线程池管理是一种以成本效率方式使用线程的技术，此处成本效率根据数量和启动开销来确定，决定使用池中线程的时候要十分小心。线程池的大小应该固定不变。
4. 线程池中的所有线程都是处于多线程单元之中。如果我们想把线程放置到单线程单元中，那么线程池就没有用了。
5. 如果我们想标识线程并执行各种操作，例如启动线程、挂起和中止等，那么线程池不能完成这样的工作。
6. 同样，我们不可能对使用线程池的任务设置优先级。
7. 对任意给定的进程，只能有一个线程池与其相关联。
8. 如果分配给线程池中的一个线程的任务被锁定，那么这个线程将不会再释放回池中。这种情况可以通过使用有效的编程技巧加以避免。

### 3. 线程池的大小

线程池中可以排队等待的任务数量取决于机器的内存数量。同样，在进程中可以激活的线程数量取决于机器中的CPU个数。
正如我们己经知道的，这是因为每个处理器在同一时间只能执行一个线程，默认情况下，处在多线程单元中的线程池的每个线程都将使用默认的任务，运行库将采用默认的优先级。此处使用的单词“默认”显得似乎有些不太明确，但这不会产生任何问题。每个系统都有的默认优先级设置。
在任意时刻，如果某个线程处于空闲状态，那么线程池就会引导工作者线程使所有的处理器保持繁忙。如果线程池中的所有线程都处于繁忙状态，并且队列中有未处理的任务，那么线程池将产生一个新的线程来完成待处理的工作。但是，产生的线程数量不能超过指定的最大值。



默认情况下，每个进程可以产生25个线程池线程。然而，这个数量可以通过编辑mscoree.h文件中定义的CorSetMaxThreads成员加以改变，万一要是有额外的线程请求的话，那么这个请求将加入到队列中，直到某些线程完成了分配给它的任务返回到线程池中为止。
.NET Framework对异步调用、建立套接字连接和注册过的等待操作等使用线程池功能。

## 三、ThreadPool 类

为了在应用程序中使用线程池，.NET Framework在System.Threading命名空间中提供了 ThreadPool类。ThreadPool类提供的线程池可以用来解决以下问题: 处理任务项、处埋异步I/O调用、处理计时器、代表其他线程等待。

##### BindHandle(SafeHandle)
将操作系统句柄绑定到 ThreadPool。返回值True绑定成功，False绑定失败。
osHandle           保存操作系统句柄的 SafeHandle。在非托管端必须为重叠 I/O 打开该句柄。

##### GetAvailableThreads(workerThreads, completionPortThreads) 
检索由 GetMaxThreads 方法返回的最大线程池线程数和当前活动线程数之间的差值。
WorkerThreads          线程池中工作者线程的最大数目。
completionPortThreads  线程池中异步 I/O 线程的最大数目。

##### GetMaxThreads(workerThreads, completionPortThreads)
检索可以同时处于活动状态的线程池请求的数目。所有大于此数目的请求将保持排队状态，直到线程池线程变为可用。
workerThreads           线程池中工作者线程的最大数目。
completionPortThreads   线程池中异步 I/O 线程的最大数目。

##### QueueUserWorkItem(WaitCallback)
##### ThreadPool.QueueUserWorkItem(WaitCallback, state)
将一个任务项排列到线程池中。返回值True执行成功，False执行失败。
callBack                一个 WaitCallback(委托)，表示要执行的方法。
State                   传递给委托的对象。

##### RegisterWaitForSingleObject(WaitHandle, WaitOrTimerCallback,state, Int32, Boolean)
注册一个等待 WaitHandle 的委托，并指定超时值（毫秒）。
waitObject                    要注册的WaitHandle。使用WaitHandle而非Mutex。
callBack                   向waitObject参数发出信号时调用的WaitOrTimerCallback委托。
State                       传递给委托的对象。
millisecondsTimeOutInterval 以毫秒为单位的超时。为0立即返回。为-1永远不过期。
executeOnlyOnce            为true表示委托后线程将不再在waitObject参数上等待；为false表示每次完成等待操作后都重置计时器，直到注销等待。

##### UnsafeRegisterWaitForSingleObject(WaitHandle,WaitOrTimerCallback,Object,Int32,Boolean)
注册一个等待 WaitHandle 的委托，并使用超时时间（毫秒）。此方法不将调用堆栈传播到辅助线程。
waitObject                     要注册的 WaitHandle。使用 WaitHandle 而非 Mutex。
callBack                       向 waitObject 参数发出信号时调用的委托。
State                          传递给委托的对象。
millisecondsTimeOutInterval    以毫秒为单位的超时。为0立即返回。为-1永远不过期。
executeOnlyOnce             为true表示委托后线程将不再在waitObject参数上等待；为false表示每次完成等待操作后都重置计时器，直到注销等待。

## 四、VB.NET中线程池的编程

ThreadPool类的3个规则：

1. 每个ThreadPool对象只能有一个工作者线程；
2. 每个进程只能有一个ThreadPool对象；
3. 第一次创建ThreadPool对象是当我们调用ThreadPool.QueueUserWorkItem( )方法，或者是通过计时器或已注册等待的操作调用回调方法时发生的。

ThreadPool类的一个普通用法就是不用设置每个线程的属性而启动多个独立的任务。
例：微软例子

```vbnet
Imports System.Threading  
Public Class Example  
    <MTAThread> Public Shared Sub Main()  
        ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf ThreadProc)) '委托方法入队  
        'ThreadPool.QueueUserWorkItem(AddressOf ThreadProc)              ‘等同上面语句  
  
        Console.WriteLine("Main thread does some wor, then sleeps.")  
        Thread.Sleep(1000)  
  
        Console.WriteLine("Main thread exits.")  
        Console.ReadLine()  
    End Sub  
    Shared Sub ThreadProc(stateInfo As Object) '无状态传来，为空  
        Console.WriteLine("Hello from the thread pool.")  
    End Sub  
End Class
```

说明：创建线程池后，两线程入队，Main线程活动时，ThreadProc只能等待；Main线程Sleep时，ThreadProc激活，其执行完后，Main线程又激活。
              
例：理解线程池中各线程执行次序。

```vbnet
Imports System.Threading  
Friend Class ObjState  
    Friend inarg1 As String  
    Friend inarg2 As String  
    Friend outval As String  
End Class  
Module ThreadAppModule  
    Sub Taskl(ByVal StateObj As Object)     '任务1  
        Dim StObj As ObjState = CType(StateObj, ObjState) '传来的状态转类型  
        Console.WriteLine("Input Argument 1 in task 1:" & StObj.inarg1)  
        Console.WriteLine("Input Argument 2 in task 1: " & StObj.inarg2)  
        StObj.outval = "From Task1 " & StObj.inarg1 & " " & StObj.inarg2 '此对象还可作返回值  
    End Sub  
    Sub Task2(ByVal StateObj As Object)     '任务2  
        Dim StObj As ObjState = CType(StateObj, ObjState)  
        Console.WriteLine("Input Argument 1 in task 2:" & StObj.inarg1)  
        Console.WriteLine("Input Argument 2 in task 2:" & StObj.inarg2)  
        StObj.outval = "From Task2 " & StObj.inarg1 & " " & StObj.inarg2  
    End Sub  
    Sub Main()  
        Dim StObj1 As New ObjState()  
        Dim StObj2 As New ObjState()  
        StObj1.inarg1 = "String Param1 of task 1" '分别设置两对象值  
        StObj1.inarg2 = "String Param2 of task 1"  
        StObj2.inarg1 = "String Param1 of task 2"  
        StObj2.inarg2 = "String Param2 of task 2"  
        '进程中只有一个Threadpool，且成员多为共享，故不必单独为其实例化对象，直接用类。  
        ThreadPool.QueueUserWorkItem(New Threading.WaitCallback(AddressOf Taskl), StObj1)  
        ThreadPool.QueueUserWorkItem(New Threading.WaitCallback(AddressOf Task2), StObj2)  
        Console.Read()  
    End Sub  
End Module
```

说明：任务1与任务2通过线程池逐个激活（反正不让任务线程空闲）,传递给委托方法的对象都被转了定义的ObjState类型。

例：理解定时触发器与开关门的关系

```vbnet
Imports System.Threading  
Public Class vbThreadPool  
    Private Shared i As Integer = 0  
    Public Shared Sub main()  
        Dim arev As New AutoResetEvent(False)      '1、自动同步  
        'Dim arev As New ManualResetEvent(False)   ‘2、手动同步  
        '注册(添加）一个定时器(定时触发委托项),第一参数为开关量以确定开或关，此处False为关，受阻。  
        ThreadPool.RegisterWaitForSingleObject(arev, AddressOf workitem, Nothing, 1000, False) '3、定时触发器  
        arev.Set()   '4、开关量打开(True),定时器启动（如Timer一样，每1秒调用委托方法)  
        Console.Read()  
    End Sub  
    Public Shared Sub workitem(ByVal obj As Object, ByVal signaled As Boolean)  
        i += 1  
        Console.WriteLine("Thread Pool Work Item Ivoked:" & i.ToString)  
    End Sub  
End Class
```

说明：本例代码简单，理解较难。

#### 1. 理解线程池的RegisterWaitForSingleObject。
它是一个定时触发器，类似Timer控件一样，即每隔一段时间触发委托方法。
第一参数WaitHandle是一个关开量（开True,关False）,它可以是手动同步ManualResetEvent或自动同步AutoResetEvent,它类似一个大门；第二参数委托方法，类似大门内关着的一匹马；第三参数state状态对象，类似给马的货物或装饰（当然也可以没有Nothing）；第四参数间隔时间,类似开大门时需要间隔多久（每开一次门，就跑出一匹马，方法就执行起来了），第五参数是否只开一次门（方法也就只能执行一次，如右图）。为真，总共只执行一次；为假，不止执行一次。
因此，上面3处代码可以解释为：每隔1秒触发打开大门，马儿就跑出来，马儿无货物托运，并且总共大门不止开一次。即一直间隔1秒地去开门。
#### 2. 理解Set
Set的目的，在AutoResentEvent自动同步中，直接打开门（True），于是下面左图或右图第一语句都是没有等1秒，就执行了。因为Set是另一个人来打开门（不是定时触发器来打开门），也即打开门有两种方式。既然门开了，马儿就直接跑出来了，还管什么定时触发器的1秒间隔？所以第一句都没有等待1秒，就直接显示出来了。
但后面语句(红箭头)都是等待了1秒才显示。这里又涉及到手动同步（ManualResetEvent）或自动同步（AutoResetEvent）的区别：
若是用的1处的自动同步（AutoResetEvent）,它就类似自动门一样，打开(Set为真)后，跑出一匹马，然后大门会自动关上(为假)，所以这里定时触发器的间隔时间就体现出来了：每隔一秒去开大门，跑出马后会自动关上大门，所以后面每隔一秒会显示信息。
若是用的2处的手动同步（ManualResetEvent），情况就不一样的，大门不是自动大门，打开(Set其为True)后不会自动关门(False)。如果用了4处的开大门，大门不会再关闭，大大地敞开，于是马儿一匹接一匹的向外跑，于是信息就无间隔的连续一直显示下去。定时触发器中的时间间隔就不会起作用。
本来定时触发器也有自动关门的作用，因为没有开也就没有与之对应的关（因为开是别人做的）,所以它也只有干瞪眼看着一匹接一匹的马儿跑。如果我们注释掉1处使用2处语句，同时注释掉4处，就会发现：第一句也会间隔1秒（因为没人去开门，所以定时触发器来开门），然后，后面也是每隔一秒显示（因为定时触发器自已开门，也会自动去关门）。


## 五、在.NET中的可伸缩性

Windows 操作系统管理着如何将线程分配给处理器，同时，触发任何进程会自动启动一个线程。.NET Framework对处理器分配不提供细粒度的控制，它宁愿让操作系统控制调度，因为与CLR相比，处理器提供更多的载入信息。然而，它还是对整个进程运行于哪个处理器提供了一些控制措施。但是这适用于进程中的所有线程，选择哪些处理器来处理超出了我们的话题。
如果只有一个线程（即主线程），那么线程中的每个任务都将运打在相同的处理器上。然而，如果创建一个新的线程，那么操作系统将安排线程将在哪个处理器上执行。系统在作出这个判断时会消耗一些处理器资源，所以对于那些小的任务而言，这种消耗通常不值得，因为执行这些任务的时间几乎和判断由哪个处理器来执行线程的时间一样。
然而，随着Windows版本的延续，这种分配占用的时间越来越少，同时对于除最细微的任务之外的事情而言,当使用线程时，您将发现通过创建新线程来执行任务将提高系统性能。这也只有在对称多处现器(symmetric multi-processor, SMP)系统中您才能看到线程的真正优点，因为所有的处理器都会被充分用于分配承担应用程序的负荷。

##### 通用线程池管理器类的设计
以前创建线程的方法过于松散自由，现在线程池又过于僵硬呆板。下面创建一个线程池管理类，它维护指定数量线程的线程池，这些线程用于请求的应用程序。这样既可以在代码中更加容易控制这些线程，同时在实例化线程对象时更快地执行线程。简单地说，就是综合两者的优点。
代码较长，逻辑图有点类似下面（引用别人的图），只是少了一部分超时的判断。

提示：右击关键词，可以选择“转到定义”（迅速到定义处）或“转到实现”（迅速到实现处）,工具栏左端面有一个“后退”，可迅速回到前一个代码定位处。

首先添加一个新的类（管理器类）:
```vbnet
Imports System.Threading  
Imports System.Text  
Namespace GenThreadPool '通用线程池管理  
    Public Interface IThreadPool       'ThreadPool接口，用于GenThreadPoolImpl类  
        Sub AddJob(jobToRun As Thread) '添加作业  
        Function GetStats() As Stats   '获取状态  
    End Interface  
  
    Public Class GenThreadPoolImpl  
        Implements IThreadPool  
        Private m_maxThreads As Integer         '最多线程  
        Private m_minThreads As Integer         '最少线程  
        Private m_maxIdleTime As Integer        '最长空闲时间（超时删除对应线程)  
        Private Shared m_debug As Boolean       '是否调试  
        Private m_pendingJobs As ArrayList      '等待的作业量（数组形式，以便列队进入线程池)  
        Private m_availableThreads As ArrayList '可用线程数  
  
        Public Property PendingJobs() As ArrayList  
            Get  
                Return m_pendingJobs  
            End Get  
            Set  
                m_pendingJobs = Value  
            End Set  
        End Property  
        Public Property AvailableThreads() As ArrayList  
            Get  
                Return m_availableThreads  
            End Get  
            Set  
                m_availableThreads = Value  
            End Set  
        End Property  
        Public Property Debug() As Boolean  
            Get  
                Return m_debug  
            End Get  
            Set  
                m_debug = Value  
            End Set  
        End Property  
        Public Property MaxIdleTime() As Integer  
            Get  
                Return m_maxIdleTime  
            End Get  
            Set  
                m_maxIdleTime = Value  
            End Set  
        End Property  
        Public Property MaxThreads() As Integer  
            Get  
                Return m_maxThreads  
            End Get  
            Set  
                m_maxThreads = Value  
            End Set  
        End Property  
        Public Property MinThreads() As Integer  
            Get  
                Return m_minThreads  
            End Get  
            Set  
                m_minThreads = Value  
            End Set  
        End Property  
  
        Public Sub New()       '默认构造。只允许1个线程在池中，0.3秒后销毁  
            m_maxThreads = 1  
            m_minThreads = 0  
            m_maxIdleTime = 300  
            m_pendingJobs = ArrayList.Synchronized(New ArrayList)      '对数组同步包装（仍为数组),因为这样上  
            m_availableThreads = ArrayList.Synchronized(New ArrayList) '锁才线程安全，以防其它线程同时进行修改。  
            m_debug = False  
        End Sub  
        Public Sub New(ByVal maxThreads As Integer, ByVal minThreads As Integer, ByVal maxIdleTime As Integer)  
            '构造函数，用3个参数实例化  
            m_maxThreads = maxThreads  
            m_minThreads = minThreads  
            m_maxIdleTime = maxIdleTime  
            m_pendingJobs = ArrayList.Synchronized(New ArrayList)  
            m_availableThreads = ArrayList.Synchronized(New ArrayList)  
            m_debug = False  
            InitAvailableThreads()  
        End Sub  
        Private Sub InitAvailableThreads() '初始化线程池，分别进入池中  
            If m_maxThreads > 0 Then  
                For i As Integer = 1 To m_maxThreads  
                    Dim t As New Thread(AddressOf (New GenPool(Me, Me)).Run)  
                    Dim e As New ThreadElement(t)  
                    e.Idle = True  
                    m_availableThreads.Add(e)  
                Next  
            End If  
        End Sub  
        Public Sub New(ByVal maxThreads As Integer, ByVal minThreads As Integer, ByVal maxIdleTime As Integer, ByVal debug As Boolean)  
            '构造函数，用4个参数实例化，增加调试参数  
            m_maxThreads = maxThreads  
            m_minThreads = minThreads  
            m_maxIdleTime = maxIdleTime  
            m_pendingJobs = ArrayList.Synchronized(New ArrayList)  
            m_availableThreads = ArrayList.Synchronized(New ArrayList)  
            m_debug = debug  
            InitAvailableThreads()  
        End Sub  
        Public Sub AddJob(ByVal job As Thread) Implements IThreadPool.AddJob '向池中添加作业  
            If job Is Nothing Then  
                Return  '作业不存在，退出  
            End If  
  
            SyncLock Me                   '锁定GenThreadPoolImpl，防止其它线程来添加或删除作业  
                m_pendingJobs.Add(job)    '将作业添加到 ArrayList 的结尾处。  
                Dim index As Integer = FindFirstIdleThread()     '空闲可用线程的索引  
                If m_debug Then  
                    Console.WriteLine("First Idle Thread Is " & index.ToString)  
                End If  
  
                If index = -1 Then        '-1无空闲线程，故需创建新的线程  
                    If m_maxThreads = -1 Or m_availableThreads.Count < m_maxThreads Then  
                        '池中无线程，或在用（有效）线程还未达最大线程数限制--->创建线程  
                        If m_debug Then  
                            Console.WriteLine("Creating a New thread")  
                        End If  
  
                        Dim t As New Thread(AddressOf (New GenPool(Me, Me)).Run)  
                        Dim e As New ThreadElement(t) '帮助类，提供线程额外属性  
                        e.Idle = False  
                        e.GetMyThread.Start()         '线程添加到ArrayList数组前先激发  
  
                        Try                           '添加  
                            m_availableThreads.Add(e)  
                        Catch ex As OutOfMemoryException  
                            Console.WriteLine("Out Of memory: " & ex.ToString)  
                            Thread.Sleep(3000)  
                            m_availableThreads.Add(e)  
                            Console.WriteLine("Added Job again")  
                        End Try  
                        Return  
                    End If  
  
                    If m_debug Then  
                        Console.WriteLine("No Threads Available...” & GetStats.ToString)  
                    End If  
                Else  '池中找到有空的线程  
                    Try  
                        If m_debug Then  
                            Console.WriteLine("Using an existing thread...")  
                        End If  
  
                        CType(m_availableThreads(index), ThreadElement).Idle = False   '标注忙碌  
                        SyncLock CType(m_availableThreads(index), ThreadElement).GetMyThread()  
                            Monitor.Pulse(CType(m_availableThreads(index), ThreadElement).GetMyThread())  
                        End SyncLock  
                    Catch ex As Exception  
                        Console.WriteLine(("Error while reusing thread " & ex.Message))  
                        If m_debug Then  
                            Console.WriteLine("Value of index Is " & index.ToString)  
                            Console.WriteLine("Size of available threads Is " & m_availableThreads.Count.ToString)  
                            Console.WriteLine("Available Threads Is " & m_availableThreads.IsSynchronized.ToString)  
                        End If  
                    End Try  
                End If  
            End SyncLock  
        End Sub  
        Public Function GetStats() As Stats Implements IThreadPool.GetStats  '池中状态  
            Dim statsInstance As New Stats()  
            statsInstance.MaxThreads = m_maxThreads  
            statsInstance.MinThreads = m_minThreads  
            statsInstance.MaxIdleTime = m_maxIdleTime                     '最大空闲时间  
            statsInstance.PendingJobs = m_pendingJobs.Count               '等待的作业量  
            statsInstance.NumThreads = m_availableThreads.Count           '有效线程数  
            statsInstance.JobsInProgress = m_availableThreads.Count - FindIdleThreadCount() '正在处理的作业量  
            Return statsInstance  
        End Function  
        Public Function FindIdleThreadCount() As Integer  '遍历有效线程，返回空闲线程数  
            Dim idleThreads As Integer = 0  
            For i As Integer = 0 To m_availableThreads.Count - 1  
                If CType(m_availableThreads(i), ThreadElement).Idle Then '根据帮助类中的空闲标志Idle来统计  
                    idleThreads += 1  
                End If  
            Next  
            Return idleThreads  
        End Function  
        Public Function FindFirstIdleThread() As Integer '遍历，找到第一个空闲线程，就立即返回其索引  
            For i As Integer = 0 To m_availableThreads.Count - 1  
                If CType(m_availableThreads(i), ThreadElement).Idle Then  
                    Return i  
                End If  
            Next  
            Return -1  
        End Function  
        Public Function FindThread() As Integer '查找当前线程的位置(索引号)，失败为-1（说明池中无线程）  
            For i As Integer = 0 To m_availableThreads.Count - 1  
                If CType(m_availableThreads(i), ThreadElement).GetMyThread.Equals(Thread.CurrentThread) Then  
                    Return i  
                End If  
            Next  
            Return -1  
        End Function  
        Public Sub RemoveThread() '移除线程  
            For i As Integer = 0 To m_availableThreads.Count - 1  
                If CType(m_availableThreads(i), ThreadElement).GetMyThread.Equals(Thread.CurrentThread) Then  
                    m_availableThreads.RemoveAt(i)  
                    Exit Sub  
                End If  
            Next  
        End Sub  
    End Class  
    Public Class GenPool '执行线程类（执行完毕后，过了指定超时时限，将自动从池中删除）  
        Private m_lock As Object           '锁定对象  
        Private m_gn As GenThreadPoolImpl  
        Public Sub New(lock_ As Object, gn As GenThreadPoolImpl)  
            m_lock = lock_  
            m_gn = gn  
        End Sub  
        Public Sub Run() '循环运行并检测是否过期  
            Dim job As Thread  
            While True '无限循环，一直检查池中状态  
                While True  
                    SyncLock m_lock  
                        If m_gn.PendingJobs.Count = 0 Then           '后续无作业进来  
                            Dim index As Integer = m_gn.FindThread() '取当前线程索引号  
                            If index = -1 Then               '无作业，池中也无线程，退出  
                                Exit Sub  
                            End If                                 '无作业，新的线程设为空闲  
                            CType(m_gn.AvailableThreads(index), ThreadElement).Idle = True  
                            Exit While  
                        End If  
                        job = CType(m_gn.PendingJobs(0), Thread) '有作业，取出（从原作业数组中删除)  
                        m_gn.PendingJobs.RemoveAt(0)  
                    End SyncLock  
                    job.Start()             '作业执行启动  
                End While  
                Try   '无后续作业  
                    SyncLock Me  
                        If m_gn.MaxIdleTime = -1 Then '池中无空闲线程，阻塞等待  
                            Monitor.Wait(Me)  
                        Else  
                            Monitor.Wait(Me, m_gn.MaxIdleTime)  
                        End If  
                    End SyncLock  
                Catch  
                End Try  
                SyncLock m_lock  
                    If m_gn.PendingJobs.Count = 0 Then '无等待的作业(没有新的作业进来）  
                        If m_gn.MinThreads <> -1 And m_gn.AvailableThreads.Count > m_gn.MinThreads Then  
                            m_gn.RemoveThread() '池中线程不空，且有效线程大于最小线程，删除线程  
                            Return  
                        End If  
                    End If  
                End SyncLock  
            End While  
        End Sub  
    End Class  
  
    Public Class ThreadElement '对应线程的线程帮助类（以例为之设计空闲标志，获取引用）  
        Private m_idle As Boolean   '空闲线程标志  
        Private m_thread As Thread  
        Public Sub New(th As Thread)  
            m_thread = th  
            m_idle = True      '初始化即为空闲  
        End Sub  
        Public Property Idle() As Boolean '设置或获取线程空闲标志  
            Get  
                Return m_idle  
            End Get  
            Set  
                m_idle = Value  
            End Set  
        End Property  
        Public Function GetMyThread() As Thread '取得原线程  
            Return m_thread  
        End Function  
    End Class  
  
    Public Structure Stats  '状态统计  
        Public MaxThreads As Integer  
        Public MinThreads As Integer  
        Public MaxIdleTime As Integer  
        Public NumThreads As Integer  
        Public PendingJobs As Integer     '列队等待的作业量  
        Public JobsInProgress As Integer  '正在处理的作业量  
        Public Overrides Function ToString() As String        '提取状态  
            Dim sb As New StringBuilder("MaxThreads = ", 107) '容量大小107字符  
            sb.Append(MaxThreads)  
            sb.Append(ControlChars.Lf & "MinThreads=" & MinThreads)  
            sb.Append(ControlChars.Lf & "MaxIdleTime=" & MaxIdleTime)  
            sb.Append(ControlChars.Lf & "PendingJobs=" & PendingJobs)  
            sb.Append(ControlChars.Lf & "JobsInProgress=" & JobsInProgress）  
            Return sb.ToString  
        End Function  
    End Structure  
End Namespace  
```

然后，完成主程序代码，用来测试：

```vbnet
Imports System.Threading  
Namespace TestGenThreadPool  
    Public Class TestPerformance  
        Public count As Integer  
        Private m_lock As New Object()  
  
        Public Sub New(pool As GenThreadPool.IThreadPool, times As Integer)  
            Console.WriteLine("Performance using Pool [in ms]: ")  
            count = 0  
            Dim start As Long = Now.Millisecond  
            Console.WriteLine("Start Time For Job Is " & Now)  
            Dim i As Integer  
            For i = 0 To times - 1  
                Dim tl As New Thread(AddressOf (New Job(Me)).Run)  
                pool.AddJob(tl)  
            Next  
  
            While True  
                SyncLock m_lock  
                    If count = times Then  
                        Exit While  
                    End If  
                End SyncLock  
  
  
                Try  
                    Thread.Sleep(5000)  
                Catch  
                End Try  
            End While  
  
            Console.WriteLine(" " & (Now.Millisecond - start).ToString)  
            Console.WriteLine("End Time for Job is " & Now.ToString)  
            Console.WriteLine("Performance using no Pool [in ms]: ")  
            count = 0  
            start = Now.Millisecond  
            Console.WriteLine("Start Time for JobThread is " & Now.ToString)  
            For i = 0 To times - 1  
                Dim jt As New Thread(AddressOf (New JobThread(Me)).Run)  
                jt.Start()  
            Next  
  
            While True  
                SyncLock m_lock  
                    If count = times Then  
                        Exit While  
                    End If  
                End SyncLock  
                Try  
                    Thread.Sleep(5000)  
                Catch  
                End Try  
            End While  
            Console.WriteLine(" " & (Now.Millisecond - start).ToString())  
            Console.WriteLine("End Time for JobThread is ” & Now.ToString)  
        End Sub  
        NotInheritable Class JobThread  
            Private m_lock As New Object()  
            Private tpf As TestPerformance  
            Public Sub New(tpf_ As TestPerformance)  
                tpf = tpf_  
            End Sub  
            Public Sub Run()  
                SyncLock m_lock  
                    tpf.count += 1  
                End SyncLock  
            End Sub  
        End Class  
        NotInheritable Class Job  
            Private m_lock As New Object()  
            Private tpf As TestPerformance  
  
            Public Sub New(tpf_ As TestPerformance)  
                tpf = tpf_  
            End Sub  
            Public Sub Run()  
                SyncLock m_lock  
                    tpf.count += 1  
                End SyncLock  
            End Sub  
        End Class  
    End Class  
    Class TestPool  
        Private Shared i As Integer = 0  
        Private j As Integer = 0  
        Public Sub Run()  
            i += 1  
            j = i  
            Console.WriteLine("Value of i in run is {0} ", j)  
        End Sub  
        Public Shared Sub Main(args() As String)  
            Dim tp = New GenThreadPool.GenThreadPoolImpl(1000, 1000, 300, True)  
            Dim i As Integer  
            For i = 0 To 99 '添加作业到线程池管理器  
                Dim td1 As New TestPool  
                Dim t1 As New Thread(AddressOf td1.Run)  
                Dim td2 As New TestPool  
                Dim t2 As New Thread(AddressOf td2.Run)  
                Dim td3 As New TestPool  
                Dim t3 As New Thread(AddressOf td3.Run)  
                Dim td4 As New TestPool  
                Dim t4 As New Thread(AddressOf td4.Run)  
                Dim td5 As New TestPool  
                Dim t5 As New Thread(AddressOf td5.Run)  
                Dim td6 As New TestPool  
                Dim t6 As New Thread(AddressOf td6.Run)  
                Dim td7 As New TestPool  
                Dim t7 As New Thread(AddressOf td7.Run)  
                Dim td8 As New TestPool  
                Dim t8 As New Thread(AddressOf td8.Run)  
                Dim td9 As New TestPool  
                Dim t9 As New Thread(AddressOf td9.Run)  
                Dim td10 As New TestPool  
                Dim t10 As New Thread(AddressOf td10.Run)  
                Dim td11 As New TestPool  
                Dim t11 As New Thread(AddressOf td11.Run)  
                tp.AddJob(t1)  
                tp.AddJob(t2)  
                tp.AddJob(t3)  
                tp.AddJob(t4)  
                tp.AddJob(t5)  
                tp.AddJob(t6)  
                tp.AddJob(t7)  
                tp.AddJob(t8)  
                tp.AddJob(t9)  
                tp.AddJob(t10)  
                tp.AddJob(t11)  
            Next  
            Dim td12 As New TestPool  
            Dim t12 As New Thread(AddressOf td12.Run)  
            tp.AddJob(t12)  
            Dim p As New TestPerformance(tp, 1000)  
        End Sub  
    End Class  
End Namespace  
```

结果较长，同时播放视频时，就可明显感觉到视频有点卡顿，说明CPU有点忙不过来了：）

> ![]()