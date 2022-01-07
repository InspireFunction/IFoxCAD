

# WPF支持

在项目文件里将`<Project Sdk="Microsoft.NET.Sdk">`替换为`<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">`。

在`<PropertyGroup></PropertyGroup>`标签里的`<TargetFrameworks>NET45</TargetFrameworks>`下面添加:
```xml
<UseWpf>true</UseWpf>
<UseWindowsForms>true</UseWindowsForms>
```

最后的项目文件如下：

```xml
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
	<PropertyGroup>
		<TargetFramework>net47</TargetFramework>
		<!-- 支持wpf -->
		<UseWpf>true</UseWpf>
		<!-- 支持winform -->
		<UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
  <!--...其他代码-->
</Project>
```

# mvvm模式支持

## 一、简单mvvm的实现

使用WPF的最佳实践就是采用mvvm模式，为了支持在cad插件里使用mvvm，ifoxcad内裤定义了两个简单基类来完成属性通知和命令定义。当然这是一种及其简单的mvvm模式的支持，你还要自己手动来写大部分的代码来实现完整的mvvm模式。

要实现mvvm模式，要新建一个XXXView文件，一个XXXViewModel文件。我们应该采用一种通用的命名约定，即所有的gui显示都有XXXView来完成，而所有的业务逻辑都由XXXViewModel来完成。下面以一个具体的示例来说明怎么在cad的插件里使用mvvm模式。

1. 将我们上一节建立的MyWindow1文件改名为MyWindowView，然后将涉及到的类名也全部更改为MyWindowView。

2. 然后将MyWindowView.xaml文件的内容改为：

```xaml
<Window x:Class="Test.MyWindowView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:Test"
        mc:Ignorable="d"
        Title="MyWindow1" Height="450" Width="800">
    <Grid>
        <StackPanel>
            <TextBox></TextBox>
            <Button>click</Button>
        </StackPanel>
    </Grid>
</Window>
```

就是添加了一个文本框，一个按钮。

3. 新建MyWindowViewModel.cs文件，内容如下：

```c#
using IFoxCad.WPF; // 这里引入IFoxCad.WPF命名空间，以便可以使用ViewModelBase和RelayCommand

namespace Test
{
    class MyWindowViewModel : ViewModelBase
    {
    	// 定义一个属性用于在文本框里显示
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                Set(ref _name, value);
            }
        }
				
		// 定义一个命令用于按钮的点击动作
        private RelayCommand clickCommand;
        public RelayCommand ClickCommand
        {
            get
            {
                if (clickCommand == null)
                {
                    clickCommand = new RelayCommand(
                        execute => Name = "hello " + Name,  // 定义要执行的行为
                        can => {return !string.IsNullOrEmpty(Name);}); // 定义命令是否可用
                }
                return clickCommand;
            }
        }
		// 初始化Name属性为 World
        public MyWindowViewModel()
        {
            Name = "World";
        }
    }
}

```

这里需要注意的是，定义的属性是为了将属性绑定到文本框的Text属性上，这个叫做数据绑定。然后wpf里对于我们winform里的事件其实采用的更高级一些的命令来完成。本示例，定义的命令也是一个属性，这个属性返回一个RelayCommand对象的实例，这是实例的初始化函数包括两个部分，一个是要执行的动作，第二个是确定什么条件下按钮是不可用的，这个是通过命令是否可用来完成，是要命令是不能执行的，wpf会自动将控件切换为不可用状态，使其不可点击。

4. 现在回过头来对在xaml里将刚刚的viewmodel里定义的属性和命令绑定到控件上。

```xaml
<TextBox Text="{Binding Name,UpdateSourceTrigger=PropertyChanged}"></TextBox>
<Button Command="{Binding ClickCommand}">click</Button>
```

将这两行代码替换一下。然后在后台代码里(MyWindowView.xaml.cs)添加一行代码将viewmodel绑定到view上。

```c#
public MyWindowView()
{
    InitializeComponent();
    DataContext = new MyWindowViewModel(); //这里将一个viewmodel的实例绑定到view的DataContext属性上。
}
```

5. 至此，一个简单的wpf的mvvm模式的代码就完成了，下面的代码演示了怎么在cad里显示这个wpf窗体。

