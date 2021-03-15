using Microsoft.Xaml.Behaviors;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace IFoxCAD.WPF
{
    /// <summary>
    /// 命令基类
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class RelayCommand : ICommand
    {
        readonly Func<object, bool> _canExecute;
        readonly Action<object> _execute;
        /// <summary>
        /// 初始化 <see cref="RelayCommand"/> 类.
        /// </summary>
        /// <param name="execute">执行函数</param>
        public RelayCommand(Action<object> execute):this(execute,null)
        {

        }
        /// <summary>
        /// 初始化 <see cref="RelayCommand"/> 类.
        /// </summary>
        /// <param name="execute">执行函数委托</param>
        /// <param name="canExecute">是否可执行函数委托</param>
        /// <exception cref="ArgumentNullException">execute</exception>
        public RelayCommand(Action<object> execute,Func<object,bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 当出现影响是否应执行该命令的更改时发生。
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }
        /// <summary>
        /// 定义确定此命令是否可在其当前状态下执行的方法。
        /// </summary>
        /// <param name="parameter">此命令使用的数据。  如果此命令不需要传递数据，则该对象可以设置为 <see langword="null" />。</param>
        /// <returns>
        /// 如果可执行此命令，则为 <see langword="true" />；否则为 <see langword="false" />。
        /// </returns>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }
        /// <summary>
        /// 定义在调用此命令时要调用的方法。
        /// </summary>
        /// <param name="parameter">此命令使用的数据。  如果此命令不需要传递数据，则该对象可以设置为 <see langword="null" />。</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

    /// <summary>
    /// 命令泛型基类
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class RelayCommand<T> : ICommand
    {
        readonly Func<T, bool> _canExecute;
        readonly Action<T> _execute;
        /// <summary>
        /// 初始化 <see cref="RelayCommand{T}"/> 类。
        /// </summary>
        /// <param name="execute">执行函数</param>
        public RelayCommand(Action<T> execute) : this(execute, (o)=>true)
        {

        }
        
        /// <summary>
        /// 初始化 <see cref="RelayCommand{T}"/> 类。
        /// </summary>
        /// <param name="execute">执行函数委托</param>
        /// <param name="canExecute">是否可执行函数委托</param>
        /// <exception cref="System.ArgumentNullException">execute</exception>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        /// <summary>
        /// 当出现影响是否应执行该命令的更改时发生。
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }
        /// <summary>
        /// 定义确定此命令是否可在其当前状态下执行的方法。
        /// </summary>
        /// <param name="parameter">此命令使用的数据。  如果此命令不需要传递数据，则该对象可以设置为 <see langword="null" />。</param>
        /// <returns>
        /// 如果可执行此命令，则为 <see langword="true" />；否则为 <see langword="false" />。
        /// </returns>
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }
            return _canExecute((T)parameter);
        }
        /// <summary>
        /// 定义在调用此命令时要调用的方法。
        /// </summary>
        /// <param name="parameter">此命令使用的数据。  如果此命令不需要传递数据，则该对象可以设置为 <see langword="null" />。</param>
        public void Execute(object parameter)
        {
            if (_execute != null && CanExecute(parameter))
            {
                _execute((T)parameter);
            }
        }
    }

    /// <summary>
    /// 事件命令
    /// </summary>
    public class EventCommand : TriggerAction<DependencyObject>
    {
        /// <summary>
        /// 执行动作
        /// </summary>
        /// <param name="parameter">要执行的动作参数， 如果动作为提供参数，就设置为null</param>
        protected override void Invoke(object parameter)
        {
            if (CommandParameter != null)
            {
                parameter = CommandParameter;
            }
            if (Command != null)
            {
                Command.Execute(parameter);
            }
        }
        /// <summary>
        /// 事件
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        /// <summary>
        /// 事件属性
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(EventCommand), new PropertyMetadata(null));

        /// <summary>
        /// 事件参数，如果为空，将自动传入事件的真实参数
        /// </summary>
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        /// <summary>
        /// 事件参数属性
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventCommand), new PropertyMetadata(null));
    }

}
