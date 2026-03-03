#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
using System;
using System.Collections.Generic;
using System.Text;

namespace IFoxCAD.Cad;

public enum CppTypeCode
{
    Void,
    Bool,
    Char,
    UnsignedChar,
    Short,
    UnsignedShort,
    Int,
    UnsignedInt,
    Long,
    UnsignedLong,
    LongLong,
    UnsignedLongLong,
    Float,
    Double,
    LongDouble,
    WChar,
    Class,
    Ellipsis,
    Unknown
}

public enum CppCallingConvention
{
    Cdecl,
    Stdcall,
    ThisCall,
    FastCall,
    VectorCall,
    Unknown
}

public enum AccessModifier
{
    Public,
    Protected,
    Private,
    Unknown
}

public class CppType
{
    public CppTypeCode TypeCode { get; set; } = CppTypeCode.Void;
    public string ClassName { get; set; } = string.Empty;
    public bool IsPointer { get; set; }
    public bool IsReference { get; set; }
    public bool IsConst { get; set; }
    public bool IsVolatile { get; set; }

    public string GetTypeName()
    {
        var sb = new StringBuilder();
        if (IsConst) sb.Append("const ");
        if (IsVolatile) sb.Append("volatile ");
        sb.Append(GetBaseTypeName());
        if (IsPointer) sb.Append("*");
        if (IsReference) sb.Append("&");
        return sb.ToString();
    }

    private string GetBaseTypeName()
    {
        return TypeCode switch
        {
            CppTypeCode.Void => "void",
            CppTypeCode.Bool => "bool",
            CppTypeCode.Char => "char",
            CppTypeCode.UnsignedChar => "unsigned char",
            CppTypeCode.Short => "short",
            CppTypeCode.UnsignedShort => "unsigned short",
            CppTypeCode.Int => "int",
            CppTypeCode.UnsignedInt => "unsigned int",
            CppTypeCode.Long => "long",
            CppTypeCode.UnsignedLong => "unsigned long",
            CppTypeCode.LongLong => "long long",
            CppTypeCode.UnsignedLongLong => "unsigned long long",
            CppTypeCode.Float => "float",
            CppTypeCode.Double => "double",
            CppTypeCode.LongDouble => "long double",
            CppTypeCode.WChar => "wchar_t",
            CppTypeCode.Class => ClassName,
            CppTypeCode.Ellipsis => "...",
            _ => "int"
        };
    }
}

public enum DataSymbolType
{
    None,
    Vftable,
    Vbtable,
    Vcall,
    String,
    Typeof,
    LocalStaticGuard,
    VbaseDtor,
    VectorDtor,
    DefaultCtor,
    ScalarDtor,
    VecCtor,
    VecDtor,
    VecVbase,
    CopyCtor,
    UnknownData
}

