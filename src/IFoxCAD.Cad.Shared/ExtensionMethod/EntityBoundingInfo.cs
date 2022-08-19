#define Debug_Cause_Error

namespace IFoxCAD.Cad;

/// <summary>
/// AABB和OBB信息
/// </summary>
public struct BoundingInfo
{
    public double MinX;
    public double MinY;
    public double MinZ;

    public double MaxX;
    public double MaxY;
    public double MaxZ;

    /// <summary>
    /// AABB这里永远是0
    /// </summary>
    public double Angle;
    public bool IsEffective;

    public Point3d Min => new(MinX, MinY, MinZ);
    public Point3d Max => new(MaxX, MaxY, MaxZ);
    public Extents3d Extents3d => new(Min, Max);
    public Extents2d Extents2d => new(MinX, MinY, MaxX, MaxY);

    public BoundingInfo(double minX, double minY, double minZ,
                   double maxX, double maxY, double maxZ,
                   bool isEffective, double angle = 0)
    {
        MinX = minX;
        MinY = minY;
        MinZ = minZ;
        MaxX = maxX;
        MaxY = maxY;
        MaxZ = maxZ;
        IsEffective = isEffective;
        Angle = angle;
    }

    public BoundingInfo(Point3d min, Point3d max, bool isEffective, double angle = 0)
     : this(min.X, min.Y, min.Z,
            max.X, max.Y, max.Z,
            isEffective, angle)
    { }

    public BoundingInfo(Rect rect, double angle = 0)
    {
        MinX = rect.X;
        MinY = rect.Y;
        MaxX = rect.Right;
        MaxY = rect.Top;
        Angle = angle;
    }
}

public class EntityBoundingInfo
{
    #region 保存异常类型的日志
    /// <summary>
    /// 错误信息保存路径
    /// </summary>
    static string? _BoxLogAddress;
    /// <summary>
    /// 包围盒错误文件路径
    /// </summary>
    public static string BoxLogAddress
    {
        get
        {
            _BoxLogAddress ??= LogHelper.GetDefaultOption(nameof(GetBoundingInfo) + ".config");
            return _BoxLogAddress;
        }
        set { _BoxLogAddress = value; }
    }
    static readonly HashSet<string> _typeNames;
    /// <summary>
    /// 
    /// 为了保证更好的性能,
    /// 只是在第一次调用此功能的时候进行读取,
    /// 免得高频调用时候高频触发磁盘
    /// </summary>
    static EntityBoundingInfo()
    {
        _typeNames = new();
        if (!File.Exists(BoxLogAddress))
            return;
        ExceptionToHashSet();
    }

    /// <summary>
    /// 读取日志的异常到容器
    /// </summary>
    static void ExceptionToHashSet()
    {
        var old_LogAddress = LogHelper.LogAddress;
        try
        {
            LogHelper.OptionFile(BoxLogAddress);
            var logTxts = new FileLogger().ReadLog();
            for (int i = 0; i < logTxts.Length; i++)
            {
                var line = logTxts[i];
                if (line.Contains(nameof(LogTxt.备注信息)))
                {
                    int index = line.IndexOf(":");
                    index = line.IndexOf("\"", index) + 1;//1是"\""
                    int index2 = line.IndexOf("\"", index);
                    var msg = line.Substring(index, index2 - index);
                    _typeNames.Add(msg);
                }
            }
        }
        finally
        {
            LogHelper.LogAddress = old_LogAddress;
        }
    }

    /// <summary>
    /// 写入容器类型到异常日志
    /// </summary>
    /// <param name="e"></param>
    /// <param name="ent"></param>
    static void ExceptionToLog(Exception e, Entity ent)
    {
        //无法处理的错误类型将被记录
        //如果错误无法try,而是cad直接致命错误,那么此处也不会被写入,
        //这种情况无法避免程序安全性,总不能写了日志再去删除日志词条,这样会造成频繁IO的
        //遇到一个不认识的类型再去写入?然后记录它是否可以写入?
        var old_LogAddress = LogHelper.LogAddress;
        var old_FlagOutFile = LogHelper.FlagOutFile;
        try
        {
            LogHelper.FlagOutFile = true;
            LogHelper.OptionFile(BoxLogAddress);
            e.WriteLog(ent.GetType().Name, LogTarget.FileNotException);
        }
        finally
        {
            LogHelper.LogAddress = old_LogAddress;
            LogHelper.FlagOutFile = old_FlagOutFile;
        }
    }
    #endregion

