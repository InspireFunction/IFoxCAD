// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace IFoxCAD.Cad;

/// <summary>
/// 交互设置扩展
/// </summary>
[SuppressMessage("ReSharper", "StringLiteralTypo")]
public static class PromptOptionsEx
{
    /// <summary>
    /// 保留关键字列表
    /// </summary>
    internal static readonly HashSet<string> SsGetSaveKeywords =
    [
        "WINDOW", "LAST", "CROSSWINDOW", "BOX", "ALL", "FENCE", "WPOLYGON", "CPOLYGON", "GROUP",
        "ADD", "REMOVE", "MULTIPLE", "PREVIOUS", "UNDO", "AUTO", "SINGLE", "SUBOBJECT", "OBJECT"
    ];

    /// <summary>
    /// 添加关键字(顺序为：关键字, 描述, 关键字, 描述)<br/>
    /// 例如 <c>pso.AddKeywords("SZ", "设置", "OP", "选项")</c>
    /// </summary>
    /// <param name="pso">选择集选项</param>
    /// <param name="keywords">关键字</param>
    public static void AddKeywords(this PromptSelectionOptions pso, params string[] keywords)
    {
        for (var i = 0; i < keywords.Length / 2; i++)
        {
            var key = keywords[i * 2].ToUpper();
            if (SsGetSaveKeywords.FirstOrDefault(e => e.StartsWith(key)) is { } saveKey)
            {
                throw new ArgumentException($"关键字{key}与选择集保留关键字{saveKey}冲突");
            }

            var message = keywords[i * 2 + 1];
            var end = $"({key})";
            if (!message.EndsWith(end))
            {
                message += end;
            }

            pso.Keywords.Add(key, key, message);
        }

        var displayString = pso.Keywords.GetDisplayString(true);
        pso.MessageForAdding += displayString;
        pso.MessageForRemoval += displayString;
    }

    /// <summary>
    /// 将关键视视为错误抛出
    /// </summary>
    /// <param name="pso">选择集选项</param>
    public static void ThrowKeywordAsException(this PromptSelectionOptions pso)
    {
        pso.KeywordInput -= PsoOnKeywordInput;
        pso.KeywordInput += PsoOnKeywordInput;
    }

    /// <summary>
    /// 选择集关键字输入时用于抛错的事件
    /// </summary>
    private static void PsoOnKeywordInput(object sender, SelectionTextInputEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Input))
            return;
        throw new KeywordException(e.Input.ToUpper());
    }
}

/// <summary>
/// 关键字错误
/// </summary>
public class KeywordException : Exception
{
    /// <summary>
    /// 关键字错误
    /// </summary>
    /// <param name="input">关键字</param>
    public KeywordException(string input)
    {
        Input = input;
    }

    /// <summary>
    /// 关键字
    /// </summary>
    public string Input { get; }
}