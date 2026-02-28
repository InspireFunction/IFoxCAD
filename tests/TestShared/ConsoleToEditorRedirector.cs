using System.Diagnostics;

namespace Test;

/// <summary>
/// 重定向 Console
/// </summary>
public class ConsoleToEditorRedirector : TextWriter
{
    /// <summary>
    /// 重定向 Console
    /// </summary>
    public ConsoleToEditorRedirector(Document doc)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public override Encoding Encoding => Encoding.UTF8;

    /// <summary>
    /// 
    /// </summary>
    public override void WriteLine(string value)
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        doc?.Editor?.WriteMessage($"\n{value}");
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Write(string value)
    {

        var doc = Application.DocumentManager.MdiActiveDocument;
        doc?.Editor?.WriteMessage(value);
    }
}

public class Cmd
{
    /// <summary>
    /// 重定向Console命令
    /// </summary>
    /// <param name="doc"></param>
    [IFoxInitialize]
    public void RedirectConsole(Document doc)
    {
        Console.SetOut(new ConsoleToEditorRedirector(doc));
        Console.WriteLine("现在 Console.WriteLine 会输出到编辑器！\r");
    }
}
