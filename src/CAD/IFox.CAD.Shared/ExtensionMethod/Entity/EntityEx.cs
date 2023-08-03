namespace IFoxCAD.Cad;


/// <summary>
/// 实体图元扩展类
/// </summary>
public static class EntityEx
{


    #region 实体线性变换

    /// <summary>
    /// 移动实体
    /// </summary>
    /// <param name="ent">实体</param>
    /// <param name="from">基点</param>
    /// <param name="to">目标点</param>
    public static void Move(this Entity ent, Point3d from, Point3d to)
    {
        using (ent.ForWrite())
        {
            ent.TransformBy(Matrix3d.Displacement(to - from));
        }
    }
    /// <summary>
    /// 移动实体
    /// </summary>
    /// <param name="ent">实体</param>
    /// <param name="vector">向量</param>
    public static void Move(this Entity ent, Vector3d vector)
    {
        using (ent.ForWrite())
        {
            ent.TransformBy(Matrix3d.Displacement(vector));
        }
    }

    /// <summary>
    /// 缩放实体
    /// </summary>
    /// <param name="ent">实体</param>
    /// <param name="center">缩放基点坐标</param>
    /// <param name="scaleValue">缩放比例</param>
    public static void Scale(this Entity ent, Point3d center, double scaleValue)
    {
        using (ent.ForWrite())
        {
            ent.TransformBy(Matrix3d.Scaling(scaleValue, center));
        }
    }

    /// <summary>
    /// 旋转实体
    /// </summary>
    /// <param name="ent">实体</param>
    /// <param name="center">旋转中心</param>
    /// <param name="angle">转角，弧度制，正数为顺时针</param>
    /// <param name="normal">旋转平面的法向矢量</param>
    public static void Rotation(this Entity ent, Point3d center, double angle, Vector3d normal)
    {
        using (ent.ForWrite())
        {
            ent.TransformBy(Matrix3d.Rotation(angle, normal, center));
        }
    }

    /// <summary>
    /// 在XY平面内旋转实体
    /// </summary>
    /// <param name="ent">实体</param>
    /// <param name="center">旋转中心</param>
    /// <param name="angle">转角，弧度制，正数为顺时针</param>
    public static void Rotation(this Entity ent, Point3d center, double angle)
    {
        using (ent.ForWrite())
        {
            ent.TransformBy(Matrix3d.Rotation(angle, Vector3d.ZAxis.TransformBy(ent.Ecs), center));
        }
    }

    /// <summary>
    /// 按对称轴镜像实体
    /// </summary>
    /// <param name="ent">实体</param>
    /// <param name="startPoint">对称轴起点</param>
    /// <param name="endPoint">对称轴终点</param>
    public static void Mirror(this Entity ent, Point3d startPoint, Point3d endPoint)
    {
        using (ent.ForWrite())
        {
            ent.TransformBy(Matrix3d.Mirroring(new Line3d(startPoint, endPoint)));
        }
    }

    /// <summary>
    /// 按对称面镜像实体
    /// </summary>
    /// <param name="ent">实体</param>
    /// <param name="plane">对称平面</param>
    public static void Mirror(this Entity ent, Plane plane)
    {
        using (ent.ForWrite())
        {
            ent.TransformBy(Matrix3d.Mirroring(plane));
        }
    }

    /// <summary>
    /// 按对称点镜像实体
    /// </summary>
    /// <param name="ent">实体</param>
    /// <param name="basePoint">对称点</param>
    public static void Mirror(this Entity ent, Point3d basePoint)
    {
        using (ent.ForWrite())
        {
            ent.TransformBy(Matrix3d.Mirroring(basePoint));
        }
    }

    #endregion

    #region 实体范围
    /// <summary>
    /// 获取实体集合的范围
    /// </summary>
    /// <param name="ents">实体迭代器</param>
    /// <returns>实体集合的范围</returns>
    public static Extents3d GetExtents(this IEnumerable<Entity> ents)
    {
        var ext = new Extents3d();
        foreach (var item in ents)
        {
            if (item.Bounds.HasValue)
                ext.AddExtents(item.GeometricExtents);
        }
        return ext;
    }
    #endregion



    /// <summary>
    /// 获取图元包围盒
    /// </summary>
    /// <param name="ent"></param>
    /// <returns>包围盒信息</returns>
    /// 异常:
    ///   会将包围盒类型记录到所属路径中,以此查询
    public static BoundingInfo? GetBoundingBoxEx(this Entity ent)
    {
        return EntityBoundingInfo.GetBoundingInfo(ent);
    }
    /// <summary>
    /// 获取拉伸点
    /// </summary>
    /// <param name="ent">实体</param>
    /// <returns>点集</returns>
    public static IEnumerable<Point3d> GetStretchPoints(this Entity ent)
    {
        var p3dc = new Point3dCollection(); 
        ent.GetStretchPoints(p3dc);
        return p3dc.Cast<Point3d>();
    }
}