namespace TestShared;

public class TestExtents
{
    [CommandMethod(nameof(Test_BlockExtents))]
    public void Test_BlockExtents()
    {
        using var tr = new DBTrans();
        var ent = Env.Editor.GetEntity("pick the entity");
        if (ent.Status != PromptStatus.OK )
        {
            return;
        }

        var block = ent.ObjectId.GetObject<Entity>();
        if (block != null && block.Bounds.HasValue)
        {
            var extent = block.GeometricExtents;
            var pts = new List<Point3d>() {
                extent.MinPoint,
                new Point3d(extent.MinPoint.X,extent.MaxPoint.Y,0),
                extent.MaxPoint,
                new Point3d(extent.MaxPoint.X,extent.MinPoint.Y,0),

            };

            tr.CurrentSpace.AddEntity(pts.CreatePolyline(action: e => e.ColorIndex = 1));
                
            if (block is BlockReference block1)
            {
                var extents = block1.GeometryExtentsBestFit();
                var pts1 = new List<Point3d>()
                {
                    extents.MinPoint,
                    new Point3d(extents.MinPoint.X, extents.MaxPoint.Y, 0),
                    extents.MaxPoint,
                    new Point3d(extents.MaxPoint.X, extents.MinPoint.Y, 0),
                };
                tr.CurrentSpace.AddEntity(pts.CreatePolyline(action: e => e.ColorIndex = 2));

                var extents2 = block1.GetBoundingBoxEx();
                tr.CurrentSpace.AddEntity(pts.CreatePolyline(action: e => e.ColorIndex = 3));
                    
                // 此处是计算块定义的包围盒，不是块参照的，所以一般情况下不需要使用。
                var ext = new Extents3d();
                ext.AddBlockExtents(block1.BlockTableRecord.GetObject<BlockTableRecord>());
                tr.CurrentSpace.AddEntity(ext.CreatePolyline(action: e => e.ColorIndex = 4));
            }

                
        }
    }
    
    [CommandMethod(nameof(Test_entextents))]
    public void Test_entextents()
    {
        using var tr = new DBTrans();
        var a = Env.Editor.GetSelection().Value.
            GetEntities<Entity>(OpenMode.ForWrite);
        foreach (var e in a)
        {
            var b = e.Bounds.HasValue;  //获取是否有包围盒
            var name = e.ObjectId.ObjectClass.DxfName;
            Env.Print($"{name}是否有包围盒-" + b);
            if (b)
            {
                tr.CurrentSpace.AddEntity(e.Bounds!.Value.CreatePolyline(action: e =>
                {
                    e.ColorIndex = 4;
                    e.Closed = true;
                }));
                var ext = e.GetBoundingBoxEx();
                tr.CurrentSpace.AddEntity(ext?.Extents3d.CreatePolyline(action: e =>
                {
                    e.ColorIndex = 5;
                    e.Closed = true;
                }));
                if (e is Curve spline)
                {
                    var ge = spline.GetGeCurve();
                    var box = ge.BoundBlock;
                    var lst = new List<Point3d>()
                    {
                        box.BasePoint,
                        box.BasePoint + box.Direction1,
                        box.BasePoint + box.Direction2,
                        box.BasePoint + box.Direction3,
                    };
                    tr.CurrentSpace.AddEntity(lst.CreatePolyline(action: e =>
                    {
                        e.ColorIndex = 6;
                        e.Closed = true;
                    }));
                }
                
            }
            
            
            
            
        }
    }
}