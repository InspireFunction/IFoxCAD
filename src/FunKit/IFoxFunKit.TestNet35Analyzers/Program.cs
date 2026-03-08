//#define SHOW_ERRORS

using System;
using IFoxFunKit;

namespace IFoxFunKit.TestNet35Analyzers;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("测试 .NET 3.5 项目中的分析器 - 错误示例");
    }

    static Option<int> Divide(int a, int b)
    {
        if (b == 0)
            return Option<int>.None;
        return Option<int>.Some(a / b);
    }

    static Result<int> SafeDivide(int a, int b)
    {
        if (b == 0)
            return Result<int>.Err(new Exception("除数不能为零"));
        return Result<int>.Ok(a / b);
    }

    // 错误: 忽略 Option<T> 返回值，可能丢失错误信息
#if SHOW_ERRORS
    static void Example1_IgnoreReturnValue()
    {
        Divide(10, 0);
    }
#endif

    // 正确: 检查 IsSome 并处理结果
    static void Example1_Correct()
    {
        var result = Divide(10, 0);
        if (result.IsSome)
            Console.WriteLine($"结果: {result.Value}");
        else
            Console.WriteLine("除数为零");
    }

    // 错误: 变量声明后未处理 Option<T> 的值
#if SHOW_ERRORS
    static void Example2_VariableNotHandled()
    {
        var result = Divide(10, 2);
        Console.WriteLine("完成");
    }
#endif

    // 正确: 使用 Match 方法处理 Some 和 None 两种情况
    static void Example2_Correct()
    {
        var result = Divide(10, 2);
        result.Match(
            some: v => Console.WriteLine($"结果: {v}"),
            none: () => Console.WriteLine("除数为零")
        );
    }

    // 错误: 赋值语句后未处理 Option<T> 的值
#if SHOW_ERRORS
    static void Example3_AssignmentNotHandled()
    {
        Option<int> result;
        result = Divide(10, 2);
    }
#endif

    // 正确: 赋值后检查 IsNone 并处理
    static void Example3_Correct()
    {
        Option<int> result = Divide(10, 2);
        if (result.IsNone)
            Console.WriteLine("除数为零");
        else
            Console.WriteLine($"结果: {result.Value}");
    }

    // 错误: 忽略 Result<T> 返回值，可能丢失错误信息
#if SHOW_ERRORS
    static void Example4_ResultNotHandled()
    {
        SafeDivide(10, 0);
    }
#endif

    // 正确: 使用 Match 方法处理 Ok 和 Err 两种情况
    static void Example4_Correct()
    {
        var result = SafeDivide(10, 0);
        result.Match(
            ok: v => Console.WriteLine($"结果: {v}"),
            err: e => Console.WriteLine($"错误: {e.Message}")
        );
    }

    // 错误: 变量声明后未处理 Result<T> 的值
#if SHOW_ERRORS
    static void Example5_ResultVariableNotHandled()
    {
        var result = SafeDivide(10, 2);
        Console.WriteLine("计算完成");
    }
#endif

    // 正确: 检查 IsOk 并处理结果
    static void Example5_Correct()
    {
        var result = SafeDivide(10, 2);
        if (result.IsOk)
            Console.WriteLine($"结果: {result.OkValue}");
        else
            Console.WriteLine($"错误: {result.ErrValue.Message}");
    }

    // 错误: 直接使用 Option<T>.Value 属性而不检查 IsSome
    // 分析器 IFOX002 会报告错误：在访问 'Value' 之前必须先检查 'IsSome 或 IsNone'
#if SHOW_ERRORS
    static void Example6_DirectValueAccess()
    {
        var opt = Divide(10, 0);
        var value = opt.Value; // 编译错误！必须先检查 IsSome
    }
#endif

    // 正确: 先检查 IsSome 再访问 Value
    static void Example6_Correct()
    {
        var opt = Divide(10, 0);
        if (opt.IsSome)
        {
            var value = opt.Value; // 安全访问
            Console.WriteLine($"结果: {value}");
        }
        else
        {
            Console.WriteLine("除数为零");
        }
    }

    // 正确: 使用 IsNone 检查后访问 Value（在 else 分支中）
    static void Example7_CheckIsNoneThenAccessValue()
    {
        var opt = Divide(10, 0);
        if (opt.IsNone)
        {
            Console.WriteLine("除数为零");
        }
        else
        {
            var value = opt.Value; // 安全访问，因为在 else 分支中，等价于 IsSome
            Console.WriteLine($"结果: {value}");
        }
    }

    // 正确: 使用三元运算符
    static void Example8_TernaryOperator()
    {
        var opt = Divide(10, 0);
        var value = opt.IsSome ? opt.Value : 0; // 安全访问
        Console.WriteLine($"结果: {value}");
    }

    // 正确: 使用逻辑与短路
    static void Example9_LogicalAnd()
    {
        var opt = Divide(10, 0);
        if (opt.IsSome && opt.Value > 0) // 安全访问
        {
            Console.WriteLine("结果大于0");
        }
    }

    // 错误: Result 的 OkValue 未检查 IsOk
#if SHOW_ERRORS
    static void Example10_ResultOkValueWithoutCheck()
    {
        var result = SafeDivide(10, 0);
        var value = result.OkValue; // 编译错误！必须先检查 IsOk
    }
#endif

    // 正确: 检查 IsOk 后访问 OkValue
    static void Example10_Correct()
    {
        var result = SafeDivide(10, 0);
        if (result.IsOk)
        {
            var value = result.OkValue; // 安全访问
            Console.WriteLine($"结果: {value}");
        }
    }

    // 错误: Result 的 ErrValue 未检查 IsErr
#if SHOW_ERRORS
    static void Example11_ResultErrValueWithoutCheck()
    {
        var result = SafeDivide(10, 0);
        var error = result.ErrValue; // 编译错误！必须先检查 IsErr
    }
#endif

    // 正确: 检查 IsErr 后访问 ErrValue
    static void Example11_Correct()
    {
        var result = SafeDivide(10, 0);
        if (result.IsErr)
        {
            var error = result.ErrValue; // 安全访问
            Console.WriteLine($"错误: {error.Message}");
        }
    }
}
