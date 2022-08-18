namespace IFoxCAD.LoadEx;

using System.Windows.Forms;

public partial class LoaderForm : Form
{
    string? _dllPath;
    public LoaderForm()
    {
        InitializeComponent();
    }

    static string? LoadDll(string path)
    {
        //var ed = Acap.DocumentManager.MdiActiveDocument.Editor;
        var ad = new AssemblyDependent();
        var ls = new List<LoadState>();
        ad.Load(path, ls);
        //ed.WriteMessage(AssemblyDependent.PrintMessage(ls));
        return AssemblyDependent.PrintMessage(ls);
    }

    void TextBox1_TextChanged(object sender, EventArgs e)
    {
        _dllPath = textBox1.Text;
        toolTip1.SetToolTip(textBox1, Path.GetFullPath(textBox1.Text));
    }

    void LoaderForm_Load(object sender, EventArgs e)
    {
        if (_dllPath != null)
            textBox1.Text = _dllPath;
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

        LoadDll(textBox1.Text);
        MessageBox.Show("加载完毕!");
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
}

