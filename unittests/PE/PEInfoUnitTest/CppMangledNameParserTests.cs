namespace PEInfoUnitTests;

public class CppMangledNameParserTests
{
    #region 访问修饰符测试
    [Theory]
    [InlineData("??0Test@@QAE@XZ", AccessModifier.Public, "Q=virtual public")]
    [InlineData("??0Test@@IAE@XZ", AccessModifier.Public, "I=public")]
    [InlineData("??0Test@@EAE@XZ", AccessModifier.Protected, "E=protected")]
    [InlineData("??0Test@@AAE@XZ", AccessModifier.Private, "A=private")]
    [InlineData("??0Test@@MAE@XZ", AccessModifier.Protected, "M=virtual protected")]
    public void Parse_AccessModifier_ShouldReturnCorrectValue(string mangled, AccessModifier expected, string description)
    {
        var parser = new CppMangledNameParser(mangled);
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Equal(expected, info.AccessModifier);
    }
    #endregion

    #region 调用约定测试
    [Theory]
    [InlineData("??0Test@@QAE@XZ", CppCallingConvention.ThisCall, "A=thiscall")]
    [InlineData("??0Test@@QCE@XZ", CppCallingConvention.Cdecl, "C=cdecl(static)")]
    [InlineData("??0Test@@QEE@XZ", CppCallingConvention.ThisCall, "E=thiscall")]
    [InlineData("??0Test@@QGE@XZ", CppCallingConvention.ThisCall, "G=thiscall")]
    [InlineData("??0Test@@QIE@XZ", CppCallingConvention.ThisCall, "I=thiscall")]
    [InlineData("??0Test@@QKE@XZ", CppCallingConvention.Cdecl, "K=cdecl(static)")]
    [InlineData("??0Test@@QYE@XZ", CppCallingConvention.Cdecl, "Y=cdecl")]
    public void Parse_CallingConvention_ShouldReturnCorrectValue(string mangled, CppCallingConvention expected, string description)
    {
        var parser = new CppMangledNameParser(mangled);
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Equal(expected, info.CallingConvention);
    }
    #endregion

    #region 返回类型测试
    [Theory]
    [InlineData("?foo@@YAXXZ", CppTypeCode.Void, "X=void")]
    [InlineData("?foo@@YAHXZ", CppTypeCode.Int, "H=int")]
    [InlineData("?foo@@YANXZ", CppTypeCode.Double, "N=double")]
    [InlineData("?foo@@YAMXZ", CppTypeCode.Float, "M=float")]
    [InlineData("?foo@@YAJXZ", CppTypeCode.Long, "J=long")]
    [InlineData("?foo@@YAKXZ", CppTypeCode.UnsignedLong, "K=unsigned long")]
    [InlineData("?foo@@YAFXZ", CppTypeCode.Short, "F=short")]
    [InlineData("?foo@@YAGXZ", CppTypeCode.UnsignedShort, "G=unsigned short")]
    [InlineData("?foo@@YADXZ", CppTypeCode.Char, "D=char")]
    [InlineData("?foo@@YAEXZ", CppTypeCode.UnsignedChar, "E=unsigned char")]
    [InlineData("?foo@@YA_NXZ", CppTypeCode.Bool, "_N=bool")]
    [InlineData("?foo@@YA_JXZ", CppTypeCode.LongLong, "_J=long long")]
    [InlineData("?foo@@YA_KXZ", CppTypeCode.UnsignedLongLong, "_K=unsigned long long")]
    [InlineData("?foo@@YA_WXZ", CppTypeCode.WChar, "_W=wchar_t")]
    public void Parse_ReturnType_ShouldReturnCorrectValue(string mangled, CppTypeCode expected, string description)
    {
        var parser = new CppMangledNameParser(mangled);
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Equal(expected, info.ReturnType.TypeCode);
    }
    #endregion

    #region 参数列表测试
    [Fact]
    public void Parse_NoParameters_XZ_ShouldReturnEmptyList()
    {
        var parser = new CppMangledNameParser("??0AcEdJig@@QAE@XZ");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Empty(info.Parameters);
    }

