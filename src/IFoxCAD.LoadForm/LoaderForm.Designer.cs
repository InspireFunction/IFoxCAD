namespace IFoxCAD.LoadEx;

partial class LoaderForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.loadDlldialog = new System.Windows.Forms.OpenFileDialog();
        this.textBox1 = new System.Windows.Forms.TextBox();
        this.button1 = new System.Windows.Forms.Button();
        this.label1 = new System.Windows.Forms.Label();
        this.button2 = new System.Windows.Forms.Button();
        this.label2 = new System.Windows.Forms.Label();
        this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
        this.SuspendLayout();
        // 
        // textBox1
        // 
        this.textBox1.Location = new System.Drawing.Point(88, 35);
        this.textBox1.Name = "textBox1";
        this.textBox1.ReadOnly = true;
        this.textBox1.Size = new System.Drawing.Size(634, 34);
        this.textBox1.TabIndex = 0;
        this.textBox1.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
        // 
        // button1
        // 
        this.button1.AutoSize = true;
        this.button1.Location = new System.Drawing.Point(743, 30);
        this.button1.Name = "button1";
        this.button1.Size = new System.Drawing.Size(177, 42);
        this.button1.TabIndex = 1;
        this.button1.Text = "手动选择DLL文件";
        this.button1.UseVisualStyleBackColor = true;
        this.button1.Click += new System.EventHandler(this.Button1_Click);
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.label1.ForeColor = System.Drawing.Color.Red;
        this.label1.Location = new System.Drawing.Point(25, 97);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(428, 27);
        this.label1.TabIndex = 2;
        this.label1.Text = "直接拖拽DLL文件到本窗体可自动加载DLL文件";
        // 
        // button2
        // 
        this.button2.AutoSize = true;
        this.button2.Location = new System.Drawing.Point(743, 89);
        this.button2.Name = "button2";
        this.button2.Size = new System.Drawing.Size(177, 42);
        this.button2.TabIndex = 1;
        this.button2.Text = "加载";
        this.button2.UseVisualStyleBackColor = true;
        this.button2.Click += new System.EventHandler(this.Button2_Click);
        // 
        // label2
        // 
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(25, 38);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(57, 27);
        this.label2.TabIndex = 3;
        this.label2.Text = "路径:";
        // 
        // toolTip1
        // 
        this.toolTip1.AutomaticDelay = 200;
        this.toolTip1.AutoPopDelay = 3000;
        this.toolTip1.InitialDelay = 200;
        this.toolTip1.ReshowDelay = 40;
        this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
        // 
        // LoaderForm
        // 
        this.AllowDrop = true;
        this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 27F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(944, 160);
        this.Controls.Add(this.label2);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.button2);
        this.Controls.Add(this.button1);
        this.Controls.Add(this.textBox1);
        this.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "LoaderForm";
        this.Text = ".net dll 加载器";
        this.Load += new System.EventHandler(this.LoaderForm_Load);
        this.DragDrop += new System.Windows.Forms.DragEventHandler(this.LoaderForm_DragDrop);
        this.DragEnter += new System.Windows.Forms.DragEventHandler(this.LoaderForm_DragEnter);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.OpenFileDialog loadDlldialog;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ToolTip toolTip1;
}