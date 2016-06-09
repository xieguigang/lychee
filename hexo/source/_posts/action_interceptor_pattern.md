---
title: The Action Interceptor Pattern
tags: [CodeProject,note,转载搬运,architecture,vb.net]
date: 2016-06-09
source: http://www.codeproject.com/Articles/1104555/The-Action-Interceptor-Pattern?msg=5258531#xx5258531xx
---

> source: http://www.codeproject.com/Articles/1104555/The-Action-Interceptor-Pattern?msg=5258531#xx5258531xx

## Introduction

In Interceptor in the Wild by João Matos Silva has presented how to use an IoC framework, NInject, to inject new behaviors into existing methods without hacking into the method implementations. In this article, we will see an old-school yet beneficial way to reach the same goal, but without messing around with IoC frameworks.

![](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/Class.png)

<!--more-->

## Background

>**Notice:**
The article Interceptor in the Wild by João Matos Silva was very well written and pleased to follow. This article offers the alternative to it and reuses examples in it. I suggest you read it before going on with this alternative.

In Silva's article, the good sides of IoC frameworks were demonstrated. However, there are pros and cons in IoC frameworks in practical use. Please correct me if any of the following listed is wrong.

1. **Abstraction of object creation**
    + **Pros**: Dynamic object factory can be achieved at runtime.
    + **Cons**:
        a. It hides the constructor of the types, which may be difficult for programmers to follow when the injection chains become complicated. 
	    "What instance is resolved by the IoC?" is one of the most common issue programmers will usually encounter.
        b. It requires some efforts when we try to instantiate objects, if we are trying to call:
        [**b1**] _Constructors with parameters: relative discussions here and there. And this can usually causes problems when the programmers are refactoring the constructor, 
		without knowing that it is somewhere referenced by the IoC module indirectly._
        [**b2**] _Private or internal constructors: relative discussion here and there._
        [**b3**] _Constructors which could throw exceptions, and we have to work harder to address them._
        c. **Performance penalty**.
2. **Runtime parameterization**. It is not so easy to alter the parameter of interceptors in IoC. And again, performance penalty here is unavoidable.
3. **Partially overlapped interception**: An IoC binder affects all methods bounded to the type via the given interface. Given that we have a type with three methods: M1, M2 and M3, and we are going to use all of them in the code. We want to bind behavior B1 to M1, B1 and B2 to M2, but no behavior to M3. Afterwards, we are going to call the bounded M1, bounded M2, and the unbounded M3. You need to write a lot to achieve this. It is not so easy to implement, understand or debug with IoC frameworks at all.
4. IoC binders only affect instance methods, not static methods, unless you wrap them with instance classes.
5. **Complexity**. IoC requires the programmer to program new classes or interfaces for behavioral interceptors, thus more types have to be added to the project.

## An Alternative to IoC Injections
If injecting functionalities to methods is our goal, and the above issues in IoC frameworks are your headache, the Action Interceptor Pattern is your way to go. The following is an example of an Action Interceptor. It can be as simple as the following five lines:

```vbnet
<Extension>
Public Function WaitAMinute(Of TArg, TResult)(func As Func(Of TArg, TResult)) As Func(Of TArg, TResult)
    Return Function(arg)
               Call Thread.Sleep(TimeSpan.FromMinutes(1))
               Return func(arg)
           End Function
End Function
```

You may ask: What? An extension method of Func?

Yep! Wrapping a function with another function, and the job will be done. Just a single method will save our days. Not yet convinced? Please read on.

## The Usage of Action Interceptors
I will use the NativeClient example in Interceptor in the Wild to demonstrate how Action Interceptors can be an alternative to IoC injections.

### Decreasing Fault Rate / Increasing Success Rate

In the Silva's article Interceptor in the Wild, the NativeClient very well shows us a service which can be broken at any time, and the RetryInterceptor remedies this by retrying when something goes wrong. The same pattern can be implemented with Action Intercetor.

To demonstrate the result. Let's take a look back to the primitive client code:

```vbnet
Const TimesToInvoke As Integer = 1000

Public Sub Main(args As String())
    Dim counter = New StatsCounter()
    Dim client = New NaiveClient(counter)
    
    counter.Stopwatch.Start()
    
    For i As var = 0 To TimesToInvoke - 1
        Try
            client.GetMyDate(DateTime.Today.AddDays(i Mod 30))
            counter.TotalSuccess += 1
        Catch ex As Exception
            counter.TotalError += 1
        End Try
    Next
	
    counter.Stopwatch.[Stop]()
    counter.PrintStats()
    Console.WriteLine("Press any key to exit")
    Console.ReadKey()
End Sub
```

