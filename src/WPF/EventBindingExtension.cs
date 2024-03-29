﻿namespace IFoxCAD.WPF;

/// <summary>
/// 事件绑定标签类
/// </summary>
/// <seealso cref="System.Windows.Markup.MarkupExtension" />
public class EventBindingExtension : MarkupExtension
{
    /// <summary>
    /// 命令属性
    /// </summary>
    public string? Command { get; set; }
    /// <summary>
    /// 命令参数属性
    /// </summary>
    public string? CommandParameter { get; set; }
    /// <summary>
    /// 当在派生类中实现时，返回用作此标记扩展的目标属性值的对象。
    /// </summary>
    /// <param name="serviceProvider">可为标记扩展提供服务的服务提供程序帮助程序。</param>
    /// <returns>
    /// 要在应用了扩展的属性上设置的对象值。
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// </exception>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (serviceProvider is null)
            throw new ArgumentNullException(nameof(serviceProvider));
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget targetProvider)
            throw new InvalidOperationException(message: $"{nameof(ProvideValue)}:{nameof(IProvideValueTarget)}");

        if (targetProvider.TargetObject is not FrameworkElement targetObject)
            throw new InvalidOperationException(message: $"{nameof(ProvideValue)}:{nameof(FrameworkElement)}");

        if (targetProvider.TargetProperty is not MemberInfo memberInfo)
            throw new InvalidOperationException(message: $"{nameof(ProvideValue)}:{nameof(MemberInfo)}");

        if (string.IsNullOrWhiteSpace(Command))
        {
            Command = memberInfo.Name.Replace("Add", "");
            if (Command.Contains("Handler"))
                Command = Command.Replace("Handler", "Command");
            else
                Command += "Command";
        }

        return CreateHandler(memberInfo, Command!, targetObject.GetType());
    }

    private Type? GetEventHandlerType(MemberInfo memberInfo)
    {
        Type? eventHandlerType = null;
        if (memberInfo is EventInfo eventInfo)
        {
            // var info = memberInfo as EventInfo;
            // var eventInfo = info;
            eventHandlerType = eventInfo.EventHandlerType;
        }
        else if (memberInfo is MethodInfo methodInfo)
        {
            // var info = memberInfo as MethodInfo;
            // var methodInfo = info;
            var pars = methodInfo.GetParameters();
            eventHandlerType = pars[1].ParameterType;
        }

        return eventHandlerType;
    }

#pragma warning disable IDE0060 // 删除未使用的参数
    private object? CreateHandler(MemberInfo memberInfo, string cmdName, Type targetType)
#pragma warning restore IDE0060 // 删除未使用的参数
    {
        var eventHandlerType = GetEventHandlerType(memberInfo);
        if (eventHandlerType is null)
            return null;

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

        if (CommandParameter is null)
            gen.Emit(OpCodes.Ldnull);
        else
            gen.Emit(OpCodes.Ldstr, CommandParameter);

        gen.Emit(OpCodes.Call, getMethod);
        gen.Emit(OpCodes.Ret);

        return method.CreateDelegate(eventHandlerType);
    }

    static readonly MethodInfo getMethod = typeof(EventBindingExtension)
        .GetMethod("HandlerIntern", new Type[] { typeof(object), typeof(object), typeof(string), typeof(string) });

#pragma warning disable IDE0051 // 删除未使用的私有成员
    static void Handler(object sender, object args)
#pragma warning restore IDE0051 // 删除未使用的私有成员
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
    public static void HandlerIntern(object sender, object args, string cmdName, string? commandParameter)
    {
        if (sender is FrameworkElement fe)
        {
            var cmd = GetCommand(fe, cmdName);
            object? commandParam = null;
            if (!string.IsNullOrWhiteSpace(commandParameter))
                commandParam = GetCommandParameter(fe, args, commandParameter!);
            if ((cmd is not null) && cmd.CanExecute(commandParam))
                cmd.Execute(commandParam);
        }
    }

    internal static ICommand? GetCommand(FrameworkElement target, string cmdName)
    {
        var vm = FindViewModel(target);
        if (vm is null)
            return null;

        var vmType = vm.GetType();
        var cmdProp = vmType.GetProperty(cmdName);
        if (cmdProp is not null)
            return cmdProp.GetValue(vm) as ICommand;
#if DEBUG
        throw new Exception("EventBinding path error: '" + cmdName + "' property not found on '" + vmType + "' 'DelegateCommand'");
#else
        return null;
#endif
    }

    internal static object GetCommandParameter(FrameworkElement target, object args, string commandParameter)
    {
        var classify = commandParameter.Split('.');
        object ret = classify[0] switch
        {
            "$e" => args,
            "$this" => classify.Length > 1 ? FollowPropertyPath(target, commandParameter.Replace("$this.", ""), target.GetType()) : target,
            _ => commandParameter,
        };
        return ret;
    }

    internal static ViewModelBase? FindViewModel(FrameworkElement? target)
    {
        if (target is null)
            return null;
        if (target.DataContext is ViewModelBase vm)
            return vm;
        return FindViewModel(target.GetParentObject() as FrameworkElement);
    }

    internal static object FollowPropertyPath(object target, string path, Type? valueType = null)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        if (path is null)
            throw new ArgumentNullException(nameof(path));

        valueType ??= target.GetType();
        var spls = path.Split('.');
        for (int i = 0; i < spls.Length; i++)
        {
            var property = valueType.GetProperty(spls[i]);
            if (property is null)
                throw new NullReferenceException("property");

            target = property.GetValue(target);
            valueType = property.PropertyType;
        }
        return target;
    }
}