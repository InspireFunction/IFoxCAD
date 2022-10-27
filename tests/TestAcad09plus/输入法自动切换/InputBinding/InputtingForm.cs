namespace InputBinding;
using System.Windows.Forms;

public partial class InputtingForm : Form
{
    public InputtingForm()
    {
        InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        if (!File.Exists(InputHelper.SectionFile))
        {
            InputVar.SF = true;
            return;
        }

        var str = InputHelper.GetValue(InputHelper.Section, "Shift切换");
        checkBox1.Checked = InputVar.SF = bool.Parse(str);
        str = InputHelper.GetValue(InputHelper.Section, "Ctrl切换");
        checkBox2.Checked = InputVar.CT = bool.Parse(str);
        str = InputHelper.GetValue(InputHelper.Section, "Ctrl+空格");
        checkBox4.Checked = InputVar.CK = bool.Parse(str);
        str = InputHelper.GetValue(InputHelper.Section, "Ctrl+Shift");
        checkBox5.Checked = InputVar.CS = bool.Parse(str);
        str = InputHelper.GetValue(InputHelper.Section, "Win+空格");
        checkBox6.Checked = InputVar.AS = bool.Parse(str);
        str = InputHelper.GetValue(InputHelper.Section, "去多余字母");
        checkBox3.Checked = InputVar.DY = bool.Parse(str);
        str = InputHelper.GetValue(InputHelper.Section, "增加字母");
        checkBox7.Checked = InputVar.SL = bool.Parse(str);
    }
    /// <summary>
    /// 保存配置并退出
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button1_Click(object sender, EventArgs e)
    {
        try
        {
            InputVar.SF = checkBox1.Checked;
            InputVar.CT = checkBox2.Checked;
            InputVar.DY = checkBox3.Checked;
            InputVar.CK = checkBox4.Checked;
            InputVar.CS = checkBox5.Checked;
            InputVar.AS = checkBox6.Checked;
            InputVar.SL = checkBox7.Checked;
            InputHelper.WritePrivateProfileString(InputHelper.Section, "Shift切换", checkBox1.Checked.ToString(), InputHelper.SectionFile);
            InputHelper.WritePrivateProfileString(InputHelper.Section, "Ctrl切换", checkBox2.Checked.ToString(), InputHelper.SectionFile);
            InputHelper.WritePrivateProfileString(InputHelper.Section, "Ctrl+空格", checkBox4.Checked.ToString(), InputHelper.SectionFile);
            InputHelper.WritePrivateProfileString(InputHelper.Section, "Ctrl+Shift", checkBox5.Checked.ToString(), InputHelper.SectionFile);
            InputHelper.WritePrivateProfileString(InputHelper.Section, "Win+空格", checkBox6.Checked.ToString(), InputHelper.SectionFile);
            InputHelper.WritePrivateProfileString(InputHelper.Section, "去多余字母", checkBox3.Checked.ToString(), InputHelper.SectionFile);
            InputHelper.WritePrivateProfileString(InputHelper.Section, "增加字母", checkBox7.Checked.ToString(), InputHelper.SectionFile);

            if (InputHelper.SectionFile != null)
                MessageBox.Show("保存成功");
        }
        catch
        {
            MessageBox.Show("保存失败");
        }
        Close();
    }
    /// <summary>
    /// 临时关闭自动切换
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button2_Click(object sender, EventArgs e)
    {
        Inputting.IFoxInput();
    }
    /// <summary>
    /// 打开自动切换
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button3_Click(object sender, EventArgs e)
    {
        Inputting.IFoxInput();
    }

    /// <summary>
    /// Shift切换中英文(默认)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox1_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox1.Checked)
            return;
        checkBox2.Checked = false;
        checkBox4.Checked = false;
        checkBox5.Checked = false;
        checkBox6.Checked = false;
    }
    /// <summary>
    /// Ctrl切换中英文
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox2_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox2.Checked)
            return;
        checkBox1.Checked = false;
        checkBox4.Checked = false;
        checkBox5.Checked = false;
        checkBox6.Checked = false;
    }
    /// <summary>
    /// 消除切换后命令行多余字母
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox3_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox3.Checked)
            return;
        checkBox7.Checked = false;
    }
    /// <summary>
    /// Ctrl+空格
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox4_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox4.Checked)
            return;
        checkBox1.Checked =
        checkBox2.Checked =
        checkBox5.Checked =
        checkBox6.Checked = false;
    }
    /// <summary>
    /// Ctrl+Shift
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox5_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox5.Checked)
            return;
        checkBox1.Checked =
        checkBox2.Checked =
        checkBox4.Checked =
        checkBox6.Checked = false;
    }
    /// <summary>
    /// Win+空格
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox6_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox6.Checked)
            return;
        checkBox1.Checked =
        checkBox2.Checked =
        checkBox4.Checked =
        checkBox5.Checked = false;
    }
    /// <summary>
    /// 解决切换后需多按一次命令首字母
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox7_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox7.Checked)
            return;
        checkBox3.Checked = false;
    }
}