---
trigger: 当爸爸需要使用DllImport调用AutoCAD未公开API,处理非托管代码交互,查询acad.exe导出接口或遇到P/Invoke相关问题时
---

# DllImport助手

## 技能描述
DllImport助手是针对AutoCAD二次开发中需要调用未公开API(通过DllImport导入acad.exe等非托管DLL)的专项工具,提供版本管理,接口查询和代码生成规范,确保不同AutoCAD版本的兼容性.

## 功能
- 管理不同AutoCAD版本的DllImport声明
- 查询未公开的API接口列表
- 提供版本兼容的预处理指令规范
- 指导PEInfo工具导出接口

## 查询未公开的API接口

### 接口列表文件位置

**当前项目中的接口文件**:
```
{项目文件夹}/.trae/skills/dllimport助手/acad08的exe接口.txt
```

**不同版本对应文件**:
- `acad08的exe接口.txt` → AutoCAD 2008
- `acad10的exe接口.txt` → AutoCAD 2010
- `acad13的exe接口.txt` → AutoCAD 2013
- `acad18的exe接口.txt` → AutoCAD 2018
- `acad24的exe接口.txt` → AutoCAD 2024

**注**:如果对应版本的接口文件不存在,需要使用PEInfo工具导出

### 使用流程
1. 查看对应版本的接口列表文件
2. 确认需要的API函数是否存在
3. 按照规范编写DllImport声明
4. **必须编写测试命令**,只有测试通过才能写入主工程

## DllImport代码规范

### 版本预处理要求

**强行要求使用预处理的年份名称**,而不是.NET版本号(因为兼容性极差):

```csharp
#if acad08
[DllImport("acad.exe", EntryPoint = "acedCmd", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
#endif
```

**项目中的预处理符号配置**:
在 `.csproj` 文件中定义(已配置好,直接使用即可):
```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <DefineConstants>$(DefineConstants);acad08</DefineConstants>
</PropertyGroup>
```

**常用年份符号**:
- `acad08` - AutoCAD 2008
- `acad10` - AutoCAD 2010
- `acad13` - AutoCAD 2013
- `acad18` - AutoCAD 2018
- `acad24` - AutoCAD 2024

### 完整示例

```csharp
using System.Runtime.InteropServices;

public class UnmanagedAPI
{
#if acad08
    [DllImport("acad.exe", EntryPoint = "acedCmd", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    public static extern int acedCmd(IntPtr args);
#endif
}
```

## 版本不对应时的处理方式

### 使用PEInfo工具导出接口

当需要使用DllImport但发现项目版本不对应时,可以使用 **PeInfo类** 导出未公开的接口.

### PEInfo项目位置
```
acad_IFoxCAD_35/tests/PEInfoTest
```

### 改造和导出步骤

1. **修改项目配置**(如需要)
   - .NET版本
   - 32位/64位架构

2. **运行PEInfo工具**
   - 可以导出 `acad.exe`
   - 可以导出 `acdbmgd.dll`
   - 可以导出 `accoremgd.dll`
   - 等其他exe/dll的接口

3. **获取接口列表**
   - 将导出的接口保存到对应版本的txt文件
   - 更新 `.trae/skills/dllimport助手/` 下的接口文档

## 工作流程

### 当需要使用DllImport时:

1. **确认需求**
   - 确定需要调用的API功能
   - 确认目标AutoCAD版本

2. **查询接口**
   - 查看对应版本的接口列表文件
   - 确认函数签名和参数

3. **编写声明**
   - 使用年份预处理指令(如 `#if acad08`)
   - 正确设置 `EntryPoint`,`CharSet`,`CallingConvention`

4. **编写测试**
   - 创建测试命令验证功能
   - 确保测试通过

5. **集成到主工程**
   - 只有测试通过后才能写入主工程

## 注意事项

- **必须使用年份预处理指令**(acad08,acad10等),而非.NET版本号
- **不同版本的函数签名可能不同**,需要分别测试
- **32位和64位架构**可能有差异,需要确认目标平台
- **优先使用托管API**,只有在没有托管替代方案时才使用DllImport
- **测试是必须的**,未测试的DllImport代码不能进入主工程

## 常见处理手法与知识积累

### 动态知识更新要求

**重要**:在测试和使用DllImport过程中,如果遇到问题并找到解决方案,**必须将解决手法记录到本技能文档中**,供后续任务参考和规避.

### 非托管指针获取

当DllImport函数需要提供非托管指针时,可以使用以下方法:

#### 1. Database对象的UnmanagedObject属性
```csharp
// 获取Database的底层非托管指针
Database db = HostApplicationServices.WorkingDatabase;
IntPtr dbPtr = db.UnmanagedObject;
```

#### 2. ResultBuffer的UnmanagedObject属性
```csharp
// 创建ResultBuffer并获取其非托管指针
ResultBuffer args = new ResultBuffer(
    new TypedValue((int)LispDataType.Text, "command"),
    new TypedValue((int)LispDataType.Text, "option")
);

// 调用DllImport函数
int result = UnmanagedAPI.acedCmd(args.UnmanagedObject);
```

#### 3. 其他常用对象的UnmanagedObject
```csharp
// Entity对象
Entity ent = ...;
IntPtr entPtr = ent.UnmanagedObject;
```

#### 4. 没有UnmanagedObject的
ObjectId

### 常见问题与解决方案

#### 问题1:调用返回错误代码
- **现象**:DllImport函数返回非零错误码
- **解决**:检查参数类型是否匹配,特别是指针类型和字符串编码

#### 问题2:内存访问冲突
- **现象**:调用后出现AccessViolationException
- **解决**:
  - 确保对象在使用期间不被GC回收
  - 使用 `GC.KeepAlive(obj)` 保持对象存活
  - 检查指针是否有效

#### 问题3:字符串传递乱码
- **现象**:中文字符显示为乱码
- **解决**:确保 `CharSet = CharSet.Auto` 或 `CharSet.Unicode`

### 知识积累记录模板

当发现新的处理手法时,按以下格式添加到本节:

```markdown
#### 问题X:问题描述
- **现象**:具体表现
- **解决**:解决方案代码示例
- **记录时间**:YYYY-MM-DD
```

## 相关文件

- 接口列表:`/.trae/skills/dllimport助手/acad{年份}的exe接口.txt`
- PEInfo工具:`/tests/PEInfoTest/`
- 参考技能:`/.trae/skills/cad开发助手/SKILL.md`

