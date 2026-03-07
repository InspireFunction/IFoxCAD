# IFoxFunKit.TestNet35Analyzers

## 项目说明

本项目是为了验证 **.NET Framework 3.5** 是否能够使用 Roslyn 语法分析器（Analyzer）而创建的测试项目。

### 核心目的

1. **验证兼容性**：证明即使在老旧的 .NET 3.5 框架下，依然可以使用现代的 Roslyn 分析器进行代码检查
2. **强制处理返回值**：配合 `IFoxFunKit.Analyzers` 分析器，强制开发者处理 `Option<T>` 和 `Result<T>` 类型的返回值

### 项目配置要点

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net35</TargetFramework>
        <LangVersion>preview</LangVersion>
        <EnableNETAnalyzers>true</EnableNETAnalyzers>
        <AnalysisLevel>latest</AnalysisLevel>
    </PropertyGroup>

    <ItemGroup>
        <Analyzer Include="..\IFoxFunKit.Analyzers\bin\Debug\netstandard2.0\IFoxFunKit.Analyzers.dll" />
    </ItemGroup>
</Project>
```

- `TargetFramework` 设置为 `net35`
- `LangVersion` 设置为 `preview` 以支持较新的 C# 语法
- `EnableNETAnalyzers` 启用 Roslyn 分析器
- 通过 `Analyzer` Item 引入分析器 DLL

### 工作原理

分析器会在编译时检查代码，如果发现 `Option<T>` 或 `Result<T>` 的返回值未被正确处理，将产生编译错误 **IFOX001**。

---

## 可触发错误的示例代码

### 错误示例 1：忽略返回值

```csharp
// 错误：直接调用返回 Option/Result 的方法而不处理结果
Divide(10, 0);  // IFOX001: 返回值类型 'Option<int>' 必须被处理

static Option<int> Divide(int a, int b)
{
    if (b == 0)
        return Option<int>.None;
    return Option<int>.Some(a / b);
}
```

### 错误示例 2：声明变量后未处理

```csharp
// 错误：声明了变量但从未处理
var result = Divide(10, 2);  // IFOX001: 返回值类型 'Option<int>' 必须被处理
Console.WriteLine("完成");

static Option<int> Divide(int a, int b)
{
    if (b == 0)
        return Option<int>.None;
    return Option<int>.Some(a / b);
}
```

### 错误示例 3：赋值后未处理

```csharp
Option<int> result;
result = Divide(10, 2);  // IFOX001: 返回值类型 'Option<int>' 必须被处理

static Option<int> Divide(int a, int b)
{
    if (b == 0)
        return Option<int>.None;
    return Option<int>.Some(a / b);
}
```

### 错误示例 4：Result 类型未处理

```csharp
// 错误：Result 返回值被忽略
SafeDivide(10, 0);  // IFOX001: 返回值类型 'Result<int>' 必须被处理

static Result<int> SafeDivide(int a, int b)
{
    if (b == 0)
        return Result<int>.Err(new Exception("除数不能为零"));
    return Result<int>.Ok(a / b);
}
```

---

## 正确的处理方式

### 方式 1：使用 if 语句检查属性

```csharp
var result = Divide(10, 2);
if (result.IsSome)
{
    Console.WriteLine("结果: " + result.Value);
}
else
{
    Console.WriteLine("无结果");
}
```

### 方式 2：使用 Match 方法

```csharp
var result = Divide(10, 2);
result.Match(
    some => Console.WriteLine("结果: " + some),
    () => Console.WriteLine("无结果")
);
```

### 方式 3：使用 IsNone/IsErr 检查

```csharp
var result = SafeDivide(10, 0);
if (result.IsErr)
{
    Console.WriteLine("错误: " + result.ErrValue.Message);
}
else
{
    Console.WriteLine("结果: " + result.OkValue);
}
```

### 方式 4：使用 UnwrapOr 提供默认值

```csharp
var result = Divide(10, 2);
var value = result.UnwrapOr(0);
Console.WriteLine("结果: " + value);
```

### 方式 5：作为参数传递

```csharp
void ProcessResult(Option<int> option)
{
    if (option.IsSome)
        Console.WriteLine("处理: " + option.Value);
}

ProcessResult(Divide(10, 2));  // 正确：返回值被传递给其他方法
```

---

## 分析器检测的有效成员访问

以下成员访问会被视为"已处理"：

| Option<T> | Result<T> |
|-----------|-----------|
| `IsSome` | `IsOk` |
| `IsNone` | `IsErr` |
| `Value` | `OkValue` |
| `UnwrapOr` | `ErrValue` |
| `UnwrapOrElse` | `UnwrapOr` |
| `Expect` | `UnwrapOrElse` |
| `ExpectErr` | `Expect` |
| `Map` | `ExpectErr` |
| `Bind` | `Map` |
| `Filter` | `MapErr` |
| `Match` | `Bind` |
| `OkOr` | `Match` |
| `OkOrElse` | `OkToOption` |
| `TryGetValue` | `ErrToOption` |
| | `TryGetOk` |
| | `TryGetErr` |

---

## 编译与测试

```bash
# 先编译分析器项目
dotnet build ../IFoxFunKit.Analyzers

# 再编译测试项目
dotnet build
```

如果代码中存在未处理的 `Option` 或 `Result` 返回值，编译将会失败并显示错误信息。
