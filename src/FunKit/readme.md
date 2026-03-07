# IFoxFunKit

IFoxFunKit 是一个 C# 函数式编程类型库，提供 `Option<T>` 和 `Result<T, TErr>` 类型。

## 特性

- **Option<T>** - 表示可能有值的计算结果（类似 F# 的 Option）
- **Result<T, TErr>** - 表示可能失败的计算结果（类似 Rust 的 Result）
- **Roslyn 分析器** - 强制处理返回值，避免忽略错误

## 安装

```bash
dotnet add package IFoxFunKit
```

## 使用示例

### Option<T>

```csharp
using IFoxFunKit;

// 创建 Some 值
var someValue = Option<int>.Some(42);

// 使用 Match 处理所有分支
var result = someValue.Match(
    some: v => $"值是: {v}",
    none: () => "没有值"
);

// 使用 UnwrapOr 提供默认值
int value = noneValue.UnwrapOr(0);
```

### Result<T, TErr>

```csharp
using IFoxFunKit;

// 创建 Ok 和 Err 值
var okResult = Result<int, string>.Ok(100);
var errResult = Result<int, string>.Err("出错了");

// 使用 Match 处理所有分支
var message = okResult.Match(
    ok: v => $"成功: {v}",
    err: e => $"错误: {e}"
);
```

