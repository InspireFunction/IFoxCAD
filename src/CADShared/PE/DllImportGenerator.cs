using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IFoxCAD.Cad;

public class DllImportGeneratorResult
{
    public bool Success { get; set; }
    public string Code { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new List<string>();
    public List<string> Warnings { get; set; } = new List<string>();
    public int TotalFunctions { get; set; }
    public int SuccessfulFunctions { get; set; }
    public int FailedFunctions { get; set; }
    public Dictionary<string, List<string>> ParseErrorsByFunction { get; set; } = new Dictionary<string, List<string>>();
}

public class CompileResult
{
    public bool Success { get; set; }
    public System.Reflection.Assembly? Assembly { get; set; }
    public string? Errors { get; set; }
    public string? Code { get; set; }
}

public static class DllImportGenerator
{
    private const int MAX_FUNCTIONS = 50000;
    private const int MAX_CLASSES = 5000;
    private const int MAX_FUNCTIONS_PER_CLASS = 2000;

    private static readonly HashSet<string> CSharpKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is",
        "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
        "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte",
        "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
        "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
        "ushort", "using", "virtual", "void", "volatile", "while"
    };

    public static DllImportGeneratorResult GenerateDllImportCode(
        PeInfo peInfo,
        string namespaceName,
        string className,
        string dllName)
    {
        var result = new DllImportGeneratorResult();

        if (peInfo == null)
        {
            result.Errors.Add("peInfo 参数为空");
            return result;
        }

        if (string.IsNullOrEmpty(namespaceName))
        {
            result.Errors.Add("namespaceName 参数为空");
            return result;
        }

        if (string.IsNullOrEmpty(className))
        {
            result.Errors.Add("className 参数为空");
            return result;
        }

        if (string.IsNullOrEmpty(dllName))
        {
            result.Errors.Add("dllName 参数为空");
            return result;
        }

        try
        {
            var sb = new StringBuilder();

            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine();
            sb.AppendLine("    using System;");
            sb.AppendLine("    using System.Runtime.InteropServices;");
            sb.AppendLine();

            sb.AppendLine($"    public static class {SanitizeIdentifier(className)}");
            sb.AppendLine("    {");

            var functionNames = peInfo.ExportDirectory?.FunctionNames();
            if (functionNames == null)
            {
                functionNames = new HashSet<string>();
            }

            result.TotalFunctions = functionNames.Count;

            if (functionNames.Count > MAX_FUNCTIONS)
            {
                result.Warnings.Add($"函数数量 {functionNames.Count} 超过限制 {MAX_FUNCTIONS}，只处理前 {MAX_FUNCTIONS} 个");
                functionNames = new HashSet<string>(functionNames.Take(MAX_FUNCTIONS));
            }

            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int duplicateCount = 0;

            foreach (var functionName in functionNames)
            {
                if (string.IsNullOrEmpty(functionName))
                {
                    continue;
                }

                try
                {
                    var parser = new CppMangledNameParser(functionName);
                    var cppInfo = parser.Parse();

                    if (cppInfo == null)
                    {
                        result.FailedFunctions++;
                        var parserErrors = parser.GetErrors();
                        if (parserErrors.Count > 0)
                        {
                            result.ParseErrorsByFunction[functionName] = parserErrors.Select(e => $"位置{e.Position}: {e.Message}").ToList();
                        }
                        continue;
                    }

                    var csInfo = CppToCSharpMapper.MapFunction(cppInfo);
                    if (csInfo == null)
                    {
                        result.FailedFunctions++;
                        result.Warnings.Add($"函数 {functionName} 映射失败");
                        continue;
                    }

                    var uniqueName = GenerateUniqueMethodName(csInfo, usedNames, ref duplicateCount);
                    GenerateMethodDeclaration(sb, functionName, uniqueName, csInfo, dllName);
                    result.SuccessfulFunctions++;
                }
                catch (Exception ex)
                {
                    result.FailedFunctions++;
                    result.Warnings.Add($"处理函数 {functionName} 时发生异常: {ex.Message}");
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            result.Code = sb.ToString();
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"生成代码时发生异常: {ex.Message}");
            result.Success = false;
        }

        return result;
    }

    public static DllImportGeneratorResult GenerateGroupedDllImportCode(
        PeInfo peInfo,
        string namespaceName,
        string dllName)
    {
        var result = new DllImportGeneratorResult();

        if (peInfo == null)
        {
            result.Errors.Add("peInfo 参数为空");
            return result;
        }

        if (string.IsNullOrEmpty(namespaceName))
        {
            result.Errors.Add("namespaceName 参数为空");
            return result;
        }

        if (string.IsNullOrEmpty(dllName))
        {
            result.Errors.Add("dllName 参数为空");
            return result;
        }

        try
        {
            var sb = new StringBuilder();

            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine();
            sb.AppendLine("    using System;");
            sb.AppendLine("    using System.Runtime.InteropServices;");
            sb.AppendLine();

            var functionNames = peInfo.ExportDirectory?.FunctionNames();
            if (functionNames == null)
            {
                functionNames = new HashSet<string>();
            }

            result.TotalFunctions = functionNames.Count;

            if (functionNames.Count > MAX_FUNCTIONS)
            {
                result.Warnings.Add($"函数数量 {functionNames.Count} 超过限制 {MAX_FUNCTIONS}，只处理前 {MAX_FUNCTIONS} 个");
                functionNames = new HashSet<string>(functionNames.Take(MAX_FUNCTIONS));
            }

            var classGroups = new Dictionary<string, List<FunctionInfo>>(StringComparer.OrdinalIgnoreCase);
            var globalFunctions = new List<FunctionInfo>();

            foreach (var functionName in functionNames)
            {
                if (string.IsNullOrEmpty(functionName))
                {
                    continue;
                }

                try
                {
                    var parser = new CppMangledNameParser(functionName);
                    var cppInfo = parser.Parse();

                    if (cppInfo == null)
                    {
                        result.FailedFunctions++;
                        var parserErrors = parser.GetErrors();
                        if (parserErrors.Count > 0)
                        {
                            result.ParseErrorsByFunction[functionName] = parserErrors.Select(e => $"位置{e.Position}: {e.Message}").ToList();
                        }
                        continue;
                    }

                    var csInfo = CppToCSharpMapper.MapFunction(cppInfo);
                    if (csInfo == null)
                    {
                        result.FailedFunctions++;
                        result.Warnings.Add($"函数 {functionName} 映射失败");
                        continue;
                    }

                    var functionInfo = new FunctionInfo
                    {
                        EntryPoint = functionName,
                        CppInfo = cppInfo,
                        CsInfo = csInfo
                    };

                    var className = !string.IsNullOrEmpty(csInfo.ClassName) ? csInfo.ClassName : "_Global";

                    if (!classGroups.ContainsKey(className))
                    {
                        classGroups[className] = new List<FunctionInfo>();
                    }

                    if (classGroups[className].Count >= MAX_FUNCTIONS_PER_CLASS)
                    {
                        result.Warnings.Add($"类 {className} 的函数数量超过限制 {MAX_FUNCTIONS_PER_CLASS}");
                        continue;
                    }

                    classGroups[className].Add(functionInfo);
                    result.SuccessfulFunctions++;
                }
                catch (Exception ex)
                {
                    result.FailedFunctions++;
                    result.Warnings.Add($"处理函数 {functionName} 时发生异常: {ex.Message}");
                }
            }

            if (classGroups.Count > MAX_CLASSES)
            {
                result.Warnings.Add($"类数量 {classGroups.Count} 超过限制 {MAX_CLASSES}，只处理前 {MAX_CLASSES} 个");
                classGroups = classGroups.Take(MAX_CLASSES).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            foreach (var group in classGroups.OrderBy(g => g.Key))
            {
                var className = SanitizeIdentifier(group.Key);
                var functions = group.Value;

                sb.AppendLine($"    /// <summary>");
                sb.AppendLine($"    /// {group.Key} 类的Native方法 ({functions.Count} 个函数)");
                sb.AppendLine($"    /// </summary>");
                sb.AppendLine($"    public static class {className}");
                sb.AppendLine("    {");

                var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int duplicateCount = 0;

                foreach (var func in functions)
                {
                    var uniqueName = GenerateUniqueMethodName(func.CsInfo, usedNames, ref duplicateCount, isGroupedMode: true);
                    GenerateMethodDeclaration(sb, func.EntryPoint, uniqueName, func.CsInfo, dllName);
                }

                sb.AppendLine("    }");
                sb.AppendLine();
            }

            sb.AppendLine("}");

            result.Code = sb.ToString();
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"生成代码时发生异常: {ex.Message}");
            result.Success = false;
        }

        return result;
    }

    private static string GenerateUniqueMethodName(
        CSharpFunctionInfo csInfo,
        HashSet<string> usedNames,
        ref int duplicateCount,
        bool isGroupedMode = false)
    {
        string baseName;
        
        if (isGroupedMode && !string.IsNullOrEmpty(csInfo.ClassName))
        {
            baseName = csInfo.Name;
        }
        else
        {
            baseName = csInfo.FullName;
        }

        if (csInfo.IsConstructor)
        {
            baseName = isGroupedMode ? "ctor" : $"{csInfo.ClassName}_ctor";
        }
        else if (csInfo.IsDestructor)
        {
            baseName = isGroupedMode ? "dtor" : $"{csInfo.ClassName}_dtor";
        }

        baseName = SanitizeIdentifier(baseName);

        var uniqueName = baseName;
        int suffix = 1;

        while (usedNames.Contains(uniqueName))
        {
            duplicateCount++;
            uniqueName = $"{baseName}_{suffix}";
            suffix++;

            if (suffix > 10000)
            {
                uniqueName = $"{baseName}_dup_{duplicateCount}";
                break;
            }
        }

        usedNames.Add(uniqueName);
        return uniqueName;
    }

    private static void GenerateMethodDeclaration(
        StringBuilder sb,
        string entryPoint,
        string methodName,
        CSharpFunctionInfo csInfo,
        string dllName)
    {
        if (csInfo.IsDataSymbol)
        {
            GenerateDataSymbolDeclaration(sb, entryPoint, methodName, csInfo, dllName);
            return;
        }

        var callingConv = csInfo.CallingConvention.ToString();

        sb.AppendLine($"        [DllImport(\"{dllName}\", EntryPoint = \"{EscapeString(entryPoint)}\", CallingConvention = CallingConvention.{callingConv})]");

        var returnType = csInfo.ReturnType;
        var parameters = csInfo.GetParameterList();

        sb.AppendLine($"        public static extern {returnType} {methodName}({parameters});");
        sb.AppendLine();
    }

    private static void GenerateDataSymbolDeclaration(
        StringBuilder sb,
        string entryPoint,
        string methodName,
        CSharpFunctionInfo csInfo,
        string dllName)
    {
        var symbolTypeDesc = csInfo.DataSymbolType switch
        {
            DataSymbolType.Vftable => "虚函数表",
            DataSymbolType.Vbtable => "虚基类表",
            DataSymbolType.Vcall => "虚函数调用",
            DataSymbolType.String => "字符串常量",
            DataSymbolType.Typeof => "类型信息",
            DataSymbolType.LocalStaticGuard => "局部静态守卫",
            _ => "数据符号"
        };

        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// {symbolTypeDesc} - {csInfo.ClassName}");
        sb.AppendLine($"        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。");
        sb.AppendLine($"        /// </summary>");
        sb.AppendLine($"        [DllImport(\"{dllName}\", EntryPoint = \"{EscapeString(entryPoint)}\", CallingConvention = CallingConvention.Cdecl)]");
        sb.AppendLine($"        public static extern IntPtr {methodName}();");
        sb.AppendLine();
    }

    private static string SanitizeIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "_";
        }

