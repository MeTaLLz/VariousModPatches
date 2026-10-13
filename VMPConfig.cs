using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace VariousModPatches
{
    [BackgroundColor(120, 60, 55, 220)]
    [SliderColor(200, 130, 70, 255)]
    public class VMPConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("QoL")]
        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchTerrariaAmbience_Message = true;

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
        public bool PatchShapeOfLight = true;

        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchInfernumMasterPatch = true;

        [Header("Fixes")]
        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchTerrariaAmbience_Audio = true;

        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchKZVisualMod = true;

        /*
        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchRainOverhaul = true;
        */

        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchCalamityHunt = true;

        [Header("Performance")]
        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool PatchInfernumMode = true;

        [Header("Debug")]
        [BackgroundColor(120, 60, 55, 220)]
        [DefaultValue(false)]
        public bool DisableVersionWarnings = false;
    }
}