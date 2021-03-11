using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;

namespace IFoxCAD.Cad
{
    // TODO 命名词典的属性

    public class DBTrans : IDisposable
    {
        #region 私有字段

        private bool disposedValue;

        private readonly bool _commit;

        #endregion

        #region 公开属性
        /// <summary>
        /// 数据库
        /// </summary>
        public Database Database { get; private set; }
        /// <summary>
        /// 文档
        /// </summary>
        public Document Document { get; private set; }
        /// <summary>
        /// 命令行
        /// </summary>
        public Editor Editor { get; private set; }
        /// <summary>
        /// 事务管理器
        /// </summary>
        public Transaction Trans { get; private set; }

        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数，默认事务退出时提交全部操作
        /// </summary>
        /// <param name="commit"></param>
        public DBTrans(bool commit = true)
        {
            Document = Application.DocumentManager.MdiActiveDocument;
            Database = Document.Database;
            Editor = Document.Editor;
            Trans = Database.TransactionManager.StartTransaction();
            _commit = commit;
        }

        #endregion

        #region 符号表

        /// <summary>
        /// 块表
        /// </summary>
        public SymbolTable<BlockTable, BlockTableRecord> BlockTable => new(this, Database.BlockTableId);

        public SymbolTable<BlockTable, BlockTableRecord> CurrentSpace => new(this, Database.CurrentSpaceId);
        /// <summary>
        /// 层表
        /// </summary>
        public SymbolTable<LayerTable, LayerTableRecord> LayerTable => new(this, Database.LayerTableId);
        /// <summary>
        /// 文字样式表
        /// </summary>
        public SymbolTable<TextStyleTable, TextStyleTableRecord> TextStyleTable => new(this, Database.TextStyleTableId);

        /// <summary>
        /// 注册应用程序表
        /// </summary>
        public SymbolTable<RegAppTable, RegAppTableRecord> RegAppTable => new(this, Database.RegAppTableId);

        /// <summary>
        /// 标注样式表
        /// </summary>
        public SymbolTable<DimStyleTable, DimStyleTableRecord> DimStyleTable => new(this, Database.DimStyleTableId);

        /// <summary>
        /// 线型表
        /// </summary>
        public SymbolTable<LinetypeTable, LinetypeTableRecord> LinetypeTable => new(this, Database.LinetypeTableId);

        /// <summary>
        /// 用户坐标系表
        /// </summary>
        public SymbolTable<UcsTable, UcsTableRecord> UcsTable => new(this, Database.UcsTableId);

        /// <summary>
        /// 视图表
        /// </summary>
        public SymbolTable<ViewTable, ViewTableRecord> ViewTable => new(this, Database.ViewTableId);

        /// <summary>
        /// 视口表
        /// </summary>
        public SymbolTable<ViewportTable, ViewportTableRecord> ViewportTable => new(this, Database.ViewportTableId);
        #endregion

        #region 获取对象
        /// <summary>
        /// 根据对象id获取图元对象
        /// </summary>
        /// <typeparam name="T">要获取的图元对象的类型</typeparam>
        /// <param name="id">对象id</param>
        /// <param name="mode">打开模式，默认为只读</param>
        /// <param name="openErased">是否打开已删除对象，默认为不打开</param>
        /// <param name="forceOpenOnLockedLayer">是否打开锁定图层对象，默认为不打开</param>
        /// <returns>图元对象，类型不匹配时返回 <see langword="null"/> </returns>
        public T GetObject<T>(ObjectId id,
                              OpenMode mode = OpenMode.ForRead,
                              bool openErased = false,
                              bool forceOpenOnLockedLayer = false) where T : DBObject
        {
            return Trans.GetObject(id, mode, openErased, forceOpenOnLockedLayer) as T;
        }


        #endregion

