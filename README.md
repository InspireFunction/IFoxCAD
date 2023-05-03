# IFoxCAD 说明

基于.NET的Cad二次开发类库。

#### 一、项目来源

起初 **雪山飞狐（又狐哥）** 在明经论坛发布了[开源库](http://bbs.mjtd.com/thread-75701-1-1.html)，后来狐哥自己的项目进行了极大的丰富后形成NFox类库。然后 **落魄山人** 在征得 雪山飞狐的同意后，对NFox类库进行了整理，增加了注释等，重新发布了NFox类库。

后来，经过一段时间的更新后，由于莫名其妙的原因NFox类库挂掉了。而这时山人同学已经基本吃透NFox类库，考虑到NFox的封装过于复杂，遂进行了重构。

重构的类库命名为IFoxCAD， 寓意为：**I(爱)Fox(狐哥)**，本项目发布于**Inspire Function（中文名：跃动方程）** 组织下，感谢 **小轩轩** 给起的名字。

可以加群交流：

![ifoxcad用户交流群群二维码](./docs/png/ifoxcad用户交流群群二维码.png)

#### 二、 快速入门

- 打开vs，新建一个standard类型的类库项目，**注意，需要选择类型的时候一定要选standard2.0** 

- 双击项目，打开项目文件：
  
  - 修改项目文件里的`<TargetFramework>netcore2.0</TargetFramework>`为`<TargetFrameworks>NET45</TargetFrameworks>`。其中的net45，可以改为NET45以上的标准TFM（如：net45、net46、net47等等）。同时可以指定多版本。具体的详细的教程见 [VS通过添加不同引用库，建立多条件编译](https://www.yuque.com/vicwjb/zqpcd0/ufbwyl)。
  
  - 在 `<PropertyGroup> xxx </PropertyGroup>` 中增加 `<LangVersion>preview</LangVersion>`，主要是为了支持最新的语法，本项目采用了最新的语法编写。项目文件现在的内容类似如下：

```xml
<Project Sdk="Microsoft.NET.Sdk">
   <PropertyGroup>
       <TargetFramework>net45</TargetFramework>
       <LangVersion>preview</LangVersion>
   </PropertyGroup>
</Project>
```

- 右键项目文件，选择管理nuget程序包。

- 在nuget程序里搜索**ifox**，记得将包括预发行版打钩。截止本文最后更新时，nuget上最新的版本为ifox.cad.source 0.5.2.1版本和ifox.Basal.source 0.5.2.3版本。点击安装就可以。

- 添加引用，在新建的项目里的cs文件里添加相关的引用

```csharp
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using IFoxCAD.Cad;
```

- 添加代码

```csharp
[CommandMethod(nameof(Hello))]
public void Hello()
{
    using DBTrans tr = new();
    var line1 = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
    tr.CurrentSpace.AddEntity(line1);
    // 如果您没有添加<LangVersion>preview</LangVersion>到项目文件里的话：按如下旧语法：
    // using(var tr = new DBTrans())
    // {
    //     var line1 = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
    //     tr.CurrentSpace.AddEntity(line1);
    // }
}
```

这段代码就是在cad的当前空间内添加了一条直线。

- 生成，然后打开cad，netload命令将刚刚生成的dll加载。如果需要调试需要设置启动程序为cad。

- 运行hello命令，然后缩放一下视图，现在一条直线和一个圆已经显示在屏幕上了

#### 三、屏蔽IFox的元组、索引、范围功能

<mark>特别提醒：</mark> 考虑到早期的框架没有提供System.Range类型(net core 开始提供)、System.Index类型(net core 开始提供)、System.ValueTuple类型(net 47开始提供)，本项目IFox.Basal包里包含了他们。 如果引用了包含System.Range等类型的第三方包（如IndexRange等），请在项目文件中定义NOINDEX、NORANGE、NOVALUETUPLE常量，以避免重复定义。上述代码能起作用的前提是用源码包，普通包暂时无解。

```xml
<PropertyGroup Condition="'$(TargetFramework)' == 'NET47'">
	<DefineConstants>$(Configuration);NOINDEX;NORANGE;NOVALUETUPLE</DefineConstants>
</PropertyGroup>
```

**NOINDEX、NORANGE、NOVALUETUPLE 分别针对三种类型，哪种类型冲突就定义哪种。**

#### 四、编译 IFox 源码工程

由于vs2022抛弃了某几个net版本，所以我们同时安装vs2019和vs2022，然后使用vs2022;
其中的原因是vs2019拥有全部net版本,而vs2022拥有最新的分析器和语法。

编译本项目需要你准备好git，具体的安装教程可以去网上搜索一下。当然也可以利用vs的git来完成。

首先在gitee上fork本项目到你的账号，然后clone到本地。

原生git使用命令行，打开终端/powershell/cmd，cd到你要存放的目录，然后运行下面的命令，把里面的yourname替换为你的名字，这样就在本地创建了一个ifoxcad文件夹，里面就是本项目的所有源码。

```
git clone https://gitee.com/yourname/ifoxcad.git
```

当然也可以采用vs的图形化操作，打开vs，选择 克隆存储库-->填入仓库地址和存放路径-->点击克隆。新手小白推荐用此办法。

打开ifoxcad文件夹，双击解决方案文件，打开vs，等待项目打开，加载nuget包，然后生成就可以了。

**切记，不要用低版本的vs打开本项目，因为本项目采用了某些新的语法，所以老版本的vs是不兼容的。**

#### 五、IFoxCad 项目模版

可以在vs扩展菜单-管理扩展中搜索ifoxcad，即可安装项目模板。使用项目模版可以方便的创建支持多目标多版本的使用ifoxcad类库的项目和类。如果无法在vs的市场里下载，就去上面的QQ群里下载。

项目模版里的自动加载选择了简单api，ifox还提供了一套功能更强大的api，具体的可以参考[自动加载和初始化](/docs/autoreg.md)。

#### 六、使用IFoxCad的几种方式

目前ifox提供了三种使用方式，**建议一般的用户使用第二种源码包的形式。有志于本项目发展并想提交点代码的可以选择第三种。**

- 第一种是直接使用普通的nuget包。
  
  此种方式使用便捷，只要在项目中引用了IFox.CAD.ACAD的包，就可以直接使用了。缺点一是无法控制ifox提供的元组功能的屏蔽，导致和其他的三方包的冲突；二是生成目录里带有ifox的dll。

- 第二种是使用源码包。
  
  此种方式使用便捷，只要在项目中引用了IFox.Basal.Source和IFox.CAD.Source两个nuget包就可以直接使用了。优点就是使用简单，生成的目录里没有ifox的dll，同时还可以通过定义预处理常量的方式屏蔽ifox提供的元组等功能。缺点就是无法修改源码，即便解包修改了，也不会同步到nuget上。

- 第三种是使用git子模块。
  
  此种方法使用步骤复杂，需要熟悉git及其子模块的使用，需要引用ifox里的共享项目文件。优点就是可以使用最新的代码，可以修改代码。具体的可以参考如下说明进行：
  
  **让 IFox 作为您的子模块**
  
  IFox的develop分支是一个多cad版本分支,您可以利用此作为您的[git项目子模块](https://www.cnblogs.com/JJBox/p/13876501.html#_label13).
  
  子模块是以`共享工程`的方式加入到您的工程的,其为`IFox.CAD.Shared`:
1. 千万不要用`IFox.CAD.ACAD`内的工程作为引用,否则您将遭遇cad加载失效.

2. 一些全局命名空间的缺少,我们也建议您使用全局命名空间来补充,
   您只需要按照`IFox.CAD.ACAD`的`GlobalUsings.cs`文件一样添加就好了.

3. 若您使用acad是09版本以下的，比如 07 08版本,建议你升级至09 版本以上.

4. 上面的例子告诉了大家如何使用子模块。

#### 七、软件架构及相关说明

1. [软件架构说明](/docs/关于IFoxCAD的架构说明.md)

2. [扩展函数说明](/docs/关于扩展函数的说明.md)

3. [事务管理器用法](/docs/DBTrans.md)

4. [选择集过滤器用法](/docs/SelectionFilter.md)

5. [符号表用法](/docs/SymbolTable.md)

6. [WPF支持](/docs/WPF.md)

7. [自动加载与初始化](/docs/autoreg.md)

8. 天秀的打开模式提权
   
   由于cad的对象是有打开模式，是否可写等等，为了安全起见，在处理对象时，一般是用读模式打开，然后需要写数据的时候在提权为写模式，然后在降级到读模式，但是这个过程中，很容易漏掉某些步骤，然后cad崩溃。为了处理这些情况，内裤提供了提权类来保证读写模式的有序转换。
   
   ```csharp
   // 第一种方式，采用的是事务管理的模式
   using(line.ForWrite()) // 开启对象写模式提权事务
   {
     // 处理代码
   } // 关闭事务自动处理读写模式
   // 第二种方式，采用的是委托的形式
   line.ForWrite(e => {
     // 处理代码
   });
   ```

9. 未完待续。。。。
