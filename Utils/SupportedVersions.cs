using System;
using System.Collections.Generic;

namespace VariousModPatches;

public static class SupportedVersions
{
    private static readonly Dictionary<string, Version> _versions = new()
    {
        { "TerrariaAmbience", new(0, 3, 7, 1) },
        { "InfernumMasterPatch", new(1, 2, 1) },
        { "KZ_VisualMod", new(0, 0, 0, 2) },
        { "RecipeFavoritesSync", new(0, 19) },
        { "SOTS", new(0, 25, 1, 9) },
        { "MagicStorage", new(0, 7, 0, 11) },
        { "CalamityHunt", new(1, 2, 3) },
        { "CalamityMod", new(2, 2, 4) },
        { "InfernumMode", new(2, 0, 1, 35) },
        { "ShapeOfLight_New", new(0, 0, 2, 12) }
    };

    public static Version Get(string modName)
    {
        return _versions.TryGetValue(modName, out var version) ? version : null;
    }
}