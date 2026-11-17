using Terraria.ModLoader;

namespace VariousModPatches;

public enum PatchSide
{
    None = 0,
    Singleplayer = 1,
    MultiplayerClient = 2,
    Server = 4,
    
    Client = Singleplayer | MultiplayerClient,
    
    Any = Singleplayer | MultiplayerClient | Server
}

public static class PatchApplier
{
    public static bool ShouldApplyPatch(bool configValue, string modName, string patchName, 
        PatchSide allowedSides, out Mod mod)
    {
        mod = null;

        if (!configValue) return false;

        PatchSide currentSide = Netmode.IsSingleplayer ? PatchSide.Singleplayer :
                                Netmode.IsMultiplayerClient ? PatchSide.MultiplayerClient :
                                PatchSide.Server;

        if ((allowedSides & currentSide) == 0)
            return false;

        mod = GetMod(modName, patchName);
        return mod != null;
    }

    public static Mod GetMod(string modName, string patchName, bool isSelf = true)
    {
        if (!ModLoader.TryGetMod(modName, out Mod mod))
            return null;

        if (mod.Version != SupportedVersions.Get(modName))
        {
            string label = isSelf ? "Mod" : modName;
            ModLog.Warn(patchName, $"{label} version changed! Expected {SupportedVersions.Get(modName)}, got {mod.Version}.");
            ModLog.Info(patchName, "If you experience issues, disable this patch in config.");
        }

        return mod;
    }
}