

namespace TestShared
{
    public class TestExtents
    {
        [CommandMethod(nameof(TestExtents))]
        public void TestBlockExtents()
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
                    var pts1 = new List<Point3d>() {
                    extents.MinPoint,
                    new Point3d(extents.MinPoint.X,extents.MaxPoint.Y,0),
                    extents.MaxPoint,
                    new Point3d(extents.MaxPoint.X,extents.MinPoint.Y,0),};
                    tr.CurrentSpace.AddEntity(pts.CreatePolyline(action: e => e.ColorIndex = 2));

                    var extents2 = block1.GetBoundingBoxEx();
                    tr.CurrentSpace.AddEntity(pts.CreatePolyline(action: e => e.ColorIndex = 3));
                }

                
            }
        }
    }
}
