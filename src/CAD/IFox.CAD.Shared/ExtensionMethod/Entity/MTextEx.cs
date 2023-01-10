namespace IFoxCAD.Cad;

/// <summary>
/// 多行文字扩展类
/// </summary>
public static class MTextEx
{

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
    public static void ExplodeFragments<T>(this MText mt, T obj, Func<MTextFragment, T, MTextFragmentCallbackStatus> mTextFragmentCallback)
    {
        mt.ExplodeFragments(
            (f, o) => 
            mTextFragmentCallback(f, (T)o), obj);
    }

    /// <summary>
    /// 获取多行文字的无格式文本
    /// </summary>
    /// <param name="mt">多行文字</param>
    /// <returns>文本</returns>
    public static string GetUnFormatString(this MText mt)
    {
        List<string> strs = new();
        mt.ExplodeFragments(
            strs,
            (f, o) => {
                o.Add(f.Text);
                return MTextFragmentCallbackStatus.Continue;
            });
        return string.Join("", strs.ToArray());
    }

}