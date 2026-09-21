using Terraria.ModLoader;

namespace VariousModPatches
{
    public static class VersionChecker
    {
        public static Mod GetModChecked(string modName, string patchName, bool isSelf = true)
        {
            if (!ModLoader.TryGetMod(modName, out Mod mod))
                return null;

            if (mod.Version == SupportedVersions.Get(modName))
                return mod;

            if (!Instances.Config.DisableVersionWarnings)
            {
                string label = isSelf ? "Mod" : modName;
                FileLogger.Warn(patchName, $"{label} version changed! Expected {SupportedVersions.Get(modName)}, got {mod.Version}.");
                FileLogger.Info(patchName, "If you experience issues, disable this patch in config.");
            }

            return mod;
        }
    }
}