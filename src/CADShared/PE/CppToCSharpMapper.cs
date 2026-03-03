#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

using System;
using System.Collections.Generic;
using System.Text;

namespace IFoxCAD.Cad;

public class CSharpParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "IntPtr";
    public CppType? CppType { get; set; }
    public bool IsThisPointer { get; set; }
}

public class CSharpFunctionInfo
{
    public string Name { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string FullName => string.IsNullOrEmpty(ClassName) ? Name : $"{ClassName}_{Name}";
    public bool IsConstructor { get; set; }
    public bool IsDestructor { get; set; }
    public bool IsDataSymbol { get; set; }
    public DataSymbolType DataSymbolType { get; set; } = DataSymbolType.None;
    public string ReturnType { get; set; } = "void";
    public List<CSharpParameter> Parameters { get; set; } = new List<CSharpParameter>();
    public System.Runtime.InteropServices.CallingConvention CallingConvention { get; set; }
    public bool HasEllipsis { get; set; }
    public List<string> Warnings { get; set; } = new List<string>();

    public string GetParameterList()
    {
        return CppToCSharpMapper.GenerateParameterList(this);
    }
}

public static class CppToCSharpMapper
{
    private static readonly Dictionary<CppTypeCode, string> BasicTypeMap = new Dictionary<CppTypeCode, string>
    {
        { CppTypeCode.Void, "void" },
        { CppTypeCode.Bool, "bool" },
        { CppTypeCode.Char, "sbyte" },
        { CppTypeCode.UnsignedChar, "byte" },
        { CppTypeCode.Short, "short" },
        { CppTypeCode.UnsignedShort, "ushort" },
        { CppTypeCode.Int, "int" },
        { CppTypeCode.UnsignedInt, "uint" },
        { CppTypeCode.Long, "int" },
        { CppTypeCode.UnsignedLong, "uint" },
        { CppTypeCode.LongLong, "long" },
        { CppTypeCode.UnsignedLongLong, "ulong" },
        { CppTypeCode.Float, "float" },
        { CppTypeCode.Double, "double" },
        { CppTypeCode.LongDouble, "decimal" },
        { CppTypeCode.WChar, "char" },
    };

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

    public static string MapToCSharp(CppType? cppType, bool isReturnType = false)
    {
        if (cppType == null)
        {
            return "void";
        }

        if (cppType.IsPointer || cppType.IsReference)
        {
            if (cppType.TypeCode == CppTypeCode.Char && cppType.IsPointer)
            {
                return isReturnType ? "IntPtr" : "string";
            }

            if (cppType.TypeCode == CppTypeCode.WChar && cppType.IsPointer)
            {
                return isReturnType ? "IntPtr" : "string";
            }

            if (cppType.TypeCode == CppTypeCode.Void && cppType.IsPointer)
            {
                return "IntPtr";
            }

            return "IntPtr";
        }

        if (BasicTypeMap.TryGetValue(cppType.TypeCode, out var csType))
        {
            return csType;
        }

        if (cppType.TypeCode == CppTypeCode.Class)
        {
            return "IntPtr";
        }

        if (cppType.TypeCode == CppTypeCode.Ellipsis)
        {
            return "__arglist";
        }

        if (cppType.TypeCode == CppTypeCode.Unknown)
        {
            return "IntPtr";
        }

        return "int";
    }

    public static CSharpFunctionInfo? MapFunction(CppFunctionInfo? cppInfo)
    {
        if (cppInfo == null)
        {
            return null;
        }

        var csInfo = new CSharpFunctionInfo
        {
            Name = SanitizeIdentifier(cppInfo.Name),
            ClassName = string.IsNullOrEmpty(cppInfo.ClassName) ? "" : SanitizeIdentifier(cppInfo.ClassName),
            IsConstructor = cppInfo.IsConstructor,
            IsDestructor = cppInfo.IsDestructor,
            IsDataSymbol = cppInfo.IsDataSymbol,
            DataSymbolType = cppInfo.DataSymbolType,
            CallingConvention = MapCallingConvention(cppInfo.CallingConvention)
        };

        if (cppInfo.IsDataSymbol)
        {
            csInfo.ReturnType = "IntPtr";
            return csInfo;
        }

        csInfo.ReturnType = MapToCSharp(cppInfo.ReturnType, true);

        if (!cppInfo.IsStatic && !string.IsNullOrEmpty(cppInfo.ClassName))
        {
            csInfo.Parameters.Add(new CSharpParameter
            {
                Name = "thisPtr",
                Type = "IntPtr",
                IsThisPointer = true
            });
        }

        for (int i = 0; i < cppInfo.Parameters.Count; i++)
        {
            var param = cppInfo.Parameters[i];
            var paramName = $"arg{i}";

            if (param.TypeCode == CppTypeCode.Ellipsis)
            {
                csInfo.HasEllipsis = true;
                csInfo.Warnings.Add($"函数 {cppInfo.Name} 包含可变参数，已跳过");
                continue;
            }

            csInfo.Parameters.Add(new CSharpParameter
            {
                Name = paramName,
                Type = MapToCSharp(param),
                CppType = param
            });
        }

        return csInfo;
    }

    public static System.Runtime.InteropServices.CallingConvention MapCallingConvention(CppCallingConvention convention)
    {
        return convention switch
        {
            CppCallingConvention.Cdecl => System.Runtime.InteropServices.CallingConvention.Cdecl,
            CppCallingConvention.Stdcall => System.Runtime.InteropServices.CallingConvention.StdCall,
            CppCallingConvention.ThisCall => System.Runtime.InteropServices.CallingConvention.ThisCall,
            CppCallingConvention.FastCall => System.Runtime.InteropServices.CallingConvention.FastCall,
            _ => System.Runtime.InteropServices.CallingConvention.Cdecl
        };
    }

    public static string SanitizeIdentifier(string name)
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

    public static string GenerateParameterList(CSharpFunctionInfo? csInfo)
    {
        if (csInfo?.Parameters == null || csInfo.Parameters.Count == 0)
        {
            return "";
        }

        var sb = new StringBuilder();
        for (int i = 0; i < csInfo.Parameters.Count; i++)
        {
            var param = csInfo.Parameters[i];

            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(param.Type);
            sb.Append(" ");
            sb.Append(param.Name);
        }

        return sb.ToString();
    }
}
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
