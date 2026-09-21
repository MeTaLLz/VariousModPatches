using Terraria.ModLoader;

namespace VariousModPatches
{
    public static class Instances
    {   
        private static VariousModPatches _mod;
        public static VariousModPatches Mod => _mod ??= ModContent.GetInstance<VariousModPatches>();

        public static VMPConfig Config => ModContent.GetInstance<VMPConfig>();
    }
}