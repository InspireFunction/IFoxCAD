

namespace TestShared
{
    public static class TestDBobject
    {
        [CommandMethod(nameof(TestForWrite))]
        public static void TestForWrite()
        {
            using var tr = new DBTrans();
            var ent = Env.Editor.GetEntity("\npick entity");
            if (ent.Status is not PromptStatus.OK) return;
            var entid = ent.ObjectId.GetObject<Entity>()!;
            Tools.TestTimes3(100000, "using:", i => {
                using (entid.ForWrite())
                {
                    entid.ColorIndex = i % 7;
                }
            });
            Tools.TestTimes3(100000, "action:", i => {
                entid.ForWrite(e => {
                    e.ColorIndex = i % 7;
                });
            });
        }
    }
}
