using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ModHearth
{
    /// <summary>
    /// Stores all data relevant to a mod.
    /// More comprehensive than DFHMod, but not used in the actual creation of modpacks.
    /// </summary>
    public class ModReference
    {
        // Data found in modinfo files.
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

        // Path of mod folder, not path to info.
        public string path;

        // Did this modref fail creation.
        public bool failed;

        // Is this modref missing a version (one mod did this, dfhack set version to 1 so this matches it).
        public bool MissingVersion = false;

        // #FIXME: pointless constructor.
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
            failed = false;
        }

        // #TODO: do more testing with workshop mods, to see if there are more that break the pattern.
        // Generate a ModReference from string (text from the info file).
        public ModReference(string modInfo, string path)
        {
            // Set path.
            this.path = path;

            // Match for each field. Regex got extra complex with since one or two mods did things like '[ID]:modid' instead of '[ID:modid]' for some reason.
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

            // Handling of multiple groups since problem mods added more regex.
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

            // If this mod has no ID, that's not recoverable. Failed.
            if (!idMatch.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   Non-Recoverable Mod Error: Mod missing ID. Path: {path}");
                Console.ForegroundColor = ConsoleColor.White;
                this.failed = true;
                return;
            }
            ID = idValue;

            // One single mod was missing a version, and dfhack just set the version to 1, so do the same?
            if (!numVerMatch.Success)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"   Possibly-Recoverable Mod Error: Mod version issue. Mod missing either numeric version. Setting ver to 1. Path: {path}");
                Console.ForegroundColor = ConsoleColor.White;
                numericVersion = "1";
                MissingVersion = true;
                //this.failed = true;
                //return;
            }
            else 
                numericVersion = numVerValue;

            // If the mod is missing a display version that's fine, just set it to be numericVersion.
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

            // If the mod is missing earliest compatible versions, that's fine, those fields aren't used right now. #FIXME: what do these fields do? 
            if (!earliestNumVerMatch.Success || !earliestDispVerMatch.Success)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   Recoverable Mod Error: Mod missing earliest compatible numeric or earliest compatible display version. Path: {path}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            earliestCompatibleNumericVersion = earliestNumVerValue;
            earliestCompatibleDisplayedVersion = earliestDispVerValue;

            // If the author match fails, unfortunate but not going to affect function.
            if (!authMatch.Success)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   Recoverable Mod Error: Mod missing author. Path: {path}");
                Console.ForegroundColor = ConsoleColor.White;
                author = "author unknown";
            }
            else
                author = authValue;

            // Try to get either the name or the steam name, if either one is present this can proceed.
            name = "";
            steamName = "";

            if (nameMatch.Success)
                name = nameValue;

            if (steamNameMatch.Success)
                steamName = steamNameValue;

            // If either name is missing, copy from the other.
            if (name == "")
                name = steamName;
            if (steamName == "")
                steamName = name;

            // If no name is present, just use id instead.
            if (name == "" && steamName == "")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   Recoverable Mod Error: Mod missing name and steam name. Path: {path}");
                Console.ForegroundColor = ConsoleColor.White;

                name = steamName = $"ID: {ID}";
            }


            // Try to get either the description or the steam description.
            description = "";
            steamDescription = "";

            if (descMatch.Success)
                description = descValue;

            if (steamDescMatch.Success)
                steamDescription = steamDescValue;

            // If either description is missing, copy from the other.
            if (description == "")
                description = steamDescription;
            if (steamDescription == "")
                steamDescription = description;

            // If no description is present, it's not really an issue.
            if (description == "" && steamDescription == "")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   Recoverable Mod Error: Mod missing description and steam description. Path: {path}");
                Console.ForegroundColor = ConsoleColor.White;
                description = "description unknown";
                steamDescription = "description unknown";
            }

            // If there isn't a steam ID, assume it's a local mod and proceed.
            if(!steamIDMatch.Success)
            {
                steamID = "";
            }
            else
                steamID = steamIDValue;
        }

        // Use this mods ID and numvericVersion to create the DFHMod.
        public DFHMod ToDFHMod()
        {
            // DFHack does this to version, visible in the JSON file.
            int version = int.Parse(numericVersion.Replace(".", ""));
            DFHMod mod = new DFHMod(ID, version);
            return mod;
        }

        // Functionally get the ToString/HashCode of this mod as a DFHMod. Mainly used for HashMap keys.
        public string DFHackCompatibleString()
        {
            DFHMod temp = ToDFHMod();
            return temp.ToString();
        }
    }
}
