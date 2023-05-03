# 符号表用法

每个图形文件都包含有9个固定的符号表。不能往数据库里添加新的符号表。如图层表（LayerTable），其中包含图层表记录，还有块表（BlockTable），其中包含块表记录等。所有的图形实体（线、圆、弧等等）都属于一个块表记录。缺省情况下，任何图形文件都包含为模型空间和图纸空间预定义的块表记录。每个符号表都有对应的符号表记录，可以理解为符号表是一个集合，而符号表记录是这个集合的元素。CAD的符号表和符号表记录的对应关系如下：

| 名称      | 符号表            | 符号表记录                |
|:-------:|:--------------:|:--------------------:|
| 块表      | BlockTable     | BlockTableRecord     |
| 标注样式表   | DimStyleTable  | DimStyleTableRecord  |
| 图层表     | LayerTable     | LayerTableRecord     |
| 线型表     | LinetypeTable  | LinetypeTableRecord  |
| 注册应用程序表 | RegAppTable    | RegAppTableRecord    |
| 字体样式表   | TextStyleTable | TextStyleTableRecord |
| 坐标系表    | UcsTable       | UcsTableRecord       |
| 视口表     | ViewportTable  | ViewportTableRecord  |
| 视图表     | ViewTable      | ViewTableRecord      |

那么如何来操作这些符号表呢？下面是一个新建图层的例子：

```csharp
Document acDoc = Application.DocumentManager.MdiActiveDocument;
Database acCurDb = acDoc.Database;
using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
{
  // 返回当前数据库的图层表
  LayerTable acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,OpenMode.ForRead) as LayerTable;
  // 检查图层表里是否有图层 MyLayer
  if (acLyrTbl.Has("MyLayer") != true)
  {
    // 以写模式打开图层表
    acLyrTbl.UpgradeOpen();
    // 新创建一个图层表记录，并命名为”MyLayer”
    LayerTableRecord acLyrTblRec = new LayerTableRecord();
    acLyrTblRec.Name = "MyLayer";
    // 添加新的图层表记录到图层表，添加事务
    acLyrTbl.Add(acLyrTblRec);
    acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
    //提交修改
    acTrans.Commit();
  }
  // 关闭事务，回收内存；
} 
```

上面的例子用了20多行的代码来完成一个很简单的功能，这就是AutoCAD提供的api太过于基础，没有进行进一步的封装造成。那么如果我们单独为图层表封装一个函数来处理图层表，其他的8个符号表也要同样的各自封装函数，这样看起来没什么问题，但是对于代码的复用却没有很好的考虑进去。仔细思考一下，其实对于符号来说无非就是增删改三个主要的操作等，对于符号表记录来说无非就是一些属性的操作，增加实体的操作等。那么有没有一种办法可以统一管理9个符号表呢？其实AutoCAD提供了9个符号表和符号表记录的抽象基类，SymbolTable和SymbolTableRecord，但是这两个类提供的功能又很简单，只有寥寥几个函数和属性，完全不能满足我们的需求。因此ifoxcad内裤提供了符号表类来封装9个符号表的大部分功能。那么用内裤来完成上述的操作是什么样子的呢？见下面的例子：

```csharp
// 以下代码采用最新的c#版本语法
using var tr = new DBTrans(); // 打开事务
var layertable = tr.LayerTable.Add("MyLayer"); //添加图层
```

同样的功能我们只需要两行就可以搞定了。那么有同学会问了，我同样单独对每个符号表的封装一样可以达到这个效果？是的，确实可以达到一样的效果，但是我只封装了一次，只是针对符号表的差异部分做了一些增量的处理，其他的代码都是复用的，而你要写9次。

言归正传，通过上述的例子，我们会发现几个现象：

1. 符号表的操作是在事务内。
2. 符号表成了事务的属性
3. 添加符号表记录到符号表调用Add函数就可以了(其实提供了好多的重载，来完成不同的细粒度的操作)。

