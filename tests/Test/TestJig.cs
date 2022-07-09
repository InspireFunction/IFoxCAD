using System.Windows.Forms;

namespace Test;

public class Commands_Jig
{
    //已在数据库的图元如何进入jig
    [CommandMethod("TestCmd_jig33")]
    public static void TestCmd_jig33()
    {
        Circle cir;
        using var tr = new DBTrans();
        var per = tr.Editor.GetEntity("\n点选圆形:");
        if (per.Status != PromptStatus.OK)
            return;
        cir = tr.GetObject<Circle>(per.ObjectId, OpenMode.ForWrite);

        JigEx moveJig = null;
        moveJig = new JigEx((mousePoint, drawEntitys) => {
            //cir.UpgradeOpen();//已经提权了,所以这里不需要提权
            cir.Move(cir.StartPoint, mousePoint);
            //cir.DowngradeOpen();

            //此处会Dispose图元,
            //所以此处不加入已经在数据库的图元,而是加入new Entity的.
            //drawEntitys.Enqueue(cir);
        });
        moveJig.SetOptions(cir.GeometricExtents.MinPoint);

        //此处不会Dispose图元,
        //0x01 此处加入已经在数据的图元
        //0x02 加入不刷新的图元,例如亮显会被刷新冲刷掉
        moveJig.WorldDrawEvent += draw => {
            draw.RawGeometry.Draw(cir);
        };

        while (true)
        {
            var prDrag = moveJig.Drag();
            if (prDrag.Status == PromptStatus.OK)
                break;
        }
    }


    //不在数据库的图元如何进入jig
    [CommandMethod("TestCmd_Jig44")]
    public void TestCmd_Jig44()
    {
        using var tr = new DBTrans();
        var per = Env.Editor.GetEntity("\n请选择一条多段线:");
        if (per.Status != PromptStatus.OK)
            return;
        var ent = tr.GetObject<Entity>(per.ObjectId, OpenMode.ForWrite);
        if (ent is not Polyline pl)
            return;

        /*
         * 鼠标采样器执行时修改鼠标基点
         * 原因: 多段线与鼠标垂直点作为 BasePoint ,jig鼠标点为确定点
         * 所以需要先声明再传入指针,但是我发现null也可以.
         */
        JigEx jig = null;
        jig = new JigEx((mousePoint, drawEntitys) => {
            var closestPt = pl.GetClosestPointTo(mousePoint, false);

            var sop = jig.SetOptions(closestPt);
            sop.Keywords.Add("A");
            sop.Keywords.Add(" ");/*这里是无效的,因为jig.SetOptions()内部设置,但是这里设置了会显示,最好注释掉*/

            //生成文字
            var dictString = (pl.GetDistAtPoint(closestPt) * 0.001).ToString("0.00");
            var acText = new TextInfo(dictString, closestPt, AttachmentPoint.BaseLeft, textHeight: 200)
                        .AddDBTextToEntity();

            //加入刷新队列
            drawEntitys.Enqueue(acText);
        });
        jig.SetOptions(per.PickedPoint);

        bool flag = true;
        while (flag)
        {
            var pr = jig.Drag();
            if (pr.Status == PromptStatus.Keyword)
            {
                switch (pr.StringResult)
                {
                    case "A":
                        tr.Editor.WriteMessage($"\n 您触发了关键字{pr.StringResult}");
                        flag = false;
                        break;
                    case " ":
                        tr.Editor.WriteMessage("\n 此句永远不会执行,另见: jig.SetOptions()的 JigPointOptions()内注释");
                        flag = false;
                        break;
                }
            }
            else if (pr.Status != PromptStatus.OK)//右键,空格,回车,都在这里结束
                return;
            else
                flag = false;
        }
        tr.CurrentSpace.AddEntity(jig.Entitys);
    }

    [CommandMethod("TestCmd_loop")]
    public void TestCmd_loop()
    {
        DocumentCollection dm =
          Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
        Editor ed = dm.MdiActiveDocument.Editor;
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
                {
                    bCanceled = true;
                }
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
        Document doc =
          Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        Editor ed = doc.Editor;
        PromptStringOptions pso = new("\nEnter text string")
        {
            AllowSpaces = true
        };
        var pr = ed.GetString(pso);
        if (pr.Status != PromptStatus.OK)
            return;
        Transaction tr =
          doc.TransactionManager.StartTransaction();
        using (tr)
        {
            BlockTableRecord btr =
              (BlockTableRecord)tr.GetObject(
                db.CurrentSpaceId, OpenMode.ForWrite
              );
            // Create the text object, set its normal and contents

            var acText = new TextInfo(pr.StringResult, 
                             Point3d.Origin, 
                             AttachmentPoint.BaseLeft, textHeight: 200)
                             .AddDBTextToEntity();
          
            acText.Normal = ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis;
            btr.AppendEntity(acText);
            tr.AddNewlyCreatedDBObject(acText, true);

            // Create our jig
            TextPlacementJig pj = new(tr, db, acText);
            // Loop as we run our jig, as we may have keywords
            PromptStatus stat = PromptStatus.Keyword;
            while (stat == PromptStatus.Keyword)
            {
                PromptResult res = ed.Drag(pj);
                stat = res.Status;
                if (
                  stat != PromptStatus.OK &&
                  stat != PromptStatus.Keyword
                )
                    return;
            }
            tr.Commit();
        }
    }
    class TextPlacementJig : EntityJig
    {
        // Declare some internal state
        readonly Database _db;
        readonly Transaction _tr;
        Point3d _position;
        double _angle, _txtSize;
        // Constructor
        public TextPlacementJig(
          Transaction tr, Database db, Entity ent
        ) : base(ent)
        {
            _db = db;
            _tr = tr;
            _angle = 0;
            _txtSize = 1;
        }
        protected override SamplerStatus Sampler(
          JigPrompts jp
        )
        {
            // We acquire a point but with keywords
            JigPromptPointOptions po =
              new(
                "\nPosition of text"
              );
            po.UserInputControls =
              (UserInputControls.Accept3dCoordinates |
                UserInputControls.NullResponseAccepted |
                UserInputControls.NoNegativeResponseAccepted |
                UserInputControls.GovernedByOrthoMode);
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
                        {
                            break;
                        }
                    case "Italic":
                        {
                            break;
                        }
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
                        {
                            break;
                        }
                    case "RIght":
                        {
                            break;
                        }
                    case "Middle":
                        {
                            break;
                        }
                }
                return SamplerStatus.OK;
            }
            else if (ppr.Status == PromptStatus.OK)
            {
                // Check if it has changed or not (reduces flicker)
                if (
                  _position.DistanceTo(ppr.Value) <
                    Tolerance.Global.EqualPoint
                )
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
}
