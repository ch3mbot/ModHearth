namespace ModHearth.V2
{
    partial class MainForm
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
            rightModlistPanel = new VerticalFlowPanel();
            leftModlistPanel = new VerticalFlowPanel();
            modlistComboBox = new ComboBox();
            modlistColumnTableLayout = new TableLayoutPanel();
            outerTableLayout = new TableLayoutPanel();
            rightPanel = new Panel();
            refreshModsButton = new Button();
            playGameButton = new Button();
            undoChangesButton = new Button();
            saveButton = new Button();
            modInfoPanel = new TableLayoutPanel();
            modTitleLabel = new Label();
            modDescriptionLabel = new Label();
            modPictureBox = new PictureBox();
            renameListButton = new Button();
            deleteListButton = new Button();
            newListButton = new Button();
            importButton = new Button();
            exportButton = new Button();
            modlistColumnTableLayout.SuspendLayout();
            outerTableLayout.SuspendLayout();
            rightPanel.SuspendLayout();
            modInfoPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)modPictureBox).BeginInit();
            SuspendLayout();
            // 
            // rightModlistPanel
            // 
            rightModlistPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rightModlistPanel.AutoScroll = true;
            rightModlistPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rightModlistPanel.BackColor = Color.DodgerBlue;
            rightModlistPanel.Location = new Point(196, 3);
            rightModlistPanel.Name = "rightModlistPanel";
            rightModlistPanel.Size = new Size(187, 792);
            rightModlistPanel.TabIndex = 4;
            // 
            // leftModlistPanel
            // 
            leftModlistPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            leftModlistPanel.AutoScroll = true;
            leftModlistPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            leftModlistPanel.BackColor = Color.DodgerBlue;
            leftModlistPanel.Location = new Point(3, 3);
            leftModlistPanel.Name = "leftModlistPanel";
            leftModlistPanel.Size = new Size(187, 792);
            leftModlistPanel.TabIndex = 3;
            // 
            // modlistComboBox
            // 
            modlistComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            modlistComboBox.FormattingEnabled = true;
            modlistComboBox.Location = new Point(3, 49);
            modlistComboBox.Name = "modlistComboBox";
            modlistComboBox.Size = new Size(191, 23);
            modlistComboBox.TabIndex = 6;
            modlistComboBox.SelectedIndexChanged += modlistComboBox_SelectedIndexChanged;
            // 
            // modlistColumnTableLayout
            // 
            modlistColumnTableLayout.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            modlistColumnTableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            modlistColumnTableLayout.ColumnCount = 2;
            modlistColumnTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            modlistColumnTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            modlistColumnTableLayout.Controls.Add(leftModlistPanel, 0, 0);
            modlistColumnTableLayout.Controls.Add(rightModlistPanel, 1, 0);
            modlistColumnTableLayout.Location = new Point(651, 3);
            modlistColumnTableLayout.Name = "modlistColumnTableLayout";
            modlistColumnTableLayout.RowCount = 1;
            modlistColumnTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            modlistColumnTableLayout.Size = new Size(386, 798);
            modlistColumnTableLayout.TabIndex = 7;
            // 
            // outerTableLayout
            // 
            outerTableLayout.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            outerTableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            outerTableLayout.ColumnCount = 3;
            outerTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 648F));
            outerTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            outerTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            outerTableLayout.Controls.Add(rightPanel, 2, 0);
            outerTableLayout.Controls.Add(modlistColumnTableLayout, 1, 0);
            outerTableLayout.Controls.Add(modInfoPanel, 0, 0);
            outerTableLayout.Location = new Point(12, 12);
            outerTableLayout.Name = "outerTableLayout";
            outerTableLayout.RowCount = 1;
            outerTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            outerTableLayout.Size = new Size(1240, 804);
            outerTableLayout.TabIndex = 8;
            // 
            // rightPanel
            // 
            rightPanel.Controls.Add(exportButton);
            rightPanel.Controls.Add(importButton);
            rightPanel.Controls.Add(newListButton);
            rightPanel.Controls.Add(deleteListButton);
            rightPanel.Controls.Add(renameListButton);
            rightPanel.Controls.Add(refreshModsButton);
            rightPanel.Controls.Add(modlistComboBox);
            rightPanel.Controls.Add(playGameButton);
            rightPanel.Controls.Add(undoChangesButton);
            rightPanel.Controls.Add(saveButton);
            rightPanel.Location = new Point(1043, 3);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new Size(194, 798);
            rightPanel.TabIndex = 0;
            // 
            // refreshModsButton
            // 
            refreshModsButton.Location = new Point(150, 3);
            refreshModsButton.Name = "refreshModsButton";
            refreshModsButton.Size = new Size(43, 43);
            refreshModsButton.TabIndex = 10;
            refreshModsButton.Text = "R";
            refreshModsButton.UseVisualStyleBackColor = true;
            refreshModsButton.Click += refreshModsButton_Click;
            // 
            // playGameButton
            // 
            playGameButton.Location = new Point(101, 3);
            playGameButton.Name = "playGameButton";
            playGameButton.Size = new Size(43, 43);
            playGameButton.TabIndex = 9;
            playGameButton.Text = "P";
            playGameButton.UseVisualStyleBackColor = true;
            playGameButton.Click += playGameButton_Click;
            // 
            // undoChangesButton
            // 
            undoChangesButton.Location = new Point(52, 3);
            undoChangesButton.Name = "undoChangesButton";
            undoChangesButton.Size = new Size(43, 43);
            undoChangesButton.TabIndex = 8;
            undoChangesButton.Text = "U";
            undoChangesButton.UseVisualStyleBackColor = true;
            undoChangesButton.Click += undoChangesButton_Click;
            // 
            // saveButton
            // 
            saveButton.Location = new Point(3, 3);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(43, 43);
            saveButton.TabIndex = 7;
            saveButton.Text = "S";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += saveButton_Click;
            // 
            // modInfoPanel
            // 
            modInfoPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            modInfoPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            modInfoPanel.ColumnCount = 1;
            modInfoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            modInfoPanel.Controls.Add(modTitleLabel, 0, 0);
            modInfoPanel.Controls.Add(modDescriptionLabel, 0, 2);
            modInfoPanel.Controls.Add(modPictureBox, 0, 1);
            modInfoPanel.Location = new Point(3, 3);
            modInfoPanel.Name = "modInfoPanel";
            modInfoPanel.RowCount = 3;
            modInfoPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            modInfoPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 364F));
            modInfoPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            modInfoPanel.Size = new Size(642, 798);
            modInfoPanel.TabIndex = 8;
            // 
            // modTitleLabel
            // 
            modTitleLabel.Dock = DockStyle.Fill;
            modTitleLabel.Location = new Point(3, 0);
            modTitleLabel.Name = "modTitleLabel";
            modTitleLabel.Size = new Size(636, 42);
            modTitleLabel.TabIndex = 0;
            modTitleLabel.Text = "label1";
            // 
            // modDescriptionLabel
            // 
            modDescriptionLabel.Dock = DockStyle.Fill;
            modDescriptionLabel.Location = new Point(3, 406);
            modDescriptionLabel.Name = "modDescriptionLabel";
            modDescriptionLabel.Size = new Size(636, 392);
            modDescriptionLabel.TabIndex = 1;
            modDescriptionLabel.Text = "label2";
            // 
            // modPictureBox
            // 
            modPictureBox.Dock = DockStyle.Fill;
            modPictureBox.Location = new Point(3, 45);
            modPictureBox.Name = "modPictureBox";
            modPictureBox.Size = new Size(636, 358);
            modPictureBox.TabIndex = 2;
            modPictureBox.TabStop = false;
            // 
            // renameListButton
            // 
            renameListButton.Location = new Point(58, 78);
            renameListButton.Name = "renameListButton";
            renameListButton.Size = new Size(69, 23);
            renameListButton.TabIndex = 11;
            renameListButton.Text = "Rename";
            renameListButton.UseVisualStyleBackColor = true;
            renameListButton.Click += renameListButton_Click;
            // 
            // deleteListButton
            // 
            deleteListButton.Location = new Point(133, 78);
            deleteListButton.Name = "deleteListButton";
            deleteListButton.Size = new Size(61, 23);
            deleteListButton.TabIndex = 12;
            deleteListButton.Text = "Delete";
            deleteListButton.UseVisualStyleBackColor = true;
            deleteListButton.Click += deleteListButton_Click;
            // 
            // newListButton
            // 
            newListButton.Location = new Point(3, 78);
            newListButton.Name = "newListButton";
            newListButton.Size = new Size(49, 23);
            newListButton.TabIndex = 13;
            newListButton.Text = "New";
            newListButton.UseVisualStyleBackColor = true;
            newListButton.Click += newListButton_Click;
            // 
            // importButton
            // 
            importButton.Location = new Point(3, 107);
            importButton.Name = "importButton";
            importButton.Size = new Size(89, 23);
            importButton.TabIndex = 14;
            importButton.Text = "Import";
            importButton.UseVisualStyleBackColor = true;
            importButton.Click += importButton_Click;
            // 
            // exportButton
            // 
            exportButton.Location = new Point(100, 107);
            exportButton.Name = "exportButton";
            exportButton.Size = new Size(94, 23);
            exportButton.TabIndex = 15;
            exportButton.Text = "Export";
            exportButton.UseVisualStyleBackColor = true;
            exportButton.Click += exportButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1264, 828);
            Controls.Add(outerTableLayout);
            Name = "MainForm";
            Text = "MainForm";
            modlistColumnTableLayout.ResumeLayout(false);
            outerTableLayout.ResumeLayout(false);
            rightPanel.ResumeLayout(false);
            modInfoPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)modPictureBox).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private VerticalFlowPanel rightModlistPanel;
        private VerticalFlowPanel leftModlistPanel;
        private ComboBox modlistComboBox;
        private TableLayoutPanel modlistColumnTableLayout;
        private TableLayoutPanel outerTableLayout;
        private Panel rightPanel;
        private Button refreshModsButton;
        private Button playGameButton;
        private Button undoChangesButton;
        private Button saveButton;
        private TableLayoutPanel modInfoPanel;
        private Label modTitleLabel;
        private Label modDescriptionLabel;
        private PictureBox modPictureBox;
        private Button newListButton;
        private Button deleteListButton;
        private Button renameListButton;
        private Button exportButton;
        private Button importButton;
    }
}