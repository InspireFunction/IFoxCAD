# 选择集过滤器用法

## 选择集过滤器简介

桌子提供了选择集过滤器是为了更精确的选择对象。可以通过使用选择过滤器来限制哪些对象被选中并添加到选择集，选择过滤器列表通过属性或类型过滤所选对象。

在桌子的 .net api 中：选择过滤器由一对 TypedValue 参数构成。TypedValue 的第一个参数表明过滤器的类型（例如对象），第二个参数为要过滤的值（例如圆）。过滤器类型是一个 DXF 组码，用来指定使用哪种过滤器。

默认的使用桌子api来创建选择集（带过滤器）分三步：

1. 创建一个TypedValue数组来定义过滤器条件
   
   ```csharp
   TypedValue[] acTypValAr = new TypedValue[1]; // 创建数组
   acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "CIRCLE"), 0); 
   // 添加一个过滤条件，例如选择圆
   
   // 如果要创建多个过滤条件怎么办？
   TypedValue[] acTypValAr = new TypedValue[3];
   acTypValAr.SetValue(new TypedValue((int)DxfCode.Color, 5), 0);
   acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "CIRCLE"), 1);
   acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, "0"), 2);
   // 实际上只要不停的往数组里添加条件就可以了
   ```

2. 创建SelectionFilter对象
   
   ```csharp
   // 将过滤器条件赋值给 SelectionFilter 对象
   SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);
   ```

3. 创建选择集
   
   ```csharp
   // 请求用户在图形区域选择对象
   PromptSelectionResult acSSPrompt;
   acSSPrompt = acDocEd.GetSelection(acSelFtr);
   ```

看起来很是简单对不对，单个条件和多个条件的过滤非常简单。当指定多个选择条件时，AutoCAD 假设所选对象必须满足每个条件。我们还可以用另外一种方式定义过滤条件。对于数值项，可以使用关系运算（比如，圆的半径必须大于等于 5.0）。对于所有项，可以使用逻辑运算（比如单行文字或多行文字）。使用 DXF 组码-4 或常量 DxfCode.Operator 表示选择过滤器中的关系预算符类型。运算符本身用字符串表示。

比如：

1. 过滤半径大于等于5.0的圆
   
   ```csharp
   TypedValue[] acTypValAr = {
         new TypedValue((int)DxfCode.Start, "CIRCLE"),
         new TypedValue((int)DxfCode.Operator, ">="), 
         new TypedValue(40, 5)
   };
   ```

2. 过滤单行文本或者多行文本
   
   ```csharp
   TypedValue[] acTypValAr = {
         new TypedValue((int)DxfCode.Operator, "<or"),
         new TypedValue((int)DxfCode.Start, "TEXT"),
         new TypedValue((int)DxfCode.Start, "MTEXT"),
         new TypedValue((int)DxfCode.Operator, "or>")
   };
   ```

3. 更复杂的过滤条件呢？比如选择的对象为不是位于0图层的直线，或者位于2图层的组码10的x坐标>10,y坐标>10的非圆图元。
   
   对应的lisp代码如下：
   
   ```lisp
   '((-4 . "<or")
         (-4 . "<not")
             (-4 . "<and")
                 (0 . "line")
           (8 . "0")
         (-4 . "and>")
         (-4 . "not>")
         (-4 . "<and")
                 (-4 . "<not")(0 . "circle")(-4 . "not>")
           (8 . "2")
                 (-4 . ">,>,*")(10 10 10 0)
       (-4 . "and>")
   (-4 . "or>"))
   ```
   
   对应的c#代码：
   
   ```csharp
   TypedValue[] acTypValAr = {
         new TypedValue((int)DxfCode.Operator, "<or"),
                 new TypedValue((int)DxfCode.Operator, "<not"),
                     new TypedValue((int)DxfCode.Operator, "<and"),
                         new TypedValue((int)DxfCode.Start, "LINE"),
                         new TypedValue((int)DxfCode.LayerName, "0"),
                     new TypedValue((int)DxfCode.Operator, "and>"),
                 new TypedValue((int)DxfCode.Operator, "not>"),
                 new TypedValue((int)DxfCode.Operator, "<and"),
                     new TypedValue((int)DxfCode.Operator, "<not"),
                         new TypedValue((int)DxfCode.Start, "CIRCLE"),
                     new TypedValue((int)DxfCode.Operator, "not>"),
                     new TypedValue((int)DxfCode.LayerName, "2"),
                     new TypedValue((int)DxfCode.Operator, ">,>,*"),
                     new TypedValue(10, new Point3d(10,10,0)),
                 new TypedValue((int)DxfCode.Operator, "and>"),
         new TypedValue((int)DxfCode.Operator, "or>")
   };
   ```
   
   这个过滤器是不是看起来很乱，一眼看去根本不知道是要过滤什么，写起来也很麻烦。所以说，虽然桌子提供了api，但是简单的过滤条件很好用，但是复杂的过滤条件就很复杂了。
   
   因此IFox内裤提供了关于选择集过滤器的辅助类来帮助用户用更简单的方式来创建选择集的过滤器。

