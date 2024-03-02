using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModHearth.V2
{
    public class ModHearthManager2
    {
        private Dictionary<string, ModReference> modrefMap;
        public ModReference GetModRef(string key) => modrefMap[key];
        public DFHackMod GetDFHackMod(string key) => modrefMap[key].ToDFHackMod();
        public ModReference RefFromDFHack(DFHackMod dfmod) => modrefMap[dfmod.ToString()];

        public List<DFHackMod> activeMods;
        public HashSet<DFHackMod> unactiveMods;
        public HashSet<DFHackMod> modPool;

        public DFHackModlist SelectedModlist => modlists[selectedModlistIndex];
        public List<DFHackModlist> modlists;
        public int selectedModlistIndex;

        private ModHearthConfig config;

        private static readonly string configPath = "config.json";
        private static readonly string modlistPath = "modlists/";

        private MainForm form;

        public ModHearthManager2() 
        {
            Console.WriteLine("Creating Hearth");

            form = MainForm.instance;

            //get and load data
            AttemptLoadConfig();
            FixConfig();

            //find all mods and add to a big list
            FindAllMods();

            FindModlists();

            Console.WriteLine();
            Console.WriteLine($"Found {modrefMap.Count} mods and {modlists.Count} modlists");
            Console.WriteLine();
        }

        //alter the current modlist and save to dfhackfile
        public void SaveCurrentModlist()
        {
            SelectedModlist.modlist = new List<DFHackMod>(activeMods);

            SaveAllLists();
        }

        public void SaveAllLists()
        {

            string dfHackModlistPath = Path.Combine(config.DFFolderPath, @"dfhack-config\mod-manager.json");
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true // Enable pretty formatting
            };
            string modlistJson = JsonSerializer.Serialize(modlists, options);
            File.WriteAllText(dfHackModlistPath, modlistJson);
        }

        public void SetSelectedModlist(int index)
        {
            selectedModlistIndex = index;
            activeMods = new List<DFHackMod>();
            unactiveMods = new HashSet<DFHackMod>(modPool);
            foreach (DFHackMod dfm in SelectedModlist.modlist)
            {
                activeMods.Add(dfm);
                unactiveMods.Remove(dfm);
            }
        }
        public void MoveMod(ModReference mod, int newIndex, bool sourceLeft, bool destinationLeft)
        {
            DFHackMod dfm = mod.ToDFHackMod();
            if (sourceLeft && destinationLeft)
            {
                //do nothing since disabled mod order doesn't matter
            }
            else if (!sourceLeft && !destinationLeft)
            {
                //swap the mod order around abit
                int oldIndex = activeMods.IndexOf(dfm);

                //if we remove the mod from the old index, it shifts the whole list down
                if (oldIndex < newIndex)
                    newIndex--;

                //remove from old index and add to new index
                activeMods.RemoveAt(oldIndex);
                if (newIndex == activeMods.Count)
                    activeMods.Add(dfm);
                else
                    activeMods.Insert(newIndex, dfm);

            }
            else if (!sourceLeft && destinationLeft)
            {
                //disable and add to disabled. simple enough
                activeMods.Remove(dfm);
                unactiveMods.Add(dfm);
            }
            else if (sourceLeft && !destinationLeft)
            {
                //insert/add from disabled to enabled
                unactiveMods.Remove(dfm);
                if (newIndex == activeMods.Count)
                    activeMods.Add(dfm);
                else
                    activeMods.Insert(newIndex, dfm);
            }
        }

        #region initialization file stuff
        private void FindAllMods()
        {
            modrefMap = new Dictionary<string, ModReference>();
            modPool = new HashSet<DFHackMod>();

            Console.WriteLine("Finding all mods... ");
            List<string> modFolders = new List<string>(Directory.GetDirectories(config.ModsPath));

            //add vanilla mod folders #fix# exclude obvious non-data folders #fix# popup on broken mods (would you like to delete)
            string vanillaDataPath = Path.Combine(config.DFFolderPath, @"data\vanilla");
            foreach (string vanillaModDir in Directory.GetDirectories(vanillaDataPath))
            {
                modFolders.Add(vanillaModDir);
            }

            //remove known non-mod folders
            List<string> temp = new List<string>();
            foreach (string modPath in modFolders)
            {
                //ignore three non-mod folders
                if (modPath.Contains("mod_upload") || modPath.Contains("examples and notes") || modPath.Contains("interaction examples"))
                    continue;
                temp.Add(modPath);

            }
            modFolders = temp;

            //load all the mods
            foreach (string modFolder in modFolders)
            {
                string infoPath = Path.Combine(modFolder, "info.txt");
                if (File.Exists(infoPath))
                {
                    string modInfo = File.ReadAllText(infoPath);
                    ModReference modRef = new ModReference(modInfo, modFolder);
                    if (!modRef.failed)
                    {
                        Console.WriteLine($"   Valid mod found: {modRef.name}");
                        modrefMap.Add(modRef.DFHackCompatibleString(), modRef);
                        modPool.Add(modRef.ToDFHackMod());
                    }
                    else
                    {
                        //#fix# double error log?
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"   Broken mod found. path:{modFolder}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    //Console.WriteLine("added " + modRef.DFHackCompatibleString());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"   Broken mod found. No info file. path:{modFolder}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        private void FindModlists()
        {
            string dfHackModlistPath = Path.Combine(config.DFFolderPath, @"dfhack-config\mod-manager.json");
            string dfHackModlistJson = File.ReadAllText(dfHackModlistPath);
            modlists = new List<DFHackModlist>(JsonSerializer.Deserialize<List<DFHackModlist>>(dfHackModlistJson));

            Console.WriteLine();
            bool modMissing = false;
            Console.WriteLine("Found modlists: ");
            string missingMessage = $"Some mods missing. \nModlists will be modified to not require lost mods. \nMissing mods: ";
            HashSet<DFHackMod> notFound = new HashSet<DFHackMod>();
            for (int i = 0; i < modlists.Count; i++)
            {
                DFHackModlist modlist = modlists[i];

                HashSet<DFHackMod> thisListMissingMods = new HashSet<DFHackMod>();
                foreach(DFHackMod mod in modlist.modlist)
                {
                    if(!modPool.Contains(mod))
                    {
                        modMissing = true;
                        notFound.Add(mod);
                        thisListMissingMods.Add(mod);
                        missingMessage += $"\n{mod}";
                    }
                }
                //#fix# could be a faster foor loop but eh
                foreach(DFHackMod m in thisListMissingMods)
                {
                    modlist.modlist.Remove(m);
                }

                Console.WriteLine("   Name: " + modlist.name);
                Console.WriteLine("   Default: " + modlist.@default);
                Console.WriteLine("   Mods count: " + modlist.modlist.Count);
                Console.WriteLine();

                if (modlist.@default)
                {
                    SetSelectedModlist(i);
                }
                modlists[i] = modlist;
            }
            if(modMissing)
            {
                MessageBox.Show(missingMessage, "Missing Mods", MessageBoxButtons.OK);
            }
        }

        public void FixConfig()
        {
            if (config == null)
                config = new ModHearthConfig();
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
            SaveConfigFile();
        }

        private string GetDFPath()
        {
            MessageBox.Show("Please find the path to your Dwarf Fortress.exe.", "DF.exe location", MessageBoxButtons.OK);
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
            }
        }

        public void SaveConfigFile()
        {
            Console.WriteLine("Config saved.");
            string jsonContent = JsonSerializer.Serialize(config);
            File.WriteAllText(configPath, jsonContent);
        }
        #endregion
    }
}
