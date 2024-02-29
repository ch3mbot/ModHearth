namespace ModHearth
{
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
            pictureBox1 = new PictureBox();
            leftModlistPanel = new VerticalFlowPanel();
            rightModlistPanel = new VerticalFlowPanel();
            modlistComboBox = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(12, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(636, 358);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // leftModlistPanel
            // 
            leftModlistPanel.AutoScroll = true;
            leftModlistPanel.Location = new Point(654, 12);
            leftModlistPanel.Name = "leftModlistPanel";
            leftModlistPanel.Size = new Size(172, 100);
            leftModlistPanel.TabIndex = 1;
            leftModlistPanel.Paint += flowLayoutPanel1_Paint;
            // 
            // rightModlistPanel
            // 
            rightModlistPanel.AutoScroll = true;
            rightModlistPanel.Location = new Point(832, 12);
            rightModlistPanel.Name = "rightModlistPanel";
            rightModlistPanel.Size = new Size(185, 100);
            rightModlistPanel.TabIndex = 2;
            rightModlistPanel.Paint += flowLayoutPanel2_Paint;
            // 
            // modlistComboBox
            // 
            modlistComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            modlistComboBox.FormattingEnabled = true;
            modlistComboBox.Location = new Point(1131, 12);
            modlistComboBox.Name = "modlistComboBox";
            modlistComboBox.Size = new Size(121, 23);
            modlistComboBox.TabIndex = 3;
            modlistComboBox.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1264, 681);
            Controls.Add(modlistComboBox);
            Controls.Add(rightModlistPanel);
            Controls.Add(leftModlistPanel);
            Controls.Add(pictureBox1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox1;
        private VerticalFlowPanel leftModlistPanel;
        private VerticalFlowPanel rightModlistPanel;
        private ComboBox modlistComboBox;
    }
}