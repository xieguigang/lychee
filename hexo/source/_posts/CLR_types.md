---
title: What does the Visual Studio “Any CPU” target mean?
tags: [note, VisualStudio, Compiler]
date: 2016-06-11
---

> stackoverflow: http://stackoverflow.com/questions/516730/what-does-the-visual-studio-any-cpu-target-mean

An ``AnyCPU`` assembly will ``JIT`` to ``64 bit`` code when loaded into ``64 bit`` process and ``32 bit`` when loaded into a ``32 bit`` process.
By limiting the CPU you would be saying there is something being used by the assembly (something likely unmanaged) that requires 32 bits or 64 bits.
I think most of the important stuff has been said, but I just thought I'd add one thing:

+ If you compile as ``Any CPU`` and run on an ``x64`` platform, then you won't be able to load 32-bit dlls, because your app wasn't started in ``WOW64``, but those dlls need to run there.
+ If you compile as ``x86``, then the ``x64`` system will run you app in ``WOW64``, and you'll be able to load ``32-bit`` dlls.

So I think you should choose ``Any CPU`` if your dependencies can run in either environment, but choose ``x86`` if you have ``32-bit`` dependencies. This article from Microsoft explains this a bit:

> [/CLRIMAGETYPE (Specify Type of CLR Image)](https://msdn.microsoft.com/en-us/library/31zwwc39.aspx)

<!--more-->

## /CLRIMAGETYPE (Specify Type of CLR Image)

``Visual Studio 2015``

```
/CLRIMAGETYPE:{IJW|PURE|SAFE|SAFE32BITPREFERRED}
```

#### Remarks
The linker accepts native objects and also ``MSIL`` objects that are compiled by using ``/clr``, ``/clr:pure``, or ``/clr:safe``. When mixed objects in the same build are passed, the verifiability of the resulting output file is, by default, equal to the lowest level of verifiability of the input modules. For example, if you pass both a safe and a pure module to the linker, the output file will be pure. If you pass a native image and a mixed mode image (compiled by using ``/clr``), the resulting image will be a mixed mode image.
You can use ``/CLRIMAGETYPE`` to specify a lower level of verifiability, if that is what you need.
In .NET 4.5, ``/CLRIMAGETYPE`` supports a ``SAFE32BITPREFERRED`` option. This sets—in the ``PE`` header of the ``image—flags`` that indicate that the ``MSIL`` objects are safe and can be run on all platforms, but that ``32-bit`` execution environments are preferred. This option enables an app to run on ARM platforms and also specifies that it should run under ``WOW64`` on ``64-bit`` operating systems instead of using the ``64-bit`` execution environment.
When an ``.exe`` that was compiled by using ``/clr`` or ``/clr:pure`` is run on a ``64-bit`` operating system, the application is run under ``WOW64``, which enables a ``32-bit`` application to run on a ``64-bit`` operating system. By default, an .exe that's compiled by using ``/clr:safe`` is run under the operating system's ``64-bit`` support. However, it is possible that your safe application loads a ``32-bit`` component. In that case, a safe image running under the operating system's ``64-bit`` support will fail when it loads the ``32-bit`` application. To ensure that a safe image continues to run when it loads a ``32-bit`` component on a ``64-bit`` operating system, use the ``/CLRIMAGETYPE:SAFE32BITPREFERRED`` option. If your code does not have to run on ``ARM`` platforms, you can specify the ``/CLRIMAGETYPE:PURE`` option to change the metadata (``.corflags``), marking it to be run under ``WOW64`` (and substituting your own entry symbol):

```
cl /clr:safe t.cpp /link /clrimagetype:pure /entry:?main@@$$HYMHXZ /subsystem:console
```

For information about how to determine the ``CLR`` image type of a file, see [``/CLRHEADER``](https://msdn.microsoft.com/en-us/library/ds03hhk8.aspx).

##### To set this linker option in the Visual Studio development environment
Open the project's **Property Pages** dialog box. For details, see [How to: Open Project Property Pages](https://msdn.microsoft.com/en-us/library/e79xc5h1.aspx).
Expand the **Configuration Properties** node.
Expand the **Linker** node.
Select the **Advanced** property page.
Modify the **CLR Image Type** property.

##### To set this linker option programmatically
See [CLRImageType](https://msdn.microsoft.com/en-us/library/microsoft.visualstudio.vcprojectengine.vclinkertool.clrimagetype.aspx).

##### See Also
[Setting Linker Options](https://msdn.microsoft.com/en-us/library/wk97ab1b.aspx)
[Linker Options](https://msdn.microsoft.com/en-us/library/y0zzbyt4.aspx)