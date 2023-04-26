namespace IFoxCAD.Cad;

/// <summary>
/// 瞬态容器
/// </summary>
public class JigExTransient : IDisposable
{
    #region 私有字段
    // 整数集,暂时不知道有什么意义
    readonly IntegerCollection _integerCollection;
    // 维护集合
    readonly  HashSet<Entity> _entities;
    readonly TransientManager _manager;
    #endregion

    #region 公开属性
    /// <summary>
    /// 对象集合
    /// </summary>
    public Entity[] Entities => _entities.ToArray();
    /// <summary>
    /// 数量
    /// </summary>
    public int Count => _entities.Count;
    #endregion

    #region 构造函数
    /// <summary>
    /// 瞬态容器
    /// </summary>
    public JigExTransient()
    {
        _integerCollection = new();
        _entities = new(); 
        _manager=TransientManager.CurrentTransientManager;
    }
    #endregion

    #region 方法
    /// <summary>
    /// 判断瞬态容器里是否含有对象
    /// </summary>
    /// <param name="ent">对象</param>
    /// <returns>含有返回true</returns>
    public bool Contains(Entity ent)
    {
        return _entities.Contains(ent);
    }

    /// <summary>
    /// 向瞬态容器中添加对象
    /// </summary>
    /// <param name="ent">图元</param>
    /// <param name="tdm">绘图模式</param>
    public void Add(Entity ent, TransientDrawingMode tdm = TransientDrawingMode.Main)
    {
        if (_entities.Add(ent))
        {
            _manager.AddTransient(ent, tdm, 128, _integerCollection);
        }
    }


    /// <summary>
    /// 从瞬态容器中移除图元
    /// </summary>
    /// <param name="ent">已经加入瞬态容器的图元</param>
    public void Remove(Entity ent)
    {
        if (!Contains(ent))
            return;
        _manager.EraseTransient(ent, _integerCollection);
        _entities.Remove(ent);
    }

    /// <summary>
    /// 清空瞬态容器并移除图元显示
    /// </summary>
    public void Clear()
    {
        foreach (var ent in _entities)
        {
            _manager.EraseTransient(ent, _integerCollection);
        }
        _entities.Clear();
    }


    /// <summary>
    /// 更新单个显示
    /// </summary>
    /// <param name="ent">已经加入瞬态容器的图元</param>
    public void Update(Entity ent)
    {
        if (!Contains(ent))
            return;
        _manager.UpdateTransient(ent, _integerCollection);
    }

    /// <summary>
    /// 更新全部显示
    /// </summary>
    public void UpdateAll()
    {
        foreach (var ent in _entities)
            Update(ent);
    }
    #endregion

    #region IDisposable接口相关函数
    public bool IsDisposed { get; private set; } = false;

    /// <summary>
    /// 手动释放
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 析构函数调用释放
    /// </summary>
    ~JigExTransient()
    {
        Dispose(false);
    }

    /// <summary>
    /// 销毁瞬态容器
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed) return;
        IsDisposed = true;

        Clear();// 清空瞬态容器并移除对象在图纸上的显示
    }
    #endregion
}