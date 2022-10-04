namespace Test;

using System.Windows.Forms;

public class Commands_Jig
{
    // 已在数据库的图元如何进入jig
    [CommandMethod("TestCmd_jig33")]
    public static void TestCmd_jig33()
    {
        using DBTrans tr = new();
        var per = tr.Editor?.GetEntity("\n点选圆形:");
        if (per?.Status != PromptStatus.OK)
            return;
        var cir = tr.GetObject<Circle>(per.ObjectId, OpenMode.ForWrite);
        if (cir == null)
            return;
        var oldSp = cir.StartPoint;
        JigEx? moveJig = null;
        moveJig = new JigEx((mousePoint, drawEntitys) => {
            moveJig!.SetOptions(oldSp);// 回调过程中也可以修改基点
            // cir.UpgradeOpen();// 已经提权了,所以这里不需要提权
            cir.Move(cir.StartPoint, mousePoint);
            // cir.DowngradeOpen();

            // 此处会Dispose图元,
            // 所以此处不加入已经在数据库的图元,而是加入new Entity的.
            // drawEntitys.Enqueue(cir);
        });
        moveJig.SetOptions(cir.GeometricExtents.MinPoint, orthomode: true);

        // 此处详见方法注释
        moveJig.DatabaseEntityDraw(draw => {
            draw.RawGeometry.Draw(cir);
        });

        while (true)
        {
            var prDrag = moveJig.Drag();
            if (prDrag.Status == PromptStatus.OK)
                break;
        }
        moveJig.Dispose();
    }


    // 不在数据库的图元如何进入jig
    [CommandMethod("TestCmd_Jig44")]
    public void TestCmd_Jig44()
    {
        using DBTrans tr = new();
        var per = Env.Editor.GetEntity("\n请选择一条多段线:");
        if (per.Status != PromptStatus.OK)
            return;
        var ent = tr.GetObject<Entity>(per.ObjectId, OpenMode.ForWrite);
        if (ent is not Polyline pl)
            return;

        /*
         * 鼠标采样器执行时修改鼠标基点
         * 原因: 多段线与鼠标垂直点作为 BasePoint,jig鼠标点为确定点
         * 所以需要先声明再传入指针,但是我发现null也可以.
         */
        JigPromptPointOptions? options = null;
        using var jig = new JigEx((mousePoint, drawEntitys) => {
            var closestPt = pl.GetClosestPointTo(mousePoint, false);

            // 回调过程中SetOptions会覆盖配置,所以如果想增加关键字或者修改基点,
            // 不要这样做: jig.SetOptions(closestPt) 而是使用底层暴露
            options!.BasePoint = closestPt;

            // 需要避免重复加入同一个关键字
            if (!options.Keywords.Contains("A"))
                options.Keywords.Add("A");

            // 生成文字
            var dictString = (pl.GetDistAtPoint(closestPt) * 0.001).ToString("0.00");
            var acText = new TextInfo(dictString, closestPt, AttachmentPoint.BaseLeft, textHeight: 200)
                        .AddDBTextToEntity();

            // 加入刷新队列
            drawEntitys.Enqueue(acText);
        });

        options = jig.SetOptions(per.PickedPoint);

        // 如果没有这个,那么空格只会是 PromptStatus.None 而不是 PromptStatus.Keyword
        // options.Keywords.Add(" ", " ", "空格结束啊");
        // jig.SetSpaceIsKeyword();

        bool flag = true;
        while (flag)
        {
            var pr = jig.Drag();
            if (pr.Status == PromptStatus.Keyword)
            {
                switch (pr.StringResult)
                {
                    case "A":
                    tr.Editor?.WriteMessage($"\n 您触发了关键字{pr.StringResult}");
                    flag = false;
                    break;
                    case " ":
                    tr.Editor?.WriteMessage("\n 触发关键字空格");
                    flag = false;
                    break;
                }
            }
            else if (pr.Status != PromptStatus.OK)// PromptStatus.None == 右键,空格,回车,都在这里结束
            {
                tr.Editor?.WriteMessage(Environment.NewLine + pr.Status.ToString());
                return;
            }
            else
                flag = false;
        }
        tr.CurrentSpace.AddEntity(jig.Entitys);
    }

