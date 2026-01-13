namespace Sample.WinForms;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        zenithView = new Zenith.NET.Views.WinForms.ZenithView();
        comboBox = new ComboBox();
        SuspendLayout();
        // 
        // zenithView
        // 
        zenithView.Dock = DockStyle.Fill;
        zenithView.Location = new Point(0, 0);
        zenithView.Name = "zenithView";
        zenithView.Size = new Size(1484, 850);
        zenithView.TabIndex = 0;
        // 
        // comboBox
        // 
        comboBox.Dock = DockStyle.Right;
        comboBox.FormattingEnabled = true;
        comboBox.Location = new Point(1364, 0);
        comboBox.Margin = new Padding(0);
        comboBox.Name = "comboBox";
        comboBox.Size = new Size(120, 32);
        comboBox.TabIndex = 1;
        comboBox.SelectedIndexChanged += OnSelectedIndexChanged;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(11F, 24F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1484, 850);
        Controls.Add(comboBox);
        Controls.Add(zenithView);
        Name = "Form1";
        Text = "Form1";
        ResumeLayout(false);
    }

    #endregion

    private Zenith.NET.Views.WinForms.ZenithView zenithView;
    private ComboBox comboBox;
}
