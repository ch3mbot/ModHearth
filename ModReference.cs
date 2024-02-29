using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ModHearth
{
    public class ModReference
    {
        public string ID;
        public string numericVersion;
        public string displayedVersion;
        public string earliestCompatibleNumericVersion;
        public string earliestCompatibleDisplayedVersion;
        public string author;
        public string name;
        public string description;

        public string steamName;
        public string steamDescription;
        public string steamID;

        //path of mod folder, not path to info
        public string path;

        public bool failed;

        public bool MissingVersion = false;

        public Control matchingControl;

        public ModReference(string ID, string numericVersion, string displayedVersion, string earliestCompatibleNumericVersion, string earliestCompatibleDisplayedVersion, string author, string name, string description, string steamName, string steamDescription, string steamID, string path)
        {
            this.ID = ID;
            this.numericVersion = numericVersion;
            this.displayedVersion = displayedVersion;
            this.earliestCompatibleNumericVersion = earliestCompatibleNumericVersion;
            this.earliestCompatibleDisplayedVersion = earliestCompatibleDisplayedVersion;
            this.author = author;
            this.name = name;
            this.description = description;
            this.steamName = steamName;
            this.steamDescription = steamDescription;
            this.steamID = steamID;
            this.path = path;
            this.failed = false;
        }

        public ModReference(string modInfo, string path)
        {
            //set path
            this.path = path;

            // Match for each field
            Match idMatch = Regex.Match(modInfo, @"\[ID\]:*(.*?)\n|\[ID:*(.*?)\]", RegexOptions.IgnoreCase);
            Match numVerMatch = Regex.Match(modInfo, @"\[NUMERIC_VERSION\]:*(.*?)\n|\[NUMERIC_VERSION:*(.*?)\]", RegexOptions.IgnoreCase);
            Match dispVerMatch = Regex.Match(modInfo, @"\[DISPLAYED_VERSION\]:*(.*?)\n|\[DISPLAYED_VERSION:*(.*?)\]", RegexOptions.IgnoreCase);
            Match earliestNumVerMatch = Regex.Match(modInfo, @"\[EARLIEST_COMPATIBLE_NUMERIC_VERSION\]:*(.*?)\n|\[EARLIEST_COMPATIBLE_NUMERIC_VERSION:*(.*?)\]", RegexOptions.IgnoreCase);
            Match earliestDispVerMatch = Regex.Match(modInfo, @"\[EARLIEST_COMPATIBLE_DISPLAYED_VERSION\]:*(.*?)\n|\[EARLIEST_COMPATIBLE_DISPLAYED_VERSION:*(.*?)(\]|\n)", RegexOptions.IgnoreCase);
            Match authMatch = Regex.Match(modInfo, @"\[AUTHOR\]:*(.*?)\n|\[AUTHOR:*(.*?)\]", RegexOptions.IgnoreCase);
            Match nameMatch = Regex.Match(modInfo, @"\[NAME\]:*(.*?)\n|\[NAME:*(.*?)\]", RegexOptions.IgnoreCase);
            Match descMatch = Regex.Match(modInfo, @"\[DESCRIPTION\]:*(.*?)\n|\[DESCRIPTION:*((.|\n)*?)\]", RegexOptions.IgnoreCase);
            Match steamNameMatch = Regex.Match(modInfo, @"\[STEAM_TITLE\]:*(.*?)\n|\[STEAM_TITLE:*(.*?)\]", RegexOptions.IgnoreCase);
            Match steamDescMatch = Regex.Match(modInfo, @"\[STEAM_DESCRIPTION\]:*(.*?)\n|\[STEAM_DESCRIPTION:*(.*?)\]", RegexOptions.IgnoreCase);
            Match steamIDMatch = Regex.Match(modInfo, @"\[STEAM_FILE_ID\]:*(.*?)\n|\[STEAM_FILE_ID:*(.*?)\]", RegexOptions.IgnoreCase);

            string idValue = idMatch.Groups[1].Success ? idMatch.Groups[1].Value : idMatch.Groups[2].Value;
            string numVerValue = numVerMatch.Groups[1].Success ? numVerMatch.Groups[1].Value : numVerMatch.Groups[2].Value;
            string dispVerValue = dispVerMatch.Groups[1].Success ? dispVerMatch.Groups[1].Value : dispVerMatch.Groups[2].Value;
            string earliestNumVerValue = earliestNumVerMatch.Groups[1].Success ? earliestNumVerMatch.Groups[1].Value : earliestNumVerMatch.Groups[2].Value;
            string earliestDispVerValue = earliestDispVerMatch.Groups[1].Success ? earliestDispVerMatch.Groups[1].Value : earliestDispVerMatch.Groups[2].Value;
            string authValue = authMatch.Groups[1].Success ? authMatch.Groups[1].Value : authMatch.Groups[2].Value;
            string nameValue = nameMatch.Groups[1].Success ? nameMatch.Groups[1].Value : nameMatch.Groups[2].Value;
            string descValue = descMatch.Groups[1].Success ? descMatch.Groups[1].Value : descMatch.Groups[2].Value;
            string steamNameValue = steamNameMatch.Groups[1].Success ? steamNameMatch.Groups[1].Value : steamNameMatch.Groups[2].Value;
            string steamDescValue = steamDescMatch.Groups[1].Success ? steamDescMatch.Groups[1].Value : steamDescMatch.Groups[2].Value;
            string steamIDValue = steamIDMatch.Groups[1].Success ? steamIDMatch.Groups[1].Value : steamIDMatch.Groups[2].Value;

            if (!idMatch.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   Non-Recoverable Mod Error: Mod missing ID. Path: {path}");
                Console.ForegroundColor = ConsoleColor.White;
                this.failed = true;
                return;
            }
            ID = idValue;

            if (!numVerMatch.Success)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"   Possibly-Recoverable Mod Error: Mod version issue. Mod missing either numeric version. Setting ver to . Path: {path}");
                Console.ForegroundColor = ConsoleColor.White;
                numericVersion = "1";
                MissingVersion = true;
                //this.failed = true;
                //return;
            }
            else 
                numericVersion = numVerValue;
            if(!dispVerMatch.Success)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   Recoverable Mod Error: Mod missing display version. Setting to numeric. Path: {path}");
                Console.ForegroundColor = ConsoleColor.White;
                displayedVersion = numericVersion;
            }
            else
            {
                displayedVersion = dispVerValue;
            }

            if (!earliestNumVerMatch.Success || !earliestDispVerMatch.Success)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   Recoverable Mod Error: Mod missing earliest compatible numeric or earliest compatible display version. Path: {path}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            earliestCompatibleNumericVersion = earliestNumVerValue;
            earliestCompatibleDisplayedVersion = earliestDispVerValue;

            if (!authMatch.Success)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   Recoverable Mod Error: Mod missing author. Path: {path}");
                Console.ForegroundColor = ConsoleColor.White;
                author = "author unknown";
            }
            else
                author = authValue;

            //try to get both names
            name = "";
            steamName = "";

            if (nameMatch.Success)
                name = nameValue;

            if (steamNameMatch.Success)
                steamName = steamNameValue;

            //if either name is missing, copy from the other
            if (name == "")
                name = steamName;
            if (steamName == "")
                steamName = name;

            //if no name is present, that's bad
            if (name == "" && steamName == "")
            {
                //only happens if the mod dev is dumb

                //if(alternameNameMatch.Success)
                //{

                //}
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   Non-Recoverable Mod Error: Mod missing name and steam name. Path: {path}");
                Console.ForegroundColor = ConsoleColor.White;
                this.failed = true;
                return;
            }


            //try to get both descriptions
            description = "";
            steamDescription = "";

            if (descMatch.Success)
                description = descValue;

            if (steamDescMatch.Success)
                steamDescription = steamDescValue;

            //if either name is missing, copy from the other
            if (description == "")
                description = steamDescription;
            if (steamDescription == "")
                steamDescription = description;

            //if no name is present, that's bad
            if (description == "" && steamDescription == "")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   Recoverable Mod Error: Mod missing description and steam description. Path: {path}");
                Console.ForegroundColor = ConsoleColor.White;
                description = "description unknown";
                steamDescription = "description unknown";
            }

            //#fix# mark mod as local maybe
            if(!steamIDMatch.Success)
            {
                //Console.WriteLine($"Recoverable? Mod Error: Mod missing steam ID. Path: {path}");
                steamID = "";
            }
            else
                steamID = steamIDValue;
        }

        //#fix# probably remove
        private void DumpInfo(string path, string info)
        {
            Console.WriteLine("-X-mod info dump:");
            Console.WriteLine($"-X-path:\n{path}");
            Console.WriteLine($"-X-info:\n{info}");
        }

        //#fix# probably remove
        private string FixVersionString(string version)
        {
            if (!version.Contains("."))
                return version + ".0";
            return version;
        }

        public DFHackMod ToDFHackMod()
        {
            DFHackMod mod = new DFHackMod();
            mod.id = ID;
            if(numericVersion == null)
            {
                Console.WriteLine("######################## HUH???? 1. " + ID + " path: " + path);
                //mod.version = 
                return mod;
            }
            mod.version = int.Parse(numericVersion.Replace(".", ""));
            return mod;
        }

        public string DFHackCompatibleString()
        {
            DFHackMod temp = ToDFHackMod();
            return temp.ToString();
        }
    }
}
