# IFoxCAD 大模型技能模板

## 技能概览
IFoxCAD 是一个为 AutoCAD、ZW CAD 等 CAD 平台提供的扩展库，提供丰富的 CAD 开发接口和工具函数。本技能模板为大模型提供完整的 IFoxCAD 功能描述，使其能够理解和使用该库的各种功能。

## 技能分类

### 1. CAD 实体操作技能
#### 1.1 实体创建与管理
- **功能**: 创建、修改、删除 CAD 实体
- **参数**: 
  - `entity_type`: 实体类型（Line, Circle, Polyline 等）
  - `properties`: 实体属性字典
- **返回**: ObjectId 或创建结果
- **示例**:
  ```csharp
  var line = new Line(startPoint, endPoint);
  var id = transaction.AddNewlyCreatedDBObject(line, true);
  ```

#### 1.2 实体属性操作
- **功能**: 获取和修改实体属性
- **参数**:
  - `object_id`: 实体对象ID
  - `property_name`: 属性名
  - `new_value`: 新值
- **示例**:
  ```csharp
  using(var entity = tr.GetObject(objectId, OpenMode.ForWrite))
  {
      entity.ColorIndex = 1;
  }
  ```

#### 1.3 实体批量操作
- **功能**: 批量处理多个实体
- **参数**:
  - `entity_ids`: 实体ID集合
  - `operation`: 操作类型
- **示例**:
  ```csharp
  entities.Erase(); // 批量删除
  ```

### 2. 事务管理技能
#### 2.1 数据库事务操作
- **功能**: 管理 CAD 数据库事务
- **参数**:
  - `database`: 目标数据库
  - `action`: 事务操作委托
- **示例**:
  ```csharp
  using(var tr = new DBTrans(db))
  {
      // 执行数据库操作
      tr.Commit();
  }
  ```

#### 2.2 事务安全操作
- **功能**: 确保事务操作的安全性
- **参数**:
  - `object_id`: 对象ID
  - `open_mode`: 打开模式
- **示例**:
  ```csharp
  var entity = tr.GetObject<Entity>(id, OpenMode.ForRead);
  ```

### 3. 选择集与过滤器技能
#### 3.1 选择集创建
- **功能**: 创建和管理选择集
- **参数**:
  - `filter`: 过滤条件
  - `prompt`: 提示信息
- **示例**:
  ```csharp
  var selection = ed.GetSelection(filter);
  ```

#### 3.2 高级过滤器
- **功能**: 使用复杂条件过滤实体
- **参数**:
  - `conditions`: 过滤条件数组
  - `logic_operator`: 逻辑操作符
- **示例**:
  ```csharp
  var filter = new SelectionFilter(filterElements);
  ```

### 4. 几何计算技能
#### 4.1 点、线、面计算
- **功能**: 几何关系计算
- **参数**:
  - `geometry1`: 几何对象1
  - `geometry2`: 几何对象2
- **示例**:
  ```csharp
  var distance = geom1.GetDistanceTo(geom2);
  ```

#### 4.2 包围盒计算
- **功能**: 计算实体包围盒
- **参数**:
  - `entities`: 实体集合
- **示例**:
  ```csharp
  var extents = entities.GetBoundingBoxEx();
  ```

### 5. 系统变量管理技能
#### 5.1 系统变量读取
- **功能**: 读取 CAD 系统变量
- **参数**:
  - `variable_name`: 变量名
- **示例**:
  ```csharp
  var units = SystemVariableManager.Lunits;
  ```

#### 5.2 系统变量设置
- **功能**: 设置 CAD 系统变量
- **参数**:
  - `variable_name`: 变量名
  - `value`: 设置值
- **示例**:
  ```csharp
  SystemVariableManager.Luprec = 4;
  ```

### 6. 符号表操作技能
#### 6.1 图层管理
- **功能**: 管理 CAD 图层
- **参数**:
  - `layer_name`: 图层名
  - `properties`: 图层属性
