namespace IFoxCAD.Cad;

internal static class SupportPathEx
{
    private const string kName = "ACAD";

    /// <summary>
    /// 获取支持路径
    /// </summary>
    /// <returns>路径列表</returns>
    public static List<string> Get()
    {
        var str = Env.GetEnv(kName);
        var set = str.ToLower()
            .Split([";"], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.TrimEnd('\\'))
            .ToHashSet();
        return set.ToList();
    }

    /// <summary>
    /// 添加支持路径
    /// </summary>
    /// <param name="dirs">路径列表</param>
    public static void Add(params IEnumerable<string> dirs)
    {
        var dirSet = dirs.ToLowerDirSet();
        if (dirSet.Count == 0)
            return;
        var supportPaths = Get();
        foreach (var dir in dirSet)
        {
            if (!Directory.Exists(dir))
                continue;
            if (supportPaths.Contains(dir))
                continue;
            supportPaths.Insert(0, dir);
        }

        Set(supportPaths);
    }

    /// <summary>
    /// 移除支持路径
    /// </summary>
    /// <param name="dirs">路径列表</param>
    public static void Remove(params IEnumerable<string> dirs)
    {
        var dirSet = dirs.ToLowerDirSet();
        var supportPaths = Get();
        supportPaths.RemoveAll(dirSet.Contains);
        Set(supportPaths);
    }

    /// <summary>
    /// 设置支持路径
    /// </summary>
    /// <param name="dirs">路径列表</param>
    private static void Set(IEnumerable<string> dirs)
    {
        var set = dirs.Where(Directory.Exists).ToLowerDirSet();
        var str = string.Join(";", set);
        Env.SetEnv(kName, str);
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