        var sb = new StringBuilder();

        char firstChar = name[0];
        if (!char.IsLetter(firstChar) && firstChar != '_')
        {
            sb.Append('_');
        }

        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                sb.Append(c);
            }
            else if (c == ':' || c == ' ')
            {
                if (sb.Length > 0 && sb[sb.Length - 1] != '_')
                {
                    sb.Append('_');
                }
            }
            else
            {
                sb.Append('_');
            }
        }

        var result = sb.ToString();

        if (string.IsNullOrEmpty(result) || result == "_")
        {
            return "_func";
        }

        if (CSharpKeywords.Contains(result))
        {
            result = "_" + result;
        }

        return result;
    }

    private static string EscapeString(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    public static void GenerateDllImportFile(
        PeInfo peInfo,
        string namespaceName,
        string className,
        string dllName,
        string outputPath)
    {
        var result = GenerateDllImportCode(peInfo, namespaceName, className, dllName);
        
        if (!result.Success)
        {
            throw new InvalidOperationException($"生成代码失败: {string.Join(", ", result.Errors.ToArray())}");
        }

        File.WriteAllText(outputPath, result.Code, Encoding.UTF8);
    }

    public static CompileResult CompileDllImportCode(
        PeInfo peInfo,
        string namespaceName,
        string className,
        string dllName)
    {
        var result = GenerateDllImportCode(peInfo, namespaceName, className, dllName);
        
        if (!result.Success)
        {
            return new CompileResult
            {
                Success = false,
                Errors = string.Join(", ", result.Errors.ToArray()),
                Code = result.Code
            };
        }

        return CompileCode(result.Code);
    }

    public static CompileResult CompileGroupedDllImportCode(
        PeInfo peInfo,
        string namespaceName,
        string dllName)
    {
        var result = GenerateGroupedDllImportCode(peInfo, namespaceName, dllName);
        
        if (!result.Success)
        {
            return new CompileResult
            {
                Success = false,
                Errors = string.Join(", ", result.Errors.ToArray()),
                Code = result.Code
            };
        }

        return CompileCode(result.Code);
    }

    public static CompileResult CompileCode(string code)
    {
        var result = new CompileResult { Code = code };

        try
        {
            var provider = CodeDomProvider.CreateProvider("CSharp");
            var parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false,
                CompilerOptions = "/optimize"
            };

            parameters.ReferencedAssemblies.Add("System.dll");

            var compilerResult = provider.CompileAssemblyFromSource(parameters, code);

            result.Success = !compilerResult.Errors.HasErrors;

            if (compilerResult.Errors.HasErrors)
            {
                var errors = new StringBuilder();
                foreach (CompilerError error in compilerResult.Errors)
                {
                    errors.AppendLine($"行 {error.Line}: {error.ErrorText}");
                }
                result.Errors = errors.ToString();
            }
            else
            {
                result.Assembly = compilerResult.CompiledAssembly;
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors = $"编译异常: {ex.Message}";
        }

        return result;
    }

    private class FunctionInfo
    {
        public string EntryPoint { get; set; } = string.Empty;
        public CppFunctionInfo CppInfo { get; set; } = null!;
        public CSharpFunctionInfo CsInfo { get; set; } = null!;
    }
}
