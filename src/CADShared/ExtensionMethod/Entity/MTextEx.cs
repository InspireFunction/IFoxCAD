using ArgumentNullException = System.ArgumentNullException;

namespace IFoxCAD.Cad;

/// <summary>
/// 多行文字扩展类
/// </summary>
public static class MTextEx
{
    /// <summary>
    /// 创建多行文字
    /// </summary>
    /// <param name="position">插入点</param>
    /// <param name="text">文本内容</param>
    /// <param name="height">文字高度</param>
    /// <param name="database">文字所在的数据库</param>
    /// <param name="action">文字属性设置委托</param>
    /// <returns>文字对象id</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public static MText CreateMText(Point3d position, string text, double height, Database? database = null,
        Action<MText>? action = null)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentNullException(nameof(text), "创建文字无内容");

        var db = database ?? DBTrans.Top.Database;
        using var _ = new SwitchDatabase(db);

        var mText = new MText();

        mText.SetDatabaseDefaults(db);

        mText.TextHeight = height; // 高度
        mText.Contents = text; // 内容
        mText.Location = position; // 插入点

        action?.Invoke(mText);

        return mText;
    }

    /// <summary>
    /// 炸散多行文字
    /// </summary>
    /// <typeparam name="T">存储多行文字炸散之后的对象的类型</typeparam>
    /// <param name="mt">多行文字</param>
    /// <param name="obj">存储对象变量</param>
    /// <param name="mTextFragmentCallback">回调函数，用于处理炸散之后的对象
    /// <para>
    /// <see cref="MTextFragment"/>多行文字炸散后的对象<br/>
    /// <see cref="MTextFragmentCallbackStatus"/>回调函数处理的结果
    /// </para>
    /// </param>
    public static void ExplodeFragments<T>(this MText mt, T obj,
        Func<MTextFragment, T, MTextFragmentCallbackStatus> mTextFragmentCallback)
    {
        mt.ExplodeFragments((f, o) => mTextFragmentCallback(f, (T)o), obj);
    }

    /// <summary>
    /// 获取多行文字的无格式文本
    /// </summary>
    /// <param name="mt">多行文字</param>
    /// <returns>文本</returns>
    public static string GetUnFormatString(this MText mt)
    {
        List<string> strs = [];
        mt.ExplodeFragments(strs, (f, o) =>
        {
            o.Add(f.Text);
            return MTextFragmentCallbackStatus.Continue;
        });
        return string.Join("", strs.ToArray());
    }
}