    /// <summary>
    /// 获取图元包围盒
    /// </summary>
    /// <param name="ent"></param>
    /// <returns>(左下角,右上角,是否有效)</returns>
    /// 异常:
    ///   会将包围盒类型记录到所属路径中,以此查询
    public static BoundingInfo GetBoundingInfo(Entity ent)
    {
#if Debug_Cause_Error
        //错误类型处理
        if (ent is AttributeDefinition) //属性定义
            return new(Point3d.Origin, Point3d.Origin, false);
        else if (ent is Xline xline)//参照线
            return new(xline.BasePoint, xline.BasePoint, true);
        else if (ent is Ray ray)//射线
            return new(ray.BasePoint, ray.BasePoint, true);
#endif
        //指定类型处理
        if (ent is BlockReference brf)
            return GetBoxInfoInBlockReference(brf);
        else if (ent is MText mText)
            return GetBoxInfoInMText(mText);
        else if (!_typeNames.Contains(ent.GetType().Name)) //屏蔽天正等等缺失包围盒的类型
            try
            {
                return new(ent.GeometricExtents.MinPoint, ent.GeometricExtents.MaxPoint, true);
            }
            catch (Exception e) { ExceptionToLog(e, ent); }
        return new(Point3d.Origin, Point3d.Origin, false);
    }

    /// <summary>
    /// 处理块参照
    /// </summary>
    static BoundingInfo GetBoxInfoInBlockReference(BlockReference brf)
    {
        try
        {
            var fit = brf.GeometryExtentsBestFit();//这个获取是原点附近,需要平移到块基点
            var minX = fit.MinPoint.X + brf.Position.X;
            var minY = fit.MinPoint.Y + brf.Position.Y;
            var minZ = fit.MinPoint.Z + brf.Position.Z;
            var maxX = fit.MaxPoint.X + brf.Position.X;
            var maxY = fit.MaxPoint.Y + brf.Position.Y;
            var maxZ = fit.MaxPoint.Z + brf.Position.Z;
            return new(minX, minY, minZ, maxX, maxY, maxZ, true);
        }
        catch
        {
            //如果是一条参照线的组块,将导致获取包围盒时报错
            //0x01 是否需要进入块内,然后拿到每个图元的BasePoint再计算中点?感觉过于复杂.
            //0x02 这个时候拿基点走就算了
            return new(brf.Position, brf.Position, true);
        }
    }

    /// <summary>
    /// 处理多行文字
    /// </summary>
    static BoundingInfo GetBoxInfoInMText(MText mtxt)
    {
        /*
         * MText Aussehen
         * ------------------------------------
         * |                |                 |
         * |                |                 |ht
         * |                |                 |
         * |-----wl-------插入点------wr-------|
         * |                |                 |
         * |                |                 |hb
         * |                |                 |
         * ------------------------------------
         */

        double width = mtxt.ActualWidth;//实际宽度
        double height = mtxt.ActualHeight;//实际高度
        double wl = 0.0;
        double hb = 0.0;

        //对齐方式
        switch (mtxt.Attachment)
        {
            case AttachmentPoint.TopCenter:
            case AttachmentPoint.MiddleCenter:
            case AttachmentPoint.BottomCenter:
                wl = width * -0.5;
                break;
            case AttachmentPoint.TopRight:
            case AttachmentPoint.MiddleRight:
            case AttachmentPoint.BottomRight:
                wl = -width;
                break;
        }
        switch (mtxt.Attachment)
        {
            case AttachmentPoint.TopLeft:
            case AttachmentPoint.TopCenter:
            case AttachmentPoint.TopRight:
                hb = -height;//下边线到插入点的距离
                break;
            case AttachmentPoint.MiddleLeft:
            case AttachmentPoint.MiddleCenter:
            case AttachmentPoint.MiddleRight:
                hb = height * -0.5;
                break;
        }

        double wr = width + wl;
        double ht = height + hb;

        Point3d center = mtxt.Location;
        Point3d ptMin = new(center.X + wl, center.Y + hb, 0);
        Point3d ptMax = new(center.X + wr, center.Y + ht, 0);

        return new(ptMin, ptMax, true, mtxt.Rotation);
    }
}