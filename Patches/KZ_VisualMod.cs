using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Light;
using System;

namespace VariousModPatches
{
    public class KZ_VisualMod : ModSystem
    {
        public override void PostSetupContent()
        {
            if (!NetmodeHelper.IsSingleplayer) return;
            if (Instances.Config.PatchKZVisualMod_LightningMode == -1) return;

            var mod = VersionChecker.GetModChecked("KZ_VisualMod", "KZ_VisualMod");
            if (mod == null) return; 

            try
            {
                var mode = Instances.Config.PatchKZVisualMod_LightningMode;
                Lighting.Mode = mode switch
                {
                    0 => LightMode.Color,
                    1 => LightMode.White,
                    2 => LightMode.Retro,
                    3 => LightMode.Trippy,
                    _ => LightMode.Color
                };
            }
            catch (Exception ex)
            {
                FileLogger.Error("KZ_VisualMod", $"Failed to set lighting mode: {ex.Message}");
            }
        }
    }
}

// Hidden code, I literally cannot remove this feature