namespace IFoxCAD.Cad;

/// <summary>
/// 文档锁管理器，用于管理文档的锁定和解锁。
/// </summary>
public sealed class DocumentLockManager : IDisposable
{
    /// <summary>
    /// 用于存储文档锁的实例，如果文档未锁定则为null。
    /// </summary>
    private readonly DocumentLock? _documentLock;

    /// <summary>
    /// 初始化DocumentLockManager实例。
    /// </summary>
    /// <param name="doc">需要进行锁定管理的文档。</param>
    internal DocumentLockManager(Document doc)
    {
        // 如果文档未锁定，则尝试锁定文档，否则不创建锁实例。
        _documentLock = doc.LockMode(false) == DocumentLockMode.NotLocked ? doc.LockDocument() : null;
    }

    /// <summary>
    /// 表示当前实例是否已被释放。
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// 释放当前实例持有的资源。
    /// </summary>
    public void Dispose()
    {
        // 如果资源已经被释放，则不再次释放。
        if (IsDisposed)
            return;
        // 释放文档锁资源，如果存在的话。
        _documentLock?.Dispose();

        // 标记当前实例为已释放状态。
        IsDisposed = true;
    }
}

/// <summary>
/// 提供Document类型的扩展方法来方便创建DocumentLockManager实例。
/// </summary>
public static class DocumentLockManagerExtension
{
    /// <summary>
    /// 安全锁定文档，返回一个新的DocumentLockManager实例。
    /// </summary>
    /// <param name="doc">需要进行锁定的文档。</param>
    /// <returns>DocumentLockManager实例，用于管理文档锁。</returns>
    public static DocumentLockManager SecurelyLock(this Document doc)
    {
        // 创建并返回DocumentLockManager实例。
        return new DocumentLockManager(doc);
    }
}
