using IFoxCAD.Cad;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace DllImportGeneratorTest;

class SimpleTest
{
    static void Main(string[] args)
    {
        Console.WriteLine("DllImport代码生成器 - 按类名分组生成");
        Console.WriteLine("========================================");

        var acadexePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoCAD 2008\\acad.exe");

        Console.WriteLine($"目标文件: {acadexePath}");
        Console.WriteLine();

        if (!File.Exists(acadexePath))
        {
            throw new Exception($"错误: 文件不存在: {acadexePath} 请确保 acad.exe 文件存在于指定路径");
        }

        try
        {
            Console.WriteLine("正在读取PE文件...");
            var peInfo = new PeInfo(acadexePath);

            if (!peInfo.OpenFile)
            {
                Console.WriteLine("错误: 无法打开文件");
                return;
            }

            var functionNames = peInfo.ExportDirectory.FunctionNames();
            Console.WriteLine($"找到 {functionNames.Count} 个导出函数");


            Console.WriteLine();


            Console.WriteLine("=== 调试: 解析 ??_7 虚函数表符号 ===");
            var vftableTests = new[] { "??_7AcPane@@6B@", "??_7AcDbObject@@6B@", "??_8SomeClass@@6B@" };
            foreach (var testFunc in vftableTests)
            {
                Console.WriteLine($"--- 测试: {testFunc} ---");
                var vftableParser = new CppMangledNameParser(testFunc);
                var vftableCppInfo = vftableParser.Parse();
                if (vftableCppInfo != null)
                {
                    Console.WriteLine($"[DEBUG] CppInfo.Name: '{vftableCppInfo.Name}'");
                    Console.WriteLine($"[DEBUG] CppInfo.ClassName: '{vftableCppInfo.ClassName}'");
                    Console.WriteLine($"[DEBUG] CppInfo.FullName: '{vftableCppInfo.FullName}'");
                    Console.WriteLine($"[DEBUG] CppInfo.IsConstructor: {vftableCppInfo.IsConstructor}");
                    Console.WriteLine($"[DEBUG] CppInfo.IsDestructor: {vftableCppInfo.IsDestructor}");
                    Console.WriteLine($"[DEBUG] CppInfo.IsDataSymbol: {vftableCppInfo.IsDataSymbol}");
                    Console.WriteLine($"[DEBUG] CppInfo.DataSymbolType: {vftableCppInfo.DataSymbolType}");
                    Console.WriteLine($"[DEBUG] CppInfo.HasErrors: {vftableCppInfo.HasErrors}");
                    Console.WriteLine($"[DEBUG] CppInfo.ParseErrors: [{string.Join(", ", vftableCppInfo.ParseErrors.ToArray())}]");

                    var vftableCsInfo = CppToCSharpMapper.MapFunction(vftableCppInfo);
                    if (vftableCsInfo != null)
                    {
                        Console.WriteLine($"[DEBUG] CsInfo.Name: '{vftableCsInfo.Name}'");
                        Console.WriteLine($"[DEBUG] CsInfo.ClassName: '{vftableCsInfo.ClassName}'");
                        Console.WriteLine($"[DEBUG] CsInfo.FullName: '{vftableCsInfo.FullName}'");
                        Console.WriteLine($"[DEBUG] CsInfo.IsDataSymbol: {vftableCsInfo.IsDataSymbol}");
                        Console.WriteLine($"[DEBUG] CsInfo.DataSymbolType: {vftableCsInfo.DataSymbolType}");
                        if (vftableCsInfo.IsDataSymbol)
                        {
                            Console.WriteLine($"[OK] 数据符号已正确识别");
                        }
                        else
                        {
                            Console.WriteLine($"[ERROR] 数据符号未被识别！");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] CsInfo 为 null");
                    }
                }
                else
                {
                    Console.WriteLine($"[ERROR] CppInfo 为 null");
                }
                Console.WriteLine();
            }
            Console.WriteLine("=== 调试结束 ===");
            Console.WriteLine();

            Console.WriteLine("=== 调试: 解析构造函数参数 ===");
            var ctorTests = new[] {
                "??0AcEdJig@@QAE@XZ",           // 无参构造函数
                "??0AcDbObject@@QAE@ABV0@@Z",   // 拷贝构造函数
                "??0AcGePoint3d@@QAE@NN@Z",     // 带两个double参数的构造函数
            };
            foreach (var testFunc in ctorTests)
            {
                Console.WriteLine($"--- 测试: {testFunc} ---");
                var parser = new CppMangledNameParser(testFunc);
                var cppInfo = parser.Parse();
                if (cppInfo != null)
                {
                    Console.WriteLine($"[DEBUG] Name: '{cppInfo.Name}'");
                    Console.WriteLine($"[DEBUG] ClassName: '{cppInfo.ClassName}'");
                    Console.WriteLine($"[DEBUG] IsConstructor: {cppInfo.IsConstructor}");
                    Console.WriteLine($"[DEBUG] CallingConvention: {cppInfo.CallingConvention}");
                    Console.WriteLine($"[DEBUG] ReturnType.TypeCode: {cppInfo.ReturnType.TypeCode}");
                    Console.WriteLine($"[DEBUG] Parameters.Count: {cppInfo.Parameters.Count}");
                    for (int i = 0; i < cppInfo.Parameters.Count; i++)
                    {
                        var p = cppInfo.Parameters[i];
                        Console.WriteLine($"[DEBUG]   Parameter[{i}]: TypeCode={p.TypeCode}, IsPointer={p.IsPointer}, IsReference={p.IsReference}, ClassName={p.ClassName}");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine("=== 调试结束 ===");
            Console.WriteLine();

            var dllName = Path.GetFileName(acadexePath);
            var namespaceName = "AutoCAD.Api";

            Console.WriteLine("正在生成代码...");
            var result = DllImportGenerator.GenerateGroupedDllImportCode(peInfo, namespaceName, dllName);

            Console.WriteLine();
            Console.WriteLine("=== 生成结果 ===");
            Console.WriteLine($"成功: {result.Success}");
            Console.WriteLine($"总函数数: {result.TotalFunctions}");
            Console.WriteLine($"成功处理: {result.SuccessfulFunctions}");
            Console.WriteLine($"失败处理: {result.FailedFunctions}");

            if (result.Errors.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("=== 错误列表 ===");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"[错误] {error}");
                }
            }

            if (result.Warnings.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"=== 警告列表 (共 {result.Warnings.Count} 条，显示前 20 条) ===");
                int warningCount = 0;
                foreach (var warning in result.Warnings)
                {
                    if (warningCount++ >= 20)
                    {
                        Console.WriteLine($"... 还有 {result.Warnings.Count - 20} 条警告未显示");
                        break;
                    }
                    Console.WriteLine($"[警告] {warning}");
                }
            }

            if (result.ParseErrorsByFunction.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"=== 解析错误详情 (共 {result.ParseErrorsByFunction.Count} 个函数，显示前 10 个) ===");
                int errorFuncCount = 0;
                foreach (var kvp in result.ParseErrorsByFunction)
                {
                    if (errorFuncCount++ >= 10)
                    {
                        Console.WriteLine($"... 还有 {result.ParseErrorsByFunction.Count - 10} 个函数有解析错误");
                        break;
                    }
                    Console.WriteLine($"函数: {kvp.Key}");
                    foreach (var err in kvp.Value)
                    {
                        Console.WriteLine($"  - {err}");
                    }
                }
            }

            // 放在项目文件夹上面,然后导出就可以自动加入本工程,查看语法是否错误了
            var ppp = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\DllImportGeneratorTest"));
            // ppp = Environment.CurrentDirectory;

            Console.WriteLine();
            Console.WriteLine("=== 导出接口列表到txt文件 ===");
            var interfaceListPath = Path.Combine(ppp, "acad08的exe接口.txt");
            var sortedNames = functionNames.OrderBy(n => n).ToArray();
            File.WriteAllLines(interfaceListPath, sortedNames, Encoding.UTF8);
            Console.WriteLine($"✓ 接口列表已保存: {interfaceListPath}");
            Console.WriteLine($"  函数数量: {sortedNames.Length}");
            Console.WriteLine($"acad08的exe接口: {interfaceListPath}");
            Console.WriteLine();

            if (result.Success)
            {
                var outputPath = Path.Combine(ppp, "generated_code.cs");
                File.WriteAllText(outputPath, result.Code);
                Console.WriteLine($"generated_code目标路径: {outputPath}");
                Console.WriteLine();
                var fileInfo = new FileInfo(outputPath);
                Console.WriteLine($"  文件大小: {fileInfo.Length / 1024:N0} KB");
            }

            Console.WriteLine("完成！");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"=== 异常信息 ===");
            Console.WriteLine($"类型: {ex.GetType().FullName}");
            Console.WriteLine($"消息: {ex.Message}");
            Console.WriteLine($"堆栈: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                Console.WriteLine();
                Console.WriteLine($"内部异常: {ex.InnerException.Message}");
            }
        }
    }
}
