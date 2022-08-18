namespace IFoxCAD.LoadEx;

using System.Windows.Forms;

public partial class LoaderForm : Form
{
    string? _dllPath;
    AssemblyDependent _ad;
    public LoaderForm()
    {
        //Owner = form;
        //MdiParent = form;
        StartPosition = FormStartPosition.CenterScreen;//在当前屏幕中央
        InitializeComponent();
        _ad = new AssemblyDependent();
    }

    void LoaderForm_Load(object sender, EventArgs e)
    {
        // if (_dllPath != null)
        //     textBox1.Text = _dllPath;
#if NET35
        textBox1.Text = "G:\\K01.惊惊连盒\\net35\\JoinBoxAcad.dll";
#else
        textBox1.Text = "G:\\K01.惊惊连盒\\net48\\JoinBoxAcad.dll";
#endif

    }

    string? LoadDll(string path)
    {
        var ls = new List<LoadState>();
        _ad.Load(path, ls);
        return AssemblyDependent.PrintMessage(ls);
    }

    void TextBox1_TextChanged(object sender, EventArgs e)
    {
        _dllPath = textBox1.Text;
        if (string.IsNullOrEmpty(_dllPath?.Trim()))
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

        var msg = LoadDll(textBox1.Text);
        if (msg != null)
            MessageBox.Show(msg, "加载完毕!");
        else
            MessageBox.Show("无任何信息", "加载出现问题!");
    }

    void LoaderForm_DragDrop(object sender, DragEventArgs e)
    {
        int i = 0;
        var dllPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
        foreach (var item in dllPaths)
            if (item.EndsWith(".dll"))
            {
                LoadDll(item);
                i++;
                if (i == 1)
                {
                    textBox1.Text = item;
                    _dllPath = item;
                }
            }
        MessageBox.Show("加载完毕!");
    }

    void LoaderForm_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.All;
        else
            e.Effect = DragDropEffects.None;
    }

    private void Button2_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
            Button2_Click(sender, e);
    }
}

