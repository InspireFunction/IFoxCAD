namespace IFoxCAD.Cad
{
    //因为我想用字段...所以从接口改成了类
    /// <summary>
    /// 约束传入的对象都要含有包围盒的定义
    /// </summary>
    public class IHasRect: Rect
    {
        /// <summary>
        /// 颜色
        /// </summary>
        //public System.Drawing.Color Color;

        /// <summary>
        /// 是一个点
        /// </summary>
        public bool IsPoint;
    }
}