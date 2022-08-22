namespace TestConsole;

[Flags]
public enum PlugIn
{
    [Description("惊惊")]
    JoinBox = 1,
    [Description("源泉")]
    YuanQuan = 2,
    [Description("迷你")]
    IMinCad = 4,

    Lisp = JoinBox | YuanQuan | IMinCad,
     
    DOCBAR = 8,
    DUOTAB = 16,

    All = Lisp | DOCBAR | DUOTAB
}


[Flags]
public enum PlugIn2
{
    [Description("惊惊")]
    JoinBox = 1,
    [Description("源泉")]
    YuanQuan = 2,
    [Description("迷你")]
    IMinCad = 4,

    [Description("*Lisp*")]
    Lisp = JoinBox | YuanQuan | IMinCad,

    DOCBAR = 8,
    DUOTAB = 16,

    //all == *Lisp*|DOCBAR|DUOTAB
    //采取的行为是:注释的行为是特殊的,就按照注释的,否则,遍历子元素提取注释
    All = Lisp | DOCBAR | DUOTAB
}