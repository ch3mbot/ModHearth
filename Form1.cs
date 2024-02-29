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

        private void RefreshSizeProperly()
        {
            Console.WriteLine("Form refreshed");
            int w = this.ClientSize.Width; int h = this.ClientSize.Height;
            int availableMidSpace = w - 2 * Style.largeBorder - 3 * Style.smallBorder - Style.rightPanelW - Style.leftPanelW;
            int panelW = availableMidSpace / 2;
            int panelLX = Style.largeBorder + Style.smallBorder + Style.leftPanelW;
            int availableHeight = h - 2 * Style.largeBorder;
            int rightPanelX = panelLX + availableMidSpace + Style.smallBorder;

            leftModlistPanel.Width = panelW;
            leftModlistPanel.Height = availableHeight;
            rightModlistPanel.Width = panelW;
            rightModlistPanel.Height = availableHeight;
            leftModlistPanel.Location = new Point(panelLX, Style.largeBorder);
            rightModlistPanel.Location = new Point(panelLX + Style.smallBorder + panelW, Style.largeBorder);
            leftModlistPanel.BackColor = Color.Blue;
            rightModlistPanel.BackColor = Color.Blue;


            modlistComboBox.Width = Style.rightPanelW;
            modlistComboBox.Location = new Point(rightPanelX, Style.largeBorder);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshSizeProperly();

            //set which modlist is enabled right now
            foreach (DFHackModlist dfhackModlist in manager.modlists)
            {
                modlistComboBox.Items.Add(dfhackModlist.name);
            }
            modlistComboBox.SelectedIndex = manager.selectedModlistIndex;

            Console.WriteLine("Form loaded");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private bool UnsavedChangesPrompt(string message) 
        {            
            if (!manager.ModpackAltered)
                return true;

            //ask to save changes first
            DialogResult result = MessageBox.Show($"You have unsaved changes to {manager.selectedModlist.name}. Do you want to save before {message}?", "Confirmation", MessageBoxButtons.YesNoCancel);

            if (result == DialogResult.Yes)
            {
                //#fix# console color message function
                SaveChanges();
                return true;
            }
            else if (result == DialogResult.No) 
            {
                DiscardChanges();
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Undoing action");
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
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
            manager.ResetModlist();

            //remove unsaved changes star
            ResetChanges();
        }

        private void ResetChanges() 
        {
            manager.ModpackAltered = false;
            visualChangesMadeFlag = false;
            manualIndexChangeFlag = true;
            modlistComboBox.Items[currentModlistIndex] = manager.selectedModlist.name;
        }

        public void ShowUnsavedChanges() 
        {
            //we make visual changes, and then mark that the index has been modified
            if(!visualChangesMadeFlag) 
            {
                visualChangesMadeFlag = true;
                manualIndexChangeFlag = true;
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
            if(!UnsavedChangesPrompt("Changing modpack")) 
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
            RefreshModColumn(leftModlistPanel, new List<ModReference>(manager.disabledMods));
            RefreshModColumn(rightModlistPanel, manager.enabledMods);
        }

        private void RefreshModColumn(Panel col, List<ModReference> modrefList)
        {
            col.Controls.Clear();
            foreach (ModReference modref in modrefList)
            {
                col.Controls.Add(new DraggableModRef(modref, col, this));
            }
        }


        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void flowLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

    }
}