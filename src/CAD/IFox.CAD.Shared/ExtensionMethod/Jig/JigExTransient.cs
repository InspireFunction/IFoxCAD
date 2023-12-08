using Autodesk.AutoCAD.GraphicsInterface;

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
    readonly  HashSet<Drawable> _drawableSet;
    readonly TransientManager _manager;
    #endregion

    #region 公开属性
    /// <summary>
    /// 图元集合
    /// </summary>
    public Entity[] Entities => _drawableSet.OfType<Entity>().ToArray();
    /// <summary>
    /// 对象集合
    /// </summary>
    public Drawable[] Drawables => _drawableSet.ToArray();
    /// <summary>
    /// 数量
    /// </summary>
    public int Count => _drawableSet.Count;
    #endregion

    #region 构造函数
    /// <summary>
    /// 瞬态容器
    /// </summary>
    public JigExTransient()
    {
        _integerCollection = new();
        _drawableSet = new(); 
        _manager=TransientManager.CurrentTransientManager;
    }
    #endregion

    #region 方法
    /// <summary>
    /// 判断瞬态容器里是否含有对象
    /// </summary>
    /// <param name="drawable">对象</param>
    /// <returns>含有返回true</returns>
    public bool Contains(Drawable drawable)
    {
        return _drawableSet.Contains(drawable);
    }

    /// <summary>
    /// 向瞬态容器中添加对象
    /// </summary>
    /// <param name="ent">图元</param>
    /// <param name="tdm">绘图模式</param>
    public void Add(Drawable drawable, TransientDrawingMode tdm = TransientDrawingMode.Main)
    {
        if (_drawableSet.Add(drawable))
        {
            _manager.AddTransient(drawable, tdm, 128, _integerCollection);
        }
    }


    /// <summary>
    /// 从瞬态容器中移除图元
    /// </summary>
    /// <param name="drawable">已经加入瞬态容器的图元</param>
    public void Remove(Drawable drawable)
    {
        if (!Contains(drawable))
            return;
        _manager.EraseTransient(drawable, _integerCollection);
        _drawableSet.Remove(drawable);
    }

    /// <summary>
    /// 清空瞬态容器并移除图元显示
    /// </summary>
    public void Clear()
    {
        foreach (var drawable in _drawableSet)
        {
            _manager.EraseTransient(drawable, _integerCollection);
        }
        _drawableSet.Clear();
    }


    /// <summary>
    /// 更新单个显示
    /// </summary>
    /// <param name="drawable">已经加入瞬态容器的图元</param>
    public void Update(Drawable drawable)
    {
        if (!Contains(drawable))
            return;
        _manager.UpdateTransient(drawable, _integerCollection);
    }

    /// <summary>
    /// 更新全部显示
    /// </summary>
    public void UpdateAll()
    {
        foreach (var drawable in _drawableSet)
            Update(drawable);
    }
    #endregion

    #region IDisposable接口相关函数
    /// <summary>
    /// 是否注销
    /// </summary>
    public bool IsDisposed { get; private set; }

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
