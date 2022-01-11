namespace IFoxCAD.WPF;

/// <summary>
/// 事件绑定标签类
/// </summary>
/// <seealso cref="System.Windows.Markup.MarkupExtension" />
public class EventBindingExtension : MarkupExtension
{
    /// <summary>
    /// 命令属性
    /// </summary>
    public string Command { get; set; }
    /// <summary>
    /// 命令参数属性
    /// </summary>
    public string CommandParameter { get; set; }
    /// <summary>
    /// 当在派生类中实现时，返回用作此标记扩展的目标属性值的对象。
    /// </summary>
    /// <param name="serviceProvider">可为标记扩展提供服务的服务提供程序帮助程序。</param>
    /// <returns>
    /// 要在应用了扩展的属性上设置的对象值。
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// </exception>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }
        if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget targetProvider))
        {
            throw new InvalidOperationException();
        }

        if (!(targetProvider.TargetObject is FrameworkElement targetObject))
        {
            throw new InvalidOperationException();
        }

        var memberInfo = targetProvider.TargetProperty as MemberInfo;
        if (memberInfo == null)
        {
            throw new InvalidOperationException();
        }

        if (string.IsNullOrWhiteSpace(Command))
        {
            Command = memberInfo.Name.Replace("Add", "");
            if (Command.Contains("Handler"))
            {
                Command = Command.Replace("Handler", "Command");
            }
            else
            {
                Command += "Command";
            }
        }

        return CreateHandler(memberInfo, Command, targetObject.GetType());
    }

    private Type GetEventHandlerType(MemberInfo memberInfo)
    {
        Type eventHandlerType = null;
        if (memberInfo is EventInfo)
        {
            var info = memberInfo as EventInfo;
            var eventInfo = info;
            eventHandlerType = eventInfo.EventHandlerType;
        }
        else if (memberInfo is MethodInfo)
        {
            var info = memberInfo as MethodInfo;
            var methodInfo = info;
            ParameterInfo[] pars = methodInfo.GetParameters();
            eventHandlerType = pars[1].ParameterType;
        }

        return eventHandlerType;
    }

    private object CreateHandler(MemberInfo memberInfo, string cmdName, Type targetType)
    {
        Type eventHandlerType = GetEventHandlerType(memberInfo);

        if (eventHandlerType == null) return null;

        var handlerInfo = eventHandlerType.GetMethod("Invoke");
        var method = new DynamicMethod("", handlerInfo.ReturnType,
            new Type[]
            {
                    handlerInfo.GetParameters()[0].ParameterType,
                    handlerInfo.GetParameters()[1].ParameterType,
            });

        var gen = method.GetILGenerator();
        gen.Emit(OpCodes.Ldarg, 0);
        gen.Emit(OpCodes.Ldarg, 1);
        gen.Emit(OpCodes.Ldstr, cmdName);
        if (CommandParameter == null)
        {
            gen.Emit(OpCodes.Ldnull);
        }
        else
        {
            gen.Emit(OpCodes.Ldstr, CommandParameter);
        }
        gen.Emit(OpCodes.Call, getMethod);
        gen.Emit(OpCodes.Ret);

        return method.CreateDelegate(eventHandlerType);
    }

    static readonly MethodInfo getMethod = typeof(EventBindingExtension).GetMethod("HandlerIntern", new Type[] { typeof(object), typeof(object), typeof(string), typeof(string) });

    static void Handler(object sender, object args)
    {
        HandlerIntern(sender, args, "cmd", null);
    }
    /// <summary>
    /// Handlers the intern.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="cmdName">Name of the command.</param>
    /// <param name="commandParameter">The command parameter.</param>
    public static void HandlerIntern(object sender, object args, string cmdName, string commandParameter)
    {
        var fe = sender as FrameworkElement;
        if (fe != null)
        {
            ICommand cmd = GetCommand(fe, cmdName);
            object commandParam = null;
            if (!string.IsNullOrWhiteSpace(commandParameter))
            {
                commandParam = GetCommandParameter(fe, args, commandParameter);
            }
            if ((cmd != null) && cmd.CanExecute(commandParam))
            {
                cmd.Execute(commandParam);
            }
        }
    }

    internal static ICommand GetCommand(FrameworkElement target, string cmdName)
    {
        var vm = FindViewModel(target);
        if (vm == null) return null;

        var vmType = vm.GetType();
        var cmdProp = vmType.GetProperty(cmdName);
        if (cmdProp != null)
        {
            return cmdProp.GetValue(vm) as ICommand;
        }
#if DEBUG
        throw new Exception("EventBinding path error: '" + cmdName + "' property not found on '" + vmType + "' 'DelegateCommand'");
#endif

        return null;
    }

    internal static object GetCommandParameter(FrameworkElement target, object args, string commandParameter)
    {
        var classify = commandParameter.Split('.');
        object ret;
        switch (classify[0])
        {
            case "$e":
                ret = args;
                break;
            case "$this":
                ret = classify.Length > 1 ? FollowPropertyPath(target, commandParameter.Replace("$this.", ""), target.GetType()) : target;
                break;
            default:
                ret = commandParameter;
                break;
        }

        return ret;
    }

    internal static ViewModelBase FindViewModel(FrameworkElement target)
    {
        if (target == null) return null;

        if (target.DataContext is ViewModelBase vm) return vm;

        var parent = target.GetParentObject() as FrameworkElement;

        return FindViewModel(parent);
    }

    internal static object FollowPropertyPath(object target, string path, Type valueType = null)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (path == null) throw new ArgumentNullException(nameof(path));

        Type currentType = valueType ?? target.GetType();

        foreach (string propertyName in path.Split('.'))
        {
            PropertyInfo property = currentType.GetProperty(propertyName);
            if (property == null) throw new NullReferenceException("property");

            target = property.GetValue(target);
            currentType = property.PropertyType;
        }
        return target;
    }
}
