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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModHearth
{
    public partial class MainForm : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public static MainForm instance;

        public ModHearthManager manager;

        public VerticalFlowPanel LeftModlistPanel => leftModlistPanel;
        public VerticalFlowPanel RightModlistPanel => rightModlistPanel;

        private bool changesMade;
        private bool changesMarked = false;
        private int lastIndex;
        private bool modifyingCombobox = false;

        public MainForm()
        {
            //make console function
            AllocConsole();
            Console.ForegroundColor = ConsoleColor.White;

            //basic initialization
            instance = this;
            InitializeComponent();
            manager = new ModHearthManager();

            //#fix# not implemented yet
            refreshModsButton.Enabled = false;

            //disable/enable change related buttons
            SetChangesMade(false);

            //set up combobox and modreference controls
            SetupModlistBox();
            GenerateModrefControls();
        }

        private void SetupModlistBox()
        {
            modifyingCombobox = true;

            //go through the modpacks
            foreach (DFHackModlist m in manager.modpacks)
            {
                modlistComboBox.Items.Add(m.name);
            }

            //set the index to the default modpack
            modlistComboBox.SelectedIndex = manager.selectedModlistIndex;
            lastIndex = manager.selectedModlistIndex;

            modifyingCombobox = false;
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
            modDescriptionLabel.Text = "changes just marked";
        }

        private void UnmarkChanges(int index)
        {
            if (!changesMarked)
                return;
            string currstr = modlistComboBox.Items[index].ToString();
            modlistComboBox.Items[index] = currstr.Substring(0, currstr.Length - 1);
            changesMarked = false;
            modDescriptionLabel.Text = "changes just unmarked";
        }
        private void SetChangesMade(bool changesMade)
        {
            //set actual variable
            this.changesMade = changesMade;

            //no undoing if there are no changes
            undoChangesButton.Enabled = changesMade;

            //if there are changes then no renaming, importing, exporting, or new lists
            renameListButton.Enabled = !changesMade;
            importButton.Enabled = !changesMade;
            exportButton.Enabled = !changesMade;
            newListButton.Enabled = !changesMade;

            //debugging
            modTitleLabel.Text = "changesMade: " + changesMade;

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
            SaveList();
        }

        private void undoChangesButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to reset modlist changes?", "Undo changes", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                //undo our changes and immediately refresh
                UndoListChanges(true);
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

        //save the current modlist to file.
        private void SaveList()
        {
            manager.SaveCurrentModlist();

            //no point in refreshing, nothing has changes. this effectively unmarks changes.
            UndoListChanges(false);
        }

        private void UndoListChanges(bool doRefresh)
        {
            //manager sets activemods to the ones from the index
            manager.SetSelectedModlist(lastIndex);

            //if we should refresh immediately
            if(doRefresh)
                RefreshModlistPanels();

            //unmark changes and fix buttons
            UnmarkChanges(lastIndex);
            SetChangesMade(false);

        }

        private void modlistComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            //if we are programatically modifying combobox, do nothing
            if (modifyingCombobox)
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
                    SaveList();
                }
                else if (result == DialogResult.No)
                {
                    //do not refresh that list.
                    UndoListChanges(false);
                }
                else
                {
                    //if cancel, set index to last, then return.
                    modifyingCombobox = true;
                    modlistComboBox.SelectedIndex = lastIndex;
                    modifyingCombobox = false;
                    return;
                }
            }

            //if no changes were made, or yes/no was selected, then change the index
            manager.SetSelectedModlist(modlistComboBox.SelectedIndex);
            RefreshModlistPanels();
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

            RegisterNewModpack(newList);
        }

        //adds a new modpack to the list, and saves immediately. no changes made since it saves.
        private void RegisterNewModpack(DFHackModlist newList)
        {
            modifyingCombobox = true;

            //add the new list to the manager, and save it to the lists file
            manager.modpacks.Add(newList);
            manager.SaveAllLists();

            //add to combobox and select
            modlistComboBox.Items.Add(newList.name);
            modlistComboBox.SelectedIndex = modlistComboBox.Items.Count - 1;

            //set manager to it and refresh
            manager.SetSelectedModlist(modlistComboBox.SelectedIndex);
            RefreshModlistPanels();

            modifyingCombobox = false;
        }


        private void renameListButton_Click(object sender, EventArgs e)
        {
            string newName = Interaction.InputBox("Please enter a new name for the modpack", "New Modpack Name", manager.SelectedModlist.name);
            if(string.IsNullOrEmpty(newName))
                return;

            //change names but do not set changesmade to true. renaming is instant.
            modifyingCombobox = true;

            manager.SelectedModlist.name = newName;
            modlistComboBox.Items[modlistComboBox.SelectedIndex] = newName;

            SaveList();

            modifyingCombobox = false;
        }

        private void deleteListButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to delete {manager.SelectedModlist.name}? This is final.", "Delete modlist", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                //undo our changes, but do not refresh
                UndoListChanges(false);

                //minimum one modlist #fix# this is just because not having any modlists needs extra logic #fix# add new modlist if none present on start
                if (manager.modpacks.Count == 1)
                {
                    MessageBox.Show("You cannot delete the last modlist.", "Failed", MessageBoxButtons.OK);
                    return;
                }

                modifyingCombobox = true;

                //remove the modpack from both
                modlistComboBox.Items.RemoveAt(modlistComboBox.SelectedIndex);
                manager.modpacks.Remove(manager.SelectedModlist);

                //overwrite lists file with missing modpack
                manager.SaveAllLists();

                //set the index and refresh
                manager.SetSelectedModlist(0);
                modlistComboBox.SelectedIndex = 0;
                RefreshModlistPanels();

                modifyingCombobox = false;
            }
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.json)|*.json";
            openFileDialog.Title = "Select a Modpack JSON File";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    string importedString = File.ReadAllText(filePath);

                    DFHackModlist importedList = JsonSerializer.Deserialize<DFHackModlist>(importedString);
                    for(int i = 0; i < manager.modpacks.Count; i++) 
                    {
                        DFHackModlist otherModlist = manager.modpacks[i];
                        if(otherModlist.name == importedList.name)
                        {
                            DialogResult result = MessageBox.Show($"A modpack with the name {otherModlist.name} is already present. Would you like to overwrite it?", "Modlist Already Present", MessageBoxButtons.YesNo);
                            if(result == DialogResult.Yes)
                            {
                                Console.WriteLine($"guh??");
                                manager.SetSelectedModlist(i);
                                manager.SetActiveMods(importedList.modlist);
                                RefreshModlistPanels();

                                SetChangesMade(true);
                                MarkChanges(i);
                                RefreshModlistPanels();
                            }
                            return;
                        }
                    }
                    RegisterNewModpack(importedList);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.json)|*.json";
            saveFileDialog.Title = "Save Modpack JSON File";
            saveFileDialog.DefaultExt = "json";
            saveFileDialog.AddExtension = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filePath = saveFileDialog.FileName;
                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        WriteIndented = true // Enable pretty formatting
                    };
                    string exportString = JsonSerializer.Serialize(manager.SelectedModlist, options);
                    File.WriteAllText(filePath, exportString);

                    MessageBox.Show("File saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
