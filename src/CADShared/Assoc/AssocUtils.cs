namespace IFoxCAD.Cad.Assoc;

/// <summary>
/// 关联动作辅助类
/// </summary>
public static class AssocUtils
{
    /// <summary>
    /// 创建关系动作并提交到数据库<br/>
    /// 替代AssocActionBody.CreateActionAndActionBodyAndPostToDatabase();函数
    /// </summary>
    /// <param name="actionBodyClass">actionBody的RXClass</param>
    /// <param name="ownerId">拥有者Id</param>
    /// <param name="actionId">动作id</param>
    /// <param name="actionBodyId">动作bodyId</param>
    /// <returns>错误信息</returns>
    public static ErrorStatus CreateActionAndActionBodyAndPostToDatabase(RXClass actionBodyClass,
        ObjectId ownerId, out ObjectId actionId, out ObjectId actionBodyId)
    {
        actionId = actionBodyId = ObjectId.Null;
        try
        {
            if (!actionBodyClass.IsDerivedFrom(RXObject.GetClass(typeof(AssocActionBody))) ||
                Activator.CreateInstance(actionBodyClass.GetRuntimeType()) is not AssocActionBody
                    actionBody)
                return ErrorStatus.NotThatKindOfClass;
            var db = ownerId.Database;
            using var tr = new DBTrans(db);
            var networkId = AssocNetwork.GetInstanceFromObject(ownerId, true, true, "");
            var network = (AssocNetwork)tr.GetObject(networkId, OpenMode.ForWrite);
            actionBodyId = db.AddDBObject(actionBody);
            tr.Transaction.AddNewlyCreatedDBObject(actionBody, true);
            using var action = new AssocAction();
            action.ActionBody = actionBodyId;
            actionId = db.AddDBObject(action);
            network.AddAction(actionId, true);
            return ErrorStatus.OK;
        }
        catch (AcException e)
        {
            return e.ErrorStatus;
        }
        catch (Exception)
        {
            return ErrorStatus.InternetUnknownError;
        }
    }
}