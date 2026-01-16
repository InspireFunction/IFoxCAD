namespace Test.wpf;

using System.Windows;

/// <summary>
/// TestView.xaml 的交互逻辑
/// </summary>
public partial class TestView : Window
{
    public TestView()
    {
        InitializeComponent();
        DataContext = new TestViewModel();
    }
}
