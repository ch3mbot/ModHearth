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
        // Console allocation magic.
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        // Manager reference.
        public ModHearthManager manager;

        // Public access to modlist panels (for draggable mod references mainly).
        public VerticalFlowPanel LeftModlistPanel => leftModlistPanel;
        public VerticalFlowPanel RightModlistPanel => rightModlistPanel;

        // Tracking unsaved changes and visually marking them.
        private bool changesMade;
        private bool changesMarked = false;

        // Tracking which modrefPanels are highlighted.
        private HashSet<Control> highlightAffected = new HashSet<Control>();

        // ComboBox handling.
        private int lastIndex;
        private bool modifyingCombobox = false;

        public MainForm()
        {
            // Make console function.
            AllocConsole();
            Console.ForegroundColor = ConsoleColor.White;

            // Basic initialization.
            InitializeComponent();
            manager = new ModHearthManager();

            // TODO: refreshing isn't implemented yet.
            refreshModsButton.Enabled = false;

            // Disable/enable change related buttons.
            SetChangesMade(false);

            // Set up combobox and modreference controls.
            SetupModlistBox();
            GenerateModrefControls();
        }

        private void SetupModlistBox()
        {
            modifyingCombobox = true;

            // Go through the modpacks, add them to combobox.
            foreach (DFHModpack m in manager.modpacks)
            {
                modpackComboBox.Items.Add(m.name);
            }

            // Set the index to the default modpack (manager selected default modpack when created).
            modpackComboBox.SelectedIndex = manager.selectedModlistIndex;
            lastIndex = manager.selectedModlistIndex;

            modifyingCombobox = false;
        }

        // Generate a full list of controls for each side.
        private void GenerateModrefControls()
        {
            GenerateModrefControlSided(leftModlistPanel, new List<DFHMod>(manager.disabledMods), true);
            GenerateModrefControlSided(rightModlistPanel, new List<DFHMod>(manager.enabledMods), false);
        }


        // Generate draggable references, then initialize the panel.
        private void GenerateModrefControlSided(VerticalFlowPanel panel, List<DFHMod> members, bool left)
        {
            List<ModRefPanel> conts = new List<ModRefPanel>();
            foreach (DFHMod dfm in manager.modPool)
            {
                conts.Add(new ModRefPanel(manager.GetModRef(dfm.ToString()), this));
            }

            panel.Initialize(conts, members, !left);
        }

        // When a modrefPanel is being dragged, it calls this. Just handles highlighting.
        public void ModrefMouseMove(Point position)
        {
            // Undo old highlights.
            UnsetSurroundingToHighlight();

            // See if mouse is in panel.
            bool overLeft = leftModlistPanel.GetIndexAtPosition(position, out int indexL);
            bool overRight = rightModlistPanel.GetIndexAtPosition(position, out int indexR);
            
            // Return if not in panel.
            if (!overLeft && !overRight)
            {
                return;
            }

            // Assume left, swap right if needed.
            int index = indexL;
            VerticalFlowPanel panel = leftModlistPanel;

            if (overRight)
            {
                index = indexR;
                panel = rightModlistPanel;
            }

            // Highlight accordingly.
            SetSurroundingToHighlight(index, panel);
        }

        // When a modrefPanel is dropped, it calls this.
        public void ModrefMouseUp(Point position, ModRefPanel modrefPanel)
        {
            // Undo old highlights.
            UnsetSurroundingToHighlight();

            // See if mouse is in panel.
            bool overLeft = leftModlistPanel.GetIndexAtPosition(position, out int indexL);
            bool overRight = rightModlistPanel.GetIndexAtPosition(position, out int indexR);

            // Return if not in panel.
            if (!overLeft && !overRight)
            {
                return;
            }

            // Assume left, swap right if needed.
            int index = indexL;
            VerticalFlowPanel destinationPanel = leftModlistPanel;

            if (overRight)
            {
                index = indexR;
                destinationPanel = rightModlistPanel;
            }

            // Changes have been made.
            SetAndMarkChanges(true);

            // Have the manager apply the changes to the actual enabled mods, then refresh panels to show.
            manager.MoveMod(modrefPanel.modref, index, modrefPanel.vParent == leftModlistPanel, destinationPanel == leftModlistPanel);
            RefreshModlistPanels();
        }

        // If changes aren't already marked, adds a * to the combobox entry.
        private void MarkChanges(int index)
        {
            if (changesMarked)
                return;

            modifyingCombobox = true;

            modpackComboBox.Items[index] = modpackComboBox.Items[index].ToString() + "*";
            changesMarked = true;

            modifyingCombobox = false;
        }

        // If changes are marked, unmark them.
        private void UnmarkChanges(int index)
        {
            if (!changesMarked)
                return;
            modifyingCombobox = true;

            string currstr = modpackComboBox.Items[index].ToString();
            modpackComboBox.Items[index] = currstr.Substring(0, currstr.Length - 1);
            changesMarked = false;

            modifyingCombobox = false;
        }

        // Record that changes were or weren't made, and enable/disable appropriate buttons.
        private void SetChangesMade(bool changesMade)
        {
            // Set actual variable.
            this.changesMade = changesMade;

            // No undoing if there are no changes.
            undoChangesButton.Enabled = changesMade;

            // If there are changes then no renaming, importing, exporting, or new lists.
            renameListButton.Enabled = !changesMade;
            importButton.Enabled = !changesMade;
            exportButton.Enabled = !changesMade;
            newListButton.Enabled = !changesMade;
        }
        
        // Tell both modlistPanels to update which modreferencePanels are visible based on lists from manager.
        private void RefreshModlistPanels()
        {
            leftModlistPanel.UpdateVisibleOrder(new List<DFHMod>(manager.disabledMods));
            RightModlistPanel.UpdateVisibleOrder(manager.enabledMods);
        }

        // Highlight the panel at and before index, if they exist.
        private void SetSurroundingToHighlight(int index, VerticalFlowPanel parentPanel)
        {
            if (index > 0)
            {
                Control selected = parentPanel.GetVisibleControlAtIndex(index - 1);
                selected.BackgroundImage = Resource1.highlight_bottom;
                highlightAffected.Add(selected);
            }
            if (index < parentPanel.memberMods.Count)
            {
                Control selected = parentPanel.GetVisibleControlAtIndex(index);
                selected.BackgroundImage = Resource1.highlight_top;
                highlightAffected.Add(selected);
            }
        }

        // Loop through all highlighted panels and unhighlight them.
        private void UnsetSurroundingToHighlight()
        {
            foreach (Control c in highlightAffected)
            {
                c.BackgroundImage = null;
            }
        }

        // Set changesMade, fix buttons, and mark/unmark changes.
        private void SetAndMarkChanges(bool changesMade)
        {
            SetChangesMade(changesMade);
            if (changesMade)
                MarkChanges(lastIndex);
            else
                UnmarkChanges(lastIndex);
        }

        // Tell manager to set the modlist to index, then refresh both panels.
        private void SetAndRefreshModpack(int index)
        {
            manager.SetSelectedModpack(index);
            RefreshModlistPanels();
        }

        // Save the list.
        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveCurrentModpack();
        }

        private void undoChangesButton_Click(object sender, EventArgs e)
        {
            // Ask the user if they really want to undo their changes.
            DialogResult result = MessageBox.Show("Are you sure you want to reset modlist changes?", "Undo changes", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                // Undo our changes and immediately refresh
                UndoListChanges();
            }
        }

        // TODO: rune the dwarf fortress executable.
        private void playGameButton_Click(object sender, EventArgs e)
        {

        }

        // TODO: rescan mod folders and recreate controls for modlistPanels. For things like downloading non workshop mods, or mod creation.
        private void refreshModsButton_Click(object sender, EventArgs e)
        {

        }

        private void SaveCurrentModpack()
        {
            // Save changes to modpack.
            manager.SaveCurrentModpack();

            // Unmark changes and fix buttons.
            SetAndMarkChanges(false);
        }

        private void UndoListChanges()
        {
            // Set the modpack to lastIndex (loads modlist from modpack, undoing changes)
            SetAndRefreshModpack(lastIndex);

            // Unmark changes and fix buttons.
            SetAndMarkChanges(false);

        }

        private void modlistComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            // If we are programatically modifying combobox, do nothing.
            if (modifyingCombobox)
                return;

            Console.WriteLine($"Changed from index {lastIndex} to {modpackComboBox.SelectedIndex}");

            // If the modlist didn't change, do nothing.
            if (lastIndex == modpackComboBox.SelectedIndex)
                return;

            // If changes were made, prompt user to either save changes, discard changes, or cancel index change.
            if (changesMade)
            {
                DialogResult result = MessageBox.Show($"You have unsaved changes to {manager.SelectedModlist.name}, do you want to save before continuing?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    // If yes, save changes before proceeding with swap.
                    SaveCurrentModpack();
                }
                else if (result == DialogResult.No)
                {
                    // If no, unset changes and fix buttons before proceeding (changing modlists discards changes inherently).
                    SetAndMarkChanges(false);
                }
                else
                {
                    // If cancel, set index to last, then return, so as not to proceed with swap.
                    modifyingCombobox = true;
                    modpackComboBox.SelectedIndex = lastIndex;
                    modifyingCombobox = false;
                    return;
                }
            }

            // Either no changes were made, or the user decided to proceed. Tell manager to swap to new index and update lastIndex.
            SetAndRefreshModpack(modpackComboBox.SelectedIndex);
            lastIndex = modpackComboBox.SelectedIndex;
        }

        // Creates a new modpack. This can only be pressed when no unsaved changes. 
        private void newPackButton_Click(object sender, EventArgs e)
        {
            // Ask the user for a name.
            string newName = Interaction.InputBox("Please enter a name for the new modpack", "New Modpack Name", "");
            if (string.IsNullOrEmpty(newName))
                return;

            // Create a new modpack.
            DFHModpack newPack = new DFHModpack(false, new List<DFHMod>(), newName);

            // Register the modpack.
            RegisterNewModpack(newPack);
        }

        // Adds a new modpack to the list, and saves immediately. No changes are recorded since saving is immediate.
        private void RegisterNewModpack(DFHModpack newList)
        {
            modifyingCombobox = true;

            // Add the new modpack to the manager, and save it to the modpacks file
            manager.modpacks.Add(newList);
            manager.SaveAllModpacks();

            //add to combobox and select
            modpackComboBox.Items.Add(newList.name);
            modpackComboBox.SelectedIndex = modpackComboBox.Items.Count - 1;

            //set manager to it and refresh
            manager.SetSelectedModpack(modpackComboBox.SelectedIndex);
            RefreshModlistPanels();

            modifyingCombobox = false;
        }

        // Renames a modpack, and saves immediately. This can only be pressed when no unsaved changes.
        private void renameModpackButton_Click(object sender, EventArgs e)
        {
            string newName = Interaction.InputBox("Please enter a new name for the modpack", "New Modpack Name", manager.SelectedModlist.name);
            if(string.IsNullOrEmpty(newName))
                return;

            // Change names but do not set changesmade to true.
            modifyingCombobox = true;

            manager.SelectedModlist.name = newName;
            modpackComboBox.Items[modpackComboBox.SelectedIndex] = newName;

            // Save the current modpack to file.
            SaveCurrentModpack();

            modifyingCombobox = false;
        }

        // Deletes a modpack, and saves immediately. This can only be pressed when no unsaved changes. Fails if there is only one modpack left.
        private void deleteListButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to delete {manager.SelectedModlist.name}? This is final.", "Delete modlist", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                // Undo our changes and fix buttons.
                SetAndMarkChanges(false);

                // Minimum one modpack FIXME: this is just because not having any modlists needs extra logic. Would be easy enough to generate a vanilla modpack if all are gone though.
                if (manager.modpacks.Count == 1)
                {
                    MessageBox.Show("You cannot delete the last modlist.", "Failed", MessageBoxButtons.OK);
                    return;
                }

                modifyingCombobox = true;

                // Remove the modpack from both manager and combobox.
                modpackComboBox.Items.RemoveAt(modpackComboBox.SelectedIndex);
                manager.modpacks.Remove(manager.SelectedModlist);

                // Overwrite modpack file with missing modpack.
                manager.SaveAllModpacks();

                // Set the index to 0 and refresh.
                modpackComboBox.SelectedIndex = 0;
                SetAndRefreshModpack(0);

                modifyingCombobox = false;
            }
        }

        // Imports a JSON file and converts it to a modpack. This can only be pressed when no unsaved changes.
        private void importButton_Click(object sender, EventArgs e)
        {
            // Get the user to select a JSON file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.json)|*.json";
            openFileDialog.Title = "Select a Modpack JSON File";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Try opening and deserializing the file.
                    string filePath = openFileDialog.FileName;
                    string importedString = File.ReadAllText(filePath);
                    DFHModpack importedList = JsonSerializer.Deserialize<DFHModpack>(importedString);

                    // Check if another modpack by the same name exists. If so ask the user to overwrite. FIXME: it is unknown if dfhack allows multiple modpacks with the same name, but it is avoided anyways.
                    for(int i = 0; i < manager.modpacks.Count; i++) 
                    {
                        DFHModpack otherModlist = manager.modpacks[i];
                        if(otherModlist.name == importedList.name)
                        {
                            // If the user said yes to overwrite, then do a more complex process, to allow the undo button to revert the imported changes.
                            DialogResult result = MessageBox.Show($"A modpack with the name {otherModlist.name} is already present. Would you like to overwrite it?", "Modlist Already Present", MessageBoxButtons.YesNo);
                            if(result == DialogResult.Yes)
                            {
                                // Set the combobox to the matching modlist.
                                modifyingCombobox = true;
                                modpackComboBox.SelectedIndex = i;
                                lastIndex = i;
                                modifyingCombobox = false;

                                // Set the selected modpack to the matching one.
                                manager.SetSelectedModpack(i);

                                // Overwrite the enabled mods and refresh panels.
                                manager.SetActiveMods(importedList.modlist);
                                RefreshModlistPanels();

                                // Set changes made and mark changes.
                                SetChangesMade(true);
                                MarkChanges(i);

                            }
                            return;
                        }
                    }

                    // If a modpack with a matching name wasn't found, register this modpack as a new modpack.
                    RegisterNewModpack(importedList);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Take the current modlist and export it to a JSON file. This can only be pressed when no unsaved changes.
        private void exportButton_Click(object sender, EventArgs e)
        {
            // Allow the user to choose a save location.
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.json)|*.json";
            saveFileDialog.Title = "Save Modpack JSON File";
            saveFileDialog.DefaultExt = "json";
            saveFileDialog.AddExtension = true;

            // Try and save the file to that location.
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
