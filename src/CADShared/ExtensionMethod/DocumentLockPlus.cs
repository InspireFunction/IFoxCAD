namespace IFoxCAD.Cad;


/// <summary>
/// 包裹官方的文档锁
/// </summary>
public class DocumentLockPlus : IDisposable
{
    Document _doc;
    private bool _disposed;

    /// <summary>
    /// 包裹官方的文档锁
    /// </summary>
    /// <param name="doc"></param>
    public DocumentLockPlus(Document doc)
    {
        _doc = doc;
    }

    /// <summary>
    /// 释放
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">是否释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DocumentLockManager.RemoveLock(_doc);
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~DocumentLockPlus()
    {
        Dispose(false);
    }
}