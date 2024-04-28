namespace PromptLab;

partial class MainForm
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        refreshButton = new Button();
        SuspendLayout();
        // 
        // refreshButton
        // 
        refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        refreshButton.Image = (Image)resources.GetObject("refreshButton.Image");
        refreshButton.Location = new Point(1283, -2);
        refreshButton.Name = "refreshButton";
        refreshButton.Size = new Size(41, 29);
        refreshButton.TabIndex = 0;
        refreshButton.UseVisualStyleBackColor = true;
        refreshButton.Click += RefreshButtonClick;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1326, 900);
        Controls.Add(refreshButton);
        Name = "MainForm";
        Text = "Prompt Lab";
        WindowState = FormWindowState.Maximized;
        Click += RefreshButtonClick;
        ResumeLayout(false);
    }

    #endregion

    private Button refreshButton;
}
