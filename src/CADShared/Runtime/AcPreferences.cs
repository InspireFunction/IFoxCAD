#if !NET35
namespace IFoxCAD.Cad;

/// <summary>
/// AcapPreference扩展
/// </summary>
public static class AcPreferences
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public static class Display
    {
        static Display()
        {
            dynamic preferences = Acap.Preferences;
            _acadDisplay = preferences.Display;
        }

        private static readonly dynamic _acadDisplay;

        /// <summary>
        /// 布局显示边距
        /// </summary>
        public static bool LayoutDisplayMargins
        {
            get => _acadDisplay.LayoutDisplayMargins;
            set => _acadDisplay.LayoutDisplayMargins = value;
        }

        /// <summary>
        /// 布局显示纸
        /// </summary>
        public static bool LayoutDisplayPaper
        {
            get => _acadDisplay.LayoutDisplayPaper;
            set => _acadDisplay.LayoutDisplayPaper = value;
        }

        /// <summary>
        /// 布局显示纸张阴影
        /// </summary>
        public static bool LayoutDisplayPaperShadow
        {
            get => _acadDisplay.LayoutDisplayPaperShadow;
            set => _acadDisplay.LayoutDisplayPaperShadow = value;
        }

        /// <summary>
        /// 布局显示绘图设置
        /// </summary>
        public static bool LayoutShowPlotSetup
        {
            get => _acadDisplay.LayoutShowPlotSetup;
            set => _acadDisplay.LayoutShowPlotSetup = value;
        }

        /// <summary>
        /// 布局创建视口
        /// </summary>
        public static bool LayoutCreateViewport
        {
            get => _acadDisplay.LayoutCreateViewport;
            set => _acadDisplay.LayoutCreateViewport = value;
        }

        /// <summary>
        /// 显示滚动条
        /// </summary>
        public static bool DisplayScrollBars
        {
            get => _acadDisplay.DisplayScrollBars;
            set => _acadDisplay.DisplayScrollBars = value;
        }

        /// <summary>
        /// 显示屏幕菜单
        /// </summary>
        public static bool DisplayScreenMenu
        {
            get => _acadDisplay.DisplayScreenMenu;
            set => _acadDisplay.DisplayScreenMenu = value;
        }

        /// <summary>
        /// 使用光标十字的大小
        /// </summary>
        public static int CursorSize
        {
            get => _acadDisplay.CursorSize;
            set => _acadDisplay.CursorSize = value;
        }

        /// <summary>
        /// 停靠的可见线
        /// </summary>
        public static int DockedVisibleLines
        {
            get => _acadDisplay.DockedVisibleLines;
            set => _acadDisplay.DockedVisibleLines = value;
        }

        /// <summary>
        /// 显示光栅图像
        /// </summary>
        public static bool ShowRasterImage
        {
            get => _acadDisplay.ShowRasterImage;
            set => _acadDisplay.ShowRasterImage = value;
        }

        /// <summary>
        /// 模型空间背景颜色
        /// </summary>
        public static Color GraphicsWinModelBackgrndColor
        {
            get
            {
                uint color = _acadDisplay.GraphicsWinModelBackgrndColor;
                return UIntToColor(color);
            }
            set
            {
                var color = ColorToUInt(value);
                _acadDisplay.GraphicsWinModelBackgrndColor = color;
            }
        }

        /// <summary>
        /// 命令栏win文本背景颜色
        /// </summary>
        public static Color TextWinBackgrndColor
        {
            get
            {
                uint color = _acadDisplay.TextWinBackgrndColor;
                return UIntToColor(color);
            }
            set
            {
                var color = ColorToUInt(value);
                _acadDisplay.TextWinBackgrndColor = color;
            }
        }

        /// <summary>
        /// 命令栏win文本字体颜色
        /// </summary>
        public static Color TextWinTextColor
        {
            get
            {
                uint color = _acadDisplay.TextWinTextColor;
                return UIntToColor(color);
            }
            set
            {
                var color = ColorToUInt(value);
                _acadDisplay.TextWinTextColor = color;
            }
        }

        /// <summary>
        /// 模型鼠标十字颜色
        /// </summary>
        public static Color ModelCrosshairColor
        {
            get
            {
                uint color = _acadDisplay.ModelCrosshairColor;
                return UIntToColor(color);
            }
            set
            {
                var color = ColorToUInt(value);
                _acadDisplay.ModelCrosshairColor = color;
            }
        }

        /// <summary>
        /// 布局鼠标十字颜色
        /// </summary>
        public static Color LayoutCrosshairColor
        {
            get
            {
                uint color = _acadDisplay.LayoutCrosshairColor;
                return UIntToColor(color);
            }
            set
            {
                var color = ColorToUInt(value);
                _acadDisplay.LayoutCrosshairColor = color;
            }
        }

        /// <summary>
        /// 自动跟踪VEC颜色
        /// </summary>
        public static Color AutoTrackingVecColor
        {
            get
            {
                uint color = _acadDisplay.AutoTrackingVecColor;
                return UIntToColor(color);
            }
            set
            {
                var color = ColorToUInt(value);
                _acadDisplay.AutoTrackingVecColor = color;
            }
        }

        /// <summary>
        /// 文本字体
        /// </summary>
        public static string TextFont
        {
            get => _acadDisplay.TextFont;
            set => _acadDisplay.TextFont = value;
        }

        /// <summary>
        /// 文本字体样式
        /// </summary>
        public static dynamic TextFontStyle
        {
            get => _acadDisplay.TextFontStyle;
            set => _acadDisplay.TextFontStyle = value;
        }

        /// <summary>
        /// 文本字体大小
        /// </summary>
        public static int TextFontSize
        {
            get => _acadDisplay.TextFontSize;
            set => _acadDisplay.TextFontSize = value;
        }

        /// <summary>
        /// 历史文本的容量,最多2048行
        /// </summary>
        public static int HistoryLines
        {
            get => _acadDisplay.HistoryLines;
            set => _acadDisplay.HistoryLines = value;
        }

        /// <summary>
        /// 最大化自动设置窗体
        /// </summary>
        public static bool MaxAutoCADWindow
        {
            get => _acadDisplay.MaxAutoCADWindow;
            set => _acadDisplay.MaxAutoCADWindow = value;
        }

        /// <summary>
        /// 显示布局选项卡
        /// </summary>
        public static bool DisplayLayoutTabs
        {
            get => _acadDisplay.DisplayLayoutTabs;
            set => _acadDisplay.DisplayLayoutTabs = value;
        }

        /// <summary>
        /// 图像框架亮点
        /// </summary>
        public static bool ImageFrameHighlight
        {
            get => _acadDisplay.ImageFrameHighlight;
            set => _acadDisplay.ImageFrameHighlight = value;
        }

        /// <summary>
        /// 真彩色图像
        /// </summary>
        public static bool TrueColorImages
        {
            get => _acadDisplay.TrueColorImages;
            set => _acadDisplay.TrueColorImages = value;
        }

        /// <summary>
        /// 参照淡化
        /// </summary>
        public static int XRefFadeIntensity
        {
            get => _acadDisplay.XRefFadeIntensity;
            set => _acadDisplay.XRefFadeIntensity = value;
        }
    }

    private static uint ColorToUInt(Color color)
    {
        var c = color.ColorValue;
        return (uint)(c.R | c.G << 8 | c.B << 16);
    }

    private static Color UIntToColor(uint color)
    {
        var r = (byte)(color >> 0);
        var g = (byte)(color >> 8);
        var b = (byte)(color >> 16);
        return Color.FromRgb(r, g, b);
    }
} 
#endif