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
