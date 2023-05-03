## 自动加载与初始化

### 1、简单版

为了将程序集的初始化和通过写注册表的方式实现自动加载统一设置，减少每次重复的工作量，类裤提供了`AutoLoad`抽象类来完成此功能，只要在需要初始化的类继承`AutoLoad`类，然后实现`Initialize()` 和 `Terminate()` 两个函数就可以了。
特别强调的是，一个程序集里只能有一个类继承，不管是不是同一个命名空间。

如果要将dll的目录加入支持文件目录，请在 `Initialize` 函数中调用`AppendSupportPath(CurrentDirectory.FullName);`

其他需要初始化执行的函数及设置都需要在 `Initialize` 函数中执行。

### 2、功能版

使用特性进行分段初始化是目前最佳选择，下面的说明已和最新版本不符，等待修正吧。

```csharp
 using Autodesk.AutoCAD.Runtime;
 using IFoxCAD.Cad;
 using System;
 using System.Reflection;

/*
 * 自动执行接口
 * 这里必须要实现一次这个接口,才能使用 IFoxInitialize 特性进行自动执行
 */
public class CmdINI : AutoRegAssem
{
    // 这里可以写任何普通的函数，也可以写下面 AutoTest 类里的实现了 IFoxInitialize 特性的初始化函数
    // 继承AutoRegAssem的主要作用是写注册表用来自动加载dll，同时执行实现了 IFoxInitialize 特性的函数
    // 注意这里的自动执行是在cad启动后，加载了dll之后执行，而不是运行命令后执行。

    [IFoxInitialize]
    public void InitOne()
    { 
        // TODO 您想在加载dll之后自动执行的函数
        // 可以随便在哪里类里 可以多次实现 IFoxInitialize 特性
    }

}

// 其他的类中的函数:
// 实现自动接口之后,在任意一个函数上面使用此特性,减少每次改动 CmdINI 类
public class AutoTest
{
    [IFoxInitialize]
    public void Initialize()
    { 
        // TODO 您想在加载dll之后自动执行的函数
    }
    [IFoxInitialize]
    public void InitTwo()
    { 
        // TODO 您想在加载dll之后自动执行的函数
        // 可以随便在哪里类里 可以多次实现 IFoxInitialize 特性
    }
    [IFoxInitialize(isInitialize: false)] // 特性的参数为false的时候就表示卸载时执行的函数
    public void Terminate()
    {
         // TODO 您想在关闭cad时自动执行的函数
    }
}
```