    [Fact]
    public void Parse_TwoDoubleParameters_NN_ShouldReturnTwoDoubles()
    {
        var parser = new CppMangledNameParser("??0AcGePoint3d@@QAE@NN@Z");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Equal(2, info.Parameters.Count);
        Assert.Equal(CppTypeCode.Double, info.Parameters[0].TypeCode);
        Assert.Equal(CppTypeCode.Double, info.Parameters[1].TypeCode);
    }

    [Fact]
    public void Parse_TwoIntParameters_HH_ShouldReturnTwoInts()
    {
        var parser = new CppMangledNameParser("?foo@@YAHHH@Z");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Equal(2, info.Parameters.Count);
        Assert.Equal(CppTypeCode.Int, info.Parameters[0].TypeCode);
        Assert.Equal(CppTypeCode.Int, info.Parameters[1].TypeCode);
    }

    [Fact]
    public void Parse_CopyConstructor_ABV_ShouldReturnClassReference()
    {
        var parser = new CppMangledNameParser("??0AcDbObject@@QAE@ABV0@@Z");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Single(info.Parameters);
        Assert.True(info.Parameters[0].IsReference);
        Assert.Equal(CppTypeCode.Class, info.Parameters[0].TypeCode);
    }
    #endregion

    #region 构造函数测试
    [Fact]
    public void Parse_Constructor_ShouldSetFlagsCorrectly()
    {
        var parser = new CppMangledNameParser("??0AcEdJig@@QAE@XZ");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Equal(".ctor", info.Name);
        Assert.Equal("AcEdJig", info.ClassName);
        Assert.True(info.IsConstructor);
        Assert.False(info.IsDestructor);
        Assert.False(info.IsDataSymbol);
    }
    #endregion

    #region 析构函数测试
    [Fact]
    public void Parse_Destructor_ShouldSetFlagsCorrectly()
    {
        var parser = new CppMangledNameParser("??1AcEdJig@@UAE@XZ");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Equal(".dtor", info.Name);
        Assert.Equal("AcEdJig", info.ClassName);
        Assert.False(info.IsConstructor);
        Assert.True(info.IsDestructor);
        Assert.False(info.IsDataSymbol);
    }
    #endregion

    #region 虚函数表测试
    [Theory]
    [InlineData("??_7AcPane@@6B@", "AcPane")]
    [InlineData("??_7AcDbObject@@6B@", "AcDbObject")]
    [InlineData("??_7AcEdJig@@6B@", "AcEdJig")]
    public void Parse_Vftable_ShouldSetDataSymbolFlags(string mangled, string expectedClassName)
    {
        var parser = new CppMangledNameParser(mangled);
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.True(info.IsDataSymbol);
        Assert.Equal(DataSymbolType.Vftable, info.DataSymbolType);
        Assert.Equal("vftable", info.Name);
        Assert.Equal(expectedClassName, info.ClassName);
    }
    #endregion

    #region 虚基类表测试
    [Fact]
    public void Parse_Vbtable_ShouldSetDataSymbolFlags()
    {
        var parser = new CppMangledNameParser("??_8SomeClass@@6B@");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.True(info.IsDataSymbol);
        Assert.Equal(DataSymbolType.Vbtable, info.DataSymbolType);
        Assert.Equal("vbtable", info.Name);
        Assert.Equal("SomeClass", info.ClassName);
    }
    #endregion

    #region 运算符重载测试
    [Theory]
    [InlineData("??4Test@@QAEAAV0@ABV0@@Z", "operator=")]
    [InlineData("??2Test@@SAPAXI@Z", "operator new")]
    [InlineData("??3Test@@SAXPAX@Z", "operator delete")]
    [InlineData("??6Test@@QAEAAV0@H@Z", "operator<<")]
    [InlineData("??5Test@@QAEAAV0@H@Z", "operator>>")]
    [InlineData("??_0Test@@SAPAXI@Z", "operator new[]")]
    [InlineData("??_1Test@@SAXPAX@Z", "operator delete[]")]
    [InlineData("??_6Test@@QAEAAV0@XZ", "operator()")]
    public void Parse_OperatorOverload_ShouldReturnCorrectName(string mangled, string expectedName)
    {
        var parser = new CppMangledNameParser(mangled);
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Equal(expectedName, info.Name);
    }
    #endregion

