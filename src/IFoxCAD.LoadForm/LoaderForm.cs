namespace IFoxCAD.LoadEx;

using System.Windows.Forms;

public partial class LoaderForm : Form
{
    public string? DllPath;
    readonly AssemblyDependent _ad;

    public LoaderForm()
    {
        //Owner = form;
        //MdiParent = form;
        InitializeComponent();
        _ad = new();
    }

    void LoaderForm_Load(object sender, EventArgs e)
    {
        StartPosition = FormStartPosition.CenterScreen;//在当前屏幕中央
        if (DllPath != null)
            textBox1.Text = DllPath;
    }

    void TextBox1_TextChanged(object sender, EventArgs e)
    {
        DllPath = textBox1.Text;
        if (string.IsNullOrEmpty(DllPath?.Trim()))
            return;
        toolTip1.SetToolTip(textBox1, Path.GetFullPath(textBox1.Text));
    }

    void Button1_Click(object sender, EventArgs e)
    {
        loadDlldialog.Filter = "dll文件(*.dll)|*.dll";
        loadDlldialog.CheckFileExists = true;

        var dr = loadDlldialog.ShowDialog();
        if (dr == DialogResult.OK)
            textBox1.Text = Path.GetFullPath(loadDlldialog.FileName);
    }

    void Button2_Click(object sender, EventArgs e)
    {
        if (!File.Exists(textBox1.Text))
        {
            MessageBox.Show($"文件 {textBox1.Text} 不存在!", "提示", MessageBoxButtons.OK);
            return;
        }
        LoadDlls(new() { textBox1.Text });
    }

    //鼠标拖拽文件到窗口
    void LoaderForm_DragDrop(object sender, DragEventArgs e)
    {
        var paths = (string[])e.Data.GetData(DataFormats.FileDrop);

        var pathHash = new HashSet<string>();
        for (int i = 0; i < paths.Length; i++)
        {
            if (!paths[i].EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                continue;
            pathHash.Add(paths[i]);
        }

        if (pathHash.Count == 0)
            return;

        DllPath = textBox1.Text = pathHash.First();
        LoadDlls(pathHash);
    }

    /// <summary>
    /// 加载插件
    /// </summary>
    /// <param name="paths"></param>
    void LoadDlls(HashSet<string> paths)
    {
        if (paths.Count == 0)
            return;

        List<LoadState> ls = new();
        foreach (var item in paths)
            _ad.Load(item, ls);
        var msg = AssemblyDependent.PrintMessage(ls);
        if (msg != null)
            MessageBox.Show(msg, "加载完毕!");
        else
            MessageBox.Show("无任何信息", "加载出现问题!");
    }

    //鼠标样式修改
    void LoaderForm_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.All;
        else
            e.Effect = DragDropEffects.None;
    }

    void LoaderForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
            Button2_Click(sender, e);
    }
}

