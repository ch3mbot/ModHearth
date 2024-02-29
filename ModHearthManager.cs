using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModHearth
{
    [System.Serializable]
    public class ModHearthConfig
    {
        //path to DF.exe
        public string DFEXEPath { get; set; }
        public string DFFolderPath => Path.GetDirectoryName(DFEXEPath);
        public string ModsPath => Path.Combine(DFFolderPath, "Mods");

    }

    public class ModHearthManager
    {
        public Dictionary<string, ModReference> allMods;
        public List<ModReference> enabledMods;
        public HashSet<ModReference> disabledMods;

        public ModHearthConfig config;

        public static string configPath = "config.json";
        public static string modlistPath = "modlists/";

        public List<DFHackModlist> actualModLists;

        public Form1 form;

        public DFHackModlist lastSelectedModlist => actualModLists[selectedModlistIndex];
        public int selectedModlistIndex;

        public bool ModpackAltered = false;


        public ModHearthManager(Form1 form)
        {
            this.form = form;

            Console.WriteLine("Creating Hearth");
            //get and load data
            AttemptLoadConfig();
            FixConfig();

            //find all mods and add to a big list
            FindAllMods();

            FindModlists();

            
        }

        public void ModAbledChange(ModReference reference, bool enabled, int newIndex)
        {
            if(!enabled)
            {
                enabledMods.Remove(reference);
                disabledMods.Add(reference);
            }
            else
            {
                disabledMods.Remove(reference);
                if (newIndex >= enabledMods.Count)
                    enabledMods.Add(reference);
                else
                    enabledMods.Insert(newIndex, reference);
            }
            ModpackAltered = true;
            form.ShowUnsavedChanges();
        }

        public void ModOrderChange(ModReference reference, int newIndex)
        {
            if (!enabledMods.Contains(reference))
                return;
            int currentIndex = enabledMods.IndexOf(reference);

            if (currentIndex < newIndex)
                newIndex--;

            if (currentIndex == newIndex)
            {

            }
            else if (currentIndex < newIndex)
            {
                enabledMods.RemoveAt(currentIndex);
                if (newIndex < enabledMods.Count)
                {
                    enabledMods.Insert(newIndex, reference);
                }
                else
                {
                    enabledMods.Add(reference);
                }
            }
            else
            {
                enabledMods.RemoveAt(currentIndex);
                enabledMods.Insert(newIndex, reference);
            }
            Console.WriteLine($"moved from {currentIndex} to {newIndex}");
            ModpackAltered = true;
            form.ShowUnsavedChanges();
        }

        public void SetSelectedModlist(int index)
        {   
            selectedModlistIndex = index;

            enabledMods = new List<ModReference>();
            disabledMods = new HashSet<ModReference>();
            foreach (DFHackMod dfhmod in lastSelectedModlist.modlist)
            {
                if(!allMods.ContainsKey(dfhmod.ToString()))
                {
                    //#fix# should we pop up a window?
                    //Console.ForegroundColor = ConsoleColor.Yellow;
                    //Console.WriteLine($"WARNING: mod {dfhmod.id} was not found.");
                    //Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    enabledMods.Add(allMods[dfhmod.ToString()]);
                }
            }
            foreach(ModReference modref in allMods.Values)
            {
                if(!enabledMods.Contains(modref))
                {
                    disabledMods.Add(modref);
                }
            }
        }

        private void FindAllMods()
        {
            allMods = new Dictionary<string, ModReference>();

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
            foreach(string modPath in modFolders) 
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
                    if(!modRef.failed)
                    {
                        Console.WriteLine($"   Valid mod found: {modRef.name}");
                        allMods.Add(modRef.DFHackCompatibleString(), modRef);
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
            actualModLists = new List<DFHackModlist>(JsonSerializer.Deserialize<List<DFHackModlist>>(dfHackModlistJson));

            Console.WriteLine();
            Console.WriteLine("Found modlists: ");
            for(int i = 0; i < actualModLists.Count; i++) 
            {
                DFHackModlist modlist = actualModLists[i];
                Console.WriteLine("   Name: " + modlist.name);
                Console.WriteLine("   Default: " + modlist.@default);
                Console.WriteLine("   Mods count: " + modlist.modlist.Count);
                Console.WriteLine();

                if(modlist.@default)
                {
                    SetSelectedModlist(i);
                    form.currentModlistIndex = i;
                }
            }
        }

        public void FixConfig()
        {
            if(config == null)
                config = new ModHearthConfig();
            if(String.IsNullOrEmpty(config.DFEXEPath))
            {
                Console.WriteLine("Config file missing DF path.");
                string newPath = "";
                while(string.IsNullOrEmpty(newPath))
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
            if(result == DialogResult.OK) 
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

                    if(config == null)
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

        public void SaveModlist() 
        {
            //set the saved list to a copy of this one
            List<DFHackMod> newlist = new List<DFHackMod>();
            enabledMods.ForEach((x) => newlist.Add(x.ToDFHackMod()));
            lastSelectedModlist.modlist = newlist;
            
            string dfHackModlistPath = Path.Combine(config.DFFolderPath, @"dfhack-config\mod-manager.json");
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true // Enable pretty formatting
            };
            string modlistJson = JsonSerializer.Serialize(actualModLists, options);
            File.WriteAllText(dfHackModlistPath, modlistJson);
        }
    }
}
