using Terraria.ModLoader;

namespace VariousModPatches
{
	public class VariousModPatches : Mod
    {
        private static VariousModPatches _mod;
        public static VariousModPatches Instance => _mod ??= ModContent.GetInstance<VariousModPatches>();
    }
}
