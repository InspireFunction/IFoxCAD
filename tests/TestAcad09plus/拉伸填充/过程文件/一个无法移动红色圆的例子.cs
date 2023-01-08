// 一个无法移动红色圆的例子 https://www.keanw.com/2008/08/rolling-back-th.html
#if true2
namespace JoinBoxAcad;

public class CmdReactor
{
    Document? _doc;
    ObjectIdCollection _ids = new();
    Point3dCollection _pts = new();

    [CommandMethod(nameof(Test_REACTOR))]
    public void Test_REACTOR()
    {
        _doc = Acap.DocumentManager.MdiActiveDocument;
        _doc.CommandWillStart += Doc_CommandWillStart;
    }

    /// <summary>
    /// 挂载一个命令反应器,如果出现了move就挂载一个
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Doc_CommandWillStart(object sender, CommandEventArgs e)
    {
        if (e.GlobalCommandName == "MOVE")
        {
            _ids.Clear();
            _pts.Clear();

            if (_doc is null)
                return;
            _doc.Database.ObjectOpenedForModify += Db_ObjectOpenedForModify;
            _doc.CommandCancelled += Doc_CommandEnded;
            _doc.CommandEnded += Doc_CommandEnded;
            _doc.CommandFailed += Doc_CommandEnded;
        }
    }

    /// <summary>
    /// 卸载一堆反应器
    /// </summary>
    void RemoveEventHandlers()
    {
        if (_doc is null)
            return;
        _doc.CommandCancelled -= Doc_CommandEnded;
        _doc.CommandEnded -= Doc_CommandEnded;
        _doc.CommandFailed -= Doc_CommandEnded;
        _doc.Database.ObjectOpenedForModify -= Db_ObjectOpenedForModify;
    }

    void Doc_CommandEnded(object sender, CommandEventArgs e)
    {
        // 在恢复位置之前删除数据库reactor
        RemoveEventHandlers();
        RollbackLocations();
    }

    /// <summary>
    /// 颜色是1的圆加入集合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Db_ObjectOpenedForModify(object sender, ObjectEventArgs e)
    {
        if (e.DBObject is Circle circle && circle.ColorIndex == 1)// 如果颜色是1
        {
            // 不含有就加入集合
            if (!_ids.Collection.Contains(circle.ObjectId))
            {
                _ids.Add(circle.ObjectId);
                _pts.Add(circle.Center);
            }
        }
    }

    /// <summary>
    /// 修改圆心
    /// </summary>
    void RollbackLocations()
    {
        Debug.Assert(_ids.Count == _pts.Count, "预计相同数量的ID和位置");
        _doc?.Database.Action(tr => {
            int i = 0;
            foreach (ObjectId id in _ids.Collection)
            {
                var circle = tr.GetObject(id, OpenMode.ForWrite) as Circle;
                if (circle is not null)
                    circle.Center = _pts[i++];
            }
        });
    }
}
#endif