    #region 复杂函数测试
    [Fact]
    public void Parse_ComplexFunction_AcquireAngle_ShouldParseCorrectly()
    {
        var parser = new CppMangledNameParser("?acquireAngle@AcEdJig@@QAE?AW4DragStatus@1@AAN@Z");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Equal("acquireAngle", info.Name);
        Assert.Equal("AcEdJig", info.ClassName);
    }

    [Fact]
    public void Parse_StaticDataMember_ShouldParseCorrectly()
    {
        var parser = new CppMangledNameParser("?gpDesc@AcEdJig@@2PAVAcRxClass@@A");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Equal("gpDesc", info.Name);
        Assert.Equal("AcEdJig", info.ClassName);
    }
    #endregion

    #region 边界情况测试
    [Fact]
    public void Parse_EmptyString_ShouldReturnNull()
    {
        var parser = new CppMangledNameParser("");
        var info = parser.Parse();
        
        Assert.Null(info);
    }

    [Fact]
    public void Parse_NullString_ShouldReturnNull()
    {
        var parser = new CppMangledNameParser(null!);
        var info = parser.Parse();
        
        Assert.Null(info);
    }

    [Fact]
    public void Parse_SimpleCFunction_ShouldReturnBasicInfo()
    {
        var parser = new CppMangledNameParser("simpleFunc");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Equal("simpleFunc", info.Name);
        Assert.Equal(CppCallingConvention.Cdecl, info.CallingConvention);
    }
    #endregion

    #region void参数问题测试
    [Theory]
    [InlineData("??0CMetaFile@@QAE@AAUtagFORMATETC@@@Z", "CMetaFile", 1)]
    [InlineData("??0CMetaFileEnhanced@@QAE@AAUtagFORMATETC@@@Z", "CMetaFileEnhanced", 1)]
    [InlineData("??0CMetaFileStandard@@QAE@AAUtagFORMATETC@@@Z", "CMetaFileStandard", 1)]
    public void Parse_StructReferenceParameter_ShouldNotHaveVoidParameter(string mangled, string expectedClassName, int expectedParamCount)
    {
        var parser = new CppMangledNameParser(mangled);
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.True(info.IsConstructor);
        Assert.Equal(expectedClassName, info.ClassName);
        Assert.Equal(expectedParamCount, info.Parameters.Count);
        
        if (info.Parameters.Count > 0)
        {
            var firstParam = info.Parameters[0];
            Assert.NotEqual(CppTypeCode.Void, firstParam.TypeCode);
            Assert.True(firstParam.IsReference);
        }
    }

    [Fact]
    public void MapToCSharp_StructReferenceParameter_ShouldNotReturnVoid()
    {
        var parser = new CppMangledNameParser("??0CMetaFile@@QAE@AAUtagFORMATETC@@@Z");
        var cppInfo = parser.Parse();
        
        Assert.NotNull(cppInfo);
        Assert.Single(cppInfo.Parameters);
        
        var cppParam = cppInfo.Parameters[0];
        var csType = CppToCSharpMapper.MapToCSharp(cppParam);
        
        Assert.NotEqual("void", csType);
        Assert.Equal("IntPtr", csType);
    }

    [Fact]
    public void Parse_AAUPrefix_ShouldParseStructReferenceCorrectly()
    {
        var parser = new CppMangledNameParser("??0CMetaFile@@QAE@AAUtagFORMATETC@@@Z");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.True(info.IsConstructor);
        Assert.Equal("CMetaFile", info.ClassName);
        
        Assert.Single(info.Parameters);
        var param = info.Parameters[0];
        Assert.Equal(CppTypeCode.Class, param.TypeCode);
        Assert.True(param.IsReference);
        Assert.Equal("tagFORMATETC", param.ClassName);
    }
    #endregion

