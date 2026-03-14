---
name: cad开发技能
description: 针对AutoCAD二次开发场景的专项技能,提供API函数表查询,代码生成规范,编译导出XML函数表等工作流程支持.
---

# CAD开发技能

## 技能描述
CAD开发技能是针对AutoCAD二次开发(基于IFoxCAD框架)的专项工具,提供API函数查询,代码生成指导,函数表管理等功能,帮助开发者高效编写符合规范的CAD插件代码.

## 功能
- 查询和利用现有项目内的API函数表管理
- 指导CAD代码编写规范和最佳实践
- 管理编译时导出的XML函数表文档
- 提供CAD特定功能的代码生成建议

## 函数表管理

### 函数表文件位置
这是上一次编译导出的函数名表,包含了项目中可用的API函数信息.
```
#.trae\skills\cad开发技能\IFoxCAD.Acad08.xml
```

或者在工程的输入文件夹中查找,例如:

```
{项目文件夹}\tests\TestAcad08\bin\Debug\net35\IFoxCAD.Acad08.xml
```

```
{项目文件夹}\tests\TestAcad08\Release\Debug\net35\IFoxCAD.Acad08.xml
```

注:不同版本/年份

### 使用函数表
当编写CAD代码任务时:
1. 列出任务中可能需要的函数名
2. 查看 [函数表文件位置](#函数表文件位置) 
3. 检查是否可以复用里面已有的API

### 编写代码注意事项
- 可以先不用判断函数名是否已经删除,因为编译时会提示
- 优先使用函数表中已有的API,保持代码一致性

## 未公开API的调用

如果需要使用DllImport导入未公开的AutoCAD API,请参考专项技能:

**[dllimport技能](..\dllimport技能\SKILL.md)**

包含内容:
- 查询未公开的API接口列表
- DllImport版本管理和预处理指令规范
- PEInfo工具导出接口的方法

## 编译导出函数表

### 导出XML函数表(小小可以直接执行)
编译C#工程时需要导出函数表XML,并放入到 `.trae` 文件夹,这样可以动态更新函数表.

**编译命令示例:**

```bash
csc /doc:.trae\api-documentation.xml YourCode.cs
```

### 导出配置(小小不执行,是提示爸爸,不要直接修改爸爸的文件)
在 `.csproj` 文件或者`Directory.Build.props`

启用XML文档导出:

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

## CAD代码编写规范

### 1. 命令定义
```csharp
/// <summary>
/// 命令名称
/// </summary>
[CommandMethod(nameof(CommandName))]
public void CommandName()
{
    // 实现代码
}
```

### 2. 事务处理
采用IFoxCad事务栈,新的C#语法,并且简化了代码:
```csharp
 using var tr = DBTrans.Create();
```

### 3. 错误处理
```csharp
try
{
    // CAD操作
}
catch (Autodesk.AutoCAD.Runtime.Exception ex)
{
    // 处理CAD特定异常
}
```

## 工作流程

### 当接收到CAD代码编写任务时:

1. **分析需求**
   - 理解需要实现的CAD功能
   - 确定涉及的AutoCAD对象(Entity,BlockTable,Layer等)

2. **查询函数表**
   - 读取 [函数表文件位置](#函数表文件位置) 
   - 查找可复用的相关API

3. **生成代码**
   - 按照CAD规范编写代码
   - 添加详细的XML文档注释

4. **编译验证**
   - 编译并导出新的函数表
   - 如有编译错误,参考 `skills\编译问题技能`

## 适用场景

- 编写AutoCAD命令
- 操作CAD图形对象(Entity,Block,Layer等)
- 处理CAD文档和数据库
- 创建CAD用户界面(Palette,Dialog等)

## 性能优化指南

CAD开发中的性能优化需要特别注意AutoCAD API的单线程特性与操作系统API的多线程能力之间的区别.

### 并行处理规则

#### ❌ 禁止并行的情况(AutoCAD API)

AutoCAD API **不是线程安全的**,以下操作**严禁使用并行处理**:

1. **图形对象操作**
   ```csharp
   // ❌ 错误:不能在Parallel.ForEach中操作AutoCAD实体
   Parallel.ForEach(entities, entity =>
   {
       entity.Color = Color.Red; // 危险!可能导致崩溃
   });

   // ✅ 正确:使用单线程循环
   foreach (var entity in entities)
   {
       entity.Color = Color.Red;
   }
   ```

2. **数据库操作**
   ```csharp
   // ❌ 错误:不能并行访问数据库
   Parallel.ForEach(ids, id =>
   {
       using var tr = db.TransactionManager.StartTransaction();
       // ... 危险操作
   });

   // ✅ 正确:单线程处理
   using var tr = db.TransactionManager.StartTransaction();
   foreach (var id in ids)
   {
       // ... 安全操作
   }
   ```

3. **文档操作**
   - 禁止并行访问 `Application.DocumentManager`
   - 禁止并行操作多个文档的图形数据

#### ✅ 可以使用并行的情况(纯数据/操作系统API)

以下场景**可以安全使用并行处理**:

1. **纯数据计算**
   ```csharp
   // ✅ 正确:纯数学计算可以并行
   var results = points.AsParallel()
       .Select(p => CalculateDistance(p, center))
       .ToList();
   ```

2. **文件I/O操作**
   ```csharp
   // ✅ 正确:操作系统文件操作可以并行
   Parallel.ForEach(filePaths, path =>
   {
       var data = File.ReadAllBytes(path);
       ProcessData(data);
   });
   ```

3. **网络请求**
   ```csharp
   // ✅ 正确:网络操作可以并行
   var tasks = urls.Select(url => httpClient.GetStringAsync(url));
   var results = await Task.WhenAll(tasks);
   ```

4. **非CAD相关的数据处理**
   ```csharp
   // ✅ 正确:LINQ数据处理可以并行
   var filtered = largeDataSet.AsParallel()
       .Where(x => x.Value > threshold)
       .OrderBy(x => x.Name)
       .ToList();
   ```

### 性能优化策略

#### 1. 批量操作优于单个操作

```csharp
// ❌ 低效:逐个添加实体
foreach (var data in dataList)
{
    var line = new Line(data.Start, data.End);
    db.AddToModelSpace(line); // 每次都要修改数据库
}

// ✅ 高效:批量添加
var lines = dataList.Select(data => new Line(data.Start, data.End)).ToList();
db.AddToModelSpace(lines); // 一次性修改数据库
```

#### 2. 使用选择集优化过滤

```csharp
// ❌ 低效:遍历所有实体再过滤
var allEntities = modelSpace.Cast<ObjectId>()
    .Select(id => tr.GetObject(id, OpenMode.ForRead))
    .OfType<Line>()
    .Where(line => line.Length > 100)
    .ToList();

// ✅ 高效:使用选择集预过滤
var filter = new SelectionFilter(new[]
{
    new TypedValue((int)DxfCode.Start, "LINE")
});
var result = editor.SelectAll(filter);
// 再对结果进行进一步过滤
```

#### 3. 避免重复的属性访问

```csharp
// ❌ 低效:重复访问属性
for (int i = 0; i < polyline.NumberOfVertices; i++)
{
    var point = polyline.GetPoint2dAt(i);
    // 每次循环都访问 NumberOfVertices 属性
}

// ✅ 高效:缓存属性值
int vertexCount = polyline.NumberOfVertices;
for (int i = 0; i < vertexCount; i++)
{
    var point = polyline.GetPoint2dAt(i);
}
```

#### 4.延迟加载与缓存
cad的API,难以使用长效缓存,毕竟编写命令作用域方便理解,例如容器是块表/图层表/字典表...
只有系统API的部分,才可以通过时间释放变量,见项目文件 `MemoryCache.cs`

### 内存管理

#### 1. 及时释放COM对象

```csharp
// ✅ 使用using语句确保资源释放
{
    // 采用IFoxCad事务栈,新的C#语法,并且简化了代码:
    using var tr = DBTrans.Create();

} // 自动释放
```

```
// ✅ 对于COM对象,使用try-finally
ComObject comObj = null;
try
{
    comObj = GetComObject();
    // ... 操作
}
finally
{
    if (comObj != null)
    {
        Marshal.ReleaseComObject(comObj);
        Debug.WriteLine("[ProcessData] COM对象已释放");
    }
}
```

#### 2. 大对象处理

```csharp
// ✅ 分批处理大量数据
public void ProcessLargeDataset(IEnumerable<EntityData> dataList)
{
    Debug.WriteLine("[ProcessLargeDataset] 开始处理大数据集");
    const int batchSize = 1000;
    var batches = dataList.Chunk(batchSize);

    int batchIndex = 0;
    foreach (var batch in batches)
    {
        Debug.WriteLine($"[ProcessLargeDataset] 处理第 {++batchIndex} 批,数量: {batch.Count()}");
        ProcessBatch(batch);
        GC.Collect(); // 每批处理后建议垃圾回收
    }
}
```

### 调试与监控

#### 性能计时

```csharp
// ✅ 使用Stopwatch进行性能监控
public void OptimizedOperation()
{
    var sw = Stopwatch.StartNew();
    Debug.WriteLine("[OptimizedOperation] 开始执行");

    // ... 操作

    sw.Stop();
    Debug.WriteLine($"[OptimizedOperation] 执行完成,耗时: {sw.ElapsedMilliseconds}ms");
}
```

#### 内存监控

```csharp
// ✅ 监控内存使用
public void MemoryIntensiveOperation()
{
    long beforeMemory = GC.GetTotalMemory(false);
    Debug.WriteLine($"[MemoryIntensiveOperation] 操作前内存: {beforeMemory / 1024 / 1024}MB");

    // ... 操作

    long afterMemory = GC.GetTotalMemory(true);
    Debug.WriteLine($"[MemoryIntensiveOperation] 操作后内存: {afterMemory / 1024 / 1024}MB, " +
        $"增加: {(afterMemory - beforeMemory) / 1024 / 1024}MB");
}
```

### 常见性能陷阱

1. **在循环中频繁打开/关闭事务**
2. **重复查询数据库获取相同数据**
3. **在UI线程执行耗时操作(应使用BackgroundWorker或异步)**
4. **不及时释放COM对象导致内存泄漏**
5. **对AutoCAD API使用并行处理**
6. **频繁刷新图形显示(应使用Document.LockDocument)**

### 最佳实践总结

- ✅ 批量操作,减少数据库访问次数
- ✅ 使用选择集预过滤,减少遍历数据量
- ✅ 缓存常用数据,避免重复查询
- ✅ 及时释放资源,避免内存泄漏
- ❌ 永远不要对AutoCAD API使用并行处理
- ❌ 避免在循环中频繁创建事务
- ❌ 不要频繁刷新图形显示

## 注意事项

- 确保引用了正确的AutoCAD API程序集
- 注意AutoCAD版本兼容性
- 事务处理必须正确提交或回滚
- 长时间操作应考虑使用进度指示器

