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
            modlistComboBox.SelectedIndex = modlistComboBox.Items.IndexOf(manager.selectedModlist.name);

            Console.WriteLine("Form loaded");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedOption = modlistComboBox.SelectedItem.ToString();
            manager.SetSelectedModlist(selectedOption);
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