        #region 添加实体
        /// <summary>
        /// 添加实体到块表记录
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="btr">块表记录， 默认为当前空间</param>
        /// <returns>实体对象id</returns>
        public ObjectId AddEntity(Entity entity, BlockTableRecord btr = null)
        {
            btr ??= BlockTable.GetRecord(Database.CurrentSpaceId);
            return btr.AddEntity(Trans, entity);
        }
        /// <summary>
        /// 添加实体集合到块表记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="ents">实体集合</param>
        /// <param name="btr">块表记录， 默认为当前空间</param>
        /// <returns>实体对象id集合</returns>
        public List<ObjectId> AddEntity<T>(IEnumerable<T> ents, BlockTableRecord btr = null) where T : Entity
        {
            btr ??= BlockTable.GetRecord(Database.CurrentSpaceId);
            return btr.AddEntity(Trans, ents);
        }
        #endregion



        
        #region 插入块参照
     
        /// <summary>
        /// 插入块参照
        /// </summary>
        /// <param name="position">插入点</param>
        /// <param name="blockName">块名</param>
        /// <param name="scale">块插入比例，默认为0</param>
        /// <param name="rotation">块插入旋转角(弧度)，默认为0</param>
        /// <param name="atts">属性字典{Tag,Value}，默认为null</param>
        /// <returns>块参照对象id</returns>
        public ObjectId InsertBlock(Point3d position,
                                    string blockName,
                                    Scale3d scale = default,
                                    double rotation = default,
                                    Dictionary<string, string> atts = default)
        {
            if (!BlockTable.Has(blockName))
            {
                Editor.WriteMessage($"\n不存在名字为{blockName}的块定义。");
                return ObjectId.Null;
            }
            return InsertBlock(position, BlockTable[blockName], scale, rotation, atts);
        }
        /// <summary>
        /// 插入块参照
        /// </summary>
        /// <param name="position">插入点</param>
        /// <param name="blockId">块定义id</param>
        /// <param name="scale">块插入比例，默认为0</param>
        /// <param name="rotation">块插入旋转角(弧度)，默认为0</param>
        /// <param name="atts">属性字典{Tag,Value}，默认为null</param>
        /// <returns>块参照对象id</returns>
        public ObjectId InsertBlock(Point3d position,
                                    ObjectId blockId,
                                    Scale3d scale = default,
                                    double rotation = default,
                                    Dictionary<string, string> atts = default)
        {
            if (!BlockTable.Has(blockId))
            {
                Editor.WriteMessage($"\n不存在名字为{GetObject<BlockTableRecord>(blockId).Name}的块定义。");
                return ObjectId.Null;
            }
            using var blockref = new BlockReference(position, blockId)
            {
                ScaleFactors = scale,
                Rotation = rotation
            };
            var objid = AddEntity(blockref);
            if (atts != default)
            {
                var btr = GetObject<BlockTableRecord>(blockref.BlockTableRecord);
                if (btr.HasAttributeDefinitions)
                {
                    var attdefs = btr
                        .GetEntities<AttributeDefinition>(Trans)
                        .Where(attdef => !(attdef.Constant || attdef.Invisible));
                    foreach (var attdef in attdefs)
                    {
                        using AttributeReference attref = new();
                        attref.SetAttributeFromBlock(attdef, blockref.BlockTransform);
                        attref.Position = attdef.Position.TransformBy(blockref.BlockTransform);
                        attref.AdjustAlignment(Database);
                        if (atts.ContainsKey(attdef.Tag))
                        {
                            attref.TextString = atts[attdef.Tag];
                        }

                        blockref.AttributeCollection.AppendAttribute(attref);
                        Trans.AddNewlyCreatedDBObject(attref, true);
                    }
                }
            }
            return objid;
        }

        #endregion

        #region 实体刷新
        /// <summary>
        /// 刷新实体显示
        /// </summary>
        /// <param name="entity">实体对象</param>
        public void Flush(Entity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            entity.RecordGraphicsModified(true);
            Trans.TransactionManager.QueueForGraphicsFlush();
        }

        /// <summary>
        /// 刷新实体显示
        /// </summary>
        /// <param name="id">实体id</param>
        public void Flush(ObjectId id) => Flush(GetObject<Entity>(id));
        #endregion

        #region idispose接口相关函数

        public void Abort()
        {
            Trans.Abort();
        }

        public void Commit()
        {
            if (_commit)
            {
                Trans.Commit();
            }
            else
            {
                Abort();
            }
            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // 释放托管状态(托管对象)
                    Commit();
                    Trans.Dispose();

                }

                // 释放未托管的资源(未托管的对象)并替代终结器
                // 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        ~DBTrans()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
