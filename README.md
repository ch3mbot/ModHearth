# ModHearth Mod Manager for Dwarf Fortress
This is a mod manager for the steam version of Dwarf Fortress, made to interact with both DFHack and steam workshop mods.

## User Information:

### Requirements:
- Dwarf Fortress steam version
> FIXME: mac/linux support by rewriting all of UI with something cross-platform instead of winforms
- Windows PC 
> FIXME: uninstall and reinstall DFHack, check if it comes with mod manager by default
- DFHack Installed
- Game has been launched at least once

### Installation Guide
1. Go to [releases](https://www.google.ca) and download the most recent version 
2. Extract zip to suitable location
3. Run ModHearth.exe
4. Locate Dwarf Fortress.exe

### Instructions
> FIXME: should instructions be put here? what buttons do? Most is self explanatory.

## Contributor Information

### General Functionality
This tool works by pulling mods from the dwarf fortress mods folder, and pulling modpacks from the dfhack config mod-manager.json.
ModReferences are generated from found mod folders, while modpacks are generated from the dfhack config.
Loading modpacks into the game is done via altering mod-manager.json and using dfhacks normal mod management once the game loads.

### Term Definitions
#### DFHMod
DFHack only deals with mod name and mod version. This is all that's saved and loaded.
These are set up to act like a value type.  

#### ModReference
> FIXME: mod dependency detection and storage here, integration into drag and drop system

A more comprehensive object storing more information about mods, mostly for displaying. Is not saved.
Can easily be converted to a DFHMod.

#### DFHModPack
An object representing a modpack matching how DFHack handles them. Only has a name, a bool default, and a list of DFHMods.
DFHack stores a list of these in a json file, which it loads into the game. 