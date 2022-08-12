
using System.Windows;
using System.Windows.Input;


namespace Test.wpf;

class TestViewModel : ViewModelBase
{
    
    private string name;

    public string Name
    {
        get { return name; }
        set { Set(ref name, value); }
    }

    private RelayCommand clickCommand;

    public RelayCommand ClickCommand
    {
        get
        {
            if (clickCommand is null)
            {
                clickCommand = new(
                    execute => Name = "hello " + Name,
                    can => !string.IsNullOrEmpty(Name));
            }
            return clickCommand;
        }
    }

    private bool receiveMouseMove;

    public bool ReceiveMouseMove
    {
        get { return receiveMouseMove; }
        set { Set(ref receiveMouseMove, value); }
    }

    private string tipText;

    public string TipText
    {
        get { return tipText; }
        set { Set(ref tipText, value); }
    }

    private RelayCommand loadedCommand;
    public RelayCommand LoadCommand
    {
        get
        {
            if (loadedCommand is null)
            {
                loadedCommand = new(
                    execute => MessageBox.Show("程序加载完毕"));
            }
            return loadedCommand;
        }
    }

    private RelayCommand<MouseEventArgs> mouseMoveCommand;

    public RelayCommand<MouseEventArgs> MouseMoveCommand
    {
        get
        {
            if (mouseMoveCommand is null)
            {
                mouseMoveCommand = new(
                    execute =>
                    {
                        var pt = execute.GetPosition(execute.Device.Target);
                        var left = "左键放开";
                        var mid = "中键放开";
                        var right = "右键放开";

                        if (execute.LeftButton == MouseButtonState.Pressed)
                        {
                            left = "左键放下";
                        }
                        if (execute.MiddleButton == MouseButtonState.Pressed)
                        {
                            mid = "中键放下";
                        }
                        if (execute.RightButton == MouseButtonState.Pressed)
                        {
                            right = "右键放下";
                        }
                        TipText = $"当前鼠标位置：X={pt.X},Y={pt.Y}。当前鼠标状态：{left}、{mid}、{right}";
                    },
                    can => ReceiveMouseMove);
            }
            return mouseMoveCommand;
        }
    }






    public TestViewModel()
    {
        Name = "world";
    }



}
