namespace IFoxCAD.Event;

public class EventTemporaryShutdownManager : IDisposable
{
    private readonly CadEvent _cadEvent;
    /// <summary>
    /// 临时关闭事件，dispose的时候重开,防止事件嵌套时使用
    /// </summary>
    /// <param name="cadEvent">事件枚举</param>
    internal EventTemporaryShutdownManager(CadEvent cadEvent)
    {
        _cadEvent = cadEvent;
        EventFactory.RemoveEvent(_cadEvent);
    }
    #region Dispose

    private bool _isDisposed = false;
    public bool IsDisposed => _isDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_isDisposed)
            {
                EventFactory.AddEvent(_cadEvent);
                _isDisposed = true;
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