```c#
[CommandMethod("test")]
public void Test()
{
   var test = new MyWindowView();
   Application.ShowModalWindow(test);
}
```

6. 最后，这个窗体的效果是，当你点击按钮时，文本框的文字前面会加上hello。当你将文本框的文字全部删除后，按钮会变成不可用状态。如果你在试验的时候没有这个效果，这是cad的延迟导致的。多删除几次试几次后就会如期运行。

## 二、mvvm中的事件处理

在WPF里，并不是所有的控件都提供了commad属性用于绑定命令，所以还是需要进行控件的事件处理的，比如窗口的Loaded事件，鼠标事件，键盘事件等。关于WPF的事件处理，IFoxCad内裤提供了两种方式进行处理，一种就是利用 **Microsoft.Xaml.Behaviors.dll** 这个类库，利用了 **i:Interaction.Triggers** 标签在xaml文件里将命令绑定到事件上，这种方式是网上比较常见的一种方式；第二种是自定义了一个xaml标签 **eb:EventBinding** ，利用这个标签将命令绑定到事件上。两种方式实现的效果是一样的，但是 **eb:EventBinding**  标签绑定的方式的代码量要小一些。

下面就两种方式实现同一种事件处理的效果提供了两种方式的代码示例作为说明。由于两种方式的差异主要在xaml文件里，ViewModel的代码是一样的。因此主要讲述两种xaml的差异部分，ViewModel的代码直接贴在下面不做讲解。

```c#
public class TestViewModel : ViewModelBase
{

        private bool _IsReceiveMouseMove = true;
        public bool IsReceiveMouseMove
        {
            get { return _IsReceiveMouseMove; }
            set
            {
                Set(ref _IsReceiveMouseMove, value);
            }
        }

        private string _tipText;
        public string TipText
        {
            get { return _tipText; }
            set
            {
                Set(ref _tipText, value);
            }
        }



        private RelayCommand loadedCommand;

        public RelayCommand LoadedCommand
        {
            get 
            {
                if (loadedCommand == null)
                {
                    loadedCommand = new RelayCommand(execute => MessageBox.Show("程序加载完毕！"));
                    
                }
                return loadedCommand;
            }
            
        }

        private RelayCommand<MouseEventArgs> mouseMoveCommand;
        public RelayCommand<MouseEventArgs> MouseMoveCommand
        {
            get
            {
                if (mouseMoveCommand == null)
                {
                    mouseMoveCommand = 
                      new RelayCommand<MouseEventArgs>(
                            e =>
                            {
                                var point = e.GetPosition(e.Device.Target);
                                var left = "左键放开";
                                var mid = "中键放开";
                                var right = "右键放开";

                                if (e.LeftButton == MouseButtonState.Pressed)
                                {
                                    left = "左键放下";
                                }
                                if (e.MiddleButton == MouseButtonState.Pressed)
                                {
                                    mid = "中键放下";
                                }
                                if (e.RightButton == MouseButtonState.Pressed)
                                {
                                    right = "右键放下";
                                }

                                TipText = $"当前鼠标位置 X:{point.X} Y:{point.Y} 当前鼠标状态：{left} {mid} {right}.";
                            },
                        o => IsReceiveMouseMove);
                }
                return mouseMoveCommand;
            }
        }
}
```

### 2.1 自定义标签的方式

首先是在xaml里引入命名空间。

`xmlns:eb="clr-namespace:IFoxCAD.WPF;assembly=IFoxCAD.WPF"`

然后

`Loaded="{eb:EventBinding Command=LoadedCommand}"`

`MouseMove="{eb:EventBinding Command=MouseMoveCommand,CommandParameter=$e}"` 这里要注意的是显式的传入了鼠标移动事件的参数。

注意命令参数部分，如果这个事件是带参数的，或者说这个命令是带参数的，要传入参数。

关于命令及命令参数使用方式如下：

- Command
  1. `{eb:EventBinding}`  利用简单的名字匹配来自动搜寻命令，也就是说不用指定命令名，不是很推荐。
  2. `{eb:EventBinding Command=CommandName}` 指定命令名，建议总是使用这种方式

- CommandParameter
  1. `$e` (事件参数，这里特指的是事件本身带的参数，也就是你以前写事件的处理函数时候的XXXXEventArgs e这个参数)
  2. `$this` or ​`$this.Property` (view本身或者属性)
  3. `string` (要传入的字符串)

