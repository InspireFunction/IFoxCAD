using System.Windows;
using System.Windows.Media;

namespace IFoxCAD.WPF
{
    /// <summary>
    /// 依赖属性扩展类
    /// </summary>
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// 获取父对象依赖属性
        /// </summary>
        /// <param name="child">子对象</param>
        /// <returns>依赖属性</returns>
        public static DependencyObject? GetParentObject(this DependencyObject child)
        {
            if (child is null)
                return null;

            if (child is ContentElement contentElement)
            {
                var parent = ContentOperations.GetParent(contentElement);
                if (parent is not null) return parent;

                var fce = contentElement as FrameworkContentElement;
                return fce?.Parent;
            }

            if (child is FrameworkElement frameworkElement)
            {
                var parent = frameworkElement.Parent;
                if (parent is not null)
                    return parent;
            }

            return VisualTreeHelper.GetParent(child);
        }
    }
}
