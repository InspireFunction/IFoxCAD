// using System;
// using IFoxFunKit;

// namespace IFoxCAD.CAD.TestAnalyzer
// {
//     /// <summary>
//     /// 测试 IFoxFunKit 分析器是否正常工作
//     /// </summary>
//     public static class IFoxFunKitTest
//     {
//         /// <summary>
//         /// 这个方法故意不处理 Option 的返回值，应该触发分析器警告
//         /// </summary>
//         public static void TestOptionNotHandled()
//         {
//             // 这行应该触发分析器警告：Option<T>.IsSome 的返回值没有被处理
//             var _ = GetOption().IsSome;

//             // 这行也应该触发警告：Option<T>.Value 的返回值没有被处理
//             var __ = GetOption().Value;
//         }

//         /// <summary>
//         /// 这个方法正确处理了 Option 返回值，不应该有警告
//         /// </summary>
//         public static void TestOptionHandled()
//         {
//             var option = GetOption();

//             // 正确处理：检查 IsSome 并使用 Value
//             if (option.IsSome)
//             {
//                 var value = option.Value;
//                 Console.WriteLine(value);
//             }

//             // 或者使用 UnwrapOr 提供默认值
//             var result = option.UnwrapOr("default");
//         }

//         /// <summary>
//         /// 这个方法故意不处理 Result 的返回值，应该触发分析器警告
//         /// </summary>
//         public static void TestResultNotHandled()
//         {
//             // 这行应该触发分析器警告：Result<TOk, TErr>.IsOk 的返回值没有被处理
//             GetResult().IsOk;

//             // 这行也应该触发警告：Result<TOk, TErr>.OkValue 的返回值没有被处理
//             var _ = GetResult().OkValue;
//         }

//         /// <summary>
//         /// 这个方法正确处理了 Result 返回值，不应该有警告
//         /// </summary>
//         public static void TestResultHandled()
//         {
//             var result = GetResult();

//             // 正确处理：检查 IsOk 并使用 OkValue
//             if (result.IsOk)
//             {
//                 var value = result.OkValue;
//                 Console.WriteLine(value);
//             }
//             else
//             {
//                 var error = result.ErrValue;
//                 Console.WriteLine($"Error: {error}");
//             }
//         }

//         private static Option<string> GetOption()
//         {
//             return Option<string>.Some("test");
//         }

//         private static Result<string, string> GetResult()
//         {
//             return Result<string, string>.Ok("success");
//         }
//     }
// }
