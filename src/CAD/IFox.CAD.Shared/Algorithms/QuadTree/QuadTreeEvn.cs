#pragma warning disable CA2211 // 非常量字段应当不可见
namespace IFoxCAD.Cad;
/// <summary>
/// 四叉树环境变量
/// </summary>
public class QuadTreeEvn
{
    /// <summary>
    /// 最小的节点有一个面积(一定要大于0)
    /// </summary>
    public static double MinArea = 1e-6;

    /// <summary>
    /// 选择模式
    /// </summary>
    public static QuadTreeSelectMode SelectMode;

    /// <summary>
    /// 最大深度
    /// </summary>
    public static int QuadTreeMaximumDepth = 2046;

    /// <summary>
    /// 节点内容超过就分裂
    /// </summary>
    public static int QuadTreeContentsCountSplit = 20;
}
#pragma warning restore CA2211 // 非常量字段应当不可见