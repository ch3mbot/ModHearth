using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace ModHearth
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public ModHearthManager manager;

        public Form1()
        {
            AllocConsole();
            Console.ForegroundColor = ConsoleColor.White;
            InitializeComponent();
            manager = new ModHearthManager(this);
        }

        public Panel LeftModlistPanel => leftModlistPanel;
        public Panel RightModlistPanel => rightModlistPanel;
        private bool manualIndexChangeFlag = false;

        public int currentModlistIndex;
        public bool visualChangesMadeFlag = false;

        private Button saveButton, unchangeButton, reloadButton;

        private Dictionary<string, DraggableModRef> DragModRefMap;

        private TextBox leftSearch, rightSearch;

        private void RefreshSizeProperly()
        {
            Console.WriteLine("Form refreshed");
            //area math
            int w = this.ClientSize.Width; int h = this.ClientSize.Height;
            int availableMidSpace = w - 2 * Style.largeBorder - 3 * Style.smallBorder - Style.rightPanelW - Style.leftPanelW;
            int panelW = availableMidSpace / 2;
            int panelLX = Style.largeBorder + Style.smallBorder + Style.leftPanelW;
            int availableHeight = h - 2 * Style.largeBorder;
            int rightPanelX = panelLX + availableMidSpace + Style.smallBorder;

            //mod list columns
            leftModlistPanel.Width = panelW;
            leftModlistPanel.Height = availableHeight;
            rightModlistPanel.Width = panelW;
            rightModlistPanel.Height = availableHeight;
            leftModlistPanel.Location = new Point(panelLX, Style.largeBorder);
            rightModlistPanel.Location = new Point(panelLX + Style.smallBorder + panelW, Style.largeBorder);
            leftModlistPanel.BackColor = Color.Blue;
            rightModlistPanel.BackColor = Color.Blue;

            //right panel
            int btnWidth = (Style.rightPanelW - 2 * Style.smallBorder) / 3;
            int btnHeight = btnWidth;
            saveButton.Size = unchangeButton.Size = reloadButton.Size = new Size(btnWidth, btnHeight);
            saveButton.Location = new Point(rightPanelX, Style.largeBorder);
            unchangeButton.Location = new Point(rightPanelX + btnWidth + Style.smallBorder, Style.largeBorder);
            reloadButton.Location = new Point(rightPanelX + 2 * (btnWidth + Style.smallBorder), Style.largeBorder);

            modlistComboBox.Width = Style.rightPanelW;
            modlistComboBox.Location = new Point(rightPanelX, Style.smallBorder + btnHeight + Style.largeBorder);

            //for some reason columns show up blank initially
            RefreshModColumns();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AddMissingElements();
            GenerateDraggableModRefs();
            RefreshSizeProperly();

            //set which modlist is enabled right now
            foreach (DFHackModlist dfhackModlist in manager.actualModLists)
            {
                modlistComboBox.Items.Add(dfhackModlist.name);
            }
            modlistComboBox.SelectedIndex = manager.selectedModlistIndex;



            Console.WriteLine("Form loaded");
        }

        private void GenerateDraggableModRefs() 
        {
            DragModRefMap = new Dictionary<string, DraggableModRef>();
            foreach(string key in manager.allMods.Keys) 
            {
                ModReference modref = manager.allMods[key];
                DraggableModRef newDragRef = new DraggableModRef(modref, this);
                modref.matchingControl = newDragRef;
                DragModRefMap.Add(modref.ToDFHackMod().ToString(), newDragRef);
            }
        }

        private void AddMissingElements() 
        {
            saveButton = new Button();
            saveButton.Text = "S";
            saveButton.Click += saveButton_Clicked;
            
            unchangeButton = new Button();
            unchangeButton.Text = "U";
            unchangeButton.Click += unchangeButton_Clicked;
            unchangeButton.Enabled = false;

            reloadButton = new Button();
            reloadButton.Text = "R";
            reloadButton.Click += reloadButton_Clicked;

            Controls.Add(saveButton);
            Controls.Add(unchangeButton);
            Controls.Add(reloadButton);

            leftSearch = new TextBox();
            rightSearch = new TextBox();

            leftSearch.TextChanged += leftSearch_changed;
            rightSearch.TextChanged += rightSearch_changed;
        }

        private void leftSearch_changed(object sender, EventArgs e) 
        {

        }

        private void rightSearch_changed(object sender, EventArgs e) 
        {

        }

        private void saveButton_Clicked(object sender, EventArgs e) 
        {
            SaveChanges();
        }
        private void unchangeButton_Clicked(object sender, EventArgs e) 
        {
            if(ResetChangesPrompt("Are you sure you want to reset changes?"))
            {
                DiscardChanges();
                RefreshModColumns();
                //manager.SetSelectedModlist(manager.selectedModlistIndex);
            }
            else 
            {
                Console.WriteLine("!!changes left intact");
            }
        }
        private void reloadButton_Clicked(object sender, EventArgs e) 
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private bool ResetChangesPrompt(string questionMessage) 
        {
            DialogResult result = MessageBox.Show($"You have unsaved changes to {manager.lastSelectedModlist.name}. {questionMessage}", "Confirmation", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                return true;
            }
            return false;
            
        }

        private bool UnsavedChangesPrompt(string questionMessage) 
        {            
            if (!manager.ModpackAltered)
                return true;

            //ask to save changes first
            DialogResult result = MessageBox.Show($"You have unsaved changes to {manager.lastSelectedModlist.name}. {questionMessage}", "Confirmation", MessageBoxButtons.YesNoCancel);

            if (result == DialogResult.Yes)
            {
                //#fix# console color message function
                SaveChanges();
                return true;
            }
            if (result == DialogResult.No) 
            {
                DiscardChanges();
                return true;
            }

            //they hit cancel, so the action can't procees
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Undoing action");
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        public void SaveChanges() 
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Saving changes");
            Console.ForegroundColor = ConsoleColor.White;
            manager.SaveModlist();

            //remove unsaved changes star
            ResetChanges();
        }

        public void DiscardChanges()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Discarding changes");
            Console.ForegroundColor = ConsoleColor.White;
            manager.SetSelectedModlist(manager.selectedModlistIndex);

            //remove unsaved changes star
            ResetChanges();
        }

        private void ResetChanges() 
        {
            manager.ModpackAltered = false;
            visualChangesMadeFlag = false;
            manualIndexChangeFlag = true;
            unchangeButton.Enabled = false;
            modlistComboBox.Items[currentModlistIndex] = manager.lastSelectedModlist.name;
        }

        public void ShowUnsavedChanges() 
        {
            //we make visual changes, and then mark that the index has been modified
            if(!visualChangesMadeFlag) 
            {
                visualChangesMadeFlag = true;
                manualIndexChangeFlag = true;
                unchangeButton.Enabled = true;
                modlistComboBox.Items[currentModlistIndex] += "*";
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("index change attempt to: " + modlistComboBox.SelectedIndex);
            //if name was altered by us do not run prompt. needed since index change happens when name change
            if (manualIndexChangeFlag) {
                manualIndexChangeFlag = false;
                return;
            }
            //if we have unsaved changes, and the prompt says we can't continue, then reset index
            if(!UnsavedChangesPrompt("Do you want to save before changing modpack?")) 
            {
                manualIndexChangeFlag = true;
                modlistComboBox.SelectedIndex = currentModlistIndex;
                return;
            }
            //if we did change index, 
            currentModlistIndex = modlistComboBox.SelectedIndex;
            Console.WriteLine($"modlist changed to {currentModlistIndex}: {modlistComboBox.Items[currentModlistIndex]}");
            manager.SetSelectedModlist(currentModlistIndex);
            RefreshModColumns();
        }

        private void RefreshModColumns()
        {
            RefreshModColumn(leftModlistPanel, new List<ModReference>(manager.disabledMods), true);
            RefreshModColumn(rightModlistPanel, manager.enabledMods, false);
        }

        private void RefreshModColumn(VerticalFlowPanel col, List<ModReference> modrefList, bool left)
        {
            if (col.initialized)
            {
                col.UpdateVisibleOrder(!left);
                return;
            }
            List<Control> conts = new List<Control>();
            foreach (ModReference modref in modrefList)
            {
                DraggableModRef selected = DragModRefMap[modref.ToDFHackMod().ToString()];
                selected.Initialize(col);
                conts.Add(selected);
            }
            col.Controls.AddRange(conts.ToArray());
            col.ResumeLayout(true);
        }


        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void flowLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

    }
}