In the above code, you don't need to care about how GetMyDate was implemented, since we will not touch it and we will just change the client side code which calls that function. Please just keep in mind that it was a broken function and it had a probability of 0.3 to throw exceptions.

The result of execution was, at no doubt, very bad. It took a long time to run and execution fail rates were as high as 325/1000. Here was a possible output.

```json
Execution Time: 36.0658004 seconds
Total Executions: 1000
Execution Sucess: 675
Total Sucess: 675
Execution Fail: 325
Total Fail: 325
```

Now, we will introduce a new RetryIfFailed action interceptor to automatically retry the action after failures, and we can easily configure how many times we want to retry at run time. by assigning value to the maxRetry parameter.

```vbnet
    <Extension>
    Public Function RetryIfFailed(Of TArg, TResult)(func As Func(Of TArg, TResult), maxRetry As Integer) As Func(Of TArg, TResult)
        Return _
            Function(arg)
                Dim t As New Pointer(Scan0)
				
                Do
                    Try
                        Return func(arg)
                    Catch generatedExceptionName As Exception
                        If ++t > maxRetry Then
                            Throw
                        End If
                    End Try
                Loop While True
            End Function
    End Function
```

Then, we apply it to the calling procedure.

```vbnet
Dim counter = New StatsCounter()
Dim client = New NaiveClient(counter)
' get the method we are going to retry with
Dim getMyDate As Func(Of DateTime, String) = AddressOf client.GetMyDate

Call counter.Stopwatch.Start()
' intercept it with RetryIfFailed interceptor, which retries once at most
getMyDate = getMyDate.RetryIfFailed(1)

For i As Integer = 0 To TimesToInvoke
    Try 
        ' call the intercepted method instead of client.GetMyDate
        getMyDate(DateTime.Today.AddDays(i Mod 30))
        counter.TotalSuccess++
    Catch ex As Exception 
        counter.TotalError+=1
    End Try
Next

Call counter.Stopwatch.Stop()
```

The above program gave a better result. The execution fault rate dropped from the original 325/1000 to 91/1000, simply because we have retried once more.

```
Execution Time: 59.6016893 seconds
Total Executions: 1302
Execution Sucess: 909
Total Sucess: 909
Execution Fail: 393
Total Fail: 91
```

If you are not happy with the result, you can tweak the maxRetry parameter in the interceptor to a larger value, for instance 3. Here was the result of "getMyDate.RetryIfFailed(3)" and the fault rate was reduced to around 8/1000.

```
Execution Time: 65.8972493 seconds
Total Executions: 1421
Execution Sucess: 992
Total Sucess: 992
Execution Fail: 429
Total Fail: 8
```

### Caching the Result

In Interceptor in the Wild, Silva also demonstrated caching can be injected to methods via a PoorMansCacheProvider and a CacheInterceptor. The same result can be achieved with the Action Interceptor pattern.

```csharp
<Extension>
Public Function GetOrCache(Of TArg, TResult, TCache As {Class, IDictionary(Of TArg, TResult)})(func As Func(Of TArg, TResult), cache As TCache) As Func(Of TArg, TResult)
    Return _
        Function(arg)
            Dim value As TResult

            If cache.TryGetValue(arg, value) Then
                Return value
            End If

            value = func(arg)
            cache.Add(arg, value)

            Return value
        End Function
End Function
```

We can easily apply it to the calling procedure, after the RetryIfFailed interceptor.

```vbnet
Dim counter = New StatsCounter()
Dim client = New NaiveClient(counter)
Dim cache = New Dictionary(Of DateTime, String)()
Dim getMyDate As Func(Of DateTime, String) = AddressOf client.GetMyDate

counter.Stopwatch.Start()
getMyDate = getMyDate.RetryIfFailed(3).GetOrCache(cache)

For i As Integer = 0 To TimesToInvoke - 1
    Try
        getMyDate(DateTime.Today.AddDays(i Mod 30))
        counter.TotalSuccess += 1
    Catch ex As Exception
        counter.TotalError += 1
    End Try
Next

counter.Stopwatch.[Stop]()
counter.PrintStats()

Console.WriteLine("Benchmarks: ")
BenchmarkWithActionInterceptor()
BenchmarkWithNInjectInterceptor()
Console.WriteLine("Press any key to exit")
Console.ReadKey()
```

>Note:
Although **GetOrCache** is appended after **RetryIfFailed**, since it is actually wrapping around the original function, the cache provided to GetOrCache will be firstly accessed before RetryIfFailed is call.
>
In short, the later attached methods will be executed earlier.