public class CppFunctionInfo
{
    public string Name { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string FullName => string.IsNullOrEmpty(ClassName) ? Name : $"{ClassName}::{Name}";
    public bool IsConstructor { get; set; }
    public bool IsDestructor { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsStatic { get; set; }
    public bool IsDataSymbol { get; set; }
    public DataSymbolType DataSymbolType { get; set; } = DataSymbolType.None;
    public CppType ReturnType { get; set; } = new CppType();
    public List<CppType> Parameters { get; set; } = new List<CppType>();
    public CppCallingConvention CallingConvention { get; set; } = CppCallingConvention.Cdecl;
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public List<string> ParseErrors { get; set; } = new List<string>();
    public bool HasErrors => ParseErrors.Count > 0;
}

public class ParseError
{
    public int Position { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
}

public class CppMangledNameParser
{
    private const int MAX_ITERATIONS = 1000;
    private const int MAX_NAME_LENGTH = 1000;
    private const int MAX_PARAMETERS = 100;

    private readonly string _mangledName;
    private int _position;
    private readonly List<ParseError> _errors = new List<ParseError>();
    private int _iterationCount;

    public CppMangledNameParser(string mangledName)
    {
        _mangledName = mangledName ?? string.Empty;
        _position = 0;
        _iterationCount = 0;
    }

    public CppFunctionInfo? Parse()
    {
        if (string.IsNullOrEmpty(_mangledName))
        {
            AddError(0, "修饰名为空");
            return null;
        }

        if (_mangledName.Length > MAX_NAME_LENGTH)
        {
            AddError(0, $"修饰名过长: {_mangledName.Length} > {MAX_NAME_LENGTH}");
            return null;
        }

        try
        {
            if (_mangledName.StartsWith("_"))
            {
                return ParseCName();
            }

            if (_mangledName.StartsWith("?"))
            {
                return ParseCppName();
            }

            return CreateSimpleFunction(_mangledName);
        }
        catch (Exception ex)
        {
            AddError(_position, $"解析异常: {ex.Message}");
            return null;
        }
    }

    public List<ParseError> GetErrors() => new List<ParseError>(_errors);

    private void AddError(int pos, string message)
    {
        var context = pos < _mangledName.Length
            ? _mangledName.Substring(Math.Max(0, pos - 5), Math.Min(10, _mangledName.Length - Math.Max(0, pos - 5)))
            : _mangledName;
        _errors.Add(new ParseError { Position = pos, Message = message, Context = context });
    }

    private bool CheckIteration()
    {
        _iterationCount++;
        if (_iterationCount > MAX_ITERATIONS)
        {
            AddError(_position, $"超过最大迭代次数 {MAX_ITERATIONS}，可能存在死循环");
            return false;
        }
        return true;
    }

    private bool AdvancePosition(int steps = 1)
    {
        _position += steps;
        if (_position > _mangledName.Length)
        {
            AddError(_position, "位置超出字符串范围");
            return false;
        }
        return true;
    }

    private CppFunctionInfo CreateSimpleFunction(string name)
    {
        return new CppFunctionInfo
        {
            Name = name,
            ReturnType = new CppType { TypeCode = CppTypeCode.Void },
            Parameters = new List<CppType>(),
            CallingConvention = CppCallingConvention.Cdecl
        };
    }

    private CppFunctionInfo ParseCName()
    {
        var name = _mangledName;

        if (name.StartsWith("_"))
            name = name.Substring(1);

        var atIndex = name.IndexOf('@');
        if (atIndex > 0)
        {
            name = name.Substring(0, atIndex);
        }

        var info = CreateSimpleFunction(name);
        info.CallingConvention = atIndex > 0 ? CppCallingConvention.Stdcall : CppCallingConvention.Cdecl;
        return info;
    }

    private CppFunctionInfo? ParseCppName()
    {
        _position = 0;
        var info = new CppFunctionInfo();

        if (!AdvancePosition())
            return null;

        var nameParts = ParseNameParts();
        if (nameParts.Count == 0)
        {
            AddError(_position, "无法解析函数名");
            return null;
        }

        var firstPart = nameParts[0];
        if (string.IsNullOrEmpty(firstPart))
        {
            AddError(_position, "函数名为空");
            return null;
        }

        if (firstPart.StartsWith("??"))
        {
            ParseSpecialName(firstPart, info, nameParts);
        }
        else
        {
            info.Name = firstPart;
            if (nameParts.Count > 1)
            {
                info.ClassName = string.Join("::", nameParts.GetRange(1, nameParts.Count - 1).ToArray());
            }
        }

        SkipAtSigns();

        if (_position < _mangledName.Length)
        {
            bool isMemberFunction = !string.IsNullOrEmpty(info.ClassName) || info.IsConstructor || info.IsDestructor;

            if (isMemberFunction)
            {
                if (IsStaticDataMemberCode())
                {
                    ParseStaticDataMember(info);
                    return info;
                }

                if (!ParseAccessModifier(info))
                    return info;

                if (!ParseFunctionCode(info))
                    return info;
            }
            else
            {
                if (!ParseGlobalFunctionCode(info))
                    return info;
            }

            if (!info.IsConstructor && !info.IsDestructor && _position < _mangledName.Length)
            {
                info.ReturnType = ParseType();
            }

            info.Parameters = ParseParameterList();
        }

        return info;
    }

    private bool IsStaticDataMemberCode()
    {
        if (_position >= _mangledName.Length)
            return false;

        var code = _mangledName[_position];
        return code == '0' || code == '1' || code == '2';
    }

    private void ParseStaticDataMember(CppFunctionInfo info)
    {
        if (_position >= _mangledName.Length)
            return;

        var code = _mangledName[_position];
        info.AccessModifier = code switch
        {
            '0' => AccessModifier.Private,
            '1' => AccessModifier.Protected,
            '2' => AccessModifier.Public,
            _ => AccessModifier.Private
        };

        info.IsDataSymbol = true;
        info.DataSymbolType = DataSymbolType.UnknownData;
        info.IsStatic = true;

        if (!AdvancePosition()) return;

        if (_position < _mangledName.Length)
        {
            info.ReturnType = ParseType();
        }
    }

    private bool ParseGlobalFunctionCode(CppFunctionInfo info)
    {
        if (_position >= _mangledName.Length)
            return true;

        var code = _mangledName[_position];

        switch (code)
        {
            case 'Y':
            case 'Z':
            info.CallingConvention = CppCallingConvention.Cdecl;
            break;
            default:
            info.CallingConvention = CppCallingConvention.Cdecl;
            break;
        }

        return AdvancePosition();
    }

    private List<string> ParseNameParts()
    {
        var parts = new List<string>();
        int consecutiveAtCount = 0;

        while (_position < _mangledName.Length && consecutiveAtCount < 2)
        {
            if (!CheckIteration()) break;

            var part = ParseIdentifier();
            if (string.IsNullOrEmpty(part))
                break;

            parts.Add(part);
            consecutiveAtCount = 0;

            if (_position < _mangledName.Length && _mangledName[_position] == '@')
            {
                if (!AdvancePosition()) break;

                if (_position < _mangledName.Length && _mangledName[_position] == '@')
                {
                    if (!AdvancePosition()) break;
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return parts;
    }

    private string ParseIdentifier()
    {
        if (_position >= _mangledName.Length)
            return string.Empty;

        if (_mangledName[_position] == '?')
        {
            return ParseSpecialIdentifier();
        }

        var sb = new StringBuilder();
        int startCount = _iterationCount;

        while (_position < _mangledName.Length)
        {
            if (!CheckIteration()) break;
            if (_iterationCount - startCount > 200)
            {
                AddError(_position, "标识符解析超过200次迭代");
                break;
            }

            var c = _mangledName[_position];
            if (c == '@')
                break;

            sb.Append(c);
            if (!AdvancePosition()) break;
        }

        return sb.ToString();
    }

    private string ParseSpecialIdentifier()
    {
        if (_position >= _mangledName.Length || _mangledName[_position] != '?')
            return string.Empty;

        if (!AdvancePosition()) return string.Empty;

        if (_position >= _mangledName.Length)
            return "??";

        if (_mangledName[_position] == '?')
        {
            if (!AdvancePosition()) return "??";

            string code;
            if (_position < _mangledName.Length && _mangledName[_position] == '_')
            {
                if (!AdvancePosition()) return "??_";
                code = "??_" + _mangledName[_position];
                if (!AdvancePosition()) return code;
            }
            else
            {
                code = "??" + (_position < _mangledName.Length ? _mangledName[_position].ToString() : "");
                if (_position < _mangledName.Length && !AdvancePosition()) return code;
            }

            var rest = ReadUntilAt();
            return code + rest;
        }

        if (_mangledName[_position] == '_')
        {
            if (!AdvancePosition()) return "??_";
            var digit = _position < _mangledName.Length ? _mangledName[_position].ToString() : "";
            if (!AdvancePosition()) return "??_" + digit;
            var rest = ReadUntilAt();
            return "??_" + digit + rest;
        }

        var c = _mangledName[_position];
        if (char.IsLetterOrDigit(c))
        {
            var code = "??" + c.ToString();
            if (!AdvancePosition()) return code;
            var rest = ReadUntilAt();
            return code + rest;
        }

        var nameBuilder = new StringBuilder();
        while (_position < _mangledName.Length)
        {
            if (!CheckIteration()) break;
            var c2 = _mangledName[_position];
            if (c2 == '@')
                break;
            nameBuilder.Append(c2);
            if (!AdvancePosition()) break;
        }

        return nameBuilder.ToString();
    }

    private string ReadUntilAt()
    {
        var sb = new StringBuilder();
        while (_position < _mangledName.Length)
        {
            if (!CheckIteration()) break;
            if (_mangledName[_position] == '@')
                break;
            sb.Append(_mangledName[_position]);
            if (!AdvancePosition()) break;
        }
        return sb.ToString();
    }

    private void SkipAtSigns()
    {
        int skipCount = 0;
        while (_position < _mangledName.Length && _mangledName[_position] == '@')
        {
            if (!CheckIteration()) break;
            if (!AdvancePosition()) break;
            skipCount++;
            if (skipCount > 10)
            {
                AddError(_position, "跳过@符号超过10次");
                break;
            }
        }
    }

    private bool ParseAccessModifier(CppFunctionInfo info)
    {
        if (_position >= _mangledName.Length)
            return true;

        var code = _mangledName[_position];
        info.AccessModifier = code switch
        {
            'A' or 'B' or 'C' or 'D' => AccessModifier.Private,
            'E' or 'F' or 'G' or 'H' => AccessModifier.Protected,
            'I' or 'J' or 'K' or 'L' => AccessModifier.Public,
            'M' or 'N' or 'O' or 'P' => AccessModifier.Protected,
            'Q' or 'R' or 'S' or 'T' => AccessModifier.Public,
            'U' or 'V' or 'W' or 'X' => AccessModifier.Private,
            _ => AccessModifier.Public
        };

        return AdvancePosition();
    }

    private bool ParseFunctionCode(CppFunctionInfo info)
    {
        if (_position >= _mangledName.Length)
            return true;

        var code = _mangledName[_position];

        switch (code)
        {
            case 'A':
            case 'B':
            info.CallingConvention = CppCallingConvention.ThisCall;
            break;
            case 'C':
            case 'D':
            info.IsStatic = true;
            info.CallingConvention = CppCallingConvention.Cdecl;
            break;
            case 'E':
            case 'F':
            info.CallingConvention = CppCallingConvention.ThisCall;
            break;
            case 'G':
            case 'H':
            info.CallingConvention = CppCallingConvention.ThisCall;
            break;
            case 'I':
            case 'J':
            info.CallingConvention = CppCallingConvention.ThisCall;
            break;
            case 'K':
            case 'L':
            info.IsStatic = true;
            info.CallingConvention = CppCallingConvention.Cdecl;
            break;
            case 'M':
            case 'N':
            info.CallingConvention = CppCallingConvention.ThisCall;
            break;
            case 'O':
            case 'P':
            info.CallingConvention = CppCallingConvention.ThisCall;
            break;
            case 'Q':
            case 'R':
            info.CallingConvention = CppCallingConvention.ThisCall;
            break;
            case 'S':
            case 'T':
            info.IsStatic = true;
            info.CallingConvention = CppCallingConvention.Cdecl;
            break;
            case 'U':
            case 'V':
            info.CallingConvention = CppCallingConvention.ThisCall;
            break;
            case 'W':
            case 'X':
            info.CallingConvention = CppCallingConvention.ThisCall;
            break;
            case 'Y':
            case 'Z':
            info.CallingConvention = CppCallingConvention.Cdecl;
            break;
            default:
            info.CallingConvention = CppCallingConvention.Cdecl;
            break;
        }

        if (!AdvancePosition()) return false;

        if (_position < _mangledName.Length)
        {
            var nextChar = _mangledName[_position];
            if (nextChar == 'E' || nextChar == 'F')
            {
                if (!AdvancePosition()) return false;
            }

            if (_position < _mangledName.Length && _mangledName[_position] == '@')
            {
                if (!AdvancePosition()) return false;
            }
        }

        return true;
    }

    private void ParseSpecialName(string code, CppFunctionInfo info, List<string> nameParts)
    {
        string typeCode;
        string classNameFromCode = string.Empty;

        if (code.Length >= 4 && code.StartsWith("??_"))
        {
            typeCode = code.Substring(0, 4);
            if (code.Length > 4)
                classNameFromCode = code.Substring(4);
        }
        else if (code.Length >= 3)
        {
            typeCode = code.Substring(0, 3);
            if (code.Length > 3)
                classNameFromCode = code.Substring(3);
        }
        else
        {
            typeCode = code;
        }

        info.Name = typeCode switch
        {
            "??0" => ".ctor",
            "??1" => ".dtor",
            "??2" => "operator new",
            "??3" => "operator delete",
            "??4" => "operator=",
            "??5" => "operator>>",
            "??6" => "operator<<",
            "??7" => "operator*",
            "??8" => "operator++",
            "??9" => "operator--",
            "??A" => "operator-",
            "??B" => "operator+",
            "??C" => "operator&",
            "??D" => "operator->",
            "??E" => "operator*",
            "??F" => "operator/",
            "??G" => "operator%",
            "??H" => "operator+",
            "??I" => "operator-",
            "??J" => "operator<<",
            "??K" => "operator>>",
            "??L" => "operator==",
            "??M" => "operator!=",
            "??N" => "operator<=",
            "??O" => "operator>=",
            "??P" => "operator&&",
            "??Q" => "operator||",
            "??R" => "operator*",
            "??S" => "operator+",
            "??T" => "operator-",
            "??U" => "operator/",
            "??V" => "operator%",
            "??W" => "operator^",
            "??X" => "operator&",
            "??Y" => "operator|",
            "??Z" => "operator~",
            "??_0" => "operator new[]",
            "??_1" => "operator delete[]",
            "??_2" => "operator->*",
            "??_3" => "operator=",
            "??_4" => "operator->*",
            "??_5" => "operator,",
            "??_6" => "operator()",
            "??_7" => "vftable",
            "??_8" => "vbtable",
            "??_9" => "vcall",
            "??_A" => "typeof",
            "??_B" => "local_static_guard",
            "??_C" => "string",
            "??_D" => "vbase_dtor",
            "??_E" => "vector_dtor",
            "??_F" => "default_ctor",
            "??_G" => "scalar_dtor",
            "??_H" => "vec_ctor",
            "??_I" => "vec_dtor",
            "??_J" => "vec_vbase",
            "??_K" => "copy_ctor",
            _ => code
        };

        info.IsConstructor = typeCode == "??0";
        info.IsDestructor = typeCode == "??1";

        switch (typeCode)
        {
            case "??_7":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.Vftable;
            info.Name = "vftable";
            break;
            case "??_8":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.Vbtable;
            info.Name = "vbtable";
            break;
            case "??_9":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.Vcall;
            info.Name = "vcall";
            break;
            case "??_A":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.Typeof;
            break;
            case "??_B":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.LocalStaticGuard;
            break;
            case "??_C":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.String;
            break;
            case "??_D":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.VbaseDtor;
            break;
            case "??_E":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.VectorDtor;
            break;
            case "??_F":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.DefaultCtor;
            break;
            case "??_G":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.ScalarDtor;
            break;
            case "??_H":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.VecCtor;
            break;
            case "??_I":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.VecDtor;
            break;
            case "??_J":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.VecVbase;
            break;
            case "??_K":
            info.IsDataSymbol = true;
            info.DataSymbolType = DataSymbolType.CopyCtor;
            break;
        }

        if (!string.IsNullOrEmpty(classNameFromCode))
        {
            info.ClassName = classNameFromCode;
        }
        else if (nameParts.Count > 1)
        {
            info.ClassName = string.Join("::", nameParts.GetRange(1, nameParts.Count - 1).ToArray());
        }
    }

    private CppType ParseType()
    {
        if (_position >= _mangledName.Length)
            return new CppType { TypeCode = CppTypeCode.Void };

        var type = new CppType();
        var c = _mangledName[_position];
        if (!AdvancePosition()) return type;

        type.TypeCode = c switch
        {
            'X' => CppTypeCode.Void,
            'D' => CppTypeCode.Char,
            'E' => CppTypeCode.UnsignedChar,
            'F' => CppTypeCode.Short,
            'G' => CppTypeCode.UnsignedShort,
            'H' => CppTypeCode.Int,
            'I' => CppTypeCode.UnsignedInt,
            'J' => CppTypeCode.Long,
            'K' => CppTypeCode.UnsignedLong,
            'M' => CppTypeCode.Float,
            'N' => CppTypeCode.Double,
            'O' => CppTypeCode.LongDouble,
            'W' => CppTypeCode.WChar,
            'P' => ParsePointerType(type, false, false),
            'Q' => ParsePointerType(type, true, false),
            'R' => ParsePointerType(type, false, true),
            'S' => ParsePointerType(type, true, true),
            'U' => ParseStructType(type),
            'V' => ParseClassType(type),
            '_' => ParseExtendedType(type),
            '@' => ParseAtTerminator(type),
            '?' => ParseComplexType(type),
            'A' => ParseReferenceModifier(type),
            'B' => ParseVolatileModifier(type),
            'C' => ParseConstModifier(type),
            _ => CppTypeCode.Int
        };

        return type;
    }

    private CppTypeCode ParseReferenceModifier(CppType type)
    {
        type.IsReference = true;
        var innerType = ParseType();
        type.TypeCode = innerType.TypeCode;
        type.IsPointer = innerType.IsPointer;
        type.ClassName = innerType.ClassName;
        return type.TypeCode;
    }

    private CppTypeCode ParseVolatileModifier(CppType type)
    {
        type.IsVolatile = true;
        var innerType = ParseType();
        type.TypeCode = innerType.TypeCode;
        type.IsPointer = innerType.IsPointer;
        type.IsReference = innerType.IsReference;
        type.ClassName = innerType.ClassName;
        return type.TypeCode;
    }

    private CppTypeCode ParseConstModifier(CppType type)
    {
        type.IsConst = true;
        var innerType = ParseType();
        type.TypeCode = innerType.TypeCode;
        type.IsPointer = innerType.IsPointer;
        type.IsReference = innerType.IsReference;
        type.ClassName = innerType.ClassName;
        return type.TypeCode;
    }

    private CppTypeCode ParsePointerType(CppType type, bool isConst, bool isVolatile)
    {
        type.IsPointer = true;
        if (isConst) type.IsConst = true;
        if (isVolatile) type.IsVolatile = true;

        if (_position < _mangledName.Length)
        {
            var ptrModifier = _mangledName[_position];
            if (!AdvancePosition()) return type.TypeCode;

            switch (ptrModifier)
            {
                case 'A':
                break;
                case 'B':
                type.IsConst = true;
                break;
                case 'C':
                type.IsVolatile = true;
                break;
                case 'E':
                break;
                case 'F':
                break;
                case 'G':
                type.IsConst = true;
                break;
                case 'H':
                type.IsVolatile = true;
                break;
                case 'I':
                type.IsConst = true;
                type.IsVolatile = true;
                break;
            }
        }

        var innerType = ParseType();
        type.TypeCode = innerType.TypeCode;
        type.ClassName = innerType.ClassName;
        if (innerType.IsConst) type.IsConst = true;
        if (innerType.IsVolatile) type.IsVolatile = true;

        return type.TypeCode;
    }

    private CppTypeCode ParseReferenceType(CppType type)
    {
        type.IsReference = true;
        return ParseType().TypeCode;
    }

    private CppTypeCode ParseClassType(CppType type)
    {
        type.TypeCode = CppTypeCode.Class;
        type.ClassName = ParseClassName();
        return type.TypeCode;
    }

    private CppTypeCode ParseStructType(CppType type)
    {
        type.TypeCode = CppTypeCode.Class;
        type.ClassName = ParseClassName();
        return type.TypeCode;
    }

    private CppTypeCode ParseExtendedType(CppType type)
    {
        if (_position >= _mangledName.Length)
        {
            AddError(_position, "扩展类型代码缺失");
            return CppTypeCode.Int;
        }

        var extCode = _mangledName[_position];
        if (!AdvancePosition()) return CppTypeCode.Int;

        return extCode switch
        {
            'N' => CppTypeCode.Bool,
            'J' => CppTypeCode.LongLong,
            'K' => CppTypeCode.UnsignedLongLong,
            'W' => CppTypeCode.WChar,
            _ => CppTypeCode.Int
        };
    }

    private CppTypeCode ParseAtTerminator(CppType type)
    {
        _position--;
        return CppTypeCode.Void;
    }

    private CppTypeCode ParseComplexType(CppType type)
    {
        type.TypeCode = CppTypeCode.Class;
        type.ClassName = ParseComplexTypeName();
        return type.TypeCode;
    }

    private string ParseClassName()
    {
        var sb = new StringBuilder();
        int loopCount = 0;

        while (_position < _mangledName.Length && loopCount < 200)
        {
            loopCount++;
            if (!CheckIteration()) break;

            var c = _mangledName[_position];
            if (c == '@')
            {
                if (!AdvancePosition()) break;
                break;
            }

            sb.Append(c);
            if (!AdvancePosition()) break;
        }

        if (loopCount >= 200)
        {
            AddError(_position, "类名解析超过200次迭代");
        }

        return sb.ToString();
    }

    private string ParseComplexTypeName()
    {
        if (_position >= _mangledName.Length || _mangledName[_position] != '?')
            return string.Empty;

        if (!AdvancePosition()) return string.Empty;

        if (_position >= _mangledName.Length)
            return string.Empty;

        var c = _mangledName[_position];
        if (!AdvancePosition()) return string.Empty;

        switch (c)
        {
            case 'A':
            case 'B':
            if (_position < _mangledName.Length && _mangledName[_position] == 'W')
            {
                if (!AdvancePosition()) return string.Empty;
                if (_position < _mangledName.Length && _mangledName[_position] == '4')
                {
                    if (!AdvancePosition()) return string.Empty;
                }
            }
            return ParseClassName();

            case 'V':
            case 'U':
            return ParseClassName();

            default:
            return ParseClassName();
        }
    }

    private List<CppType> ParseParameterList()
    {
        var parameters = new List<CppType>();
        int paramCount = 0;

        while (_position < _mangledName.Length && paramCount < MAX_PARAMETERS)
        {
            paramCount++;
            if (!CheckIteration()) break;

            var c = _mangledName[_position];

            if (c == '@' || c == 'Z')
            {
                if (!AdvancePosition()) break;
                break;
            }

            if (c == 'X')
            {
                if (!AdvancePosition()) break;
                break;
            }

            var startPos = _position;
            var type = ParseType();

            if (_position == startPos)
            {
                AddError(_position, $"参数解析位置未前进，强制跳过字符 '{_mangledName[_position]}'");
                if (!AdvancePosition()) break;
            }

            parameters.Add(type);
        }

        if (paramCount >= MAX_PARAMETERS)
        {
            AddError(_position, $"参数数量超过最大限制 {MAX_PARAMETERS}");
        }

        return parameters;
    }
}

#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
