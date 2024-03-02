using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModHearth.V2
{
    public partial class MainForm : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public static MainForm instance;

        public ModHearthManager2 manager;

        public VerticalFlowPanel LeftModlistPanel => leftModlistPanel;
        public VerticalFlowPanel RightModlistPanel => rightModlistPanel;

        private bool changesMade;
        private bool changesMarked = false;
        private int lastIndex;

        public MainForm()
        {
            AllocConsole();
            Console.ForegroundColor = ConsoleColor.White;

            instance = this;
            InitializeComponent();
            manager = new ModHearthManager2();

            refreshModsButton.Enabled = false;

            SetChangesMade(false);

            SetupModlistBox();
            GenerateModrefControls();
        }

        private bool alteredModlistBox = false;
        private void SetupModlistBox()
        {
            foreach (DFHackModlist m in manager.modlists)
            {
                modlistComboBox.Items.Add(m.name);
            }
            alteredModlistBox = true;
            modlistComboBox.SelectedIndex = manager.selectedModlistIndex;
            lastIndex = manager.selectedModlistIndex;
            alteredModlistBox = false;
        }

        private void GenerateModrefControls()
        {
            GenerateModrefControlSided(leftModlistPanel, new List<DFHackMod>(manager.unactiveMods), true);
            GenerateModrefControlSided(rightModlistPanel, new List<DFHackMod>(manager.activeMods), false);
        }
        private void GenerateModrefControlSided(VerticalFlowPanel panel, List<DFHackMod> members, bool left)
        {
            //generate draggable references
            List<ModrefPanel> conts = new List<ModrefPanel>();
            foreach (DFHackMod dfm in manager.modPool)
            {
                conts.Add(new ModrefPanel(manager.GetModRef(dfm.ToString()), this));
            }

            panel.Initialize(conts, members, !left);
        }

        public void ModrefMouseMove(Point position, ModrefPanel modrefPanel)
        {
            //undo old highlights
            UnsetSurroundingToHighlight();

            //see if mouse is in panel
            bool overLeft = LeftModlistPanel.GetIndexAtPosition(position, out int indexL);
            bool overRight = rightModlistPanel.GetIndexAtPosition(position, out int indexR);

            //return if not in panel
            if (!overLeft && !overRight)
            {
                return;
            }

            //assume left, swap right if 
            int index = indexL;
            VerticalFlowPanel panel = LeftModlistPanel;

            if (overRight)
            {
                index = indexR;
                panel = RightModlistPanel;
            }

            //highlight
            SetSurroundingToHighlight(index, panel);

        }

        public void ModrefMouseUp(Point position, ModrefPanel modrefPanel)
        {
            UnsetSurroundingToHighlight();

            bool overLeft = LeftModlistPanel.GetIndexAtPosition(position, out int indexL);
            bool overRight = rightModlistPanel.GetIndexAtPosition(position, out int indexR);

            //return if not in panel
            if (!overLeft && !overRight)
            {
                return;
            }

            int index = indexL;
            VerticalFlowPanel destinationPanel = leftModlistPanel;

            if (overRight)
            {
                index = indexR;
                destinationPanel = rightModlistPanel;
            }
            SetChangesMade(true);
            MarkChanges(modlistComboBox.SelectedIndex);
            manager.MoveMod(modrefPanel.modref, index, modrefPanel.vParent == leftModlistPanel, destinationPanel == leftModlistPanel);
            RefreshModlistPanels();
        }

        private void MarkChanges(int index)
        {
            if (changesMarked)
                return;
            modlistComboBox.Items[index] = modlistComboBox.Items[index].ToString() + "*";
            changesMarked = true;
        }

        private void UnmarkChanges(int index)
        {
            string currstr = modlistComboBox.Items[index].ToString();
            modlistComboBox.Items[index] = currstr.Substring(0, currstr.Length - 1);
            changesMarked = false;
        }

        private void RefreshModlistPanels()
        {
            leftModlistPanel.UpdateVisibleOrder(new List<DFHackMod>(manager.unactiveMods));
            RightModlistPanel.UpdateVisibleOrder(manager.activeMods);
        }

        private HashSet<Control> highlightAffected = new HashSet<Control>();
        private void SetSurroundingToHighlight(int index, VerticalFlowPanel parentPanel)
        {
            if (index > 0)
            {
                Control selected = parentPanel.GetVisibleControlAtIndex(index - 1);
                selected.BackgroundImage = Resource1.highlight_bottom;
                highlightAffected.Add(selected);
            }
            if (index < parentPanel.visibleChildren.Count)
            {
                Control selected = parentPanel.GetVisibleControlAtIndex(index);
                selected.BackgroundImage = Resource1.highlight_top;
                highlightAffected.Add(selected);
            }
        }

        private void SetChangesMade(bool changesMade)
        {
            this.changesMade = changesMade;
            undoChangesButton.Enabled = changesMade;
            renameListButton.Enabled = !changesMade;

        }

        private void UnsetSurroundingToHighlight()
        {
            foreach (Control c in highlightAffected)
            {
                c.BackgroundImage = null;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SetChangesMade(false);
            SaveLists();
        }

        private void undoChangesButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to reset modlist changes?", "Undo changes", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                UndoListChanges();
            }
        }

        private void playGameButton_Click(object sender, EventArgs e)
        {
            //#fix# run df with dfhack
        }

        private void refreshModsButton_Click(object sender, EventArgs e)
        {
            //rebuild entire form? unknonwn
        }

        private void SaveLists()
        {
            manager.SaveCurrentModlist();

            UndoListChanges();
        }

        private void UndoListChanges()
        {
            manager.SetSelectedModlist(modlistComboBox.SelectedIndex);
            RefreshModlistPanels();

            SetChangesMade(false);
            UnmarkChanges(lastIndex);

        }

        private void modlistComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            //if we tripped this by altering an entry do nothing
            if (alteredModlistBox)
            {
                return;
            }

            Console.WriteLine($"changed from index {lastIndex} to {modlistComboBox.SelectedIndex}");

            //if the modlist didn't change do nothing
            if (lastIndex == modlistComboBox.SelectedIndex)
            {
                return;
            }

            if (changesMade)
            {
                DialogResult result = MessageBox.Show($"You have unsaved changes to {manager.SelectedModlist.name}, do you want to save before continuing?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    SaveLists();
                }
                else if (result == DialogResult.No)
                {
                    UndoListChanges();
                }
                else
                {
                    //if cancel, set index to last
                    alteredModlistBox = true;
                    modlistComboBox.SelectedIndex = lastIndex;
                    alteredModlistBox = false;
                }
            }
            else
            {
                manager.SetSelectedModlist(modlistComboBox.SelectedIndex);
                RefreshModlistPanels();
            }

            lastIndex = modlistComboBox.SelectedIndex;
        }

        private void newListButton_Click(object sender, EventArgs e)
        {
            DFHackModlist newList = new DFHackModlist();
            newList.name = Interaction.InputBox("Please enter a name for the new modpack", "New Modpack Name", "");
            if (string.IsNullOrEmpty(newList.name))
                return;
            newList.@default = false;
            newList.modlist = new List<DFHackMod>();
            manager.modlists.Add(newList);

            alteredModlistBox = true;
            modlistComboBox.Items.Add(newList.name);
            modlistComboBox.SelectedIndex = modlistComboBox.Items.Count - 1;

            MarkChanges(modlistComboBox.SelectedIndex);
            manager.SetSelectedModlist(modlistComboBox.SelectedIndex);
            RefreshModlistPanels();
            SetChangesMade(true);
            undoChangesButton.Enabled = false;
            alteredModlistBox = false;
        }

        private void renameListButton_Click(object sender, EventArgs e)
        {
            string newName = Interaction.InputBox("Please enter a new name for the modpack", "New Modpack Name", manager.SelectedModlist.name);
            if(string.IsNullOrEmpty(newName))
                return;
            manager.SelectedModlist.name = newName;
            alteredModlistBox = true;
            modlistComboBox.Items[modlistComboBox.SelectedIndex] = newName;
            SetChangesMade(true);
            alteredModlistBox = false;
        }

        private void deleteListButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to delete {manager.SelectedModlist.name}?", "Delete modlist", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                if(manager.modlists.Count == 1)
                {
                    MessageBox.Show("You cannot delete the last modlist.", "Failed", MessageBoxButtons.OK);
                    return;
                }

                alteredModlistBox = true;

                modlistComboBox.Items.RemoveAt(modlistComboBox.SelectedIndex);
                manager.modlists.Remove(manager.SelectedModlist);

                modlistComboBox.SelectedIndex = 0;

                manager.SaveAllLists();
            }
        }

        private void importButton_Click(object sender, EventArgs e)
        {

        }

        private void exportButton_Click(object sender, EventArgs e)
        {

        }
    }
}