The result may look like the following, similar to the final result in Interceptor in the Wild. Since cache was in the play, the execution time was dramatically shortened and the total execution fault rate was dramatically reduced to around zero.

```
Execution Time: 0.981474 seconds
Total Executions: 36
Execution Sucess: 30
Total Sucess: 1000
Execution Fail: 6
Total Fail: 0
```

Until now, what we have done is simply adding two extension methods and changing 4 lines of code in the calling procedure.

1. No need to modify the implemetation of the underlying function
2. No need to NuGet a single bit from the Internet
3. No need to learn a single new framework
4. No need to struggle with object instantiation
5. No need to think about how to alter parameters in interceptors

Jobs got done and our days were saved.

## Points of Interest - Performance
On performance, I enclosed a simple benchmark which compares the speed of Action Inteceptor and NInject (an IoC framework) Interceptor in the code.

The code will call a NopClient which actually does nothing to compare the time spent on different types of interceptions.

```vbnet
Public Class NopClient : Implements INaiveClient

    Dim _counter As StatsCounter

    Public Sub New(counter As StatsCounter)
        _counter = counter
    End Sub

    Public Function GetMyDate([date] As DateTime) As String Implements INaiveClient.GetMyDate
        Return Nothing
    End Function
End Class
```

The setup code of NInject interceptor looks like the following:

```vbnet
Public Class [Module] : Inherits NinjectModule
    Public Overrides Sub Load()
        Kernel.Bind(Of StatsCounter)().ToConstant(New StatsCounter())
        Dim binding = Kernel.Bind(Of INaiveClient)().[To](Of NopClient)()
        binding.Intercept().[With](Of RetryInterceptor)()
    End Sub
End Class
```

During practical development, we typically create a client object which is not reusable and call its methods. For instance, we create WebRequest objects to load a web page or DbConnection objects to manage the database. Hence, in performance benchmarks, we must take object initialization into account.

Here are what we are going to time during the benchmark.

The speed of Action Interceptor.

```vbnet
For i As Integer = 0 To TimesToInvoke - 1
    Dim nopClient = New NopClient(counter)
    Dim getMyDate As Func(Of DateTime, String) = AddressOf nopClient.GetMyDate

    getMyDate = getMyDate.RetryIfFailed(3)
    getMyDate(DateTime.MinValue)
Next
```

And the speed of a NInject interceptor.

```vbnet
        For i As Integer = 0 To TimesToInvoke - 1
            Dim client = kernel.[Get](Of INaiveClient)()
            client.GetMyDate(DateTime.MinValue)
        Next
```

And here was a result taken from my computer. Action Interceptor was about hundreds of times faster than the NInject Interceptor.

```il
Action interceptor:
Execution Time: 0.1758 milliseconds

NInject interceptor:
Execution Time: 144.2733 milliseconds
```

## Conclusion
The **Action Interceptor** pattern has the following features which are the same as IoC frameworks:

	1. It does inject new behaviors to methods we want to change as IoC does (introduced in the Interceptor in the Wild): 
	   increasing execution success rates, providing cachability and efficiency, and more.
	2. The underlying basic implementation remains unchanged. 
	   It does not require you to change a single character in the injected method.

And it has the following advantages over IoC frameworks:

1. The construction logic of types is preserved. So you can happily use the old-school new className(parameters) fashion to instantiate objects or apply any factory patterns if needed. The object creation logic therefore remains clean, simple and fully in your control.
2. It does not force you to use the "injected" version all the time, you can return to use the un-injected original method any time as needed.
3. Functionalities are clearly, selectively and freely added to existing methods.
4. No new class or interface is required to be created manually.
5. New behaviors can be injected to already injected methods at run time, after object initialization.
6. It is open and fully compatible with IoC frameworks. You can take advantages of their abstraction object factory.
7. It can be applied to static methods or private methods.
8. Its performance is much superior than IoC frameworks.
9. It has no external dependency. No need to reference any third party assemblies.
10. Plain easy to learn.
11. Plain easy to debug.
12. Ready for being refactored.

The disadvantages are:

1. It does not serve as an abstract object factory.
2. It does not wrap properties, except that you wrap the hidden getter or setter method of a property.
3. It does not inject behaviors to all methods in a type by default. You must manually apply it to methods, one by one.
4. It is old-fashioned and low-tech. It does not sound COOL in front of your boss at all.
5. It is up to your decision.

IoC frameworks do give us great powers. But we have to face the tradeoffs of added code complexity and performance loss. When I saw people asking questions about IoC frameworks here and there, which were originally simple without IoC, I wondered whether they were using the right tool.