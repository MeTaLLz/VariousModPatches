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
            var config = ModContent.GetInstance<VariousModPatchesConfig>();
            if (config.PatchKZVisualMod_LightningMode < 0) return;

            var mod = VersionHelper.GetModAndWarn("KZ_VisualMod", "KZ_VisualMod");
            if (mod == null) return; 

            try
            {
                var mode = config.PatchKZVisualMod_LightningMode;
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
                Mod.Logger.Error($"[VariousModPatches] (KZ_VisualMod) Failed to set lighting mode: {ex.Message}");
            }
        }
    }
}

// Hidden code, I literally cannot remove this feature