    [CommandMethod("TestCmd_loop")]
    public void TestCmd_loop()
    {
        var dm = Acap.DocumentManager;
        var ed = dm.MdiActiveDocument.Editor;

        // Create and add our message filter
        MyMessageFilter filter = new();
        System.Windows.Forms.Application.AddMessageFilter(filter);
        // Start the loop
        while (true)
        {
            // Check for user input events
            System.Windows.Forms.Application.DoEvents();
            // Check whether the filter has set the flag
            if (filter.bCanceled == true)
            {
                ed.WriteMessage("\nLoop cancelled.");
                break;
            }
            ed.WriteMessage($"\nInside while loop...and {filter.Key}");
        }
        // We're done - remove the message filter
        System.Windows.Forms.Application.RemoveMessageFilter(filter);
    }
    // Our message filter class
    public class MyMessageFilter : IMessageFilter
    {
        public const int WM_KEYDOWN = 0x0100;
        public bool bCanceled = false;
        public Keys Key { get; private set; }
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_KEYDOWN)
            {
                // Check for the Escape keypress
                Keys kc = (Keys)(int)m.WParam & Keys.KeyCode;
                if (m.Msg == WM_KEYDOWN && kc == Keys.Escape)
                    bCanceled = true;
                Key = kc;
                // Return true to filter all keypresses
                return true;
            }
            // Return false to let other messages through
            return false;
        }
    }


    [CommandMethod("TestCmd_QuickText")]
    static public void TestCmd_QuickText()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        PromptStringOptions pso = new("\nEnter text string")
        {
            AllowSpaces = true
        };
        var pr = ed.GetString(pso);
        if (pr.Status != PromptStatus.OK)
            return;

        using var tr = doc.TransactionManager.StartTransaction();
        var btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
        // Create the text object, set its normal and contents

        var acText = new TextInfo(pr.StringResult,
                         Point3d.Origin,
                         AttachmentPoint.BaseLeft, textHeight: 200)
                         .AddDBTextToEntity();

        acText.Normal = ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis;
        btr.AppendEntity(acText);
        tr.AddNewlyCreatedDBObject(acText, true);

        // Create our jig
        var pj = new TextPlacementJig(tr, db, acText);
        // Loop as we run our jig, as we may have keywords
        var state = PromptStatus.Keyword;
        while (state == PromptStatus.Keyword)
        {
            var res = ed.Drag(pj);
            state = res.Status;
            if (state != PromptStatus.OK && state != PromptStatus.Keyword)
                return;
        }
        tr.Commit();
    }

#if true
    class TextPlacementJig : EntityJig
    {
        // Declare some internal state
        readonly Database _db;
        readonly Transaction _tr;

        Point3d _position;
        double _angle, _txtSize;

        // Constructor
        public TextPlacementJig(Transaction tr, Database db, Entity ent) : base(ent)
        {
            _db = db;
            _tr = tr;
            _angle = 0;
            _txtSize = 1;
        }

        protected override SamplerStatus Sampler(JigPrompts jp)
        {
            // We acquire a point but with keywords
            JigPromptPointOptions po = new("\nPosition of text")
            {
                UserInputControls =
                UserInputControls.Accept3dCoordinates |
                UserInputControls.NullResponseAccepted |
                UserInputControls.NoNegativeResponseAccepted |
                UserInputControls.GovernedByOrthoMode
            };
            po.SetMessageAndKeywords(
              "\nSpecify position of text or " +
              "[Bold/Italic/LArger/Smaller/" +
               "ROtate90/LEft/Middle/RIght]: ",
              "Bold Italic LArger Smaller " +
              "ROtate90 LEft Middle RIght"
            );

            PromptPointResult ppr = jp.AcquirePoint(po);
            if (ppr.Status == PromptStatus.Keyword)
            {
                switch (ppr.StringResult)
                {
                    case "Bold":
                    break;
                    case "Italic":
                    break;
                    case "LArger":
                    {
                        // Multiple the text size by two
                        _txtSize *= 2;
                        break;
                    }
                    case "Smaller":
                    {
                        // Divide the text size by two
                        _txtSize /= 2;
                        break;
                    }
                    case "ROtate90":
                    {
                        // To rotate clockwise we subtract 90 degrees and
                        // then normalise the angle between 0 and 360
                        _angle -= Math.PI / 2;
                        while (_angle < Math.PI * 2)
                        {
                            _angle += Math.PI * 2;
                        }
                        break;
                    }
                    case "LEft":
                    break;
                    case "RIght":
                    break;
                    case "Middle":
                    break;
                }
                return SamplerStatus.OK;
            }
            else if (ppr.Status == PromptStatus.OK)
            {
                // Check if it has changed or not (reduces flicker)
                if (_position.DistanceTo(ppr.Value) < Tolerance.Global.EqualPoint)
                    return SamplerStatus.NoChange;

                _position = ppr.Value;
                return SamplerStatus.OK;
            }
            return SamplerStatus.Cancel;
        }

        protected override bool Update()
        {
            // Set properties on our text object
            DBText txt = (DBText)Entity;
            txt.Position = _position;
            txt.Height = _txtSize;
            txt.Rotation = _angle;
            return true;
        }
    }
#endif
}