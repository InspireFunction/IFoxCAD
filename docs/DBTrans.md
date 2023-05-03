# 事务管理器用法

## 事务管理器介绍

事务管理器是cad .net二次开发过程绕不过去的一个部分，只要是涉及到读写cad数据的地方几乎都推荐在事务里完成。利用事务管理器可以自动的在退出事务的时候执行释放对象等操作，防止程序员不能释放对象，造成cad崩溃。

但是，在日常的使用中，会发现每次开启事务，然后完成的都是差不多的任务，然后每次都要调用commit()函数，每次都要获取到符号表，每次要写模式，读模式等提权降级操作，但是这些操作其实都可以自动完成的，因此 ifoxcad 内裤提供事务管理器类来完成本来需要手工完成的工作，让用户可以更方便的处理事务内的程序。

用事务管理器类可以完成：

- 原生cad提供的事务管理器的全部操作
- 方便的符号表操作
- 方便的基础属性操作
- 方便的对象获取操作
- 方便的字典操作

事务管理器类的类名为：DBTrans。开启默认的事务管理器写法为：

```csharp
using (DBTrans tr = new DBTrans())
{
  ....
}
```

## 原生的事务管理器操作

关于cad提供的原生事务管理器的操作不是本文档的重点，因为那操作起来麻烦，不够集中的将需要在事务内的操作做统一管理。

## 符号表操作

Ifoxcad 类库的符号表其实是一个符号表的泛型类，直接将符号表和符号表记录包装为一个整体。不用担心，在实际使用的过程中，你几乎不会关心符号表的构成原理。

Ifoxcad 类库里采用如下的符号来表示9大符号表。

| 符号表名           | 符号表含义 |
|:--------------:|:-----:|
| BlockTable     | 块表    |
| LayerTable     | 图层表   |
| DimStyleTable  | 标注样式表 |
| LinetypeTable  | 线型表   |
| RegAppTable    | 应用程序表 |
| TextStyleTable | 字体样式表 |
| UcsTable       | 坐标系表  |
| ViewportTable  | 视口表   |
| ViewTable      | 视图表   |

 **然后怎么使用呢？使用符号表一共分几步呢？** 

```csharp
using (DBTrans tr = new DBTrans()) // 第一步，开启事务
{ 
  var layerTable = tr.LayerTable;// 第二步，获取图层表  
} // 事务结束并自动提交
```

上面是一个获取层表的例子，其他的符号表都是一样的写法，因为这些符号表都是事务管理器的属性。那么获取到符号表之后能做些什么？

- **向符号表里添加元素** 
  
  ```csharp
  using (DBTrans tr = new DBTrans()) 
  { // 第一步，开启事务
       var layerTable = tr.LayerTable;
   // 第二步，获取图层表
       layerTable.Add("1");// 返回值为ObjectId
   // 第三步，向层表里添加一个元素，也就是新建一个图层。
  } // 事务结束并自动提交
  ```
  
  每个符号表都有Add函数，而且提供了不止一个重载函数。

- **添加和获取符号表里的元素** 
  
  想要添加和获取符号表内的某个元素非常的简单：
  
  ```csharp
  using (DBTrans tr = new DBTrans()) // 第一步，开启事务
  { 
   var layerTable = tr.LayerTable; // 第二步，获取图层表
   layerTable.Add("1"); // 第三步，添加名为“1”的图层，即新建图层
   ObjectId id = layerTable["1"]; // 第四步，获取图层“1”的id。   
  } // 事务结束并自动提交
  ```
  
  每个符号表都提供了索引形式的获取元素id的写法。

- **线型表** 
  
  ```csharp
  // 两种方式
  // 第一种，直接调用tr.LinetypeTable.Add("hah")函数，然后对返回值ObjectId做具体的操作。
  // 第二种，直接在Action委托里把相关的操作完成。
  tr.LinetypeTable.Add(
                     "hah",
                     ltt => 
                     {
                         ltt.AsciiDescription = "虚线";
                         ltt.PatternLength = 0.95; //线型的总长度
                         ltt.NumDashes = 4; //组成线型的笔画数目
                         ltt.SetDashLengthAt(0, 0.5); //0.5个单位的划线
                         ltt.SetDashLengthAt(1, -0.25); //0.25个单位的空格
                         ltt.SetDashLengthAt(2, 0); // 一个点
                         ltt.SetDashLengthAt(3, -0.25); //0.25个单位的空格
                     });
  // 这段代码同时演示了 ifoxcad 类库关于符号表的public ObjectId Add(string name, Action<TRecord> action)这个函数的用法。
  // 或者直接调用：
  tr.LinetypeTable.Add("hah", "虚线",0.95,new double[]{0.5,-0.25,0,-0.25});
  // 获取线型表
  tr.LinetypeTable["hah"];
  ```
  
  **其他符号表的操作类同。如果类库没有提供的Add函数的重载，那么Action委托可以完成你想完成的所有事情。** 

## 基础属性操作

事务管理器类提供了`Document`、 `Editor` 、`Database`三个属性来在事务内部处理相关事项。

同时还提供了关于字典的相关属性。

## 对象获取操作

提供了1个泛型 `GetObject<T>`函数的重载来根据ObjectId来获取到对象。

## 字典操作(未完待续)

- 扩展字典
  
  `SetXRecord` 保存扩展数据到字典
  
  `GetXRecord ` 获取扩展数据

- 对象字典
  
  `SetToDictionary` 保存数据到字典
  
  `GetFromDictionary` 从字典获取数据
  
  `GetSubDictionary` 获取子对象字典