- **示例**:
  ```csharp
  var layerId = SymbolTableEx.CreateLayer(db, layerName);
  ```

#### 6.2 块表操作
- **功能**: 管理块定义
- **参数**:
  - `block_name`: 块名
  - `entities`: 块内容
- **示例**:
  ```csharp
  var blockId = SymbolTableEx.CreateBlock(db, blockName, entities);
  ```

### 7. 图案填充技能
#### 7.1 填充创建
- **功能**: 创建图案填充
- **参数**:
  - `boundary`: 边界曲线
  - `pattern`: 填充图案
- **示例**:
  ```csharp
  var hatch = HatchEx.CreateHatch(boundaries, pattern);
  ```

#### 7.2 填充修改
- **功能**: 修改填充属性
- **参数**:
  - `hatch_id`: 填充对象ID
  - `properties`: 属性字典

### 8. Undo/Redo 管理技能
#### 8.1 操作记录
- **功能**: 记录 CAD 操作以支持撤销重做
- **参数**:
  - `action_type`: 操作类型
  - `entity_snapshot`: 实体快照 (DwgFilerEx序列化)
- **示例**:
  ```csharp
  // 使用DwgFilerEx序列化实体状态
  var dwgFiler = new DwgFilerEx(entity);
  var snapshot = dwgFiler.SerializeObject();
  ```

#### 8.2 撤销操作
- **功能**: 执行撤销操作
- **参数**:
  - `action_id`: 操作ID
- **示例**:
  ```csharp
  // 使用DwgFilerEx反序列化并恢复实体
  var dwgFiler = DwgFilerEx.DeserializeObject(snapshot);
  if (dwgFiler != null)
  {
      dwgFiler.DwgIn();
  }
  ```

### 9. 用户界面技能
#### 9.1 Jig 实时预览
- **功能**: 提供实时预览功能
- **参数**:
  - `dynamic_draw_action`: 动态绘制委托
- **示例**:
  ```csharp
  var jig = new JigExTransient();
  ```

#### 9.2 命令交互
- **功能**: 与用户进行命令行交互
- **参数**:
  - `prompt_message`: 提示信息
  - `input_type`: 输入类型

### 10. 文件操作技能
#### 10.1 文件路径处理
- **功能**: 处理 CAD 文件路径
- **参数**:
  - `file_path`: 文件路径
  - `conversion_mode`: 转换模式

#### 10.2 外部参照管理
- **功能**: 管理外部参照文件
- **参数**:
  - `xref_path`: 外参路径
  - `attach_point`: 附着点

## 使用示例

### 基础实体操作
```csharp
// 创建直线
var line = new Line(Point3d.Origin, new Point3d(100, 100, 0));
using(var tr = new DBTrans(db))
{
    tr.AddNewlyCreatedDBObject(line, true);
    tr.Commit();
}
```

### 使用扩展方法
```csharp
// 使用事务扩展方法
using(var tr = new DBTrans(db))
{
    var entity = tr.GetObject<Entity>(objectId, OpenMode.ForRead);
    var bbox = entity.GetBoundingBoxEx();
    tr.Commit();
}
```

### 实体撤销重做
```csharp
// 序列化实体状态
var dwgFiler = new DwgFilerEx(entity);
var snapshot = dwgFiler.SerializeObject();

// 恢复实体状态
var restoredFiler = DwgFilerEx.DeserializeObject(snapshot);
if (restoredFiler != null)
{
    restoredFiler.DwgIn();
}
```

## 错误处理
- 所有操作应在事务中进行
- 使用适当的打开模式避免锁定冲突
- 检查对象有效性后再操作
- 正确释放资源（使用 using 语句）

## 性能优化建议
- 批量操作优于单个操作
- 使用适当的选择过滤器减少遍历
- 及时关闭事务释放资源
- 合理使用缓存机制