完整的xaml代码如下：

```xaml
<Window x:Class="Test.TestView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:Test"
        xmlns:eb="clr-namespace:IFoxCad.WPF;assembly=IFoxCad"
        mc:Ignorable="d"
        Title="TestView" Height="450" Width="800"
        Loaded="{eb:EventBinding Command=LoadedCommand}"
        MouseMove="{eb:EventBinding Command=MouseMoveCommand,CommandParameter=$e}">
    <DockPanel>
        <CheckBox Content="接收鼠标移动消息" DockPanel.Dock="Top" Margin="5"
                  VerticalAlignment="Center" IsChecked="{Binding isReceiveMouseMove}"/>
        <Label Content="{Binding TipText}" Margin="5"/>
    </DockPanel>
</Window>
```

### 2.2 利用Behaviors的方式

首先nuget安装**Microsoft.Xaml.Behaviors.Wpf**包。

然后在xaml文件里，引入命名空间。

`xmlns:eb="clr-namespace:IFoxCad.WPF;assembly=IFoxCad"`

`xmlns:i="http://schemas.microsoft.com/xaml/behaviors"`

然后绑定命令到事件上：

```xaml
<i:Interaction.Triggers>
        <i:EventTrigger EventName="Loaded">
            <i:InvokeCommandAction Command="{Binding LoadedCommand}"/>
        </i:EventTrigger>
        <i:EventTrigger EventName="MouseMove">
            <eb:EventCommand Command="{Binding MouseMoveCommand}"/>
        </i:EventTrigger>
</i:Interaction.Triggers>
```

细心的同学可能会发现绑定命令的地方标签是不一样的。

**i:InvokeCommandAction** 这个标签是由 **Microsoft.Xaml.Behaviors.Wpf** 包提供的。

**eb:EventCommand** 这个标签是由IFoxCad内裤提供的。

两者的区别就是InvokeCommandAction 是不能传入事件的参数的，所以为了处理事件参数自定义了EventCommand。就如同上面的鼠标移动事件，是有时间参数要处理的，所以用了自定义的EventCommand，虽然xaml文件里没有显式的传入这个参数。

虽然InvokeCommandAction这个标签的后面是可以带命令参数的，比如：

```xaml
<i:Interaction.Triggers>
              <i:EventTrigger EventName="ValueChanged">
                    <i:InvokeCommandAction Command="{Binding MyCommand}" CommandParameter="{Binding Text, ElementName=textBox}"/>
             </i:EventTrigger>
        </i:Interaction.Triggers>
```

但是这个命令参数是不能处理事件自带的参数的 。

最后是完整的xaml代码：

```xaml
<Window x:Class="Test.TestView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:Test"
        xmlns:eb="clr-namespace:IFoxCad.WPF;assembly=IFoxCad"
        xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
        mc:Ignorable="d"
        Title="TestView" Height="450" Width="800">
    <i:Interaction.Triggers>
        <i:EventTrigger EventName="Loaded">
            <i:InvokeCommandAction Command="{Binding LoadedCommand}"/>
        </i:EventTrigger>
        <i:EventTrigger EventName="MouseMove">
            <eb:EventCommand Command="{Binding MouseMoveCommand}"/>
        </i:EventTrigger>
    </i:Interaction.Triggers>
    
    <DockPanel>
        <CheckBox Content="接收鼠标移动消息" DockPanel.Dock="Top" Margin="5"
                  VerticalAlignment="Center" IsChecked="{Binding isReceiveMouseMove}"/>
        <Label Content="{Binding TipText}" Margin="5"/>
    </DockPanel>
</Window>
```

### 2.3 关于两种方式的选择

送给选择困难症的：如果可以选择自定义标签的方式，简单一些。遇到问题解决不了，就用behaviors的方式，网上的资源丰富一些，也许能找到你的答案。 

## 三、 关于mvvm模式的建议

我们并不推荐严格的mvvm模式，主要原因是要引入比如message模式等方式处理类似窗口关闭，窗口间通信等问题。鉴于cad插件的界面复杂程度还没到后台事件满天飞，逻辑复杂的地步，因此后台写点事件处理，界面和后逻辑混在一起也未尝不可。

仅仅是建议，你爱怎样就怎样。

