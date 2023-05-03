#if !ac2008
namespace GripOverruleTest;

// https://through-the-interface.typepad.com/through_the_interface/2009/08/knowing-when-an-autocad-object-is-grip-edited-using-overrules-in-net.html
public class GripVectorOverrule : GripOverrule
{
    // A static pointer to our overrule instance
    static public GripVectorOverrule theOverrule = new();
    // A flag to indicate whether we're overruling
    static bool overruling = false;
    // A single set of grips would not have worked in
    // the case where multiple objects were selected.
    static Dictionary<string, Point3dCollection> _gripDict = new();

    private string GetKey(Entity e)
    {
        // Generate a key based on the name of the object's type
        // and its geometric extents
        // (We cannot use the ObjectId, as this is null during
        // grip-stretch operations.)
        return e.GetType().Name + ":" + e.GeometricExtents.ToString();
    }
    // Save the locations of the grips for a particular entity
    private void StoreGripInfo(Entity e, Point3dCollection grips)
    {
        string key = GetKey(e);
        if (_gripDict.ContainsKey(key))
        {
            // Clear the grips if any already associated
            Point3dCollection grps = _gripDict[key];
            using (grps)
                grps.Clear();
            _gripDict.Remove(key);
        }
        // Now we add our grips
        Point3d[] pts = new Point3d[grips.Count];
        grips.CopyTo(pts, 0);
        Point3dCollection gps = new(pts);
        _gripDict.Add(key, gps);
    }
    // Get the locations of the grips for an entity
    private Point3dCollection? RetrieveGripInfo(Entity e)
    {
        Point3dCollection? grips = null;
        string key = GetKey(e);
        if (_gripDict.ContainsKey(key))
            grips = _gripDict[key];
        return grips;
    }
    public override void GetGripPoints(Entity e, Point3dCollection grips, IntegerCollection snaps, IntegerCollection geomIds)
    {
        base.GetGripPoints(e, grips, snaps, geomIds);
        StoreGripInfo(e, grips);
    }
    public override void MoveGripPointsAt(Entity e, IntegerCollection indices, Vector3d offset)
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;

        var grips = RetrieveGripInfo(e);
        if (grips != null)
        {
            // Could get multiple points moved at once,
            // hence the integer collection
            foreach (int i in indices)
            {
                // Get the grip point from our internal state
                Point3d pt = grips[i];
                // Draw a vector from the grip point to the newly
                // offset location, using the index into the
                // grip array as the color (excluding colours 0 and 7).
                // These vectors don't getting cleared, which makes
                // for a fun effect.
                ed.DrawVector(
                  pt,
                  pt + offset,
                  (i >= 6 ? i + 2 : i + 1), // exclude colours 0 and 7
                  false
                );
            }
        }
        base.MoveGripPointsAt(e, indices, offset);
    }
    [CommandMethod(nameof(GripOverruleOnOff))]
    public static void GripOverruleOnOff()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        if (overruling)
            RemoveOverrule(GetClass(typeof(Entity)), theOverrule);
        else
            AddOverrule(GetClass(typeof(Entity)), theOverrule, true);
        overruling = !overruling;
        Overruling = overruling;
        ed.WriteMessage("\nGrip overruling turned {0}.", overruling ? "on" : "off");
    }
}
#endif