using Terraria.ModLoader;

namespace VariousModPatches
{
    public static class VersionHelper
    {
        public static Mod GetModAndWarn(string modName, string patchName)
        {
            if (!ModLoader.TryGetMod(modName, out Mod mod))
                return null;

            if (mod.Version == SupportedVersions.Get(modName))
                return mod;

            var config = ModContent.GetInstance<VariousModPatchesConfig>();
            if (!config.DisableVersionWarnings)
            {
                var logger = ModContent.GetInstance<VariousModPatches>().Logger;
                logger.Warn($"[VariousModPatches] ({patchName}) Mod version changed! Expected {SupportedVersions.Get(modName)}, got {mod.Version}.");
                logger.Warn($"[VariousModPatches] ({patchName}) If you experience issues, disable this patch in config.");
            }

            return mod;
        }
    }
}