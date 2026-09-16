using Terraria.ModLoader;

namespace VariousModPatches
{
    public static class Instances
    {   
        private static VariousModPatches _mod;
        public static VariousModPatches Mod => _mod ??= ModContent.GetInstance<VariousModPatches>();

        public static VariousModPatchesConfig Config => ModContent.GetInstance<VariousModPatchesConfig>();
    }
}