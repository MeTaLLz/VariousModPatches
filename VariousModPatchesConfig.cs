using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace VariousModPatches
{
    [BackgroundColor(120, 60, 55, 220)]
    [SliderColor(200, 130, 70, 255)]
    public class VariousModPatchesConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("Patches")]
        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchTerrariaAmbience_Message = true;

        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchTerrariaAmbience_Audio = true;

        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchRecipeFavoritesSync = true;

        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchMagicStorage = true;

        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchSOTS = true;

        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchInfernumMasterPatch = true;

        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchRainOverhaul = true;

        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(0)]
        [Range(-1, 3)]
        [Slider]
        [ReloadRequired]
        public int PatchKZVisualMod_LightningMode = 0;

        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchCalamityHunt = true;

        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchWorldOfPiss = true;

        [Header("Debug")]
        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(false)]
        public bool DisableVersionWarnings = false;
    }
}