符号表的操作都在事务内，这样由事务统一管理符号表的变动，减少出错的可能。

符号表作为事务的属性，那么获取符号表记录就变成了属性的索引值。`var layertable = tr.LayerTable["MyLayer"];`

不管是什么符号表，都是一个Add函数搞定添加操作。

而删除就是：`tr.LayerTable.Remove("1");`  *注意，这里的关于删除图层的操作需要调用Delete函数*

看，我教会了你操作图层表，那么其他的8个表你都会了，都是一样的操作。

## 块表添加图元

一般的情况下，添加图元的操作都是要在事务里完成。目前大部分的添加图元的自定义函数都是DataBase或Editor对象的扩展函数。但是实际上添加图元的本质是读写图形数据库，具体的手段是对块表里的块表记录的读写。而实际的操作其实都是在事务里完成，所以符合cad操作规则的写法其实应该是事务作为一系列操作的主体来进行。因此ifoxcad内裤的封装思路为：扩展块表记录的函数，在事务管理器类里通过属性调用AddEntity函数来添加图元。

对于这个添加图元的操作，一共分为如下几步：

1. 创建图元对象，可以在事务外创建，也可以在事务内创建。
2. 打开要添加图元的块表记录，在事务内打开。
3. 添加图元到块表记录

下面看示例：

- 添加图元到当前空间
  
  ```csharp
  // 以下代码采用最新的c#版本语法
  using var tr = new DBTrans(); //开启事务管理器
  var line1 = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0)); //定义一个直线
  tr.CurrentSpace.AddEntity(line1); // 将直线添加到当前绘图空间的块表记录
  ```

- 添加图元到模型/图纸空间
  
  ```csharp
  // 以下代码采用最新的c#版本语法
  using var tr = new DBTrans(); //开启事务管理器
  var line1 = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0)); //定义一个直线
  tr.CurrentSpace.AddEntity(line1); // 将直线添加到当前绘图空间的块表记录
  tr.ModelSpace.AddEntity(line1); // 将直线添加到当前模型空间的块表记录
  tr.PaperSpace.AddEntity(line1); // 将直线添加到当前图纸空间的块表记录
  ```

- 添加图元到块表
  
  ```csharp
  // 以下代码采用最新的c#版本语法
  using var tr = new DBTrans(); //开启事务管理器
  var line1 = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0)); //定义一个直线
  var btr = tr.BlockTable.Add("test"); //定义一个块表记录
  btr.AddEntity(line1); // 将直线添加到当前控件的块表记录
  ```
  
  那么大家猜一猜，这个添加到块表是实现了一种什么样的功能。

- 块表
  
  块表这里需要特殊的说明一下：
  
  比如说添加一个块，用如下代码：
  
  `tr.BlockTable.Add(blockName, btr => btr.AddEntity(ents));`
  
  这里的blockName就是块名，ents就是图元列表。这种方式虽然可以更细粒度的控制定义的块。
  
  插入块参照，比如：
  
  `tr.InsertBlock(point,objectid); // 用于插入块参照，提供了重载函数来满足不同的需求`

- 其他函数的介绍
  
  `tr.BlockTable.GetRecord()` 函数，可以获取到块表的块表记录，同理层表等符号表也有同样的函数。
  
  `tr.BlockTable.GetRecordFrom()` 函数，可以从文件拷贝块表记录，同理层表等符号表也有同样的函数。
  
  `tr.BlockTable.GetBlockFrom()` 函数，从文件拷贝块定义，同理层表等符号表也有同样用途的函数。

- 添加图元函数
  
  内裤提供了一些便利的添加图元函数，可以不用先定义一个entity对象，然后添加到块表记录。
  
  ```csharp
  using var tr = new DBTrans();
  tr.CurrentSpace.AddLine(new Point3d(0,0,0),new Point3d(1,1,0));
  tr.CurrentSpace.AddCircle(new Point3d(0,0,0),10);
  ```


