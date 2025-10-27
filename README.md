# Elin.Plugins
This is a Mod For Elin ———— https://store.steampowered.com/app/2135150/Elin/

## Important Notes

Please be aware that mods may suddenly stop working or potentially cause serious issues

Some mods may make irreversible changes to save data (Second reminder)

The mod creator's experience is still developing, and many features are implemented through brute-force coding methods

At this stage, it cannot be guaranteed that all features are implemented optimally

The base Elin game itself is still in Alpha testing, and future specifications and implementations are highly likely to change

## The repository does NOT include the following

The repository does not contain Elin-related libraries

The repository does not contain Unity-related libraries

The repository does not contain BepInEx-related libraries

Please handle dependencies independently when modifying and compiling

## Build
The projects require environment variable ElinGamePath set to the root folder of the Elin game installation.

ElinGamePath/
├─ BepInEx/
│  ├─ core/
│  │  ├─ *.dll
├─ Elin_Data/
│  ├─ Managed/
│  │  ├─ *.dll

When using CWL as the using directive, this environment variable needs to be additionally configured.

SteamContentPath/
├─ workshop/content
