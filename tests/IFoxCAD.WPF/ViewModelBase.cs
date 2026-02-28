namespace IFoxCAD.WPF;

/// <summary>
/// ViewModel基类
/// </summary>
/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
public class ViewModelBase : INotifyPropertyChanged
{
    // 事件锁对象，确保线程安全
    private readonly object _eventLock = new object();
    private PropertyChangedEventHandler? _propertyChanged;

    /// <summary>
    /// 属性值更改事件。
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged
    {
        add { lock (_eventLock) _propertyChanged += value; }
        remove { lock (_eventLock) _propertyChanged -= value; }
    }

    /// <summary>
    /// 属性改变时调用
    /// </summary>
    /// <param name="propertyName">属性名</param>
    public void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        // 获取事件处理程序的本地副本（线程安全）
        PropertyChangedEventHandler? tempHandler;
        lock (_eventLock)
        {
            tempHandler = _propertyChanged;
        }
        tempHandler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    /// <summary>
    /// 设置属性函数，自动通知属性改变事件
    /// </summary>
    /// <typeparam name="T">属性类型</typeparam>
    /// <param name="storage">属性</param>
    /// <param name="value">属性值</param>
    /// <param name="propertyName">属性名</param>
    /// <returns>成功返回 <see langword="true"/>，反之 <see langword="false"/></returns>
    protected virtual bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
    {
        if (object.Equals(storage, value))
            return false;

        storage = value;
        this.OnPropertyChanged(propertyName);

        return true;
    }
    /// <summary>
    /// 创建命令
    /// </summary>
    /// <param name="executeMethod">要调用的命令函数委托</param>
    /// <returns>WPF命令</returns>
    protected RelayCommand CreateCommand(Action<object> executeMethod)
    {
        return CreateCommand(executeMethod, (o) => true);
    }
    /// <summary>
    /// 创建命令
    /// </summary>
    /// <param name="executeMethod">要调用的命令函数委托</param>
    /// <param name="canExecuteMethod">命令是否可以执行的委托</param>
    /// <returns>WPF命令</returns>
    protected RelayCommand CreateCommand(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
    {
        return new(executeMethod, canExecuteMethod);
    }
}