## 内裤过滤器对象与cad过滤器对应关系

IFoxCad内裤对于DxfCode.Operator枚举构建了一些辅助函数来表达关系运算和逻辑运算；提供了dxf函数来表达组码。其对应的关系如下表：

| 内裤过滤器对象、函数 | cad .net api 过滤器对象、函数、枚举 | 备注                  |
|:----------:|:------------------------:|:-------------------:|
| OpFilter   | SelectionFilter          | 隐式转换                |
| OpOr       | "<OR" ... "OR>"          |                     |
| Op.Or      | "<OR" ... "OR>"          |                     |
| OpAnd      | "<AND"..."AND>"          |                     |
| Op.And     | "<AND"..."AND>"          |                     |
| OpNot      | "<NOT" ... "NOT>"        |                     |
| OpXor      | "<XOR" ... "XOR>"        |                     |
| OpEqual    | 相等运算                     |                     |
| OpComp     | 比较运算符                    |                     |
| Dxf()      | 组码函数                     | 仅用于过滤器中，不是组码操作函数    |
| !          | "<NOT" ... "NOT>"        |                     |
| ==         | "="                      |                     |
| !=         | "!="                     |                     |
| >          | ">"                      |                     |
| <          | "<"                      |                     |
| >=         | ">=" 或 ">,>,*"           | ">,>,*"用于跟point3d比较 |
| <=         | "<=" 或 "<,<,*"           | "<,<,*"用于跟point3d比较 |
| &          | "<AND"..."AND>"          |                     |
| ^          | "<XOR" ... "XOR>"        |                     |
| \|         | "<OR" ... "OR>"          |                     |

## 具体用法

IFoxCad内裤提供了三种方式来构建过滤器，其实大同小异，就是写法不一样，用户可以根据自己的喜好来选择。

- 第一种
  
  ```csharp
  var fd =
      new OpOr    //定义一个 (-4 . "<or")(...)(-4 . "or>")
      {
          !new OpAnd //定义(-4 . "<not")(-4 . "<and")(...)(-4 . "and>")(-4 . "not>")
          {
              { 0, "line" }, //{组码，组码值}
              { 8, "0" }, //{组码，组码值}
          },
          new OpAnd //定义(-4 . "<and")(...)(-4 . "and>")
          {
              !new OpEqual(0, "circle"), //定义(-4 . "<not")(...)(-4 . "not>")
              { 8, "2" }, //{组码，组码值}
              { 10, new Point3d(10,10,0), ">,>,*" }  //(-4 . ">,>,*")(10 10 10 0)
          },
      };
  editor.SelectAll(fd); //这里直接传入fd就可以了
  ```
  
  以上代码的含义为：选择的对象为不是位于0图层的直线，或者位于2图层的组码10的x坐标>10,y坐标>10的非圆图元。其同含义的lisp代码如下：
  
  ```lisp
  '((-4 . "<or")
        (-4 . "<not")
            (-4 . "<and")
                (0 . "line")
          (8 . "0")
        (-4 . "and>")
        (-4 . "not>")
        (-4 . "<and")
                (-4 . "<not")(0 . "circle")(-4 . "not>")
          (8 . "2")
                (-4 . ">,>,*")(10 10 10 0)
      (-4 . "and>")
    (-4 . "or>"))
  ```

- 第二种
  
  ```csharp
  var p = new Point3d(10, 10, 0);
  var f = OpFilter.Bulid(e => 
              !(e.Dxf(0) == "line" & e.Dxf(8) == "0") 
              | e.Dxf(0) != "circle" 
              & e.Dxf(8) == "2" 
              & e.Dxf(10) >= p);
  editor.SelectAll(f); //这里直接传入f就可以了
  ```
  
  代码含义如第一种。

- 第三种
  
  ```csharp
  var f2 = OpFilter.Bulid(
    e =>e.Or(
      !e.And(e.Dxf(0) == "line", e.Dxf(8) == "0"),
      e.And(e.Dxf(0) != "circle", e.Dxf(8) == "2", e.Dxf(10) >= new Point3d(10, 10, 0)))
  );
  editor.SelectAll(f2); //这里直接传入f2就可以了
  ```
  
  代码含义如第一种，第三种和第二种的写法非常像，区别就是关于 and 、or 、not 等运算符，一个是采用c#的语法，一个是采用定义的函数。and 与&等价，or与|等价，not 与！等价。