    #region 静态数据成员测试
    [Theory]
    [InlineData("?m_tcUserShellFolders@CProxyInet@@0PB_WB", "m_tcUserShellFolders", "CProxyInet", AccessModifier.Private)]
    [InlineData("?s_instance@MyClass@@2PAV1@A", "s_instance", "MyClass", AccessModifier.Public)]
    [InlineData("?g_value@Test@@0HA", "g_value", "Test", AccessModifier.Private)]
    [InlineData("?gpDesc@AcGzRotate@@2PAVAcRxClass@@A", "gpDesc", "AcGzRotate", AccessModifier.Public)]
    public void Parse_StaticDataMember_ShouldIdentifyAsDataSymbol(string mangled, string expectedName, string expectedClassName, AccessModifier expectedAccess)
    {
        var parser = new CppMangledNameParser(mangled);
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Equal(expectedName, info.Name);
        Assert.Equal(expectedClassName, info.ClassName);
        Assert.True(info.IsDataSymbol, "静态数据成员应该被标记为DataSymbol");
        Assert.Equal(DataSymbolType.UnknownData, info.DataSymbolType);
    }

    [Fact]
    public void Parse_StaticDataMember_ShouldNotHaveVoidParameter()
    {
        var parser = new CppMangledNameParser("?m_tcUserShellFolders@CProxyInet@@0PB_WB");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.True(info.IsDataSymbol, "静态数据成员应该被标记为DataSymbol");
        
        Assert.Empty(info.Parameters);
    }

    [Fact]
    public void MapFunction_StaticDataMember_ShouldNotGenerateVoidParameter()
    {
        var parser = new CppMangledNameParser("?m_tcUserShellFolders@CProxyInet@@0PB_WB");
        var cppInfo = parser.Parse();
        
        Assert.NotNull(cppInfo);
        
        var csInfo = CppToCSharpMapper.MapFunction(cppInfo);
        Assert.NotNull(csInfo);
        
        Assert.True(csInfo.IsDataSymbol);
        Assert.Empty(csInfo.Parameters);
        Assert.Equal("IntPtr", csInfo.ReturnType);
    }

    [Fact]
    public void Parse_StaticDataMember_TypeInfo_ShouldParseCorrectly()
    {
        var parser = new CppMangledNameParser("?m_tcUserShellFolders@CProxyInet@@0PB_WB");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.True(info.IsDataSymbol);
        
        Assert.True(info.ReturnType.IsPointer);
        Assert.True(info.ReturnType.IsConst);
        Assert.Equal(CppTypeCode.WChar, info.ReturnType.TypeCode);
    }

    [Theory]
    [InlineData('0', AccessModifier.Private)]
    [InlineData('1', AccessModifier.Protected)]
    [InlineData('2', AccessModifier.Public)]
    public void Parse_StaticDataMember_AccessModifier_ShouldParseCorrectly(char accessCode, AccessModifier expectedAccess)
    {
        var mangled = $"?s_data@Test@@{accessCode}HA";
        var parser = new CppMangledNameParser(mangled);
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.True(info.IsDataSymbol);
        Assert.Equal(expectedAccess, info.AccessModifier);
    }

    [Fact]
    public void Parse_StaticDataMember_AcGzRotate_gpDesc_ShouldParseCorrectly()
    {
        var parser = new CppMangledNameParser("?gpDesc@AcGzRotate@@2PAVAcRxClass@@A");
        var info = parser.Parse();
        
        Assert.NotNull(info);
        Assert.Equal("gpDesc", info.Name);
        Assert.Equal("AcGzRotate", info.ClassName);
        Assert.True(info.IsDataSymbol, "静态数据成员应该被标记为DataSymbol");
        Assert.Equal(AccessModifier.Public, info.AccessModifier);
        
        Assert.True(info.ReturnType.IsPointer);
        Assert.Equal(CppTypeCode.Class, info.ReturnType.TypeCode);
        Assert.Equal("AcRxClass", info.ReturnType.ClassName);
        
        var csInfo = CppToCSharpMapper.MapFunction(info);
        Assert.NotNull(csInfo);
        Assert.True(csInfo.IsDataSymbol);
        Assert.Empty(csInfo.Parameters);
        Assert.Equal("IntPtr", csInfo.ReturnType);
    }
    #endregion
}
