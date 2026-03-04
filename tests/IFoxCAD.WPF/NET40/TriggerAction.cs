#if NET40 || NET35
using System;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Xaml.Behaviors
{
    /// <summary>
    /// 兼容 .NET 4.0 的 TriggerAction 实现
    /// </summary>
    public abstract class TriggerAction : Freezable
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled",
            typeof(bool),
            typeof(TriggerAction),
            new PropertyMetadata(true));

        protected override Freezable CreateInstanceCore()
        {
            return CreateInstanceCoreOverride();
        }

        protected virtual Freezable CreateInstanceCoreOverride()
        {
            Type currentType = GetType();
            return (Freezable)Activator.CreateInstance(currentType);
        }

        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        protected abstract void Invoke(object parameter);


    }

    /// <summary>
    /// 泛型版本的 TriggerAction
    /// </summary>
    public abstract class TriggerAction<T> : TriggerAction where T : DependencyObject
    {
        protected override void Invoke(object parameter)
        {
            var element = AssociatedObject as T;
            if (element != null)
            {
                Invoke(element, parameter);
            }
        }

        protected virtual void Invoke(T sender, object parameter)
        {
            // Base implementation can be overridden in derived classes
        }

        protected T AssociatedObject
        {
            get
            {
                return (T)GetValue(AssociatedObjectProperty);
            }
        }

        internal static readonly DependencyProperty AssociatedObjectProperty =
            DependencyProperty.Register("AssociatedObject", typeof(T), typeof(TriggerAction<T>), new PropertyMetadata(null));
    }
}
#endif