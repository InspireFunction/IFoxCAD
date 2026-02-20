---
name: cad开发助手
description: 针对AutoCAD二次开发场景的专项助手，提供API函数表查询、代码生成规范、编译导出XML函数表等工作流程支持。
---

# CAD开发助手

## 技能描述
CAD开发助手是针对AutoCAD二次开发（基于IFoxCAD框架）的专项工具，提供API函数查询、代码生成指导、函数表管理等功能，帮助开发者高效编写符合规范的CAD插件代码。

## 功能
- 查询和利用现有项目内的API函数表管理
- 指导CAD代码编写规范和最佳实践
- 管理编译时导出的XML函数表文档
- 提供CAD特定功能的代码生成建议

## 函数表管理

### 函数表文件位置
这是上一次编译导出的函数名表，包含了项目中可用的API函数信息。
```
{项目文件夹}/.trae/skills/cad开发助手/IFoxCAD.Acad08.xml
```

或者在工程的输入文件夹中查找,例如:

```
{项目文件夹}/tests/TestAcad08/bin/Debug/net35/IFoxCAD.Acad08.xml
```

```
{项目文件夹}/tests/TestAcad08/Release/Debug/net35/IFoxCAD.Acad08.xml
```

注:不同版本/年份

### 使用函数表
当编写CAD代码任务时：
1. 列出任务中可能需要的函数名
2. 查看 [函数表文件位置](#函数表文件位置) 
3. 检查是否可以复用里面已有的API

### 编写代码注意事项
- 可以先不用判断函数名是否已经删除，因为编译时会提示
- 优先使用函数表中已有的API，保持代码一致性

## 未公开API的调用

如果需要使用DllImport导入未公开的AutoCAD API，请参考专项技能：

**[dllimport助手](../dllimport助手/SKILL.md)**

包含内容：
- 查询未公开的API接口列表
- DllImport版本管理和预处理指令规范
- PEInfo工具导出接口的方法

## 编译导出函数表

### 导出XML函数表(AI助手可以直接执行)
编译C#工程时需要导出函数表XML，并放入到 `.trae` 文件夹，这样可以动态更新函数表。

**编译命令示例：**

```bash
csc /doc:.trae/api-documentation.xml YourCode.cs
```

### 导出配置(AI助手不执行,是提示用户,不要直接修改用户文件)
在 `.csproj` 文件或者`Directory.Build.props`

启用XML文档导出：

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

采用新的C#语法,并且简化了代码:

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

### 当接收到CAD代码编写任务时：

1. **分析需求**
   - 理解需要实现的CAD功能
   - 确定涉及的AutoCAD对象（Entity、BlockTable、Layer等）

2. **查询函数表**
   - 读取 [函数表文件位置](#函数表文件位置) 
   - 查找可复用的相关API

3. **生成代码**
   - 按照CAD规范编写代码
   - 添加详细的XML文档注释
   - 使用UTF8BOM编码保存

4. **编译验证**
   - 编译并导出新的函数表
   - 如有编译错误，参考 `skills/编译问题助手`

## 适用场景

- 编写AutoCAD命令
- 操作CAD图形对象（Entity、Block、Layer等）
- 处理CAD文档和数据库
- 创建CAD用户界面（Palette、Dialog等）

## 注意事项

- 确保引用了正确的AutoCAD API程序集
- 注意AutoCAD版本兼容性
- 事务处理必须正确提交或回滚
- 长时间操作应考虑使用进度指示器
