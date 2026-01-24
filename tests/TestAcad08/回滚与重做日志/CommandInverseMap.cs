namespace JoinBoxAcad;

// 命令映射表
public static class CommandInverseMap
{
    // 这里不允许写任何的绘图命令,因为绘图的命令太多了,成千上万,
    // 它们通用逆操作就是创建-删除,还有撤销每个新旧值修改,这些都是无法用命令表达的.
    // 所以利用数据处理就好了啊.
    // 1,优先命中可逆命令,如果存在就清空数据区,然后执行逆命令.
    // 2,没有命中,就执行数据库撤回步骤.  DatabaseMonitor.cs上面
    static readonly Dictionary<string, string> _commandPairs = new(StringComparer.OrdinalIgnoreCase)
    {
        // CAD命令映射
        { "REFEDIT", "REFCLOSE _D" },
        //{ "REFCLOSE _S", "U" },  // 保存修改
        //{ "REFCLOSE _D", "U" },  // 放弃修改
    };

    public static string GetInverseCommand(string command)
    {
        if (_commandPairs.TryGetValue(command, out var inverse))
            return inverse;

        // 尝试匹配模式
        if (command.StartsWith("_"))
            return GetInverseCommand(command.Substring(1));

        return string.Empty;
    }
}
