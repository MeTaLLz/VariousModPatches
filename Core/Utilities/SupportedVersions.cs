using System;
using System.Collections.Generic;

namespace VariousModPatches
{
    public static class SupportedVersions
    {
        private static readonly Dictionary<string, Version> _versions = new()
        {
            { "TerrariaAmbience", new(0, 3, 7, 1) },
            { "RainOverhaul", new(1, 8, 2) },
            { "InfernumMasterPatch", new(1, 2, 1) },
            { "KZ_VisualMod", new(0, 0, 0, 2) },
            { "RecipeFavoritesSync", new(0, 13) },
            { "SOTS", new(0, 25, 1, 9) },
            { "MagicStorage", new(0, 7, 0, 11) },
            { "CalamityHunt", new(1, 2, 3) }
        };

        public static Version Get(string modName)
        {
            return _versions.TryGetValue(modName, out var version) ? version : null;
        }
    }
}