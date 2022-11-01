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
            InputVar.Shift = true;
            return;
        }

        var str = InputHelper.GetValue(InputHelper.Section, "Shift切换");
        checkBox1_shift.Checked = InputVar.Shift = bool.Parse(str);
        str = InputHelper.GetValue(InputHelper.Section, "Ctrl切换");
        checkBox2_ctrl.Checked = InputVar.Ctrl = bool.Parse(str);
        str = InputHelper.GetValue(InputHelper.Section, "Ctrl+空格");
        checkBox4_ctrlAddSpace.Checked = InputVar.CtrlAndSpace = bool.Parse(str);
        str = InputHelper.GetValue(InputHelper.Section, "Ctrl+Shift");
        checkBox5_ctrlAndShift.Checked = InputVar.CtrlAndShift = bool.Parse(str);
        str = InputHelper.GetValue(InputHelper.Section, "Win+空格");
        checkBox6_winAndSpace.Checked = InputVar.WinAndSpace = bool.Parse(str);
        str = InputHelper.GetValue(InputHelper.Section, "去多余字母");
        checkBox3_切换后多余字母.Checked = InputVar.去多余字母 = bool.Parse(str);
        str = InputHelper.GetValue(InputHelper.Section, "增加字母");
        checkBox7_切换后首字母.Checked = InputVar.增加字母 = bool.Parse(str);
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
            InputVar.Shift = checkBox1_shift.Checked;
            InputVar.Ctrl = checkBox2_ctrl.Checked;
            InputVar.CtrlAndSpace = checkBox4_ctrlAddSpace.Checked;
            InputVar.CtrlAndShift = checkBox5_ctrlAndShift.Checked;
            InputVar.WinAndSpace = checkBox6_winAndSpace.Checked;
            InputVar.去多余字母 = checkBox3_切换后多余字母.Checked;
            InputVar.增加字母 = checkBox7_切换后首字母.Checked;
            InputHelper.WritePrivateProfileString(InputHelper.Section, "Shift切换", checkBox1_shift.Checked.ToString(), InputHelper.SectionFile);
            InputHelper.WritePrivateProfileString(InputHelper.Section, "Ctrl切换", checkBox2_ctrl.Checked.ToString(), InputHelper.SectionFile);
            InputHelper.WritePrivateProfileString(InputHelper.Section, "Ctrl+空格", checkBox4_ctrlAddSpace.Checked.ToString(), InputHelper.SectionFile);
            InputHelper.WritePrivateProfileString(InputHelper.Section, "Ctrl+Shift", checkBox5_ctrlAndShift.Checked.ToString(), InputHelper.SectionFile);
            InputHelper.WritePrivateProfileString(InputHelper.Section, "Win+空格", checkBox6_winAndSpace.Checked.ToString(), InputHelper.SectionFile);
            InputHelper.WritePrivateProfileString(InputHelper.Section, "去多余字母", checkBox3_切换后多余字母.Checked.ToString(), InputHelper.SectionFile);
            InputHelper.WritePrivateProfileString(InputHelper.Section, "增加字母", checkBox7_切换后首字母.Checked.ToString(), InputHelper.SectionFile);

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
        if (!checkBox1_shift.Checked)
            return;
        checkBox2_ctrl.Checked = false;
        checkBox4_ctrlAddSpace.Checked = false;
        checkBox5_ctrlAndShift.Checked = false;
        checkBox6_winAndSpace.Checked = false;
    }
    /// <summary>
    /// Ctrl切换中英文
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox2_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox2_ctrl.Checked)
            return;
        checkBox1_shift.Checked = false;
        checkBox4_ctrlAddSpace.Checked = false;
        checkBox5_ctrlAndShift.Checked = false;
        checkBox6_winAndSpace.Checked = false;
    }
    /// <summary>
    /// 消除切换后命令行多余字母
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox3_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox3_切换后多余字母.Checked)
            return;
        checkBox7_切换后首字母.Checked = false;
    }
    /// <summary>
    /// Ctrl+空格
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox4_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox4_ctrlAddSpace.Checked)
            return;
        checkBox1_shift.Checked =
        checkBox2_ctrl.Checked =
        checkBox5_ctrlAndShift.Checked =
        checkBox6_winAndSpace.Checked = false;
    }
    /// <summary>
    /// Ctrl+Shift
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox5_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox5_ctrlAndShift.Checked)
            return;
        checkBox1_shift.Checked =
        checkBox2_ctrl.Checked =
        checkBox4_ctrlAddSpace.Checked =
        checkBox6_winAndSpace.Checked = false;
    }
    /// <summary>
    /// Win+空格
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox6_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox6_winAndSpace.Checked)
            return;
        checkBox1_shift.Checked =
        checkBox2_ctrl.Checked =
        checkBox4_ctrlAddSpace.Checked =
        checkBox5_ctrlAndShift.Checked = false;
    }
    /// <summary>
    /// 解决切换后需多按一次命令首字母
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox7_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkBox7_切换后首字母.Checked)
            return;
        checkBox3_切换后多余字母.Checked = false;
    }
}