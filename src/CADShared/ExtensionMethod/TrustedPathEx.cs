namespace IFoxCAD.Cad;

internal static class TrustedPathEx
{
    private const string kName = "TRUSTEDPATHS";

    /// <summary>
    /// 获取信任路径
    /// </summary>
    /// <returns>路径列表</returns>
    public static List<string> Get()
    {
        var str = Env.GetVar(kName).ToString()!;
        var set = str.ToLower()
            .Split([";"], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.TrimEnd('\\'))
            .ToHashSet();
        return set.ToList();
    }

    /// <summary>
    /// 添加信任路径
    /// </summary>
    /// <param name="dirs">路径列表</param>
    public static void Add(params IEnumerable<string> dirs)
    {
        var dirSet = dirs.ToLowerDirSet();
        if (dirSet.Count == 0)
            return;
        var paths = Get();
        foreach (var dir in dirSet)
        {
            if (!Directory.Exists(dir))
                continue;
            if (paths.Contains(dir))
                continue;
            paths.Insert(0, dir);
        }

        Set(paths);
    }

    /// <summary>
    /// 移除信任路径
    /// </summary>
    /// <param name="dirs">路径列表</param>
    public static void Remove(params IEnumerable<string> dirs)
    {
        var dirSet = dirs.ToLowerDirSet();
        var paths = Get();
        paths.RemoveAll(dirSet.Contains);
        Set(paths);
    }

    /// <summary>
    /// 设置信任路径
    /// </summary>
    /// <param name="dirs">路径列表</param>
    private static void Set(IEnumerable<string> dirs)
    {
        var set = dirs.Where(Directory.Exists).ToLowerDirSet();
        var str = string.Join(";", set);
        Env.SetVar(kName, str);
    }

    /// <summary>
    /// 转换为小写并去除尾部反斜杠的路径集合
    /// </summary>
    /// <param name="dirs">路径列表</param>
    /// <returns>路径集合</returns>
    private static HashSet<string> ToLowerDirSet(this IEnumerable<string> dirs)
    {
        return dirs.Select(dir => dir.ToLower().TrimEnd('\\')).ToHashSet();
    }
}