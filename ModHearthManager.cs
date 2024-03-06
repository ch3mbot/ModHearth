using ModHearth.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModHearth
{
    /// <summary>
    /// Config class, to store folder information.
    /// </summary>
    [Serializable]
    public class ModHearthConfig
    {
        // Path to DF.exe
        public string DFEXEPath { get; set; }
        public string DFFolderPath => Path.GetDirectoryName(DFEXEPath);
        public string ModsPath => Path.Combine(DFFolderPath, "Mods");

        // Should this be in lightmode?
        public int theme { get; set; }

    }

    [Serializable]
    public struct ModProblem
    {
        public string problemThrowerID;
        public string problemID;

        public enum ProblemType
        {
            MissingBefore,
            MissingAfter,
            ConflictPresent
        };

        public ProblemType problemType;

        public ModProblem(string problemThrowerID, string problemID, ProblemType problemType)
        {
            this.problemThrowerID = problemThrowerID;
            this.problemID = problemID;
            this.problemType = problemType;
        }

        public override string ToString()
        {
            switch(problemType)
            {
                case ProblemType.MissingBefore:
                    return $"Mod '{problemThrowerID}' requires mod '{problemID}' to be loaded before it.";
                case ProblemType.MissingAfter:
                    return $"Mod '{problemThrowerID}' requires mod '{problemID}' to be loaded after it.";
                case ProblemType.ConflictPresent:
                    return $"Mod '{problemThrowerID}' is incompatible with mod '{problemID}'.";
            }
            return "";
        }
    }

    public class ModHearthManager
    {
        // Maps strings to ModReferences. The keys match DFHMods.ToString() perfectly. Given a value V, V.ToDFHMod.ToString() returns it's key.
        private Dictionary<string, ModReference> modrefMap;

        // Get a ModReference given a string key.
        public ModReference GetModRef(string key) => modrefMap[key];

        // Get a DFHMod given a string key.
        public DFHMod GetDFHackMod(string key) => modrefMap[key].ToDFHMod();

        // Get a ModReference given a DFHMod key.
        public ModReference GetRefFromDFHMod(DFHMod dfmod) => modrefMap[dfmod.ToString()];

        // The sorted list of enabled DFHmods. This list is modified by the form, and when saved it overwrites the list of a ModPack.
        public List<DFHMod> enabledMods;

        // The unsorted list of disabled DFHmods
        public HashSet<DFHMod> disabledMods;

        // The unsorted list of all available DFHmods
        public HashSet<DFHMod> modPool;

        // Get the currently selected modpack
        public DFHModpack SelectedModlist => modpacks[selectedModlistIndex];

        // List of all modpacks. After a modpack in this list is modified the list is saved to file.
        public List<DFHModpack> modpacks;

        // The index of the currently selected modpack.
        public int selectedModlistIndex;

        // The file config for this class.
        private ModHearthConfig config;

        // Paths.
        private static readonly string configPath = "config.json";
        private static readonly string stylePath = "style.json";

        // Mod problem tracker.
        public List<ModProblem> modproblems;

        public ModHearthManager() 
        {
            Console.WriteLine("Crafting Hearth");

            // Get and load config file, fix if needed.
            AttemptLoadConfig();
            FixConfig();

            // Find all mods and add to the lists.
            FindAllMods();

            // Find DFHModpacks, and fix them if needed.
            FindModpacks();

            // Write some info on found things.
            Console.WriteLine();
            Console.WriteLine($"Found {modrefMap.Count} mods and {modpacks.Count} modlists");
            Console.WriteLine();
        }

        // Run the game.
        public void RunDwarfFortress()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nStarting " + config.DFEXEPath);
            Console.ForegroundColor = ConsoleColor.White;

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = config.DFEXEPath;
            processInfo.ErrorDialog = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            processInfo.WorkingDirectory = Path.GetDirectoryName(config.DFEXEPath);

            Process.Start(processInfo);
        }

        // Alter the current modpack with enabledMods and save modpack list to dfhack file.
        public void SaveCurrentModpack()
        {
            SelectedModlist.modlist = new List<DFHMod>(enabledMods);

            SaveAllModpacks();
        }

        // Save the DFHModpack list to file.
        public void SaveAllModpacks()
        {
            // Get the path, serialize with right options, and write to file.
            string dfHackModlistPath = Path.Combine(config.DFFolderPath, @"dfhack-config\mod-manager.json");
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true // Enable pretty formatting
            };
            string modlistJson = JsonSerializer.Serialize(modpacks, options);
            File.WriteAllText(dfHackModlistPath, modlistJson);
        }

        public void SetSelectedModpack(int index)
        {
            // Regenerate enabled and disabled lists to match newly selected modpack.
            selectedModlistIndex = index;
            SetActiveMods(SelectedModlist.modlist);

            // Find problems with newly selected modpack.
            FindModlistProblems();
        }

        // Changes currently enabled and disabled mods based on the given list.
        // The only time this is called (other than SetSelectedModpack) is when overwriting a modpack due to importing.
        public void SetActiveMods(List<DFHMod> mods)
        {
            enabledMods = new List<DFHMod>();
            disabledMods = new HashSet<DFHMod>(modPool);
            for (int i = 0; i < mods.Count; i++)
            {
                enabledMods.Add(mods[i]);
                disabledMods.Remove(mods[i]);
            }
        }

        // Move a mod from one place to another. Has four cases, depending on source and destination.
        public void MoveMod(ModReference mod, int newIndex, bool sourceLeft, bool destinationLeft)
        {
            // Convert mod to DFHMod.
            DFHMod dfm = mod.ToDFHMod();

            if (sourceLeft && destinationLeft)
            {
                // Do nothing since the order of disabled mods doesn't matter.
                return;
            }
            else if (!sourceLeft && !destinationLeft)
            {
                // Get the old index of the mod
                int oldIndex = enabledMods.IndexOf(dfm);

                // If the mod is removed from the old index, and it would shift the whole list down, account for that.
                if (oldIndex < newIndex)
                    newIndex--;

                // Remove from old index and insert at the new index (or add if at end of list).
                enabledMods.RemoveAt(oldIndex);
                if (newIndex == enabledMods.Count)
                    enabledMods.Add(dfm);
                else
                    enabledMods.Insert(newIndex, dfm);
            }
            else if (!sourceLeft && destinationLeft)
            {
                // Remove the mod from enabled list, and toss into disabled list (order doesn't matter).
                enabledMods.Remove(dfm);
                disabledMods.Add(dfm);
            }
            else if (sourceLeft && !destinationLeft)
            {
                // Insert/add from disabled to enabled.
                disabledMods.Remove(dfm);
                if (newIndex == enabledMods.Count)
                    enabledMods.Add(dfm);
                else
                    enabledMods.Insert(newIndex, dfm);
            }

            // Refind mod problems since mod order changed.
            FindModlistProblems();
        }

        // Go through modlist and scan for problems.
        // Tuple representing problem has problem mod, int problemType (missing before, missing after, conflict present), and string modID.
        public void FindModlistProblems()
        {
            // Set up list of problems to return.
            modproblems = new List<ModProblem>();

            // Set up a hashset of scanned mods and unscanned mods, for determining load order.
            HashSet<string> scannedModIDs = new HashSet<string>();
            HashSet<string> unscannedModIDs = new HashSet<string>();

            // Add all enabled mod IDs to unscanned.
            foreach (DFHMod dfm in enabledMods)
            {
                unscannedModIDs.Add(dfm.id);
            }

            // Loop through enabled mods, doing a mock load.
            for (int i = 0; i < enabledMods.Count; i++)
            {
                DFHMod currentDFM = enabledMods[i];
                ModReference currentMod = GetRefFromDFHMod(currentDFM);

                // Check for problems.
                if (currentMod.problematic)
                {
                    foreach (string beforeID in currentMod.require_before_me)
                        if (!scannedModIDs.Contains(beforeID))
                        {
                            modproblems.Add(new ModProblem(currentDFM.id, beforeID, ModProblem.ProblemType.MissingBefore));
                            //Console.WriteLine("Problem found: missing before mod with ID: " + beforeID + " mod needing is: " + currentDFM.id);
                        }
                    foreach (string afterID in currentMod.require_after_me)
                        if (!unscannedModIDs.Contains(afterID))
                        {
                            modproblems.Add(new ModProblem(currentDFM.id, afterID, ModProblem.ProblemType.MissingAfter));
                            //Console.WriteLine("Problem found: missing after mod with ID: " + afterID + " mod needing is: " + currentDFM.id);
                        }
                    foreach (string conflictID in currentMod.conflicts_with)
                        if (scannedModIDs.Contains(conflictID) || unscannedModIDs.Contains(conflictID))
                        {
                            modproblems.Add(new ModProblem(currentDFM.id, conflictID, ModProblem.ProblemType.ConflictPresent));
                            //Console.WriteLine("Problem found: conflict present mod with ID: " + conflictID + " mod needing is: " + currentDFM.id);
                        }
                }

                // Move to scanned.
                scannedModIDs.Add(currentDFM.id);
                unscannedModIDs.Remove(currentDFM.id);
            }
        }

        #region initialization file stuff
        // Find all mods based on mods folder.
        private void FindAllMods()
        {
            // Initialize relevant variables.
            modrefMap = new Dictionary<string, ModReference>();
            modPool = new HashSet<DFHMod>();

            // Get all mod folders.
            Console.WriteLine("Finding all mods... ");
            List<string> modFolders = new List<string>(Directory.GetDirectories(config.ModsPath));

            // Add vanilla mod folders, excluding obvious non-mod folders.
            string vanillaDataPath = Path.Combine(config.DFFolderPath, @"data\vanilla");
            foreach (string vanillaModDir in Directory.GetDirectories(vanillaDataPath))
            {
                modFolders.Add(vanillaModDir);
            }

            // Remove known non-mod folders.
            List<string> temp = new List<string>();
            foreach (string modPath in modFolders)
            {
                // Three vanilla folders.
                if (modPath.Contains("mod_upload") || modPath.Contains("examples and notes") || modPath.Contains("interaction examples"))
                    continue;
                temp.Add(modPath);

            }
            modFolders = temp;

            // Load all the mods.
            foreach (string modFolder in modFolders)
            {
                // Check if file exists, handle accordingly.
                string infoPath = Path.Combine(modFolder, "info.txt");
                if (File.Exists(infoPath))
                {
                    // Read the file and attempt to create valid ModReference from it.
                    string modInfo = File.ReadAllText(infoPath);
                    ModReference modRef = new ModReference(modInfo, modFolder);
                    if (!modRef.failed)
                    {
                        // ModRef was successful, so add it to the lists.
                        string key = modRef.DFHackCompatibleString();
                        Console.WriteLine($"   Valid mod found: {modRef.name}.");
                        modrefMap.Add(key, modRef);
                        modPool.Add(modRef.ToDFHMod());
                    }
                    else
                    {
                        // #TODO: Broken mod popup, asking if deletion is needed would be good.
                        // ModRef failed.
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"   Broken mod found. path:{modFolder}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else
                {
                    // #TODO: Broken mod popup, asking if deletion is needed would be good.
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"   Broken mod found. No info file. path:{modFolder}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        // Find modpacks from dfhack mod-manager config file.
        private void FindModpacks()
        {
            // Get paths and read file. #TODO: handling the file not existing.
            string dfHackModpackPath = Path.Combine(config.DFFolderPath, @"dfhack-config\mod-manager.json");
            string dfHackModpackJson = File.ReadAllText(dfHackModpackPath);
            modpacks = new List<DFHModpack>(JsonSerializer.Deserialize<List<DFHModpack>>(dfHackModpackJson));

            Console.WriteLine();
            Console.WriteLine("Found modlists: ");

            // Handle mods missing.
            bool modMissing = false;
            string missingMessage = $"Some mods missing. \nModlists will be modified to not require lost mods. \nMissing mods: ";
            HashSet<DFHMod> notFound = new HashSet<DFHMod>();

            // If a default modpack exists.
            bool defaultFound = false;

            // Go through modpacks, and go through their modlists, looking for mods that we don't have.
            for (int i = 0; i < modpacks.Count; i++)
            {
                DFHModpack modlist = modpacks[i];

                HashSet<DFHMod> thisListMissingMods = new HashSet<DFHMod>();
                foreach(DFHMod mod in modlist.modlist)
                {
                    if(!modPool.Contains(mod))
                    {
                        modMissing = true;
                        notFound.Add(mod);
                        thisListMissingMods.Add(mod);
                        missingMessage += $"\n{mod}";
                    }
                }
                
                // Remove the missing mods from the modlist.
                foreach(DFHMod m in thisListMissingMods)
                {
                    modlist.modlist.Remove(m);
                }

                // Write out some info on the modpack
                Console.WriteLine("   Name: " + modlist.name);
                Console.WriteLine("   Default: " + modlist.@default);
                Console.WriteLine("   Mods count: " + modlist.modlist.Count);
                Console.WriteLine();

                // If  this is the default modpack, set index to that. 
                if (modlist.@default)
                {
                    SetSelectedModpack(i);
                    defaultFound = true;
                }

                // Set modpacks[i] back to this modpack. #FIXME: why is this necessary? Isn't modpack a reference type?
                modpacks[i] = modlist;
            }

            // Set default as backup.
            if (!defaultFound)
            {
                SetSelectedModpack(0);
                modpacks[0].@default = true;
                SaveAllModpacks();
            }

            // Create default modpack if none present.
            if(modpacks.Count == 0)
            {
                DFHModpack newPack = new DFHModpack(true, new List<DFHMod>(), "Default");
                // FIXME: generate vanilla modpack in a better way than this
                newPack.modlist = GenerateVanillaModlist();

            }

            // Pop up a message notifying the user that the missing mods have been removed.
            if(modMissing)
            {
                LocationMessageBox.Show(missingMessage, "Missing Mods", MessageBoxButtons.OK);
            }
        }

        // Generated a vanilla modlist using manually generated mod ID list.
        public List<DFHMod> GenerateVanillaModlist()
        {
            // Manually made vanilla modlist.
            List<string> vanillaModIDList = new List<string>()
                {
                    "vanilla_text",
                    "vanilla_languages",
                    "vanilla_descriptors",
                    "vanilla_materials",
                    "vanilla_environment",
                    "vanilla_plants",
                    "vanilla_items",
                    "vanilla_buildings",
                    "vanilla_bodies",
                    "vanilla_creatures",
                    "vanilla_entities",
                    "vanilla_reactions",
                    "vanilla_interactions",
                    "vanilla_descriptors_graphics",
                    "vanilla_plants_graphics",
                    "vanilla_items_graphics",
                    "vanilla_buildings_graphics",
                    "vanilla_creatures_graphics",
                    "vanilla_world_map",
                    "vanilla_interface",
                    "vanilla_music",
                };

            // Get all mods that are probably vanilla.
            Dictionary<string, DFHMod> vanillaPool = new Dictionary<string, DFHMod>();
            foreach (DFHMod dfm in modPool)
            {
                if (dfm.id.ToLower().Contains("vanilla"))
                {
                    vanillaPool.Add(dfm.id, dfm);
                    Console.WriteLine("added mod with id " +  dfm.id);
                }
            }

            // Load vanilla mods into pack.
            List<DFHMod> vanillaList = new List<DFHMod>();
            for (int i = 0; i < vanillaModIDList.Count; i++)
            {
                Console.WriteLine("looking for mod with id " + vanillaModIDList[i]);
                vanillaList.Add(vanillaPool[vanillaModIDList[i]]);
            }

            return vanillaList;
        }

        // Get the theme from config.
        public int GetTheme()
        {
            return config.theme;
        }
        
        // Save the theme to config file.
        public void SetTheme(int theme)
        {
            config.theme = theme;
            SaveConfigFile();
        }


        // Fix the config file if it's broken or missing.
        public void FixConfig()
        {
            // If it's missing create it.
            if (config == null)
                config = new ModHearthConfig();

            // If it's missing the path to dwarf fortress executable, get the path.
            if (String.IsNullOrEmpty(config.DFEXEPath))
            {
                Console.WriteLine("Config file missing DF path.");
                string newPath = "";
                while (string.IsNullOrEmpty(newPath))
                {
                    newPath = GetDFPath();
                }
                config.DFEXEPath = newPath;
            }

            // Save the fixed config file.
            SaveConfigFile();
        }

        // Destroy the config file, to be remade.
        public void DestroyConfig()
        {
            if (File.Exists(configPath))
            {
                File.Delete(configPath);
            }
        }

        // Get the path to the dwarf fortress executable from the user.
        private string GetDFPath()
        {
            LocationMessageBox.Show("Please find the path to your Dwarf Fortress.exe.", "DF.exe location", MessageBoxButtons.OK);
            OpenFileDialog dfFileDialog = new OpenFileDialog();
            dfFileDialog.Filter = "Executable files (*.exe)|Dwarf Fortress.exe";
            DialogResult result = dfFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string selectedFilePath = dfFileDialog.FileName;
                Console.WriteLine("DF path set to: " + selectedFilePath);
                return selectedFilePath;
            }
            return "";
        }

        // Attmempt loading config. If broken or failed, run FixConfig.
        public void AttemptLoadConfig()
        {
            Console.WriteLine("Attempting config file load.");
            try
            {
                if (File.Exists(configPath))
                {
                    Console.WriteLine("Config file found.");
                    string jsonContent = File.ReadAllText(configPath);

                    // Deserialize the JSON content into an object
                    config = JsonSerializer.Deserialize<ModHearthConfig>(jsonContent);

                    if (config == null)
                    {
                        Console.WriteLine("Config file borked.");
                        FixConfig();
                    }
                }
                else
                {
                    Console.WriteLine("Config file missing.");
                    FixConfig();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                config = null;
                FixConfig();

            }
        }

        // Save the config to file.
        public void SaveConfigFile()
        {
            Console.WriteLine("Config saved.");
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true // Enable pretty formatting
            };
            string jsonContent = JsonSerializer.Serialize(config);
            File.WriteAllText(configPath, jsonContent);
        }

        // Try to load style file.
        public Style LoadStyle()
        {
            Style style = new Style();

            try
            {
                if (File.Exists(stylePath))
                {
                    Console.WriteLine("Style file found.");
                    string jsonContent = File.ReadAllText(stylePath);

                    // Deserialize the JSON content into an object
                    Style foundStyle = JsonSerializer.Deserialize<Style>(jsonContent);

                    // If broken, save working one, otherwise, use found one.
                    if (foundStyle == null)
                    {
                        Console.WriteLine("Style file borked. Style regenerated.");
                        style = Style.lightModeStyle();
                        SaveStyle(style);
                    }
                    else
                    {
                        style = foundStyle;
                    }
                }
                else
                {
                    Console.WriteLine("Style file missing. New style reated.");
                    style = Style.lightModeStyle();
                    SaveStyle(style);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }


            // Set global instance and return.
            Style.instance = style;
            return style;
        } 

        // Save the style to file.
        private void SaveStyle(Style style)
        {
            Console.WriteLine("Style saved.");
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true // Enable pretty formatting
            };
            string jsonContent = JsonSerializer.Serialize(style, options);
            File.WriteAllText(stylePath, jsonContent);
        }
        #endregion
    }
}
