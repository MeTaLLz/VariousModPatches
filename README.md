# VariousModPatches

Patches various annoyances and issues in other mods that authors haven't addressed yet. NOT an optimizer — just small fixes for specific problems.

## Available patches

- **Terraria Ambience:**
  - Removes the startup message from the mod when entering a world
  - Fixes being unable to exit the resource pack menu after leaving a world, caused by audio errors from the mod
- **Recipe Browser × Magic Storage: Fully Connected:**
  - Removes the startup message from the mod when entering a world
- **Magic Storage:**
  - Removes the patron reminder message from the mod
- **Secrets of the Shadows:**
  - Removes the startup messages from the mod when entering a world
- **Infernum Master Patch:**
  - When loading a world, automatically enables Infernum Master Patch difficulty if Master + Death + Infernum are active (i.e. the world was saved at the IMP difficulty)
- **KZ的视觉效果前置 (KZ_VisualMod, library for Luminous Water Reflection 潮之韵(Demo) and Shape of Light 光之形):**
  - Fixes forced lighting mode switching after mods reload, which may alternate between White and Color, as well as the Retro and Trippy modes being impossible to switch to in the menu
- **Rain Overhaul:**
  - Prevents sudden disappearance of Devourer of Gods from Calamity and Empress of Light from WoTE (and possibly other issues), only if Rain World mode is disabled
- **Calamity: Hunt of the Old God:**
  - Prevents a crash during mod unloading caused by Calamity: Hunt of the Old God calling player rendering, which triggers draw layers from some mods that are already partially unloaded (e.g. WeaponOut Lite)
- **World of Piss:**
  - Disables random Piss Moons, leaving only the angler quest trigger
- **Split:**
  - Removes the armor reforge restriction at the Goblin Tinkerer when using mods that add their own armor prefixes (e.g. Exxo Avalon Origins)

This mod will become lighter as these issues are addressed by mod authors.