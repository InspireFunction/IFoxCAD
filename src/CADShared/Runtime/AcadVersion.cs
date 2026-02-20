#pragma warning disable CS1591 // 缺少XML注释
#pragma warning disable CS1572 // XML注释中有不存在的参数
#pragma warning disable CS1573 // 参数在XML注释中没有匹配的参数标记


namespace IFoxCAD.Cad;

/// <summary>
/// cad版本号类
/// </summary>
public static class AcadVersion
{
    private static readonly string _pattern = @"Autodesk\\AutoCAD\\R(\d+)\.(\d+)\\.*?";

    /// <summary>
    /// 所有安装的cad的版本号
    /// </summary>
    public static List<CadVersion> Versions
    {
        get
        {
            string[] copys = Registry.LocalMachine
                            .OpenSubKey(@"SOFTWARE\Autodesk\Hardcopy")
                            .GetValueNames();

            var _versions = new List<CadVersion>();
            for (int i = 0; i < copys.Length; i++)
            {
                if (!Regex.IsMatch(copys[i], _pattern))
                    continue;

                var gs = Regex.Match(copys[i], _pattern).Groups;
                var ver = new CadVersion
                {
                    ProductRootKey = copys[i],
                    ProductName = Registry.LocalMachine
                                .OpenSubKey("SOFTWARE")
                                .OpenSubKey(copys[i])
                                .GetValue("ProductName")
                                .ToString(),

                    Major = int.Parse(gs[1].Value),
                    Minor = int.Parse(gs[2].Value),
                };
                _versions.Add(ver);
            }
            return _versions;
        }
    }

    /// <summary>已打开的cad的版本号</summary>
    /// <param name="app">已打开cad的application对象</param>
    /// <returns>cad版本号对象</returns>
    public static CadVersion? FromApp(object app)
    {
        if (app == null)
            throw new ArgumentNullException(nameof(app));

        string acver = app.GetType()
                        .InvokeMember(
                            "Version",
                            BindingFlags.GetProperty,
                            null,
                            app,
                            new object[0]).ToString();

        var gs = Regex.Match(acver, @"(\d+)\.(\d+).*?").Groups;
        int major = int.Parse(gs[1].Value);
        int minor = int.Parse(gs[2].Value);
        for (int i = 0; i < Versions.Count; i++)
            if (Versions[i].Major == major && Versions[i].Minor == minor)
                return Versions[i];
        return null;
    }
}

public static class VersionTool
{
    // 年号.
    // 1,不用存年号数组,索引+2000就是年号了,
    // 2,官方是Acad2000i,用2001年顶替
    // 3,官方没有Acad2003,但要满足1的规则,全部数组用上年数据补齐03年.
    // static readonly int[] _years =>[
    // 2000, 2001, 2002, 2003/*虚拟03年*/,
    // 2004, 2005, 2006,
    // 2007, 2008, 2009,
    // 2010, 2011, 2012,
    // 2013, 2014, 2015, 2016, 2017,
    // 2018, 2019, 2020, 2021, 2022,
    // 2023, 2024, 2025];
    // 字段:public static readonly int[] Years;
    // 构造:Years = _years;
    // 异或判断数值是否一致
    // Debug.Assert((_years.Length ^ DwgVers.Length ^ AcadVers.Length) == AcadVers.Length, "怎么长度不一样了捏?");
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexToYear(int index)
    {
        var year = index + 2000;
        if (year < 2000) return -1;
        if (year > 2000 + _acadVers.Length) return -1;
        return year;
    }

    // 和二分法一样,
    // 没有命中时将第一个大于搜索值的索引作为返回值并取反,
    // 以提供没有命中的信息(负数)和最近值取反后再插入.
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int YearToIndex(int year)
    {
        var index = year - 2000;
        if (index < 0) return ~0;
        if (index > _acadVers.Length - 1)
            return ~(_acadVers.Length - 1);
        return index;
    }

    // https://www.autodesk.com.cn/support/technical/article/caas/sfdcarticles/sfdcarticles/CHS/drawing-version-codes-for-autocad.html
    // DWG签名的AC号.
    private static readonly int[] _dwgVers = [
        1015, 1015, 1015, 1015 /*补齐03年*/,
        1018, 1018, 1018,
        1021, 1021, 1021,
        1024, 1024, 1024,
        1027, 1027, 1027, 1027, 1027,
        1032, 1032, 1032, 1032, 1032,
        1032, 1032, 1032
    ];

    // CAD版本号,它与注册表和COM相关.
    private static readonly string[] _acadVers = [
        "R15.0", "R15.1", "R15.2", "R15.2" /*补齐03年*/,
        "R16.0", "R16.1", "R16.2",
        "R17.0", "R17.1", "R17.2",
        "R18.0", "R18.1", "R18.2",
        "R19.0", "R19.1", "R20.0", "R20.1", "R21.0",
        "R22.0", "R23.0", "R23.1", "R24.0", "R24.1",
        "R24.2", "R24.3", "R25.0"
    ];

    // 运行中的CAD版本号,位移+位或.
    public static readonly int AcadVerOR;

    // 这几个只读数组,有序等长,通过二分法共同索引.
    public static readonly int[] AcadVers;
    public static readonly int[] DwgVers;

    // 静态构造函数
    static VersionTool()
    {
        var major = Acap.Version.Major;
        var minor = Acap.Version.Minor;

        // Q16.16定点数:0.5*2^16来得到小数部分
        var fp = minor * 0.1f * (1 << 16);
        AcadVerOR = (int)(((uint)major << 16) | (ushort)fp);

        // 消除字符串从而不需要构造hashmap.
        // 位移+位或,使得它们从小数映射到整数,
        // 并且保持大小关系,仍然有序可以能够二分.
        // 编译时:C#没有循环消除,反编译源码还能看见,
        // 运行时:避免求一次循环,可以把结果再重新粘贴回代码.
        // --就像快速平方根倒数,夹逼求浮点数,再转16进制,然后粘贴到代码上,成为魔法数
        var v = new int[_acadVers.Length];
        for (var i = 0; i < _acadVers.Length; i++)
        {
            var s = _acadVers[i];
            var m = (s[1] - '0') * 10 + (s[2] - '0');
            var n = (s[4] - '0'); //0是R,3是小数点,所以跳到4
            var f = n * 0.1f * (1 << 16);
            v[i] = (int)(((uint)m << 16) | (ushort)f);
        }

        AcadVers = v;
        _acadVers = null!; //积极释放
        DwgVers = _dwgVers;

#if DEBUG

        // 防止开发者忘记更新数组,提供一个断言来进行维护
        var index = Array.BinarySearch(AcadVers, AcadVerOR);
        System.Diagnostics.Debug.Assert(index > 0, "运行中的CAD版本未记录,请维护DWG版本号/CAD版本号");
        System.Diagnostics.Debug.Assert(DwgVers.Length == AcadVers.Length, "怎么长度不一样了捏");

        // 解码版本号
        var major2 = AcadVerOR >> 16;
        var minor2 = AcadVerOR & 0xFFFF; //位与掩码,获取低16bit
#endif
    }
}